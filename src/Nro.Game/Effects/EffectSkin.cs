using System.Runtime.CompilerServices;
using Nro.Core.Logging;
using Nro.Core.Util;

namespace Nro.Game.Effects;

// Quản lý hiệu ứng trang phục có thời hạn của một người chơi: trạng thái từ đồ, pet và kỹ năng sự kiện; Update chạy mỗi tick zone, thực hiện hiệu ứng tuần hoàn rồi gỡ hiệu ứng hết hạn qua các dịch vụ hiệu ứng.
// Nguồn Java: nro/effect/EffectSkin.java (giữ khoảng cách (int)sqrt(dx^2 + dy^2), duyệt bản sao người chơi xung quanh từ cuối và điều kiện thời gian now - last > period nghiêm ngặt).
// Mỗi aura có mốc thời gian riêng; lỗi của một hiệu ứng được ghi log và không chặn hiệu ứng tiếp theo, trừ các phương thức Java vốn không bắt lỗi được nêu dưới đây.
// Giữ quirk Java: id trang phục ngoài miền ở updateCTHaiTac/updateBunmaTocMau/updateTiecbaiBien và updateTrainArmor có thể làm Update ném IndexOutOfRangeException; CT Hải Tặc chỉ gửi item cho người đầu tiên; Tiệc bãi biển giảm 5% mãi và chỉ cho láng giềng; virus gặp mặt nạ thì dừng không lưu giờ; ba buff sát thương được cộng cho chủ mỗi láng giềng; Ma Phong Ba chừa HP cách 0 một trị tuyệt đối 100 khi sát thương gây chết.
// Khác Java có chủ đích: Environment thay singleton gI()/Util, truy cập lớp khác qua Effects/Ports, vật phẩm không có danh sách option bị bỏ qua thay vì NullPointerException; Dispose giữ Player rồi Update không làm gì; không dùng khóa theo D1 và danh sách láng giềng là bản sao.
public class EffectSkin
{
    // Các câu thoại ngẫu nhiên của aura Ở dơ (Java: textOdo).
    public static readonly string[] DirtyTalk =
    [
        "Hôi quá, tránh xa ta ra",
        "Biến đi",
        "Trời ơi đồ ở dơ",
        "Thúi quá",
        "Mùi gì hôi quá",
    ];

    private static readonly string[] RabbitBulmaTalk = ["Wow, sexy quá"];

    private static readonly string[] DamageBuffTalk = ["Wow!", "Đẹp quá!", "Xinh quá!"];

    private static readonly string[] TextFounder =
    [
        "MaiTienDung", "Anh MaiTienDung!", "Idol Vip Quá!", "Idol",
        "Trùm Server Đây Rồi!", "Nhà Sáng Lập MaiTienDung!",
    ];

    private static readonly string[] TextXinbato =
    [
        "Bực bội quá",
        "Phân tâm quá",
        "Nực quá",
        "Phân tâm quá",
        "Tránh ra đi Xinbatô ơi",
    ];

    private static readonly string[] SnowflakeTalk =
    [
        "Lạnh quá...",
        "Ư... lạnh ghê...",
        "Brrr...",
        "Chết rét mất...",
    ];

    private static readonly string[] PumpkinTalk =
    [
        "Khoan đã!",
        "Khônggg... bí?!",
        "Tôi... tròn rồi!",
        "Cứu tôi với!",
        "Đầu tôi đâu?!",
    ];

    private static readonly string[] CarrotTalk =
    [
        "Biến thành cà rốt cho ta!",
        "Cà rốt chi thuật!",
        "Cà rốt hóa thân! Hô biến!",
        "Cà rốt hóa hình! Giờ thì chạy đi!",
        "Biến hình... cà rốt style!",
        "Lệnh Thỏ ban ra, biến thành củ đi!",
    ];

    private static readonly string[] VirusTalk =
    [
        "Khặc khặc...",
        "Nóng quá...",
        "Choáng váng...",
        "Khè... khụ!",
        "Không ổn...",
        "Đầu... đau quá...",
        "Chóng mặt...",
    ];

    private EffectEnvironment? environment;
    private bool disposed;

    private long lastTimeAllyHpRecovery10Seconds;
    private long lastTimePumpkin;
    private long lastTimeVirus;
    private long lastTimeSnowflake;

    private long lastTimeEffNezuko;
    private long lastTimeEffTanjiro;
    private long lastTimeEffInoHashi;
    private long lastTimeEffZenitsu;
    private long lastTimeEffInosuke;

    private long lastTimeDirty;
    private long lastTimeFounder;
    private long lastTimeCoolDamage;
    private long lastTimeCuteDamage;
    private long lastTimeRabbitBulma;
    private long lastTimeBulmaHairColor;
    private long lastTimeBeachParty;
    private long lastTimeMafuba;
    private long lastTimeCellHpKiDrain;
    private long lastTimeXinbato;
    private long lastTimeRabbitLeader;

    // Java đặt trường này bằng chữ "t" thường; khác trường công khai LastTimeDraburaFrost bên dưới.
    private long lastTimeDraburaFrost;

    // Java đặt tên trường này bằng chữ "L" hoa.
    private long LastTimeFideLeader;

    private long lastTimeSkillInvisibility;
    private long lastTimePetrification;
    private long lastTimeHalloween;

    // Giá trị trạng thái IsNezuko của hiệu ứng trang phục.
    public bool IsNezuko { get; set; }

    // Giá trị trạng thái IsTanjiro của hiệu ứng trang phục.
    public bool IsTanjiro { get; set; }

