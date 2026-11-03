namespace Nro.Core;

/// <summary>
/// The time zone the Java server runs in: <c>Asia/Ho_Chi_Minh</c> (the JVM default zone). Vietnam has had a fixed UTC+7 offset (no daylight saving)
/// since 1975-06-13, so the zone is modelled as a FIXED +07:00 offset instead of looked up by name. That keeps every date calculation independent of the
/// machine's time zone, of the installed tz database (a slim Docker image may have none) and of <c>InvariantGlobalization</c>.
/// DELIBERATE DIFFERENCE from Java: instants before 1975-06-13 (when Java would use +08:00 or +07:00 of the historical rules) are also shown at +07:00.
/// </summary>
public static class VietnamTime
{
    /// <summary>The fixed UTC offset: +07:00.</summary>
    public static readonly TimeSpan Offset = TimeSpan.FromHours(7);

    /// <summary>The offset in milliseconds.</summary>
    public const long OffsetMilliseconds = 7L * 60 * 60 * 1000;

    /// <summary>A <see cref="TimeZoneInfo"/> with the fixed +07:00 offset (id and names <c>Asia/Ho_Chi_Minh</c>).</summary>
    public static readonly TimeZoneInfo Zone = TimeZoneInfo.CreateCustomTimeZone("Asia/Ho_Chi_Minh", Offset, "Asia/Ho_Chi_Minh", "Indochina Time");
}
