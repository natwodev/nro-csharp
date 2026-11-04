namespace Nro.Core.IO;

// Kiểm tra tham số dùng chung cho JavaDataOutput và JavaDataInput
// Nguồn Java: các bước kiểm tra mà JDK làm trước mỗi lệnh write/read(byte[], off, len)
internal static class JavaIoChecks
{
    // Báo lỗi khi đoạn [fromIndex, fromIndex + size) vượt ra ngoài mảng dài length, kiểm tra TRƯỚC khi đụng tới dữ liệu
    // (Java: java.util.Objects.checkFromIndexSize, ném IndexOutOfBoundsException; .NET ném ArgumentOutOfRangeException)
    internal static void CheckFromIndexSize(int fromIndex, int size, int length)
    {
        if ((length | fromIndex | size) < 0 || size > length - fromIndex)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fromIndex),
                $"Range [{fromIndex}, {fromIndex} + {size}) out of bounds for length {length}");
        }
    }
}
