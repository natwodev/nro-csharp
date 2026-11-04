using Nro.Core.IO;

namespace Nro.Network.Framing;

// Khung gói đã giải mã, gồm mã lệnh và payload.
// Nguồn Java: network/io/MessageSendCollect.java (lớp giá trị C# tách mới; payload không bị sao chép khi tạo Message).

public readonly record struct Frame(sbyte Command, byte[] Payload)
{

    // Tạo Message ở chế độ đọc trên payload hiện có, không sao chép mảng.
    public Message ToMessage() => new(Command, Payload);
}
