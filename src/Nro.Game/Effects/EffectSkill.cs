
namespace Nro.Game.Effects;
// Quản lý trạng thái hiệu ứng kỹ năng có thời hạn của một người chơi, còn EffectSkillService đổi trạng thái và gửi gói; Update kiểm theo thứ tự cố định và có thể kết thúc nhiều hiệu ứng trong một lần gọi.
// Nguồn Java: nro/effect/EffectSkill.java (byte Java là sbyte, mốc epoch là long, thời lượng là int; tên trường chỉ viết hoa chữ đầu theo quy tắc Player shell).
// Hiệu ứng hết khi now - lastTime > duration: đúng tại t+d vẫn còn, lần Update từ t+d+1 mới gỡ; không dùng khóa vì chỉ tick zone sở hữu người chơi chạm tới.
// Khác Java có chủ đích: Environment được factory gán thay singleton gI(); gọi Update, RemoveSkillEffectWhenDie hoặc IsHaveEffectSkill trước đó ném InvalidOperationException; Dispose bỏ tham chiếu đối tượng khác nhưng giữ Player và các lần Update sau không làm gì.
// Setter Player công khai giữ seam @Setter private Player player của kiểm thử Java.
public class EffectSkill
{
    private EffectEnvironment? environment;
    private bool disposed;

    // Hoá Bí Ngô
    // Giá trị trạng thái IsPumpkin của hiệu ứng.
    // (Java: isBiNgo).
    public bool IsPumpkin { get; set; }

    // Giá trị trạng thái LastTimePumpkin của hiệu ứng.
    // (Java: LastTimeBiNgo).
    public long LastTimePumpkin { get; set; }

    // Giá trị trạng thái TimePumpkin của hiệu ứng.
    // (Java: TimeBiNgo).
    public int TimePumpkin { get; set; }

    // Bông tuyết
    // Giá trị trạng thái IsSnowflake của hiệu ứng.
    // (Java: isBongTuyet).
    public bool IsSnowflake { get; set; }

    // Giá trị trạng thái LastTimeSnowflake của hiệu ứng.
    // (Java: LastTimeBongTuyet).
    public long LastTimeSnowflake { get; set; }

    // Giá trị trạng thái TimeSnowflake của hiệu ứng.
    // (Java: TimeBongTuyet).
    public int TimeSnowflake { get; set; }

    // Virus
    // Giá trị trạng thái IsVirus của hiệu ứng.
    public bool IsVirus { get; set; }

    // Giá trị trạng thái LastTimeVirus của hiệu ứng.
    public long LastTimeVirus { get; set; }

    // Giá trị trạng thái TimeVirus của hiệu ứng.
    public int TimeVirus { get; set; }

    // Mabu hold
    // Giá trị trạng thái IsMabuHold của hiệu ứng.
    public bool IsMabuHold { get; set; }

    // PvP Siêu Thần Thủy
    // Giá trị trạng thái IsPvpSuperDivineWater của hiệu ứng.
    // (Java: isPKSTT).
    public bool IsPvpSuperDivineWater { get; set; }

    // Giá trị trạng thái LastTimePvpSuperDivineWater của hiệu ứng.
    // (Java: lastTimePKSTT).
    public long LastTimePvpSuperDivineWater { get; set; }

    // Giá trị trạng thái TimePvpSuperDivineWater của hiệu ứng.
    // (Java: timePKSTT).
    public int TimePvpSuperDivineWater { get; set; }

    // PvP Commeson
    // Giá trị trạng thái IsPvpCommeson của hiệu ứng.
    // (Java: isPKCommeson).
    public bool IsPvpCommeson { get; set; }

    // Giá trị trạng thái LastTimePvpCommeson của hiệu ứng.
    // (Java: lastTimePKCommeson).
    public long LastTimePvpCommeson { get; set; }

    // Giá trị trạng thái TimePvpCommeson của hiệu ứng.
    // (Java: timePKCommeson).
    public int TimePvpCommeson { get; set; }

