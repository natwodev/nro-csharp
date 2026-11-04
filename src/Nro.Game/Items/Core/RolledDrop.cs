namespace Nro.Game.Items.Core;

// Chứa kết quả quay rơi đồ gồm id mẫu và danh sách tùy chọn theo thứ tự Java.
// Nguồn Java: ItemService.randDoTL, randDoTLBoss (ItemMap thuộc phần bản đồ nên nơi gọi dựng ItemMap từ giá trị này; mỗi lượt quay tạo danh sách mới).
public sealed record RolledDrop(short TemplateId, List<ItemOption> Options);
