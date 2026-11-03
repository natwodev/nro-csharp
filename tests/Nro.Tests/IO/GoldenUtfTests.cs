using System.Text.Json;
using Nro.Core.IO;
using Xunit.Abstractions;

namespace Nro.Tests.IO;

/// <summary>
/// Compares ALL cases in <c>tests/golden/utf.json</c> (generated from the real JDK 17 <c>DataOutputStream.writeUTF</c>/<c>DataInputStream.readUTF</c>):
/// 30 write cases, 2 write-error cases, 17 read cases. Each case runs through both <see cref="JavaDataOutput"/>/<see cref="JavaDataInput"/> and <see cref="Message"/>.
/// </summary>
public class GoldenUtfTests
{
    private readonly ITestOutputHelper _output;

    public GoldenUtfTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void WriteCases_write_exactly_the_same_bytes_as_the_JDK_and_read_back_correctly()
    {
        using JsonDocument doc = GoldenFile.Open("utf.json");
        JsonElement cases = doc.RootElement.GetProperty("writeCases");
        var failures = new List<string>();
        int compared = 0;

        foreach (JsonElement c in cases.EnumerateArray())
        {
            string id = c.GetProperty("id").GetString()!;
            string s = c.TryGetProperty("inputRepeat", out JsonElement repeat)
                ? GoldenFile.FromRepeat(repeat)
                : GoldenFile.FromUtf16Units(c.GetProperty("utf16"));

            var dos = new JavaDataOutput();
            dos.WriteUTF(s);
            byte[] wire = dos.ToArray();

            var message = new Message(1);
            message.WriteUTF(s);
            byte[] viaMessage = message.GetData();

            CompareWire(failures, id, c, wire);
            if (!wire.AsSpan().SequenceEqual(viaMessage))
            {
                failures.Add($"{id}: Message.WriteUTF output differs from JavaDataOutput.WriteUTF");
            }

            if (c.GetProperty("utfLength").GetInt32() != wire.Length - 2)
            {
                failures.Add($"{id}: utfLength={c.GetProperty("utfLength").GetInt32()} but bytes written = {wire.Length - 2}");
            }

            if (dos.Size != wire.Length)
            {
                failures.Add($"{id}: Size={dos.Size} differs from bytes written {wire.Length}");
            }

            if (c.GetProperty("roundTrip").GetBoolean())
            {
                var back = new JavaDataInput(wire);
                string read = back.ReadUTF();
                if (!string.Equals(read, s, StringComparison.Ordinal))
                {
                    failures.Add($"{id}: read back a different string (length {read.Length} vs {s.Length})");
                }

                if (back.Available != 0)
                {
                    failures.Add($"{id}: {back.Available} bytes left over after reading");
                }

                var backMessage = new Message(1, wire);
                if (!string.Equals(backMessage.ReadUTF(), s, StringComparison.Ordinal))
                {
                    failures.Add($"{id}: Message.ReadUTF returned a different string");
                }
            }

            compared++;
        }

        _output.WriteLine($"utf.json writeCases: compared {compared}/{cases.GetArrayLength()} cases");
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Assert.Equal(cases.GetArrayLength(), compared);
    }

    [Fact]
    public void WriteErrors_throw_the_right_type_and_message_and_write_no_bytes()
    {
        using JsonDocument doc = GoldenFile.Open("utf.json");
        JsonElement cases = doc.RootElement.GetProperty("writeErrors");
        var failures = new List<string>();
        int compared = 0;

        foreach (JsonElement c in cases.EnumerateArray())
        {
            string id = c.GetProperty("id").GetString()!;
            string s = GoldenFile.FromRepeat(c.GetProperty("inputRepeat"));
            JsonElement throws = c.GetProperty("throws");
            string javaClass = throws.GetProperty("class").GetString()!;
            string javaMessage = throws.GetProperty("message").GetString()!;

            if (javaClass != "java.io.UTFDataFormatException")
            {
                failures.Add($"{id}: vector has an unexpected exception class {javaClass}");
                continue;
            }

            // 1 byte is already present beforehand: the error must occur BEFORE writing and must not corrupt the existing data.
            var dos = new JavaDataOutput();
            dos.WriteByte(0x55);
            Exception? ex = Record.Exception(() => dos.WriteUTF(s));
            CheckException(failures, id + " (JavaDataOutput)", ex, typeof(UtfDataFormatException), javaMessage);
            if (dos.Size != 1 || !dos.ToArray().AsSpan().SequenceEqual(new byte[] { 0x55 }))
            {
                failures.Add($"{id}: buffer not left intact after the error (Size={dos.Size})");
            }

            var message = new Message(1);
            message.WriteByte(0x55);
            Exception? exMessage = Record.Exception(() => message.WriteUTF(s));
            CheckException(failures, id + " (Message)", exMessage, typeof(UtfDataFormatException), javaMessage);
            if (!message.GetData().AsSpan().SequenceEqual(new byte[] { 0x55 }))
            {
                failures.Add($"{id}: Message not left intact after the error");
            }

            compared++;
        }

        _output.WriteLine($"utf.json writeErrors: compared {compared}/{cases.GetArrayLength()} cases");
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Assert.Equal(cases.GetArrayLength(), compared);
    }

