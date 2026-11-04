namespace Nro.Core.Enums;

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
