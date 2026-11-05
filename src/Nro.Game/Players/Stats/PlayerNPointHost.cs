using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

using Nro.Core.Templates;

namespace Nro.Game.Players.Stats;

// Host mặc định chụp dữ liệu nằm trên Player: cờ, thời hạn thẻ, hệ số clan, buff, mã số, ItemTime, EffectSkill, EffectSkin, vật phẩm, Ngọc Rồng, nội tại, kỹ năng chọn và chỉ số đệ tử.
// Nguồn Java: nro/player/NPoint.java (Java gọi các singleton MapService.gI(), ServerManager.gI(), InventoryService.gI(), Service.gI(); D1 cấm gI(), nên H-01/J phải kế thừa lớp này và ghi đè hook bằng dịch vụ thực. Lớp không trạng thái, dùng chung Shared).
// DEFERRED: Charms, Fusion.typeFusion, PointFusion, Card, Clan, Satellite, MySession.Vip_Point và bản ghi đệ tử còn placeholder; các vị từ map, cấu hình/thuộc tính server, hạng Whis, PowerLimit, thưởng inventory/badge, HP quanh người tung cầu và mọi lời gọi ra đều chưa nối. Trước khi ghi đè, thưởng tắt và lời gọi không làm gì.
public class PlayerNPointHost : INPointHost
{
    // Thực thể host không trạng thái dùng chung cho new NPoint(player).
    public static readonly PlayerNPointHost Shared = new();

    private static readonly ConcurrentDictionary<(Type Source, Type Target), (PropertyInfo Source, PropertyInfo Target)[]> Maps = new();

    // Chụp dữ liệu người chơi thành bản NPointFacts cho một lần tính chỉ số.
    public virtual NPointFacts Capture(NPoint npoint)
    {
        Player player = npoint.Player;
        NPointFacts facts = new();
        CapturePlayer(player, facts.Player);
        NPoint? petPoint = player.Pet?.NPoint;
        if (petPoint is not null)
        {
            facts.Pet = new NPointPetFacts
            {
                HpMax = petPoint.HpMax,
                MpMax = petPoint.MpMax,
                Damage = petPoint.Damage,
            };
        }

        CaptureComponents(player, facts);
        CaptureCollaborators(npoint, facts);
        return facts;
    }

    // Nạp phần Player của facts; IsPl theo isPl() Java nhưng không kiểm NonInteractiveNPC vì lớp đó thuộc Nro.Content.
    public static void CapturePlayer(Player player, NPointPlayerFacts p)
    {
        p.Id = player.Id;
        p.Name = player.Name;
        p.Gender = player.Gender;
        p.IsPl = IsPl(player);
        p.IsPlMan = IsPlMan(player);
        p.IsBotAny = player.IsBot || player.IsBot_New || player.IsBot_Event || player.IsBot_Valentine;
        p.IsBoss = player.IsBoss;
        p.IsPet = player.IsPet;
        p.IsFather = player.IsFather;
        p.IsMother = player.IsMother;
        p.IsBodyClone = player.IsBodyClone;
        p.IsMabuMapProtected = player.IsMabuMapProtected;
        p.WeeklyCard = player.WeeklyCard;
        p.LastWeeklyCardTime = player.LastWeeklyCardTime;
        p.MonthlyCard = player.MonthlyCard;
        p.LastMonthlyCardTime = player.LastMonthlyCardTime;
        p.YearlyCard = player.YearlyCard;
        p.LastYearlyCardTime = player.LastYearlyCardTime;
        p.SupremeCard = player.SupremeCard;
        p.LastSupremeCardTime = player.LastSupremeCardTime;
        p.ClanHpBonusPercent = player.ClanHpBonusPercent;
        p.ClanMpBonusPercent = player.ClanMpBonusPercent;
        p.ClanDamageBonusPercent = player.ClanDamageBonusPercent;
        p.UsesEmperorTitle = player.UsesEmperorTitle;
        p.LastEmperorTitleTime = player.LastEmperorTitleTime;
        p.TransformationState = player.TransformationState;
        p.Trbne = player.Trbne;
        p.CFlag = player.CFlag;
        p.TypeChibi = player.TypeChibi;
        p.Buff = player.Buff;
        p.IsAccountProtected = player.IsAccountProtected;
    }

