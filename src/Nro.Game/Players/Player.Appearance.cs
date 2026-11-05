using Nro.Core.Consts;
using Nro.Core.Templates;
using Nro.Game.Items.Core;
using Nro.Game.Players.Ports;

namespace Nro.Game.Players;

// Ngoại hình tối thiểu: mục tiêu pet, hiệu ứng trước mặt và trang phục pet.
// Nguồn Java: nro/player/Player.java (dòng 1460–2085; H-01.2: 1460–1626, H-01.3: 1627–2085).
public partial class Player
{
    // Loại hợp thể dùng riêng cho lựa chọn ngoại hình cho đến khi Relations/Fusion cung cấp trạng thái đó (Java: fusion.typeFusion).
    public sbyte AppearanceFusionType { get; set; }

    // Đệ tử được đọc để phối bộ cải trang hợp thể; null khi người chơi không có đệ tử (Java: Detu).
    public Player? AppearancePet { get; set; }

    // Cờ người chơi đang chịu phù hộ bản đồ Mabu để chọn thân/chân thần (Java: isPhuHoMapMabu).
    public bool IsMabuMapProtection { get; set; }

    // Cờ vật phẩm Đuôi Khỉ còn hiệu lực, do adapter ItemTime cấp vì ItemTime chưa chứa trạng thái này (Java: itemTime.IsDuoiKhi).
    public bool IsMonkeyTailActive { get; set; }

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

    // Lấy đầu hiển thị; thứ tự hiệu ứng, hợp thể, cải trang và trang bị giữ nguyên Java (Java: getHead).
    public short GetHead(IPlayerRandom? random = null)
    {
        if (EffectSkill is { IsBottled: true })
        {
            return idOutfitMafuba[EffectSkill.BottledType][0];
        }

        if (EffectSkill is { IsStone: true })
        {
            return 454;
        }

        if (EffectSkill is { IsHalloween: true })
        {
            return idOutfitHalloween[EffectSkill.HalloweenOutfitId][Gender][0];
        }

        if (EffectSkill is { IsMonkey: true })
        {
            return unchecked((short)ConstPlayer.MONKEY_HEADS[EffectSkill.LevelMonkey - 1]);
        }

        if (IsMonkeyTailActive)
        {
            IPlayerRandom source = random ?? throw new InvalidOperationException("A player random source is required while the monkey-tail item is active.");
            return unchecked((short)ConstPlayer.MONKEY_TAIL_HEADS[source.NextInt(0, 6)]);
        }

        short? effectPart = GetEffectOutfitPart(0);
        if (effectPart is not null)
        {
            return effectPart.Value;
        }

        if (HasVisibleFusion())
        {
            short? fusionPart = GetFusionOutfitPart(0);
            if (fusionPart is not null)
            {
                return fusionPart.Value;
            }
        }
        else if (GetCostumeTemplate()?.Head is int costumeHead && costumeHead != -1)
        {
            return unchecked((short)costumeHead);
        }

        return Head;
    }

    // Lấy thân hiển thị; phù hộ Mabu đứng trước hợp thể đúng thứ tự Java (Java: getBody).
    public short GetBody()
    {
        if (EffectSkill is { IsBottled: true })
        {
            return idOutfitMafuba[EffectSkill.BottledType][1];
        }

        if (EffectSkill is { IsStone: true })
        {
            return 455;
        }

        if (EffectSkill is { IsHalloween: true })
        {
            return idOutfitHalloween[EffectSkill.HalloweenOutfitId][Gender][1];
        }

        if (EffectSkill is { IsMonkey: true } || IsMonkeyTailActive)
        {
            return 193;
        }

        short? effectPart = GetEffectOutfitPart(1);
        if (effectPart is not null)
        {
            return effectPart.Value;
        }

        if (IsMabuMapProtection)
        {
            return idOutfitGod[Gender][1];
        }

        if (HasVisibleFusion())
        {
            short? fusionPart = GetFusionOutfitPart(1);
            if (fusionPart is not null)
            {
                return fusionPart.Value;
            }
        }

        if (GetCostumeTemplate()?.Body is int costumeBody && costumeBody != -1)
        {
            return unchecked((short)costumeBody);
        }

        return GetBodyItem(0)?.Part ?? (Gender == ConstPlayer.NAMEKIAN ? (short)59 : (short)57);
    }

