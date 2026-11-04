namespace Nro.Game.Items.Inventory.Ports;

// Ghi nhận trong bộ nhớ việc người chơi đã nhận thưởng gift code; dịch vụ gift code quản lý số lượt còn lại và lịch sử dùng.
// Nguồn Java: nro/inventory/InventoryService.java (interface phụ của InventoryService; việc giảm countLeft và lưu lịch sử thuộc GiftCodeManager).
public interface IGiftCodeUsageTracker
{
    // Đánh dấu người chơi đã dùng mã sau khi vật phẩm thưởng đã được trao; kết quả GiftCode của Java không dùng.
    void MarkUsed(int playerId, string code);
}
