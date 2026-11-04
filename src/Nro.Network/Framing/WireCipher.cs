using System.Text;

namespace Nro.Network.Framing;

// Trạng thái XOR lặp theo từng kết nối, gồm khóa, cờ đã gửi khóa và hai vị trí khóa.
// Nguồn Java: network/session/Session.java và network/io/MessageSendCollect.java (mỗi byte XOR với byte khóa kế tiếp; vị trí đọc và ghi tách biệt, cùng bắt đầu từ 0; một luồng đọc và một luồng ghi có thể đồng thời, nhưng không hỗ trợ hai luồng cùng đọc hoặc cùng ghi).

public sealed class WireCipher
{

    // Văn bản khóa mặc định của giao thức Java, giữ nguyên trong mã nguồn.
    public const string DefaultKeyText = "NguyenDucVuEntertainment";

    private byte[] _key;
    private int _readPosition;
    private int _writePosition;
    private volatile bool _keySent;

    // Tạo trạng thái XOR từ bản sao khóa không rỗng.
    public WireCipher(ReadOnlySpan<byte> key)
    {
        _key = CheckedCopy(key);
    }

    // Tạo trạng thái XOR với khóa mặc định của giao thức.
    public static WireCipher CreateDefault() => new(Encoding.ASCII.GetBytes(DefaultKeyText));

    // Các byte khóa hiện tại dưới dạng view chỉ đọc.
    public ReadOnlySpan<byte> Key => _key;

    // Cho biết gói khóa -27 đã gửi; gói thường chỉ XOR sau thời điểm này.
    public bool KeySent
    {
        get => _keySent;
        set => _keySent = value;
    }

    // Chỉ số khóa kế tiếp dùng cho chiều đọc.
    public int ReadPosition => _readPosition;

    // Chỉ số khóa kế tiếp dùng cho chiều ghi.
    public int WritePosition => _writePosition;

    // Thay khóa và đặt lại cả hai vị trí về 0 sau khi client nhận gói khóa.
    public void SetKey(ReadOnlySpan<byte> key)
    {
        _key = CheckedCopy(key);
        _readPosition = 0;
        _writePosition = 0;
    }

    internal void SetPositions(int readPosition, int writePosition)
    {
        if ((uint)readPosition >= (uint)_key.Length || (uint)writePosition >= (uint)_key.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(readPosition), "A pointer must be inside the key.");
        }

        _readPosition = readPosition;
        _writePosition = writePosition;
    }

    // XOR byte với khóa đọc kế tiếp rồi làm tiến vị trí đọc (Java: readKey).
    public byte ReadKey(byte b)
    {
        byte result = (byte)(_key[_readPosition++] ^ b);
        if (_readPosition >= _key.Length)
        {
            _readPosition %= _key.Length;
        }

        return result;
    }

    // XOR byte với khóa ghi kế tiếp rồi làm tiến vị trí ghi (Java: writeKey).
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