    // Lấy chân hiển thị; phù hộ Mabu chỉ áp dụng khi không hợp thể đúng nhánh Java (Java: getLeg).
    public short GetLeg()
    {
        if (EffectSkill is { IsBottled: true })
        {
            return idOutfitMafuba[EffectSkill.BottledType][2];
        }

        if (EffectSkill is { IsStone: true })
        {
            return 456;
        }

        if (EffectSkill is { IsHalloween: true })
        {
            return idOutfitHalloween[EffectSkill.HalloweenOutfitId][Gender][2];
        }

        if (EffectSkill is { IsMonkey: true } || IsMonkeyTailActive)
        {
            return 194;
        }

        short? effectPart = GetEffectOutfitPart(2);
        if (effectPart is not null)
        {
            return effectPart.Value;
        }

        if (IsMabuMapProtection && AppearanceFusionType == ConstPlayer.NON_FUSION)
        {
            return idOutfitGod[Gender][2];
        }

        if (HasVisibleFusion())
        {
            short? fusionPart = GetFusionOutfitPart(2);
            if (fusionPart is not null)
            {
                return fusionPart.Value;
            }
        }
        else if (GetCostumeTemplate()?.Leg is int costumeLeg && costumeLeg != -1)
        {
            return unchecked((short)costumeLeg);
        }

        return GetBodyItem(1)?.Part ?? (Gender == ConstPlayer.NAMEKIAN ? (short)60 : (short)58);
    }

    // Lấy hào quang; dữ liệu ra-đa được truyền vào để không gọi RadarService singleton (Java: getAura).
    public sbyte GetAura(IReadOnlyList<(int Id, sbyte Used)>? radarCards = null, IReadOnlyDictionary<int, sbyte>? radarAuraIds = null)
    {
        sbyte aura = -1;
        ItemTemplate? costume = IsPl() ? GetCostumeTemplate() : null;
        if (costume is not null)
        {
            aura = costume.Id switch
            {
                2013 => 10,
                2014 => 14,
                2015 => 15,
                2059 => 11,
                _ => aura,
            };
        }

        if (aura == -1 && radarCards is not null && radarAuraIds is not null)
        {
            foreach ((int id, sbyte used) in radarCards)
            {
                if (used == 1 && radarAuraIds.TryGetValue(id, out sbyte radarAura) && radarAura > 0)
                {
                    aura = radarAura;
                    break;
                }
            }
        }

        return aura == -1 ? GetAuraPower() : aura;
    }

    // Lấy hào quang theo sức mạnh và giữ hai ngưỡng 80/100 tỷ cùng mã 6 của Java (Java: auraPower).
    public sbyte GetAuraPower()
    {
        if (NPoint is null)
        {
            return 0;
        }

        return NPoint.Power switch
        {
            >= 180_000_000_000L => 84,
            >= 120_000_000_000L => 83,
            >= 110_000_000_000L => 82,
            >= 100_000_000_000L => 6,
            >= 80_000_000_000L => 6,
            _ => -1,
        };
    }

    // Lấy cờ sau lưng; trạng thái nhiệm vụ và ảnh bang là tham số để không phụ thuộc singleton TaskService hay Clan (Java: getFlagBag).
    public short GetFlagBag(bool isHoldingBlackBall, int taskId, short? clanImageId)
    {
        if (isHoldingBlackBall)
        {
            return 31;
        }

        if (IdNRNM is >= 353 and <= 359 || IsHoldingContestedNamekBall)
        {
            return 30;
        }

        if (GetBodyItem(8)?.Part is short flagPart)
        {
            return flagPart;
        }

        if (taskId == ConstTask.TASK_3_2)
        {
            return 28;
        }

        return clanImageId ?? -1;
    }

    // Lấy thú cưỡi; bảng chuyển đổi mount do caller truyền vào thay DataGame.MAP_MOUNT_NUM singleton (Java: getMount).
    public short GetMount(IReadOnlyDictionary<int, short>? mountNumbers = null)
    {
        ItemTemplate? template = GetBodyItem(9);
        if (template is null)
        {
            return -1;
        }

        if (template.Type == 24)
        {
            return template.Gender == 3 || template.Gender == Gender ? template.Id : (short)-1;
        }

        if (template.Id < 500)
        {
            return template.Id;
        }

        return mountNumbers is not null && mountNumbers.TryGetValue(template.Id, out short mount) ? mount : (short)-1;
    }

    // Lấy nón; Java hiện luôn trả -1 (Java: getHat).
    public int GetHat() => -1;

    // Lấy đầu siêu biến hình, an toàn trả -1 khi chỉ số Java vượt dải mảng 0..5 thay vì clamp (Java: getHeadSuper).
    public short GetHeadSuper() => GetSuperOutfitPart(0);

    // Lấy thân siêu biến hình, an toàn trả -1 khi chỉ số Java vượt dải mảng 0..5 thay vì clamp (Java: getBodySuper).
    public short GetBodySuper() => GetSuperOutfitPart(1);

    // Lấy chân siêu biến hình, an toàn trả -1 khi chỉ số Java vượt dải mảng 0..5 thay vì clamp (Java: getLegSuper).
    public short GetLegSuper() => GetSuperOutfitPart(2);

