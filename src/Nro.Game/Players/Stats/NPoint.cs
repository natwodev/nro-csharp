using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Players.Stats;

// Lưu chỉ số nhân vật và các trình truy cập nhỏ; các phép tính được tách sang Players/Stats. byte Java là sbyte và kiểu số nguyên giữ nguyên để bảo toàn tràn số.
// Nguồn Java: nro/player/NPoint.java (Host cung cấp bản chụp để lớp chỉ làm số học; Clock/Rand thay System.currentTimeMillis/Util.rand; không an toàn luồng theo D1; Dispose chỉ xóa danh sách và giữ Player khác Java).


public partial class NPoint : IDisposable
{

    // Giới hạn tối đa MAX_LIMIT của Java.

    public const sbyte MAX_LIMIT = 13;

    private long lastTimeHoiPhuc;
    private long lastTimeHoiPhuc10s;
    private long lastTimeHoiStamina;

    // Chỉ GetDamageAttack làm mới intrinsic; SetOptions và ApplyIntrinsicGoldPercent dùng nội tại lần đánh trước, null trước lần đầu như Java.
    private NPointIntrinsic? intrinsic;

    private int percentDameIntrinsic;

    // Khởi tạo NPoint.

    public NPoint(Player player)
    {
        Player = player;
    }

    // Giữ player của Java, gồm cả setter Lombok.

    public Player Player { get; set; }

    // Cung cấp phần còn lại của game; mặc định chỉ đọc Player shell đến khi dịch vụ Đợt 3 thay thế.

    public INPointHost Host { get; set; } = PlayerNPointHost.Shared;

    // Nguồn System.currentTimeMillis(); kiểm thử có thể thay bằng TimeProvider giả.

    public VietnamClock Clock { get; set; } = new();

    // Nguồn ngẫu nhiên thay Util.rand static của Java; kiểm thử có thể inject nguồn có hạt giống hoặc kịch bản.

    public RandomUtil Rand { get; set; } = new(SystemRandomProvider.Instance);

    // Lưu trạng thái IsCritical (Java: isCrit).

    public bool IsCritical { get; set; }

    // Lưu trạng thái IsGuaranteedCritical (Java: isCrit100).

    public bool IsGuaranteedCritical { get; set; }

    // Lưu trạng thái IsTeleportCritical (Java: isCritTele).

    public bool IsTeleportCritical { get; set; }

    // Lưu trạng thái DamageAfter (Java: dameAfter).

    public long DamageAfter { get; set; }

    // Giữ ngưỡng cấp sức mạnh; null cho tới InitPowerLimit (Java: powerLimit).

    public PowerLimitInfo? PowerLimit { get; set; }

    // Lưu trạng thái NumAttack (Java: numAttack).

    public sbyte NumAttack { get; set; }

    // Lưu trạng thái Stamina (Java: stamina).

    public int Stamina { get; set; }

    // Lưu trạng thái MaxStamina (Java: maxStamina).

    public int MaxStamina { get; set; }

    // Lưu trạng thái LimitPower (Java: limitPower).

    public sbyte LimitPower { get; set; }

    // Lưu trạng thái Power (Java: power).

    public long Power { get; set; }

    // Lưu trạng thái Potential (Java: tiemNang).

    public long Potential { get; set; }

    // Lưu trạng thái Hp (Java: hp).

    public long Hp { get; set; }

    // Lưu trạng thái HpMax (Java: hpMax).

    public long HpMax { get; set; }

    // Lưu trạng thái BaseHp (Java: hpg).

    public long BaseHp { get; set; }

    // Lưu trạng thái Mp (Java: mp).

    public long Mp { get; set; }

    // Lưu trạng thái MpMax (Java: mpMax).

    public long MpMax { get; set; }

    // Lưu trạng thái BaseMp (Java: mpg).

    public long BaseMp { get; set; }

    // Lưu trạng thái Damage (Java: dame).

    public long Damage { get; set; }

    // Lưu trạng thái BaseDamage (Java: dameg).

    public long BaseDamage { get; set; }

