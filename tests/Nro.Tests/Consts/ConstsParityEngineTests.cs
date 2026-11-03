using System.Text.Json.Nodes;

namespace Nro.Tests.Consts;

/// <summary>
/// Tests the COMPARER of <see cref="ConstsParityTests"/> on its own, on every type in the mapping table that the 20 real consts classes
/// do not fully use: short, long, char, boolean, float, double (including -0.0 and NaN), empty arrays, null arrays, arrays containing
/// null, multi-dimensional arrays, and strings with control characters/NBSP/surrogates/combining marks. The fixture JSON below is the REAL output
/// of port-csharp/tools/consts/ConstsDump.java run on port-csharp/tools/consts/selftest/AllKinds.java
/// (minus the two private/instance fields), with only the non-ASCII characters changed to \uXXXX so it does not depend on the file's encoding.
/// </summary>
public class ConstsParityEngineTests
{
    private const string FixtureClassJson = """
        {
          "name": "AllKinds",
          "package": "consts",
          "modifiers": "public",
          "superclass": "java.lang.Object",
          "interfaces": [],
          "methods": [],
          "nestedClasses": [],
          "fieldCount": 32,
          "fields": [
            {"name": "A_BYTE", "type": "byte[]", "access": "public", "static": true, "final": true, "value": [-1, 127, -128]},
            {"name": "A_CHAR", "type": "char[]", "access": "public", "static": true, "final": true, "value": [97, 7879]},
            {"name": "A_DBL", "type": "double[]", "access": "public", "static": true, "final": true, "value": ["0x3ff0000000000000", "0x7ff0000000000000"]},
            {"name": "A_EMPTY", "type": "String[]", "access": "public", "static": true, "final": true, "value": []},
            {"name": "A_INT", "type": "int[]", "access": "public", "static": true, "final": true, "value": [1, -2, 2147483647]},
            {"name": "A_JAG", "type": "int[][]", "access": "public", "static": true, "final": true, "value": [[1], [], [2, 3]]},
            {"name": "A_NULL", "type": "int[]", "access": "public", "static": true, "final": true, "value": null},
            {"name": "A_STR", "type": "String[]", "access": "public", "static": true, "final": true, "value": ["x", null, "y"]},
            {"name": "B_MAX", "type": "byte", "access": "public", "static": true, "final": true, "value": 127},
            {"name": "B_MIN", "type": "byte", "access": "public", "static": true, "final": true, "value": -128},
            {"name": "CH_A", "type": "char", "access": "public", "static": true, "final": true, "value": 65},
            {"name": "CH_MAX", "type": "char", "access": "public", "static": true, "final": true, "value": 65535},
            {"name": "CH_VN", "type": "char", "access": "public", "static": true, "final": true, "value": 7879},
            {"name": "D_NAN", "type": "double", "access": "public", "static": true, "final": true, "value": "0x7ff8000000000000"},
            {"name": "D_NEG_INF", "type": "double", "access": "public", "static": true, "final": true, "value": "0xfff0000000000000"},
            {"name": "D_TENTH", "type": "double", "access": "public", "static": true, "final": true, "value": "0x3fb999999999999a"},
            {"name": "F_NEG_ZERO", "type": "float", "access": "public", "static": true, "final": true, "value": "0x80000000"},
            {"name": "F_ONE_AND_HALF", "type": "float", "access": "public", "static": true, "final": true, "value": "0x3fc00000"},
            {"name": "I_MIN", "type": "int", "access": "public", "static": true, "final": true, "value": -2147483648},
            {"name": "L_MAX", "type": "long", "access": "public", "static": true, "final": true, "value": 9223372036854775807},
            {"name": "L_MIN", "type": "long", "access": "public", "static": true, "final": true, "value": -9223372036854775808},
            {"name": "MUTABLE", "type": "int", "access": "public", "static": true, "final": false, "value": 5},
            {"name": "NO", "type": "boolean", "access": "public", "static": true, "final": true, "value": false},
            {"name": "S_CONCAT", "type": "String", "access": "public", "static": true, "final": true, "value": "abc\bd"},
            {"name": "S_CTRL", "type": "String", "access": "public", "static": true, "final": true, "value": "a\b\n\t\"\\\u0001\u007f\u0085z"},
            {"name": "S_EMPTY", "type": "String", "access": "public", "static": true, "final": true, "value": ""},
            {"name": "S_MIN", "type": "short", "access": "public", "static": true, "final": true, "value": -32768},
            {"name": "S_NBSP", "type": "String", "access": "public", "static": true, "final": true, "value": "a\u00a0b\u2028c"},
            {"name": "S_NFD", "type": "String", "access": "public", "static": true, "final": true, "value": "e\u0302\u0323"},
            {"name": "S_PAIR", "type": "String", "access": "public", "static": true, "final": true, "value": "\ud83d\ude00"},
            {"name": "S_VN", "type": "String", "access": "public", "static": true, "final": true, "value": "\u0110\u1ea1i h\u1ed9i \u0111\u1ea5u"},
            {"name": "YES", "type": "boolean", "access": "public", "static": true, "final": true, "value": true}
          ]
        }
        """;