    // Chụp component người chơi sở hữu; component null của new Player() trần được hiểu là không có hiệu ứng. Cờ được chép theo tên thuộc tính theo quy ước Player shell; kiểm thử sẽ lỗi nếu facts không có đối ứng.
    private static void CaptureComponents(Player player, NPointFacts facts)
    {
        if (player.ItemTime is not null)
        {
            CopyByName(player.ItemTime, facts.ItemTime);
        }

        if (player.EffectSkill is not null)
        {
            CopyByName(player.EffectSkill, facts.EffectSkill);
        }

        if (player.EffectSkin is not null)
        {
            CopyByName(player.EffectSkin, facts.EffectSkin);
        }

        if (player.EffectFlagBag is not null)
        {
            facts.Player.UseFishSkewer = player.EffectFlagBag.UseFishSkewer;
        }

        if (player.SetClothes is not null)
        {
            CopyByName(player.SetClothes, facts.SetClothes);
        }

        if (player.Inventory is not null)
        {
            foreach (Item item in player.Inventory.ItemsBody)
            {
                facts.Inventory.ItemsBody.Add(ToFact(item));
            }

            foreach (Item item in player.Inventory.ItemsBag)
            {
                facts.Inventory.ItemsBag.Add(ToFact(item));
            }

            if (player.Inventory.TrainingArmor is not null)
            {
                facts.Inventory.TrainingArmor = ToFact(player.Inventory.TrainingArmor);
            }
        }

        if (player.RewardBlackBall is not null)
        {
            facts.BlackBallTimeOut = [.. player.RewardBlackBall.TimeOutOfDateReward];
        }

        Intrinsic? intrinsic = player.PlayerIntrinsic?.Intrinsic;
        if (intrinsic is not null)
        {
            facts.Intrinsic = new NPointIntrinsic { Id = intrinsic.Id, Param1 = intrinsic.Param1 };
        }

        facts.SkillSelect = ToFact(player.PlayerSkill?.SkillSelect);
        if (player.Zone is not null)
        {
            facts.Zone = new NPointZoneFacts { MapId = player.Zone.Map.MapId, MapType = player.Zone.Map.Type };
        }
    }

    // Chuyển Item thành NPointItem và giữ Source để có thể ghi trả vào vật phẩm thật.
    internal static NPointItem ToFact(Item item)
    {
        ItemTemplate? template = item.Template;
        return new NPointItem
        {
            IsNotNullItem = template is not null,
            TemplateId = template?.Id ?? 0,
            TemplateType = template?.Type ?? 0,
            ItemOptions = [.. item.ItemOptions.Select(io => new NPointOption(io.OptionTemplate.Id, io.Param))],
            TrainArmorPercent = TrainArmorPercent(template?.Id ?? 0),
            Source = item,
        };
    }

    private static NPointSkillFacts? ToFact(Skill? skill)
    {
        return skill is null ? null : new NPointSkillFacts { TemplateId = skill.Template?.Id ?? 0, Damage = skill.Damage, Point = skill.Point };
    }

    // Tra phần trăm giáp tập luyện theo template, lặp lại bảng ItemService vì phương thức dịch vụ cần một instance.
    internal static int TrainArmorPercent(int templateId) => templateId switch
    {
        529 or 534 => 10,
        530 or 535 => 20,
        531 or 536 => 30,
        1797 => 40,
        _ => 0,
    };

    // Chép thuộc tính cùng tên, đọc được sang thuộc tính ghi được của source/target cho bool và các số nguyên; thuộc tính chỉ có ở target giữ mặc định.
    public static void CopyByName(object source, object target)
    {
        foreach ((PropertyInfo from, PropertyInfo to) in MatchingProperties(source.GetType(), target.GetType()))
        {
            object? value = from.GetValue(source);
            if (value is not null)
            {
                to.SetValue(target, from.PropertyType == to.PropertyType ? value : Convert.ChangeType(value, to.PropertyType, CultureInfo.InvariantCulture));
            }
        }
    }

    // Liệt kê thuộc tính ghi được của target không có thuộc tính đọc cùng tên ở source để kiểm thử lệch mô hình.
    public static IReadOnlyList<string> UnmatchedProperties(Type source, Type target)
    {
        HashSet<string> matched = [.. MatchingProperties(source, target).Select(m => m.Target.Name)];
        return [.. target.GetProperties().Where(p => p.CanWrite && !matched.Contains(p.Name)).Select(p => p.Name).Order(StringComparer.Ordinal)];
    }

