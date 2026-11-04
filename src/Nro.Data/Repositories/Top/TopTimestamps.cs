using Nro.Data.Db;
namespace Nro.Data.Repositories.Top;

// Chuyển thời điểm đăng nhập sang mili giây epoch theo quy tắc NULL thành 0 của TopServer.
// Nguồn Java: nro/server/TopServer.java (tương đương Timestamp.getTime; thời điểm được coi là UTC).
internal static class TopTimestamps
{
    // Trả mili giây epoch UTC, hoặc 0 khi thời điểm NULL (Java: Timestamp.getTime).
    public static long ToMillis(DateTime? time) =>
        time is { } value ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)).ToUnixTimeMilliseconds() : 0;
}
