using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Items.Inventory;

// Triển khai thật của IInventoryItemFactory, chuyển các thao tác tạo/sao chép vật phẩm của kho đồ sang ItemService và đồng hồ được tiêm.
// Nguồn Java: nro/inventory/InventoryService.java (lớp phụ của InventoryService; mỗi lệnh trả đối tượng mới, không lưu đệm).
public sealed class InventoryItemFactory : IInventoryItemFactory
{
    private readonly ItemService itemService;
    private readonly VietnamClock clock;

    // Khởi tạo factory với catalog vật phẩm và nguồn thời gian bắt buộc.
    public InventoryItemFactory(ItemService itemService, VietnamClock clock)
    {
        ArgumentNullException.ThrowIfNull(itemService);
        ArgumentNullException.ThrowIfNull(clock);
        this.itemService = itemService;
        this.clock = clock;
    }

    // Tạo một ô trống mới qua ItemService.
    public Item CreateItemNull() => itemService.CreateItemNull();

    // Sao chép vật phẩm qua ItemService.
    public Item CopyItem(Item item) => itemService.CopyItem(item);

    // Tạo vật phẩm số lượng một qua ItemService.
    public Item CreateNewItem(short tempId) => itemService.CreateNewItem(tempId);

    // Tạo Item chỉ có template, số lượng 0, chưa có văn bản và lấy thời điểm hiện tại.
    public Item NewItem(short tempId) => new(itemService.Catalog, clock.CurrentTimeMillis(), tempId);

    // Tạo option theo template và tham số qua ItemService.
    public ItemOption CreateItemOption(int templateId, int param) => itemService.CreateOption(templateId, param);
}
