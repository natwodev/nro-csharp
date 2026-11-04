using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Items.Time;

// Lưu cờ, thời điểm bắt đầu và thời lượng của mọi hiệu ứng vật phẩm có thời hạn trên người chơi.
// Nguồn Java: models/Item/ItemTime.java (84 kiểm tra hết hạn giữ nguyên thứ tự Java vì thứ tự gửi lại chỉ số có ý nghĩa; đồng hồ và dịch vụ được tiêm qua Clock/Host; Dispose giữ Player không null; dữ liệu chỉ đọc-ghi trong tick zone nên không đồng bộ hóa).
public class ItemTime
{
    // Id các dòng chữ máy khách hiển thị (gói 65)
    public const sbyte BARRACKS = 0;
    public const sbyte TREASURE_UNDER_SEA = 1;
    public const sbyte DESTRON_GAS = 2;
    public const sbyte SNAKE_WAY = 3;
    public const sbyte RESCUE_MI_NUONG = 4;
    public const sbyte TEXT_FREE_TALISMAN = 5;
    public const sbyte TEXT_DAILY_CHECK_IN = 6;
    public const sbyte CLAN_BOSS_MAP = 7;

    // Các hằng thời lượng theo mili giây của hiệu ứng vật phẩm; tên và giá trị giữ nguyên Java.
    public const int TIME_ITEM_3S = 3000;
    public const int TIME_ITEM_5S = 5000;
    public const int TIME_ITEM_10S = 10000;
    public const int TIME_ITEM_15S = 15000;
    public const int TIME_ITEM_20S = 20000;
    public const int TIME_ITEM_25S = 25000;
    public const int TIME_ITEM_30S = 30000;
    public const int TIME_ITEM_35S = 35000;
    public const int TIME_ITEM_40S = 40000;
    public const int TIME_ITEM_45S = 45000;
    public const int TIME_ITEM_50S = 50000;
    public const int TIME_ITEM_55S = 55000;
    public const int TIME_ITEM_1M = 60000;
    public const int TIME_ITEM_1M30S = 90000;
    public const int TIME_ITEM_2M = 120000;
    public const int TIME_ITEM_3M = 180000;
    public const int TIME_ITEM_5M = 300000;
    public const int TIME_ITEM_10M = 600000;
    public const int TIME_ITEM_15M = 900000;
    public const int TIME_ITEM_20M = 1200000;
    public const int TIME_ITEM_30M = 1800000;
    public const int TIME_ITEM_40M = 2400000;
    public const int TIME_ITEM_50M = 3000000;
    public const int TIME_ITEM_60M = 3600000;
    public const int TIME_ITEM_70M = 4200000;
    public const int TIME_ITEM_80M = 4800000;
    public const int TIME_ITEM_90M = 5400000;
    public const int TIME_ITEM_100M = 6000000;
    public const int TIME_ITEM_110M = 6600000;
    public const int TIME_ITEM_120M = 7200000;
    public const int TIME_ITEM_130M = 7800000;
    public const int TIME_ITEM_140M = 8400000;
    public const int TIME_ITEM_150M = 9000000;
    public const int TIME_ITEM_160M = 9600000;
    public const int TIME_ITEM_170M = 10200000;
    public const int TIME_ITEM_180M = 10800000;
    public const int TIME_ITEM_190M = 11400000;
    public const int TIME_ITEM_200M = 12000000;
    public const int TIME_ITEM_210M = 12600000;
    public const int TIME_ITEM_220M = 13200000;
    public const int TIME_ITEM_230M = 13800000;
    public const int TIME_ITEM_240M = 14400000;
    public const int TIME_ITEM_250M = 15000000;
    public const int TIME_ITEM_260M = 15600000;
    public const int TIME_ITEM_270M = 16200000;
    public const int TIME_ITEM_280M = 16800000;
    public const int TIME_ITEM_290M = 17400000;
    public const int TIME_ITEM_300M = 18000000;
    public const int TIME_OPEN_POWER = 86400000;
    public const int TIME_SCANNER = 1800000;
    public const int TIME_EAT_MEAL = 600000;
    public const int TIME_MONKEY_TAIL = 600000;
    public const int TIME_SKILL = 300000;
    public const int TIME_BROLY = 300000;

