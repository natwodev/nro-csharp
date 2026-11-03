using System.Text;

namespace Nro.Network.Framing;

/// <summary>
/// The per-connection cipher state: the session key, the "key sent" flag and the two independent key pointers.
/// Port of the state spread over <c>Session</c> (<c>KEYS</c>, <c>sentKey</c>) and <c>MessageSendCollect</c> (<c>curR</c>, <c>curW</c>).
/// <para>
/// The cipher is a repeating-key XOR stream: every byte that goes through <see cref="WriteKey"/> or <see cref="ReadKey"/> is XORed
/// with the next key byte, and that direction's pointer advances, wrapping at the key length. The read and write directions have
/// SEPARATE pointers, so the same key is used from position 0 in each direction.
/// </para>
/// <para>
/// Threading: like the Java original, one reader may use the read side while one writer uses the write side at the same time
/// (each side touches only its own pointer). Two concurrent writers, or two concurrent readers, are not supported.
/// </para>
/// </summary>
public sealed class WireCipher
{
    /// <summary>The key text hard-coded in <c>Session.java:59</c>; the key every connection starts with.</summary>
    public const string DefaultKeyText = "NguyenDucVuEntertainment";

    private byte[] _key;
    private int _readPosition;
    private int _writePosition;
    private volatile bool _keySent;

    public WireCipher(ReadOnlySpan<byte> key)
    {
        _key = CheckedCopy(key);
    }

    /// <summary>A cipher with the protocol's default session key (<see cref="DefaultKeyText"/>).</summary>
    public static WireCipher CreateDefault() => new(Encoding.ASCII.GetBytes(DefaultKeyText));

    /// <summary>The current key bytes (a view, do not modify).</summary>
    public ReadOnlySpan<byte> Key => _key;

    /// <summary>
    /// Java <c>session.sentKey()</c>. While false, ordinary packets travel in clear text. It becomes true right after the key packet
    /// (<c>-27</c>) is produced, and from then on every packet is encrypted.
    /// </summary>
    public bool KeySent
    {
        get => _keySent;
        set => _keySent = value;
    }

    /// <summary>The next key index the reader will use.</summary>
    public int ReadPosition => _readPosition;

    /// <summary>The next key index the writer will use.</summary>
    public int WritePosition => _writePosition;

    /// <summary>
    /// Replaces the key and restarts both pointers at 0. Used by a client that has just received the key packet
    /// (in Java <c>KeyHandler.setKey</c>; the pointers were still 0 at that moment).
    /// </summary>
    public void SetKey(ReadOnlySpan<byte> key)
    {
        _key = CheckedCopy(key);
        _readPosition = 0;
        _writePosition = 0;
    }

    /// <summary>Test hook: places both pointers (the golden vectors start entries at arbitrary positions).</summary>
    internal void SetPositions(int readPosition, int writePosition)
    {
        if ((uint)readPosition >= (uint)_key.Length || (uint)writePosition >= (uint)_key.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(readPosition), "A pointer must be inside the key.");
        }

        _readPosition = readPosition;
        _writePosition = writePosition;
    }

    /// <summary>Java <c>readKey</c>: XORs <paramref name="b"/> with the next read-side key byte and advances the read pointer.</summary>
    public byte ReadKey(byte b)
    {
        byte result = (byte)(_key[_readPosition++] ^ b);
        if (_readPosition >= _key.Length)
        {
            _readPosition %= _key.Length;
        }

        return result;
    }

    /// <summary>Java <c>writeKey</c>: XORs <paramref name="b"/> with the next write-side key byte and advances the write pointer.</summary>
    public byte WriteKey(byte b)
    {
        byte result = (byte)(_key[_writePosition++] ^ b);
        if (_writePosition >= _key.Length)
        {
            _writePosition %= _key.Length;
        }

        return result;
    }

    private static byte[] CheckedCopy(ReadOnlySpan<byte> key)
    {
        if (key.IsEmpty)
        {
            throw new ArgumentException("The key must contain at least one byte.", nameof(key));
        }

        return key.ToArray();
    }
}