    private static (PropertyInfo Source, PropertyInfo Target)[] MatchingProperties(Type source, Type target)
    {
        return Maps.GetOrAdd((source, target), static key =>
        {
            List<(PropertyInfo, PropertyInfo)> pairs = [];
            foreach (PropertyInfo to in key.Target.GetProperties())
            {
                PropertyInfo? from = key.Source.GetProperty(to.Name);
                if (to.CanWrite && from is { CanRead: true } && (from.PropertyType == to.PropertyType || (IsNumeric(from.PropertyType) && IsNumeric(to.PropertyType))))
                {
                    pairs.Add((from, to));
                }
            }

            return [.. pairs];
        });
    }

    private static bool IsNumeric(Type t) => t == typeof(sbyte) || t == typeof(short) || t == typeof(int) || t == typeof(long);

    // Xác định người chơi thực, không phải đệ tử, họ hàng, boss, phân thân, bot, Saibamen hay thú đi theo.
    public static bool IsPl(Player player)
    {
        return player.IsPlayer && !player.IsPet && !player.IsFather && !player.IsMother && !player.IsBoss && !player.IsPetFollow && !player.IsTangMonk && !player.IsBodyClone
            && !player.IsBot_Valentine && !player.IsBot && !player.IsBot_Event && !player.IsBot_New && !player.IsSaibamen && !player.IsLover && !player.IsChildOne
            && !player.IsChildTwo && !player.IsChildThree;
    }

    // Xác định nhóm người/chủ theo Player.isPlMan() của Java.
    public static bool IsPlMan(Player player)
    {
        return (player.IsPlayer || player.IsPet || player.IsFather || player.IsMother || player.IsLover || player.IsChildOne || player.IsChildTwo || player.IsChildThree || player.IsBodyClone)
            && !player.IsBoss && !player.IsPetFollow && !player.IsTangMonk && !player.IsBot && !player.IsBot_Event && !player.IsBot_New && !player.IsBot_Valentine && !player.IsSaibamen;
    }

    // Hook bổ sung facts từ dịch vụ hoặc lớp chưa chuyển; mặc định giữ trạng thái không có hiệu ứng.
    protected virtual void CaptureCollaborators(NPoint npoint, NPointFacts facts)
    {
    }

    // Ghi các kết quả tính chỉ số có thể ghi trả vào đối tượng thực.
    public virtual void Commit(NPoint npoint, NPointFacts facts)
    {
        // Ghi số áo World Cup NPoint đã đếm về trạng thái SetClothes đã port.
        CommitWorldcup(npoint, facts.SetClothes.Worldcup);
        Player player = npoint.Player;
        if (player.Inventory is not null && facts.Inventory.TrainingArmor?.Source is Item armor)
        {
            player.Inventory.TrainingArmor = armor;
        }
    }

    // Hook ghi số áo World Cup NPoint đếm được vào player.setClothes.worldcup.
    protected virtual void CommitWorldcup(NPoint npoint, sbyte worldcup)
    {
        npoint.Player.SetClothes.Worldcup = worldcup;
    }

    // Chụp dữ liệu cần riêng cho một đòn tấn công.
    public virtual NPointAttackFacts CaptureAttack(NPoint npoint)
    {
        Player player = npoint.Player;
        Skill skill = player.PlayerSkill?.SkillSelect ?? throw new NullReferenceException("player.playerSkill.skillSelect is null");
        NPointAttackFacts af = new() { SkillSelect = ToFact(skill)!, IsPet = player.IsPet };
        Intrinsic? intrinsic = player.PlayerIntrinsic?.Intrinsic;
        if (intrinsic is not null)
        {
            af.Intrinsic = new NPointIntrinsic { Id = intrinsic.Id, Param1 = intrinsic.Param1 };
        }

        if (player.EffectSkill is not null)
        {
            CopyByName(player.EffectSkill, af.EffectSkill);
        }

        if (player.EffectSkin is not null)
        {
            CopyByName(player.EffectSkin, af.EffectSkin);
        }

        if (player.SetClothes is not null)
        {
            CopyByName(player.SetClothes, af.SetClothes);
        }

        CaptureAttackCollaborators(npoint, af);
        return af;
    }

    // Hook chụp bùa chủ của đệ tử, Manager.readInt và HP quanh người tung Quả cầu kênh khí.
    protected virtual void CaptureAttackCollaborators(NPoint npoint, NPointAttackFacts facts)
    {
    }

