using Nro.Core.Templates;
using Nro.Game.Items.Core;

namespace Nro.Game.Players;

// Ngoại hình tối thiểu: mục tiêu pet, hiệu ứng trước mặt và trang phục pet.
// Nguồn Java: nro/player/Player.java (dòng 1460–1626).
public partial class Player
{
    // Kiểm tra mục tiêu được pet nhận diện (Java: isTargerDe).
    public bool IsTargetForPet(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.IsPetFollow || player.IsTangMonk || player.IsBoss || player.IsPl();
    }

    // Lấy hiệu ứng trước mặt theo mức option 72 thấp nhất của năm món trang bị (Java: getEffFront).
    public sbyte GetFrontEffect()
    {
        if (Inventory?.ItemsBody is not { Count: >= 20 } items)
        {
            return -1;
        }

        int? shirtLevel = GetUpgradeLevel(items[0]);
        int? pantsLevel = GetUpgradeLevel(items[1]);
        int? glovesLevel = GetUpgradeLevel(items[2]);
        int? bootsLevel = GetUpgradeLevel(items[3]);
        int? ringLevel = GetUpgradeLevel(items[4]);
        if (shirtLevel is null || pantsLevel is null || glovesLevel is null || bootsLevel is null || ringLevel is null)
        {
            return -1;
        }

        int minimumLevel = Math.Min(shirtLevel.Value, Math.Min(pantsLevel.Value, Math.Min(glovesLevel.Value, Math.Min(bootsLevel.Value, ringLevel.Value))));
        return minimumLevel switch
        {
            >= 8 => 8,
            >= 7 => 7,
            >= 6 => 6,
            >= 5 => 5,
            >= 4 => 4,
            _ => -1,
        };
    }

    // Lấy mức option 72 đầu tiên của một món trang bị; ô null hoặc không có option không tạo hiệu ứng (Java: vòng lặp itemOptions trong getEffFront).
    private static int? GetUpgradeLevel(Item? item)
    {
        if (item is null)
        {
            return null;
        }

        foreach (ItemOption? option in item.ItemOptions)
        {
            if (option?.OptionTemplate?.Id == 72)
            {
                return option.Param;
            }
        }

        return null;
    }

    // Lấy đầu trang phục pet ở ô body 7 (Java: getHeadThuCung).
    public short GetPetHead() => GetPetOutfitPart(static template => template.Head);

    // Lấy thân trang phục pet ở ô body 7 (Java: getBodyThuCung).
    public short GetPetBody() => GetPetOutfitPart(static template => template.Body);

    // Lấy chân trang phục pet ở ô body 7 (Java: getLegThuCung).
    public short GetPetLeg() => GetPetOutfitPart(static template => template.Leg);

    // Lấy một phần trang phục pet; trả -1 khi không phải người chơi hoặc ô trang bị không hợp lệ (Java: getHeadThuCung, getBodyThuCung, getLegThuCung).
    private short GetPetOutfitPart(Func<ItemTemplate, int> getPart)
    {
        if (!IsPl() || Inventory?.ItemsBody is not { Count: > 7 } items || items[7] is not { Template: { } template })
        {
            return -1;
        }

        return unchecked((short)getPart(template));
    }
}