    // Lưu trạng thái Defense (Java: def).

    public int Defense { get; set; }

    // Lưu trạng thái BaseDefense (Java: defg).

    public int BaseDefense { get; set; }

    // Lưu trạng thái CriticalChance (Java: crit).

    public int CriticalChance { get; set; }

    // Lưu trạng thái BaseCriticalChance (Java: critg).

    public int BaseCriticalChance { get; set; }

    // Lưu trạng thái Speed (Java: speed).

    public sbyte Speed { get; set; } = 5;

    // Lưu trạng thái Teleport (Java: teleport).

    public bool Teleport { get; set; }

    // Lưu trạng thái IsSolarFlareResistant (Java: khangTDHS).

    public bool IsSolarFlareResistant { get; set; }

    // Lưu trạng thái IsBoneTransformationImmune (Java: khangHoaXuong).

    public bool IsBoneTransformationImmune { get; set; }

    // Lưu trạng thái IsPumpkinAura (Java: isHoaBiNgoXungQuanh).

    public bool IsPumpkinAura { get; set; }

    // Lưu trạng thái WearingMabu (Java: wearingMabu).

    public bool WearingMabu { get; set; }

    // Lưu trạng thái WearingBuiBui (Java: wearingBuiBui).

    public bool WearingBuiBui { get; set; }

    // Lưu trạng thái IsFusionActive (Java: isTacDungHopThe).

    public bool IsFusionActive { get; set; }

    // Giữ các phần trăm tốc độ, áp dụng nối tiếp lên Speed (Java: tlSpeed).

    public List<int> SpeedPercent { get; set; } = [];

    // Giữ tỷ lệ may mắn (Java: tlMayman).

    public int LuckPercent { get; set; }

    // Lưu trạng thái HpBonus (Java: hpAdd).

    public int HpBonus { get; set; }

    // Lưu trạng thái MpBonus (Java: mpAdd).

    public int MpBonus { get; set; }

    // Lưu trạng thái DamageBonus (Java: dameAdd).

    public int DamageBonus { get; set; }

    // Lưu trạng thái DefenseBonus (Java: defAdd).

    public int DefenseBonus { get; set; }

    // Lưu trạng thái CriticalChanceBonus (Java: critAdd).

    public int CriticalChanceBonus { get; set; }

    // Lưu trạng thái HpRegenBonus (Java: hpHoiAdd).

    public int HpRegenBonus { get; set; }

    // Lưu trạng thái MpRegenBonus (Java: mpHoiAdd).

    public int MpRegenBonus { get; set; }

    // Giữ các phần trăm sát thương chí mạng; tổng ở CriticalDamagePercent (Java: tlDameCrit).

    public List<int> CriticalDamagePercents { get; set; } = [];

    // Lưu trạng thái CriticalDamagePercent (Java: tlSDCM).

    public int CriticalDamagePercent { get; set; }

    // Giữ các phần trăm HP (Java: tlHp).

    public List<int> HpPercents { get; set; } = [];

    // Lưu trạng thái MpPercents (Java: tlMp).

    public List<int> MpPercents { get; set; } = [];

    // Giữ các phần trăm phòng thủ (Java: tlDef).

    public List<int> DefensePercents { get; set; } = [];

    // Giữ phần trăm sát thương, còn MobAttackDamagePercents là sát thương với quái.

    public List<int> DamagePercents { get; set; } = [];

    // Lưu trạng thái MobAttackDamagePercents (Java: tlDameAttMob).

    public List<int> MobAttackDamagePercents { get; set; } = [];

    // Giữ HP/KI hồi mỗi 30 giây và KI truyền do tùy chọn Cute.

    public long HpRegen { get; set; }

    // Lưu trạng thái MpRegen (Java: mpHoi).

    public long MpRegen { get; set; }

    // Lưu trạng thái CuteMpRegen (Java: mpHoiCute).

    public long CuteMpRegen { get; set; }

    // Lưu trạng thái HpRegenPercent (Java: tlHpHoi).

    public int HpRegenPercent { get; set; }

