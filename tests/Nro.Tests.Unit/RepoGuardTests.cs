using System.Text.Json;

namespace Nro.Tests.Unit;

/// <summary>
/// Tests the guard itself: builds a valid FAKE repo tree, then tries each known bypass (audit report F09/F18).
/// Every bypass must be reported by <see cref="RepoGuard"/>; a valid tree reports nothing.
/// </summary>
public sealed class RepoGuardTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "nro-guard-" + Guid.NewGuid().ToString("N"));

    public RepoGuardTests()
    {
        Directory.CreateDirectory(_root);
        Put("Directory.Build.props",
            "<Project><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable>" +
            "<TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>");
        foreach (var (project, allowed) in RepoGuard.Allowed)
        {
            var refs = string.Concat(allowed.Select(a => $"<ProjectReference Include=\"..\\{a}\\{a}.csproj\" />"));
            Put($"src/{project}/{project}.csproj", $"<Project Sdk=\"Microsoft.NET.Sdk\"><ItemGroup>{refs}</ItemGroup></Project>");
            Put($"src/{project}/obj/project.assets.json", Assets(project, allowed, RepoGuard.Closure(project)));
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private void Put(string rel, string text)
    {
        var path = Path.Combine(_root, rel.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    private static string Assets(string project, IEnumerable<string> direct, IEnumerable<string> closure)
    {
        var libs = closure.ToDictionary(c => c + "/1.0.0", _ => (object)new Dictionary<string, object> { ["type"] = "project" });
        var model = new Dictionary<string, object>
        {
            ["version"] = 3,
            ["targets"] = new Dictionary<string, object> { ["net10.0"] = libs },
            ["libraries"] = libs,
            ["projectFileDependencyGroups"] = new Dictionary<string, object>
            {
                ["net10.0"] = direct.Select(d => d + " >= 1.0.0").ToArray(),
            },
            ["project"] = new Dictionary<string, object> { ["name"] = project },
        };
        return JsonSerializer.Serialize(model);
    }

    private List<string> All() =>
        [.. RepoGuard.CsprojProblems(_root), .. RepoGuard.BuildFileProblems(_root), .. RepoGuard.AssetsProblems(_root)];

    [Fact]
    public void Valid_tree_has_no_problems()
    {
        var problems = All();

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void Detects_a_ProjectReference_that_violates_the_rules_in_a_csproj()
    {
        Put("src/Nro.Data/Nro.Data.csproj",
            "<Project Sdk=\"Microsoft.NET.Sdk\"><ItemGroup><ProjectReference Include=\"..\\Nro.Core\\Nro.Core.csproj\" />" +
            "<ProjectReference Include=\"..\\Nro.Network\\Nro.Network.csproj\" /></ItemGroup></Project>");

        Assert.Contains(RepoGuard.CsprojProblems(_root), p => p.Contains("Nro.Data has a disallowed reference: Nro.Network"));
    }

    [Fact]
    public void Detects_a_hidden_real_dependency_in_project_assets_even_when_the_csproj_is_clean()
    {
        // Exactly scenario F09: the csproj is clean but Directory.Build.targets made Data depend on Network at restore time.
        Put("src/Nro.Data/obj/project.assets.json", Assets("Nro.Data", ["Nro.Core", "Nro.Network"], ["Nro.Core", "Nro.Network"]));

        var problems = RepoGuard.AssetsProblems(_root);

        Assert.Contains(problems, p => p.Contains("Nro.Data") && p.Contains("DIRECT") && p.Contains("Nro.Network"));
        Assert.Contains(problems, p => p.Contains("Nro.Data") && p.Contains("does not declare"));
    }

    [Fact]
    public void Detects_a_transitive_dependency_that_violates_the_rules()
    {
        // Core -> Core then indirectly pulls Host into Content: direct is valid but transitive is not.
        Put("src/Nro.Content/obj/project.assets.json", Assets("Nro.Content", ["Nro.Game"], ["Nro.Game", "Nro.Core", "Nro.Data", "Nro.Network", "Nro.Host"]));

        Assert.Contains(RepoGuard.AssetsProblems(_root), p => p.Contains("TRANSITIVE") && p.Contains("Nro.Host"));
    }

    [Fact]
    public void Detects_missing_project_assets()
    {
        File.Delete(Path.Combine(_root, "src", "Nro.Core", "obj", "project.assets.json"));

        Assert.Contains(RepoGuard.AssetsProblems(_root), p => p.StartsWith("Missing src/Nro.Core/obj/project.assets.json", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("src/Directory.Build.targets", "<Project />")]
    [InlineData("Directory.Build.targets", "<Project />")]
    [InlineData("src/Nro.Data/Extra.targets", "<Project />")]
    [InlineData("src/Nro.Data/Extra.props", "<Project />")]
    [InlineData("src/Directory.Build.props", "<Project />")]
    [InlineData("Directory.Build.rsp", "-p:NoWarn=CS8618")]
    [InlineData("Directory.Solution.props", "<Project />")]
    public void Detects_extra_build_files(string rel, string content)
    {
        Put(rel, content);

        Assert.True(RepoGuard.BuildFileProblems(_root).Count > 0, rel);
    }

    [Fact]
    public void Detects_Import_in_a_csproj_and_in_the_root_props()
    {
        Put("src/Nro.Data/Nro.Data.csproj",
            "<Project Sdk=\"Microsoft.NET.Sdk\"><Import Project=\"..\\extra.props\" /><ItemGroup><ProjectReference Include=\"..\\Nro.Core\\Nro.Core.csproj\" /></ItemGroup></Project>");
        Assert.Contains(RepoGuard.BuildFileProblems(_root), p => p.Contains("<Import"));
        Assert.Contains(RepoGuard.CsprojProblems(_root), p => p.Contains("<Import>"));

        Put("src/Nro.Data/Nro.Data.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\" />");
        Put("Directory.Build.props", "<Project><Import Project=\"x.props\" /></Project>");
        Assert.Contains(RepoGuard.BuildFileProblems(_root), p => p.Contains("<Import"));
    }

    [Fact]
    public void Detects_ItemGroup_Target_in_the_root_Directory_Build_props()
    {
        Put("Directory.Build.props",
            "<Project><PropertyGroup><Nullable>enable</Nullable></PropertyGroup>" +
            "<ItemGroup Condition=\"'$(MSBuildProjectName)'=='Nro.Data'\"><ProjectReference Include=\"$(MSBuildThisFileDirectory)src/Nro.Network/Nro.Network.csproj\" /></ItemGroup></Project>");

        var problems = RepoGuard.BuildFileProblems(_root);

        Assert.Contains(problems, p => p.Contains("<ItemGroup>"));
        Assert.Contains(problems, p => p.Contains("<ProjectReference>"));
    }

    [Theory]
    [InlineData("<Compile Include=\"..\\Nro.Network\\**\\*.cs\" />")]
    [InlineData("<Compile Include=\"..\\..\\src\\Nro.Network\\Session.cs\" Link=\"Session.cs\" />")]
    [InlineData("<Compile Include=\"Local.cs\" Link=\"..\\Escape.cs\" />")]
    [InlineData("<Compile Include=\"$(MSBuildThisFileDirectory)..\\Nro.Game\\X.cs\" />")]
    [InlineData("<Compile Include=\"C:\\other\\X.cs\" />")]
    [InlineData("<Compile Update=\"../x.cs\" />")]
    [InlineData("<EmbeddedResource Include=\"..\\..\\secret.resx\" />")]
    [InlineData("<Reference Include=\"Nro.Network\"><HintPath>..\\Nro.Network\\bin\\Nro.Network.dll</HintPath></Reference>")]
    public void Detects_Compile_Link_outside_the_project_and_binary_Reference(string item)
    {
        Put("src/Nro.Data/Nro.Data.csproj",
            "<Project Sdk=\"Microsoft.NET.Sdk\"><ItemGroup><ProjectReference Include=\"..\\Nro.Core\\Nro.Core.csproj\" />" + item + "</ItemGroup></Project>");

        Assert.True(RepoGuard.CsprojProblems(_root).Count > 0, item);
    }

    [Theory]
    [InlineData("<Compile Include=\"Sub\\**\\*.cs\" />")]
    [InlineData("<Compile Include=\"Sub\\..\\Other\\X.cs\" />")]
    [InlineData("<Compile Remove=\"Old\\**\" />")]
    public void Allows_Compile_inside_the_project_directory(string item)
    {
        Put("src/Nro.Data/Nro.Data.csproj",
            "<Project Sdk=\"Microsoft.NET.Sdk\"><ItemGroup><ProjectReference Include=\"..\\Nro.Core\\Nro.Core.csproj\" />" + item + "</ItemGroup></Project>");

        Assert.True(RepoGuard.CsprojProblems(_root).Count == 0, item);
    }

    [Theory]
    [InlineData("<PropertyGroup><NoWarn>CS8618</NoWarn></PropertyGroup>")]
    [InlineData("<PropertyGroup><WarningsNotAsErrors>CS8618</WarningsNotAsErrors></PropertyGroup>")]
    [InlineData("<PropertyGroup><Nullable>disable</Nullable></PropertyGroup>")]
    [InlineData("<PropertyGroup><TreatWarningsAsErrors>false</TreatWarningsAsErrors></PropertyGroup>")]
    public void Detects_relaxed_warnings_in_a_csproj(string group)
    {
        Put("src/Nro.Core/Nro.Core.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\">" + group + "</Project>");

        Assert.True(RepoGuard.CsprojProblems(_root).Count > 0, group);
    }

    [Theory]
    [InlineData("a/b.cs", false)]
    [InlineData("./a/b.cs", false)]
    [InlineData("a/../b.cs", false)]
    [InlineData("**/*.cs", false)]
    [InlineData("../b.cs", true)]
    [InlineData("a/../../b.cs", true)]
    [InlineData("..\\b.cs", true)]
    [InlineData("/abs/b.cs", true)]
    [InlineData("C:/b.cs", true)]
    [InlineData("$(X)/b.cs", true)]
    public void EscapesDirectory_works_correctly(string value, bool expected)
    {
        Assert.Equal(expected, RepoGuard.EscapesDirectory(value));
    }
}
