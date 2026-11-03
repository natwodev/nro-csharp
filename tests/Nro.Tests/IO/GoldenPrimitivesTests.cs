using System.Globalization;
using System.Text.Json;
using Nro.Core.IO;
using Xunit.Abstractions;

namespace Nro.Tests.IO;

/// <summary>
/// Compares ALL cases in <c>tests/golden/primitives.json</c> (generated from the real JDK 17 <c>DataOutputStream</c>/<c>DataInputStream</c> and
/// <c>Message.writeCris/writeShortToLong/writeByteToLong</c>): 115 write cases, 51 read cases, 2 composite cases.
/// Methods that exist on <see cref="JavaDataOutput"/>/<see cref="JavaDataInput"/> are run directly AND through <see cref="Message"/>; the three
/// <c>WriteCris/WriteShortToLong/WriteByteToLong</c> exist only on <see cref="Message"/>, so they run through <see cref="Message"/>.
/// </summary>
public class GoldenPrimitivesTests
{
    private readonly ITestOutputHelper _output;

    public GoldenPrimitivesTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void WriteCases_write_exactly_the_same_bytes_as_the_JDK()
    {
        using JsonDocument doc = GoldenFile.Open("primitives.json");
        JsonElement cases = doc.RootElement.GetProperty("writeCases");
        var failures = new List<string>();
        int compared = 0;
        int viaStream = 0;

        foreach (JsonElement c in cases.EnumerateArray())
        {
            string id = c.GetProperty("id").GetString()!;
            string method = c.GetProperty("method").GetString()!;
            string arg = c.GetProperty("arg").GetString()!;
            string? bits = c.TryGetProperty("argBits", out JsonElement b) ? b.GetString() : null;
            string expected = c.GetProperty("hex").GetString()!;

            var message = new Message(7);
            WriteViaMessage(message, method, arg, bits);
            string actualMessage = GoldenFile.ToHex(message.GetData());
            if (actualMessage != expected)
            {
                failures.Add($"{id}: Message hex JDK={expected} C#={actualMessage}");
            }

            if (method is not ("writeCris" or "writeShortToLong" or "writeByteToLong"))
            {
                var dos = new JavaDataOutput();
                WriteViaStream(dos, method, arg, bits);
                string actualStream = GoldenFile.ToHex(dos.ToArray());
                if (actualStream != expected)
                {
                    failures.Add($"{id}: JavaDataOutput hex JDK={expected} C#={actualStream}");
                }

                if (dos.Size != expected.Length / 2)
                {
                failures.Add($"{id}: Size={dos.Size}, expected {expected.Length / 2}");
                }

                viaStream++;
            }

            compared++;
        }

        _output.WriteLine($"primitives.json writeCases: compared {compared}/{cases.GetArrayLength()} cases ({viaStream} of them also run directly through JavaDataOutput)");
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Assert.Equal(cases.GetArrayLength(), compared);
    }

    [Fact]
    public void ReadCases_read_the_same_value_and_remaining_byte_count_as_the_JDK()
    {
        using JsonDocument doc = GoldenFile.Open("primitives.json");
        JsonElement cases = doc.RootElement.GetProperty("readCases");
        var failures = new List<string>();
        int compared = 0;

        foreach (JsonElement c in cases.EnumerateArray())
        {
            string id = c.GetProperty("id").GetString()!;
            string method = c.GetProperty("method").GetString()!;
            byte[] bytes = GoldenFile.Hex(c.GetProperty("hex").GetString()!);
            JsonElement result = c.GetProperty("result");
            int remaining = c.GetProperty("remainingBytes").GetInt32();

            var input = new JavaDataInput(bytes);
            string viaStream = ReadViaStream(input, method, result);
            var message = new Message(7, bytes);
            string viaMessage = ReadViaMessage(message, method, result);

            string expected = ExpectedRead(method, result);
            if (viaStream != expected)
            {
                failures.Add($"{id}: JavaDataInput returned \"{viaStream}\", JDK \"{expected}\"");
            }

            if (viaMessage != expected)
            {
                failures.Add($"{id}: Message returned \"{viaMessage}\", JDK \"{expected}\"");
            }

            if (input.Available != remaining || message.Reader().Available != remaining)
            {
                failures.Add($"{id}: {input.Available}/{message.Reader().Available} bytes remaining, JDK {remaining}");
            }

            compared++;
        }

        _output.WriteLine($"primitives.json readCases: compared {compared}/{cases.GetArrayLength()} cases");
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Assert.Equal(cases.GetArrayLength(), compared);
    }