    // Lưu trạng thái MpRegenPercent (Java: tlMpHoi).

    public int MpRegenPercent { get; set; }

    // Lưu trạng thái SelfAndAllyHpRegenPercent (Java: tlHpHoiBanthan_DongMinh).

    public int SelfAndAllyHpRegenPercent { get; set; }

    // Lưu trạng thái SelfAndTeamHpRegenPercent (Java: tlHpHoiBanThanVaDongDoi).

    public int SelfAndTeamHpRegenPercent { get; set; }

    // Lưu trạng thái SelfAndTeamMpRegenPercent (Java: tlMpHoiBanThanVaDongDoi).

    public int SelfAndTeamMpRegenPercent { get; set; }

    // Lưu trạng thái HpAbsorptionPercent (Java: tlHutHp).

    public int HpAbsorptionPercent { get; set; }

    // Lưu trạng thái MpAbsorptionPercent (Java: tlHutMp).

    public int MpAbsorptionPercent { get; set; }

    // Lưu trạng thái MobHpAbsorptionPercent (Java: tlHutHpMob).

    public int MobHpAbsorptionPercent { get; set; }

    // Lưu trạng thái AroundHpMpAbsorptionPercent (Java: tlHutHpMpXQ).

    public int AroundHpMpAbsorptionPercent { get; set; }

    // Lưu trạng thái ControlResistancePercent (Java: tlFixStun).

    public int ControlResistancePercent { get; set; }

    // Lưu trạng thái CuteDamagePercent (Java: tlCuteAddame).

    public int CuteDamagePercent { get; set; }

    // Giữ tỷ lệ phản sát thương (Java: tlPST).

    public int ReflectDamagePercent { get; set; }

    // Giữ các phần trăm nhận tiềm năng/sức mạnh (Java: tlTNSM).

    public List<int> PowerPotentialPercents { get; set; } = [];

    // Lưu trạng thái GoldPercent (Java: tlGold).

    public int GoldPercent { get; set; }

    // Lưu trạng thái DodgePercent (Java: tlNeDon).

    public int DodgePercent { get; set; }

    // Lưu trạng thái BombPercent (Java: tlBom).

    public int BombPercent { get; set; }

    // Lưu trạng thái ArmorPercent (Java: tlGiap).

    public int ArmorPercent { get; set; }

    // Lưu trạng thái MeleeArmorPenetrationPercent (Java: tlxgcc).

    public int MeleeArmorPenetrationPercent { get; set; }

    // Lưu trạng thái RangedArmorPenetrationPercent (Java: tlxgc).

    public int RangedArmorPenetrationPercent { get; set; }

    // Lưu trạng thái AccuracyPercent (Java: tlchinhxac).

    public int AccuracyPercent { get; set; }

    // Lưu trạng thái TrueDamagePercent (Java: tlstc).

    public int TrueDamagePercent { get; set; }

    // Lưu trạng thái StackingDamagePercent (Java: tlCongDonSD).

    public int StackingDamagePercent { get; set; }

    // Lưu trạng thái LowMpDamageReductionPercent (Java: tlSubDamePercenMp20).

    public int LowMpDamageReductionPercent { get; set; }

    // Lưu trạng thái EarthlingAttackPercent (Java: tlTanCongTocTraiDat).

    public int EarthlingAttackPercent { get; set; }

    // Lưu trạng thái NamekianAttackPercent (Java: tlTanCongTocNamec).

    public int NamekianAttackPercent { get; set; }

    // Lưu trạng thái SaiyanAttackPercent (Java: tlTanCongTocXayda).

    public int SaiyanAttackPercent { get; set; }

    // Lưu trạng thái EarthlingDamageReductionPercent (Java: tlGiamSatThuongTraiDat).

    public int EarthlingDamageReductionPercent { get; set; }

    // Lưu trạng thái NamekianDamageReductionPercent (Java: tlGiamSatThuongNamec).

    public int NamekianDamageReductionPercent { get; set; }

    // Lưu trạng thái SaiyanDamageReductionPercent (Java: tlGiamSatThuongXayda).

