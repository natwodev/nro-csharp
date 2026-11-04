namespace Nro.Data.Db;

// Cung cấp hợp đồng thực thi SQL kiểu ConnectDB với tham số vị trí và kết quả chụp trong bộ nhớ.
// Nguồn Java: jbcd/ConnectDB.java (NroDatabase mượn kết nối riêng từ pool cho từng lệnh; NroTransaction dùng chung kết nối và giao dịch).
public interface INroSqlExecutor
{
    // Chạy truy vấn và chụp toàn bộ kết quả; lệnh không trả tập kết quả ném InvalidOperationException (Java: executeQuery).
    Task<IResultSnapshot> ExecuteQueryAsync(string sql, params object?[] args);

    // Chạy truy vấn với tham số và mã hủy (Java: executeQuery).
    Task<IResultSnapshot> ExecuteQueryAsync(string sql, object?[] args, CancellationToken cancellationToken);

    // Chạy truy vấn không tham số với mã hủy (Java: executeQuery).
    Task<IResultSnapshot> ExecuteQueryAsync(string sql, CancellationToken cancellationToken);

    // Chạy INSERT, UPDATE, DELETE hoặc DDL và trả số dòng tác động; DDL trả 0 như JDBC, giữ quy tắc insert ... () (Java: executeUpdate).
    Task<int> ExecuteUpdateAsync(string sql, params object?[] args);

    // Chạy lệnh cập nhật với tham số và mã hủy (Java: executeUpdate).
    Task<int> ExecuteUpdateAsync(string sql, object?[] args, CancellationToken cancellationToken);

    // Chạy lệnh cập nhật không tham số với mã hủy (Java: executeUpdate).
    Task<int> ExecuteUpdateAsync(string sql, CancellationToken cancellationToken);
}
