using Nro.Core.Util;
using Nro.Game.Items.Core;
using Nro.Game.Players;

namespace Nro.Game.Items.Inventory;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/Utils/ItemCheckUtil.java (dòng 1–71).
public sealed class ItemCheckUtil
{
    // Người chơi sở hữu túi và tiền cần kiểm tra (Java: player).
    private readonly Player player;

    // Cổng đọc vật phẩm túi đã có của Inventory (Java: InventoryService.gI()).
    private readonly IInventoryService inventoryService;

    // Các dòng điều kiện đã kiểm tra (Java: statusLines).
    private readonly List<string> statusLines = [];

    // Cờ tích lũy tất cả điều kiện đủ (Java: allEnough).
    private bool allEnough = true;

    // Tạo bộ kiểm tra với người chơi và cổng inventory (Java: ItemCheckUtil).
    public ItemCheckUtil(Player player, IInventoryService inventoryService)
    {
        this.player = player ?? throw new ArgumentNullException(nameof(player));
        this.inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
    }

    // Kiểm tra số lượng vật phẩm trong túi và ghi dòng trạng thái màu Java (Java: check).
    public ItemCheckUtil Check(int itemId, int requiredQuantity, string itemName)
    {
        ArgumentNullException.ThrowIfNull(itemName);
        Item? item = inventoryService.FindItemBag(player, itemId);
        int have = item?.Quantity ?? 0;
        bool enough = have >= requiredQuantity;
        allEnough &= enough;
        statusLines.Add((enough ? "|2|" : "|7|") + itemName + " " + have + "/" + requiredQuantity);
        return this;
    }

    // Trả cờ tất cả điều kiện đã kiểm tra có đủ (Java: isAllEnough).
    public bool IsAllEnough() => allEnough;

    // Ghép tiêu đề, điều kiện vật phẩm và giá vàng/hồng ngọc, đồng thời tích lũy cờ đủ (Java: getStatusText).
    public string GetStatusText(string title, bool costGold, bool costRuby, long goldCost, int rubyCost)
    {
        ArgumentNullException.ThrowIfNull(title);
        var output = new System.Text.StringBuilder();
        output.Append("|1|").Append(title).Append('\n');
        foreach (string line in statusLines)
        {
            output.Append(line).Append('\n');
        }

        if (costGold)
        {
            bool enoughGold = player.Inventory.GetGold() >= goldCost;
            output.Append(enoughGold ? "|2|" : "|7|").Append("Giá Vàng: ").Append(Util.Format(goldCost)).Append('\n');
            allEnough &= enoughGold;
        }

        if (costRuby)
        {
            bool enoughRuby = player.Inventory.GetRuby() >= rubyCost;
            output.Append(enoughRuby ? "|2|" : "|7|").Append("Giá Hồng Ngọc: ").Append(Util.Format(rubyCost)).Append('\n');
            allEnough &= enoughRuby;
        }

        return output.ToString();
    }
}