    // Bình (Mafuba)
    // Giá trị trạng thái IsBottled của hiệu ứng.
    // (Java: isBinh).
    public bool IsBottled { get; set; }

    // Giá trị trạng thái BottledType của hiệu ứng.
    // (Java: typeBinh).
    public int BottledType { get; set; }

    // Giá trị trạng thái LastTimeBottled của hiệu ứng.
    // (Java: lastTimeUpBinh).
    public long LastTimeBottled { get; set; }

    // Giá trị trạng thái TimeBottled của hiệu ứng.
    // (Java: timeBinh).
    public int TimeBottled { get; set; }

    // Giá trị trạng thái MafubaUser của hiệu ứng.
    // (Java: playerUseMafuba).
    public Player? MafubaUser { get; set; }

    // Đang dùng Mafuba
    // Giá trị trạng thái IsUsingMafuba của hiệu ứng.
    // (Java: isUseMafuba).
    public bool IsUsingMafuba { get; set; }

    // Giá trị trạng thái LastTimeUsingMafuba của hiệu ứng.
    // (Java: lastTimeUseMafuba).
    public long LastTimeUsingMafuba { get; set; }

    // Giá trị trạng thái TimeUsingMafuba của hiệu ứng.
    // (Java: timeUseMafuba).
    public int TimeUsingMafuba { get; set; }

    // Intrinsic
    // Giá trị trạng thái IsIntrinsic của hiệu ứng.
    public bool IsIntrinsic { get; set; }

    // Giá trị trạng thái LastTimeUseSkill của hiệu ứng.
    public long LastTimeUseSkill { get; set; }

    // Giá trị trạng thái SkillId của hiệu ứng.
    // (Java: skillID).
    public int SkillId { get; set; }

    // Giá trị trạng thái Cooldown của hiệu ứng.
    public int Cooldown { get; set; }

    // Intrinsic vip
    // Giá trị trạng thái IsIntrinsicVip của hiệu ứng.
    public bool IsIntrinsicVip { get; set; }

    // Giá trị trạng thái LastTimeUseSkillVip của hiệu ứng.
    public long LastTimeUseSkillVip { get; set; }

    // Giá trị trạng thái SkillIdVip của hiệu ứng.
    // (Java: skillIDVip).
    public int SkillIdVip { get; set; }

    // Giá trị trạng thái CooldownVip của hiệu ứng.
    public int CooldownVip { get; set; }

    // Hóa đá
    // Giá trị trạng thái IsStone của hiệu ứng.
    public bool IsStone { get; set; }

    // Giá trị trạng thái LastTimeStone của hiệu ứng.
    public long LastTimeStone { get; set; }

    // Giá trị trạng thái TimeStone của hiệu ứng.
    public int TimeStone { get; set; }

    // Tàng hình
    // Giá trị trạng thái IsInvisible của hiệu ứng.
    // (Java: isTanHinh).
    public bool IsInvisible { get; set; }

    // Giá trị trạng thái LastTimeInvisible của hiệu ứng.
    // (Java: lastTimeTanHinh).
    public long LastTimeInvisible { get; set; }

    // Giá trị trạng thái TimeInvisible của hiệu ứng.
    // (Java: timeTanHinh).
    public int TimeInvisible { get; set; }

    // Buff sát thương
    // Giá trị trạng thái IsDamageBuff của hiệu ứng.
    // (Java: isDameBuff).
    public bool IsDamageBuff { get; set; }

    // Giá trị trạng thái LastTimeDamageBuff của hiệu ứng.
    // (Java: lastTimeDameBuff).
    public long LastTimeDamageBuff { get; set; }

    // Giá trị trạng thái TimeDamageBuff của hiệu ứng.
    // (Java: timeDameBuff).
    public int TimeDamageBuff { get; set; }