    [Fact]
    public void ReadCases_read_the_same_value_or_exception_as_the_JDK()
    {
        using JsonDocument doc = GoldenFile.Open("utf.json");
        JsonElement cases = doc.RootElement.GetProperty("readCases");
        var failures = new List<string>();
        int compared = 0;

        foreach (JsonElement c in cases.EnumerateArray())
        {
            string id = c.GetProperty("id").GetString()!;
            byte[] bytes = GoldenFile.Hex(c.GetProperty("hex").GetString()!);
            JsonElement result = c.GetProperty("result");

            // two read paths: directly and through Message
            for (int via = 0; via < 2; via++)
            {
                string label = id + (via == 0 ? " (JavaDataInput)" : " (Message)");
                var input = new JavaDataInput(bytes);
                var message = new Message(1, bytes);
                Func<string> readUtf = via == 0 ? input.ReadUTF : message.ReadUTF;
                int Available() => via == 0 ? input.Available : message.Reader().Available;

                if (result.TryGetProperty("throws", out JsonElement throws))
                {
                    Type expectedType = MapJavaException(throws.GetProperty("class").GetString()!);
                    string? expectedMessage = throws.GetProperty("message").ValueKind == JsonValueKind.Null
                        ? null
                        : throws.GetProperty("message").GetString();
                    Exception? ex = Record.Exception(() => readUtf());
                    CheckException(failures, label, ex, expectedType, expectedMessage);
                }
                else
                {
                    string got = readUtf();
                    string expected = GoldenFile.FromUtf16Units(result.GetProperty("utf16"));
                    if (!string.Equals(got, expected, StringComparison.Ordinal))
                    {
                        failures.Add($"{label}: read a different string (length {got.Length}, expected {expected.Length})");
                    }

                    if (result.TryGetProperty("input", out JsonElement input2) && !string.Equals(got, input2.GetString(), StringComparison.Ordinal))
                    {
                        failures.Add($"{label}: differs from the vector's input field");
                    }

                    int remaining = result.GetProperty("remainingBytes").GetInt32();
                    if (Available() != remaining)
                    {
                        failures.Add($"{label}: {Available()} bytes unread, vector says {remaining}");
                    }
                }
            }

            compared++;
        }

        _output.WriteLine($"utf.json readCases: compared {compared}/{cases.GetArrayLength()} cases");
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Assert.Equal(cases.GetArrayLength(), compared);
    }

    private static Type MapJavaException(string javaClass) => javaClass switch
    {
        "java.io.UTFDataFormatException" => typeof(UtfDataFormatException),
        "java.io.EOFException" => typeof(EndOfStreamException),
        _ => throw new InvalidOperationException("Vector has an exception class that is not mapped: " + javaClass),
    };

    /// <summary>Must be exactly the right TYPE (a subclass or superclass is not accepted) and, if the vector has a message, exactly the right message.</summary>
    private static void CheckException(List<string> failures, string label, Exception? ex, Type expectedType, string? expectedMessage)
    {
        if (ex is null)
        {
            failures.Add($"{label}: no exception thrown, expected {expectedType.Name}");
            return;
        }

        if (ex.GetType() != expectedType)
        {
            failures.Add($"{label}: threw {ex.GetType().Name} ({ex.Message}), expected {expectedType.Name}");
            return;
        }

        if (expectedMessage is not null && ex.Message != expectedMessage)
        {
            failures.Add($"{label}: message \"{ex.Message}\", JDK: \"{expectedMessage}\"");
        }
    }

    private static void CompareWire(List<string> failures, string id, JsonElement c, byte[] wire)
    {
        if (c.TryGetProperty("hex", out JsonElement hex))
        {
            string expected = hex.GetString()!;
            string actual = GoldenFile.ToHex(wire);
            if (expected != actual)
            {
                failures.Add($"{id}: hex JDK={expected} C#={actual}");
            }

            return;
        }

        // large payload: the vector keeps only the length, the first 16 bytes and the SHA-256
        int expectedLength = c.GetProperty("wireLength").GetInt32();
        if (wire.Length != expectedLength)
        {
            failures.Add($"{id}: length {wire.Length} bytes, JDK {expectedLength}");
        }

        string expectedHead = c.GetProperty("wireHead16Hex").GetString()!;
        string actualHead = GoldenFile.ToHex(wire.AsSpan(0, Math.Min(16, wire.Length)));
        if (expectedHead != actualHead)
        {
            failures.Add($"{id}: first 16 bytes JDK={expectedHead} C#={actualHead}");
        }

        string expectedSha = c.GetProperty("wireSha256").GetString()!;
        string actualSha = GoldenFile.Sha256Hex(wire);
        if (expectedSha != actualSha)
        {
            failures.Add($"{id}: SHA-256 JDK={expectedSha} C#={actualSha}");
        }
    }
}
