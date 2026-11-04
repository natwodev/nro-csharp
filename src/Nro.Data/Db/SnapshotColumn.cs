using Nro.Data.Db;
namespace Nro.Data.Db;

// Lưu siêu dữ liệu một cột kết quả gồm tên cột và tên bảng nguồn.
// Nguồn Java: jbcd/ConnectResultSet.java (cột biểu thức không có tên bảng; tên cột được dùng để lập khóa chữ thường).
// Tạo siêu dữ liệu cột theo tên trình điều khiển trả về và bảng nguồn nếu có.
public readonly record struct SnapshotColumn(string Name, string? Table = null);
