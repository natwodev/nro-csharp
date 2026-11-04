using System.Reflection;
using System.Text.Json;
using Nro.Network.Configurations;
using Nro.Network.Framing;
using Nro.Tests.Unit.Core.IO;

namespace Nro.Tests.Unit.Network.Configurations;

/// <summary>
/// Compares the C# <see cref="Cmd_message"/> with the REAL values of the Java class <c>network/logic/Cmd_message.java</c>
/// (tests/golden/cmd-message.json, produced by port-csharp/tools/network/gen_cmd_message.py from the class compiled with javac).
/// Every Java constant must exist in C# with the same value and the C# class must have no extra constant.
/// </summary>
public class CmdMessageParityTests
{
    private static readonly Lazy<JsonDocument> Doc = new(() => GoldenFile.Open("cmd-message.json"));

    private static Dictionary<string, int> GoldenValues()
    {
        JsonElement cls = Doc.Value.RootElement.GetProperty("classes")[0];
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (JsonElement f in cls.GetProperty("fields").EnumerateArray())
        {
            Assert.Equal("byte", f.GetProperty("type").GetString());
            map.Add(f.GetProperty("name").GetString()!, f.GetProperty("value").GetInt32());
        }

        return map;
    }

    private static Dictionary<string, int> CSharpValues() =>
        typeof(Cmd_message).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToDictionary(f => f.Name, f => Convert.ToInt32(f.GetRawConstantValue()), StringComparer.Ordinal);

    [Fact]
    public void Golden_describes_one_class_with_288_byte_constants()
    {
        JsonElement root = Doc.Value.RootElement;
        Assert.Equal("nro-consts-golden/1", root.GetProperty("format").GetString());
        Assert.Equal(1, root.GetProperty("classCount").GetInt32());
        Assert.Equal(288, root.GetProperty("fieldCount").GetInt32());
        Assert.Equal(288, GoldenValues().Count);
    }

    [Fact]
    public void Every_Java_constant_exists_in_CSharp_with_the_same_value_and_there_are_no_extras()
    {
        Dictionary<string, int> java = GoldenValues();
        Dictionary<string, int> cs = CSharpValues();

        Assert.Empty(java.Keys.Except(cs.Keys));
        Assert.Empty(cs.Keys.Except(java.Keys));
        foreach ((string name, int value) in java)
        {
            Assert.True(cs[name] == value, $"{name}: Java {value}, C# {cs[name]}");
        }
    }

    [Fact]
    public void Constants_are_sbyte_compile_time_constants_in_a_public_static_class()
    {
        Type type = typeof(Cmd_message);
        Assert.True(type.IsPublic && type.IsAbstract && type.IsSealed, "Cmd_message must be a public static class");
        foreach (FieldInfo f in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            Assert.True(f.IsLiteral && f.FieldType == typeof(sbyte), $"{f.Name} must be a `const sbyte`");
        }
    }

    [Fact]
    public void The_comparison_has_teeth_a_wrong_value_a_missing_and_an_extra_constant_are_all_detected()
    {
        Dictionary<string, int> java = GoldenValues();
        Dictionary<string, int> cs = CSharpValues();

        var wrong = new Dictionary<string, int>(cs) { ["LOGIN"] = cs["LOGIN"] + 1 };
        Assert.Contains(java, kv => wrong[kv.Key] != kv.Value);

        var missing = new Dictionary<string, int>(cs);
        missing.Remove("GET_SESSION_ID");
        Assert.NotEmpty(java.Keys.Except(missing.Keys));

        var extra = new Dictionary<string, int>(cs) { ["EXTRA"] = 1 };
        Assert.NotEmpty(extra.Keys.Except(java.Keys));
    }

    [Fact]
    public void The_wire_constants_agree_with_Cmd_message()
    {
        Assert.Equal(Cmd_message.GET_SESSION_ID, WireCommands.GetSessionId);

        // Every command that uses a 3-byte size on the wire must be a real command of the protocol.
        HashSet<int> known = CSharpValues().Values.ToHashSet();
        foreach (sbyte cmd in WireCommands.LargeSizeCommands)
        {
            Assert.Contains((int)cmd, known);
        }
    }
}
