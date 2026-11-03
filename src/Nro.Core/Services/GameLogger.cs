using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Nro.Core;

/// <summary>
/// Port of <c>Utils/Logger.java</c> on top of <see cref="ILogger"/>: the same method names (PascalCase) so a call site ports one to one, but the output
/// (console, file, rotation, colours) is whatever the <c>ILogger</c> providers configured by the host do.
/// <para>
/// Each Java method becomes one <see cref="ILogger"/> entry at a fixed level: <c>log/logln/primary/info/system/success/title/section</c> and the topic loggers (<c>server</c>, <c>login</c>, <c>save</c>,
/// <c>db</c>, <c>backup</c>, <c>event*</c>, <c>antiDdos</c>, <c>namek</c>, <c>boss</c>, <c>giftcode</c>) are <see cref="LogLevel.Information"/>; <c>warning/warn/dupLogin/antiDdosBlock</c> are
/// <see cref="LogLevel.Warning"/> (Java printed the last two in yellow/red but they are expected defensive events, not faults); <c>error/err</c> and every <c>*Error</c>/<c>serverError</c>
/// are <see cref="LogLevel.Error"/>. The Java "tag" becomes the prefix <c>[TAG]</c> of the message text and is also the <c>{Tag}</c> structured property.
/// </para>
/// <para>
/// DROPPED ON PURPOSE (see also <see cref="Logger"/>): ANSI colours and the 14-column tag padding (a provider decides how to render); the <c>HH:mm:ss</c> timestamp (providers add their own);
/// the global lock and the console writes (providers are thread-safe); the "no newline" variants (<c>log</c> and <c>logln</c> both write one entry; a trailing line break in the text is removed);
/// decorative rules of <c>title/section/line/eventTitle/antiDdosTitle</c> (one entry carrying the title; <see cref="Line"/> writes nothing); <c>fileLog(player, text)</c>, which spawned a thread per call and
/// appended to <c>log/&lt;player&gt;_log.txt</c> (nothing calls it; a file sink belongs to the logging provider, e.g. a Serilog/NLog file target).
/// </para>
/// </summary>
public sealed class GameLogger
{
    private readonly ILogger _logger;

