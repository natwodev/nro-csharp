namespace Nro.Game.Effects.Ports;

// Cung cấp dữ liệu túi đồ và dịch vụ vật phẩm mà hiệu ứng cần khi lớp Inventory còn PLACEHOLDER của track D/E.
// Nguồn Java: services/InventoryService.java và ItemService.java (cổng C#; các phương thức danh sách phải trả bản sao để bên gọi giữ khi trò chơi thay đổi túi đồ).
public interface IEffectInventoryAccess
{
    // Tìm vật phẩm hiệu ứng băng Drabura trong túi người chơi (Java: InventoryService.findItemSkinDraburaFrost).
    bool HasDraburaFrostCostume(Player player);

    // Kiểm tra người chơi có Bông Tuyết (Java: InventoryService.findBongTuyet).
    bool HasSnowflake(Player player);

    // Kiểm tra avatar khẩu trang trong túi đồ (Java: InventoryService.findAvatarKhauTrang).
    bool HasFaceMaskAvatar(Player player);

    // Trả vật phẩm ô trang bị, không bao giờ null; ô trống trả IsNotNullItem false (Java: player.inventory.itemsBody.get).
    IEffectItem GetBodyItem(Player player, int index);

    // Trả giáp luyện tập, null nếu không có (Java: player.inventory.trainArmor).
    IEffectItem? GetTrainArmor(Player player);

    // Trả bản sao danh sách vật phẩm túi; từng phần tử có thể null (Java: player.inventory.itemsBag).
    IReadOnlyList<IEffectItem?> GetBagItems(Player player);

    // Trả bản sao danh sách vật phẩm rương; từng phần tử có thể null (Java: player.inventory.itemsBox).
    IReadOnlyList<IEffectItem?> GetBoxItems(Player player);

    // Kiểm tra vật phẩm là giáp luyện tập (Java: ItemService.isTrainArmor).
    bool IsTrainArmor(IEffectItem item);
}
