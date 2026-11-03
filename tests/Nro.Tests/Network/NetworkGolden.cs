using System.Text.Json;
using Nro.Network.Framing;
using Nro.Tests.IO;

namespace Nro.Tests.Network;

/// <summary>Helpers shared by the frame and handshake golden tests (vectors in tests/golden, generated from the real Java code).</summary>
internal static class NetworkGolden
{
    /// <summary>The vectors' payload formula: <c>payload[i] = (byte)((i * 131 + (i >> 8) * 17 + seed) &amp; 0xFF)</c> in 64-bit arithmetic.</summary>
    public static byte[] Payload(int length, int seed)
    {
        byte[] data = new byte[length];
        for (int i = 0; i < length; i++)
        {
            long v = (long)i * 131 + ((long)i >> 8) * 17 + seed;
            data[i] = (byte)(v & 0xFF);
        }

        return data;
    }

    /// <summary>The payload of a vector entry: the inline <c>payloadHex</c> when present, otherwise generated from <c>payloadLength</c> and <c>payloadSeed</c>.</summary>
    public static byte[] PayloadOf(JsonElement entry)
    {
        if (entry.TryGetProperty("payloadHex", out JsonElement hex))
        {
            return GoldenFile.Hex(hex.GetString()!);
        }

        return Payload(entry.GetProperty("payloadLength").GetInt32(), entry.GetProperty("payloadSeed").GetInt32());
    }

    /// <summary>Checks a wire image against the entry: the full <c>wireHex</c>, or length + first 16 bytes + SHA-256 for long frames.</summary>
    public static void AssertWire(JsonElement entry, byte[] actual, string context)
    {
        if (entry.TryGetProperty("wireHex", out JsonElement hex))
        {
            Assert.True(hex.GetString() == GoldenFile.ToHex(actual), $"{context}: wire differs");
            return;
        }

        Assert.True(entry.GetProperty("wireLength").GetInt32() == actual.Length, $"{context}: wire length differs");
        Assert.True(entry.GetProperty("wireHead16Hex").GetString() == GoldenFile.ToHex(actual.AsSpan(0, 16)), $"{context}: wire head differs");
        Assert.True(entry.GetProperty("wireSha256").GetString() == GoldenFile.Sha256Hex(actual), $"{context}: wire hash differs");
    }

    public static WireCipher CipherFromHex(string keyHex, bool keySent)
    {
        return new WireCipher(GoldenFile.Hex(keyHex)) { KeySent = keySent };
    }

    /// <summary>A client-role cipher whose write pointer equals the server's read pointer: it produces what the server's reader expects.</summary>
    public static WireCipher ClientWriterFor(WireCipher serverCipher)
    {
        var client = new WireCipher(serverCipher.Key) { KeySent = serverCipher.KeySent };
        client.SetPositions(0, serverCipher.ReadPosition);
        return client;
    }

    public static MemoryStream Stream(byte[] bytes) => new(bytes, writable: false);

    /// <summary>Reads exactly one frame from <paramref name="wire"/> and returns it with the number of bytes consumed.</summary>
    public static (Frame Frame, int Consumed) ReadOne(FrameReader reader, byte[] wire)
    {
        using MemoryStream stream = Stream(wire);
        Frame? frame = reader.ReadAsync(stream).AsTask().GetAwaiter().GetResult();
        Assert.NotNull(frame);
        return (frame.Value, (int)stream.Position);
    }
}