    // Ghi trả các hiệu ứng da thay đổi sau khi tấn công.
    public virtual void CommitAttack(NPoint npoint, NPointAttackFacts facts)
    {
        EffectSkin? skin = npoint.Player.EffectSkin;
        if (skin is not null)
        {
            skin.IsBlastDamageMultiplierReady = facts.EffectSkin.IsBlastDamageMultiplierReady;
            skin.IsBlastDamageMultiplierApplied = facts.EffectSkin.IsBlastDamageMultiplierApplied;
            skin.LastTimeBlastDamageMultiplier = facts.EffectSkin.LastTimeBlastDamageMultiplier;
        }
    }

    // Chụp trạng thái cần cho update mỗi tick.
    public virtual NPointUpdateFacts CaptureUpdate(NPoint npoint)
    {
        EffectSkill? effect = npoint.Player.EffectSkill;
        if (effect is null)
        {
            return new NPointUpdateFacts { HasEffectSkill = false };
        }

        Skill? skill = npoint.Player.PlayerSkill?.SkillSelect;
        return new NPointUpdateFacts
        {
            HasEffectSkill = true,
            IsCharging = effect.IsCharging,
            CountCharging = effect.CountCharging,
            IsHaveEffectSkill = effect.Environment is not null && effect.IsHaveEffectSkill(), // without an environment the effect system is not wired: nothing holds the player
            HasSkillSelect = skill is not null,
            SkillSelectPoint = skill?.Point ?? 0,
        };
    }

    // Trả ngưỡng sức mạnh; mặc định chưa có dịch vụ nên trả null.
    public virtual PowerLimitInfo? GetPowerLimit(int limitPower) => null;

    // Hook xử lý người chơi chết; mặc định chưa nối dịch vụ.
    public virtual void SetDie(Player player)
    {
    }

    // Hook hồi HP và MP; mặc định chưa nối dịch vụ.
    public virtual void RestoreHpMp(Player player, long hp, long mp)
    {
    }

    // Hook gửi thể lực hiện tại; mặc định chưa gửi gói.
    public virtual void SendCurrentStamina(Player player)
    {
    }

    // Hook gửi tin nhắn chat; mặc định chưa gửi gói.
    public virtual void Chat(Player player, string text)
    {
    }

    // Hook dừng nạp năng lượng; mặc định chưa nối hiệu ứng.
    public virtual void StopCharge(Player player)
    {
    }

    // Tăng bộ đếm nạp năng lượng nếu EffectSkill tồn tại.
    public virtual int IncrementCountCharging(Player player)
    {
        EffectSkill? effect = player.EffectSkill;
        if (effect is null)
        {
            return 0;
        }

        effect.CountCharging++;
        return effect.CountCharging;
    }

    // Xóa trạng thái và bộ đếm nạp năng lượng khi EffectSkill tồn tại.
    public virtual void ClearCharging(Player player)
    {
        EffectSkill? effect = player.EffectSkill;
        if (effect is not null)
        {
            effect.IsCharging = false;
            effect.CountCharging = 0;
        }
    }

    // Hook gửi thông báo thường; mặc định chưa gửi gói.
    public virtual void SendNotification(Player player, string text)
    {
    }

    // Hook gửi hộp thông báo xác nhận; mặc định chưa gửi gói.
    public virtual void SendNotificationOk(Player player, string text)
    {
    }

    // Hook gửi tiền hiện tại; mặc định chưa gửi gói.
    public virtual void SendMoney(Player player)
    {
    }

    // Hook gửi chỉ số hiện tại; mặc định chưa gửi gói.
    public virtual void SendPoint(Player player)
    {
    }

    // Hook gửi thông tin đệ tử gốc; mặc định chưa gửi gói.
    public virtual void SendBasePetInfo(Player player)
    {
    }

    // Hook hiển thị thông tin đệ tử; mặc định chưa gửi gói.
    public virtual void ShowInfoPet(Player player)
    {
    }

    // Hook kiểm tra nhiệm vụ sức mạnh hoàn thành; mặc định chưa nối nhiệm vụ.
    public virtual void CheckDoneTaskPower(Player player, long power)
    {
    }

    // Hook kiểm tra nhiệm vụ dùng tiềm năng hoàn thành; mặc định chưa nối nhiệm vụ.
    public virtual void CheckPotentialUseTaskCompletion(Player player)
    {
    }
}
