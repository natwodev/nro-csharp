using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nro.Core.Logging;

// Lối vào tĩnh chuyển mọi lệnh ghi log sang một GameLogger; khi host chưa gọi Configure(ILogger) hoặc Configure(ILoggerFactory) thì mọi thứ đi vào NullLogger (không in gì)
// Nguồn Java: Utils/Logger.java (giữ cùng hình dạng vì khoảng 120 chỗ trong game gọi Logger.xxx(...) từ khắp nơi, kể cả code tĩnh)
// Code mới nhận được ILogger qua dependency injection thì nên dùng cách đó (hoặc nhận một GameLogger) thay vì gọi lớp này.
// Những gì bị bỏ so với lớp Java và mức log của từng hàm: xem ghi chú ở GameLogger.
public static class Logger
{
    // Mã ANSI của lớp Java. Các chỗ gọi như Logger.log(Logger.GREEN, "text") vẫn truyền chúng vào nên giữ nguyên tên (không còn gì in chúng ra: màu là việc của provider console).
    // RESET: trả về màu mặc định
    public const string RESET = "\u001b[0m";
    // Màu chữ
    public const string BLACK = "\u001b[0;30m";
    public const string RED = "\u001b[0;31m";
    public const string GREEN = "\u001b[0;32m";
    public const string YELLOW = "\u001b[0;33m";
    public const string BLUE = "\u001b[0;34m";
    public const string PURPLE = "\u001b[0;35m";
    public const string CYAN = "\u001b[0;36m";
    public const string WHITE = "\u001b[0;37m";
    // Màu nền
    public const string BLACK_BG = "\u001b[40m";
    public const string RED_BG = "\u001b[41m";
    public const string GREEN_BG = "\u001b[42m";
    public const string YELLOW_BG = "\u001b[43m";
    public const string BLUE_BG = "\u001b[44m";
    public const string PURPLE_BG = "\u001b[45m";
    public const string CYAN_BG = "\u001b[46m";
    public const string WHITE_BG = "\u001b[47m";
    // Kiểu chữ: đậm, gạch chân, đảo màu
    public const string BOLD = "\u001b[1m";
    public const string UNDERLINE = "\u001b[4m";
    public const string REVERSED = "\u001b[7m";

    // GameLogger đang dùng; volatile để mọi luồng thấy ngay khi Configure/Reset thay nó
    private static volatile GameLogger _current = new(NullLogger.Instance);

    // GameLogger mà mọi hàm tĩnh chuyển tiếp tới
    public static GameLogger Current => _current;

    // Chuyển mọi lệnh gọi tĩnh sang logger cho trước; gọi một lần lúc khởi động (host làm việc này sau khi dựng xong phần logging)
    public static void Configure(ILogger logger) => _current = new GameLogger(logger);

    // Chuyển mọi lệnh gọi tĩnh sang một logger của loggerFactory có category là Nro
    public static void Configure(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        Configure(loggerFactory.CreateLogger("Nro"));
    }

    // Trở lại NullLogger im lặng (dùng trong test)
    public static void Reset() => _current = new GameLogger(NullLogger.Instance);

    // Ghi cảnh báo kèm message nếu có (Java chỉ dùng tham số phụ đầu tiên), rồi MỘT dòng lỗi đính kèm exception, ghi tên lớp và hàm gọi
    // (trình biên dịch tự điền method, Java thì soi stack) (Java: logException(Class<?> clazz, Exception ex, String... log))
    public static void LogException(Type clazz, Exception ex, string? message = null, [CallerMemberName] string method = "")
        => Current.LogException(clazz, ex, message, method);

    // Đường kẻ trang trí: không ghi gì (Java: line())
    public static void Line() => Current.Line();

    // Mã băm định danh của đối tượng dạng hex ("null" nếu null), id ngắn để nối các dòng log của cùng một phiên (Java: shortSession(Object))
    public static string ShortSession(object? session) => GameLogger.ShortSession(session);

    // Ghi mức Information, tag LOG (Java: log(String))
    public static void Log(string text) => Current.Log(text);

    // Ghi mức Information, tag LOG; màu bị bỏ qua, Java cũng đã bỏ qua (luôn in màu xanh lá) (Java: log(String color, String text))
    public static void Log(string color, string text) => Current.Log(color, text);

    // Ghi mức Information, tag LOG; bỏ qua màu chữ và màu nền như Java (Java: log(String color, String background, String text))
    public static void Log(string color, string background, string text) => Current.Log(color, background, text);

    // Ghi mức Information, tag LOG (Java: logln(String))
    public static void Logln(string text) => Current.Logln(text);

    // Ghi mức Information, tag LOG; màu bị bỏ qua, Java cũng đã bỏ qua (luôn in màu xanh lá) (Java: logln(String color, String text))
    public static void Logln(string color, string text) => Current.Logln(color, text);

    // Ghi mức Information, tag OK (Java: success(String))
    public static void Success(string text) => Current.Success(text);

    // Ghi mức Information, tag OK (Java: successln(String))
    public static void Successln(string text) => Current.Successln(text);

    // Ghi mức Warning, tag WARN (Java: warning(String))
    public static void Warning(string text) => Current.Warning(text);

    // Ghi mức Warning, tag WARN (Java: warningln(String))
    public static void Warningln(string text) => Current.Warningln(text);

    // Ghi mức Error, tag ERROR (Java: error(String))
    public static void Error(string text) => Current.Error(text);

    // Ghi mức Error, tag ERROR (Java: errorln(String))
    public static void Errorln(string text) => Current.Errorln(text);