    private static Type? ResolveFixture(string simpleName) => simpleName == "AllKinds" ? typeof(AllKindsFixture) : null;

    private static JsonObject FixtureClass() =>
        JsonNode.Parse(FixtureClassJson) as JsonObject ?? throw new InvalidDataException("Fixture JSON is corrupt");

    [Fact]
    public void Fixture_class_with_every_kind_matches_100_percent()
    {
        JsonObject cls = FixtureClass();
        int declared = GoldenJson.Arr(cls["fields"], "fields").Count;

        ParityReport report = ConstsParity.Verify(ResolveFixture, [cls]);

        Assert.True(report.Problems.Count == 0, ConstsParity.Format(report));
        Assert.Equal(declared, report.FieldsChecked);
        Assert.Equal(1, report.ClassesChecked);
        Assert.True(declared >= 30, "The fixture class must cover every kind (at least 30 fields)");
    }

    [Fact]
    public void Fixture_class_every_mutated_field_is_detected()
    {
        string[] names = GoldenJson.Arr(FixtureClass()["fields"], "fields")
            .Select(f => GoldenJson.Str(GoldenJson.Obj(f, "field")["name"], "name")).ToArray();
        var missed = new List<string>();

        foreach (string name in names)
        {
            foreach (string kind in new[] { "value", "type", "final" })
            {
                JsonObject cls = FixtureClass();
                JsonObject field = GoldenJson.Arr(cls["fields"], "fields").Select(f => GoldenJson.Obj(f, "field"))
                    .Single(f => GoldenJson.Str(f["name"], "name") == name);
                string jtype = GoldenJson.Str(field["type"], "type");

                switch (kind)
                {
                    case "value":
                        field["value"] = Perturb(jtype, field["value"]);
                        break;
                    case "type":
                        field["type"] = OtherType(jtype);
                        break;
                    default:
                        field["final"] = !GoldenJson.Bool(field["final"], "final");
                        break;
                }

                ParityReport report = ConstsParity.Verify(ResolveFixture, [cls]);
                // Errors for array elements are reported as "AllKinds.NAME[i]: ...", all other errors as "AllKinds.NAME: ...".
                bool detected = report.Problems.Any(p => p.StartsWith("AllKinds." + name + ":", StringComparison.Ordinal)
                    || p.StartsWith("AllKinds." + name + "[", StringComparison.Ordinal));
                if (!detected)
                {
                    missed.Add($"{name} ({kind} changed from {jtype})");
                }
            }
        }

        Assert.True(missed.Count == 0, "The comparer missed: " + string.Join(", ", missed));
    }