    // Chọn phần hiệu ứng không thuộc khối bình/đá/Halloween/khỉ, theo đúng chuỗi else-if Java (Java: getHead, getBody, getLeg).
    private short? GetEffectOutfitPart(int part) => EffectSkill switch
    {
        { IsCumber: true } => part switch { 0 => 1903, 1 => 1904, _ => 1902 },
        { IsCumber2: true } => part switch { 0 => 1288, 1 => 1289, _ => 1290 },
        { IsPain: true } => part switch { 0 => 1450, 1 => 1451, _ => 1452 },
        { IsKefla: true } => part switch { 0 => 1554, 1 => 1555, _ => 1556 },
        { IsChocolate: true } => part switch { 0 => 412, 1 => 413, _ => 414 },
        _ when EffectSkin is { IsChocolate: true } => part switch { 0 => 412, 1 => 413, _ => 414 },
        _ when EffectSkin is { IsRabbitLeader: true } => part switch { 0 => 406, 1 => 407, _ => 408 },
        { IsPumpkin: true } => part switch { 0 => 760, 1 => 761, _ => 762 },
        _ when EffectSkin is { IsDraburaFrost: true } => part switch { 0 => 1210, 1 => 1211, _ => 1212 },
        _ => null,
    };

    // Kiểm tra điều kiện hiện bộ hợp thể; Java đòi type khác 0 và cờ hienThiHopThe bằng 1 (Java: fusion.typeFusion, hienThiHopThe).
    private bool HasVisibleFusion() => AppearanceFusionType != ConstPlayer.NON_FUSION && FusionDisplayState == 1;

    // Lấy phần của bộ hợp thể, gồm trường hợp hai cải trang 2014/2015 và sáu cải trang fusion trước loại hợp thể (Java: getHead, getBody, getLeg).
    private short? GetFusionOutfitPart(int part)
    {
        short? costumeId = GetCostumeTemplate()?.Id;
        short? petCostumeId = AppearancePet?.GetCostumeTemplate()?.Id;
        if ((costumeId == 2014 && petCostumeId == 2015) || (costumeId == 2015 && petCostumeId == 2014))
        {
            return part switch { 0 => 1912, 1 => 1913, _ => 1914 };
        }

        int costumeIndex = costumeId switch { 601 => 0, 602 => 1, 603 => 2, 640 => 3, 641 => 4, 639 => 5, _ => -1 };
        if (costumeIndex >= 0)
        {
            return idOutfitCaiTrangFusion[costumeIndex][part];
        }

        int fusionIndex = AppearanceFusionType switch
        {
            ConstPlayer.FUSION_DANCE => Gender == ConstPlayer.NAMEKIAN ? 2 : 0,
            ConstPlayer.PORATA_FUSION => Gender == ConstPlayer.NAMEKIAN ? 2 : 1,
            ConstPlayer.PORATA_FUSION_2 => 3 + Gender,
            ConstPlayer.PORATA_FUSION_3 => 6 + Gender,
            ConstPlayer.PORATA_FUSION_4 => 9 + Gender,
            ConstPlayer.PORATA_FUSION_5 => 12 + Gender,
            _ => -1,
        };
        return fusionIndex >= 0 ? idOutfitFusion[fusionIndex][part] : null;
    }

    // Lấy template ở ô body và trả null cho ô chưa nạp/không tồn tại, thay lỗi chỉ mục bằng sentinel của port (Java: inventory.itemsBody.get).
    private ItemTemplate? GetBodyItem(int index)
    {
        return Inventory?.ItemsBody is { } items && index >= 0 && index < items.Count ? items[index]?.Template : null;
    }

    // Lấy cải trang tại ô body 5 (Java: inventory.itemsBody.get(5)).
    private ItemTemplate? GetCostumeTemplate() => GetBodyItem(5);

    // Lấy một phần siêu biến hình; Java không clamp chỉ số, port chỉ bảo vệ index ngoài 0..5 bằng -1 (Java: getHeadSuper, getBodySuper, getLegSuper).
    private short GetSuperOutfitPart(int part)
    {
        (int skillId, short[][] outfits) = Gender switch
        {
            ConstPlayer.EARTHLING => (27, idOutFitSuperEarth),
            ConstPlayer.NAMEKIAN => (28, idOutFitSuperNamec),
            ConstPlayer.SAIYAN => (29, idOutFitSuperSaiyan),
            _ => (-1, []),
        };
        if (skillId == -1)
        {
            return -1;
        }

        int? point = PlayerSkill?.GetSkillbyId(skillId)?.Point;
        if (point is null)
        {
            return -1;
        }

        int index = (point.Value - 1) - NumUseSkill;
        return index is >= 0 and < 6 ? outfits[index][part] : (short)-1;
    }
}
