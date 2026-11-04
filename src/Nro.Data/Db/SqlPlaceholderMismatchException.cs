namespace Nro.Data.Db;

// Ngoại lệ báo số dấu ? tham số thực trong SQL khác số đối số được truyền; thông điệp chỉ chứa hai số, không chứa SQL hay giá trị tham số.
// Nguồn Java: jbcd/ConnectDB.java (C# tách lỗi riêng để phát hiện lệch tham số khi đổi JDBC sang Npgsql).
public sealed class SqlPlaceholderMismatchException : ArgumentException
{
    // Tạo ngoại lệ từ số chỗ giữ chỗ và số đối số thực tế.
    public SqlPlaceholderMismatchException(int placeholderCount, int argumentCount)
        : base($"The number of '?' placeholders in the SQL statement ({placeholderCount}) does not match the number of arguments passed ({argumentCount}).")
    {
        PlaceholderCount = placeholderCount;
        ArgumentCount = argumentCount;
    }

    // Số dấu ? là tham số thực trong SQL.
    public int PlaceholderCount { get; }

    // Số đối số đã truyền.
    public int ArgumentCount { get; }
}
