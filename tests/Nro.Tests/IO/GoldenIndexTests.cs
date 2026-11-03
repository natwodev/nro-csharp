using System.Text.Json;

namespace Nro.Tests.IO;

/// <summary>
/// Keeps the golden vectors that the IO tests rely on from being hand-edited or going stale: the SHA-256 and the case count must match <c>golden-index.json</c>
/// (written by the P0-05 harness when it generated them from the real Java code). If the vectors are legitimately regenerated, the index changes with them and the tests still pass.
/// </summary>
public class GoldenIndexTests
{
    [Theory]
    [InlineData("utf.json")]
    [InlineData("primitives.json")]
    public void Vector_matches_sha256_in_index(string fileName)
    {
        (string expectedSha, _) = GoldenFile.IndexEntry(fileName);
        string actualSha = GoldenFile.Sha256OfFileIgnoringCrLf(fileName);

        Assert.Equal(expectedSha, actualSha);
    }

    [Fact]
    public void Case_count_in_utf_json_equals_case_count_in_index()
    {
        using JsonDocument doc = GoldenFile.Open("utf.json");
        JsonElement root = doc.RootElement;
        int total = root.GetProperty("writeCases").GetArrayLength()
            + root.GetProperty("writeErrors").GetArrayLength()
            + root.GetProperty("readCases").GetArrayLength();

        Assert.Equal(GoldenFile.IndexEntry("utf.json").Cases, total);
    }

    [Fact]
    public void Case_count_in_primitives_json_equals_case_count_in_index()
    {
        using JsonDocument doc = GoldenFile.Open("primitives.json");
        JsonElement root = doc.RootElement;
        int total = root.GetProperty("writeCases").GetArrayLength()
            + root.GetProperty("readCases").GetArrayLength()
            + root.GetProperty("compositeCases").GetArrayLength();

        Assert.Equal(GoldenFile.IndexEntry("primitives.json").Cases, total);
    }
}
