namespace Nro.Game.Effects.Ports;

// Cung cấp các quy tắc thuần về bản đồ và thời lượng kỹ năng để hiệu ứng không phụ thuộc dữ liệu chúng không sở hữu và kiểm thử có thể chọn kết quả.
// Nguồn Java: services/MapService.java và utils/SkillUtil.java (cổng C# cho danh sách bản đồ đã nạp và bảng kỹ năng).
public interface IEffectRules
{
    // Kiểm tra bản đồ ngoại tuyến (Java: MapService.isMapOffline).
    bool IsMapOffline(int mapId);

    // Kiểm tra bản đồ chiến tranh ngọc rồng, id 85 đến 91 (Java: MapService.isMapBlackBallWar).
    bool IsMapBlackBallWar(int mapId);

    // Kiểm tra bản đồ Mabu 12 giờ, id 114 đến 120 (Java: MapService.isMapMaBu12H).
    bool IsMapMaBu12H(int mapId);

    // Trả thời lượng biến khỉ: (cấp + 5) * 10000 mili giây (Java: SkillUtil.getTimeMonkey).
    int GetTimeMonkey(int level);

    // Trả thời lượng khiên: (cấp + 2) * 5000 mili giây (Java: SkillUtil.getTimeShield).
    int GetTimeShield(int level);

    // Trả thời lượng biến hình 300000 mili giây, giữ nguyên lỗi chính tả tên Java (Java: SkillUtil.getTimeTranformation).
    int GetTimeTranformation();

    // Trả thời lượng tắt thở 300000 mili giây (Java: SkillUtil.getTimett).
    int GetTimett();

    // Trả thời lượng sức mạnh bộc phá: (cấp + 5) * 5000 mili giây (Java: SkillUtil.getTimeSucManhBocPha).
    int GetTimeSucManhBocPha(int level);
}
