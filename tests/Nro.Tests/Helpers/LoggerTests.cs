using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Nro.Core;

namespace Nro.Tests.Helpers;

/// <summary>Collection that never runs in parallel with another one: <see cref="Logger"/> holds static state (the configured <see cref="ILogger"/>).</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class StaticLoggerCollection
{
    public const string Name = "static Nro.Core.Logger";
}

/// <summary>
/// <see cref="GameLogger"/> and the static <see cref="Logger"/>: every method of the Java class reaches the <see cref="ILogger"/> at the documented level with the
/// expected <c>[TAG] text</c>. The expected levels and tags below are written out by hand from <c>Utils/Logger.java</c> (not read back from the code under test).
/// </summary>
[Collection(StaticLoggerCollection.Name)]
public class LoggerTests
{
    private const LogLevel I = LogLevel.Information;
    private const LogLevel W = LogLevel.Warning;
    private const LogLevel E = LogLevel.Error;

    // C# method name -> (level, tag) for the single-argument methods; the Java method name is the same with a lower-case first letter.
    private static readonly (string Method, LogLevel Level, string Tag)[] Fixed =
    [
        ("Log", I, "LOG"), ("Logln", I, "LOG"),
        ("Success", I, "OK"), ("Successln", I, "OK"),
        ("Warning", W, "WARN"), ("Warningln", W, "WARN"),
        ("Error", E, "ERROR"), ("Errorln", E, "ERROR"),
        ("Primary", I, "INFO"), ("Primaryln", I, "INFO"),
        ("Connect", I, "CONNECT"), ("Server", I, "SERVER"), ("ServerInfo", I, "SERVER_INFO"), ("ServerError", E, "SERVER_ERR"),
        ("Login", I, "LOGIN"), ("Save", I, "SAVE"), ("DupLogin", W, "DUP_LOGIN"),
        ("Backup", I, "BACKUP"), ("BackupInfo", I, "BACKUP_INFO"), ("BackupError", E, "BACKUP_ERR"),
        ("Db", I, "DB"), ("DbInfo", I, "DB_INFO"), ("DbError", E, "DB_ERR"),
        ("Event", I, "EVENT"), ("EventInfo", I, "EVENT_INFO"), ("EventReward", I, "EVENT_REWARD"), ("EventError", E, "EVENT_ERR"),
        ("AntiDdos", I, "ANTI_DDOS"), ("AntiDdosInfo", I, "DDOS_INFO"), ("AntiDdosBlock", W, "DDOS_BLOCK"), ("AntiDdosUnblock", I, "DDOS_UNLOCK"),
        ("Namek", I, "NAMEK"), ("Boss", I, "BOSS"), ("Giftcode", I, "GIFTCODE"),
        ("EventTitle", I, "EVENT"), ("AntiDdosTitle", I, "ANTI_DDOS"), ("Title", I, "LOG"), ("Section", I, "LOG"),
    ];

    // methods taking (tag, text)
    private static readonly (string Method, LogLevel Level)[] Tagged = [("Info", I), ("Success", I), ("Warn", W), ("Err", E), ("System", I)];

    // every method name of the Java class (see Utils/Logger.java); fileLog is deliberately not ported
    private static readonly string[] JavaMethodNames =
    [
        "log", "logln", "success", "successln", "warning", "warningln", "error", "errorln", "primary", "primaryln", "info", "warn", "err", "system",
        "connect", "server", "serverInfo", "serverError", "login", "save", "dupLogin", "backup", "backupInfo", "backupError", "db", "dbInfo", "dbError",
        "eventTitle", "event", "eventInfo", "eventReward", "eventError", "antiDdosTitle", "antiDdos", "antiDdosInfo", "antiDdosBlock", "antiDdosUnblock",
        "namek", "boss", "giftcode", "line", "title", "section", "logException", "shortSession",
    ];

    private static string Pascal(string javaName) => char.ToUpperInvariant(javaName[0]) + javaName[1..];

    public static IEnumerable<object[]> FixedMethods() => Fixed.Select(f => new object[] { f.Method });

    public static IEnumerable<object[]> TaggedMethods() => Tagged.Select(t => new object[] { t.Method });

