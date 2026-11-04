using System.Text.Json;
using Nro.Network.Framing;
using Nro.Tests.Unit.Core.IO;

namespace Nro.Tests.Unit.Network.Framing;

/// <summary>The hello and the key packet (<c>-27</c>) checked against <c>tests/golden/handshake-key.json</c>.</summary>
public class HandshakeGoldenTests
{
    private static readonly Lazy<JsonDocument> Doc = new(() => GoldenFile.Open("handshake-key.json"));

    [Fact]
    public void Client_hello_is_read_as_command_minus_27_with_no_payload()
    {
        JsonElement hello = Doc.Value.RootElement.GetProperty("clientHello");
        WireCipher cipher = WireCipher.CreateDefault();

        (Frame frame, int consumed) = NetworkGolden.ReadOne(FrameReader.ForServer(cipher), GoldenFile.Hex(hello.GetProperty("wireHex").GetString()!));

        Assert.Equal(WireCommands.GetSessionId, frame.Command);
        Assert.Equal(hello.GetProperty("cmd").GetInt32(), frame.Command);
        Assert.Empty(frame.Payload);
        Assert.Equal(hello.GetProperty("consumedBytes").GetInt32(), consumed);
        Assert.Equal(hello.GetProperty("curR").GetInt32(), cipher.ReadPosition);
        Assert.False(cipher.KeySent);
    }

    [Fact]
    public void Key_packet_and_the_frames_after_it_match_the_golden_for_every_key()
    {
        int cases = 0;
        foreach (JsonElement c in Doc.Value.RootElement.GetProperty("cases").EnumerateArray())
        {
            string id = c.GetProperty("id").GetString()!;
            byte[] key = GoldenFile.Hex(c.GetProperty("keyHex").GetString()!);
            JsonElement packet = c.GetProperty("keyPacket");

            Assert.True(packet.GetProperty("payloadHex").GetString() == GoldenFile.ToHex(KeyPacket.BuildPayload(key)), $"{id}: key packet payload differs");

            var server = new WireCipher(key);
            Assert.Equal(packet.GetProperty("sentKeyBefore").GetBoolean(), server.KeySent);
            byte[] wire = KeyPacket.CreateServerFrame(server);
            Assert.True(packet.GetProperty("wireHex").GetString() == GoldenFile.ToHex(wire), $"{id}: key packet wire differs");
            Assert.Equal(packet.GetProperty("sentKeyAfter").GetBoolean(), server.KeySent);
            Assert.Equal(packet.GetProperty("serverCurWAfter").GetInt32(), server.WritePosition);

            // The client side: read the key packet (it is in clear text), derive the key, switch the cipher on.
            var client = new WireCipher(WireCipher.DefaultKeyText.Select(ch => (byte)ch).ToArray());
            (Frame keyFrame, _) = NetworkGolden.ReadOne(FrameReader.ForClient(client), wire);
            Assert.Equal(WireCommands.GetSessionId, keyFrame.Command);
            byte[] derived = KeyPacket.ParsePayload(keyFrame.Payload);
            Assert.True(c.GetProperty("clientDerivedKeyHex").GetString() == GoldenFile.ToHex(derived), $"{id}: derived key differs");
            client.SetKey(derived);
            client.KeySent = c.GetProperty("clientSentKeyAfter").GetBoolean();

            FrameReader clientReader = FrameReader.ForClient(client);
            foreach (JsonElement f in c.GetProperty("framesAfterHandshake").EnumerateArray())
            {
                string where = $"{id} frame {f.GetProperty("index").GetInt32()}";
                sbyte cmd = (sbyte)f.GetProperty("cmd").GetInt32();
                byte[] payload = GoldenFile.Hex(f.GetProperty("payloadHex").GetString()!);

                byte[] frameWire = FrameWriter.Encode(server, cmd, payload);
                Assert.True(f.GetProperty("wireHex").GetString() == GoldenFile.ToHex(frameWire), $"{where}: wire differs");
                Assert.True(f.GetProperty("serverCurWAfter").GetInt32() == server.WritePosition, $"{where}: server pointer differs");

                (Frame read, _) = NetworkGolden.ReadOne(clientReader, frameWire);
                Assert.True(cmd == read.Command && payload.AsSpan().SequenceEqual(read.Payload), $"{where}: the client read something else");
                Assert.True(f.GetProperty("clientCurRAfter").GetInt32() == client.ReadPosition, $"{where}: client pointer differs");
            }

            cases++;
        }

        Assert.Equal(3, cases);
    }
}