    public int SaiyanDamageReductionPercent { get; set; }

    // Lưu trạng thái ClanDamagePercent (Java: tlDameClan).

    public int ClanDamagePercent { get; set; }

    // Lưu trạng thái ClanHpPercent (Java: tlHpClan).

    public int ClanHpPercent { get; set; }

    // Lưu trạng thái ClanMpPercent (Java: tlMpClan).

    public int ClanMpPercent { get; set; }

    // Lưu trạng thái BossDamagePercent (Java: tlDameBoss).

    public int BossDamagePercent { get; set; }

    // Lưu trạng thái RunningMobDamagePercent (Java: tlDameMobRun).

    public int RunningMobDamagePercent { get; set; }

    // Lưu trạng thái MonkeyMobDamagePercent (Java: tlDameMobMonkey).

    public int MonkeyMobDamagePercent { get; set; }

    // Lưu trạng thái FlyingMobDamagePercent (Java: tlDameMobFly).

    public int FlyingMobDamagePercent { get; set; }

    // Lưu trạng thái IsXinbato (Java: isXinbato).

    public bool IsXinbato { get; set; }

    // Lưu trạng thái IsRabbitLeader (Java: isThoDaiCa).

    public bool IsRabbitLeader { get; set; }

    // Lưu trạng thái PetPowerPotentialPercent (Java: tlTNSMPet).

    public int PetPowerPotentialPercent { get; set; }

    // Lưu trạng thái ChargedBlastLevel (Java: xChuong).

    public int ChargedBlastLevel { get; set; }

    // Thực hiện TinhSealSetCount (Java: setTinhAn).

    public int TinhSealSetCount { get; set; }

    // Thực hiện NhatSealSetCount (Java: setNhatAn).

    public int NhatSealSetCount { get; set; }

    // Thực hiện NguyetSealSetCount (Java: setNguyetAn).

    public int NguyetSealSetCount { get; set; }

    // Lưu trạng thái SexyDamagePercent (Java: tlSexyDame).

    public int SexyDamagePercent { get; set; }

    // Lưu trạng thái CoolDamagePercent (Java: tlCoolDame).

    public int CoolDamagePercent { get; set; }

    // Lưu trạng thái DamageReductionPercent (Java: tlSubSD).

    public int DamageReductionPercent { get; set; }

    // Lưu trạng thái HpReductionPercent (Java: tlSubHP).

    public int HpReductionPercent { get; set; }

    // Lưu trạng thái MpReductionPercent (Java: tlSubMP).

    public int MpReductionPercent { get; set; }

    // Lưu trạng thái ChargedBlastNullification (Java: voHieuChuong).

    public int ChargedBlastNullification { get; set; }

    // Lưu trạng thái giáp tập luyện. (Java: trainArmor)

    public NPointItem? TrainingArmor { get; set; }

    // Lưu trạng thái WearingTrainArmor (Java: wearingTrainArmor).

    public bool WearingTrainArmor { get; set; }

    // Lưu trạng thái WearingInvisibilityArmor (Java: wearingVoHinh).

    public bool WearingInvisibilityArmor { get; set; }

    // Lưu trạng thái IsColdImmune (Java: isKhongLanh).

    public bool IsColdImmune { get; set; }

    // Lưu trạng thái IsFounder (Java: isFounder).

    public bool IsFounder { get; set; }

    // Lưu trạng thái HasTinhSeal (Java: isTinhAn).

    public bool HasTinhSeal { get; set; }

    // Lưu trạng thái HasNhatSeal (Java: isNhatAn).

    public bool HasNhatSeal { get; set; }

    // Lưu trạng thái HasNguyetSeal (Java: isNguyetAn).

    public bool HasNguyetSeal { get; set; }

    // Lưu trạng thái IsInvisible (Java: isTanHinh).

    public bool IsInvisible { get; set; }

    // Lưu trạng thái IsPetrified (Java: isHoaDa).

    public bool IsPetrified { get; set; }

    // Lưu trạng thái IsSPLCostume (Java: isDoSPL).