    // Giá trị trạng thái DamageBuffPercent của hiệu ứng.
    // (Java: tileDameBuff).
    public int DamageBuffPercent { get; set; }

    // Fide đại ca
    // Giá trị trạng thái IsFideLeader của hiệu ứng.
    // (Java: isFideDaiCa).
    public bool IsFideLeader { get; set; }

    // Giá trị trạng thái LastTimeFideLeader của hiệu ứng.
    // (Java: LastTimeFideDaiCa).
    public long LastTimeFideLeader { get; set; }

    // Giá trị trạng thái TimeFideLeader của hiệu ứng.
    // (Java: TimeFideDaiCa).
    public int TimeFideLeader { get; set; }

    // Xinbato
    // Giá trị trạng thái IsXinbato của hiệu ứng.
    public bool IsXinbato { get; set; }

    // Giá trị trạng thái LastTimeXinbato của hiệu ứng.
    public long LastTimeXinbato { get; set; }

    // Giá trị trạng thái TimeXinbato của hiệu ứng.
    public int TimeXinbato { get; set; }

    // Halloween
    // Giá trị trạng thái IsHalloween của hiệu ứng.
    public bool IsHalloween { get; set; }

    // Giá trị trạng thái LastTimeHalloween của hiệu ứng.
    public long LastTimeHalloween { get; set; }

    // Giá trị trạng thái TimeHalloween của hiệu ứng.
    public int TimeHalloween { get; set; }

    // Giá trị trạng thái HalloweenOutfitId của hiệu ứng.
    // (Java: idOutfitHalloween).
    public int HalloweenOutfitId { get; set; }

    // Chibi
    // Giá trị trạng thái IsChibi của hiệu ứng.
    public bool IsChibi { get; set; }

    // Giá trị trạng thái LastTimeChibi của hiệu ứng.
    public long LastTimeChibi { get; set; }

    // Giá trị trạng thái TimeChibi của hiệu ứng.
    public int TimeChibi { get; set; }

    // Thái dương hạ san
    // Giá trị trạng thái IsStun của hiệu ứng.
    public bool IsStun { get; set; }

    // Giá trị trạng thái LastTimeStartStun của hiệu ứng.
    public long LastTimeStartStun { get; set; }

    // Giá trị trạng thái TimeStun của hiệu ứng.
    public int TimeStun { get; set; }

    // Khiên năng lượng
    // Giá trị trạng thái IsShielding của hiệu ứng.
    public bool IsShielding { get; set; }

    // Giá trị trạng thái LastTimeShieldUp của hiệu ứng.
    public long LastTimeShieldUp { get; set; }

    // Giá trị trạng thái TimeShield của hiệu ứng.
    public int TimeShield { get; set; }

    // Biến khỉ
    // Giá trị trạng thái IsMonkey của hiệu ứng.
    public bool IsMonkey { get; set; }

    // Giá trị trạng thái LevelMonkey của hiệu ứng.
    public sbyte LevelMonkey { get; set; }

    // Giá trị trạng thái LastTimeUpMonkey của hiệu ứng.
    public long LastTimeUpMonkey { get; set; }

    // Giá trị trạng thái TimeMonkey của hiệu ứng.
    public int TimeMonkey { get; set; }

    // Use skill monkey
    // Giá trị trạng thái IsUseSkillMonkey của hiệu ứng.
    public bool IsUseSkillMonkey { get; set; }

    // Giá trị trạng thái LastTimeUseSkillMonkey của hiệu ứng.
    public long LastTimeUseSkillMonkey { get; set; }

    // Giá trị trạng thái TimeUseSkillMonkey của hiệu ứng.
    public int TimeUseSkillMonkey { get; set; }

    // Tái tạo năng lượng
    // Giá trị trạng thái IsCharging của hiệu ứng.
    public bool IsCharging { get; set; }

    // Giá trị trạng thái CountCharging của hiệu ứng.
    public int CountCharging { get; set; }

