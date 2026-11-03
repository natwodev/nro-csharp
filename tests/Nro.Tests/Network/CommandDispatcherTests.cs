using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Nro.Core.IO;
using Nro.Network.Configurations;
using Nro.Network.Consts;
using Nro.Network.Interfaces;
using Nro.Network.Services;

namespace Nro.Tests.Network;

/// <summary>Command routing: handlers, unknown and not-yet-implemented commands, the character gate, error isolation, sub-commands, and the rule table vs the specification.</summary>
public class CommandDispatcherTests
{
    private sealed class FakeSession(long id = 1, string ip = "10.0.0.1") : ISession
    {
        public long Id { get; } = id;

        public string IpAddress { get; } = ip;

        public bool IsConnected => true;

        public bool Send(Message message) => true;

        public void Disconnect()
        {
        }
    }

    private sealed class Gate(bool hasCharacter) : ICharacterGate
    {
        public bool Has { get; set; } = hasCharacter;

        public bool HasCharacter(ISession session) => Has;
    }

    private sealed class CapturingLogger : ILogger
    {
        private readonly List<(LogLevel Level, string Message)> _entries = [];

        public IReadOnlyList<(LogLevel Level, string Message)> Entries
        {
            get
            {
                lock (_entries)
                {
                    return [.. _entries];
                }
            }
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            lock (_entries)
            {
                _entries.Add((logLevel, formatter(state, exception)));
            }
        }
    }

    private static Message Packet(sbyte command, params byte[] payload) => new(command, payload);

    private static CommandDispatcher NewDispatcher(bool hasCharacter, out Gate gate, CommandDispatcherOptions? options = null, ILogger? logger = null)
    {
        gate = new Gate(hasCharacter);
        return new CommandDispatcher(gate, options, logger);
    }

    private static sbyte AValueThatIsNotACommand()
    {
        for (int v = sbyte.MinValue; v <= sbyte.MaxValue; v++)
        {
            if (!CommandRules.KnownCommands.Contains((sbyte)v))
            {
                return (sbyte)v;
            }
        }

        throw new InvalidOperationException("Every sbyte value is a command");
    }

    [Fact]
    public async Task A_registered_command_reaches_its_handler_with_the_payload_intact()
    {
        CommandDispatcher dispatcher = NewDispatcher(hasCharacter: true, out _);
        byte[]? seen = null;
        dispatcher.Register(5, (session, message, ct) =>
        {
            seen = new byte[message.Reader().Available];
            message.Reader().ReadFully(seen);
            return ValueTask.CompletedTask;
        });

        await dispatcher.OnMessageAsync(new FakeSession(), Packet(5, 1, 2, 3), CancellationToken.None);

        Assert.Equal(new byte[] { 1, 2, 3 }, seen);
        Assert.Equal(1, dispatcher.Statistics.Handled);
    }

    [Fact]
    public async Task A_command_without_a_handler_is_dropped_quietly_when_it_exists_in_the_protocol_and_with_a_warning_when_it_does_not()
    {
        var logger = new CapturingLogger();
        CommandDispatcher dispatcher = NewDispatcher(hasCharacter: true, out _, logger: logger);
        var session = new FakeSession();

        await dispatcher.OnMessageAsync(session, Packet(Cmd_message.LOGIN), CancellationToken.None);
        sbyte alien = AValueThatIsNotACommand();
        await dispatcher.OnMessageAsync(session, Packet(alien, 9), CancellationToken.None);

        Assert.Equal(1, dispatcher.Statistics.NotImplemented);
        Assert.Equal(1, dispatcher.Statistics.Unknown);
        Assert.Single(logger.Entries, e => e.Level == LogLevel.Warning && e.Message.Contains(alien.ToString(), StringComparison.Ordinal));
        Assert.DoesNotContain(logger.Entries, e => e.Level >= LogLevel.Warning && e.Message.Contains("has no handler", StringComparison.Ordinal));
    }