    public bool IsSPLCostume { get; set; }

    // Lưu trạng thái IsRabbitBulma (Java: isThoBulma).

    public bool IsRabbitBulma { get; set; }

    // Lưu trạng thái IsBulmaColoredHair (Java: isBunmaTocMau).

    public bool IsBulmaColoredHair { get; set; }

    // Lưu trạng thái IsBeachParty (Java: isTiecBaiBien).

    public bool IsBeachParty { get; set; }

    // Lưu trạng thái PercentDirtyHpLoss (Java: tlHpGiamODo).

    public int PercentDirtyHpLoss { get; set; }

    // Lưu trạng thái IsGogeta (Java: isGogeta).

    public bool IsGogeta { get; set; }

    // Lưu trạng thái Grants20ExperienceOnMobAttack (Java: cong20ExpKhiAttackMob).

    public bool Grants20ExperienceOnMobAttack { get; set; }

    // Lưu cấp Bông tai (Java: levelBT).

    public int EarringLevel { get; set; }


    // Nạp PowerLimit từ Host theo LimitPower (Java: initPowerLimit).

    public void InitPowerLimit()
    {
        PowerLimit = Host.GetPowerLimit(LimitPower);
    }

    // Trả hạng bắt đầu từ 1 theo Id, hoặc -1 khi không có trong danh sách (Java: getPlayerRank).

