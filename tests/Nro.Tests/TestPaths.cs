namespace Nro.Tests;

/// <summary>Finds paths inside the repo starting from the test run directory (walks up to Nro.sln).</summary>
internal static class TestPaths
{
    public static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Nro.sln")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new InvalidOperationException("Nro.sln was not found when walking up from the test run directory");
        }

        return dir.FullName;
    }

    /// <summary>Golden data file generated from the Java code, located in tests/golden.</summary>
    public static string Golden(string fileName) => Path.Combine(RepoRoot(), "tests", "golden", fileName);
}
