using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nro.Core;

/// <summary>
/// Static entry point with the same shape as Java's <c>Utils.Logger</c> (about 120 call sites in the game use <c>Logger.xxx(...)</c> from everywhere, including static code), forwarding to a
/// <see cref="GameLogger"/>. Until the host calls <see cref="Configure(ILogger)"/> or <see cref="Configure(ILoggerFactory)"/> everything goes to <see cref="NullLogger"/> (nothing printed).
/// New code that can receive an <see cref="ILogger"/> by dependency injection should do that (or take a <see cref="GameLogger"/>) instead of calling this class.
/// What was dropped from the Java class, and the level of each method, is described on <see cref="GameLogger"/>.
/// </summary>
public static class Logger
{
    // ANSI codes of the Java class. Call sites such as Logger.log(Logger.GREEN, "text") pass them, so the names stay (nothing prints them any more: colour belongs to the console provider).
    public const string RESET = "\u001b[0m";
    public const string BLACK = "\u001b[0;30m";
    public const string RED = "\u001b[0;31m";
    public const string GREEN = "\u001b[0;32m";
    public const string YELLOW = "\u001b[0;33m";
    public const string BLUE = "\u001b[0;34m";
    public const string PURPLE = "\u001b[0;35m";
    public const string CYAN = "\u001b[0;36m";
    public const string WHITE = "\u001b[0;37m";
    public const string BLACK_BG = "\u001b[40m";
    public const string RED_BG = "\u001b[41m";
    public const string GREEN_BG = "\u001b[42m";
    public const string YELLOW_BG = "\u001b[43m";
    public const string BLUE_BG = "\u001b[44m";
    public const string PURPLE_BG = "\u001b[45m";
    public const string CYAN_BG = "\u001b[46m";
    public const string WHITE_BG = "\u001b[47m";
    public const string BOLD = "\u001b[1m";
    public const string UNDERLINE = "\u001b[4m";
    public const string REVERSED = "\u001b[7m";

    private static volatile GameLogger _current = new(NullLogger.Instance);

    /// <summary>The logger every static method forwards to.</summary>
    public static GameLogger Current => _current;

    /// <summary>Sends all static calls to <paramref name="logger"/>. Call once at start-up (the host does this after building its logging).</summary>
    public static void Configure(ILogger logger) => _current = new GameLogger(logger);

    /// <summary>Sends all static calls to a logger of <paramref name="loggerFactory"/> whose category is <c>Nro</c>.</summary>
    public static void Configure(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        Configure(loggerFactory.CreateLogger("Nro"));
    }

    /// <summary>Back to the silent <see cref="NullLogger"/> (tests).</summary>
    public static void Reset() => _current = new GameLogger(NullLogger.Instance);

    /// <inheritdoc cref="GameLogger.LogException"/>
    public static void LogException(Type clazz, Exception ex, string? message = null, [CallerMemberName] string method = "")
        => Current.LogException(clazz, ex, message, method);

    /// <inheritdoc cref="GameLogger.Line"/>
    public static void Line() => Current.Line();

    /// <inheritdoc cref="GameLogger.ShortSession"/>
    public static string ShortSession(object? session) => GameLogger.ShortSession(session);

    /// <summary>Java <c>log(String)</c>: information, tag LOG.</summary>
    public static void Log(string text) => Current.Log(text);

    /// <summary>Java <c>log(String color, String text)</c>: the colour was already ignored in Java (it always printed green).</summary>
    public static void Log(string color, string text) => Current.Log(color, text);

    /// <summary>Java <c>log(String color, String background, String text)</c>: colours ignored, as in Java.</summary>
    public static void Log(string color, string background, string text) => Current.Log(color, background, text);

    /// <summary>Java <c>logln(String)</c>: information, tag LOG.</summary>
    public static void Logln(string text) => Current.Logln(text);

    /// <summary>Java <c>logln(String color, String text)</c>: the colour was already ignored in Java (it always printed green).</summary>
    public static void Logln(string color, string text) => Current.Logln(color, text);

    /// <summary>Java <c>success(String)</c>: information, tag OK.</summary>
    public static void Success(string text) => Current.Success(text);

    /// <summary>Java <c>successln(String)</c>: information, tag OK.</summary>
    public static void Successln(string text) => Current.Successln(text);

    /// <summary>Java <c>warning(String)</c>: warning, tag WARN.</summary>
    public static void Warning(string text) => Current.Warning(text);

    /// <summary>Java <c>warningln(String)</c>: warning, tag WARN.</summary>
    public static void Warningln(string text) => Current.Warningln(text);

    /// <summary>Java <c>error(String)</c>: error, tag ERROR.</summary>
    public static void Error(string text) => Current.Error(text);

    /// <summary>Java <c>errorln(String)</c>: error, tag ERROR.</summary>
    public static void Errorln(string text) => Current.Errorln(text);

    /// <summary>Java <c>primary(String)</c>: information, tag INFO.</summary>
    public static void Primary(string text) => Current.Primary(text);

    /// <summary>Java <c>primaryln(String)</c>: information, tag INFO.</summary>
    public static void Primaryln(string text) => Current.Primaryln(text);

    /// <summary>Java <c>connect(String)</c>: information, tag CONNECT.</summary>
    public static void Connect(string text) => Current.Connect(text);

    /// <summary>Java <c>server(String)</c>: information, tag SERVER.</summary>
    public static void Server(string text) => Current.Server(text);