    // Giá trị trạng thái IsZenitsu của hiệu ứng trang phục.
    public bool IsZenitsu { get; set; }

    // Giá trị trạng thái IsInosuke của hiệu ứng trang phục.
    public bool IsInosuke { get; set; }

    // Giá trị trạng thái IsInoHashi của hiệu ứng trang phục.
    public bool IsInoHashi { get; set; }

    // Giá trị trạng thái LastTimeInoHashi của hiệu ứng trang phục.
    public long LastTimeInoHashi { get; set; }

    // Giá trị trạng thái LastTimeZenitsu của hiệu ứng trang phục.
    public long LastTimeZenitsu { get; set; }

    // Giá trị trạng thái LastTimeNezuko của hiệu ứng trang phục.
    public long LastTimeNezuko { get; set; }

    // Giá trị trạng thái LastTimeInosuke của hiệu ứng trang phục.
    public long LastTimeInosuke { get; set; }

    // Giá trị trạng thái LastTimeTanjiro của hiệu ứng trang phục.
    public long LastTimeTanjiro { get; set; }

    // Giá trị trạng thái TimeNezuko của hiệu ứng trang phục.
    public int TimeNezuko { get; set; }

    // Giá trị trạng thái TimeTanjiro của hiệu ứng trang phục.
    public int TimeTanjiro { get; set; }

    // Giá trị trạng thái TimeInosuke của hiệu ứng trang phục.
    public int TimeInosuke { get; set; }

    // Giá trị trạng thái TimeZenitsu của hiệu ứng trang phục.
    public int TimeZenitsu { get; set; }

    // Giá trị trạng thái TimeInoHashi của hiệu ứng trang phục.
    public int TimeInoHashi { get; set; }

    // Cờ biến Sôcôla của hiệu ứng trang phục (Java: isSocola).
    public bool IsChocolate { get; set; }

    // Thời lượng biến Sôcôla của hiệu ứng trang phục (Java: timeSocola).
    public int TimeChocolate { get; set; }

    // Mốc biến Sôcôla của hiệu ứng trang phục (Java: lastTimeSocola).
    public long LastTimeChocolate { get; set; }

    // Cờ Thỏ Đại Ca của hiệu ứng trang phục (Java: isThoDaiKa).
    public bool IsRabbitLeader { get; set; }

    // Thời lượng Thỏ Đại Ca của hiệu ứng trang phục (Java: timeThoDaiKa).
    public int TimeRabbitLeader { get; set; }

    // Mốc Thỏ Đại Ca của hiệu ứng trang phục (Java: lastTimeThoDaiKa).
    public long LastTimeRabbitLeader { get; set; }

    // Giá trị trạng thái IsDraburaFrost của hiệu ứng trang phục.
    public bool IsDraburaFrost { get; set; }

    // Giá trị trạng thái TimeDraburaFrost của hiệu ứng trang phục.
    public int TimeDraburaFrost { get; set; }

    // Giá trị trạng thái LastTimeDraburaFrost của hiệu ứng trang phục.
    public long LastTimeDraburaFrost { get; set; }

    // Giá trị trạng thái TimeSlow của hiệu ứng trang phục.
    public int TimeSlow { get; set; }

    // Giá trị trạng thái LastTimeSlow của hiệu ứng trang phục.
    public long LastTimeSlow { get; set; }

    // Giá trị trạng thái IsSlow của hiệu ứng trang phục.
    public bool IsSlow { get; set; }

    // Giá trị trạng thái LastTimeAttack của hiệu ứng trang phục.
    public long LastTimeAttack { get; set; }

    // Mốc vô hình của hiệu ứng trang phục (Java: lastTimeVoHinh).
    public long LastTimeInvisible { get; set; }

    // Giá trị trạng thái LastTimeAddTimeTrainArmor của hiệu ứng trang phục.
    public long LastTimeAddTimeTrainArmor { get; set; }

    // Giá trị trạng thái LastTimeSubTimeTrainArmor của hiệu ứng trang phục.
    public long LastTimeSubTimeTrainArmor { get; set; }

    // Cờ vô hình của hiệu ứng trang phục (Java: isVoHinh).
    public bool IsInvisible { get; set; }

    // Mốc hệ số nhân HP/KI của hiệu ứng trang phục (Java: lastTimeXHPKI).
    public long LastTimeHpKiMultiplier { get; set; }

    // Hệ số nhân HP/KI của hiệu ứng trang phục (Java: xHPKI).
    public int HpKiMultiplier { get; set; }

    // Mốc hệ số nhân sát thương của hiệu ứng trang phục (Java: lastTimeXDame).
    public long LastTimeDamageMultiplier { get; set; }

    // Hệ số nhân sát thương của hiệu ứng trang phục (Java: xDame).
    public int DamageMultiplier { get; set; }

    // Mốc cập nhật trang phục Hải Tặc Tí Hon (Java: lastTimeUpdateCTHT).
    public long LastTimePirateCostumeUpdate { get; set; }

    // Giá trị trạng thái LastTimeMabu của hiệu ứng trang phục.
    public long LastTimeMabu { get; set; }

    // Giá trị trạng thái LastTimeBuiBui của hiệu ứng trang phục.
    public long LastTimeBuiBui { get; set; }

    // Mốc kích hoạt hệ số nhân sát thương đòn chưởng (Java: lastTimeXChuong).
    public long LastTimeBlastDamageMultiplier { get; set; }

