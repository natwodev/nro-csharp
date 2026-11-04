using Nro.Network.Configurations;

namespace Nro.Network.Framing;

// Hằng số giao thức dùng chung cho mã hóa/giải mã khung và bắt tay.
// Nguồn Java: network/io/MessageSendCollect.java (lệnh -27 mở đầu kết nối để trao đổi khóa không mã hóa; đúng tám lệnh server gửi dùng trường kích thước 3 byte vì payload có thể vượt 64 KiB).

public static class WireCommands
{

    // Lệnh -27 mở đầu kết nối: client xin khóa, server trả khóa ở dạng không mã hóa.
    public const sbyte GetSessionId = Cmd_message.GET_SESSION_ID;

    // Đúng tám lệnh server gửi dùng trường kích thước 3 byte thay cho 2 byte.
    public static readonly IReadOnlySet<sbyte> LargeSizeCommands = new HashSet<sbyte> { -32, -66, -74, 11, -67, -87, 66, -28 };

    // Payload lớn nhất trường kích thước 2 byte biểu diễn được.
    public const int MaxTwoByteSize = 0xFFFF;

    // Payload lớn nhất trường kích thước 3 byte biểu diễn được.
    public const int MaxThreeByteSize = 0xFFFFFF;

    // Cho biết lệnh server gửi có dùng trường kích thước 3 byte hay không.
    public static bool IsLargeSize(sbyte command) => LargeSizeCommands.Contains(command);
}
