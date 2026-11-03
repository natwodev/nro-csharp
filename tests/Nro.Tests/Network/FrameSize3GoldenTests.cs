using System.Text.Json;
using Nro.Network.Framing;
using Nro.Tests.IO;

namespace Nro.Tests.Network;

/// <summary>
/// The eight large commands (3-byte size, each byte XORed with the key and then with 0x80) checked against <c>tests/golden/frame-size3.json</c>,
/// together with the way a server-side reader treats a 3-byte frame.
/// </summary>
public class FrameSize3GoldenTests
{
    private static readonly Lazy<JsonDocument> Doc = new(() => GoldenFile.Open("frame-size3.json"));

    private static string KeyHex(string name) => Doc.Value.RootElement.GetProperty("keys").GetProperty(name).GetString()!;

    [Fact]
    public void Large_command_set_matches_the_golden_list()
    {
        var golden = Doc.Value.RootElement.GetProperty("specialCmds").EnumerateArray().Select(e => (sbyte)e.GetInt32()).ToHashSet();
        Assert.Equal(8, golden.Count);
        Assert.True(golden.SetEquals(WireCommands.LargeSizeCommands));
    }

    [Fact]
    public void Independent_cases_encode_byte_for_byte()
    {
        int cases = 0;
        foreach (JsonElement c in Doc.Value.RootElement.GetProperty("cases").EnumerateArray())
        {
            string id = c.GetProperty("id").GetString()!;
            WireCipher cipher = NetworkGolden.CipherFromHex(KeyHex(c.GetProperty("key").GetString()!), c.GetProperty("sentKey").GetBoolean());
            cipher.SetPositions(0, c.GetProperty("startCurW").GetInt32());

            byte[] wire = FrameWriter.Encode(cipher, (sbyte)c.GetProperty("cmd").GetInt32(), NetworkGolden.PayloadOf(c));
            NetworkGolden.AssertWire(c, wire, id);
            Assert.True(c.GetProperty("curWAfter").GetInt32() == cipher.WritePosition, $"{id}: write pointer differs");
            cases++;
        }

        Assert.Equal(249, cases);
    }

    [Fact]
    public void Chains_keep_the_pointer_running_across_ordinary_and_large_commands()
    {
        JsonElement root = Doc.Value.RootElement;
        int steps = 0;

        steps += RunChain("chain", root.GetProperty("chain").GetProperty("keyHex").GetString()!, root.GetProperty("chain").GetProperty("steps"));
        foreach (JsonElement extra in root.GetProperty("extraChains").EnumerateArray())
        {
            steps += RunChain(extra.GetProperty("id").GetString()!, extra.GetProperty("keyHex").GetString()!, extra.GetProperty("steps"));
        }

        Assert.True(steps > 8);
    }

    private static int RunChain(string id, string keyHex, JsonElement steps)
    {
        WireCipher cipher = NetworkGolden.CipherFromHex(keyHex, keySent: false);
        int count = 0;
        foreach (JsonElement step in steps.EnumerateArray())
        {
            string where = $"{id} step {step.GetProperty("step").GetInt32()}";
            cipher.KeySent = step.GetProperty("sentKey").GetBoolean();

            sbyte cmd = (sbyte)step.GetProperty("cmd").GetInt32();
            byte[] payload = NetworkGolden.PayloadOf(step);
            Assert.Equal(WireCommands.IsLargeSize(cmd), step.GetProperty("extended").GetBoolean());

            if (!WireCommands.IsLargeSize(cmd) && payload.Length > WireCommands.MaxTwoByteSize)
            {
                // Java silently truncates the 16-bit size and writes a corrupt frame; this port refuses. To keep checking the steps that follow,
                // move the write pointer to where the golden says Java left it.
                Assert.Throws<FrameTooLargeException>(() => FrameWriter.Encode(cipher, cmd, payload));
                cipher.SetPositions(cipher.ReadPosition, step.GetProperty("curW").GetInt32());
                count++;
                continue;
            }

            byte[] wire = FrameWriter.Encode(cipher, cmd, payload);
            NetworkGolden.AssertWire(step, wire, where);
            if (step.TryGetProperty("curW", out JsonElement curW))
            {
                Assert.True(curW.GetInt32() == cipher.WritePosition, $"{where}: write pointer differs");
            }

            count++;
        }

        return count;
    }