    // Cờ sẵn sàng áp hệ số nhân sát thương đòn chưởng (Java: isXChuong).
    public bool IsBlastDamageMultiplierReady { get; set; }

    // Cờ đã áp hệ số nhân sát thương đòn chưởng (Java: isXDame).
    public bool IsBlastDamageMultiplierApplied { get; set; }

    // Người chơi sở hữu hiệu ứng, không null; setter công khai giữ @Setter của Lombok và Dispose không gán null (Java: player).
    // Giá trị trạng thái Player của hiệu ứng trang phục.
    public Player Player { get; set; }

    // Môi trường gồm dịch vụ, đồng hồ và các cổng do factory người chơi gán trước lần Update đầu tiên.
    // Môi trường dùng chung của hiệu ứng, phải được gán trước khi dùng.
    public EffectEnvironment? Environment
    {
        get => environment;
        set => environment = value;
    }

    // Tạo trạng thái hiệu ứng; hệ số HP/KI và sát thương khởi tạo là 1, tức không nhân chỉ số (Java: constructor).
    public EffectSkin(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        Player = player;
        HpKiMultiplier = 1;
        DamageMultiplier = 1;
    }

    private EffectEnvironment Env => environment
        ?? throw new InvalidOperationException("EffectSkin.Environment is not set. The player factory must assign it before the effects are used.");

    private long Now() => Env.Clock.CurrentTimeMillis();

    private bool Due(long lastTime, long miniTimeTarget) => Env.Clock.CanDoWithTime(lastTime, miniTimeTarget);

    private void LogError(Exception e, [CallerMemberName] string method = "") => Env.Logger.LogException(typeof(EffectSkin), e, null, method);

    // Tính khoảng cách giữa hai người chơi theo Util.getDistance của Java.
    private int Distance(Player a, Player b)
    {
        EffectLocation la = Env.Players.GetLocation(a);
        EffectLocation lb = Env.Players.GetLocation(b);
        return Util.GetDistance(la.X, la.Y, lb.X, lb.Y);
    }

    // Lọc mục tiêu aura: khác chủ, không boss, còn sống và trong range; các kiểm tra pl và nPoint khác null của Java không thể thất bại với danh sách zone.
    private bool IsTargetInRange(Player pl, int range)
        => !ReferenceEquals(Player, pl) && !pl.IsBoss && !Env.Players.IsDie(pl) && Distance(Player, pl) <= range;

    private string Pick(string[] texts) => texts[Env.Random.NextInt(0, texts.Length - 1)];

    // Cập nhật hiệu ứng theo mô tả lớp (Java: update).
    public void Update()
    {
        if (disposed)
        {
            return;
        }

        EffectEnvironment env = Env;
        UpdateInvisibleArmor();
        if (!env.Players.IsDie(Player) && env.Players.HasZone(Player) && !env.Rules.IsMapOffline(env.Players.GetMapId(Player)))
        {
            UpdateDirty();
            UpdateFounder();
            UpdateXinBato();
            UpdateCarrotTransformation();
            UpdateRabbitBulma();
            UpdateMafuba();
            UpdateCellHpKiDrain();
            UpdateSkillInvisibility();
            UpdatePetrification();
            UpdateChocolateTransformation();
            UpdateSlow();
            UpdateBlastDamageMultiplier();
            UpdateHalloween();
            UpdateBulmaHairColor();
            UpdateBeachParty();
            UpdateInoHashi();
            UpdateInosuke();
            UpdateNezuko();
            UpdateTanjiro();
            UpdateZenitsu();
            UpdateFideLeader();
            UpdateVirusInfection();
            UpdateSnowflake();
            UpdatePumpkin();
            UpdateDraburaFrost();
            UpdateCoolDamage();
            UpdateAllyHpRecovery();
            UpdateCuteDamage();
        }

        if (!Player.IsBoss && !Player.IsPetFollow && !Player.IsTangMonk && !Player.IsLover)
        {
            UpdateTrainArmor();
        }

        if (HpKiMultiplier != 1 && Due(LastTimeHpKiMultiplier, 1800000))
        {
            HpKiMultiplier = 1;
            env.Notifier.Point(Player);
        }

        if (DamageMultiplier != 1 && Due(LastTimeDamageMultiplier, 1800000))
        {
            DamageMultiplier = 1;
            env.Notifier.Point(Player);
        }

        if (IsChocolate && Due(LastTimeChocolate, TimeChocolate))
        {
            env.Skins.RemoveChocolate(Player);
        }

        if (IsRabbitLeader && Due(LastTimeRabbitLeader, TimeRabbitLeader))
        {
            env.Skins.RemoveRabbitLeader(Player);
        }

        if (IsDraburaFrost && Due(LastTimeDraburaFrost, TimeDraburaFrost))
        {
            env.Skins.RemoveDraburaFrost(Player);
        }

        if (IsSlow && Due(LastTimeSlow, TimeSlow))
        {
            env.Skins.RemoveSlow(Player);
        }

        // Giữ quirk Java: hiệu ứng Demon Slayer hết hạn theo mốc Eff (lần aura chạy gần nhất), không theo LastTimeNezuko/Tanjiro/... do dịch vụ lưu.
        if (IsNezuko && Due(lastTimeEffNezuko, TimeNezuko))
        {
            env.Skins.RemoveRage(Player);
        }

        if (IsTanjiro && Due(lastTimeEffTanjiro, TimeTanjiro))
        {
            env.Skins.RemoveTanjiro(Player);
        }

        if (IsInosuke && Due(lastTimeEffInosuke, TimeInosuke))
        {
            env.Skins.RemoveInosuke(Player);
        }

        if (IsInoHashi && Due(lastTimeEffInoHashi, TimeInoHashi))
        {
            env.Skins.RemoveInosukeHashibira(Player);
        }

        UpdatePirateCostume();
    }

