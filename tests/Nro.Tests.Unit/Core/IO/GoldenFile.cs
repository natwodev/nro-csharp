using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Nro.Tests.Unit.Core.IO;

/// <summary>Reads the P0-05 golden vectors (tests/golden, generated from the real Java code) via <see cref="TestPaths.Golden"/>.</summary>
internal static class GoldenFile
{
    public static JsonDocument Open(string fileName) => JsonDocument.Parse(File.ReadAllBytes(TestPaths.Golden(fileName)));

    public static byte[] Hex(string hex) => Convert.FromHexString(hex);

    public static string ToHex(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(bytes);

    public static string Sha256Hex(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    /// <summary>
    /// SHA-256 of the vector file after normalising CRLF to LF (the vectors were generated with LF; git on Windows may turn them into CRLF on checkout,
    /// whereas JSON does not depend on the line-ending style).
    /// </summary>
    public static string Sha256OfFileIgnoringCrLf(string fileName)
    {
        byte[] raw = File.ReadAllBytes(TestPaths.Golden(fileName));
        var normalized = new List<byte>(raw.Length);
        for (int i = 0; i < raw.Length; i++)
        {
            if (raw[i] == 0x0D && i + 1 < raw.Length && raw[i + 1] == 0x0A)
            {
                continue;
            }

            normalized.Add(raw[i]);
        }

        return Sha256Hex(normalized.ToArray());
    }

    /// <summary>Builds a string from a list of UTF-16 units ("utf16" in the vectors): the source of truth, which also preserves lone surrogates.</summary>
    public static string FromUtf16Units(JsonElement units)
    {
        var sb = new StringBuilder(units.GetArrayLength());
        foreach (JsonElement u in units.EnumerateArray())
        {
            sb.Append((char)u.GetInt32());
        }

        return sb.ToString();
    }

    /// <summary>The string of an "inputRepeat" case: the unit (given as code points) repeated <c>count</c> times.</summary>
    public static string FromRepeat(JsonElement repeat)
    {
        var unit = new StringBuilder();
        foreach (JsonElement cp in repeat.GetProperty("unitCodePoints").EnumerateArray())
        {
            unit.Append(char.ConvertFromUtf32(cp.GetInt32()));
        }

        string u = unit.ToString();
        int count = repeat.GetProperty("count").GetInt32();
        var sb = new StringBuilder(u.Length * count);
        for (int i = 0; i < count; i++)
        {
            sb.Append(u);
        }

        return sb.ToString();
    }

    /// <summary>The <c>golden-index.json</c> entry for a vector file (the sha256 and the case count recorded by the generator when it generated them).</summary>
    public static (string Sha256, int Cases) IndexEntry(string fileName)
    {
        using JsonDocument doc = Open("golden-index.json");
        foreach (JsonElement f in doc.RootElement.GetProperty("files").EnumerateArray())
        {
            if (f.GetProperty("name").GetString() == fileName)
            {
                return (f.GetProperty("sha256").GetString()!, f.GetProperty("cases").GetInt32());
            }
        }

        throw new InvalidOperationException($"golden-index.json has no entry for {fileName}");
    }
}
