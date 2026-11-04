using System.Text.Json;
using System.Xml.Linq;

namespace Nro.Tests;

/// <summary>
/// The "real dependency graph" guard of the repo. It does not trust the ProjectReference entries of each .csproj alone, because they can be bypassed with
/// Directory.Build.targets, &lt;Import&gt;, &lt;Compile Include/Link&gt; pointing outside the project... So it also checks:
///  1. obj/project.assets.json (the restore result: the REAL direct and transitive dependencies between projects);
///  2. it forbids Directory.Build.targets/.rsp, extra .targets/.props files, every &lt;Import&gt;, and extra content in the root Directory.Build.props;
///  3. it forbids &lt;Compile/EmbeddedResource Include|Link|Update&gt; pointing outside the project directory, and binary &lt;Reference&gt;;
///  4. it forbids relaxing warnings (NoWarn, WarningsNotAsErrors, Nullable != enable, TreatWarningsAsErrors != true).
/// Every function takes the repo root so it can be tested on a fake tree (see RepoGuardTests).
/// </summary>
public static class RepoGuard
{
    /// <summary>The table in section 3 of 02-IMPLEMENTATION-PLAN.md: project -> the projects it may reference DIRECTLY.</summary>
    public static readonly IReadOnlyDictionary<string, string[]> Allowed = new Dictionary<string, string[]>
    {
        ["Nro.Core"] = [],
        ["Nro.Data"] = ["Nro.Core"],
        ["Nro.Network"] = ["Nro.Core"],
        ["Nro.Game"] = ["Nro.Core", "Nro.Data", "Nro.Network"],
        ["Nro.Content"] = ["Nro.Game"],
        ["Nro.Host"] = ["Nro.Core", "Nro.Data", "Nro.Network", "Nro.Game", "Nro.Content"],
    };

    private static readonly string[] SkipDirs = ["bin", "obj", ".git", "node_modules", ".vs", "TestResults"];

    private static readonly string[] ForbiddenRootPropsElements =
        ["ItemGroup", "Target", "Import", "UsingTask", "ProjectReference", "PackageReference", "Compile", "Reference", "Choose", "When"];

    /// <summary>Every project that a project may depend on, including transitively.</summary>
    public static HashSet<string> Closure(string project)
    {
        var seen = new HashSet<string>();
        var stack = new Stack<string>(Allowed[project]);
        while (stack.Count > 0)
        {
            var p = stack.Pop();
            if (seen.Add(p))
            {
                foreach (var q in Allowed[p])
                {
                    stack.Push(q);
                }
            }
        }

        return seen;
    }

    public static string CsprojPath(string root, string project) => Path.Combine(root, "src", project, project + ".csproj");

