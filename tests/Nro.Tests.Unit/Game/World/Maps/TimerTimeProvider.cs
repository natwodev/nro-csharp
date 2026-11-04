namespace Nro.Tests.Unit.Game.World.Maps;

/// <summary>
/// A <see cref="TimeProvider"/> whose clock AND timers are driven by the test: <see cref="Advance"/> moves the clock and fires every timer that has become due. Used to
/// test <c>Task.Delay(delay, timeProvider, token)</c> based loops without waiting real time.
/// </summary>
internal sealed class TimerTimeProvider : TimeProvider
{
    private readonly object _gate = new();
    private readonly List<FakeTimer> _timers = [];
    private DateTimeOffset _now;

    public TimerTimeProvider(DateTimeOffset start)
    {
        _now = start;
    }

    public override DateTimeOffset GetUtcNow()
    {
        lock (_gate)
        {
            return _now;
        }
    }

    /// <summary>How long each armed timer still has to wait.</summary>
    public IReadOnlyList<TimeSpan> PendingDelays
    {
        get
        {
            lock (_gate)
            {
                return _timers.Where(t => t.Armed).Select(t => t.Due - _now).ToList();
            }
        }
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        FakeTimer timer = new(this, callback, state);
        timer.Change(dueTime, period);
        lock (_gate)
        {
            _timers.Add(timer);
        }

        return timer;
    }

    public void Advance(TimeSpan by)
    {
        List<FakeTimer> due;
        lock (_gate)
        {
            _now += by;
            due = _timers.Where(t => t.Armed && t.Due <= _now).ToList();
        }

        foreach (FakeTimer timer in due)
        {
            timer.Fire();
        }
    }

    private sealed class FakeTimer : ITimer
    {
        private readonly TimerTimeProvider _owner;
        private readonly TimerCallback _callback;
        private readonly object? _state;
        private TimeSpan _period;

        public FakeTimer(TimerTimeProvider owner, TimerCallback callback, object? state)
        {
            _owner = owner;
            _callback = callback;
            _state = state;
        }

        public bool Armed { get; private set; }

        public DateTimeOffset Due { get; private set; }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            lock (_owner._gate)
            {
                _period = period;
                Armed = dueTime != Timeout.InfiniteTimeSpan;
                Due = _owner._now + (Armed ? dueTime : TimeSpan.Zero);
            }

            return true;
        }

        public void Fire()
        {
            lock (_owner._gate)
            {
                if (_period == Timeout.InfiniteTimeSpan || _period == TimeSpan.Zero)
                {
                    Armed = false;
                }
                else
                {
                    Due = _owner._now + _period;
                }
            }

            _callback(_state);
        }

        public void Dispose()
        {
            lock (_owner._gate)
            {
                Armed = false;
            }
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
