namespace Nro.Tests.Unit.Db;

public class GiftSchemaTests
{
    [Fact]
    public void Gift_ddl_defines_the_java_columns_and_a_unique_player_id()
    {
        string[] lines = File.ReadAllLines(FindWorkspaceFile("postgres", "schema", "schema.sql"));
        int start = Array.FindIndex(lines, static line => line == "CREATE TABLE IF NOT EXISTS gift (");
        Assert.True(start >= 0, "The gift table was not found in postgres/schema/schema.sql.");
        int end = Array.FindIndex(lines, start, static line => line == ");");
        Assert.True(end > start, "The gift table has no closing delimiter.");

        string ddl = string.Join(Environment.NewLine, lines[(start + 1)..end]);
        Assert.Contains("player_id integer NOT NULL", ddl, StringComparison.Ordinal);
        Assert.Contains("gold_tan_thu boolean NOT NULL DEFAULT false", ddl, StringComparison.Ordinal);
        Assert.Contains("gem_tan_thu boolean NOT NULL DEFAULT false", ddl, StringComparison.Ordinal);
        Assert.Contains("PRIMARY KEY (player_id)", ddl, StringComparison.Ordinal);
    }

    private static string FindWorkspaceFile(params string[] parts)
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            string candidate = Path.Combine([directory.FullName, .. parts]);
            if (File.Exists(candidate)) return candidate;
        }

        throw new FileNotFoundException("Could not find the workspace PostgreSQL schema.");
    }
}
