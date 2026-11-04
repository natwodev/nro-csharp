using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.World.Mobs;

// Mô tả mảnh đồ Hủy Diệt gồm mã template và các tùy chọn kèm theo.
// Nguồn Java: nro/mob/MobDrop.java (lớp phụ của MobDrop, thay Item thật thuộc mô hình vật phẩm).
public sealed record DhdItem(int TemplateId, List<ItemMapOption> Options);

// Chứa bảng vật phẩm rơi đặc biệt và các hàm thả thưởng theo vị trí.
// Nguồn Java: nro/mob/MobDrop.java (các hàm static nhận MapRuntime thay Service.gI(), ItemService.gI() và new Random(); đồ Hủy Diệt trả DhdItem vì Item thật thuộc mô hình vật phẩm, tùy chọn có sẵn do IMobLootHost.CreateSetKichHoatOptions cung cấp).
public static class MobDrop
{
    // Các template thức ăn có thể rơi.
    public static readonly int[] FoodItemTemplateIds = [663, 664, 665, 666, 667];

    // Các template đá có thể rơi.
    public static readonly int[] StoneItemTemplateIds = [220, 221, 222, 223, 224];

    // Bảng list_do_huy_diet: hàng là hệ trái đất, namek, saiyan; cột là năm món trang bị.
    public static readonly int[][] DestructionSetTemplateIds =
    [
        [650, 651, 657, 658, 656],
        [652, 653, 659, 660, 656],
        [654, 655, 661, 662, 656],
    ];

    private static readonly int[] Shirts = [650, 652, 654];
    private static readonly int[] Pants = [651, 653, 655];
    private static readonly int[] Gloves = [657, 659, 661];
    private static readonly int[] Boots = [658, 660, 662];

    // Thả số lượng vật phẩm gần (x, y) với tùy chọn 30 không giao dịch. (Java: DropItemReWard)
    public static void DropRewardItem(MapRuntime rt, Player player, int itemId, int quantity, int x, int y)
    {
        Zone zone = player.Zone!;
        ItemMap item = new(zone, itemId, quantity, rt.NextInt(x - 50, x + 50), y, player.Id);
        item.Options.Add(new ItemMapOption(30, 0));
        rt.World.DropItemMap(zone, item);
    }

    // Thả ngẫu nhiên một món Hủy Diệt kích hoạt hợp giới tính người chơi cùng các tùy chọn đã quay. (Java: DropItemReWardDoHuyDietKichHoat)
    public static void DropActivatedDestructionSetReward(MapRuntime rt, Player player, int quantity, int x, int y)
    {
        DhdItem destructionSetItem = CreateRandomDestructionSetItem(rt, DestructionSetTemplateIds[player.Gender][rt.NextInt(0, 4)], player.Gender);
        Zone zone = player.Zone!;
        ItemMap item = new(zone, destructionSetItem.TemplateId, quantity, rt.NextInt(x - 50, x + 50), y, player.Id)
        {
            Options = destructionSetItem.Options,
        };
        rt.World.DropItemMap(zone, item);
    }

    // Kiểm tra template 663--667 không được giao dịch. (Java: IsItemKhongChoGiaoDich)
    public static bool IsItemUntradeable(int id) => id >= 663 && id <= 667;

    // Quay chỉ số món Hủy Diệt: áo 1800--2800, quần HP 85--100, găng 8500--10000, giày 80--90, nhẫn chí mạng 17--19; thêm tùy chọn cần 80 tỷ sức mạnh (21) và không giao dịch (30), chỉ số tăng 10% khi hợp giới tính. (Java: randomCS_DHD)
    public static DhdItem CreateRandomDestructionSetItem(MapRuntime rt, int itemId, int gender)
    {
        List<ItemMapOption> options = rt.Loot.CreateSetKichHoatOptions(itemId);
        const int destructionRingId = 656;
        if (Array.IndexOf(Shirts, itemId) >= 0)
        {
            options.Add(new ItemMapOption(47, Util.HighlightsItem(gender == 2, rt.Random.NextInt(1001) + 1800)));
        }

        if (Array.IndexOf(Pants, itemId) >= 0)
        {
            options.Add(new ItemMapOption(22, Util.HighlightsItem(gender == 0, rt.Random.NextInt(16) + 85)));
        }

        if (Array.IndexOf(Gloves, itemId) >= 0)
        {
            options.Add(new ItemMapOption(0, Util.HighlightsItem(gender == 2, rt.Random.NextInt(150) + 8500)));
        }

        if (Array.IndexOf(Boots, itemId) >= 0)
        {
            options.Add(new ItemMapOption(23, Util.HighlightsItem(gender == 1, rt.Random.NextInt(11) + 80)));
        }

        if (destructionRingId == itemId)
        {
            options.Add(new ItemMapOption(14, rt.Random.NextInt(3) + 17));
        }

        options.Add(new ItemMapOption(21, 80));
        options.Add(new ItemMapOption(30, 1));
        return new DhdItem(itemId, options);
    }
}
