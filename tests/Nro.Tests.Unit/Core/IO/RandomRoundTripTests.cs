using Nro.Core.Util;
using Nro.Core.IO;
using Xunit.Abstractions;

namespace Nro.Tests.Unit.Core.IO;

/// <summary>
/// Random round-trip with a fixed seed (<see cref="SplitMix64"/>): at least 10,000 strings and at least 10,000 number tuples. Besides reading back correctly,
/// the bytes written are compared with <see cref="ReferenceCodec"/> (written independently), so a test cannot be "right because of a symmetric mistake". The coverage
/// assertions (NUL, lone surrogates, surrogate pairs, every 256-unit block of the BMP, lengths right at 65535) live inside the test itself.
/// </summary>
public class RandomRoundTripTests
{
    private const int StringCases = 10_500;
    private const int TupleCases = 10_500;

    private readonly ITestOutputHelper _output;

    public RandomRoundTripTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Random_strings_written_match_the_reference_encoding_and_read_back_correctly()
    {
        var g = new SplitMix64(4242);
        var blocksSeen = new bool[256];
        int ok = 0;
        int tooLong = 0;
        int withNul = 0;
        int withLoneSurrogate = 0;
        int withPair = 0;
        int exactly65535 = 0;
        int oneByteSeen = 0;
        int twoByteSeen = 0;
        int threeByteSeen = 0;

        for (int i = 0; i < StringCases; i++)
        {
            string s = DifferentialGenerators.GenString(g);
            foreach (char ch in s)
            {
                blocksSeen[ch >> 8] = true;
            }

            if (s.Contains('\0'))
            {
                withNul++;
            }

            (bool pair, bool lone) = SurrogateKinds(s);
            if (pair)
            {
                withPair++;
            }

            if (lone)
            {
                withLoneSurrogate++;
            }

            long encodedLength = ReferenceCodec.Utf8Length(s);
            if (encodedLength == 65535)
            {
                exactly65535++;
            }

            if (s.Any(ch => ch is >= '\u0001' and <= '\u007F'))
            {
                oneByteSeen++;
            }

            if (s.Any(ch => ch == '\0' || ch is >= '\u0080' and <= '߿'))
            {
                twoByteSeen++;
            }

            if (s.Any(ch => ch >= 'ࠀ'))
            {
                threeByteSeen++;
            }

            byte[]? reference = ReferenceCodec.EncodeUtf(s);
            var dos = new JavaDataOutput();
            var message = new Message(1);
            if (reference is null)
            {
                UtfDataFormatException ex = Assert.Throws<UtfDataFormatException>(() => dos.WriteUTF(s));
                UtfDataFormatException exMessage = Assert.Throws<UtfDataFormatException>(() => message.WriteUTF(s));
                string expectedText = $"encoded string ({s[..8]}...{s[^8..]}) too long: {encodedLength} bytes";
                if (ex.Message != expectedText || exMessage.Message != expectedText || dos.Size != 0 || message.GetData().Length != 0)
                {
                    Assert.Fail($"case {i}: the too-long error has the wrong message or part of the data was already written (Size={dos.Size})");
                }

                tooLong++;
                continue;
            }

            dos.WriteUTF(s);
            message.WriteUTF(s);
            byte[] wire = dos.ToArray();
            if (!reference.AsSpan().SequenceEqual(wire) || !wire.AsSpan().SequenceEqual(message.GetData()))
            {
                Assert.Fail($"case {i}: bytes written differ from the reference encoding (string length {s.Length}, wrote {wire.Length} bytes)");
            }

            var back = new JavaDataInput(wire);
            string read = back.ReadUTF();
            if (!string.Equals(read, s, StringComparison.Ordinal) || back.Available != 0)
            {
                Assert.Fail($"case {i}: read-back differs from the original string or bytes are left over ({back.Available} left)");
            }

            ok++;
        }

        _output.WriteLine(
            $"strings={StringCases} writable={ok} tooLong={tooLong} withNul={withNul} withLoneSurrogate={withLoneSurrogate} withPair={withPair} " +
            $"exactly65535={exactly65535} bmpBlocks={blocksSeen.Count(b => b)}/256");

        Assert.Equal(StringCases, ok + tooLong);
        Assert.True(withNul > 100, "missing strings containing NUL");
        Assert.True(withLoneSurrogate > 100, "missing strings containing lone surrogates");
        Assert.True(withPair > 100, "missing strings containing surrogate pairs");
        Assert.True(oneByteSeen > 100 && twoByteSeen > 100 && threeByteSeen > 100, "missing 1/2/3-byte characters");
        Assert.True(exactly65535 > 10, "missing strings that encode to exactly 65535 bytes");
        Assert.True(tooLong > 10, "missing strings that exceed 65535 bytes");
        Assert.True(blocksSeen.All(b => b), "not all 256 BMP blocks are covered");
    }

