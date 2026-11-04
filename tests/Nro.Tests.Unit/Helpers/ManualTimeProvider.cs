namespace Nro.Tests.Unit.Helpers;

/// <summary>
/// A <see cref="TimeProvider"/> whose instant is set by the test. Its <see cref="LocalTimeZone"/> is deliberately NOT Vietnam (default UTC-8) so that any code
/// that wrongly used the provider's or the machine's local zone instead of Vietnam time would fail the clock tests.
/// </summary>
internal sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public ManualTimeProvider(DateTimeOffset utcNow, TimeZoneInfo? localZone = null)
    {
        _utcNow = utcNow;
        LocalTimeZone = localZone ?? TimeZoneInfo.CreateCustomTimeZone("Test/UTC-8", TimeSpan.FromHours(-8), "Test UTC-8", "Test UTC-8");
    }

    public override TimeZoneInfo LocalTimeZone { get; }

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void Set(DateTimeOffset utcNow) => _utcNow = utcNow;

    public void Advance(TimeSpan by) => _utcNow += by;

    /// <summary>A Vietnam wall-clock moment (UTC+7) built with the plain .NET API, independent of the code under test.</summary>
    public static DateTimeOffset Vn(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
        => new(year, month, day, hour, minute, second, millisecond, TimeSpan.FromHours(7));
}