    public static IEnumerable<string> AllFiles(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var dir = pending.Pop();
            foreach (var f in Directory.EnumerateFiles(dir))
            {
                yield return f;
            }

            foreach (var d in Directory.EnumerateDirectories(dir))
            {
                if (!SkipDirs.Contains(Path.GetFileName(d), StringComparer.OrdinalIgnoreCase))
                {
                    pending.Push(d);
                }
            }
        }
    }

    private static string Rel(string root, string file) => Path.GetRelativePath(root, file).Replace('\\', '/');

    /// <summary>The names of the projects that the csproj DECLARES it references (ProjectReference).</summary>
    public static string[] DeclaredReferences(XDocument doc) =>
        doc.Descendants()
            .Where(e => e.Name.LocalName == "ProjectReference")
            .Select(e => Path.GetFileNameWithoutExtension(((string?)e.Attribute("Include") ?? "").Replace('\\', '/')))
            .ToArray();

    // ------------------------------------------------------------------ (1) csproj
    public static List<string> CsprojProblems(string root)
    {
        var problems = new List<string>();
        foreach (var (project, allowed) in Allowed)
        {
            var csproj = CsprojPath(root, project);
            if (!File.Exists(csproj))
            {
                problems.Add($"Missing {Rel(root, csproj)}");
                continue;
            }

            var doc = XDocument.Load(csproj);
            var extra = DeclaredReferences(doc).Except(allowed).ToList();
            if (extra.Count > 0)
            {
                problems.Add($"{project} has a disallowed reference: {string.Join(", ", extra)}");
            }

            problems.AddRange(ProjectFileProblems(root, csproj, doc));
        }

        return problems;
    }

    /// <summary>Whether a relative path escapes its root directory (does not touch the file system, handles wildcard characters).</summary>
    public static bool EscapesDirectory(string value)
    {
        var v = value.Replace('\\', '/');
        if (v.StartsWith('/') || v.Contains("$(") || (v.Length > 1 && v[1] == ':'))
        {
            return true;
        }

        var depth = 0;
        foreach (var seg in v.Split('/'))
        {
            if (seg == "..")
            {
                depth--;
                if (depth < 0)
                {
                    return true;
                }
            }
            else if (seg is not ("" or "."))
            {
                depth++;
            }
        }

        return false;
    }

    private static List<string> ProjectFileProblems(string root, string file, XDocument doc)
    {
        var problems = new List<string>();
        var rel = Rel(root, file);
        foreach (var el in doc.Descendants())
        {
            var name = el.Name.LocalName;
            var text = el.Value.Trim();
            switch (name)
            {
                case "Import":
                    problems.Add($"{rel}: <Import> is forbidden (bypasses the dependency/warning rules through an extra file)");
                    break;
                case "Reference":
                    problems.Add($"{rel}: binary <Reference> is forbidden (use a ProjectReference that follows the rules)");
                    break;
                case "NoWarn":
                case "WarningsNotAsErrors":
                    problems.Add($"{rel}: <{name}> is forbidden");
                    break;
                case "TreatWarningsAsErrors" when !text.Equals("true", StringComparison.OrdinalIgnoreCase):
                    problems.Add($"{rel}: <TreatWarningsAsErrors> must be true (currently '{text}')");
                    break;
                case "Nullable" when !text.Equals("enable", StringComparison.OrdinalIgnoreCase):
                    problems.Add($"{rel}: <Nullable> must be enable (currently '{text}')");
                    break;
                case "Compile":
                case "EmbeddedResource":
                    foreach (var attr in new[] { "Include", "Update", "Link" })
                    {
                        var v = (string?)el.Attribute(attr);
                        if (v is null)
                        {
                            continue;
                        }

                        foreach (var part in v.Split(';', StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (EscapesDirectory(part))
                            {
                                problems.Add($"{rel}: <{name} {attr}=\"{part}\"> points outside the project directory");
                            }
                        }
                    }

                    break;
            }
        }

        return problems;
    }

    // ------------------------------------------------------------------ (2) extra build files
    public static List<string> BuildFileProblems(string root)
    {
        var problems = new List<string>();
        var rootProps = Path.Combine(root, "Directory.Build.props");
        if (!File.Exists(rootProps))
        {
            problems.Add("Missing Directory.Build.props at the repo root");
        }

        foreach (var file in AllFiles(root))
        {
            var name = Path.GetFileName(file);
            var rel = Rel(root, file);
            if (name.Equals("Directory.Build.targets", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Directory.Build.rsp", StringComparison.OrdinalIgnoreCase)
                || name.Equals("MSBuild.rsp", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Directory.Solution.", StringComparison.OrdinalIgnoreCase))
            {
                problems.Add($"{rel}: this file is forbidden (bypasses the dependency/warning rules)");
                continue;
            }

            if (name.EndsWith(".targets", StringComparison.OrdinalIgnoreCase))
            {
                problems.Add($"{rel}: .targets files are forbidden");
                continue;
            }

            var isProps = name.EndsWith(".props", StringComparison.OrdinalIgnoreCase);
            if (isProps && !string.Equals(file, rootProps, StringComparison.OrdinalIgnoreCase)
                && !name.Equals("Directory.Packages.props", StringComparison.OrdinalIgnoreCase))
            {
                problems.Add($"{rel}: extra/nested .props files are forbidden (only the root Directory.Build.props)");
                continue;
            }

            if (isProps && !name.Equals("Directory.Build.props", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!isProps && !name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(file);
            }
            catch (System.Xml.XmlException e)
            {
                problems.Add($"{rel}: broken XML: {e.Message}");
                continue;
            }

            foreach (var el in doc.Descendants().Where(e => e.Name.LocalName == "Import"))
            {
                problems.Add($"{rel}: <Import Project=\"{(string?)el.Attribute("Project")}\"> is forbidden");
            }

            if (isProps)
            {
                foreach (var el in doc.Descendants().Where(e => ForbiddenRootPropsElements.Contains(e.Name.LocalName)))
                {
                    problems.Add($"{rel}: Directory.Build.props may contain only properties, not <{el.Name.LocalName}>");
                }
            }
        }

        return problems;
    }

    // ------------------------------------------------------------------ (3) the real graph in project.assets.json
    public static List<string> AssetsProblems(string root)
    {
        var problems = new List<string>();
        foreach (var (project, allowed) in Allowed)
        {
            var assets = Path.Combine(root, "src", project, "obj", "project.assets.json");
            if (!File.Exists(assets))
            {
                problems.Add($"Missing {Rel(root, assets)}: not restored yet (run dotnet restore/test through the checker)");
                continue;
            }

            using var json = JsonDocument.Parse(File.ReadAllText(assets));
            var top = json.RootElement;
            var projectLibs = new HashSet<string>();
            foreach (var lib in top.GetProperty("libraries").EnumerateObject())
            {
                if (lib.Value.TryGetProperty("type", out var t) && t.GetString() == "project")
                {
                    projectLibs.Add(lib.Name.Split('/')[0]);
                }
            }

            var transitive = new HashSet<string>();
            foreach (var tfm in top.GetProperty("targets").EnumerateObject())
            {
                foreach (var lib in tfm.Value.EnumerateObject())
                {
                    if (lib.Value.TryGetProperty("type", out var t) && t.GetString() == "project")
                    {
                        transitive.Add(lib.Name.Split('/')[0]);
                    }
                }
            }

            var direct = new HashSet<string>();
            foreach (var group in top.GetProperty("projectFileDependencyGroups").EnumerateObject())
            {
                foreach (var entry in group.Value.EnumerateArray())
                {
                    var dep = (entry.GetString() ?? "").Split(' ')[0];
                    if (projectLibs.Contains(dep))
                    {
                        direct.Add(dep);
                    }
                }
            }

            transitive.Remove(project);
            var badDirect = direct.Except(allowed).ToList();
            if (badDirect.Count > 0)
            {
                problems.Add($"{project}: real DIRECT dependency (project.assets.json) violates the rules: {string.Join(", ", badDirect)}");
            }

            var badTransitive = transitive.Except(Closure(project)).ToList();
            if (badTransitive.Count > 0)
            {
                problems.Add($"{project}: real TRANSITIVE dependency (project.assets.json) violates the rules: {string.Join(", ", badTransitive)}");
            }

            var csproj = CsprojPath(root, project);
            if (File.Exists(csproj))
            {
                var declared = new HashSet<string>(DeclaredReferences(XDocument.Load(csproj)));
                var hidden = direct.Except(declared).ToList();
                if (hidden.Count > 0)
                {
                    problems.Add($"{project}: the real dependencies include {string.Join(", ", hidden)} which the .csproj does not declare (Directory.Build.targets/Import?)");
                }
            }
        }

        return problems;
    }
}
