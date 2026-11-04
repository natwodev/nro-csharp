using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Nro.Core.Logging;

// Bộ ghi nhật ký ánh xạ từng lời gọi Logger Java sang một bản ghi ILogger với mức cố định, tiền tố [TAG] trong thông điệp và thuộc tính có cấu trúc {Tag}.
// Nguồn Java: Utils/Logger.java (nhà cung cấp ILogger quyết định đầu ra console/tệp/xoay tệp/màu/dấu thời gian và đã an toàn luồng nên bỏ khóa toàn cục; log/logln cùng ghi một bản ghi, bỏ xuống dòng cuối, banner/line và fileLog bị lược bỏ như chủ đích).
public sealed class GameLogger
{
    private readonly ILogger _logger;

    // Tạo bộ ghi chuyển mọi bản ghi đến ILogger được cung cấp.
    public GameLogger(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    // Ghi cảnh báo tùy chọn (Java chỉ dùng đối số thêm đầu) rồi một lỗi kèm exception, lớp và phương thức gọi do trình biên dịch cấp; stack trace nằm trong exception để trình cung cấp kết xuất (Java: logException).
    public void LogException(Type clazz, Exception ex, string? message = null, [CallerMemberName] string method = "")
    {
        ArgumentNullException.ThrowIfNull(clazz);
        ArgumentNullException.ThrowIfNull(ex);
        if (message is not null)
        {
            Write(LogLevel.Warning, "EXCEPTION", message);
        }

        _logger.Log(LogLevel.Error, ex, "[EXCEPTION] Class: {Class} Method: {Method} Message: {ExceptionMessage}", clazz.FullName ?? clazz.Name, method, ex.Message);
    }

    // Vạch trang trí không ghi bản ghi nào, vì banner đã được lược bỏ (Java: line).
    public void Line()
    {
    }

    // Trả mã băm định danh hệ 16 của đối tượng, hoặc "null", để liên kết các dòng cùng phiên (Java: shortSession).
    public static string ShortSession(object? session)
        => session is null ? "null" : RuntimeHelpers.GetHashCode(session).ToString("x", CultureInfo.InvariantCulture);

    // Ghi thông tin với nhãn LOG (Java: log).
    public void Log(string text) => Write(LogLevel.Information, "LOG", text);

    // Ghi thông tin với nhãn LOG; màu bị bỏ qua như Java luôn in xanh (Java: log).
    public void Log(string color, string text) => Write(LogLevel.Information, "LOG", text);

    // Ghi thông tin với nhãn LOG; bỏ qua màu nền và màu chữ như Java (Java: log).
    public void Log(string color, string background, string text) => Write(LogLevel.Information, "LOG", text);

    // Ghi thông tin với nhãn LOG (Java: logln).
    public void Logln(string text) => Write(LogLevel.Information, "LOG", text);

    // Ghi thông tin với nhãn LOG; màu bị bỏ qua như Java luôn in xanh (Java: logln).
    public void Logln(string color, string text) => Write(LogLevel.Information, "LOG", text);

    // Ghi thông tin thành công với nhãn OK (Java: success).
    public void Success(string text) => Write(LogLevel.Information, "OK", text);

    // Ghi thông tin thành công với nhãn OK (Java: successln).
    public void Successln(string text) => Write(LogLevel.Information, "OK", text);

    // Ghi cảnh báo với nhãn WARN (Java: warning).
    public void Warning(string text) => Write(LogLevel.Warning, "WARN", text);

    // Ghi cảnh báo với nhãn WARN (Java: warningln).
    public void Warningln(string text) => Write(LogLevel.Warning, "WARN", text);

    // Ghi lỗi với nhãn ERROR (Java: error).
    public void Error(string text) => Write(LogLevel.Error, "ERROR", text);

    // Ghi lỗi với nhãn ERROR (Java: errorln).
    public void Errorln(string text) => Write(LogLevel.Error, "ERROR", text);

    // Ghi thông tin chính với nhãn INFO (Java: primary).
    public void Primary(string text) => Write(LogLevel.Information, "INFO", text);

    // Ghi thông tin chính với nhãn INFO (Java: primaryln).
    public void Primaryln(string text) => Write(LogLevel.Information, "INFO", text);

    // Ghi thông tin kết nối với nhãn CONNECT (Java: connect).
    public void Connect(string text) => Write(LogLevel.Information, "CONNECT", text);

    // Ghi thông tin máy chủ với nhãn SERVER (Java: server).
    public void Server(string text) => Write(LogLevel.Information, "SERVER", text);

    // Ghi thông tin máy chủ với nhãn SERVER_INFO (Java: serverInfo).
    public void ServerInfo(string text) => Write(LogLevel.Information, "SERVER_INFO", text);

    // Ghi lỗi máy chủ với nhãn SERVER_ERR (Java: serverError).
    public void ServerError(string text) => Write(LogLevel.Error, "SERVER_ERR", text);

    // Ghi thông tin đăng nhập với nhãn LOGIN (Java: login).
    public void Login(string text) => Write(LogLevel.Information, "LOGIN", text);

    // Ghi thông tin lưu dữ liệu với nhãn SAVE (Java: save).
    public void Save(string text) => Write(LogLevel.Information, "SAVE", text);

    // Ghi cảnh báo đăng nhập trùng với nhãn DUP_LOGIN (Java: dupLogin).
    public void DupLogin(string text) => Write(LogLevel.Warning, "DUP_LOGIN", text);

    // Ghi thông tin sao lưu với nhãn BACKUP (Java: backup).
    public void Backup(string text) => Write(LogLevel.Information, "BACKUP", text);

    // Ghi thông tin sao lưu với nhãn BACKUP_INFO (Java: backupInfo).
    public void BackupInfo(string text) => Write(LogLevel.Information, "BACKUP_INFO", text);

    // Ghi lỗi sao lưu với nhãn BACKUP_ERR (Java: backupError).
    public void BackupError(string text) => Write(LogLevel.Error, "BACKUP_ERR", text);

    // Ghi thông tin cơ sở dữ liệu với nhãn DB (Java: db).
    public void Db(string text) => Write(LogLevel.Information, "DB", text);

    // Ghi thông tin cơ sở dữ liệu với nhãn DB_INFO (Java: dbInfo).
    public void DbInfo(string text) => Write(LogLevel.Information, "DB_INFO", text);

    // Ghi lỗi cơ sở dữ liệu với nhãn DB_ERR (Java: dbError).
    public void DbError(string text) => Write(LogLevel.Error, "DB_ERR", text);

    // Ghi thông tin sự kiện với nhãn EVENT (Java: event).
    public void Event(string text) => Write(LogLevel.Information, "EVENT", text);

    // Ghi thông tin sự kiện với nhãn EVENT_INFO (Java: eventInfo).
    public void EventInfo(string text) => Write(LogLevel.Information, "EVENT_INFO", text);

    // Ghi thông tin phần thưởng sự kiện với nhãn EVENT_REWARD (Java: eventReward).
    public void EventReward(string text) => Write(LogLevel.Information, "EVENT_REWARD", text);

    // Ghi lỗi sự kiện với nhãn EVENT_ERR (Java: eventError).
    public void EventError(string text) => Write(LogLevel.Error, "EVENT_ERR", text);

    // Ghi thông tin chống DDoS với nhãn ANTI_DDOS (Java: antiDdos).
    public void AntiDdos(string text) => Write(LogLevel.Information, "ANTI_DDOS", text);

    // Ghi thông tin chống DDoS với nhãn DDOS_INFO (Java: antiDdosInfo).
    public void AntiDdosInfo(string text) => Write(LogLevel.Information, "DDOS_INFO", text);

    // Ghi cảnh báo chặn DDoS với nhãn DDOS_BLOCK (Java: antiDdosBlock).
    public void AntiDdosBlock(string text) => Write(LogLevel.Warning, "DDOS_BLOCK", text);

    // Ghi thông tin bỏ chặn DDoS với nhãn DDOS_UNLOCK (Java: antiDdosUnblock).
    public void AntiDdosUnblock(string text) => Write(LogLevel.Information, "DDOS_UNLOCK", text);

    // Ghi thông tin Namek với nhãn NAMEK (Java: namek).
    public void Namek(string text) => Write(LogLevel.Information, "NAMEK", text);

    // Ghi thông tin boss với nhãn BOSS (Java: boss).
    public void Boss(string text) => Write(LogLevel.Information, "BOSS", text);

    // Ghi thông tin giftcode với nhãn GIFTCODE (Java: giftcode).
    public void Giftcode(string text) => Write(LogLevel.Information, "GIFTCODE", text);

    // Ghi thông tin với nhãn do nơi gọi chọn; nhãn trống thành LOG (Java: info).
    public void Info(string tag, string text) => Write(LogLevel.Information, tag, text);

    // Ghi thành công với nhãn do nơi gọi chọn; nhãn trống thành LOG (Java: success).
    public void Success(string tag, string text) => Write(LogLevel.Information, tag, text);

    // Ghi cảnh báo với nhãn do nơi gọi chọn; nhãn trống thành LOG (Java: warn).
    public void Warn(string tag, string text) => Write(LogLevel.Warning, tag, text);

    // Ghi lỗi với nhãn do nơi gọi chọn; nhãn trống thành LOG (Java: err).
    public void Err(string tag, string text) => Write(LogLevel.Error, tag, text);

    // Ghi thông tin hệ thống với nhãn do nơi gọi chọn; nhãn trống thành LOG (Java: system).
    public void System(string tag, string text) => Write(LogLevel.Information, tag, text);

    // Ghi tiêu đề sự kiện thành một bản ghi thông tin; bỏ vạch banner trang trí (Java: eventTitle).
    public void EventTitle(string title) => Write(LogLevel.Information, "EVENT", title);

    // Ghi tiêu đề chống DDoS thành một bản ghi thông tin; bỏ vạch banner trang trí (Java: antiDdosTitle).
    public void AntiDdosTitle(string title) => Write(LogLevel.Information, "ANTI_DDOS", title);

    // Ghi tiêu đề thành một bản ghi thông tin; bỏ vạch banner trang trí (Java: title).
    public void Title(string title) => Write(LogLevel.Information, "LOG", title);

    // Ghi tiêu đề phần thành một bản ghi thông tin; bỏ vạch banner trang trí (Java: section).
    public void Section(string title) => Write(LogLevel.Information, "LOG", title);

    private void Write(LogLevel level, string? tag, string? text)
    {
        if (!_logger.IsEnabled(level))
        {
            return;
        }

        string resolvedTag = string.IsNullOrWhiteSpace(tag) ? "LOG" : tag;
        string resolvedText = (text ?? "null").TrimEnd('\r', '\n');
        _logger.Log(level, "[{Tag}] {Text}", resolvedTag, resolvedText);
    }
}