    [Fact]
    public void Random_number_tuples_written_match_the_reference_and_read_back_correctly_for_every_kind()
    {
        var g = new SplitMix64(777);
        var kindCounts = new int[16];

        for (int i = 0; i < TupleCases; i++)
        {
            int cmd = g.Below(1024) - 512;
            var message = new Message(cmd);
            sbyte expectedCommand = (sbyte)(((cmd + 128) & 0xFF) - 128);
            if (message.Command != expectedCommand)
            {
                Assert.Fail($"case {i}: Message({cmd}).Command = {message.Command}, expected {expectedCommand}");
            }

            var expected = new List<byte>();
            var ops = new List<Op>();
            int opCount = 1 + g.Below(6);
            for (int o = 0; o < opCount; o++)
            {
                Op op = NextOp(g);
                kindCounts[op.Kind]++;
                ops.Add(op);
                Apply(message, op);
                expected.AddRange(ExpectedBytes(op));
                if (message.Writer().Size != expected.Count)
                {
                    Assert.Fail($"case {i}: after an operation of kind {op.Kind} Size={message.Writer().Size}, expected {expected.Count}");
                }
            }

            byte[] data = message.GetData();
            if (!expected.ToArray().AsSpan().SequenceEqual(data))
            {
                Assert.Fail($"case {i}: bytes written differ from the reference: {GoldenFile.ToHex(data)} vs {GoldenFile.ToHex(expected.ToArray())}");
            }

            var reader = new Message(message.Command, data);
            for (int o = 0; o < ops.Count; o++)
            {
                string? problem = Verify(reader, ops[o], (i + o) % 2 == 0);
                if (problem is not null)
                {
                    Assert.Fail($"case {i}, operation {o} (kind {ops[o].Kind}): {problem}");
                }
            }

            if (reader.Reader().Available != 0)
            {
                Assert.Fail($"case {i}: {reader.Reader().Available} bytes left over after reading");
            }
        }

        _output.WriteLine("operation count by kind: " + string.Join(",", kindCounts));
        Assert.True(kindCounts.All(c => c > 500), "each number kind must be tried at least 500 times");
    }

    private static (bool Pair, bool Lone) SurrogateKinds(string s)
    {
        bool pair = false;
        bool lone = false;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (char.IsHighSurrogate(c))
            {
                if (i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
                {
                    pair = true;
                    i++;
                }
                else
                {
                    lone = true;
                }
            }
            else if (char.IsLowSurrogate(c))
            {
                lone = true;
            }
        }

        return (pair, lone);
    }

    // ------------------------------------------------------------------ write/read operations

    private readonly record struct Op(int Kind, long Value, bool Flag, string? Text, byte[]? Raw, int Offset, int Count);