    // Huýt sáo
    // Giá trị trạng thái HpWhistlePercent của hiệu ứng.
    // (Java: tiLeHPHuytSao).
    public int HpWhistlePercent { get; set; }

    // Giá trị trạng thái LastTimeWhistle của hiệu ứng.
    // (Java: lastTimeHuytSao).
    public long LastTimeWhistle { get; set; }

    // Thôi miên
    // Giá trị trạng thái IsHypnosis của hiệu ứng.
    // (Java: isThoiMien).
    public bool IsHypnosis { get; set; }

    // Giá trị trạng thái LastTimeHypnosis của hiệu ứng.
    // (Java: lastTimeThoiMien).
    public long LastTimeHypnosis { get; set; }

    // Giá trị trạng thái TimeHypnosis của hiệu ứng.
    // (Java: timeThoiMien).
    public int TimeHypnosis { get; set; }

    // Trói
    // Giá trị trạng thái IsBinding của hiệu ứng.
    // (Java: useTroi).
    public bool IsBinding { get; set; }

    // Giá trị trạng thái IsBound của hiệu ứng.
    // (Java: anTroi).
    public bool IsBound { get; set; }

    // Giá trị trạng thái LastTimeBinding của hiệu ứng.
    // (Java: lastTimeTroi).
    public long LastTimeBinding { get; set; }

    // Giá trị trạng thái TimeBinding của hiệu ứng.
    // (Java: timeTroi).
    public int TimeBinding { get; set; }

    // Giá trị trạng thái BindingPlayer của hiệu ứng.
    // (Java: plTroi).
    public Player? BindingPlayer { get; set; }

    // Giá trị trạng thái BoundPlayer của hiệu ứng.
    // (Java: plAnTroi).
    public Player? BoundPlayer { get; set; }

    // Quái bị trói mà hiệu ứng dùng qua cổng IEffectMobTarget (Java: mobAnTroi).
    public IEffectMobTarget? BoundMob { get; set; }

    // Mù do Dịch chuyển tức thời.
    // Cờ bật hiệu ứng mù do Dịch chuyển tức thời.
    // (Java: isBlindDCTT).
    public bool IsTeleportBlind { get; set; }

    // Mốc bắt đầu hiệu ứng mù do Dịch chuyển tức thời.
    // (Java: lastTimeBlindDCTT).
    public long LastTimeTeleportBlind { get; set; }

    // Thời lượng hiệu ứng mù do Dịch chuyển tức thời.
    // (Java: timeBlindDCTT).
    public int TimeTeleportBlind { get; set; }

    // Biến hình Cumber.
    // Cờ bật biến hình Cumber (Java: iscumber).
    public bool IsCumber { get; set; }

    // Mốc bắt đầu biến hình Cumber (Java: lastTimeUpcumber).
    public long LastTimeCumber { get; set; }

    // Thời lượng biến hình Cumber (Java: timecumber).
    public int TimeCumber { get; set; }

    // Biến hình Cumber 2.
    // Cờ bật biến hình Cumber 2 (Java: iscumber2).
    public bool IsCumber2 { get; set; }

    // Mốc bắt đầu biến hình Cumber 2 (Java: lastTimeUpcumber2).
    public long LastTimeCumber2 { get; set; }

    // Thời lượng biến hình Cumber 2 (Java: timecumber2).
    public int TimeCumber2 { get; set; }

    // Biến hình Pain.
    // Cờ bật biến hình Pain (Java: isPain).
    public bool IsPain { get; set; }

    // Mốc bắt đầu biến hình Pain (Java: lastTimeUpPain).
    public long LastTimeUpPain { get; set; }

    // Thời lượng biến hình Pain (Java: timePain).
    public int TimePain { get; set; }

    // Biến hình Kefla.
    // Cờ bật biến hình Kefla (Java: iskefla).
    public bool IsKefla { get; set; }

    // Mốc bắt đầu biến hình Kefla (Java: lastTimeUpkefla).
    public long LastTimeKefla { get; set; }