    // Các cờ hiệu lực và mốc bắt đầu mili giây của hiệu ứng; từng cặp IsUse/LastTime tương ứng được Update kiểm tra theo đúng thứ tự Java.
    public bool IsUseBloodTonic { get; set; }
    public bool IsUseEnergyTonic { get; set; }
    public bool IsUseXenArmor { get; set; }
    public bool IsUseRage { get; set; }
    public bool IsUseIncognito { get; set; }
    public bool IsUseBloodTonic2 { get; set; }
    public bool IsUseEnergyTonic2 { get; set; }
    public bool IsUseXenArmor2 { get; set; }
    public bool IsUseRage2 { get; set; }
    public bool IsUseIncognito2 { get; set; }
    public bool IsActive { get; set; }
    public bool IsUseRedPeach0 { get; set; }
    public bool IsUseRedPeach { get; set; }
    public bool IsUseRedPeach1 { get; set; }
    public bool IsUseRedPeach3 { get; set; }
    public bool IsUseRedPeach5 { get; set; }
    public bool IsUseRedPeach10 { get; set; }
    public bool IsUseRedPeach25 { get; set; }
    public bool IsUseRedPeach50 { get; set; }
    public bool IsUseRedPeach99 { get; set; }
    public bool IsUseRedPeach999 { get; set; }
    public bool IsUseStandardInvisibilityPotion { get; set; }
    public bool IsUseEnhancedInvisibilityPotion { get; set; }
    public long LastTimeBloodTonic { get; set; }
    public long LastTimeEnergyTonic { get; set; }
    public long LastTimeXenArmor { get; set; }
    public long LastTimeRage { get; set; }
    public long LastTimeIncognito { get; set; }
    public long LastTimeRedPeach0 { get; set; }
    public long LastTimeRedPeach { get; set; }
    public long LastTimeRedPeach1 { get; set; }
    public long LastTimeRedPeach3 { get; set; }
    public long LastTimeRedPeach5 { get; set; }
    public long LastTimeRedPeach10 { get; set; }
    public long LastTimeRedPeach25 { get; set; }
    public long LastTimeRedPeach50 { get; set; }
    public long LastTimeRedPeach99 { get; set; }
    public long LastTimeRedPeach999 { get; set; }
    public long LastTimeStandardInvisibilityPotion { get; set; }
    public long LastTimeEnhancedInvisibilityPotion { get; set; }
    public long LastTimeBloodTonic2 { get; set; }
    public long LastTimeEnergyTonic2 { get; set; }
    public long LastTimeXenArmor2 { get; set; }
    public long LastTimeRage2 { get; set; }
    public long LastTimeIncognito2 { get; set; }
    public bool IsUseScanner { get; set; }
    public long LastTimeUseScanner { get; set; }
    public bool IsOpenPower { get; set; }
    public long LastTimeOpenPower { get; set; }
    public bool IsAutoPlayEnabled { get; set; }
    public long LastTimeAutoPlay { get; set; }
    public int AutoPlayDurationMs { get; set; }
    public bool IsEatMeal { get; set; }
    public long LastTimeEatMeal { get; set; }
    public int IconMeal { get; set; }
    public bool HasDamageBoost30 { get; set; }
    public bool HasHpBoost30 { get; set; }
    public bool HasEnergyBoost30 { get; set; }
    public bool HasDamageBoost15 { get; set; }
    public bool HasHpBoost15 { get; set; }
    public bool HasEnergyBoost15 { get; set; }
    public long DamageBoost30StartTime { get; set; }
    public long HpBoost30StartTime { get; set; }
    public long EnergyBoost30StartTime { get; set; }
    public long DamageBoost15StartTime { get; set; }
    public long HpBoost15StartTime { get; set; }
    public long EnergyBoost15StartTime { get; set; }
    public bool IsCommeson { get; set; }
    public long LastCommeson { get; set; }
    public bool IsFaceMask { get; set; }
    public long LastFaceMask { get; set; }
    public bool IsUseOneEyedCandy { get; set; }
    public long LastTimeOneEyedCandy { get; set; }
    public bool IsUseDarkPumpkinSoup { get; set; }
    public long LastTimeDarkPumpkinSoup { get; set; }
    public bool IsUseSpiderCake { get; set; }
    public long LastTimeSpiderCake { get; set; }
    public bool IsUseCaterpillarHamburger { get; set; }
    public long LastTimeCaterpillarHamburger { get; set; }
    public bool IsUseStandardIpanaOintment { get; set; }
    public long LastTimeStandardIpanaOintment { get; set; }
    public bool IsUseSpecialIpanaOintment { get; set; }
    public long LastTimeSpecialIpanaOintment { get; set; }
    public bool IsUseTamarindCrab { get; set; }
    public long LastTimeTamarindCrab { get; set; }
    public bool IsUseGrilledOctopus { get; set; }
    public long LastTimeGrilledOctopus { get; set; }
    public bool IsUseBreadedShrimp { get; set; }
    public long LastTimeBreadedShrimp { get; set; }
    public long MonkeyTailStartTime { get; set; }
    public bool HasMonkeyTail { get; set; }
    public long PowerBurstStartTime { get; set; }
    public bool IsPowerBurst { get; set; }
    // Đang có hiệu lực giải trừ phép thuật ở map Mabư (Java: isUseGTPT).
    public bool HasMagicDispel { get; set; }