    public int GetPlayerRank(IReadOnlyList<Player> list, Player player)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Id == player.Id)
            {
                return i + 1;
            }
        }

        return -1;
    }

    internal static int GetPlayerRank(IReadOnlyList<long> ids, long playerId)
    {
        for (int i = 0; i < ids.Count; i++)
        {
            if (ids[i] == playerId)
            {
                return i + 1;
            }
        }

        return -1;
    }

    // Trả HP hiện tại nhưng không vượt HpMax (Java: getHP).

    public long GetHP() => Hp <= HpMax ? Hp : HpMax;

    // Chặn HP dương ở HpMax; HP không dương làm chết player (Java: setHP).

    public void SetHP(long hp)
    {
        if (hp > 0)
        {
            Hp = hp <= HpMax ? hp : HpMax;
        }
        else
        {
            Host.SetDie(Player);
        }
    }

    // Trả KI hiện tại nhưng không vượt MpMax (Java: getMP).

    public long GetMP() => Mp <= MpMax ? Mp : MpMax;

    // Chặn KI trong đoạn 0..MpMax (Java: setMP).

    public void SetMP(long mp)
    {
        if (mp > 0)
        {
            Mp = mp <= MpMax ? mp : MpMax;
        }
        else
        {
            Mp = 0;
        }
    }

    // Chỉ cộng lượng dương và chặn ở HpMax; phép cộng long giữ tràn số Java (Java: addHp).

    public void AddHp(long hp)
    {
        if (hp > 0)
        {
            long potentialHp = unchecked(Hp + hp);
            Hp = potentialHp > HpMax ? HpMax : potentialHp;
        }
    }

    // Cho phép lượng âm rồi chặn kết quả trong đoạn 0..MpMax; phép cộng long giữ tràn số Java (Java: addMp).

    public void AddMp(long mp)
    {
        long potentialMp = unchecked(Mp + mp);
        if (potentialMp > MpMax)
        {
            Mp = MpMax;
        }
        else if (potentialMp < 0)
        {
            Mp = 0;
        }
        else
        {
            Mp = potentialMp;
        }
    }

    // Gán HP thường, sàn 0; không chặn HpMax và không giết (Java: setHp).

    public void SetHp(long hp)
    {
        Hp = hp < 0 ? 0 : hp;
    }

    // Thực hiện SetMp (Java: setMp).

    public void SetMp(long mp)
    {
        Mp = mp < 0 ? 0 : mp;
    }

    // Trả 100 khi HpMax bằng 0; còn lại HP * 100 / HpMax, cắt về int (Java: getCurrPercentHP).

    public int GetCurrentHpPercent()
    {
        if (HpMax == 0)
        {
            return 100;
        }

        return unchecked((int)(Hp * 100 / HpMax));
    }

    // Không kiểm tra MpMax bằng 0 nên ném DivideByZeroException như ArithmeticException Java (Java: getCurrPercentMP).

    public int GetCurrentMpPercent() => unchecked((int)(Mp * 100 / MpMax));

    // Thực hiện SetFullHpMp (Java: setFullHpMp).

    public void SetFullHpMp()
    {
        Hp = HpMax;
        Mp = MpMax;
    }

    // Thực hiện SetFullHp (Java: setFullHp).

    public void SetFullHp()
    {
        Hp = HpMax;
    }

    // Thực hiện SetFullMp (Java: setFullMp).

    public void SetFullMp()
    {
        Mp = MpMax;
    }

    // Trừ double rồi ép về long theo Java (cắt/bão hòa); HP không dương thành 0 nhưng không giết.

    public void SubHP(double sub)
    {
        Hp = JavaNumbers.ToLong(Hp - sub);
        if (Hp <= 0)
        {
            Hp = 0;
            SetHp(0);
        }
    }

    // Thực hiện SubMP (Java: subMP).

    public void SubMP(long sub)
    {
        Mp = unchecked(Mp - sub);
        if (Mp <= 0)
        {
            Mp = 0;
        }
    }

    // Tính param * percent / 100 bằng long: nhân trước, tràn số rồi chia cắt như Java (Java: calPercent).

    public long CalculatePercent(long param, long percent) => unchecked(param * percent / 100);

    // Thực hiện SubtractPower (Java: subSucManh).

    public void SubtractPower(long point)
    {
        Power = unchecked(Power - point);
        if (Power <= 0)
        {
            Power = 0;
        }
    }

    // Thực hiện SubtractPotential (Java: subTiemNang).

    public void SubtractPotential(long point)
    {
        Potential = unchecked(Potential - point);
        if (Potential <= 0)
        {
            Potential = 0;
        }
    }

    // Thực hiện SubtractPowerPotential (Java: subSucManhTiemNang).

    public void SubtractPowerPotential(long point)
    {
        Power = unchecked(Power - point);
        Potential = unchecked(Potential - point);
        if (Power <= 0)
        {
            Power = 0;
        }

        if (Potential <= 0)
        {
            Potential = 0;
        }
    }

    // Thực hiện GetHpAbsorptionPercent (Java: getTileHutHp).

    public int GetHpAbsorptionPercent(bool isMob) => isMob ? unchecked(HpAbsorptionPercent + MobHpAbsorptionPercent) : HpAbsorptionPercent;

    // Thực hiện GetMpAbsorptionPercent (Java: getTiLeHutMp).

    public int GetMpAbsorptionPercent() => MpAbsorptionPercent;

    // Trừ phòng thủ nhưng sát thương tối thiểu là 1 (Java: subDameInjureWithDeff).

    public double ReduceDamageByDefense(double dame)
    {
        long def = Defense;
        dame -= def;
        if (dame < 0)
        {
            dame = 1;
        }

        return dame;
    }

    // Thực hiện GetGoldPercent (Java: getTlGold).

    public int GetGoldPercent() => GoldPercent;

    // Cộng thưởng vàng nội tại id 23 của lần đánh trước; null trước lần đánh đầu như Java (Java: settlGold).

    public void ApplyIntrinsicGoldPercent()
    {
        if (intrinsic is not null && intrinsic.Id == 23)
        {
            GoldPercent = unchecked(GoldPercent + intrinsic.Param1);
        }
    }

    // Xóa danh sách, không gán null và giữ Player theo quy tắc shell (Java: dispose).

    public void Dispose()
    {
        intrinsic = null;
        HpPercents.Clear();
        MpPercents.Clear();
        DefensePercents.Clear();
        DamagePercents.Clear();
        MobAttackDamagePercents.Clear();
        PowerPotentialPercents.Clear();
        SpeedPercent.Clear();
        GC.SuppressFinalize(this);
    }

    private long Now() => Clock.CurrentTimeMillis();
}