    // Thời lượng biến hình Kefla (Java: timekefla).
    public int TimeKefla { get; set; }

    // Sôcôla.
    // Cờ bật hiệu ứng Sôcôla (Java: isSocola).
    public bool IsChocolate { get; set; }

    // Mốc bắt đầu hiệu ứng Sôcôla (Java: lastTimeSocola).
    public long LastTimeChocolate { get; set; }

    // Thời lượng hiệu ứng Sôcôla (Java: timeSocola).
    public int TimeChocolate { get; set; }

    // Số lần Pem 1 HP (Java: countPem1hp).
    public int OneHpHitCount { get; set; }

    // Biến hình Super SSJ.
    // Cờ bật biến hình Super SSJ (Java: isTranformation).
    public bool IsTransformation { get; set; }

    // Cấp biến hình Super SSJ (Java: levelTranformation).
    public sbyte TransformationLevel { get; set; }

    // Mốc bắt đầu biến hình Super SSJ (Java: lastTimeTranformation).
    public long LastTimeTransformation { get; set; }

    // Thời lượng biến hình Super SSJ (Java: timeTranformation).
    public int TimeTransformation { get; set; }

    // Cờ bật tiến hóa (Java: isEvolution).
    public bool IsEvolution { get; set; }

    // Cấp tiến hóa (Java: levelEvolution).
    public sbyte LevelEvolution { get; set; }

    // Mốc bắt đầu tiến hóa (Java: lastTimeEvolution).
    public long LastTimeEvolution { get; set; }

    // Thời lượng tiến hóa (Java: timeEvolution).
    public int TimeEvolution { get; set; }

    // Sức mạnh bộc phá.
    // Cờ bật sức mạnh bộc phá (Java: isSUcManhBocPha).
    public bool IsPowerBurst { get; set; }

    // Mốc bắt đầu sức mạnh bộc phá (Java: lastTimeSUcManhBocPhaUp).
    public long LastTimePowerBurst { get; set; }

    // Thời lượng sức mạnh bộc phá (Java: timeSUcManhBocPha).
    public int TimePowerBurst { get; set; }

    // Cờ kỹ thuật đổi thân xác (Java: isBodyChangeTechnique).
    public bool IsBodyChangeTechnique { get; set; }

    // Người chơi sở hữu các hiệu ứng này (Java: player).
    public Player Player { get; set; }

    // Dịch vụ hiệu ứng và đồng hồ, phải được factory gán trước khi dùng (Java: environment).
    public EffectEnvironment? Environment
    {
        get => environment;
        set => environment = value;
    }

    // Khởi tạo trạng thái hiệu ứng cho người chơi.
    public EffectSkill(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        Player = player;
    }

    private bool Due(long lastTime, long miniTimeTarget) => Env.Clock.CanDoWithTime(lastTime, miniTimeTarget);

    private EffectEnvironment Env => environment
        ?? throw new InvalidOperationException("EffectSkill.Environment is not set. The player factory must assign it before the effects are used.");