    private static Op NextOp(SplitMix64 g)
    {
        int kind = g.Below(16);
        switch (kind)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 13:
                return new Op(kind, DifferentialGenerators.InterestingInt(g), false, null, null, 0, 0);
            case 4:
                return new Op(kind, DifferentialGenerators.InterestingLong(g), false, null, null, 0, 0);
            case 5:
                return new Op(kind, 0, g.Below(2) == 1, null, null, 0, 0);
            case 6:
                return new Op(kind, DifferentialGenerators.InterestingInt(g), false, null, null, 0, 0);
            case 7:
                return new Op(kind, DifferentialGenerators.InterestingLong(g), false, null, null, 0, 0);
            case 8:
            case 9:
            case 10:
            {
                long v = DifferentialGenerators.InterestingLong(g);
                return new Op(kind, v, g.Below(2) == 1, null, null, 0, 0);
            }

            case 11:
            case 12:
            case 15:
                return new Op(kind, 0, false, DifferentialGenerators.ShortString(g), null, 0, 0);
            default:
            {
                int len = g.Below(9);
                var raw = new byte[len];
                for (int j = 0; j < len; j++)
                {
                    raw[j] = (byte)g.Below(256);
                }

                int off = g.Below(len + 1);
                int cnt = g.Below(len - off + 1);
                return new Op(kind, 0, false, null, raw, off, cnt);
            }
        }
    }

    private static void Apply(Message m, Op op)
    {
        switch (op.Kind)
        {
            case 0: m.WriteByte((int)op.Value); break;
            case 1: m.WriteShort((int)op.Value); break;
            case 2: m.WriteChar((int)op.Value); break;
            case 3: m.WriteInt((int)op.Value); break;
            case 4: m.WriteLong(op.Value); break;
            case 5: m.WriteBoolean(op.Flag); break;
            case 6: m.WriteFloat(BitConverter.Int32BitsToSingle((int)op.Value)); break;
            case 7: m.WriteDouble(BitConverter.Int64BitsToDouble(op.Value)); break;
            case 8: m.WriteCris(op.Value, op.Flag); break;
            case 9: m.WriteShortToLong(op.Value, op.Flag); break;
            case 10: m.WriteByteToLong(op.Value, op.Flag); break;
            case 11: m.WriteBytes(op.Text!); break;
            case 12: m.WriteChars(op.Text!); break;
            case 13: m.Write((int)op.Value); break;
            case 14: m.Write(op.Raw!, op.Offset, op.Count); break;
            default: m.WriteUTF(op.Text!); break;
        }
    }

    private static IEnumerable<byte> ExpectedBytes(Op op)
    {
        switch (op.Kind)
        {
            case 0:
            case 13:
                return ReferenceCodec.BigEndian(op.Value, 1);
            case 1:
            case 2:
                return ReferenceCodec.BigEndian(op.Value, 2);
            case 3:
                return ReferenceCodec.BigEndian(op.Value, 4);
            case 4:
                return ReferenceCodec.BigEndian(op.Value, 8);
            case 5:
                return [(byte)(op.Flag ? 1 : 0)];
            case 6:
                return ReferenceCodec.BigEndian(ReferenceCodec.FloatBits(BitConverter.Int32BitsToSingle((int)op.Value)), 4);
            case 7:
                return ReferenceCodec.BigEndian(ReferenceCodec.DoubleBits(BitConverter.Int64BitsToDouble(op.Value)), 8);
            case 8:
                return ReferenceCodec.BigEndian(op.Value, op.Flag ? 4 : 8);
            case 9:
                return ReferenceCodec.BigEndian(op.Value, op.Flag ? 2 : 8);
            case 10:
                return ReferenceCodec.BigEndian(op.Value, op.Flag ? 1 : 8);
            case 11:
            {
                string text = op.Text!;
                return text.Select(c => (byte)(c & 0xFF)).ToArray();
            }

            case 12:
            {
                string text = op.Text!;
                return text.SelectMany(c => ReferenceCodec.BigEndian(c, 2)).ToArray();
            }

            case 14:
            {
                byte[] raw = op.Raw!;
                return raw.AsSpan(op.Offset, op.Count).ToArray();
            }

            default:
            {
                string text = op.Text!;
                return ReferenceCodec.EncodeUtf(text)!;
            }
        }
    }

    /// <summary>Reads back one operation; <paramref name="useUnsigned"/> alternates between signed and unsigned reads for the kinds that have both. Returns null if correct.</summary>
    private static string? Verify(Message r, Op op, bool useUnsigned)
    {
        switch (op.Kind)
        {
            case 0:
            {
                int low = (int)(op.Value & 0xFF);
                return useUnsigned
                    ? (r.ReadUnsignedByte() == low ? null : "ReadUnsignedByte")
                    : (r.ReadByte() == (sbyte)low ? null : "ReadByte");
            }

            case 1:
            {
                int low = (int)(op.Value & 0xFFFF);
                return useUnsigned
                    ? (r.ReadUnsignedShort() == low ? null : "ReadUnsignedShort")
                    : (r.ReadShort() == (short)low ? null : "ReadShort");
            }

            case 2:
                return r.ReadChar() == (char)(op.Value & 0xFFFF) ? null : "ReadChar";
            case 3:
                return r.ReadInt() == (int)op.Value ? null : "ReadInt";
            case 4:
                return r.ReadLong() == op.Value ? null : "ReadLong";
            case 5:
                return r.ReadBoolean() == op.Flag ? null : "ReadBoolean";
            case 6:
                return BitConverter.SingleToInt32Bits(r.ReadFloat()) == ReferenceCodec.FloatBits(BitConverter.Int32BitsToSingle((int)op.Value)) ? null : "ReadFloat";
            case 7:
                return BitConverter.DoubleToInt64Bits(r.ReadDouble()) == ReferenceCodec.DoubleBits(BitConverter.Int64BitsToDouble(op.Value)) ? null : "ReadDouble";
            case 8:
                return op.Flag
                    ? (r.ReadInt() == (int)op.Value ? null : "ReadInt (cris)")
                    : (r.ReadLong() == op.Value ? null : "ReadLong (cris)");
            case 9:
                return op.Flag
                    ? (r.ReadShort() == (short)op.Value ? null : "ReadShort (shortToLong)")
                    : (r.ReadLong() == op.Value ? null : "ReadLong (shortToLong)");
            case 10:
                return op.Flag
                    ? (r.ReadByte() == (sbyte)op.Value ? null : "ReadByte (byteToLong)")
                    : (r.ReadLong() == op.Value ? null : "ReadLong (byteToLong)");
            case 11:
            {
                string text = op.Text!;
                var buf = new byte[text.Length];
                r.ReadFully(buf);
                return buf.AsSpan().SequenceEqual(text.Select(c => (byte)(c & 0xFF)).ToArray()) ? null : "ReadFully (writeBytes)";
            }

            case 12:
            {
                string text = op.Text!;
                foreach (char c in text)
                {
                    if (r.ReadChar() != c)
                    {
                        return "ReadChar (writeChars)";
                    }
                }

                return null;
            }

            case 13:
                return r.ReadUnsignedByte() == (int)(op.Value & 0xFF) ? null : "ReadUnsignedByte (write)";
            case 14:
            {
                byte[] raw = op.Raw!;
                var buf = new byte[op.Count];
                r.ReadFully(buf, 0, op.Count);
                return buf.AsSpan().SequenceEqual(raw.AsSpan(op.Offset, op.Count)) ? null : "ReadFully (write array)";
            }

            default:
                return string.Equals(r.ReadUTF(), op.Text!, StringComparison.Ordinal) ? null : "ReadUTF";
        }
    }
}