    /// <param name="logger">Where every entry goes.</param>
    public GameLogger(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Java <c>logException(Class&lt;?&gt; clazz, Exception ex, String... log)</c>. Writes an optional warning with <paramref name="message"/> (Java used only the first extra argument),
    /// then ONE error entry with the exception attached, naming the class and the calling method (the compiler supplies <paramref name="method"/>, where Java inspected the stack).
    /// The 496 call sites do <c>Logger.logException(Boss.class, e)</c> / <c>(.., e, "text")</c>, which port to <c>Logger.LogException(typeof(Boss), e)</c> / <c>(.., e, "text")</c>.
    /// The stack trace is not split into one line per entry: it travels in the <see cref="Exception"/>, which providers render.
    /// </summary>
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

    /// <summary>Java <c>line()</c>: a decorative rule. Writes nothing (see the class remarks).</summary>
    public void Line()
    {
    }

    /// <summary>Java <c>shortSession(Object)</c>: the identity hash of the object in hex (<c>"null"</c> for null); a short id to correlate log lines of one session.</summary>
    public static string ShortSession(object? session)
        => session is null ? "null" : RuntimeHelpers.GetHashCode(session).ToString("x", CultureInfo.InvariantCulture);

    /// <summary>Java <c>log(String)</c>: information, tag LOG.</summary>
    public void Log(string text) => Write(LogLevel.Information, "LOG", text);

    /// <summary>Java <c>log(String color, String text)</c>: the colour was already ignored in Java (it always printed green).</summary>
    public void Log(string color, string text) => Write(LogLevel.Information, "LOG", text);

    /// <summary>Java <c>log(String color, String background, String text)</c>: colours ignored, as in Java.</summary>
    public void Log(string color, string background, string text) => Write(LogLevel.Information, "LOG", text);

    /// <summary>Java <c>logln(String)</c>: information, tag LOG.</summary>
    public void Logln(string text) => Write(LogLevel.Information, "LOG", text);

    /// <summary>Java <c>logln(String color, String text)</c>: the colour was already ignored in Java (it always printed green).</summary>
    public void Logln(string color, string text) => Write(LogLevel.Information, "LOG", text);

    /// <summary>Java <c>success(String)</c>: information, tag OK.</summary>
    public void Success(string text) => Write(LogLevel.Information, "OK", text);

    /// <summary>Java <c>successln(String)</c>: information, tag OK.</summary>
    public void Successln(string text) => Write(LogLevel.Information, "OK", text);

    /// <summary>Java <c>warning(String)</c>: warning, tag WARN.</summary>
    public void Warning(string text) => Write(LogLevel.Warning, "WARN", text);

    /// <summary>Java <c>warningln(String)</c>: warning, tag WARN.</summary>
    public void Warningln(string text) => Write(LogLevel.Warning, "WARN", text);

    /// <summary>Java <c>error(String)</c>: error, tag ERROR.</summary>
    public void Error(string text) => Write(LogLevel.Error, "ERROR", text);

    /// <summary>Java <c>errorln(String)</c>: error, tag ERROR.</summary>
    public void Errorln(string text) => Write(LogLevel.Error, "ERROR", text);

    /// <summary>Java <c>primary(String)</c>: information, tag INFO.</summary>
    public void Primary(string text) => Write(LogLevel.Information, "INFO", text);

    /// <summary>Java <c>primaryln(String)</c>: information, tag INFO.</summary>
    public void Primaryln(string text) => Write(LogLevel.Information, "INFO", text);

    /// <summary>Java <c>connect(String)</c>: information, tag CONNECT.</summary>
    public void Connect(string text) => Write(LogLevel.Information, "CONNECT", text);

    /// <summary>Java <c>server(String)</c>: information, tag SERVER.</summary>
    public void Server(string text) => Write(LogLevel.Information, "SERVER", text);

    /// <summary>Java <c>serverInfo(String)</c>: information, tag SERVER_INFO.</summary>
    public void ServerInfo(string text) => Write(LogLevel.Information, "SERVER_INFO", text);

    /// <summary>Java <c>serverError(String)</c>: error, tag SERVER_ERR.</summary>
    public void ServerError(string text) => Write(LogLevel.Error, "SERVER_ERR", text);

    /// <summary>Java <c>login(String)</c>: information, tag LOGIN.</summary>
    public void Login(string text) => Write(LogLevel.Information, "LOGIN", text);

    /// <summary>Java <c>save(String)</c>: information, tag SAVE.</summary>
    public void Save(string text) => Write(LogLevel.Information, "SAVE", text);

    /// <summary>Java <c>dupLogin(String)</c>: warning, tag DUP_LOGIN.</summary>
    public void DupLogin(string text) => Write(LogLevel.Warning, "DUP_LOGIN", text);

    /// <summary>Java <c>backup(String)</c>: information, tag BACKUP.</summary>
    public void Backup(string text) => Write(LogLevel.Information, "BACKUP", text);

    /// <summary>Java <c>backupInfo(String)</c>: information, tag BACKUP_INFO.</summary>
    public void BackupInfo(string text) => Write(LogLevel.Information, "BACKUP_INFO", text);

    /// <summary>Java <c>backupError(String)</c>: error, tag BACKUP_ERR.</summary>
    public void BackupError(string text) => Write(LogLevel.Error, "BACKUP_ERR", text);

    /// <summary>Java <c>db(String)</c>: information, tag DB.</summary>
    public void Db(string text) => Write(LogLevel.Information, "DB", text);

    /// <summary>Java <c>dbInfo(String)</c>: information, tag DB_INFO.</summary>
    public void DbInfo(string text) => Write(LogLevel.Information, "DB_INFO", text);

    /// <summary>Java <c>dbError(String)</c>: error, tag DB_ERR.</summary>
    public void DbError(string text) => Write(LogLevel.Error, "DB_ERR", text);

    /// <summary>Java <c>event(String)</c>: information, tag EVENT.</summary>
    public void Event(string text) => Write(LogLevel.Information, "EVENT", text);

    /// <summary>Java <c>eventInfo(String)</c>: information, tag EVENT_INFO.</summary>
    public void EventInfo(string text) => Write(LogLevel.Information, "EVENT_INFO", text);

    /// <summary>Java <c>eventReward(String)</c>: information, tag EVENT_REWARD.</summary>
    public void EventReward(string text) => Write(LogLevel.Information, "EVENT_REWARD", text);

    /// <summary>Java <c>eventError(String)</c>: error, tag EVENT_ERR.</summary>
    public void EventError(string text) => Write(LogLevel.Error, "EVENT_ERR", text);

    /// <summary>Java <c>antiDdos(String)</c>: information, tag ANTI_DDOS.</summary>
    public void AntiDdos(string text) => Write(LogLevel.Information, "ANTI_DDOS", text);

    /// <summary>Java <c>antiDdosInfo(String)</c>: information, tag DDOS_INFO.</summary>
    public void AntiDdosInfo(string text) => Write(LogLevel.Information, "DDOS_INFO", text);

    /// <summary>Java <c>antiDdosBlock(String)</c>: warning, tag DDOS_BLOCK.</summary>
    public void AntiDdosBlock(string text) => Write(LogLevel.Warning, "DDOS_BLOCK", text);

    /// <summary>Java <c>antiDdosUnblock(String)</c>: information, tag DDOS_UNLOCK.</summary>
    public void AntiDdosUnblock(string text) => Write(LogLevel.Information, "DDOS_UNLOCK", text);

    /// <summary>Java <c>namek(String)</c>: information, tag NAMEK.</summary>
    public void Namek(string text) => Write(LogLevel.Information, "NAMEK", text);

    /// <summary>Java <c>boss(String)</c>: information, tag BOSS.</summary>
    public void Boss(string text) => Write(LogLevel.Information, "BOSS", text);

    /// <summary>Java <c>giftcode(String)</c>: information, tag GIFTCODE.</summary>
    public void Giftcode(string text) => Write(LogLevel.Information, "GIFTCODE", text);

    /// <summary>Java <c>info(String tag, String text)</c>: information with a caller-chosen tag (blank becomes LOG).</summary>
    public void Info(string tag, string text) => Write(LogLevel.Information, tag, text);

    /// <summary>Java <c>success(String tag, String text)</c>: information with a caller-chosen tag (blank becomes LOG).</summary>
    public void Success(string tag, string text) => Write(LogLevel.Information, tag, text);

    /// <summary>Java <c>warn(String tag, String text)</c>: warning with a caller-chosen tag (blank becomes LOG).</summary>
    public void Warn(string tag, string text) => Write(LogLevel.Warning, tag, text);

    /// <summary>Java <c>err(String tag, String text)</c>: error with a caller-chosen tag (blank becomes LOG).</summary>
    public void Err(string tag, string text) => Write(LogLevel.Error, tag, text);

    /// <summary>Java <c>system(String tag, String text)</c>: information with a caller-chosen tag (blank becomes LOG).</summary>
    public void System(string tag, string text) => Write(LogLevel.Information, tag, text);

    /// <summary>Java <c>eventTitle(String)</c>: a heading. The decorative rules of the console banner are dropped; one information entry is written.</summary>
    public void EventTitle(string title) => Write(LogLevel.Information, "EVENT", title);

    /// <summary>Java <c>antiDdosTitle(String)</c>: a heading. The decorative rules of the console banner are dropped; one information entry is written.</summary>
    public void AntiDdosTitle(string title) => Write(LogLevel.Information, "ANTI_DDOS", title);

    /// <summary>Java <c>title(String)</c>: a heading. The decorative rules of the console banner are dropped; one information entry is written.</summary>
    public void Title(string title) => Write(LogLevel.Information, "LOG", title);

    /// <summary>Java <c>section(String)</c>: a heading. The decorative rules of the console banner are dropped; one information entry is written.</summary>
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
