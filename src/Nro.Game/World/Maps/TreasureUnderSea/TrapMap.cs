namespace Nro.Game.World.Maps.TreasureUnderSea;

// Lưu hình chữ nhật bẫy của bản đồ, gồm bẫy gai tại bản đồ 135 mà Map tạo và Zone kiểm tra.
// Nguồn Java: nro/map/TreasureUnderSea/TrapMap.java (PARTIAL: doPlayer gây sát thương gai và gửi gói hiệu ứng thuộc track P-01, chưa chuyển ở đây).
public class TrapMap
{
    // Tọa độ X góc bẫy.
    public int X { get; set; }

    // Tọa độ Y góc bẫy.
    public int Y { get; set; }

    // Chiều rộng vùng bẫy.
    public int W { get; set; }

    // Chiều cao vùng bẫy.
    public int H { get; set; }

    // Id hiệu ứng của bẫy.
    public int EffectId { get; set; }

    // Sát thương của bẫy (Java: dame).
    public long Damage { get; set; }
}
