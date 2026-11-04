using Nro.Core.Templates;

namespace Nro.Game.Items.Inventory.Ports;

// Tách bốn thao tác ItemService mà InventoryService cần để có thể kiểm thử bằng đối tượng giả thay vì singleton Java đọc bảng tĩnh và đồng hồ hệ thống.
// Nguồn Java: nro/inventory/InventoryService.java (interface phụ của InventoryService; mỗi lệnh luôn tạo đối tượng mới vì kho đồ sửa vật phẩm tại chỗ, không dùng chung ô trống).
public interface IInventoryItemFactory
{
    // Tạo một ô trống mới có template null.
    Item CreateItemNull();

    // Sao chép template, văn bản, số lượng, thời điểm tạo và từng option; cố ý không sao chép quantityGD như Java.
    Item CopyItem(Item item);

    // Tạo vật phẩm mới số lượng 1, có content và info, chưa có option.
    Item CreateNewItem(short tempId);

    // Tạo Item chỉ có template, số lượng 0 và chưa có info/content.
    Item NewItem(short tempId);

    // Tạo option bằng cách tra template option theo mã.
    ItemOption CreateItemOption(int templateId, int param);
}