    private void UpdateAllyHpRecovery()
    {
        EffectEnvironment env = Env;
        try
        {
            EffectNPointState np = env.Players.GetNPoint(Player);
            if (np.SelfAndAllyHpRegenPercent > 0 && Due(lastTimeAllyHpRecovery10Seconds, 10_000))
            {
                IReadOnlyList<Player> playersMap = env.Players.GetNotBosses(Player);
                for (int i = playersMap.Count - 1; i >= 0; i--)
                {
                    Player pl = playersMap[i];
                    if (IsTargetInRange(pl, 200))
                    {
                        env.Notifier.RecoverHpMp(pl, np.SelfAndAllyHpRegenPercent, 0);
                    }
                }

                lastTimeAllyHpRecovery10Seconds = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateDraburaFrost()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Inventory.HasDraburaFrostCostume(Player) && Due(lastTimeDraburaFrost, 30_000))
            {
                var targets = new List<Player>();
                foreach (Player pl in env.Players.GetNotBosses(Player))
                {
                    if (IsTargetInRange(pl, 300))
                    {
                        targets.Add(pl);
                    }
                }

                foreach (Player pl in targets)
                {
                    if (env.Random.NextInt(100) < 70)
                    {
                        int time = 5000;
                        env.Skins.SetDraburaFrost(pl, Now(), time);
                        env.Notifier.SendCostume(pl);
                        env.Notifier.Chat(Player, "Phụt!");
                        env.Notifier.Chat(pl, "Hự...");
                        env.Notifier.SendItemTime(pl, 11085, time / 1000);
                    }
                }

                lastTimeDraburaFrost = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdatePumpkin()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetNPoint(Player).IsPumpkinAura && Due(lastTimePumpkin, 10_000))
            {
                env.Notifier.Chat(Player, "Hóa bí đi!");
                IReadOnlyList<Player> playersMap = env.Players.GetNotBosses(Player);
                for (int i = playersMap.Count - 1; i >= 0; i--)
                {
                    Player pl = playersMap[i];
                    if (IsTargetInRange(pl, 200))
                    {
                        env.Notifier.SendItemTime(pl, 7057, 5);
                        env.Notifier.Chat(pl, Pick(PumpkinTalk));
                        env.Skills.SetPumpkin(pl, 5000);
                        env.Notifier.SendInfoHpMpMoney(pl);
                        env.Notifier.SendInfoNv(pl);
                        env.Notifier.Point(pl);
                    }
                }

                lastTimePumpkin = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateSnowflake()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Inventory.HasSnowflake(Player) && Due(lastTimeSnowflake, 7000))
            {
                IReadOnlyList<Player> playersMap = env.Players.GetNotBosses(Player);
                for (int i = playersMap.Count - 1; i >= 0; i--)
                {
                    Player pl = playersMap[i];
                    if (IsTargetInRange(pl, 200))
                    {
                        env.Notifier.SendItemTime(pl, 7066, 5);
                        env.Skills.SetSnowflake(pl, 5000);
                        env.Notifier.Chat(pl, Pick(SnowflakeTalk));
                        env.Notifier.SendInfoHpMpMoney(pl);
                        env.Notifier.SendInfoNv(pl);
                        env.Notifier.Point(pl);
                    }
                }

                lastTimeSnowflake = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateVirusInfection()
    {
        EffectEnvironment env = Env;
        try
        {
            if (Player.EffectSkill.IsVirus && Due(lastTimeVirus, 5000))
            {
                if (env.Random.IsTrueInt(50, 100) && !env.Players.IsDie(Player))
                {
                    env.Notifier.Chat(Player, VirusTalk[env.Random.NextInt(VirusTalk.Length)]);
                }

                if (env.Random.IsTrueInt(10, 100))
                {
                    var targets = new List<Player>();
                    foreach (Player pl in env.Players.GetNotBosses(Player))
                    {
                        if (IsTargetInRange(pl, 200))
                        {
                            targets.Add(pl);
                        }
                    }

                    foreach (Player pl in targets)
                    {
                        if (env.Players.HasFaceMaskItemTime(pl) || env.Inventory.HasFaceMaskAvatar(pl))
                        {
                            return;
                        }

                        if (!pl.EffectSkill.IsVirus)
                        {
                            env.Notifier.SendItemTime(pl, 7143, 300);
                            env.Skills.SetVirus(pl, 300_000);
                            env.Notifier.Point(pl);
                            env.Notifier.SendNotification(pl, "Bạn bị lây virus từ " + Player.Name);
                        }
                    }

                    lastTimeVirus = Now();
                }
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdatePirateCostume()
    {
        EffectEnvironment env = Env;
        EffectSetClothesState clothes = env.Players.GetSetClothes(Player);
        if (clothes.CostumePirate != -1 && env.Players.HasZone(Player) && Due(LastTimePirateCostumeUpdate, 5000))
        {
            int count = 0;
            int[] cts = new int[9];
            cts[clothes.CostumePirate - 618] = clothes.CostumePirate;
            var wearers = new List<Player> { Player };
            try
            {
                foreach (Player pl in env.Players.GetNotBosses(Player))
                {
                    EffectSetClothesState other = env.Players.GetSetClothes(pl);
                    if (!ReferenceEquals(Player, pl) && other.CostumePirate != -1 && Distance(Player, pl) <= 300)
                    {
                        cts[other.CostumePirate - 618] = other.CostumePirate;
                        wearers.Add(pl);
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e);
            }

            for (int i = 0; i < cts.Length; i++)
            {
                if (cts[i] != 0)
                {
                    count++;
                }
            }

            foreach (Player pl in wearers)
            {
                IEffectItem ct = env.Inventory.GetBodyItem(pl, 5);
                if (ct.IsNotNullItem && ct.TemplateId >= 618 && ct.TemplateId <= 626 && ct.Options is { } options)
                {
                    foreach (IEffectItemOption? io in options)
                    {
                        if (io is not null && (io.OptionId == 147 || io.OptionId == 77 || io.OptionId == 103))
                        {
                            io.Param = count * 3;
                        }
                    }
                }

                if (env.Players.IsPl(pl) && Due(LastTimePirateCostumeUpdate, 5000))
                {
                    env.Notifier.SendItemBody(pl);
                }

                pl.EffectSkin.LastTimePirateCostumeUpdate = Now();
            }
        }
    }

    private void UpdateNezuko()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetSetClothes(Player).CostumeNezuko != -1 && Due(lastTimeEffNezuko, 120000))
            {
                env.Notifier.Chat(Player, "Grừuuuuuu............");
                env.Skins.SetRage(Player, Now(), 10000);
                lastTimeEffNezuko = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateTanjiro()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetSetClothes(Player).CostumeTanjiro != -1 && Due(lastTimeEffTanjiro, 120000))
            {
                env.Notifier.Chat(Player, "Hơi Thở của Nước......");
                env.Skins.SetTanjiro(Player, Now(), 15000);
                lastTimeEffTanjiro = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateInoHashi()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetSetClothes(Player).CostumeInosukeHashibira != -1 && Due(lastTimeEffInoHashi, 120000))
            {
                env.Notifier.Chat(Player, "Hơi thở của dã thú....");
                env.Skins.SetInosukeHashibira(Player, Now(), 20000);
                lastTimeEffInoHashi = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateInosuke()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetSetClothes(Player).CostumeInosuke != -1 && Due(lastTimeEffInosuke, 120000))
            {
                env.Notifier.Chat(Player, "Hơi thở của dã thú....");
                env.Skins.SetInosuke(Player, Now(), 20000);
                lastTimeEffInosuke = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateZenitsu()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetSetClothes(Player).CostumeZenitsu != -1 && Due(lastTimeEffZenitsu, 2000))
            {
                env.Notifier.Chat(Player, "Khò khò.......");
                var targets = new List<Player>();
                foreach (Player pl in env.Players.GetNotBosses(Player))
                {
                    if (IsTargetInRange(pl, 200))
                    {
                        targets.Add(pl);
                    }
                }

                foreach (Player pl in targets)
                {
                    env.Notifier.Chat(pl, "Hoang mang quá!");
                }

                lastTimeEffZenitsu = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateCellHpKiDrain()
    {
        EffectEnvironment env = Env;
        try
        {
            EffectNPointState np = env.Players.GetNPoint(Player);
            if (np.Hp < np.HpMax || np.Mp < np.MpMax)
            {
                int param = np.AroundHpMpAbsorptionPercent;
                if (param > 0 && !env.Players.IsDie(Player) && Due(lastTimeCellHpKiDrain, 5000))
                {
                    long hpHut = 0;
                    long mpHut = 0;
                    var targets = new List<Player>();
                    foreach (Player pl in env.Players.GetNotBosses(Player))
                    {
                        if (IsTargetInRange(pl, 200))
                        {
                            targets.Add(pl);
                        }
                    }

                    EffectLocation here = env.Players.GetLocation(Player);
                    foreach (IEffectMobTarget mob in env.Players.GetMobs(Player))
                    {
                        if (mob.Hp > 1 && Util.GetDistance(here.X, here.Y, mob.X, mob.Y) <= 200)
                        {
                            long subHp = CrisGH(unchecked(mob.HpFull * param / 100));
                            if (subHp >= mob.Hp)
                            {
                                subHp = mob.Hp - 1;
                            }

                            hpHut += subHp;
                            mob.Injured(null, subHp, false);
                        }
                    }

                    foreach (Player pl in targets)
                    {
                        EffectNPointState target = env.Players.GetNPoint(pl);
                        long subHp = CrisGH(unchecked(target.HpMax * param / 100));
                        long subMp = CrisGH(unchecked(target.MpMax * param / 100));
                        if (subHp >= target.Hp)
                        {
                            subHp = target.Hp - 1;
                        }

                        if (subMp >= target.Mp)
                        {
                            subMp = target.Mp - 1;
                        }

                        hpHut += subHp;
                        mpHut += subMp;
                        env.Notifier.SendInfoHpMpMoney(pl);
                        env.Notifier.SendInfoNv(pl);
                        env.Players.Injure(pl, null, subHp, true, false);
                    }

                    env.Players.AddHp(Player, hpHut);
                    env.Players.AddMp(Player, mpHut);
                    env.Notifier.SendInfoHpMpMoney(Player);
                    env.Notifier.SendInfoNv(Player);
                    lastTimeCellHpKiDrain = Now();
                }
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private long CrisGH(double a) => Util.CrisGH(a, Env.Settings.ReadInt);

    private void UpdateDirty()
    {
        EffectEnvironment env = Env;
        try
        {
            int param = env.Players.GetNPoint(Player).PercentDirtyHpLoss;
            if (param > 0 && Due(lastTimeDirty, 10000))
            {
                IReadOnlyList<Player> playersMap = env.Players.GetNotBosses(Player);
                for (int i = playersMap.Count - 1; i >= 0; i--)
                {
                    Player pl = playersMap[i];
                    if (IsTargetInRange(pl, 200))
                    {
                        EffectNPointState target = env.Players.GetNPoint(pl);
                        long subHp = CrisGH(unchecked(target.HpMax * param / 100));
                        if (subHp >= target.Hp)
                        {
                            subHp = target.Hp - 1;
                        }

                        env.Notifier.Chat(pl, Pick(DirtyTalk));
                        env.Notifier.SendInfoHpMpMoney(pl);
                        env.Notifier.SendInfoNv(pl);
                        env.Players.Injure(pl, null, subHp, true, false);
                    }
                }

                lastTimeDirty = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateFounder()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetNPoint(Player).IsFounder && Due(lastTimeFounder, 5000))
            {
                IReadOnlyList<Player> playersMap = env.Players.GetNotBosses(Player);
                for (int i = playersMap.Count - 1; i >= 0; i--)
                {
                    Player pl = playersMap[i];
                    if (IsTargetInRange(pl, 500))
                    {
                        env.Notifier.Chat(pl, Pick(TextFounder));
                    }
                }

                lastTimeFounder = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateXinBato()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetNPoint(Player).IsXinbato && Due(lastTimeXinbato, 5000))
            {
                IReadOnlyList<Player> playersMap = env.Players.GetNotBosses(Player);
                for (int i = playersMap.Count - 1; i >= 0; i--)
                {
                    Player pl = playersMap[i];
                    if (IsTargetInRange(pl, 300))
                    {
                        env.Notifier.Chat(pl, Pick(TextXinbato));
                        env.Skills.SetXinbato(pl, 5000);
                    }
                }

                lastTimeXinbato = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateRabbitBulma()
    {
        EffectEnvironment env = Env;
        try
        {
            EffectNPointState np = env.Players.GetNPoint(Player);
            if (np.SexyDamagePercent > 0 && Due(lastTimeRabbitBulma, 10000))
            {
                IReadOnlyList<Player> playersMap = env.Players.GetNotBosses(Player);
                for (int i = playersMap.Count - 1; i >= 0; i--)
                {
                    Player pl = playersMap[i];
                    if (IsTargetInRange(pl, 120))
                    {
                        env.Notifier.Chat(pl, np.IsRabbitBulma ? Pick(RabbitBulmaTalk) : Pick(DamageBuffTalk));

                        // Giữ quirk Java: buff cộng cho chủ, một lần cho mỗi láng giềng.
                        env.Skills.SetDamageBuff(Player, 11000, np.SexyDamagePercent);
                    }
                }

                lastTimeRabbitBulma = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateCoolDamage()
    {
        EffectEnvironment env = Env;
        try
        {
            int tl = env.Players.GetNPoint(Player).CoolDamagePercent;
            if (tl > 0 && Due(lastTimeCoolDamage, 10000))
            {
                IReadOnlyList<Player> playersMap = env.Players.GetNotBosses(Player);
                for (int i = playersMap.Count - 1; i >= 0; i--)
                {
                    Player pl = playersMap[i];
                    if (IsTargetInRange(pl, 200))
                    {
                        env.Skills.SetDamageBuff(Player, 11000, tl);
                    }
                }

                lastTimeCoolDamage = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateCuteDamage()
    {
        EffectEnvironment env = Env;
        try
        {
            int tl = env.Players.GetNPoint(Player).CuteDamagePercent;
            if (tl > 0 && Due(lastTimeCuteDamage, 10000))
            {
                IReadOnlyList<Player> playersMap = env.Players.GetNotBosses(Player);
                for (int i = playersMap.Count - 1; i >= 0; i--)
                {
                    Player pl = playersMap[i];
                    if (IsTargetInRange(pl, 200))
                    {
                        env.Skills.SetDamageBuff(Player, 11000, tl);
                    }
                }

                lastTimeCuteDamage = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateFideLeader()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetSetClothes(Player).CostumeFriezaBoss != -1 && env.Players.HasZone(Player) && Due(LastTimeFideLeader, 10_000))
            {
                IReadOnlyList<Player> playersMap = env.Players.GetNotBosses(Player);
                for (int i = playersMap.Count - 1; i >= 0; i--)
                {
                    Player pl = playersMap[i];
                    if (IsTargetInRange(pl, 500) && env.Players.GetSetClothes(pl).CostumeFriezaMinion != -1 && env.Players.HasZone(pl))
                    {
                        env.Notifier.Chat(pl, "Fide đại ca");
                        env.Skills.SetFideLeader(pl, 10_000);
                    }
                }

                LastTimeFideLeader = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateBulmaHairColor()
    {
        EffectEnvironment env = Env;
        EffectSetClothesState clothes = env.Players.GetSetClothes(Player);
        if (clothes.CostumeBulmaColoredHair != -1 && env.Players.HasZone(Player) && Due(lastTimeBulmaHairColor, 5000))
        {
            int count = 0;
            int[] cts = new int[3];
            cts[clothes.CostumeBulmaColoredHair - 1208] = clothes.CostumeBulmaColoredHair;
            var wearers = new List<Player> { Player };
            try
            {
                foreach (Player pl in env.Players.GetNotBosses(Player))
                {
                    EffectSetClothesState other = env.Players.GetSetClothes(pl);
                    if (!ReferenceEquals(Player, pl) && other.CostumeBulmaColoredHair != -1 && Distance(Player, pl) <= 300)
                    {
                        cts[other.CostumeBulmaColoredHair - 1208] = other.CostumeBulmaColoredHair;
                        wearers.Add(pl);
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e);
            }

            for (int i = 0; i < cts.Length; i++)
            {
                if (cts[i] != 0)
                {
                    count++;
                }
            }

            foreach (Player pl in wearers)
            {
                IEffectItem ct = env.Inventory.GetBodyItem(pl, 5);
                if (ct.IsNotNullItem && ct.TemplateId >= 1208 && ct.TemplateId <= 1210 && ct.Options is { } options)
                {
                    foreach (IEffectItemOption? io in options)
                    {
                        if (io is not null && io.OptionId == 147)
                        {
                            io.Param = Math.Min(count * 3, 10);
                        }
                    }
                }

                if (env.Players.IsPl(pl) && Due(lastTimeBulmaHairColor, 5000))
                {
                    env.Notifier.SendItemBody(pl);
                }

                pl.EffectSkin.lastTimeBulmaHairColor = Now();
            }
        }
    }

    private void UpdateBeachParty()
    {
        EffectEnvironment env = Env;
        EffectSetClothesState clothes = env.Players.GetSetClothes(Player);
        if (clothes.CostumeBeachParty != -1 && env.Players.HasZone(Player) && Due(lastTimeBeachParty, 5000))
        {
            int count = 0;
            int[] cts = new int[3];
            cts[clothes.CostumeBeachParty - 1234] = clothes.CostumeBeachParty;

            var wearers = new List<Player> { Player };

            for (int i = 0; i < cts.Length; i++)
            {
                if (cts[i] != 0)
                {
                    count++;
                }
            }

            try
            {
                foreach (Player pl in env.Players.GetNotBosses(Player))
                {
                    EffectSetClothesState other = env.Players.GetSetClothes(pl);
                    if (!ReferenceEquals(Player, pl) && other.CostumeBeachParty != -1 && Distance(Player, pl) <= 300)
                    {
                        // Chú thích Java nói loại trang phục 1208..1210 nhưng điều kiện kiểm 1234..1236, nên bỏ mọi người mặc chính họ trang phục này.
                        if (other.CostumeBeachParty >= 1234 && other.CostumeBeachParty <= 1236)
                        {
                            continue;
                        }

                        cts[other.CostumeBeachParty - 1234] = other.CostumeBeachParty;
                        wearers.Add(pl);

                        IEffectItem ct = env.Inventory.GetBodyItem(pl, 5);
                        if (ct.IsNotNullItem && ct.TemplateId >= 1234 && ct.TemplateId <= 1236 && ct.Options is { } options)
                        {
                            foreach (IEffectItemOption? io in options)
                            {
                                if (io is not null && io.OptionId == 147)
                                {
                                    io.Param -= Math.Min(count * 5, 15);
                                }
                            }
                        }

                        if (env.Players.IsPl(pl) && Due(lastTimeBeachParty, 5000))
                        {
                            env.Notifier.SendItemBody(pl);
                        }

                        pl.EffectSkin.lastTimeBeachParty = Now();
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }
    }

    private void UpdateSkillInvisibility()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetNPoint(Player).IsInvisible && Due(lastTimeSkillInvisibility, 5000))
            {
                env.Skills.SetInvisible(Player, 1500);
                lastTimeSkillInvisibility = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdatePetrification()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetNPoint(Player).IsPetrified && Due(lastTimePetrification, 30_000))
            {
                IReadOnlyList<Player> playersMap = env.Players.GetNotBosses(Player);
                for (int i = playersMap.Count - 1; i >= 0; i--)
                {
                    Player pl = playersMap[i];
                    if (IsTargetInRange(pl, 200) && env.Random.NextInt(100) < 80)
                    {
                        env.Skills.SetIsStone(pl, 6000);
                        env.Notifier.Chat(Player, "Phẹt.....");
                        env.Notifier.Chat(pl, "hự.....");
                    }
                }

                lastTimePetrification = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateSlow()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetNPoint(Player).WearingBuiBui && Due(LastTimeBuiBui, 3000))
            {
                var targets = new List<Player>();
                foreach (Player pl in env.Players.GetNotBosses(Player))
                {
                    if (IsTargetInRange(pl, 200))
                    {
                        targets.Add(pl);
                    }
                }

                foreach (Player pl in targets)
                {
                    env.Notifier.Chat(pl, "Nặng quá!");
                    env.Skins.SetSlow(pl, Now(), 3000);
                }

                LastTimeBuiBui = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateChocolateTransformation()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetNPoint(Player).WearingMabu && Due(LastTimeMabu, 30000))
            {
                var targets = new List<Player>();
                foreach (Player pl in env.Players.GetNotBosses(Player))
                {
                    if (IsTargetInRange(pl, 200))
                    {
                        targets.Add(pl);
                    }
                }

                foreach (Player pl in targets)
                {
                    env.Notifier.Chat(Player, "Phẹt.....");
                    env.Notifier.Chat(pl, "hự.....");
                    env.Skins.SetChocolate(pl, Now(), 5000);
                    env.Notifier.SendCostume(pl);
                    env.Notifier.SendItemTime(pl, 3780, 5000 / 1000);
                }

                LastTimeMabu = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateCarrotTransformation()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetNPoint(Player).IsRabbitLeader && Due(lastTimeRabbitLeader, 330_000))
            {
                var targets = new List<Player>();
                foreach (Player pl in env.Players.GetNotBosses(Player))
                {
                    if (IsTargetInRange(pl, 200))
                    {
                        targets.Add(pl);
                    }
                }

                foreach (Player pl in targets)
                {
                    env.Skins.SetRabbitLeader(pl, Now(), 300_000);
                    env.Notifier.SendCostume(pl);
                    env.Notifier.Chat(pl, Pick(CarrotTalk));
                    env.Notifier.SendItemTime(pl, 4082, 300_000 / 1000);
                }

                lastTimeRabbitLeader = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateBlastDamageMultiplier()
    {
        EffectEnvironment env = Env;
        try
        {
            if (env.Players.GetNPoint(Player).BlastDamageMultiplier != 0 && Due(LastTimeBlastDamageMultiplier, 60000))
            {
                IsBlastDamageMultiplierReady = true;
                LastTimeBlastDamageMultiplier = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateMafuba()
    {
        EffectEnvironment env = Env;
        try
        {
            EffectSkill skill = Player.EffectSkill;
            if (skill.IsBottled && skill.MafubaUser is { } owner && Due(lastTimeMafuba, 500))
            {
                EffectSkillRef mafuba = env.Players.GetSkillById(owner, 26)
                    ?? throw new InvalidOperationException("The Mafuba owner has no Ma Phong Ba skill (Java: NullPointerException).");
                double param = unchecked(mafuba.Point * (skill.BottledType == 0 ? 1 : 2));
                long subHp = CrisGH(JavaNumbers.ToLong(env.Players.GetNPoint(owner).HpMax * param / 100));
                long hp = env.Players.GetNPoint(Player).Hp;
                if (subHp >= hp)
                {
                    subHp = Math.Abs(hp - 100);
                }

                env.Notifier.SendInfoHpMpMoney(Player);
                env.Notifier.SendInfoNv(Player);
                env.Players.Injure(Player, owner, subHp, true, false);
                lastTimeMafuba = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void UpdateHalloween()
    {
        EffectEnvironment env = Env;
        try
        {
            if (Player.EffectSkill.IsHalloween && Due(lastTimeHalloween, 10000))
            {
                IReadOnlyList<Player> playersMap = env.Players.GetNotBosses(Player);
                for (int i = playersMap.Count - 1; i >= 0; i--)
                {
                    Player pl = playersMap[i];
                    if (!ReferenceEquals(Player, pl) && !pl.EffectSkill.IsHalloween && !pl.IsPet && !pl.IsFather && !pl.IsMother && !env.Players.IsDie(pl)
                        && Distance(Player, pl) <= 200)
                    {
                        env.Skills.SetIsHalloween(pl, -1, 1800000);
                    }
                }

                lastTimeHalloween = Now();
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    // Cập nhật giáp luyện tập: mỗi phút nhàn rỗi khi mặc cộng 1 phút, mỗi phút giảm 1 cho giáp trong túi và rương (Java: updateTrainArmor).
    private void UpdateTrainArmor()
    {
        EffectEnvironment env = Env;

        // Tăng thời gian của giáp luyện tập đang mặc.
        if (Due(LastTimeAddTimeTrainArmor, 60000) && !Due(LastTimeAttack, 30000))
        {
            IEffectItem? worn = env.Inventory.GetTrainArmor(Player);
            if (env.Players.GetNPoint(Player).WearingTrainArmor && worn is not null && worn.Options is { } wornOptions)
            {
                foreach (IEffectItemOption? io in wornOptions)
                {
                    if (io is not null && io.OptionId == 9)
                    {
                        if (io.Param < 1000)
                        {
                            io.Param++;
                            env.Notifier.SendItemBody(Player);
                        }

                        break;
                    }
                }
            }

            LastTimeAddTimeTrainArmor = Now();
        }

        // Giảm thời gian của giáp luyện tập trong túi và rương.
        if (Due(LastTimeSubTimeTrainArmor, 60000))
        {
            foreach (IEffectItem? item in env.Inventory.GetBagItems(Player))
            {
                LowerTrainArmorTime(item);
            }

            foreach (IEffectItem? item in env.Inventory.GetBoxItems(Player))
            {
                LowerTrainArmorTime(item);
            }

            LastTimeSubTimeTrainArmor = Now();
            env.Notifier.SendItemBag(Player);
            env.Notifier.Point(Player);
        }
    }

    private void LowerTrainArmorTime(IEffectItem? item)
    {
        if (item is not null && item.IsNotNullItem && Env.Inventory.IsTrainArmor(item) && item.Options is { } options)
        {
            foreach (IEffectItemOption? io in options)
            {
                if (io is not null && io.OptionId == 9 && io.Param > 0)
                {
                    io.Param--;
                }
            }
        }
    }

    // Cập nhật giáp vô hình: sau 5 giây không đánh và sau 5 giây từ LastTimeInvisible thì tàng hình; đánh làm mất ngay (Java: updateVoHinh).
    private void UpdateInvisibleArmor()
    {
        if (Env.Players.GetNPoint(Player).WearingInvisibilityArmor)
        {
            if (Due(LastTimeAttack, 5000))
            {
                IsInvisible = Due(LastTimeInvisible, 5000);
            }
            else
            {
                IsInvisible = false;
            }
        }
        else
        {
            IsInvisible = false;
        }
    }

    // Dừng hiệu ứng; Java gán player null nhưng C# giữ Player theo Player shell (Java: dispose).
    public void Dispose()
    {
        disposed = true;
    }
}
