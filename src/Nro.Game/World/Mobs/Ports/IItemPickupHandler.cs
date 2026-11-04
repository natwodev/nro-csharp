namespace Nro.Game.World.Mobs.Ports;

// Kết quả xử lý nhặt vật phẩm trên mặt đất.
// Nguồn Java: Zone.pickItem (phần lớp chính chuyển qua IItemPickupHandler).
public enum ItemPickupResult
{
    // Vật phẩm đã được đưa vào túi.
    Picked,

    // Đã báo điều kiện không đạt; dừng, không kiểm tra nhiệm vụ.
    Refused,

    // Túi đồ đã đầy.
    BagFull,
}

// Tách phần Zone.pickItem tạo Item, thêm vào túi, gửi gói -20 và kiểm tra nhiệm vụ sau nhặt thành cổng tồn kho.
// Nguồn Java: nro/map/Zone.java, Zone.pickItem (zone vẫn quyết định quyền sở hữu và việc xóa vật phẩm mặt đất; handler sở hữu tồn kho).
public interface IItemPickupHandler
{
    // Thử đưa vật phẩm mặt đất vào túi người chơi.
    ItemPickupResult TryPick(Player player, ItemMap itemMap, int itemMapId);

    // Chạy các kiểm tra nhiệm vụ Java sau khi nhặt thành công, gồm checkDoneTaskPickItem và bốn kiểm tra khác.
    void OnPicked(Player player, ItemMap itemMap);
}
