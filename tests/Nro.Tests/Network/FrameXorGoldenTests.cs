using System.Text.Json;
using Nro.Network.Framing;
using Nro.Tests.IO;

namespace Nro.Tests.Network;

/// <summary>
/// Frame encoding and decoding checked against <c>tests/golden/frame-xor.json</c>: the byte-exact output of the real
/// <c>MessageSendCollect</c> (Java) for ordinary commands, in clear text and encrypted, with the key pointers tracked step by step.
/// </summary>
public class FrameXorGoldenTests
{
    private static readonly Lazy<JsonDocument> Doc = new(() => GoldenFile.Open("frame-xor.json"));

    [Fact]
    public void Golden_file_has_the_expected_shape()
    {
        JsonElement root = Doc.Value.RootElement;
        Assert.Equal(4, root.GetProperty("sequences").GetArrayLength());
        Assert.Equal(3, root.GetProperty("cmdSweep").GetProperty("sweeps").GetArrayLength());
        Assert.Equal(90, root.GetProperty("oversize").GetProperty("cases").GetArrayLength());
    }

    [Fact]
    public void Sequences_encode_and_decode_byte_for_byte_with_the_expected_pointers()
    {
        int steps = 0;
        foreach (JsonElement sequence in Doc.Value.RootElement.GetProperty("sequences").EnumerateArray())
        {
            string id = sequence.GetProperty("id").GetString()!;
            WireCipher cipher = NetworkGolden.CipherFromHex(sequence.GetProperty("keyHex").GetString()!, keySent: false);
            FrameReader reader = FrameReader.ForServer(cipher);

            foreach (JsonElement step in sequence.GetProperty("steps").EnumerateArray())
            {
                string where = $"{id} step {step.GetProperty("step").GetInt32()}";
                string op = step.GetProperty("op").GetString()!;

                if (op == "setSentKey")
                {
                    cipher.KeySent = step.GetProperty("value").GetBoolean();
                }
                else if (op == "send")
                {
                    Assert.True(step.GetProperty("sentKey").GetBoolean() == cipher.KeySent, $"{where}: key-sent flag out of sync");
                    byte[] wire = FrameWriter.Encode(cipher, (sbyte)step.GetProperty("cmd").GetInt32(), NetworkGolden.PayloadOf(step));
                    NetworkGolden.AssertWire(step, wire, where);
                }
                else
                {
                    Assert.Equal("recv", op);
                    Assert.True(step.GetProperty("sentKey").GetBoolean() == cipher.KeySent, $"{where}: key-sent flag out of sync");
                    sbyte cmd = (sbyte)step.GetProperty("cmd").GetInt32();
                    byte[] payload = NetworkGolden.PayloadOf(step);

                    byte[] wire;
                    if (step.TryGetProperty("wireHex", out JsonElement hex))
                    {
                        wire = GoldenFile.Hex(hex.GetString()!);
                    }
                    else
                    {
                        // Only the head and the hash of a long wire are stored: rebuild it as a client would write it, then check it against the vector.
                        wire = FrameWriter.Encode(NetworkGolden.ClientWriterFor(cipher), cmd, payload);
                        NetworkGolden.AssertWire(step, wire, where + " (rebuilt)");
                    }

                    (Frame frame, int consumed) = NetworkGolden.ReadOne(reader, wire);
                    Assert.True(cmd == frame.Command, $"{where}: command differs");
                    Assert.True(payload.AsSpan().SequenceEqual(frame.Payload), $"{where}: payload differs");
                    Assert.True(step.GetProperty("consumedBytes").GetInt32() == consumed, $"{where}: consumed bytes differ");
                }

                Assert.True(step.GetProperty("curR").GetInt32() == cipher.ReadPosition, $"{where}: read pointer differs");
                Assert.True(step.GetProperty("curW").GetInt32() == cipher.WritePosition, $"{where}: write pointer differs");
                steps++;
            }
        }

        Assert.Equal(35 + 35 + 97 + 97, steps);
    }