    // Mốc bắt đầu hiệu lực giải trừ phép thuật (Java: lastTimeUseGTPT).
    public long MagicDispelStartTime { get; set; }
    public bool IsUseElectricRiceCooker { get; set; }
    public long LastTimeUseElectricRiceCooker { get; set; }
    public bool IsUseCylindricalRiceCake { get; set; }
    public long LastTimeUseCylindricalRiceCake { get; set; }
    public bool IsUseSquareRiceCake { get; set; }
    public long LastTimeUseSquareRiceCake { get; set; }
    public bool IsUseSmokedSausage { get; set; }
    public long LastTimeUseSmokedSausage { get; set; }
    public bool IsUseCatFood { get; set; }
    public long LastTimeUseCatFood { get; set; }
    public bool IsUseDragonFruitNoodles { get; set; }
    public long LastTimeUseDragonFruitNoodles { get; set; }
    public bool IsUseRoastChickenRice { get; set; }
    public long LastTimeUseRoastChickenRice { get; set; }
    public bool IsUsePlumpMouse { get; set; }
    public long LastTimeUsePlumpMouse { get; set; }
    public bool IsUseCriticalChance2 { get; set; }
    public long LastTimeUseCriticalChance2 { get; set; }
    public bool IsUseCriticalChance3 { get; set; }
    public long LastTimeUseCriticalChance3 { get; set; }
    public bool IsUseDodge { get; set; }
    public long LastTimeUseDodge { get; set; }
    public bool IsUseDodge2 { get; set; }
    public long LastTimeUseDodge2 { get; set; }
    public bool IsUseReflectDamage { get; set; }
    public long LastTimeUseReflectDamage { get; set; }
    public bool IsUseReflectDamage2 { get; set; }
    public long LastTimeUseReflectDamage2 { get; set; }
    public bool IsUseReflectDamage3 { get; set; }
    public long LastTimeUseReflectDamage3 { get; set; }
    public bool IsUseKamejoko { get; set; }
    public long LastTimeUseKamejoko { get; set; }
    public bool IsUseKamejoko2 { get; set; }
    public long LastTimeUseKamejoko2 { get; set; }
    public bool IsUseRocket1h { get; set; }
    public long LastTimeUseRocket1h { get; set; }
    public bool IsUseSuperRegen { get; set; }
    public long LastTimeUseSuperRegen { get; set; }
    public bool IsUseTrueDamage { get; set; }
    public long LastTimeUseTrueDamage { get; set; }
    public bool IsUseTrueDamage2 { get; set; }
    public long LastTimeUseTrueDamage2 { get; set; }
    public bool IsUsePetPowerPotentialCharm { get; set; }
    public long LastTimeUsePetPowerPotentialCharm { get; set; }
    public bool IsUseFourLeafClover { get; set; }
    public long LastTimeUseFourLeafClover { get; set; }
    public bool IsUseDurian { get; set; }
    public long LastTimeUseDurian { get; set; }
    public bool IsUseSoulScanner { get; set; }
    public long LastTimeUseSoulScanner { get; set; }
    // Đang dùng máy dò đá siêu hóa (Java: isUseMayDoSieuHoa).
    public bool IsUsingSuperTransformationStoneDetector { get; set; }