    [Fact]
    public void Server_reader_reads_the_two_byte_frames_clients_send_even_for_commands_on_the_large_list()
    {
        int cases = 0;
        foreach (JsonElement r in Doc.Value.RootElement.GetProperty("readCases").EnumerateArray())
        {
            if (r.GetProperty("form").GetString() != "2byte")
            {
                continue;
            }

            string id = r.GetProperty("id").GetString()!;
            WireCipher cipher = NetworkGolden.CipherFromHex(KeyHex(r.GetProperty("key").GetString()!), r.GetProperty("sentKey").GetBoolean());
            cipher.SetPositions(r.GetProperty("startCurR").GetInt32(), 0);

            (Frame frame, int consumed) = NetworkGolden.ReadOne(FrameReader.ForServer(cipher), GoldenFile.Hex(r.GetProperty("wireHex").GetString()!));
            JsonElement java = r.GetProperty("javaResult");
            Assert.True(java.GetProperty("cmd").GetInt32() == frame.Command, $"{id}: command differs");
            Assert.True(java.GetProperty("payloadLength").GetInt32() == frame.Payload.Length, $"{id}: payload length differs");
            Assert.True(java.GetProperty("payloadSha256").GetString() == GoldenFile.Sha256Hex(frame.Payload), $"{id}: payload differs");
            Assert.True(java.GetProperty("consumedBytes").GetInt32() == consumed, $"{id}: consumed bytes differ");
            Assert.True(java.GetProperty("curRAfter").GetInt32() == cipher.ReadPosition, $"{id}: read pointer differs");
            cases++;
        }

        Assert.True(cases > 0);
    }

    [Fact]
    public async Task Three_byte_frames_decode_with_the_client_reader_and_make_the_server_reader_fail_cleanly()
    {
        int cases = 0;
        foreach (JsonElement r in Doc.Value.RootElement.GetProperty("readCases").EnumerateArray())
        {
            if (r.GetProperty("form").GetString() != "3byte-server")
            {
                continue;
            }

            string id = r.GetProperty("id").GetString()!;
            byte[] wire = GoldenFile.Hex(r.GetProperty("wireHex").GetString()!);
            string keyHex = KeyHex(r.GetProperty("key").GetString()!);
            bool keySent = r.GetProperty("sentKey").GetBoolean();
            int startCurR = r.GetProperty("startCurR").GetInt32();

            WireCipher clientCipher = NetworkGolden.CipherFromHex(keyHex, keySent);
            clientCipher.SetPositions(startCurR, 0);
            (Frame frame, int consumed) = NetworkGolden.ReadOne(FrameReader.ForClient(clientCipher), wire);
            Assert.True(r.GetProperty("cmd").GetInt32() == frame.Command, $"{id}: command differs");
            Assert.True(NetworkGolden.PayloadOf(r).AsSpan().SequenceEqual(frame.Payload), $"{id}: payload differs");
            Assert.True(consumed == wire.Length, $"{id}: not all bytes consumed");

            // Java's server-side readMessage misreads such a frame (it reads a 2-byte size, gets garbage and silently returns a zero-filled message).
            // This port treats the truncated stream as an error instead.
            WireCipher serverCipher = NetworkGolden.CipherFromHex(keyHex, keySent);
            serverCipher.SetPositions(startCurR, 0);
            FrameReader serverReader = FrameReader.ForServer(serverCipher);
            await Assert.ThrowsAsync<EndOfStreamException>(async () => await serverReader.ReadAsync(NetworkGolden.Stream(wire)));
            cases++;
        }

        Assert.True(cases > 0);
    }

    [Fact]
    public void Large_command_before_the_key_is_sent_still_scrambles_the_three_size_bytes()
    {
        int quirks = 0;
        foreach (JsonElement q in Doc.Value.RootElement.GetProperty("quirks").EnumerateArray())
        {
            string id = q.GetProperty("id").GetString()!;
            if (id.StartsWith("extended-cmd-before-sentKey", StringComparison.Ordinal))
            {
                WireCipher cipher = WireCipher.CreateDefault();
                Assert.False(q.GetProperty("sentKey").GetBoolean());
                byte[] wire = FrameWriter.Encode(cipher, (sbyte)q.GetProperty("cmd").GetInt32(), NetworkGolden.PayloadOf(q));
                Assert.True(q.GetProperty("wireHex").GetString() == GoldenFile.ToHex(wire), $"{id}: wire differs");
                Assert.True(q.GetProperty("curWAfter").GetInt32() == cipher.WritePosition, $"{id}: write pointer differs");
                quirks++;
            }
            else if (id == "size-overflow-16777216")
            {
                WireCipher cipher = WireCipher.CreateDefault();
                cipher.KeySent = true;
                Assert.Throws<FrameTooLargeException>(() => FrameWriter.Encode(cipher, -28, new byte[1 << 24]));
                Assert.Equal(0, cipher.WritePosition);
                quirks++;
            }
        }

        Assert.Equal(3, quirks);
    }
}