    // Ghi mức Information, tag INFO (Java: primary(String))
    public static void Primary(string text) => Current.Primary(text);

    // Ghi mức Information, tag INFO (Java: primaryln(String))
    public static void Primaryln(string text) => Current.Primaryln(text);

    // Ghi mức Information, tag CONNECT (Java: connect(String))
    public static void Connect(string text) => Current.Connect(text);

    // Ghi mức Information, tag SERVER (Java: server(String))
    public static void Server(string text) => Current.Server(text);

    // Ghi mức Information, tag SERVER_INFO (Java: serverInfo(String))
    public static void ServerInfo(string text) => Current.ServerInfo(text);

    // Ghi mức Error, tag SERVER_ERR (Java: serverError(String))
    public static void ServerError(string text) => Current.ServerError(text);

    // Ghi mức Information, tag LOGIN (Java: login(String))
    public static void Login(string text) => Current.Login(text);

    // Ghi mức Information, tag SAVE (Java: save(String))
    public static void Save(string text) => Current.Save(text);

    // Ghi mức Warning, tag DUP_LOGIN (Java: dupLogin(String))
    public static void DupLogin(string text) => Current.DupLogin(text);

    // Ghi mức Information, tag BACKUP (Java: backup(String))
    public static void Backup(string text) => Current.Backup(text);

    // Ghi mức Information, tag BACKUP_INFO (Java: backupInfo(String))
    public static void BackupInfo(string text) => Current.BackupInfo(text);

    // Ghi mức Error, tag BACKUP_ERR (Java: backupError(String))
    public static void BackupError(string text) => Current.BackupError(text);

    // Ghi mức Information, tag DB (Java: db(String))
    public static void Db(string text) => Current.Db(text);

    // Ghi mức Information, tag DB_INFO (Java: dbInfo(String))
    public static void DbInfo(string text) => Current.DbInfo(text);

    // Ghi mức Error, tag DB_ERR (Java: dbError(String))
    public static void DbError(string text) => Current.DbError(text);

    // Ghi mức Information, tag EVENT (Java: event(String))
    public static void Event(string text) => Current.Event(text);

    // Ghi mức Information, tag EVENT_INFO (Java: eventInfo(String))
    public static void EventInfo(string text) => Current.EventInfo(text);

    // Ghi mức Information, tag EVENT_REWARD (Java: eventReward(String))
    public static void EventReward(string text) => Current.EventReward(text);

    // Ghi mức Error, tag EVENT_ERR (Java: eventError(String))
    public static void EventError(string text) => Current.EventError(text);

    // Ghi mức Information, tag ANTI_DDOS (Java: antiDdos(String))
    public static void AntiDdos(string text) => Current.AntiDdos(text);

    // Ghi mức Information, tag DDOS_INFO (Java: antiDdosInfo(String))
    public static void AntiDdosInfo(string text) => Current.AntiDdosInfo(text);

    // Ghi mức Warning, tag DDOS_BLOCK (Java: antiDdosBlock(String))
    public static void AntiDdosBlock(string text) => Current.AntiDdosBlock(text);

    // Ghi mức Information, tag DDOS_UNLOCK (Java: antiDdosUnblock(String))
    public static void AntiDdosUnblock(string text) => Current.AntiDdosUnblock(text);

    // Ghi mức Information, tag NAMEK (Java: namek(String))
    public static void Namek(string text) => Current.Namek(text);

    // Ghi mức Information, tag BOSS (Java: boss(String))
    public static void Boss(string text) => Current.Boss(text);

    // Ghi mức Information, tag GIFTCODE (Java: giftcode(String))
    public static void Giftcode(string text) => Current.Giftcode(text);

    // Ghi mức Information với tag do người gọi chọn (tag trống thành LOG) (Java: info(String tag, String text))
    public static void Info(string tag, string text) => Current.Info(tag, text);

    // Ghi mức Information với tag do người gọi chọn (tag trống thành LOG) (Java: success(String tag, String text))
    public static void Success(string tag, string text) => Current.Success(tag, text);

    // Ghi mức Warning với tag do người gọi chọn (tag trống thành LOG) (Java: warn(String tag, String text))
    public static void Warn(string tag, string text) => Current.Warn(tag, text);

    // Ghi mức Error với tag do người gọi chọn (tag trống thành LOG) (Java: err(String tag, String text))
    public static void Err(string tag, string text) => Current.Err(tag, text);

    // Ghi mức Information với tag do người gọi chọn (tag trống thành LOG) (Java: system(String tag, String text))
    public static void System(string tag, string text) => Current.System(tag, text);

    // Ghi tiêu đề: bỏ các đường kẻ trang trí của banner trên console, chỉ ghi một dòng mức Information (Java: eventTitle(String))
    public static void EventTitle(string title) => Current.EventTitle(title);

    // Ghi tiêu đề: bỏ các đường kẻ trang trí của banner trên console, chỉ ghi một dòng mức Information (Java: antiDdosTitle(String))
    public static void AntiDdosTitle(string title) => Current.AntiDdosTitle(title);

    // Ghi tiêu đề: bỏ các đường kẻ trang trí của banner trên console, chỉ ghi một dòng mức Information (Java: title(String))
    public static void Title(string title) => Current.Title(title);

    // Ghi tiêu đề: bỏ các đường kẻ trang trí của banner trên console, chỉ ghi một dòng mức Information (Java: section(String))
    public static void Section(string title) => Current.Section(title);
}