    [Fact]
    public async Task A_command_that_needs_a_character_is_dropped_without_one_and_handled_with_one()
    {
        CommandDispatcher dispatcher = NewDispatcher(hasCharacter: false, out Gate gate);
        int calls = 0;
        dispatcher.Register(5, (s, m, ct) =>
        {
            calls++;
            return ValueTask.CompletedTask;
        });

        await dispatcher.OnMessageAsync(new FakeSession(), Packet(5), CancellationToken.None);
        Assert.Equal(0, calls);
        Assert.Equal(1, dispatcher.Statistics.DroppedNoCharacter);

        gate.Has = true;
        await dispatcher.OnMessageAsync(new FakeSession(), Packet(5), CancellationToken.None);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task The_login_commands_of_the_specification_are_handled_before_a_character_exists()
    {
        CommandDispatcher dispatcher = NewDispatcher(hasCharacter: false, out _);
        var reached = new List<sbyte>();
        foreach (sbyte cmd in CommandRules.NoCharacterRequired)
        {
            dispatcher.Register(cmd, (s, m, ct) =>
            {
                reached.Add(m.Command);
                return ValueTask.CompletedTask;
            });
        }

        foreach (sbyte cmd in CommandRules.NoCharacterRequired)
        {
            await dispatcher.OnMessageAsync(new FakeSession(), Packet(cmd), CancellationToken.None);
        }

        Assert.True(CommandRules.NoCharacterRequired.SetEquals(reached));
        Assert.Equal(0, dispatcher.Statistics.DroppedNoCharacter);
    }

    [Fact]
    public async Task The_character_requirement_can_be_chosen_at_registration()
    {
        CommandDispatcher dispatcher = NewDispatcher(hasCharacter: false, out _);
        int calls = 0;
        dispatcher.Register(5, (s, m, ct) =>
        {
            calls++;
            return ValueTask.CompletedTask;
        }, requiresCharacter: false);

        await dispatcher.OnMessageAsync(new FakeSession(), Packet(5), CancellationToken.None);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task A_handler_that_throws_costs_one_packet_and_the_next_one_is_handled_normally()
    {
        CommandDispatcher dispatcher = NewDispatcher(hasCharacter: true, out _);
        int good = 0;
        dispatcher.Register(5, (s, m, ct) => throw new InvalidOperationException("boom"));
        dispatcher.Register(6, (s, m, ct) =>
        {
            good++;
            return ValueTask.CompletedTask;
        });

        var session = new FakeSession();
        await dispatcher.OnMessageAsync(session, Packet(5), CancellationToken.None);
        await dispatcher.OnMessageAsync(session, Packet(6), CancellationToken.None);

        Assert.Equal(1, good);
        Assert.Equal(1, dispatcher.Statistics.Failed);
        Assert.Equal(1, dispatcher.Statistics.Handled);
    }

    [Fact]
    public async Task A_truncated_payload_is_just_another_handler_error()
    {
        CommandDispatcher dispatcher = NewDispatcher(hasCharacter: true, out _);
        dispatcher.Register(5, (s, m, ct) =>
        {
            m.Reader().ReadInt();
            return ValueTask.CompletedTask;
        });

        await dispatcher.OnMessageAsync(new FakeSession(), Packet(5, 1), CancellationToken.None);

        Assert.Equal(1, dispatcher.Statistics.Failed);
    }

    [Fact]
    public async Task Each_session_logs_at_most_five_errors_but_every_error_is_counted()
    {
        var logger = new CapturingLogger();
        CommandDispatcher dispatcher = NewDispatcher(hasCharacter: true, out _, logger: logger);
        dispatcher.Register(5, (s, m, ct) => throw new InvalidOperationException("boom"));

        var noisy = new FakeSession(1);
        for (int i = 0; i < 40; i++)
        {
            await dispatcher.OnMessageAsync(noisy, Packet(5), CancellationToken.None);
        }

        Assert.Equal(5, logger.Entries.Count(e => e.Level == LogLevel.Error));

        var other = new FakeSession(2);
        await dispatcher.OnMessageAsync(other, Packet(5), CancellationToken.None);

        Assert.Equal(6, logger.Entries.Count(e => e.Level == LogLevel.Error));
        Assert.Equal(41, dispatcher.Statistics.Failed);
    }

    [Fact]
    public async Task Cancellation_is_not_an_error_it_propagates()
    {
        CommandDispatcher dispatcher = NewDispatcher(hasCharacter: true, out _);
        dispatcher.Register(5, async (s, m, ct) => await Task.Delay(Timeout.Infinite, ct));
        using var cts = new CancellationTokenSource();

        Task run = dispatcher.OnMessageAsync(new FakeSession(), Packet(5), cts.Token).AsTask();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => run);
        Assert.Equal(0, dispatcher.Statistics.Failed);
    }

    [Fact]
    public async Task A_slow_command_is_logged_as_a_warning()
    {
        var logger = new CapturingLogger();
        CommandDispatcher dispatcher = NewDispatcher(
            hasCharacter: true, out _, new CommandDispatcherOptions { SlowCommandThreshold = TimeSpan.FromMilliseconds(30) }, logger);
        dispatcher.Register(5, async (s, m, ct) => await Task.Delay(120, ct));

        await dispatcher.OnMessageAsync(new FakeSession(), Packet(5), CancellationToken.None);

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning && e.Message.Contains("took", StringComparison.Ordinal));
    }

    [Fact]
    public void A_command_can_have_only_one_handler()
    {
        CommandDispatcher dispatcher = NewDispatcher(hasCharacter: true, out _);
        dispatcher.Register(5, (s, m, ct) => ValueTask.CompletedTask);
        Assert.Throws<InvalidOperationException>(() => dispatcher.Register(5, (s, m, ct) => ValueTask.CompletedTask));
    }

    // ---- sub-commands