    [Fact]
    public void Command_sweep_covers_all_256_commands_in_three_configurations()
    {
        int entries = 0;
        foreach (JsonElement sweep in Doc.Value.RootElement.GetProperty("cmdSweep").GetProperty("sweeps").EnumerateArray())
        {
            string sweepId = sweep.GetProperty("id").GetString()!;
            string keyHex = sweep.GetProperty("keyHex").GetString()!;
            int keyLength = sweep.GetProperty("keyLength").GetInt32();
            bool keySent = sweep.GetProperty("sentKey").GetBoolean();

            foreach (JsonElement entry in sweep.GetProperty("entries").EnumerateArray())
            {
                sbyte cmd = (sbyte)entry.GetProperty("cmd").GetInt32();
                int idx = cmd + 128;
                int startPtr = idx % keyLength;
                Assert.Equal(startPtr, entry.GetProperty("startPtr").GetInt32());
                byte[] payload = NetworkGolden.Payload(idx % 9, (idx * 3 + 1) & 0xFF);
                string where = $"{sweepId} cmd {cmd}";

                WireCipher writer = NetworkGolden.CipherFromHex(keyHex, keySent);
                writer.SetPositions(0, startPtr);
                byte[] wire = FrameWriter.Encode(writer, cmd, payload);
                Assert.True(entry.GetProperty("wireHex").GetString() == GoldenFile.ToHex(wire), $"{where}: wire differs");
                Assert.True(entry.GetProperty("curWAfter").GetInt32() == writer.WritePosition, $"{where}: write pointer differs");
                Assert.Equal(WireCommands.IsLargeSize(cmd), entry.GetProperty("extended").GetBoolean());

                if (!entry.GetProperty("extended").GetBoolean())
                {
                    WireCipher readerCipher = NetworkGolden.CipherFromHex(keyHex, keySent);
                    readerCipher.SetPositions(startPtr, 0);
                    (Frame frame, int consumed) = NetworkGolden.ReadOne(FrameReader.ForServer(readerCipher), wire);
                    Assert.True(cmd == frame.Command && payload.AsSpan().SequenceEqual(frame.Payload), $"{where}: decoded frame differs");
                    Assert.True(consumed == wire.Length, $"{where}: not all bytes consumed");
                    Assert.True(entry.GetProperty("curWAfter").GetInt32() == readerCipher.ReadPosition, $"{where}: read pointer differs");
                }

                entries++;
            }
        }

        Assert.Equal(3 * 256, entries);
    }

    [Fact]
    public void Payloads_over_65535_bytes_are_refused_instead_of_truncating_the_size_like_Java()
    {
        JsonElement root = Doc.Value.RootElement;
        string defaultKey = root.GetProperty("sequences")[0].GetProperty("keyHex").GetString()!;
        string shortKey = root.GetProperty("sequences")[1].GetProperty("keyHex").GetString()!;

        int cases = 0;
        foreach (JsonElement c in root.GetProperty("oversize").GetProperty("cases").EnumerateArray())
        {
            string key = c.GetProperty("key").GetString()! == "default" ? defaultKey : shortKey;
            WireCipher cipher = NetworkGolden.CipherFromHex(key, c.GetProperty("sentKey").GetBoolean());
            int startPtr = c.GetProperty("startPtr").GetInt32();
            cipher.SetPositions(startPtr, startPtr);

            int length = c.GetProperty("payloadLength").GetInt32();
            var ex = Assert.Throws<FrameTooLargeException>(
                () => FrameWriter.Encode(cipher, (sbyte)c.GetProperty("cmd").GetInt32(), new byte[length]));
            Assert.Equal(length, ex.Size);
            Assert.Equal(WireCommands.MaxTwoByteSize, ex.Maximum);
            Assert.True(cipher.WritePosition == startPtr && cipher.ReadPosition == startPtr, $"{c.GetProperty("id").GetString()}: a refused payload moved the pointers");
            cases++;
        }

        Assert.Equal(90, cases);
    }

    [Fact]
    public void The_two_size_overflow_quirks_throw_too()
    {
        foreach (JsonElement q in Doc.Value.RootElement.GetProperty("quirks").EnumerateArray())
        {
            WireCipher cipher = WireCipher.CreateDefault();
            cipher.KeySent = q.GetProperty("sentKey").GetBoolean();
            Assert.Throws<FrameTooLargeException>(
                () => FrameWriter.Encode(cipher, (sbyte)q.GetProperty("cmd").GetInt32(), new byte[q.GetProperty("payloadLength").GetInt32()]));
        }
    }
}