    // Gỡ mọi hiệu ứng không được giữ lại sau khi chết theo thứ tự Java (Java: removeSkillEffectWhenDie).
    public void RemoveSkillEffectWhenDie()
    {
        if (disposed)
        {
            return;
        }

        EffectEnvironment env = Env;
        IEffectSkillService service = env.Skills;
        if (IsMonkey)
        {
            service.MonkeyDown(Player);
        }

        if (IsUseSkillMonkey)
        {
            service.FinishUseMonkey(Player);
        }

        if (IsBottled)
        {
            service.RemoveBottled(Player);
        }

        if (IsTransformation)
        {
            service.RemoveTransformation(Player);
        }

        if (IsEvolution)
        {
            service.RemoveEvolution(Player);
        }

        if (IsPowerBurst)
        {
            service.RemovePowerBurst(Player);
            switch (Player.Gender)
            {
                case 0:
                    env.Notifier.RemoveItemTime(Player, 11997);
                    break;
                case 1:
                    env.Notifier.RemoveItemTime(Player, 12000);
                    break;
                case 2:
                    env.Notifier.RemoveItemTime(Player, 12003);
                    break;
                default:
                    break;
            }
        }

        if (IsShielding)
        {
            service.RemoveShield(Player);
            env.Notifier.RemoveItemTime(Player, 3784);
        }

        if (IsBinding)
        {
            service.RemoveBinding(Player);
        }

        if (IsStun)
        {
            service.RemoveStun(Player);
        }

        if (IsHypnosis)
        {
            service.RemoveHypnosis(Player);
        }

        if (IsTeleportBlind)
        {
            service.RemoveTeleportBlind(Player);
        }

        if (IsDamageBuff)
        {
            service.RemoveDamageBuff(Player);
        }

        if (IsXinbato)
        {
            service.RemoveXinbato(Player);
        }

        if (IsFideLeader)
        {
            service.RemoveFideLeader(Player);
        }

        if (IsStone)
        {
            service.RemoveStone(Player);
        }

        if (IsInvisible)
        {
            service.RemoveInvisibility(Player);
        }

        if (IsMabuHold)
        {
            service.RemoveMabuHold(Player);
        }

        if (IsVirus)
        {
            service.RemoveVirus(Player);
        }

        if (IsSnowflake)
        {
            service.RemoveSnowflake(Player);
        }

        if (IsPumpkin)
        {
            service.RemovePumpkin(Player);
        }
    }