    [Fact]
    public async Task A_sub_command_handler_reads_the_first_byte_and_routes_the_rest()
    {
        var sub = new SubCommandHandler();
        var seen = new List<(sbyte Sub, int Value)>();
        sub.Register(0, (s, m, ct) =>
        {
            seen.Add((0, m.Reader().ReadUnsignedByte()));
            return ValueTask.CompletedTask;
        });
        sub.Register(-1, (s, m, ct) =>
        {
            seen.Add((-1, m.Reader().ReadShort()));
            return ValueTask.CompletedTask;
        });
        CommandDispatcher dispatcher = NewDispatcher(hasCharacter: true, out _);
        dispatcher.Register(-29, sub);

        var session = new FakeSession();
        await dispatcher.OnMessageAsync(session, Packet(-29, 0, 42), CancellationToken.None);
        await dispatcher.OnMessageAsync(session, Packet(-29, 0xFF, 1, 2), CancellationToken.None);

        Assert.Equal([((sbyte)0, 42), ((sbyte)-1, 258)], seen);
    }

    [Fact]
    public async Task An_empty_sub_command_payload_or_an_unregistered_sub_command_is_dropped_without_an_error()
    {
        var sub = new SubCommandHandler();
        sub.Register(1, (s, m, ct) => ValueTask.CompletedTask);
        CommandDispatcher dispatcher = NewDispatcher(hasCharacter: true, out _);
        dispatcher.Register(-29, sub);

        var session = new FakeSession();
        await dispatcher.OnMessageAsync(session, Packet(-29), CancellationToken.None);
        await dispatcher.OnMessageAsync(session, Packet(-29, 77), CancellationToken.None);

        Assert.Equal(0, dispatcher.Statistics.Failed);
        Assert.Equal(2, dispatcher.Statistics.Handled);
    }

    [Fact]
    public void A_sub_command_can_have_only_one_handler()
    {
        var sub = new SubCommandHandler();
        sub.Register(1, (s, m, ct) => ValueTask.CompletedTask);
        Assert.Throws<InvalidOperationException>(() => sub.Register(1, (s, m, ct) => ValueTask.CompletedTask));
    }

    // ---- the rule tables against the specification

    [Fact]
    public void The_no_character_table_matches_section_3_4_of_the_specification()
    {
        string doc = File.ReadAllText(Path.Combine(TestPaths.RepoRoot(), "docs", "PROTOCOL.md"));
        int start = doc.IndexOf("### 3.4", StringComparison.Ordinal);
        Assert.True(start >= 0, "Section 3.4 not found in docs/PROTOCOL.md");
        string paragraph = doc[start..].Split('\n').First(l => l.StartsWith("In `nro/server/Controller.onMessage`", StringComparison.Ordinal));
        paragraph = paragraph[..paragraph.IndexOf("Specific notes", StringComparison.Ordinal)];

        var fromDoc = Regex.Matches(paragraph, "`(-?[0-9]+)`")
            .Select(m => sbyte.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture))
            .ToHashSet();

        Assert.True(fromDoc.SetEquals(CommandRules.NoCharacterRequired), "CommandRules.NoCharacterRequired differs from section 3.4 of the specification");
    }

    [Fact]
    public void Every_no_character_command_is_a_command_of_the_protocol_and_the_known_set_is_the_named_constants()
    {
        foreach (sbyte cmd in CommandRules.NoCharacterRequired)
        {
            Assert.Contains(cmd, CommandRules.KnownCommands);
        }

        Assert.Equal(234, CommandRules.KnownCommands.Count);
    }

    // ---- end to end through a real connection

    [Fact]
    public async Task Unknown_and_unimplemented_commands_never_close_a_real_connection()
    {
        var gate = new Gate(hasCharacter: true);
        var dispatcher = new CommandDispatcher(gate);
        var received = new List<sbyte>();
        dispatcher.Register(7, (s, m, ct) =>
        {
            lock (received)
            {
                received.Add(m.Command);
            }

            return ValueTask.CompletedTask;
        });

        await using var server = new GameServer(new GameServerOptions { Port = 0, BindAddress = IPAddress.Loopback }, dispatcher);
        server.Start();
        using var tcp = new TcpClient();
        await tcp.ConnectAsync(IPAddress.Loopback, server.Port);
        using var client = new TestClient(tcp);
        await client.HandshakeAsync();

        await client.SendAsync(AValueThatIsNotACommand(), [1, 2, 3]);
        await client.SendAsync(Cmd_message.LOGIN, [9]);
        await client.SendAsync(7, [1]);

        DateTime end = DateTime.UtcNow + TestTimeouts.Default;
        while (true)
        {
            lock (received)
            {
                if (received.Count == 1)
                {
                    break;
                }
            }

            Assert.True(DateTime.UtcNow < end, "The handled command never arrived");
            await Task.Delay(5);
        }

        Assert.Equal(1, server.Sessions.Count);
        Assert.Equal(1, dispatcher.Statistics.Unknown);
        Assert.Equal(1, dispatcher.Statistics.NotImplemented);
    }
}
