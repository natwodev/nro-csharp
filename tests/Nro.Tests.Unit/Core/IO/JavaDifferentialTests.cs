using System.Buffers.Binary;
using System.Security.Cryptography;
using Nro.Core.Util;
using Nro.Core.IO;
using Xunit.Abstractions;

namespace Nro.Tests.Unit.Core.IO;

/// <summary>
/// Matches the REAL JDK 17 on deterministic pseudo-random data (12,000 strings to write, 12,000 byte blocks to read, 12,000 number packets), via SHA-256.
/// The SHA-256 constants below were produced by <c>JavaDifferentialReference.java.txt</c> (same folder): that program runs the JDK's
/// <c>DataOutputStream/DataInputStream</c> and the project's own <c>network.io.Message</c> class with the SAME generator <see cref="SplitMix64"/>
/// and the SAME data-generation algorithm as <see cref="DifferentialGenerators"/>. A one-byte difference in any case changes the SHA-256.
/// </summary>
public class JavaDifferentialTests
{
    // Output of DiffGen.main on JDK 17.0.19 (see JavaDifferentialReference.java.txt to re-run it).
    private const long WriteUtfSeed = 20260101L;
    private const long ReadUtfSeed = 20260102L;
    private const long NumbersSeed = 20260103L;
    private const int Cases = 12000;

    private const string JdkWriteUtfSha256 = "725c58e199e17dfa06919e99613e2e7c383485f79a6d9bb7e88964604110143f";
    private const string JdkReadUtfSha256 = "5e9b8c9ce29e9f0fa9f5ed208d8397c2eb8e21302779db6e8207cc971ef04d88";
    private const string JdkNumbersSha256 = "f58c6aae6bf2ddf86941c2943fe487c6d6d4410babd712a3a3ab12f7addc5e20";

    private readonly ITestOutputHelper _output;

