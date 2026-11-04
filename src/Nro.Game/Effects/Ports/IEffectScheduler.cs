namespace Nro.Game.Effects.Ports;

// Lập lịch chờ không chặn luồng cho các hiệu ứng biến hình; Java gọi Thread.sleep 0,2 đến 2 giây cho Cumber, Pain, Kefla, Đuôi Khỉ và khỉ Trung Thu trước khi bật.
// Nguồn Java: nro/effect/EffectSkillService.java (cổng C# giữ quyết định D1: tick không có một luồng cho mỗi người chơi; kiểm thử có thể cấp bộ lập lịch hoàn tất tức thời hoặc theo lệnh kiểm thử).
public interface IEffectScheduler
{
    // Hoàn tất sau thời gian chờ hoặc ném OperationCanceledException nếu mã hủy bị hủy trước.
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken);
}