    [Fact]
    public void Every_method_name_of_the_Java_Logger_exists_on_both_the_instance_and_the_static_facade()
    {
        foreach (string java in JavaMethodNames)
        {
            string name = Pascal(java);
            Assert.True(typeof(GameLogger).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Any(m => m.Name == name), "GameLogger." + name);
            Assert.True(typeof(Logger).GetMethods(BindingFlags.Public | BindingFlags.Static).Any(m => m.Name == name), "Logger." + name);
        }

        // the table above covers every logging method, so no method can be added without a level decision
        string[] covered = Fixed.Select(f => f.Method).Concat(Tagged.Select(t => t.Method)).Concat(["LogException", "Line", "ShortSession", "Configure", "Reset", "Current"]).Distinct().OrderBy(n => n, StringComparer.Ordinal).ToArray();
        string[] actual = typeof(Logger).GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => m is MethodInfo { IsSpecialName: false } || m is PropertyInfo)
            .Select(m => m.Name).Distinct().OrderBy(n => n, StringComparer.Ordinal).ToArray();
        Assert.Equal(covered, actual);
    }

    [Theory]
    [MemberData(nameof(FixedMethods))]
    public void Each_single_argument_method_writes_one_entry_at_its_level_with_its_tag(string method)
    {
        (_, LogLevel level, string tag) = Fixed.Single(f => f.Method == method);

        var recorder = new RecordingLogger();
        MethodInfo instance = typeof(GameLogger).GetMethod(method, [typeof(string)])!;
        instance.Invoke(new GameLogger(recorder), ["hello world"]);
        AssertSingle(recorder, level, tag, "hello world");

        var staticRecorder = new RecordingLogger();
        Logger.Configure(staticRecorder);
        try
        {
            typeof(Logger).GetMethod(method, [typeof(string)])!.Invoke(null, ["hello world"]);
            AssertSingle(staticRecorder, level, tag, "hello world");
        }
        finally
        {
            Logger.Reset();
        }
    }

    [Theory]
    [MemberData(nameof(TaggedMethods))]
    public void Each_tag_and_text_method_uses_the_given_tag(string method)
    {
        (_, LogLevel level) = Tagged.Single(t => t.Method == method);

        var recorder = new RecordingLogger();
        typeof(GameLogger).GetMethod(method, [typeof(string), typeof(string)])!.Invoke(new GameLogger(recorder), ["MYTAG", "the text"]);
        AssertSingle(recorder, level, "MYTAG", "the text");

        var staticRecorder = new RecordingLogger();
        Logger.Configure(staticRecorder);
        try
        {
            typeof(Logger).GetMethod(method, [typeof(string), typeof(string)])!.Invoke(null, ["MYTAG", "the text"]);
            AssertSingle(staticRecorder, level, "MYTAG", "the text");
        }
        finally
        {
            Logger.Reset();
        }
    }

    private static void AssertSingle(RecordingLogger recorder, LogLevel level, string tag, string text)
    {
        RecordedEntry entry = Assert.Single(recorder.Entries);
        Assert.Equal(level, entry.Level);
        Assert.Equal($"[{tag}] {text}", entry.Message);
        Assert.Equal(tag, entry.Properties["Tag"]);
        Assert.Equal(text, entry.Properties["Text"]);
        Assert.Null(entry.Exception);
    }

    [Fact]
    public void Colour_arguments_are_accepted_and_ignored_as_in_Java()
    {
        var recorder = new RecordingLogger();
        var log = new GameLogger(recorder);
        log.Log(Logger.RED, "red text");
        log.Logln(Logger.GREEN, "green text");
        log.Log(Logger.YELLOW, Logger.BLUE_BG, "with background");

        Assert.Collection(
            recorder.Entries,
            e => Assert.Equal((I, "[LOG] red text"), (e.Level, e.Message)),
            e => Assert.Equal((I, "[LOG] green text"), (e.Level, e.Message)),
            e => Assert.Equal((I, "[LOG] with background"), (e.Level, e.Message)));
        Assert.Equal("\u001b[0;31m", Logger.RED);
        Assert.Equal("\u001b[0m", Logger.RESET);
    }

    [Theory]
    [InlineData(null, "LOG")]
    [InlineData("", "LOG")]
    [InlineData("   ", "LOG")]
    [InlineData("T", "T")]
    public void A_blank_tag_becomes_LOG_like_Java(string? tag, string expectedTag)
    {
        var recorder = new RecordingLogger();
        new GameLogger(recorder).Info(tag!, "x");
        Assert.Equal($"[{expectedTag}] x", Assert.Single(recorder.Entries).Message);
    }

    [Theory]
    [InlineData("line\n", "line")]
    [InlineData("line\r\n\r\n", "line")]
    [InlineData("a\nb\n", "a\nb")]
    [InlineData("", "")]
    [InlineData(null, "null")]
    public void A_trailing_line_break_is_removed_and_null_prints_as_null(string? text, string expected)
    {
        var recorder = new RecordingLogger();
        new GameLogger(recorder).Success(text!);
        Assert.Equal($"[OK] {expected}", Assert.Single(recorder.Entries).Message);
    }

    [Fact]
    public void Braces_in_the_text_are_not_treated_as_a_message_template()
    {
        var recorder = new RecordingLogger();
        new GameLogger(recorder).Error("failed {0} {Name} }{");
        Assert.Equal("[ERROR] failed {0} {Name} }{", Assert.Single(recorder.Entries).Message);
    }

    [Fact]
    public void An_entry_below_the_minimum_level_is_dropped_and_one_at_the_level_is_kept()
    {
        var recorder = new RecordingLogger(LogLevel.Warning);
        var log = new GameLogger(recorder);
        log.Log("info is below");
        log.Warning("warning is kept");
        log.Error("error is kept");
        Assert.Equal(new[] { "[WARN] warning is kept", "[ERROR] error is kept" }, recorder.Entries.Select(e => e.Message).ToArray());
    }

    [Fact]
    public void Line_writes_nothing_and_the_titles_write_one_entry()
    {
        var recorder = new RecordingLogger();
        var log = new GameLogger(recorder);
        log.Line();
        Assert.Empty(recorder.Entries);
        log.Section("Starting");
        Assert.Equal("[LOG] Starting", Assert.Single(recorder.Entries).Message);
    }

    [Fact]
    public void LogException_writes_the_exception_at_Error_level_with_the_class_and_the_calling_method()
    {
        var recorder = new RecordingLogger();
        var boom = new InvalidOperationException("boom");
        new GameLogger(recorder).LogException(typeof(LoggerTests), boom);

        RecordedEntry entry = Assert.Single(recorder.Entries);
        Assert.Equal(E, entry.Level);
        Assert.Same(boom, entry.Exception);
        Assert.Contains("[EXCEPTION]", entry.Message);
        Assert.Contains(typeof(LoggerTests).FullName!, entry.Message);
        Assert.Contains(nameof(LogException_writes_the_exception_at_Error_level_with_the_class_and_the_calling_method), entry.Message);
        Assert.Contains("boom", entry.Message);
    }

    [Fact]
    public void LogException_with_a_text_writes_a_warning_first_like_Java()
    {
        var recorder = new RecordingLogger();
        var boom = new IOException("disk");
        new GameLogger(recorder).LogException(typeof(LoggerTests), boom, "Lỗi drop nhẫn nhiệm vụ 31");

        Assert.Equal(2, recorder.Entries.Count);
        Assert.Equal(W, recorder.Entries[0].Level);
        Assert.Equal("[EXCEPTION] Lỗi drop nhẫn nhiệm vụ 31", recorder.Entries[0].Message);
        Assert.Null(recorder.Entries[0].Exception);
        Assert.Equal(E, recorder.Entries[1].Level);
        Assert.Same(boom, recorder.Entries[1].Exception);
    }

    [Fact]
    public void LogException_argument_checks()
    {
        var log = new GameLogger(new RecordingLogger());
        Assert.Throws<ArgumentNullException>(() => log.LogException(null!, new Exception("x")));
        Assert.Throws<ArgumentNullException>(() => log.LogException(typeof(LoggerTests), null!));
        Assert.Throws<ArgumentNullException>(() => new GameLogger(null!));
    }

    [Fact]
    public void Static_facade_reaches_the_configured_ILogger_for_LogException_and_logs_nothing_when_unconfigured()
    {
        Logger.Reset();
        Logger.Error("goes to the null logger"); // must not throw
        Logger.LogException(typeof(LoggerTests), new Exception("ignored"));

        var recorder = new RecordingLogger();
        Logger.Configure(recorder);
        try
        {
            var boom = new Exception("static boom");
            Logger.LogException(typeof(LoggerTests), boom, "context");
            Logger.Namek("kame");
            Assert.Equal(3, recorder.Entries.Count);
            Assert.Equal(new[] { W, E, I }, recorder.Entries.Select(e => e.Level).ToArray());
            Assert.Same(boom, recorder.Entries[1].Exception);
            Assert.Equal("[NAMEK] kame", recorder.Entries[2].Message);
        }
        finally
        {
            Logger.Reset();
        }
    }

    [Fact]
    public void Configure_with_a_factory_uses_the_Nro_category()
    {
        var factory = new CapturingLoggerFactory();
        Logger.Configure(factory);
        try
        {
            Logger.Success("via factory");
            Assert.Equal("Nro", factory.Category);
            Assert.Equal("[OK] via factory", Assert.Single(factory.Logger.Entries).Message);
        }
        finally
        {
            Logger.Reset();
        }
    }

    [Fact]
    public void ShortSession_is_null_text_or_the_stable_hex_identity_hash()
    {
        Assert.Equal("null", Logger.ShortSession(null));
        var session = new object();
        string id = Logger.ShortSession(session);
        Assert.Equal(RuntimeHelpers.GetHashCode(session).ToString("x"), id);
        Assert.Equal(id, GameLogger.ShortSession(session));
        Assert.NotEqual(id, Logger.ShortSession(new object()));
    }

    [Fact]
    public async Task Concurrent_logging_loses_no_entry()
    {
        var recorder = new RecordingLogger();
        var log = new GameLogger(recorder);
        await Task.WhenAll(Enumerable.Range(0, 8).Select(t => Task.Run(() =>
        {
            for (int i = 0; i < 500; i++)
            {
                log.Info("T" + t, "m" + i);
            }
        })));
        Assert.Equal(4000, recorder.Entries.Count);
    }

    private sealed class CapturingLoggerFactory : ILoggerFactory
    {
        public RecordingLogger Logger { get; } = new();

        public string? Category { get; private set; }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            Category = categoryName;
            return Logger;
        }

        public void Dispose()
        {
        }
    }
}