    /// <summary>Java <c>serverInfo(String)</c>: information, tag SERVER_INFO.</summary>
    public static void ServerInfo(string text) => Current.ServerInfo(text);

    /// <summary>Java <c>serverError(String)</c>: error, tag SERVER_ERR.</summary>
    public static void ServerError(string text) => Current.ServerError(text);

    /// <summary>Java <c>login(String)</c>: information, tag LOGIN.</summary>
    public static void Login(string text) => Current.Login(text);

    /// <summary>Java <c>save(String)</c>: information, tag SAVE.</summary>
    public static void Save(string text) => Current.Save(text);

    /// <summary>Java <c>dupLogin(String)</c>: warning, tag DUP_LOGIN.</summary>
    public static void DupLogin(string text) => Current.DupLogin(text);

    /// <summary>Java <c>backup(String)</c>: information, tag BACKUP.</summary>
    public static void Backup(string text) => Current.Backup(text);

    /// <summary>Java <c>backupInfo(String)</c>: information, tag BACKUP_INFO.</summary>
    public static void BackupInfo(string text) => Current.BackupInfo(text);

    /// <summary>Java <c>backupError(String)</c>: error, tag BACKUP_ERR.</summary>
    public static void BackupError(string text) => Current.BackupError(text);

    /// <summary>Java <c>db(String)</c>: information, tag DB.</summary>
    public static void Db(string text) => Current.Db(text);

    /// <summary>Java <c>dbInfo(String)</c>: information, tag DB_INFO.</summary>
    public static void DbInfo(string text) => Current.DbInfo(text);

    /// <summary>Java <c>dbError(String)</c>: error, tag DB_ERR.</summary>
    public static void DbError(string text) => Current.DbError(text);

    /// <summary>Java <c>event(String)</c>: information, tag EVENT.</summary>
    public static void Event(string text) => Current.Event(text);

    /// <summary>Java <c>eventInfo(String)</c>: information, tag EVENT_INFO.</summary>
    public static void EventInfo(string text) => Current.EventInfo(text);

    /// <summary>Java <c>eventReward(String)</c>: information, tag EVENT_REWARD.</summary>
    public static void EventReward(string text) => Current.EventReward(text);

    /// <summary>Java <c>eventError(String)</c>: error, tag EVENT_ERR.</summary>
    public static void EventError(string text) => Current.EventError(text);

    /// <summary>Java <c>antiDdos(String)</c>: information, tag ANTI_DDOS.</summary>
    public static void AntiDdos(string text) => Current.AntiDdos(text);

    /// <summary>Java <c>antiDdosInfo(String)</c>: information, tag DDOS_INFO.</summary>
    public static void AntiDdosInfo(string text) => Current.AntiDdosInfo(text);

    /// <summary>Java <c>antiDdosBlock(String)</c>: warning, tag DDOS_BLOCK.</summary>
    public static void AntiDdosBlock(string text) => Current.AntiDdosBlock(text);

    /// <summary>Java <c>antiDdosUnblock(String)</c>: information, tag DDOS_UNLOCK.</summary>
    public static void AntiDdosUnblock(string text) => Current.AntiDdosUnblock(text);

    /// <summary>Java <c>namek(String)</c>: information, tag NAMEK.</summary>
    public static void Namek(string text) => Current.Namek(text);

    /// <summary>Java <c>boss(String)</c>: information, tag BOSS.</summary>
    public static void Boss(string text) => Current.Boss(text);

    /// <summary>Java <c>giftcode(String)</c>: information, tag GIFTCODE.</summary>
    public static void Giftcode(string text) => Current.Giftcode(text);

    /// <summary>Java <c>info(String tag, String text)</c>: information with a caller-chosen tag (blank becomes LOG).</summary>
    public static void Info(string tag, string text) => Current.Info(tag, text);

    /// <summary>Java <c>success(String tag, String text)</c>: information with a caller-chosen tag (blank becomes LOG).</summary>
    public static void Success(string tag, string text) => Current.Success(tag, text);

    /// <summary>Java <c>warn(String tag, String text)</c>: warning with a caller-chosen tag (blank becomes LOG).</summary>
    public static void Warn(string tag, string text) => Current.Warn(tag, text);

    /// <summary>Java <c>err(String tag, String text)</c>: error with a caller-chosen tag (blank becomes LOG).</summary>
    public static void Err(string tag, string text) => Current.Err(tag, text);

    /// <summary>Java <c>system(String tag, String text)</c>: information with a caller-chosen tag (blank becomes LOG).</summary>
    public static void System(string tag, string text) => Current.System(tag, text);

    /// <summary>Java <c>eventTitle(String)</c>: a heading. The decorative rules of the console banner are dropped; one information entry is written.</summary>
    public static void EventTitle(string title) => Current.EventTitle(title);

    /// <summary>Java <c>antiDdosTitle(String)</c>: a heading. The decorative rules of the console banner are dropped; one information entry is written.</summary>
    public static void AntiDdosTitle(string title) => Current.AntiDdosTitle(title);

    /// <summary>Java <c>title(String)</c>: a heading. The decorative rules of the console banner are dropped; one information entry is written.</summary>
    public static void Title(string title) => Current.Title(title);

    /// <summary>Java <c>section(String)</c>: a heading. The decorative rules of the console banner are dropped; one information entry is written.</summary>
    public static void Section(string title) => Current.Section(title);
}