    [Fact]
    public void CompositeCases_write_then_read_back_in_the_same_order()
    {
        using JsonDocument doc = GoldenFile.Open("primitives.json");
        JsonElement cases = doc.RootElement.GetProperty("compositeCases");
        var failures = new List<string>();
        int compared = 0;

        foreach (JsonElement c in cases.EnumerateArray())
        {
            string id = c.GetProperty("id").GetString()!;
            string expected = c.GetProperty("hex").GetString()!;
            var ops = new List<(string Method, string Arg)>();
            foreach (JsonElement op in c.GetProperty("ops").EnumerateArray())
            {
                string text = op.GetString()!;
                int colon = text.IndexOf(':');
                ops.Add((text[..colon], text[(colon + 1)..]));
            }

            var dos = new JavaDataOutput();
            var message = new Message(7);
            foreach ((string method, string arg) in ops)
            {
                WriteViaStream(dos, method, arg, null);
                WriteViaMessage(message, method, arg, null);
            }

            if (GoldenFile.ToHex(dos.ToArray()) != expected)
            {
                failures.Add($"{id}: JavaDataOutput produced {GoldenFile.ToHex(dos.ToArray())}, JDK {expected}");
            }

            if (GoldenFile.ToHex(message.GetData()) != expected)
            {
                failures.Add($"{id}: Message produced {GoldenFile.ToHex(message.GetData())}, JDK {expected}");
            }

            // read back with the matching "read" method, in the same order; each must yield exactly the argument that was written
            var reader = new Message(7, GoldenFile.Hex(expected));
            foreach ((string method, string arg) in ops)
            {
                string got = ReadBack(reader, method);
                string want = NormalizeArg(method, arg);
                if (got != want)
                {
                    failures.Add($"{id}: reading back {method} gave \"{got}\", written \"{want}\"");
                }
            }

            if (reader.Reader().Available != 0)
            {
                failures.Add($"{id}: {reader.Reader().Available} bytes left over after reading");
            }

            compared++;
        }

        _output.WriteLine($"primitives.json compositeCases: compared {compared}/{cases.GetArrayLength()} cases");
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Assert.Equal(cases.GetArrayLength(), compared);
    }

    // ------------------------------------------------------------------ write

    private static int ParseInt(string s) => int.Parse(s, CultureInfo.InvariantCulture);

    private static long ParseLong(string s) => long.Parse(s, CultureInfo.InvariantCulture);

    private static float FloatFrom(string arg, string? bits) => bits is null
        ? float.Parse(arg, NumberStyles.Float, CultureInfo.InvariantCulture)
        : BitConverter.Int32BitsToSingle(unchecked((int)uint.Parse(bits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)));

    private static double DoubleFrom(string arg, string? bits) => bits is null
        ? double.Parse(arg, NumberStyles.Float, CultureInfo.InvariantCulture)
        : BitConverter.Int64BitsToDouble(unchecked((long)ulong.Parse(bits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)));

    private static void WriteViaStream(JavaDataOutput dos, string method, string arg, string? bits)
    {
        switch (method)
        {
            case "writeByte": dos.WriteByte(ParseInt(arg)); break;
            case "writeShort": dos.WriteShort(ParseInt(arg)); break;
            case "writeChar": dos.WriteChar(ParseInt(arg)); break;
            case "writeInt": dos.WriteInt(ParseInt(arg)); break;
            case "writeLong": dos.WriteLong(ParseLong(arg)); break;
            case "writeBoolean": dos.WriteBoolean(bool.Parse(arg)); break;
            case "writeFloat": dos.WriteFloat(FloatFrom(arg, bits)); break;
            case "writeDouble": dos.WriteDouble(DoubleFrom(arg, bits)); break;
            case "writeUTF": dos.WriteUTF(arg); break;
            default: throw new InvalidOperationException("Vector contains a write method that is not tested: " + method);
        }
    }

    private static void WriteViaMessage(Message m, string method, string arg, string? bits)
    {
        switch (method)
        {
            case "writeByte": m.WriteByte(ParseInt(arg)); break;
            case "writeShort": m.WriteShort(ParseInt(arg)); break;
            case "writeChar": m.WriteChar(ParseInt(arg)); break;
            case "writeInt": m.WriteInt(ParseInt(arg)); break;
            case "writeLong": m.WriteLong(ParseLong(arg)); break;
            case "writeBoolean": m.WriteBoolean(bool.Parse(arg)); break;
            case "writeFloat": m.WriteFloat(FloatFrom(arg, bits)); break;
            case "writeDouble": m.WriteDouble(DoubleFrom(arg, bits)); break;
            case "writeUTF": m.WriteUTF(arg); break;
            case "writeCris":
            case "writeShortToLong":
            case "writeByteToLong":
            {
                string[] parts = arg.Split(',');
                long v = ParseLong(parts[0]);
                bool flag = bool.Parse(parts[1]);
                if (method == "writeCris")
                {
                    m.WriteCris(v, flag);
                }
                else if (method == "writeShortToLong")
                {
                    m.WriteShortToLong(v, flag);
                }
                else
                {
                    m.WriteByteToLong(v, flag);
                }

                break;
            }

            default: throw new InvalidOperationException("Vector contains a write method that is not tested: " + method);
        }
    }

