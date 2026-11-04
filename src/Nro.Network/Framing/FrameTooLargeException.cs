namespace Nro.Network.Framing;

// Ngoại lệ khi payload vượt sức chứa trường kích thước của khung.
// Nguồn Java: network/io/MessageSendCollect.java (C# từ chối payload quá 65535 byte hoặc quá 16777215 byte cho lệnh lớn thay vì Java cắt kích thước và phát khung hỏng).

public sealed class FrameTooLargeException(sbyte command, int size, int maximum)
    : InvalidOperationException($"Payload of {size} bytes is too large for command {command}: the size field holds at most {maximum}.")
{

    // Mã lệnh có payload vượt giới hạn.
    public sbyte Command { get; } = command;

    // Kích thước payload bị từ chối.
    public int Size { get; } = size;

    // Sức chứa lớn nhất của trường kích thước.
    public int Maximum { get; } = maximum;
}
