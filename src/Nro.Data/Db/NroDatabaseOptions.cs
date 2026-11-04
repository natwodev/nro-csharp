namespace Nro.Data.Db;

// Cấu hình kết nối cơ sở dữ liệu PostgreSQL cho NroDatabase.
// Nguồn Java: jbcd/ConnectDB.java (thay data/config/data_base.properties; dùng Npgsql và lấy chuỗi kết nối từ cấu hình, tuyệt đối không ghi chuỗi kết nối thật).
public sealed class NroDatabaseOptions
{
    // Chuỗi kết nối Npgsql, gồm cấu hình kích thước và thời hạn pool; không ghi vào log hoặc thông báo lỗi.
    public string ConnectionString { get; set; } = string.Empty;

    // Bật ghi SQL chạy thành công; lỗi luôn được ghi, chỉ ghi SQL không ghi giá trị tham số (Java: database.log / LOG_QUERY).
    public bool LogQuery { get; set; }

    // Thời hạn mỗi câu lệnh tính bằng giây; 0 không giới hạn.
    public int CommandTimeoutSeconds { get; set; } = 30;
}
