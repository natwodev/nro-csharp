namespace Nro.Data.Db;

// Cung cấp ảnh chụp kết quả truy vấn trong bộ nhớ, thay cho CrisResultSet của Java.
// Nguồn Java: jbcd/CrisResultSet.java (bảo toàn chỉ số cột từ 1, con trỏ -1, ép kiểu cứng, ngoại lệ và các quy tắc bất thường của ConnectResultSet; byte là sbyte, Timestamp là DateTime?).
public interface IResultSnapshot : IDisposable
{
    // Đọc cột theo chỉ số, yêu cầu int và cắt tràn về sbyte (Java: getByte).
    sbyte GetByte(int column);

    // Đọc cột theo tên, yêu cầu int và cắt tràn về sbyte (Java: getByte).
    sbyte GetByte(string column);

    // Đọc cột theo chỉ số, yêu cầu long và cắt tràn về int (Java: getInt).
    int GetInt(int column);

    // Đọc cột theo tên, yêu cầu đúng kiểu int (Java: getInt).
    int GetInt(string column);

    // Đọc cột theo chỉ số, yêu cầu int và cắt tràn về short (Java: getShort).
    short GetShort(int column);

    // Đọc cột theo tên, yêu cầu int và cắt tràn về short (Java: getShort).
    short GetShort(string column);

    // Đọc cột theo chỉ số, yêu cầu đúng kiểu float (Java: getFloat).
    float GetFloat(int column);

    // Đọc cột theo tên, yêu cầu đúng kiểu float (Java: getFloat).
    float GetFloat(string column);

    // Đọc cột theo chỉ số, yêu cầu đúng kiểu double (Java: getDouble).
    double GetDouble(int column);

    // Đọc cột theo tên, yêu cầu đúng kiểu double (Java: getDouble).
    double GetDouble(string column);

    // Đọc cột theo chỉ số, yêu cầu đúng kiểu long (Java: getLong).
    long GetLong(int column);

    // Đọc cột theo tên, yêu cầu đúng kiểu long (Java: getLong).
    long GetLong(string column);

    // Đọc chuỗi theo String.valueOf của Java; NULL thành "null" và số, bool, thời gian theo toString (Java: getString).
    string GetString(int column);

    // Đọc chuỗi theo String.valueOf của Java (Java: getString).
    string GetString(string column);

    // Đọc bool: int bằng 1 là true, còn lại phải là bool (Java: getBoolean).
    bool GetBoolean(int column);

    // Đọc bool: int bằng 1 là true, còn lại phải là bool (Java: getBoolean).
    bool GetBoolean(string column);

    // Đọc giá trị thô đã chụp, NULL trả null (Java: getObject).
    object? GetObject(int column);

    // Đọc giá trị thô đã chụp, NULL trả null (Java: getObject).
    object? GetObject(string column);

    // Đọc Timestamp theo DateTime; NULL trả null, sai kiểu ném InvalidCastException (Java: getTimestamp).
    DateTime? GetTimestamp(int column);

    // Đọc Timestamp theo DateTime; NULL trả null, sai kiểu ném InvalidCastException (Java: getTimestamp).
    DateTime? GetTimestamp(string column);

    // Tăng chỉ số dòng và trả true khi còn trong kết quả, không chặn ở cuối (Java: next).
    bool Next();

    // Tăng chỉ số dòng và chỉ trả true khi bằng dòng cuối, không nhảy về dòng đầu (Java: first).
    bool First();

    // Đặt chỉ số dòng nhưng chỉ kiểm tra chỉ số hiện tại, luôn trả true (Java: gotoResult).
    bool GotoResult(int index);

    // Chuyển đến dòng đầu; kết quả rỗng ném "No data available", luôn trả true (Java: gotoFirst).
    bool GotoFirst();

    // Chuyển về trước dòng đầu với chỉ số -1, không kiểm tra cả sau Dispose (Java: gotoBeforeFirst).
    void GotoBeforeFirst();

    // Chuyển đến dòng cuối; kết quả rỗng có chỉ số -1, luôn trả true (Java: gotoLast).
    bool GotoLast();

    // Lấy số dòng kết quả (Java: getRows).
    int GetRows();
}
