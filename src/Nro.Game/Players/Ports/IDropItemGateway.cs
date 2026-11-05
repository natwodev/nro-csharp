using Nro.Game.World.Maps;

namespace Nro.Game.Players.Ports;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/DropItem.java (dòng 23–35).
public interface IDropItemGateway
{
    // Đếm ô trống túi người chơi (Java: InventoryService.getCountEmptyBag).
    int GetEmptyBagCount(Player player);

    // Kiểm tra vật phẩm mẫu có trong túi (Java: InventoryService.isExistItemBag).
    bool HasBagItem(Player player, int itemTemplateId);

    // Kiểm tra vật phẩm rơi riêng của người chơi (Java: ItemMapService.findItemMapByPlayer).
    bool HasItemMapForPlayer(Player player, int itemTemplateId);

    // Tạo và gửi vật phẩm rơi chỉ cho người chơi; implementation tạo ItemMap thực (Java: Service.dropItemMapForMe).
    void DropItemMapForPlayer(Player player, Zone zone, int itemTemplateId, int quantity, int x, int y, IReadOnlyList<ItemMapOption> options);
}
