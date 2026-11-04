using System.Reflection;
using System.Xml.Linq;

namespace Nro.Tests.Unit;

/// <summary>
/// Keeps the dependency rules of section 3 of 02-IMPLEMENTATION-PLAN.md:
/// Core <- Data/Network <- Game <- Content <- Host. No reverse dependency is allowed.
/// It does not only read ProjectReference in the .csproj (which can be bypassed with Directory.Build.targets, Import, Compile Link):
/// it also checks the REAL graph in obj/project.assets.json and forbids the bypass routes. Details in <see cref="RepoGuard"/>.
/// </summary>
public class ArchitectureTests
{
    private static string[] ProjectReferences(string project)
    {
        var csproj = RepoGuard.CsprojPath(TestPaths.RepoRoot(), project);
        Assert.True(File.Exists(csproj), $"Missing {csproj}");
        return RepoGuard.DeclaredReferences(XDocument.Load(csproj));
    }

    [Theory]
    [InlineData("Nro.Core")]
    [InlineData("Nro.Data")]
    [InlineData("Nro.Network")]
    [InlineData("Nro.Game")]
    [InlineData("Nro.Content")]
    [InlineData("Nro.Host")]
    public void Project_references_only_allowed_projects(string project)
    {
        var extra = ProjectReferences(project).Except(RepoGuard.Allowed[project]).ToList();

        Assert.True(extra.Count == 0, $"{project} has a disallowed reference: {string.Join(", ", extra)}");
    }

    [Fact]
    public void Allowed_table_matches_section_3_of_the_plan()
    {
        // section 3: Core: none | Data: Core | Network: Core | Game: Core, Data, Network | Content: Game | Host: all
        Assert.Equal(new[] { "Nro.Content", "Nro.Core", "Nro.Data", "Nro.Game", "Nro.Host", "Nro.Network" },
            RepoGuard.Allowed.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray());
        Assert.Empty(RepoGuard.Allowed["Nro.Core"]);
        Assert.Equal(new[] { "Nro.Core" }, RepoGuard.Allowed["Nro.Data"]);
        Assert.Equal(new[] { "Nro.Core" }, RepoGuard.Allowed["Nro.Network"]);
        Assert.Equal(new[] { "Nro.Core", "Nro.Data", "Nro.Network" }, RepoGuard.Allowed["Nro.Game"].OrderBy(k => k, StringComparer.Ordinal).ToArray());
        Assert.Equal(new[] { "Nro.Game" }, RepoGuard.Allowed["Nro.Content"]);
        Assert.Equal(new[] { "Nro.Content", "Nro.Core", "Nro.Data", "Nro.Game", "Nro.Network" },
            RepoGuard.Allowed["Nro.Host"].OrderBy(k => k, StringComparer.Ordinal).ToArray());
    }

    [Theory]
    [InlineData("Nro.Core")]
    [InlineData("Nro.Data")]
    [InlineData("Nro.Network")]
    [InlineData("Nro.Game")]
    [InlineData("Nro.Content")]
    [InlineData("Nro.Host")]
    public void Real_dependencies_in_project_assets_match_the_rules(string project)
    {
        var problems = RepoGuard.AssetsProblems(TestPaths.RepoRoot()).Where(p => p.StartsWith(project + ":", StringComparison.Ordinal)
            || p.StartsWith("Missing src/" + project + "/", StringComparison.Ordinal)).ToList();

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void No_Directory_Build_targets_Import_or_extra_props()
    {
        var problems = RepoGuard.BuildFileProblems(TestPaths.RepoRoot());

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void No_Compile_Link_outside_the_project_no_binary_Reference_and_no_relaxed_warnings()
    {
        var problems = RepoGuard.CsprojProblems(TestPaths.RepoRoot());

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void Directory_Build_props_keeps_the_quality_guards()
    {
        var props = XDocument.Load(Path.Combine(TestPaths.RepoRoot(), "Directory.Build.props"));
        string Prop(string name) => props.Descendants().FirstOrDefault(e => e.Name.LocalName == name)?.Value.Trim() ?? "";

        Assert.Equal("true", Prop("TreatWarningsAsErrors"));
        Assert.Equal("enable", Prop("Nullable"));
        Assert.Equal("net10.0", Prop("TargetFramework"));
        Assert.Empty(Prop("NoWarn"));
    }

    [Fact]
    public void Every_project_targets_net10()
    {
        foreach (var name in new[] { "Nro.Core", "Nro.Data", "Nro.Network", "Nro.Game", "Nro.Content" })
        {
            var attr = Assembly.Load(name).GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>();
            Assert.NotNull(attr);
            Assert.Equal(".NETCoreApp,Version=v10.0", attr!.FrameworkName);
        }
    }
}