    /// <summary>Returns a value that is guaranteed to differ (even when the original is NaN: switches to a finite number). Always returns a NEW node, never reuses the old one.</summary>
    private static JsonNode? Perturb(string jtype, JsonNode? value)
    {
        if (jtype.EndsWith("[]", StringComparison.Ordinal))
        {
            string element = jtype[..^2];
            var result = new JsonArray();
            if (value is not JsonArray items)
            {
                return result;
            }

            if (items.Count == 0)
            {
                result.Add(Perturb(element, null));
                return result;
            }

            for (int i = 0; i < items.Count; i++)
            {
                result.Add(i == 0 ? Perturb(element, items[i]) : items[i]?.DeepClone());
            }

            return result;
        }

        switch (jtype)
        {
            case "byte" or "short" or "int" or "long" or "char":
                return value is null ? 1L : GoldenJson.Int(value, "number") + 1;
            case "boolean":
                return value is null || !GoldenJson.Bool(value, "bool");
            case "float":
                return value is JsonValue && GoldenJson.Str(value, "bit") == "0x3f800000" ? "0x40000000" : "0x3f800000";
            case "double":
                return value is JsonValue && GoldenJson.Str(value, "bit") == "0x3ff0000000000000" ? "0x4000000000000000" : "0x3ff0000000000000";
            case "String":
                return value is null ? "x" : GoldenJson.Str(value, "string") + "x";
            default:
                throw new InvalidOperationException("Unsupported type: " + jtype);
        }
    }

    private static string OtherType(string jtype)
    {
        if (jtype.EndsWith("[]", StringComparison.Ordinal))
        {
            return jtype[..^2];
        }

        return jtype switch
        {
            "byte" => "short",
            "short" => "int",
            "int" => "long",
            "long" => "int",
            "char" => "short",
            "boolean" => "byte",
            "float" => "double",
            "double" => "float",
            _ => "int",
        };
    }
}

/// <summary>
/// The C# class corresponding to the Java fixture class port-csharp/tools/consts/selftest/AllKinds.java (without HIDDEN and INSTANCE).
/// Only used to test the comparer; it is not production code.
/// </summary>
public static class AllKindsFixture
{
    public const sbyte B_MIN = -128;
    public const sbyte B_MAX = 127;
    public const short S_MIN = -32768;
    public const int I_MIN = -2147483648;
    public const long L_MAX = 9223372036854775807L;
    public const long L_MIN = -9223372036854775808L;

    public const char CH_A = 'A';
    public const char CH_VN = '\u1ec7';
    public const char CH_MAX = '\uffff';
    public const bool YES = true;
    public const bool NO = false;

    public const float F_ONE_AND_HALF = 1.5f;
    public const float F_NEG_ZERO = -0.0f;
    public const double D_TENTH = 0.1;
    public const double D_NAN = double.NaN;
    public const double D_NEG_INF = double.NegativeInfinity;

    public const string S_EMPTY = "";
    public const string S_CTRL = "a\b\n\t\"\\\u0001\u007f\u0085z";
    public const string S_VN = "\u0110\u1ea1i h\u1ed9i \u0111\u1ea5u";
    public const string S_NFD = "e\u0302\u0323";
    public const string S_PAIR = "\ud83d\ude00";
    public const string S_NBSP = "a\u00a0b\u2028c";
    public const string S_CONCAT = "a"
        + "b" + "c\bd";

    public static readonly int[] A_INT = { 1, -2, 2147483647 };
    public static readonly sbyte[] A_BYTE = { -1, 127, -128 };
    public static readonly string[] A_EMPTY = { };
    public static readonly string?[] A_STR = { "x", null, "y" };
    public static readonly int[][] A_JAG = { new[] { 1 }, new int[0], new[] { 2, 3 } };
    public static readonly int[]? A_NULL = null;
    public static readonly char[] A_CHAR = { 'a', '\u1ec7' };
    public static readonly double[] A_DBL = { 1.0, double.PositiveInfinity };

    public static int MUTABLE = 5;
}
