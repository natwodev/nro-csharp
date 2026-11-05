using Nro.Game.Items.Core;
using Nro.Game.Items.Inventory;

namespace Nro.Tests.Unit.Game.Items.InventoryTests;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/Utils/ItemCheckUtil.java (dòng 1–71).
public sealed class ItemCheckUtilTests
{
    // Kiểm tra vật phẩm đủ/thiếu được tích lũy và dòng màu giữ nguyên Java (Java: check, isAllEnough).
    [Fact]
    public void Check_accumulates_item_requirements_and_colored_statuses()
    {
        var kit = new InvKit();
        kit.Inv.ItemsBag.Add(new Item(kit.Catalog, itemId: 10) { Quantity = 3 });
        var check = new ItemCheckUtil(kit.Player, kit.Service);

        string status = check.Check(10, 3, "Đá").Check(11, 1, "Ngọc").GetStatusText("Rèn", false, false, 0, 0);

        Assert.False(check.IsAllEnough());
        Assert.Equal("|1|Rèn\n|2|Đá 3/3\n|7|Ngọc 0/1\n", status);
    }

    // Kiểm tra vàng và hồng ngọc đủ/thiếu đổi màu, định dạng và cờ Java (Java: getStatusText).
    [Fact]
    public void Status_text_checks_gold_and_ruby_costs()
    {
        var kit = new InvKit();
        kit.Inv.Gold = 1_000;
        kit.Inv.Ruby = 2;
        var check = new ItemCheckUtil(kit.Player, kit.Service);

        string status = check.GetStatusText("Mua", true, true, 1_001, 2);

        Assert.False(check.IsAllEnough());
        Assert.Equal("|1|Mua\n|7|Giá Vàng: 1.001\n|2|Giá Hồng Ngọc: 2\n", status);
    }
}
