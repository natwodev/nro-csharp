namespace Nro.Game.World.Mobs.Ports;

// Cổng đọc các cờ sự kiện và bảo trì toàn máy chủ mà mob dùng trong mỗi tick; phần cài đặt phải nhẹ.
// Nguồn Java: EventManager và Maintenance.isRunning (Java dùng boolean static công khai, còn host C# sở hữu giá trị).
public interface IGameFlags
{
    // Cho biết sự kiện Tết đang bật.
    bool LunarNewYear { get; }

    // Cho biết sự kiện Noel đang bật.
    bool Christmas { get; }

    // Cho biết sự kiện Vu Lan đang bật.
    bool VuLanFestival { get; }

    // Cho biết sự kiện Halloween đang bật.
    bool Halloween { get; }

    // Cho biết sự kiện 8/3 đang bật.
    bool InternationalWomansDay { get; }

    // Cho biết sự kiện Trung Thu đang bật (Java: trungThu).
    bool MoonFestival { get; }

    // Cho biết sự kiện Hùng Vương đang bật.
    bool HungVuong { get; }

    // Cho biết sự kiện Valentine đang bật.
    bool ValentineDay { get; }

    // Cho biết sự kiện 20/10 đang bật.
    bool Day2010 { get; }

    // Khi bảo trì đang chạy, mob không hồi sinh. (Java: Maintenance.isRunning)
    bool MaintenanceRunning { get; }
}