    // Mốc bắt đầu dùng máy dò đá siêu hóa (Java: lastTimeUseMayDoSieuHoa).
    public long SuperTransformationStoneDetectorStartTime { get; set; }
    public bool IsUseMysticOrbScanner { get; set; }
    public long LastTimeUseMysticOrbScanner { get; set; }
    public bool IsBoneDragon { get; set; }
    public long LastTimeBoneDragon { get; set; }
    public bool IsBoneDragon2 { get; set; }
    public long LastTimeBoneDragon2 { get; set; }
    public bool IsBoneDragon3 { get; set; }
    public long LastTimeBoneDragon3 { get; set; }
    public bool IsUseRabbitMooncake1 { get; set; }
    public long LastTimeUseRabbitMooncake1 { get; set; }
    public bool IsUseRabbitMooncake2 { get; set; }
    public long LastTimeUseRabbitMooncake2 { get; set; }
    public bool IsUseRabbitMooncake3 { get; set; }
    public long LastTimeUseRabbitMooncake3 { get; set; }
    public bool IsUseMoonFestivalOneEgg { get; set; }
    public long LastTimeUseMoonFestivalOneEgg { get; set; }
    public bool IsUseMoonFestivalTwoEgg { get; set; }
    public long LastTimeUseMoonFestivalTwoEgg { get; set; }
    public bool IsUseSpecialMoonFestival { get; set; }
    public long LastTimeUseSpecialMoonFestival { get; set; }
    public bool IsUseMoonFestivalCakeBox { get; set; }
    public long LastTimeUseMoonFestivalCakeBox { get; set; }
    public bool IsUseMonkeyTailPowerPotential { get; set; }
    public long LastTimeUseMonkeyTailPowerPotential { get; set; }

    private static readonly VietnamClock DefaultClock = new();

    // Thực hiện thành viên; giữ nguyên điều kiện, thứ tự và số học của Java.
    public ItemTime(Player player, VietnamClock? clock = null, IItemHost? host = null)
    {
        Player = player;
        Clock = clock ?? DefaultClock;
        Host = host ?? NullItemHost.Instance;
    }

    // Java player. (Java: player).
    public Player Player { get; }

    // Thực hiện clock; giữ nguyên điều kiện, thứ tự và số học của Java.
    public VietnamClock Clock { get; set; }

    // Thực hiện host; giữ nguyên điều kiện, thứ tự và số học của Java.
    public IItemHost Host { get; set; }

    // strictly more than milliseconds have passed since . (Java: Util.canDoWithTime(lastTime, miniTimeTarget)).
    public bool CanDoWithTime(long lastTime, long miniTimeTarget) => Clock.CurrentTimeMillis() - lastTime > miniTimeTarget;