    // ------------------------------------------------------------------ read

    private static string FloatText(float f) => BitConverter.SingleToInt32Bits(f).ToString("x8", CultureInfo.InvariantCulture);

    private static string DoubleText(double d) => BitConverter.DoubleToInt64Bits(d).ToString("x16", CultureInfo.InvariantCulture);

    /// <summary>Expected value as a comparable string: integers as decimal strings, bool as "true"/"false", float/double as hex bits.</summary>
    private static string ExpectedRead(string method, JsonElement result) => method switch
    {
        "readFloat" or "readDouble" => result.GetProperty("bits").GetString()!,
        _ => result.GetProperty("value").GetString()!,
    };

    private static string ReadViaStream(JavaDataInput input, string method, JsonElement result) => method switch
    {
        "readByte" => input.ReadByte().ToString(CultureInfo.InvariantCulture),
        "readUnsignedByte" => input.ReadUnsignedByte().ToString(CultureInfo.InvariantCulture),
        "readShort" => input.ReadShort().ToString(CultureInfo.InvariantCulture),
        "readUnsignedShort" => input.ReadUnsignedShort().ToString(CultureInfo.InvariantCulture),
        "readChar" => ((int)input.ReadChar()).ToString(CultureInfo.InvariantCulture),
        "readInt" => input.ReadInt().ToString(CultureInfo.InvariantCulture),
        "readLong" => input.ReadLong().ToString(CultureInfo.InvariantCulture),
        "readBoolean" => input.ReadBoolean() ? "true" : "false",
        "readFloat" => FloatText(input.ReadFloat()),
        "readDouble" => DoubleText(input.ReadDouble()),
        _ => throw new InvalidOperationException("Vector contains a read method that is not tested: " + method + " / " + result),
    };

    private static string ReadViaMessage(Message message, string method, JsonElement result) => method switch
    {
        "readByte" => message.ReadByte().ToString(CultureInfo.InvariantCulture),
        "readUnsignedByte" => message.ReadUnsignedByte().ToString(CultureInfo.InvariantCulture),
        "readShort" => message.ReadShort().ToString(CultureInfo.InvariantCulture),
        "readUnsignedShort" => message.ReadUnsignedShort().ToString(CultureInfo.InvariantCulture),
        "readChar" => ((int)message.ReadChar()).ToString(CultureInfo.InvariantCulture),
        "readInt" => message.ReadInt().ToString(CultureInfo.InvariantCulture),
        "readLong" => message.ReadLong().ToString(CultureInfo.InvariantCulture),
        "readBoolean" => message.ReadBoolean() ? "true" : "false",
        "readFloat" => FloatText(message.ReadFloat()),
        "readDouble" => DoubleText(message.ReadDouble()),
        _ => throw new InvalidOperationException("Vector contains a read method that is not tested: " + method + " / " + result),
    };

    /// <summary>Reads back one value according to the WRITE method name (writeX -> the matching readX) and returns it as a string comparable with <see cref="NormalizeArg"/>.</summary>
    private static string ReadBack(Message reader, string writeMethod) => writeMethod switch
    {
        "writeByte" => reader.ReadByte().ToString(CultureInfo.InvariantCulture),
        "writeShort" => reader.ReadShort().ToString(CultureInfo.InvariantCulture),
        "writeInt" => reader.ReadInt().ToString(CultureInfo.InvariantCulture),
        "writeLong" => reader.ReadLong().ToString(CultureInfo.InvariantCulture),
        "writeBoolean" => reader.ReadBoolean() ? "true" : "false",
        "writeUTF" => reader.ReadUTF(),
        "writeFloat" => FloatText(reader.ReadFloat()),
        "writeDouble" => DoubleText(reader.ReadDouble()),
        _ => throw new InvalidOperationException("Composite case contains a method that is not tested: " + writeMethod),
    };

    private static string NormalizeArg(string writeMethod, string arg) => writeMethod switch
    {
        "writeByte" => ((sbyte)ParseInt(arg)).ToString(CultureInfo.InvariantCulture),
        "writeShort" => ((short)ParseInt(arg)).ToString(CultureInfo.InvariantCulture),
        "writeInt" => ParseInt(arg).ToString(CultureInfo.InvariantCulture),
        "writeLong" => ParseLong(arg).ToString(CultureInfo.InvariantCulture),
        "writeBoolean" => bool.Parse(arg) ? "true" : "false",
        "writeUTF" => arg,
        "writeFloat" => FloatText(FloatFrom(arg, null)),
        "writeDouble" => DoubleText(DoubleFrom(arg, null)),
        _ => throw new InvalidOperationException("Composite case contains a method that is not tested: " + writeMethod),
    };
}