    public JavaDifferentialTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void WriteUTF_12000_random_strings_match_the_JDK_byte_for_byte()
    {
        var g = new SplitMix64(WriteUtfSeed);
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        int ok = 0;
        int err = 0;
        int maxWire = 0;

        for (int i = 0; i < Cases; i++)
        {
            string s = DifferentialGenerators.GenString(g);
            var dos = new JavaDataOutput();
            try
            {
                dos.WriteUTF(s);
                byte[] wire = dos.ToArray();
                AppendByte(hash, (byte)'O');
                AppendInt(hash, wire.Length);
                hash.AppendData(wire);
                ok++;
                maxWire = Math.Max(maxWire, wire.Length);
            }
            catch (UtfDataFormatException e)
            {
                AppendByte(hash, (byte)'E');
                AppendUnits(hash, e.Message);
                err++;
            }
        }

        _output.WriteLine($"WRITE_UTF cases={Cases} ok={ok} err={err} maxWire={maxWire}");

        // the counts of successes/failures and the maximum length must also equal the Java version (guards against the C# generator drifting while the random SHA still matches: impossible, but easier to diagnose)
        Assert.Equal(11372, ok);
        Assert.Equal(628, err);
        Assert.Equal(65537, maxWire);
        Assert.Equal(JdkWriteUtfSha256, Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    [Fact]
    public void ReadUTF_12000_random_byte_blocks_give_the_same_result_and_the_same_cursor_position_as_the_JDK()
    {
        var g = new SplitMix64(ReadUtfSeed);
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        int ok = 0;
        int utfError = 0;
        int eof = 0;

        for (int i = 0; i < Cases; i++)
        {
            byte[] data;
            if (g.Below(4) == 0)
            {
                int n0 = g.Below(48);
                data = new byte[n0];
                for (int j = 0; j < n0; j++)
                {
                    data[j] = g.Below(2) == 0
                        ? (byte)DifferentialGenerators.InterestingBytes[g.Below(DifferentialGenerators.InterestingBytes.Length)]
                        : (byte)g.Below(256);
                }
            }
            else
            {
                var enc = new JavaDataOutput();
                enc.WriteUTF(DifferentialGenerators.GenSmallString(g));
                byte[] full = enc.ToArray();
                data = full.AsSpan(2).ToArray();
                int muts = g.Below(3) == 0 ? 1 + g.Below(2) : 0;
                for (int k = 0; k < muts; k++)
                {
                    if (data.Length > 0)
                    {
                        int at = g.Below(data.Length);
                        data[at] = g.Below(2) == 0
                            ? (byte)DifferentialGenerators.InterestingBytes[g.Below(DifferentialGenerators.InterestingBytes.Length)]
                            : (byte)g.Below(256);
                    }
                }
            }

            int n = data.Length;
            int mode = g.Below(8);
            int d;
            if (mode <= 5)
            {
                d = n;
            }
            else if (mode == 6)
            {
                d = n + 1 + g.Below(3);
            }
            else
            {
                d = g.Below(n + 1);
            }

            int extra = g.Below(4);
            var wire = new byte[2 + n + extra];
            wire[0] = (byte)(d >> 8);
            wire[1] = (byte)d;
            Array.Copy(data, 0, wire, 2, n);
            for (int j = 0; j < extra; j++)
            {
                wire[2 + n + j] = (byte)g.Below(256);
            }

            var input = new JavaDataInput(wire);
            try
            {
                string r = input.ReadUTF();
                AppendByte(hash, (byte)'S');
                AppendUnits(hash, r);
                AppendInt(hash, input.Available);
                ok++;
            }
            catch (UtfDataFormatException e)
            {
                AppendByte(hash, (byte)'U');
                AppendUnits(hash, e.Message);
                AppendInt(hash, input.Available);
                utfError++;
            }
            catch (EndOfStreamException)
            {
                AppendByte(hash, (byte)'E');
                AppendInt(hash, input.Available);
                eof++;
            }
        }

        _output.WriteLine($"READ_UTF cases={Cases} ok={ok} utfError={utfError} eof={eof}");
        Assert.Equal(5669, ok);
        Assert.Equal(5615, utfError);
        Assert.Equal(716, eof);
        Assert.Equal(JdkReadUtfSha256, Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    [Fact]
    public void Message_12000_random_number_packets_match_the_real_JDK_Message_class()
    {
        var g = new SplitMix64(NumbersSeed);
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        long totalOps = 0;
        long totalBytes = 0;

        for (int i = 0; i < Cases; i++)
        {
            int cmd = g.Below(1024) - 512;
            var m = new Message(cmd);
            AppendByte(hash, unchecked((byte)m.Command));
            int ops = 1 + g.Below(6);
            for (int o = 0; o < ops; o++)
            {
                int kind = g.Below(16);
                switch (kind)
                {
                    case 0: m.WriteByte(DifferentialGenerators.InterestingInt(g)); break;
                    case 1: m.WriteShort(DifferentialGenerators.InterestingInt(g)); break;
                    case 2: m.WriteChar(DifferentialGenerators.InterestingInt(g)); break;
                    case 3: m.WriteInt(DifferentialGenerators.InterestingInt(g)); break;
                    case 4: m.WriteLong(DifferentialGenerators.InterestingLong(g)); break;
                    case 5: m.WriteBoolean(g.Below(2) == 1); break;
                    case 6: m.WriteFloat(BitConverter.Int32BitsToSingle(DifferentialGenerators.InterestingInt(g))); break;
                    case 7: m.WriteDouble(BitConverter.Int64BitsToDouble(DifferentialGenerators.InterestingLong(g))); break;
                    case 8:
                    {
                        long v = DifferentialGenerators.InterestingLong(g);
                        m.WriteCris(v, g.Below(2) == 1);
                        break;
                    }

                    case 9:
                    {
                        long v = DifferentialGenerators.InterestingLong(g);
                        m.WriteShortToLong(v, g.Below(2) == 1);
                        break;
                    }

                    case 10:
                    {
                        long v = DifferentialGenerators.InterestingLong(g);
                        m.WriteByteToLong(v, g.Below(2) == 1);
                        break;
                    }

                    case 11: m.WriteBytes(DifferentialGenerators.ShortString(g)); break;
                    case 12: m.WriteChars(DifferentialGenerators.ShortString(g)); break;
                    case 13: m.Write(DifferentialGenerators.InterestingInt(g)); break;
                    case 14:
                    {
                        int len = g.Below(9);
                        var b = new byte[len];
                        for (int j = 0; j < len; j++)
                        {
                            b[j] = (byte)g.Below(256);
                        }

                        int off = g.Below(len + 1);
                        int cnt = g.Below(len - off + 1);
                        m.Write(b, off, cnt);
                        break;
                    }

                    default: m.WriteUTF(DifferentialGenerators.ShortString(g)); break;
                }

                totalOps++;
            }

            byte[] data = m.GetData();
            AppendInt(hash, data.Length);
            hash.AppendData(data);
            totalBytes += data.Length;
        }

        _output.WriteLine($"NUMBERS tuples={Cases} ops={totalOps} bytes={totalBytes}");
        Assert.Equal(41863L, totalOps);
        Assert.Equal(192199L, totalBytes);
        Assert.Equal(JdkNumbersSha256, Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    private static void AppendByte(IncrementalHash hash, byte value)
    {
        Span<byte> tmp = stackalloc byte[1];
        tmp[0] = value;
        hash.AppendData(tmp);
    }

    private static void AppendInt(IncrementalHash hash, int value)
    {
        Span<byte> tmp = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(tmp, value);
        hash.AppendData(tmp);
    }

    /// <summary>The unit count followed by each UTF-16 unit big-endian (avoids UTF-8 conversion, which differs between Java and .NET for lone surrogates).</summary>
    private static void AppendUnits(IncrementalHash hash, string s)
    {
        AppendInt(hash, s.Length);
        Span<byte> tmp = stackalloc byte[2];
        foreach (char c in s)
        {
            BinaryPrimitives.WriteUInt16BigEndian(tmp, c);
            hash.AppendData(tmp);
        }
    }
}
