using Nro.Game.Content;
using Nro.Game.Effects.Ports;
using Nro.Game.Skills.Ports;

namespace Nro.Game.Skills;

// Nguồn Java: Utils/SkillUtil.java (dòng 12–407).
// Quy tắc kỹ năng không trạng thái; dữ liệu mẫu đi qua ISkillCatalog, còn phân loại map đi qua ISkillMapRules thay các singleton Java.
public sealed class SkillUtil : IEffectRules
{
    // Danh mục ba lớp kỹ năng đã nạp. (Java: Manager.NCLASS)
    private readonly ISkillCatalog catalog;

    // Cổng phân loại map ngoại tuyến. (Java: MapService.isMapOffline)
    private readonly ISkillMapRules maps;

    // Tạo bộ quy tắc từ catalog kỹ năng và cổng map. (Java: static NCLASS / MapService.gI)
    public SkillUtil(ISkillCatalog catalog, ISkillMapRules maps)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(maps);
        this.catalog = catalog;
        this.maps = maps;
    }

    // Tạo bản sao cấp kỹ năng từ template; trả null nếu không có template hoặc cấp ngoài khoảng. (Java: createSkill)
    public Skill? CreateSkill(int templateId, int level)
    {
        SkillTemplate? template = FindSkillTemplate(templateId);
        if (template?.Skillss is not { } skills || level < 1 || level > skills.Count)
        {
            return null;
        }

        return new Skill(skills[level - 1]);
    }

    // Tìm template lần lượt trong ba lớp Trái Đất, Namếc, Xayda. (Java: findSkillTemplate)
    public SkillTemplate? FindSkillTemplate(int templateId)
    {
        for (int classId = 0; classId <= 2; classId++)
        {
            SkillTemplate? template = catalog.GetClass(classId).GetSkillTemplate(templateId);
            if (template is not null)
            {
                return template;
            }
        }

        return null;
    }

    // Tạo slot kỹ năng trống có SkillId bằng -1. (Java: createEmptySkill)
    public static Skill CreateEmptySkill() => new() { SkillId = -1 };

    // Lấy đúng cấp dùng chung từ ba lớp, giữ thứ tự catch/fallback và ngoại lệ Java khi không tìm thấy. (Java: getSkill)
    public Skill GetSkill(int templateId, int level)
    {
        try
        {
            return catalog.GetClass(0).GetSkillTemplate(templateId)!.Skillss[level - 1];
        }
        catch
        {
            try
            {
                return catalog.GetClass(1).GetSkillTemplate(templateId)!.Skillss[level - 1];
            }
            catch
            {
                return catalog.GetClass(2).GetSkillTemplate(templateId)!.Skillss[level - 1];
            }
        }
    }

    // Tạo kỹ năng cấp 0 chỉ mang id template. (Java: createSkillLevel0)
    public static Skill CreateSkillLevel0(int templateId) => new()
    {
        SkillId = -1,
        Template = new SkillTemplate { Id = unchecked((sbyte)templateId) },
    };

    // Kiểm tra kỹ năng chọn thuộc nhóm chiêu đấm. (Java: isUseSkillDam)
    public static bool IsUsingPunchSkill(Player player)
    {
        int skillId = player.PlayerSkill.SkillSelect!.Template!.Id;
        return skillId is Skill.DRAGON or Skill.DEMON or Skill.GALICK or Skill.KAIOKEN or Skill.LIEN_HOAN;
    }

    // Kiểm tra kỹ năng chọn thuộc nhóm chiêu chưởng. (Java: isUseSkillChuong)
    public static bool IsUsingBlastSkill(Player player)
    {
        int skillId = player.PlayerSkill.SkillSelect!.Template!.Id;
        return skillId is Skill.KAMEJOKO or Skill.MASENKO or Skill.ANTOMIC;
    }

    // Tính phần trăm phân thân, mặc định 10 nếu chưa có kỹ năng chọn. (Java: getPercentPhanThan)
    public static int GetClonePercent(Player? player) => player?.PlayerSkill?.SkillSelect is { } selected
        ? GetClonePercentForPoint(selected.Point)
        : 10;

    // Tính phần trăm phân thân từ điểm kỹ năng đã chọn, dùng lại cho snapshot NPoint không sở hữu Player. (Java: getPercentPhanThan)
    public static int GetClonePercentForPoint(int point) => unchecked(point * 10);

    // Tính thời lượng hóa khỉ theo cấp. (Java: getTimeMonkey)
    public static int GetTimeMonkey(int level) => unchecked((level + 5) * 10_000);

    // Tính phần trăm HP cộng thêm khi hóa khỉ. (Java: getPercentHpMonkey)
    public static int GetPercentHpMonkey(int level) => unchecked((level + 3) * 10);

    // Tính phần trăm sát thương cộng thêm khi hóa khỉ. (Java: getPercentDameMonkey)
    public static int GetMonkeyDamagePercent(int level) => unchecked(level + 3);

    // Tính thời lượng choáng Thái Dương Hạ San. (Java: getTimeStun)
    public static int GetStunDuration(int level) => unchecked((level + 2) * 1000);

    // Trả thời lượng Sôcôla cố định. (Java: getTimeSocola)
    public static int GetChocolateDuration() => 30_000;

    // Trả thời lượng hóa bình cố định; tham số cấp bị Java bỏ qua. (Java: getTimeBinh)
    public static int GetBottleDuration(int level) => 10_000;

    // Tính thời lượng khiên năng lượng theo cấp. (Java: getTimeShield)
    public static int GetTimeShield(int level) => unchecked((level + 2) * 5000);

    // Trả thời lượng biến hình cố định. (Java: getTimeTranformation)
    public static int GetTimeTranformation() => 300_000;

    // Trả thời lượng tiến hóa cố định. (Java: getTimett)
    public static int GetTimett() => 300_000;

    // Tính thời lượng trói theo cấp. (Java: getTimeTroi)
    public static int GetBindDuration(int level) => unchecked(level * 5000);

    // Tính thời lượng mù do dịch chuyển tức thời theo cấp. (Java: getTimeDCTT)
    public static int GetTeleportBlindDuration(int level) => unchecked((level + 1) * 500);

    // Tính thời lượng thôi miên theo cấp. (Java: getTimeThoiMien)
    public static int GetHypnosisDuration(int level) => unchecked((level + 4) * 1000);

    // Tính phạm vi Thái Dương Hạ San theo cấp. (Java: getRangeStun)
    public static int GetStunRange(int level) => unchecked(120 + (level * 30));

    // Tính phạm vi tự sát theo cấp. (Java: getRangeBom)
    public static int GetSelfDestructRange(int level) => unchecked(500 + (level * 80));

    // Tính phạm vi Quả Cầu Kênh Khí theo cấp. (Java: getRangeQCKK)
    public static int GetEnergyBallRange(int level) => unchecked(350 + (level * 30));

    // Tính phần trăm HP của huýt sáo theo cấp. (Java: getPercentHPHuytSao)
    public static int GetWhistleHpPercent(int level) => unchecked((level + 3) * 10);

    // Tính phần trăm sát thương Sức Mạnh Bộc Phá theo cấp. (Java: getPercentDameSucManhBocPha)
    public static int GetPowerBurstDamagePercent(int level) => unchecked((level + 1) * 5);

    // Tính thời lượng Sức Mạnh Bộc Phá theo cấp. (Java: getTimeSucManhBocPha)
    public static int GetPowerBurstDuration(int level) => unchecked((level + 5) * 5000);

    // Tính phần trăm HP trị thương theo cấp. (Java: getPercentTriThuong)
    public static int GetHealHpPercent(int level) => unchecked((level + 9) * 5);

    // Tính phần trăm nạp năng lượng theo cấp. (Java: getPercentCharge)
    public static int GetPercentCharge(int level) => unchecked(level + 3);

    // Lấy template đệ trứng theo cấp; chỉ số cấp ngoài 1–7 ném như Java. (Java: getTempMobMe)
    public static int GetEggMobTemplateId(int level) => new[] { 8, 11, 32, 25, 43, 49, 50 }[level - 1];

    // Tính thời lượng tồn tại của đệ trứng. (Java: getTimeSurviveMobMe)
    public static int GetEggMobSurvivalDuration(int level) => unchecked(GetTimeMonkey(level) * 2);

    // Tính HP đệ trứng từ HP tối đa người chơi; cấp ngoài 1–7 ném như Java. (Java: getHPMobMe)
    public static long GetEggMobHp(long playerMaxHp, int level)
    {
        long percent = new long[] { 30, 40, 50, 60, 70, 80, 90 }[level - 1];
        return unchecked((playerMaxHp * percent) / 100);
    }

    // Lấy template Vòng Kim Cô theo cấp; cấp ngoài 1–5 ném như Java. (Java: getTempVongKimCo)
    public static int GetGoldenHeadbandTemplateId(int level) => new[] { 69, 75, 79, 87, 92 }[level - 1];

    // Trả thời lượng tồn tại Vòng Kim Cô cố định. (Java: getTimeSurviveVongKimCo)
    public static int GetGoldenHeadbandSurvivalDuration(int level) => 180_000;

    // Tính HP Vòng Kim Cô từ HP tối đa người chơi. (Java: getHPVongKimCo)
    public static int GetGoldenHeadbandHp(int playerMaxHp, int level)
    {
        int percent = new[] { 100, 110, 120, 130, 140 }[level - 1];
        return unchecked((playerMaxHp * percent) / 100);
    }

    // Tính sát thương Vòng Kim Cô từ sát thương người chơi. (Java: getDAMEVongKimCo)
    public static int GetGoldenHeadbandDamage(int playerDamage, int level)
    {
        int percent = new[] { 100, 110, 120, 130, 140 }[level - 1];
        return unchecked((playerDamage * percent) / 100);
    }

    // Tìm kỹ năng đã học đầu tiên theo id template. (Java: getSkillbyId)
    public static Skill? GetSkillById(Player player, int templateId) => player.PlayerSkill is null
        ? null
        : player.PlayerSkill.GetSkillbyId(templateId);

    // Nâng một kỹ năng pet, giới hạn cấp 7 và rút ngắn hồi chiêu ô thứ hai. (Java: upSkillPet)
    public bool UpSkillPet(List<Skill> skills, int index)
    {
        Skill current = skills[index];
        int templateId = current.Template!.Id;
        int level = current.Point + 1;
        if (level > 7)
        {
            return false;
        }

        Skill? source = FindSkillTemplate(templateId)?.Skillss is { } sourceLevels && level >= 1 && level <= sourceLevels.Count
            ? sourceLevels[level - 1]
            : null;
        Skill replacement = new(source!);
        if (index == 1)
        {
            replacement.CoolDown = 1000;
        }

        skills[index] = replacement;
        return true;
    }

    // Ánh xạ id vật phẩm sách kỹ năng thành id template, không có trả -1. (Java: getTempSkillSkillByItemID)
    public static sbyte GetTemplateIdByItemId(int itemTemplateId) => itemTemplateId switch
    {
        >= 66 and <= 72 => Skill.DRAGON,
        (>= 79 and <= 84) or 86 => Skill.DEMON,
        >= 87 and <= 93 => Skill.GALICK,
        >= 94 and <= 100 => Skill.KAMEJOKO,
        >= 101 and <= 107 => Skill.MASENKO,
        >= 108 and <= 114 => Skill.ANTOMIC,
        >= 115 and <= 121 => Skill.THAI_DUONG_HA_SAN,
        >= 122 and <= 128 => Skill.TRI_THUONG,
        >= 129 and <= 135 => Skill.TAI_TAO_NANG_LUONG,
        >= 300 and <= 306 => Skill.KAIOKEN,
        >= 307 and <= 313 => Skill.QUA_CAU_KENH_KHI,
        >= 314 and <= 320 => Skill.BIEN_KHI,
        >= 321 and <= 327 => Skill.TU_SAT,
        >= 328 and <= 334 => Skill.MAKANKOSAPPO,
        >= 335 and <= 341 => Skill.DE_TRUNG,
        >= 434 and <= 440 => Skill.KHIEN_NANG_LUONG,
        >= 474 and <= 480 => Skill.SOCOLA,
        >= 481 and <= 487 => Skill.LIEN_HOAN,
        >= 488 and <= 494 => Skill.DICH_CHUYEN_TUC_THOI,
        >= 495 and <= 501 => Skill.THOI_MIEN,
        >= 502 and <= 508 => Skill.TROI,
        >= 509 and <= 515 => Skill.HUYT_SAO,
        _ => -1,
    };

    // Tìm kỹ năng đã học ứng với vật phẩm sách kỹ năng. (Java: getSkillByItemID)
    public static Skill? GetSkillByItemId(Player player, int itemTemplateId)
    {
        sbyte templateId = GetTemplateIdByItemId(itemTemplateId);
        return templateId == -1 ? null : GetSkillById(player, templateId);
    }

    // Lấy cấp kỹ năng dùng chung ứng với vật phẩm sách kỹ năng. (Java: getSkillByLevel)
    public Skill? GetSkillByLevel(int itemTemplateId, sbyte level)
    {
        sbyte templateId = GetTemplateIdByItemId(itemTemplateId);
        return templateId == -1 ? null : GetSkill(templateId, level);
    }

    // Thay kỹ năng có cùng template trong danh sách đã học đầu tiên. (Java: setSkill)
    public static void SetSkill(Player player, Skill skill)
    {
        List<Skill> skills = player.PlayerSkill.Skills!;
        for (int index = 0; index < skills.Count; index++)
        {
            if (skills[index].Template!.Id == skill.Template!.Id)
            {
                skills[index] = skill;
                break;
            }
        }
    }

    // Phân loại hình tấn công: trị thương 2, chưởng 1, còn lại 0. (Java: getTyleSkillAttack)
    public static sbyte GetSkillAttackType(Skill skill) => skill.Template!.Id switch
    {
        Skill.TRI_THUONG => 2,
        Skill.KAMEJOKO or Skill.MASENKO or Skill.ANTOMIC => 1,
        _ => 0,
    };

    // Kiểm tra map ngoại tuyến qua cổng phân loại map. (Java: MapService.isMapOffline)
    public bool IsMapOffline(int mapId) => maps.IsMapOffline(mapId);

    // Kiểm tra map chiến tranh ngọc rồng theo dải id. (Java: MapService.isMapBlackBallWar)
    public static bool IsMapBlackBallWar(int mapId) => mapId is >= 85 and <= 91;

    // Kiểm tra map Mabu 12 giờ theo dải id. (Java: MapService.isMapMaBu12H)
    public static bool IsMapMaBu12H(int mapId) => mapId is >= 114 and <= 120;

    // Cài IEffectRules với phân loại map chiến tranh ngọc rồng. (Java: MapService.isMapBlackBallWar)
    bool IEffectRules.IsMapBlackBallWar(int mapId) => IsMapBlackBallWar(mapId);

    // Cài IEffectRules với phân loại map Mabu 12 giờ. (Java: MapService.isMapMaBu12H)
    bool IEffectRules.IsMapMaBu12H(int mapId) => IsMapMaBu12H(mapId);

    // Cài IEffectRules với thời lượng hóa khỉ. (Java: SkillUtil.getTimeMonkey)
    int IEffectRules.GetTimeMonkey(int level) => GetTimeMonkey(level);

    // Cài IEffectRules với thời lượng khiên. (Java: SkillUtil.getTimeShield)
    int IEffectRules.GetTimeShield(int level) => GetTimeShield(level);

    // Cài IEffectRules với thời lượng biến hình. (Java: SkillUtil.getTimeTranformation)
    int IEffectRules.GetTimeTranformation() => GetTimeTranformation();

    // Cài IEffectRules với thời lượng tiến hóa. (Java: SkillUtil.getTimett)
    int IEffectRules.GetTimett() => GetTimett();

    // Cài IEffectRules với thời lượng Sức Mạnh Bộc Phá. (Java: SkillUtil.getTimeSucManhBocPha)
    int IEffectRules.GetPowerBurstDuration(int level) => GetPowerBurstDuration(level);
}
