namespace Nro.Network.Configurations;

/// <summary>Settings of the <see cref="Services.CommandDispatcher"/>.</summary>
public sealed class CommandDispatcherOptions
{
    /// <summary>Java <c>Controller.onMessage</c> warns when one command takes longer than 5000 ms.</summary>
    public TimeSpan SlowCommandThreshold { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Java logs the first 5 handler errors and then stays silent (a counter that never resets). Here every session has its own budget of
    /// this many logged errors, so one client sending malformed packets cannot flood the log or hide the errors of the others. Later errors are
    /// still counted in <see cref="Services.CommandStatistics.Failed"/>.
    /// </summary>
    public int MaxLoggedErrorsPerSession { get; init; } = 5;
}