    // Cập nhật update; giữ nguyên điều kiện, thứ tự và số học của Java (Java: update).
    public void Update()
    {
        if (IsUseMonkeyTailPowerPotential && CanDoWithTime(LastTimeUseMonkeyTailPowerPotential, TIME_ITEM_30M))
        {
            IsUseMonkeyTailPowerPotential = false;
            Host.SendPoint(Player);
        }

        if (IsUseMoonFestivalCakeBox && CanDoWithTime(LastTimeUseMoonFestivalCakeBox, TIME_ITEM_150M))
        {
            IsUseMoonFestivalCakeBox = false;
            Host.SendPoint(Player);
        }

        if (IsUseSpecialMoonFestival && CanDoWithTime(LastTimeUseSpecialMoonFestival, TIME_ITEM_120M))
        {
            IsUseSpecialMoonFestival = false;
            Host.SendPoint(Player);
        }

        if (IsUseMoonFestivalTwoEgg && CanDoWithTime(LastTimeUseMoonFestivalTwoEgg, TIME_ITEM_90M))
        {
            IsUseMoonFestivalTwoEgg = false;
            Host.SendPoint(Player);
        }

        if (IsUseMoonFestivalOneEgg && CanDoWithTime(LastTimeUseMoonFestivalOneEgg, TIME_ITEM_60M))
        {
            IsUseMoonFestivalOneEgg = false;
            Host.SendPoint(Player);
        }

        if (IsUseRabbitMooncake3 && CanDoWithTime(LastTimeUseRabbitMooncake3, TIME_ITEM_20M))
        {
            IsUseRabbitMooncake3 = false;
            Host.SendPoint(Player);
        }

        if (IsUseRabbitMooncake2 && CanDoWithTime(LastTimeUseRabbitMooncake2, TIME_ITEM_10M))
        {
            IsUseRabbitMooncake2 = false;
            Host.SendPoint(Player);
        }

        if (IsUseRabbitMooncake1 && CanDoWithTime(LastTimeUseRabbitMooncake1, TIME_ITEM_10M))
        {
            IsUseRabbitMooncake1 = false;
            Host.SendPoint(Player);
        }

        if (IsBoneDragon3 && CanDoWithTime(LastTimeBoneDragon3, TIME_ITEM_30M))
        {
            IsBoneDragon3 = false;
            Host.SendPoint(Player);
        }

        if (IsBoneDragon2 && CanDoWithTime(LastTimeBoneDragon2, TIME_ITEM_30M))
        {
            IsBoneDragon2 = false;
            Host.SendPoint(Player);
        }

        if (IsBoneDragon && CanDoWithTime(LastTimeBoneDragon, TIME_ITEM_30M))
        {
            IsBoneDragon = false;
            Host.SendPoint(Player);
        }

        if (IsUseMysticOrbScanner && CanDoWithTime(LastTimeUseMysticOrbScanner, TIME_ITEM_60M))
        {
            IsUseMysticOrbScanner = false;
            Host.SendPoint(Player);
        }

        if (IsUseSoulScanner && CanDoWithTime(LastTimeUseSoulScanner, TIME_ITEM_60M))
        {
            IsUseSoulScanner = false;
            Host.SendPoint(Player);
        }

        if (IsUsingSuperTransformationStoneDetector && CanDoWithTime(SuperTransformationStoneDetectorStartTime, TIME_ITEM_60M))
        {
            IsUsingSuperTransformationStoneDetector = false;
            Host.SendPoint(Player);
        }

        if (IsUsePetPowerPotentialCharm && CanDoWithTime(LastTimeUsePetPowerPotentialCharm, TIME_ITEM_30M))
        {
            IsUsePetPowerPotentialCharm = false;
            Host.SendPoint(Player);
        }

        if (IsUseFourLeafClover && CanDoWithTime(LastTimeUseFourLeafClover, TIME_ITEM_30M))
        {
            IsUseFourLeafClover = false;
            Host.SendPoint(Player);
        }

        if (IsUseDurian && CanDoWithTime(LastTimeUseDurian, TIME_ITEM_10M))
        {
            IsUseDurian = false;
            Host.SendPoint(Player);
        }

        if (IsUseTrueDamage && CanDoWithTime(LastTimeUseTrueDamage, TIME_ITEM_10M))
        {
            IsUseTrueDamage = false;
            Host.SendPoint(Player);
        }

        if (IsUseTrueDamage2 && CanDoWithTime(LastTimeUseTrueDamage2, TIME_ITEM_10M))
        {
            IsUseTrueDamage2 = false;
            Host.SendPoint(Player);
        }

        if (IsUseRocket1h && CanDoWithTime(LastTimeUseRocket1h, TIME_ITEM_60M))
        {
            IsUseRocket1h = false;
            Host.SendPoint(Player);
        }

        if (IsUseSuperRegen && CanDoWithTime(LastTimeUseSuperRegen, TIME_ITEM_3S))
        {
            IsUseSuperRegen = false;
            Host.SendPoint(Player);
        }

        if (IsUseKamejoko2 && CanDoWithTime(LastTimeUseKamejoko2, TIME_ITEM_30S))
        {
            IsUseKamejoko2 = false;
            Host.SendPoint(Player);
        }

        if (IsUseKamejoko && CanDoWithTime(LastTimeUseKamejoko, TIME_ITEM_30S))
        {
            IsUseKamejoko = false;
            Host.SendPoint(Player);
        }

        if (IsUseReflectDamage3 && CanDoWithTime(LastTimeUseReflectDamage3, TIME_ITEM_30S))
        {
            IsUseReflectDamage3 = false;
            Host.SendPoint(Player);
        }

        if (IsUseReflectDamage2 && CanDoWithTime(LastTimeUseReflectDamage2, TIME_ITEM_30S))
        {
            IsUseReflectDamage2 = false;
            Host.SendPoint(Player);
        }

        if (IsUseReflectDamage && CanDoWithTime(LastTimeUseReflectDamage, TIME_ITEM_30S))
        {
            IsUseReflectDamage = false;
            Host.SendPoint(Player);
        }

        if (IsUseDodge2 && CanDoWithTime(LastTimeUseDodge2, TIME_ITEM_30S))
        {
            IsUseDodge2 = false;
            Host.SendPoint(Player);
        }

        if (IsUseDodge && CanDoWithTime(LastTimeUseDodge, TIME_ITEM_30S))
        {
            IsUseDodge = false;
            Host.SendPoint(Player);
        }

        if (IsUseCriticalChance3 && CanDoWithTime(LastTimeUseCriticalChance3, TIME_ITEM_10M))
        {
            IsUseCriticalChance3 = false;
            Host.SendPoint(Player);
        }

        if (IsUseCriticalChance2 && CanDoWithTime(LastTimeUseCriticalChance2, TIME_ITEM_10M))
        {
            IsUseCriticalChance2 = false;
            Host.SendPoint(Player);
        }

        if (IsUsePlumpMouse && CanDoWithTime(LastTimeUsePlumpMouse, TIME_ITEM_60M))
        {
            IsUsePlumpMouse = false;
            Host.SendPoint(Player);
        }

        if (IsUseRoastChickenRice && CanDoWithTime(LastTimeUseRoastChickenRice, TIME_ITEM_10M))
        {
            IsUseRoastChickenRice = false;
            Host.SendPoint(Player);
        }

        if (IsUseDragonFruitNoodles && CanDoWithTime(LastTimeUseDragonFruitNoodles, TIME_ITEM_10M))
        {
            IsUseDragonFruitNoodles = false;
            Host.SendPoint(Player);
        }

        if (IsUseSmokedSausage && CanDoWithTime(LastTimeUseSmokedSausage, TIME_ITEM_3M))
        {
            IsUseSmokedSausage = false;
            Host.SendPoint(Player);
        }

        if (IsUseCatFood && CanDoWithTime(LastTimeUseCatFood, TIME_ITEM_15M))
        {
            IsUseCatFood = false;
            Host.SendPoint(Player);
        }

        if (IsUseCylindricalRiceCake && CanDoWithTime(LastTimeUseCylindricalRiceCake, TIME_ITEM_60M))
        {
            IsUseCylindricalRiceCake = false;
            Host.SendPoint(Player);
        }

        if (IsUseSquareRiceCake && CanDoWithTime(LastTimeUseSquareRiceCake, TIME_ITEM_60M))
        {
            IsUseSquareRiceCake = false;
            Host.SendPoint(Player);
        }

        if (IsUseElectricRiceCooker && CanDoWithTime(LastTimeUseElectricRiceCooker, TIME_ITEM_30M))
        {
            IsUseElectricRiceCooker = false;
            Host.SendPoint(Player);
        }

        if (IsPowerBurst && CanDoWithTime(PowerBurstStartTime, TIME_ITEM_10M))
        {
            IsPowerBurst = false;
            Host.SendPoint(Player);
        }

        if (HasMonkeyTail && CanDoWithTime(MonkeyTailStartTime, TIME_ITEM_2M))
        {
            HasMonkeyTail = false;
            Host.RemoveMonkeyTail(Player);
            Host.SendPoint(Player);
        }

        if (IsUseBreadedShrimp && CanDoWithTime(LastTimeBreadedShrimp, TIME_ITEM_10M))
        {
            IsUseBreadedShrimp = false;
            Host.SendPoint(Player);
        }

        if (IsUseGrilledOctopus && CanDoWithTime(LastTimeGrilledOctopus, TIME_ITEM_10M))
        {
            IsUseGrilledOctopus = false;
            Host.SendPoint(Player);
        }

        if (IsUseTamarindCrab && CanDoWithTime(LastTimeTamarindCrab, TIME_ITEM_10M))
        {
            IsUseTamarindCrab = false;
            Host.SendPoint(Player);
        }

        if (IsUseStandardIpanaOintment && CanDoWithTime(LastTimeStandardIpanaOintment, TIME_ITEM_10M))
        {
            IsUseStandardIpanaOintment = false;
            Host.SendPoint(Player);
        }

        if (IsUseSpecialIpanaOintment && CanDoWithTime(LastTimeSpecialIpanaOintment, TIME_ITEM_30M))
        {
            IsUseSpecialIpanaOintment = false;
            Host.SendPoint(Player);
        }

        if (HasMagicDispel && CanDoWithTime(MagicDispelStartTime, TIME_ITEM_10M))
        {
            HasMagicDispel = false;
        }

        if (IsFaceMask && CanDoWithTime(LastFaceMask, TIME_ITEM_30M))
        {
            IsFaceMask = false;
            Host.SendPoint(Player);
        }

        if (IsUseCaterpillarHamburger && CanDoWithTime(LastTimeCaterpillarHamburger, TIME_ITEM_30M))
        {
            IsUseCaterpillarHamburger = false;
            Host.SendPoint(Player);
        }

        if (IsUseSpiderCake && CanDoWithTime(LastTimeSpiderCake, TIME_ITEM_30M))
        {
            IsUseSpiderCake = false;
            Host.SendPoint(Player);
        }

        if (IsUseDarkPumpkinSoup && CanDoWithTime(LastTimeDarkPumpkinSoup, TIME_ITEM_30M))
        {
            IsUseDarkPumpkinSoup = false;
            Host.SendPoint(Player);
        }

        if (IsUseOneEyedCandy && CanDoWithTime(LastTimeOneEyedCandy, TIME_ITEM_30M))
        {
            IsUseOneEyedCandy = false;
            Host.SendPoint(Player);
        }

        if (IsCommeson && CanDoWithTime(LastCommeson, TIME_ITEM_50M))
        {
            IsCommeson = false;
            Host.SendPoint(Player);
        }

        if (HasDamageBoost30 && CanDoWithTime(DamageBoost30StartTime, TIME_ITEM_60M))
        {
            HasDamageBoost30 = false;
            Host.SendPoint(Player);
        }

        if (HasHpBoost30 && CanDoWithTime(HpBoost30StartTime, TIME_ITEM_60M))
        {
            HasHpBoost30 = false;
            Host.SendPoint(Player);
        }

        if (HasEnergyBoost30 && CanDoWithTime(EnergyBoost30StartTime, TIME_ITEM_60M))
        {
            HasEnergyBoost30 = false;
            Host.SendPoint(Player);
        }

        if (HasDamageBoost15 && CanDoWithTime(DamageBoost15StartTime, TIME_ITEM_30M))
        {
            HasDamageBoost15 = false;
            Host.SendPoint(Player);
        }

        if (HasHpBoost15 && CanDoWithTime(HpBoost15StartTime, TIME_ITEM_30M))
        {
            HasHpBoost15 = false;
            Host.SendPoint(Player);
        }

        if (HasEnergyBoost15 && CanDoWithTime(EnergyBoost15StartTime, TIME_ITEM_30M))
        {
            HasEnergyBoost15 = false;
            Host.SendPoint(Player);
        }

        if (IsUseStandardInvisibilityPotion && CanDoWithTime(LastTimeStandardInvisibilityPotion, TIME_ITEM_1M))
        {
            IsUseStandardInvisibilityPotion = false;
            Host.SendPoint(Player);
            Host.SendNotification(Player, "Thuốc Tàng Hình Thường Đã Hết Tác Dụng!");
        }

        if (IsUseEnhancedInvisibilityPotion && CanDoWithTime(LastTimeEnhancedInvisibilityPotion, TIME_ITEM_10M))
        {
            IsUseEnhancedInvisibilityPotion = false;
            Host.SendPoint(Player);
            Host.SendNotification(Player, "Thuốc Tàng Hình Cải Tiến Đã Hết Tác Dụng!");
        }

        if (IsUseRedPeach0 && CanDoWithTime(LastTimeRedPeach0, TIME_ITEM_10S))
        {
            IsUseRedPeach0 = false;
            Host.SendPoint(Player);
        }

        if (IsUseRedPeach && CanDoWithTime(LastTimeRedPeach, TIME_ITEM_30S))
        {
            IsUseRedPeach = false;
            Host.SendPoint(Player);
        }

        if (IsUseRedPeach1 && CanDoWithTime(LastTimeRedPeach1, TIME_ITEM_30S))
        {
            IsUseRedPeach1 = false;
            Host.SendPoint(Player);
        }

        if (IsUseRedPeach3 && CanDoWithTime(LastTimeRedPeach3, TIME_ITEM_30S))
        {
            IsUseRedPeach3 = false;
            Host.SendPoint(Player);
        }

        if (IsUseRedPeach5 && CanDoWithTime(LastTimeRedPeach5, TIME_ITEM_1M))
        {
            IsUseRedPeach5 = false;
            Host.SendPoint(Player);
        }

        if (IsUseRedPeach10 && CanDoWithTime(LastTimeRedPeach10, TIME_ITEM_1M))
        {
            IsUseRedPeach10 = false;
            Host.SendPoint(Player);
        }

        if (IsUseRedPeach25 && CanDoWithTime(LastTimeRedPeach25, TIME_ITEM_1M))
        {
            IsUseRedPeach25 = false;
            Host.SendPoint(Player);
        }

        if (IsUseRedPeach50 && CanDoWithTime(LastTimeRedPeach50, TIME_ITEM_10M))
        {
            IsUseRedPeach50 = false;
            Host.SendPoint(Player);
        }

        if (IsUseRedPeach99 && CanDoWithTime(LastTimeRedPeach99, TIME_ITEM_10M))
        {
            IsUseRedPeach99 = false;
            Host.SendPoint(Player);
        }

        if (IsUseRedPeach999 && CanDoWithTime(LastTimeRedPeach999, TIME_ITEM_10M))
        {
            IsUseRedPeach999 = false;
            Host.SendPoint(Player);
        }

        if (IsEatMeal && CanDoWithTime(LastTimeEatMeal, TIME_EAT_MEAL))
        {
            IsEatMeal = false;
            Host.SendPoint(Player);
        }

        if (IsUseBloodTonic && CanDoWithTime(LastTimeBloodTonic, TIME_ITEM_10M))
        {
            IsUseBloodTonic = false;
            Host.SendPoint(Player);
        }

        if (IsUseEnergyTonic && CanDoWithTime(LastTimeEnergyTonic, TIME_ITEM_10M))
        {
            IsUseEnergyTonic = false;
            Host.SendPoint(Player);
        }

        if (IsUseXenArmor && CanDoWithTime(LastTimeXenArmor, TIME_ITEM_10M))
        {
            IsUseXenArmor = false;
        }

        if (IsUseRage && CanDoWithTime(LastTimeRage, TIME_ITEM_10M))
        {
            IsUseRage = false;
            Host.SendPoint(Player);
        }

        if (IsUseIncognito && CanDoWithTime(LastTimeIncognito, TIME_ITEM_10M))
        {
            IsUseIncognito = false;
        }

        if (IsUseBloodTonic2 && CanDoWithTime(LastTimeBloodTonic2, TIME_ITEM_10M))
        {
            IsUseBloodTonic2 = false;
            Host.SendPoint(Player);
        }

        if (IsUseEnergyTonic2 && CanDoWithTime(LastTimeEnergyTonic2, TIME_ITEM_10M))
        {
            IsUseEnergyTonic2 = false;
            Host.SendPoint(Player);
        }

        if (IsUseXenArmor2 && CanDoWithTime(LastTimeXenArmor2, TIME_ITEM_10M))
        {
            IsUseXenArmor2 = false;
        }

        if (IsUseRage2 && CanDoWithTime(LastTimeRage2, TIME_ITEM_10M))
        {
            IsUseRage2 = false;
            Host.SendPoint(Player);
        }

        if (IsUseIncognito2 && CanDoWithTime(LastTimeIncognito2, TIME_ITEM_10M))
        {
            IsUseIncognito2 = false;
        }

        if (IsOpenPower && CanDoWithTime(LastTimeOpenPower, TIME_OPEN_POWER))
        {
            Host.IncreasePowerLimit(Player);
            Host.SendNotification(Player, "Giới hạn sức mạnh của bạn đã được tăng lên 1 bậc");
            IsOpenPower = false;
        }

        if (IsUseScanner && CanDoWithTime(LastTimeUseScanner, TIME_SCANNER))
        {
            IsUseScanner = false;
        }

        if (IsAutoPlayEnabled && CanDoWithTime(LastTimeAutoPlay, AutoPlayDurationMs))
        {
            IsAutoPlayEnabled = false;
            Host.SendMessage(Player, ItemTimeService.BuildCanAutoPlayMessage(IsAutoPlayEnabled));
        }
    }

    // Giải phóng dispose; giữ nguyên điều kiện, thứ tự và số học của Java (Java: dispose).
    public void Dispose()
    {
    }
}