    // Kết thúc theo thứ tự Java mọi hiệu ứng hết hạn trong mỗi tick zone; mỗi kiểm tra đọc lại cờ sau lần gỡ trước và không phụ thuộc người chơi chết hay map offline (Java: update).
    public void Update()
    {
        if (disposed)
        {
            return;
        }

        EffectEnvironment env = Env;
        IEffectSkillService service = env.Skills;
        if (IsMonkey && Due(LastTimeUpMonkey, TimeMonkey))
        {
            service.MonkeyDown(Player);
        }

        if (IsUseSkillMonkey && Due(LastTimeUseSkillMonkey, TimeUseSkillMonkey))
        {
            service.FinishUseMonkey(Player);
        }

        if (IsPvpSuperDivineWater && Due(LastTimePvpSuperDivineWater, TimePvpSuperDivineWater))
        {
            service.RemovePvpSuperDivineWater(Player);
        }

        if (IsPvpCommeson && Due(LastTimePvpCommeson, TimePvpCommeson))
        {
            service.RemovePvpCommeson(Player);
        }

        if (IsBottled && Due(LastTimeBottled, TimeBottled))
        {
            service.RemoveBottled(Player);
        }

        if (IsUsingMafuba && Due(LastTimeUsingMafuba, TimeUsingMafuba))
        {
            service.FinishUseMafuba(Player);
        }

        if (IsDamageBuff && Due(LastTimeDamageBuff, TimeDamageBuff))
        {
            service.RemoveDamageBuff(Player);
        }

        if (IsXinbato && Due(LastTimeXinbato, TimeXinbato))
        {
            service.RemoveXinbato(Player);
        }

        if (IsFideLeader && Due(LastTimeFideLeader, TimeFideLeader))
        {
            service.RemoveFideLeader(Player);
        }

        if (IsHalloween && Due(LastTimeHalloween, TimeHalloween))
        {
            service.RemoveHalloween(Player);
        }

        if (IsChibi && Due(LastTimeChibi, TimeChibi))
        {
            service.RemoveChibiBird(Player);
        }

        if (IsCumber && Due(LastTimeCumber, TimeCumber))
        {
            service.RemoveCumber(Player);
        }

        if (IsCumber2 && Due(LastTimeCumber2, TimeCumber2))
        {
            service.RemoveCumber2(Player);
        }

        if (IsPain && Due(LastTimeUpPain, TimePain))
        {
            service.RemovePain(Player);
        }

        if (IsKefla && Due(LastTimeKefla, TimeKefla))
        {
            service.RemoveKefla(Player);
        }

        if (IsShielding && Due(LastTimeShieldUp, TimeShield))
        {
            service.RemoveShield(Player);
        }

        // Giữ quirk Java: biến hình tại đây luôn kéo dài 300000 ms, bất kể TimeTransformation.
        if (IsTransformation && Due(LastTimeTransformation, 300000))
        {
            service.RemoveTransformation(Player);
        }

        if (IsEvolution && Due(LastTimeEvolution, TimeEvolution))
        {
            service.RemoveEvolution(Player);
        }

        if (IsPowerBurst && Due(LastTimePowerBurst, TimePowerBurst))
        {
            service.RemovePowerBurst(Player);
        }

        // Điều kiện Java: (useTroi && timeUp) || (plAnTroi != null && plAnTroi.isDie()) || (useTroi && isHaveEffectSkill()).
        if ((IsBinding && Due(LastTimeBinding, TimeBinding))
            || (BoundPlayer is not null && env.Players.IsDie(BoundPlayer))
            || (IsBinding && IsHaveEffectSkill()))
        {
            service.RemoveBinding(Player);
        }

        if (IsStun && Due(LastTimeStartStun, TimeStun))
        {
            service.RemoveStun(Player);
        }

        if (IsHypnosis && Due(LastTimeHypnosis, TimeHypnosis))
        {
            service.RemoveHypnosis(Player);
        }

        if (IsTeleportBlind && Due(LastTimeTeleportBlind, TimeTeleportBlind))
        {
            service.RemoveTeleportBlind(Player);
        }

        if (IsChocolate && Due(LastTimeChocolate, TimeChocolate))
        {
            service.RemoveChocolate(Player);
        }

        // Java quy định hiệu ứng Huýt Sáo kéo dài 30 giây.
        if (HpWhistlePercent != 0 && Due(LastTimeWhistle, 30000))
        {
            service.RemoveWhistle(Player);
        }

        if (IsStone && Due(LastTimeStone, TimeStone))
        {
            service.RemoveStone(Player);
        }

        if (IsInvisible && Due(LastTimeInvisible, TimeInvisible))
        {
            service.RemoveInvisibility(Player);
        }

        if (IsIntrinsic && Due(LastTimeUseSkill, Cooldown))
        {
            service.ReleaseCooldownSkillByIntrinsic(Player);
        }

        if (IsIntrinsicVip && Due(LastTimeUseSkillVip, CooldownVip))
        {
            service.ReleaseCooldownSkillByIntrinsicVip(Player);
        }

        if (IsVirus && Due(LastTimeVirus, TimeVirus))
        {
            service.RemoveVirus(Player);
        }

        if (IsSnowflake && Due(LastTimeSnowflake, TimeSnowflake))
        {
            service.RemoveSnowflake(Player);
        }

        if (IsPumpkin && Due(LastTimePumpkin, TimePumpkin))
        {
            service.RemovePumpkin(Player);
        }
    }

    // Trả đúng khi người chơi còn sống và bị hiệu ứng ngăn hành động: choáng, mù, trói, thôi miên, hóa đá, Mabu hold hoặc chờ biến khỉ (Java: isHaveEffectSkill).
    public bool IsHaveEffectSkill()
    {
        return (IsStun || IsTeleportBlind || IsBound || IsHypnosis || IsStone || IsMabuHold || IsUseSkillMonkey) && !Env.Players.IsDie(Player);
    }

    // Dừng hiệu ứng và bỏ tham chiếu người chơi/quái khác để thu gom; Java còn gán player null nhưng C# giữ Player theo Player shell (Java: dispose).
    public void Dispose()
    {
        disposed = true;
        BoundPlayer = null;
        BindingPlayer = null;
        BoundMob = null;
        MafubaUser = null;
    }
}
