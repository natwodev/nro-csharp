using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Consts;
using Nro.Core.IO;
using static Nro.Game.Items.Time.ItemTime;

namespace Nro.Game.Items.Time;

// Gửi thời gian hiệu ứng, đồng hồ phó bản và cờ tự đánh tới máy khách, đồng thời xóa chúng khi cần.
// Nguồn Java: models/Item/ItemTimeService.java (host và đồng hồ được tiêm thay singleton; thứ tự danh sách gói giữ nguyên Java; thời gian còn lại ép int rồi ghi short nên giá trị âm/lớn có thể tràn, đồng hồ đọc riêng cho từng hiệu ứng và vài cờ dùng chung icon).
public class ItemTimeService
{
    private readonly IItemHost host;
    private readonly VietnamClock clock;

    // Thực hiện thành viên; giữ nguyên điều kiện, thứ tự và số học của Java.
    public ItemTimeService(IItemHost host, VietnamClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(host);
        this.host = host;
        this.clock = clock ?? new VietnamClock();
    }

    private long Now => clock.CurrentTimeMillis();

    // Tạo gói tin báo bật tự động luyện tập; giữ nguyên điều kiện, thứ tự và số học của Java (Java: sendCanAutoPlay).
    internal static Message BuildCanAutoPlayMessage(bool isAutoTrainingEnabled)
    {
        var msg = new Message(-116);
        msg.Writer().WriteByte(isAutoTrainingEnabled ? 1 : 0);
        return msg;
    }

    // Gửi send all item time; giữ nguyên điều kiện, thứ tự và số học của Java (Java: sendAllItemTime).
    public void SendAllItemTime(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        SendTextTreasureUnderSea(player);
        SendTextBarracks(player);
        SendTextSnakeWay(player);
        SendTextDestronGas(player);
        SendTextTimePickBarracks(player);
        ItemTime t = player.ItemTime;
        ExternalItemTimes fx = host.GetExternalItemTimes(player);
        if (fx.FusionType == ConstPlayer.FUSION_DANCE)
        {
            SendItemTime(player, player.Gender == ConstPlayer.NAMEKIAN ? 3901 : 3790, (int)((fx.FusionDurationMs - (Now - fx.FusionLastTime)) / 1000));
        }

        if (t.IsUseMonkeyTailPowerPotential)
        {
            SendItemTime(player, 5072, (int)(( TIME_ITEM_30M - (Now - t.LastTimeUseMonkeyTailPowerPotential)) / 1000));
        }

        if (t.IsUseMoonFestivalCakeBox)
        {
            SendItemTime(player, 4126, (int)((TIME_ITEM_150M - (Now - t.LastTimeUseMoonFestivalCakeBox)) / 1000));
        }

        if (t.IsUseSpecialMoonFestival)
        {
            SendItemTime(player, 4125, (int)((TIME_ITEM_120M - (Now - t.LastTimeUseSpecialMoonFestival)) / 1000));
        }

        if (t.IsUseMoonFestivalTwoEgg)
        {
            SendItemTime(player, 4043, (int)((TIME_ITEM_90M - (Now - t.LastTimeUseMoonFestivalTwoEgg)) / 1000));
        }

        if (t.IsUseMoonFestivalOneEgg)
        {
            SendItemTime(player, 4042, (int)((TIME_ITEM_60M - (Now - t.LastTimeUseMoonFestivalOneEgg)) / 1000));
        }

        if (t.IsUseRabbitMooncake3)
        {
            SendItemTime(player, 11677, (int)((TIME_ITEM_20M - (Now - t.LastTimeUseRabbitMooncake3)) / 1000));
        }

        if (t.IsUseRabbitMooncake2)
        {
            SendItemTime(player, 11676, (int)((TIME_ITEM_10M - (Now - t.LastTimeUseRabbitMooncake2)) / 1000));
        }

        if (t.IsUseRabbitMooncake1)
        {
            SendItemTime(player, 11675, (int)((TIME_ITEM_10M - (Now - t.LastTimeUseRabbitMooncake1)) / 1000));
        }

        if (t.IsBoneDragon)
        {
            SendItemTime(player, 6579, (int)((TIME_ITEM_30M - (Now - t.LastTimeBoneDragon)) / 1000));
        }

        if (t.IsBoneDragon2)
        {
            SendItemTime(player, 6580, (int)((TIME_ITEM_30M - (Now - t.LastTimeBoneDragon2)) / 1000));
        }

        if (t.IsBoneDragon3)
        {
            SendItemTime(player, 6581, (int)((TIME_ITEM_30M - (Now - t.LastTimeBoneDragon3)) / 1000));
        }

        if (t.IsUseMysticOrbScanner)
        {
            SendItemTime(player, 434, (int)((TIME_ITEM_60M - (Now - t.LastTimeUseMysticOrbScanner)) / 1000));
        }

        if (t.IsUseSoulScanner)
        {
            SendItemTime(player, 11493, (int)((TIME_ITEM_60M - (Now - t.LastTimeUseSoulScanner)) / 1000));
        }

        if (t.IsUsingSuperTransformationStoneDetector)
        {
            SendItemTime(player, 27021, (int)((TIME_ITEM_60M - (Now - t.SuperTransformationStoneDetectorStartTime)) / 1000));
        }

        if (t.IsUsePetPowerPotentialCharm)
        {
            SendItemTime(player, 13540, (int)((TIME_ITEM_30M - (Now - t.LastTimeUsePetPowerPotentialCharm)) / 1000));
        }

        if (t.IsUseFourLeafClover)
        {
            SendItemTime(player, 13618, (int)((TIME_ITEM_30M - (Now - t.LastTimeUseFourLeafClover)) / 1000));
        }

        if (t.IsUseDurian)
        {
            SendItemTime(player, 13747, (int)((TIME_ITEM_10M - (Now - t.LastTimeUseDurian)) / 1000));
        }

        if (t.IsUseTrueDamage)
        {
            SendItemTime(player, 12766, (int)((TIME_ITEM_10M - (Now - t.LastTimeUseTrueDamage)) / 1000));
        }

        if (t.IsUseTrueDamage2)
        {
            SendItemTime(player, 12767, (int)((TIME_ITEM_10M - (Now - t.LastTimeUseTrueDamage2)) / 1000));
        }

        if (t.IsUseSuperRegen)
        {
            SendItemTime(player, 27019, (int)((TIME_ITEM_3S - (Now - t.LastTimeUseSuperRegen)) / 1000));
        }

        if (t.IsUseRocket1h)
        {
            SendItemTime(player, 27009, (int)((TIME_ITEM_60M - (Now - t.LastTimeUseRocket1h)) / 1000));
        }

        if (t.IsUseKamejoko2)
        {
            SendItemTime(player, 30867, (int)((TIME_ITEM_30S - (Now - t.LastTimeUseKamejoko2)) / 1000));
        }

        if (t.IsUseKamejoko)
        {
            SendItemTime(player, 30867, (int)((TIME_ITEM_30S - (Now - t.LastTimeUseKamejoko)) / 1000));
        }

        if (t.IsUseReflectDamage3)
        {
            SendItemTime(player, 30866, (int)((TIME_ITEM_30S - (Now - t.LastTimeUseReflectDamage3)) / 1000));
        }

        if (t.IsUseReflectDamage2)
        {
            SendItemTime(player, 30866, (int)((TIME_ITEM_30S - (Now - t.LastTimeUseReflectDamage2)) / 1000));
        }

        if (t.IsUseReflectDamage)
        {
            SendItemTime(player, 30866, (int)((TIME_ITEM_30S - (Now - t.LastTimeUseReflectDamage)) / 1000));
        }

        if (t.IsUseDodge2)
        {
            SendItemTime(player, 32752, (int)((TIME_ITEM_30S - (Now - t.LastTimeUseDodge2)) / 1000));
        }

        if (t.IsUseDodge)
        {
            SendItemTime(player, 32752, (int)((TIME_ITEM_30S - (Now - t.LastTimeUseDodge)) / 1000));
        }

        if (t.IsUseCriticalChance3)
        {
            SendItemTime(player, 32751, (int)((TIME_ITEM_10M - (Now - t.LastTimeUseCriticalChance3)) / 1000));
        }

        if (t.IsUseCriticalChance2)
        {
            SendItemTime(player, 32751, (int)((TIME_ITEM_10M - (Now - t.LastTimeUseCriticalChance2)) / 1000));
        }

        if (t.IsUsePlumpMouse)
        {
            SendItemTime(player, 12831, (int)((TIME_ITEM_60M - (Now - t.LastTimeUsePlumpMouse)) / 1000));
        }

        if (t.IsUseRoastChickenRice)
        {
            SendItemTime(player, 12396, (int)((TIME_ITEM_10M - (Now - t.LastTimeUseRoastChickenRice)) / 1000));
        }

        if (t.IsUseDragonFruitNoodles)
        {
            SendItemTime(player, 12395, (int)((TIME_ITEM_10M - (Now - t.LastTimeUseDragonFruitNoodles)) / 1000));
        }

        if (t.IsUseSmokedSausage)
        {
            SendItemTime(player, 10890, (int)((TIME_ITEM_3M - (Now - t.LastTimeUseSmokedSausage)) / 1000));
        }

        if (t.IsUseCatFood)
        {
            SendItemTime(player, 10891, (int)((TIME_ITEM_15M - (Now - t.LastTimeUseCatFood)) / 1000));
        }

        if (t.IsUseCylindricalRiceCake)
        {
            SendItemTime(player, 7079, (int)((TIME_ITEM_60M - (Now - t.LastTimeUseCylindricalRiceCake)) / 1000));
        }

        if (t.IsUseSquareRiceCake)
        {
            SendItemTime(player, 7080, (int)((TIME_ITEM_60M - (Now - t.LastTimeUseSquareRiceCake)) / 1000));
        }

        if (t.IsUseElectricRiceCooker)
        {
            SendItemTime(player, 11173, (int)((TIME_ITEM_30M - (Now - t.LastTimeUseElectricRiceCooker)) / 1000));
        }

        if (t.HasMagicDispel)
        {
            SendItemTime(player, 3778, (int)((TIME_ITEM_10M - (Now - t.MagicDispelStartTime)) / 1000));
        }

        if (t.IsPowerBurst)
        {
            SendItemTime(player, 10713, (int)(( TIME_ITEM_10M - (Now - t.PowerBurstStartTime)) / 1000));
        }

        if (t.HasMonkeyTail)
        {
            SendItemTime(player, 5072, (int)(( TIME_ITEM_2M - (Now - t.MonkeyTailStartTime)) / 1000));
        }

        if (t.IsUseTamarindCrab)
        {
            SendItemTime(player, 8060, (int)(( TIME_ITEM_10M - (Now - t.LastTimeTamarindCrab)) / 1000));
        }

        if (t.IsUseGrilledOctopus)
        {
            SendItemTime(player, 8061, (int)(( TIME_ITEM_10M - (Now - t.LastTimeGrilledOctopus)) / 1000));
        }

        if (t.IsUseBreadedShrimp)
        {
            SendItemTime(player, 8062, (int)(( TIME_ITEM_10M - (Now - t.LastTimeBreadedShrimp)) / 1000));
        }

        if (t.IsUseStandardIpanaOintment)
        {
            SendItemTime(player, 9068, (int)(( TIME_ITEM_10M - (Now - t.LastTimeStandardIpanaOintment)) / 1000));
        }

        if (t.IsUseSpecialIpanaOintment)
        {
            SendItemTime(player, 9068, (int)(( TIME_ITEM_30M - (Now - t.LastTimeSpecialIpanaOintment)) / 1000));
        }

        if (t.IsUseCaterpillarHamburger)
        {
            SendItemTime(player, 8247, (int)(( TIME_ITEM_30M - (Now - t.LastTimeCaterpillarHamburger)) / 1000));
        }

        if (t.IsUseSpiderCake)
        {
            SendItemTime(player, 8246, (int)(( TIME_ITEM_30M - (Now - t.LastTimeSpiderCake)) / 1000));
        }

        if (t.IsUseDarkPumpkinSoup)
        {
            SendItemTime(player, 8244, (int)(( TIME_ITEM_30M - (Now - t.LastTimeDarkPumpkinSoup)) / 1000));
        }

        if (t.IsUseOneEyedCandy)
        {
            SendItemTime(player, 8243, (int)(( TIME_ITEM_30M - (Now - t.LastTimeOneEyedCandy)) / 1000));
        }

        if (t.IsFaceMask)
        {
            SendItemTime(player, 7149, (int)(( TIME_ITEM_30M - (Now - t.LastFaceMask)) / 1000));
        }

        if (t.IsCommeson)
        {
            SendItemTime(player, 5829, (int)(( TIME_ITEM_50M - (Now - t.LastCommeson)) / 1000));
        }

        if (t.HasDamageBoost30)
        {
            SendItemTime(player, 24360, (int)((TIME_ITEM_60M - (Now - t.DamageBoost30StartTime)) / 1000));
        }

        if (t.HasHpBoost30)
        {
            SendItemTime(player, 24361, (int)((TIME_ITEM_60M - (Now - t.HpBoost30StartTime)) / 1000));
        }

        if (t.HasEnergyBoost30)
        {
            SendItemTime(player, 24362, (int)((TIME_ITEM_60M - (Now - t.EnergyBoost30StartTime)) / 1000));
        }

        if (t.HasDamageBoost15)
        {
            SendItemTime(player, 24360, (int)((TIME_ITEM_30M - (Now - t.DamageBoost15StartTime)) / 1000));
        }

        if (t.HasHpBoost15)
        {
            SendItemTime(player, 24361, (int)((TIME_ITEM_30M - (Now - t.HpBoost15StartTime)) / 1000));
        }

        if (t.HasEnergyBoost15)
        {
            SendItemTime(player, 24362, (int)((TIME_ITEM_30M - (Now - t.EnergyBoost15StartTime)) / 1000));
        }

        if (t.IsUseStandardInvisibilityPotion)
        {
            SendItemTime(player, 24252, (int)((TIME_ITEM_1M - (Now - t.LastTimeStandardInvisibilityPotion)) / 1000));
        }

        if (t.IsUseEnhancedInvisibilityPotion)
        {
            SendItemTime(player, 24252, (int)((TIME_ITEM_10M - (Now - t.LastTimeEnhancedInvisibilityPotion)) / 1000));
        }

        if (t.IsUseRedPeach0)
        {
            SendItemTime(player, 4547, (int)((TIME_ITEM_10S - (Now - t.LastTimeRedPeach0)) / 1000));
        }

        if (t.IsUseRedPeach)
        {
            SendItemTime(player, 4547, (int)((TIME_ITEM_30S - (Now - t.LastTimeRedPeach)) / 1000));
        }

        if (t.IsUseRedPeach1)
        {
            SendItemTime(player, 4547, (int)((TIME_ITEM_30S - (Now - t.LastTimeRedPeach1)) / 1000));
        }

        if (t.IsUseRedPeach3)
        {
            SendItemTime(player, 4547, (int)((TIME_ITEM_30S - (Now - t.LastTimeRedPeach3)) / 1000));
        }

        if (t.IsUseRedPeach5)
        {
            SendItemTime(player, 4547, (int)((TIME_ITEM_1M - (Now - t.LastTimeRedPeach5)) / 1000));
        }

        if (t.IsUseRedPeach10)
        {
            SendItemTime(player, 4547, (int)((TIME_ITEM_1M - (Now - t.LastTimeRedPeach10)) / 1000));
        }

        if (t.IsUseRedPeach25)
        {
            SendItemTime(player, 4547, (int)((TIME_ITEM_1M - (Now - t.LastTimeRedPeach25)) / 1000));
        }

        if (t.IsUseRedPeach50)
        {
            SendItemTime(player, 4547, (int)((TIME_ITEM_10M - (Now - t.LastTimeRedPeach50)) / 1000));
        }

        if (t.IsUseRedPeach99)
        {
            SendItemTime(player, 4547, (int)((TIME_ITEM_10M - (Now - t.LastTimeRedPeach99)) / 1000));
        }

        if (t.IsUseRedPeach999)
        {
            SendItemTime(player, 4547, (int)((TIME_ITEM_10M - (Now - t.LastTimeRedPeach999)) / 1000));
        }

        if (t.IsUseBloodTonic)
        {
            SendItemTime(player, 2755, (int)((TIME_ITEM_10M - (Now - t.LastTimeBloodTonic)) / 1000));
        }

        if (t.IsUseEnergyTonic)
        {
            SendItemTime(player, 2756, (int)((TIME_ITEM_10M - (Now - t.LastTimeEnergyTonic)) / 1000));
        }

        if (t.IsUseXenArmor)
        {
            SendItemTime(player, 2757, (int)((TIME_ITEM_10M - (Now - t.LastTimeXenArmor)) / 1000));
        }

        if (t.IsUseRage)
        {
            SendItemTime(player, 2754, (int)((TIME_ITEM_10M - (Now - t.LastTimeRage)) / 1000));
        }

        if (t.IsUseIncognito)
        {
            SendItemTime(player, 2760, (int)((TIME_ITEM_10M - (Now - t.LastTimeIncognito)) / 1000));
        }

        if (t.IsUseBloodTonic2)
        {
            SendItemTime(player, 10714, (int)((TIME_ITEM_10M - (Now - t.LastTimeBloodTonic2)) / 1000));
        }

        if (t.IsUseEnergyTonic2)
        {
            SendItemTime(player, 10715, (int)((TIME_ITEM_10M - (Now - t.LastTimeEnergyTonic2)) / 1000));
        }

        if (t.IsUseXenArmor2)
        {
            SendItemTime(player, 10712, (int)((TIME_ITEM_10M - (Now - t.LastTimeXenArmor2)) / 1000));
        }

        if (t.IsUseRage2)
        {
            SendItemTime(player, 10716, (int)((TIME_ITEM_10M - (Now - t.LastTimeRage2)) / 1000));
        }

        if (t.IsUseIncognito2)
        {
            SendItemTime(player, 10717, (int)((TIME_ITEM_10M - (Now - t.LastTimeIncognito2)) / 1000));
        }

        if (t.IsOpenPower)
        {
            SendItemTime(player, 3783, (int)((TIME_OPEN_POWER - (Now - t.LastTimeOpenPower)) / 1000));
        }

        if (t.IsUseScanner)
        {
            SendItemTime(player, 2758, (int)((TIME_SCANNER - (Now - t.LastTimeUseScanner)) / 1000));
        }

        if (fx.IsCumber && fx.LastTimeCumber >= 0)
        {
            SendItemTime(player, 30907, (int)((180000 - (Now - fx.LastTimeCumber)) / 1000));
        }

        if (fx.IsCumber2 && fx.LastTimeCumber2 >= 0)
        {
            SendItemTime(player, 20126, (int)((180000 - (Now - fx.LastTimeCumber2)) / 1000));
        }

        if (fx.IsPain && fx.LastTimeUpPain >= 0)
        {
            SendItemTime(player, 25265, (int)((180000 - (Now - fx.LastTimeUpPain)) / 1000));
        }

        if (fx.IsKefla && fx.LastTimeKefla >= 0)
        {
            SendItemTime(player, 13769, (int)((300000 - (Now - fx.LastTimeKefla)) / 1000));
        }

        if (t.IsEatMeal)
        {
            SendItemTime(player, t.IconMeal, (int)((TIME_EAT_MEAL - (Now - t.LastTimeEatMeal)) / 1000));
        }

        if (t.IsAutoPlayEnabled)
        {
            SendItemTime(player, 4387, t.AutoPlayDurationMs / 1000);
        }
    }

    // Thực hiện clear all item time; giữ nguyên điều kiện, thứ tự và số học của Java (Java: clearAllItemTime).
    public void ClearAllItemTime(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        try
        {
            ItemTime t = player.ItemTime;
            t.IsUseMoonFestivalCakeBox = false;
            RemoveItemTime(player, 4126);

            t.IsUseSpecialMoonFestival = false;
            RemoveItemTime(player, 4125);

            t.IsUseMoonFestivalTwoEgg = false;
            RemoveItemTime(player, 4043);

            t.IsUseMoonFestivalOneEgg = false;
            RemoveItemTime(player, 4042);

            t.IsUseRabbitMooncake3 = false;
            RemoveItemTime(player, 11677);

            t.IsUseRabbitMooncake2 = false;
            RemoveItemTime(player, 11676);

            t.IsUseRabbitMooncake1 = false;
            RemoveItemTime(player, 11675);

            t.IsBoneDragon = false;
            RemoveItemTime(player, 6579);

            t.IsBoneDragon2 = false;
            RemoveItemTime(player, 6580);

            t.IsBoneDragon3 = false;
            RemoveItemTime(player, 6581);

            t.IsUseMysticOrbScanner = false;
            RemoveItemTime(player, 434);

            t.IsUseSoulScanner = false;
            RemoveItemTime(player, 11493);

            t.IsUsingSuperTransformationStoneDetector = false;
            RemoveItemTime(player, 27021);

            t.IsUsePetPowerPotentialCharm = false;
            RemoveItemTime(player, 13540);

            t.IsUseFourLeafClover = false;
            RemoveItemTime(player, 13618);

            t.IsUseDurian = false;
            RemoveItemTime(player, 13747);

            t.IsUseTrueDamage = false;
            RemoveItemTime(player, 12766);

            t.IsUseTrueDamage2 = false;
            RemoveItemTime(player, 12767);

            t.IsUseSuperRegen = false;
            RemoveItemTime(player, 27019);

            t.IsUseRocket1h = false;
            RemoveItemTime(player, 27009);

            t.IsUseKamejoko2 = false;
            RemoveItemTime(player, 30867);

            t.IsUseKamejoko = false;
            RemoveItemTime(player, 30867);

            t.IsUseReflectDamage3 = false;
            RemoveItemTime(player, 30866);

            t.IsUseReflectDamage2 = false;
            RemoveItemTime(player, 30866);

            t.IsUseReflectDamage = false;
            RemoveItemTime(player, 30866);

            t.IsUseDodge2 = false;
            RemoveItemTime(player, 32752);

            t.IsUseDodge = false;
            RemoveItemTime(player, 32752);

            t.IsUseCriticalChance3 = false;
            RemoveItemTime(player, 32751);

            t.IsUseCriticalChance2 = false;
            RemoveItemTime(player, 32751);

            t.IsUsePlumpMouse = false;
            RemoveItemTime(player, 12831);

            t.IsUseRoastChickenRice = false;
            RemoveItemTime(player, 12396);

            t.IsUseDragonFruitNoodles = false;
            RemoveItemTime(player, 12395);

            t.IsUseSmokedSausage = false;
            RemoveItemTime(player, 10890);

            t.IsUseCatFood = false;
            RemoveItemTime(player, 10891);

            t.IsUseCylindricalRiceCake = false;
            RemoveItemTime(player, 7079);

            t.IsUseSquareRiceCake = false;
            RemoveItemTime(player, 7080);

            t.IsUseElectricRiceCooker = false;
            RemoveItemTime(player, 11173);

            t.HasMagicDispel = false;
            RemoveItemTime(player, 3778);

            t.IsPowerBurst = false;
            RemoveItemTime(player, 10713);

            t.HasMonkeyTail = false;
            RemoveItemTime(player, 5072);

            t.IsUseTamarindCrab = false;
            RemoveItemTime(player, 8060);

            t.IsUseGrilledOctopus = false;
            RemoveItemTime(player, 8061);

            t.IsUseBreadedShrimp = false;
            RemoveItemTime(player, 8062);

            t.IsUseStandardIpanaOintment = false;
            RemoveItemTime(player, 9068);

            t.IsUseSpecialIpanaOintment = false;
            RemoveItemTime(player, 9068);

            t.IsUseCaterpillarHamburger = false;
            RemoveItemTime(player, 8247);

            t.IsUseSpiderCake = false;
            RemoveItemTime(player, 8246);

            t.IsUseDarkPumpkinSoup = false;
            RemoveItemTime(player, 8244);

            t.IsUseOneEyedCandy = false;
            RemoveItemTime(player, 8243);

            t.IsFaceMask = false;
            RemoveItemTime(player, 7149);

            t.IsCommeson = false;
            RemoveItemTime(player, 5829);

            t.HasDamageBoost30 = false;
            RemoveItemTime(player, 24360);

            t.HasHpBoost30 = false;
            RemoveItemTime(player, 24361);

            t.HasEnergyBoost30 = false;
            RemoveItemTime(player, 24362);

            t.HasDamageBoost15 = false;
            RemoveItemTime(player, 24360);

            t.HasHpBoost15 = false;
            RemoveItemTime(player, 24361);

            t.HasEnergyBoost15 = false;
            RemoveItemTime(player, 24362);

            t.IsUseStandardInvisibilityPotion = false;
            RemoveItemTime(player, 24252);

            t.IsUseEnhancedInvisibilityPotion = false;
            RemoveItemTime(player, 24252);

            t.IsUseRedPeach0 = false;
            RemoveItemTime(player, 4547);

            t.IsUseRedPeach = false;
            RemoveItemTime(player, 4547);

            t.IsUseRedPeach1 = false;
            RemoveItemTime(player, 4547);

            t.IsUseRedPeach3 = false;
            RemoveItemTime(player, 4547);

            t.IsUseRedPeach5 = false;
            RemoveItemTime(player, 4547);

            t.IsUseRedPeach10 = false;
            RemoveItemTime(player, 4547);

            t.IsUseRedPeach25 = false;
            RemoveItemTime(player, 4547);

            t.IsUseRedPeach50 = false;
            RemoveItemTime(player, 4547);

            t.IsUseRedPeach99 = false;
            RemoveItemTime(player, 4547);

            t.IsUseRedPeach999 = false;
            RemoveItemTime(player, 4547);

            t.IsUseBloodTonic = false;
            RemoveItemTime(player, 2755);

            t.IsUseEnergyTonic = false;
            RemoveItemTime(player, 2756);

            t.IsUseXenArmor = false;
            RemoveItemTime(player, 2757);

            t.IsUseRage = false;
            RemoveItemTime(player, 2754);

            t.IsUseIncognito = false;
            RemoveItemTime(player, 2760);

            t.IsUseBloodTonic2 = false;
            RemoveItemTime(player, 10714);

            t.IsUseEnergyTonic2 = false;
            RemoveItemTime(player, 10715);

            t.IsUseXenArmor2 = false;
            RemoveItemTime(player, 10712);

            t.IsUseRage2 = false;
            RemoveItemTime(player, 10716);

            t.IsUseIncognito2 = false;
            RemoveItemTime(player, 10717);

            t.IsAutoPlayEnabled = false;
            RemoveItemTime(player, 4387);

            t.IsUseScanner = false;
            RemoveItemTime(player, 2758);

            t.IsOpenPower = false;
            RemoveItemTime(player, 3783);

            host.SendPoint(player);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(ItemTimeService), e);
        }
    }

    // Bật turn on tdlt; giữ nguyên điều kiện, thứ tự và số học của Java (Java: turnOnTDLT).
    public void TurnOnAutoPlay(Player player, Item item)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(item);
        int min = 0;
        foreach (ItemOption io in item.ItemOptions)
        {
            if (io.OptionTemplate.Id == 1)
            {
                min = io.Param;
                io.Param = 0;
                break;
            }
        }

        ItemTime t = player.ItemTime;
        t.IsAutoPlayEnabled = true;
        t.AutoPlayDurationMs = unchecked(min * 60 * 1000);
        t.LastTimeAutoPlay = Now;
        SendCanAutoPlay(player);
        SendItemTime(player, 4387, t.AutoPlayDurationMs / 1000);
        host.SendItemBag(player);
    }

    // Tắt turn off tdlt; giữ nguyên điều kiện, thứ tự và số học của Java (Java: turnOffTDLT).
    public void TurnOffAutoPlay(Player player, Item item)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(item);
        ItemTime t = player.ItemTime;
        t.IsAutoPlayEnabled = false;
        foreach (ItemOption io in item.ItemOptions)
        {
            if (io.OptionTemplate.Id == 1)
            {
                io.Param += unchecked((short)((t.AutoPlayDurationMs - (Now - t.LastTimeAutoPlay)) / 60 / 1000));
                break;
            }
        }

        SendCanAutoPlay(player);
        RemoveItemTime(player, 4387);
        host.SendItemBag(player);
    }

    // Gửi send can auto play; giữ nguyên điều kiện, thứ tự và số học của Java (Java: sendCanAutoPlay).
    public void SendCanAutoPlay(Player player)
    {
        try
        {
            Message msg = BuildCanAutoPlayMessage(player.ItemTime.IsAutoPlayEnabled);
            host.SendMessage(player, msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(ItemTimeService), e);
        }
    }

    // Gửi send text doanh trai; giữ nguyên điều kiện, thứ tự và số học của Java (Java: sendTextDoanhTrai).
    public void SendTextBarracks(Player player)
    {
        ClanDungeonTimes? clan = host.GetExternalItemTimes(player).Clan;
        if (clan != null && !clan.HaveGoneBarracks && clan.LastTimeOpenBarracks != 0)
        {
            int secondPassed = (int)((Now - clan.LastTimeOpenBarracks) / 1000);
            int secondsLeft = (clan.BarracksDurationMs / 1000) - secondPassed;
            if (secondsLeft < 0 || secondsLeft > 1800)
            {
                return;
            }

            SendTextTime(player, BARRACKS, "Trại độc nhãn:", secondsLeft);
        }
    }

    // Gửi send text time pick doanh trai; giữ nguyên điều kiện, thứ tự và số học của Java (Java: sendTextTimePickDoanhTrai).
    public void SendTextTimePickBarracks(Player player)
    {
        ClanDungeonTimes? clan = host.GetExternalItemTimes(player).Clan;
        if (clan != null && clan.HasBarracks && clan.BarracksIsTimePicking)
        {
            int secondPassed = (int)((Now - clan.BarracksLastTimePick) / 1000);
            int secondsLeft = (clan.BarracksPickDurationMs / 1000) - secondPassed;
            if (secondsLeft < 0 || secondsLeft > 1800)
            {
                return;
            }

            SendTextTime(player, BARRACKS, "Trại độc nhãn:", secondsLeft);
        }
    }

    // Java sendTextBanDoKhoBau(player). (Java: sendTextBanDoKhoBau(player)).
    public void SendTextTreasureUnderSea(Player player)
    {
        ClanDungeonTimes? clan = host.GetExternalItemTimes(player).Clan;
        if (clan != null && clan.LastTimeOpenTreasureUnderSea != 0)
        {
            int secondPassed = (int)((Now - clan.LastTimeOpenTreasureUnderSea) / 1000);
            int secondsLeft = (clan.TreasureUnderSeaDurationMs / 1000) - secondPassed;
            if (secondsLeft < 0 || secondsLeft > 1800)
            {
                return;
            }

            SendTextTime(player, TREASURE_UNDER_SEA, "Hang kho báu:", secondsLeft);
        }
    }

    // Gửi send text xinbato; giữ nguyên điều kiện, thứ tự và số học của Java (Java: sendTextXinbato).
    public void SendTextXinbato(Player player)
    {
        SendTextTime(player, TREASURE_UNDER_SEA, "Tìm nước cho Xinbatô ở đảo Kame hoặc đảo Guru", 30);
    }

    // Java sendTextConDuongRanDoc(player). (Java: sendTextConDuongRanDoc(player)).
    public void SendTextSnakeWay(Player player)
    {
        ClanDungeonTimes? clan = host.GetExternalItemTimes(player).Clan;
        if (clan != null && clan.LastTimeOpenSnakeWay != 0)
        {
            int secondPassed = (int)((Now - clan.LastTimeOpenSnakeWay) / 1000);
            int secondsLeft = (clan.SnakeWayDurationMs / 1000) - secondPassed;
            if (secondsLeft < 0 || secondsLeft > 1800)
            {
                return;
            }

            SendTextTime(player, SNAKE_WAY, "Con đường rắn độc:", secondsLeft);
        }
    }

    // Java sendTextKhiGasHuyDiet(player). (Java: sendTextKhiGasHuyDiet(player)).
    public void SendTextDestronGas(Player player)
    {
        ClanDungeonTimes? clan = host.GetExternalItemTimes(player).Clan;
        if (clan != null && clan.LastTimeOpenDestronGas != 0)
        {
            int secondPassed = (int)((Now - clan.LastTimeOpenDestronGas) / 1000);
            int secondsLeft = (clan.DestronGasDurationMs / 1000) - secondPassed;
            if (secondsLeft < 0 || secondsLeft > 1800)
            {
                return;
            }

            SendTextTime(player, DESTRON_GAS, "Khí gas hủy diệt:", secondsLeft);
        }
    }

    // Gửi send text map boss bang hoi; giữ nguyên điều kiện, thứ tự và số học của Java (Java: sendTextMapBossBangHoi).
    public void SendTextClanBossMap(Player player)
    {
        ClanDungeonTimes? clan = host.GetExternalItemTimes(player).Clan;
        if (clan != null && clan.LastTimeOpenBossOfTheGangs != 0)
        {
            int secondPassed = (int)((Now - clan.LastTimeOpenBossOfTheGangs) / 1000);
            int secondsLeft = (clan.BossOfTheGangsDurationMs / 1000) - secondPassed;
            if (secondsLeft < 0 || secondsLeft > 1800)
            {
                return;
            }

            SendTextTime(player, CLAN_BOSS_MAP, "Hạ Boss Bang Hội [lần thứ " + (clan.BossClanRound - 1) + "] thời gian: ", secondsLeft);
        }
    }

    // Java removeTextDoanhTrai(player). (Java: removeTextDoanhTrai(player)).
    public void RemoveTextBarracks(Player player) => RemoveTextTime(player, BARRACKS);

    // Java removeTextBanDoKhoBau(player). (Java: removeTextBanDoKhoBau(player)).
    public void RemoveTextTreasureUnderSea(Player player) => RemoveTextTime(player, TREASURE_UNDER_SEA);

    // Java removeTextConDuongRanDoc(player). (Java: removeTextConDuongRanDoc(player)).
    public void RemoveTextSnakeWay(Player player) => RemoveTextTime(player, SNAKE_WAY);

    // Java removeTextKhiGasHuyDiet(player). (Java: removeTextKhiGasHuyDiet(player)).
    public void RemoveTextDestronGas(Player player) => RemoveTextTime(player, DESTRON_GAS);

    // Java removeTextMapBossBangHoi(player). (Java: removeTextMapBossBangHoi(player)).
    public void RemoveTextClanBossMap(Player player) => RemoveTextTime(player, CLAN_BOSS_MAP);

    // Xóa remove text time; giữ nguyên điều kiện, thứ tự và số học của Java (Java: removeTextTime).
    public void RemoveTextTime(Player player, sbyte id) => SendTextTime(player, id, "", 0);

    // Gửi send text time; giữ nguyên điều kiện, thứ tự và số học của Java (Java: sendTextTime).
    public void SendTextTime(Player player, sbyte id, string text, int seconds)
    {
        try
        {
            var msg = new Message(65);
            msg.Writer().WriteByte(id);
            msg.Writer().WriteUTF(text);
            msg.Writer().WriteShort(seconds);
            host.SendMessage(player, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(ItemTimeService), e);
        }
    }

    // Gửi send item time; giữ nguyên điều kiện, thứ tự và số học của Java (Java: sendItemTime).
    public void SendItemTime(Player player, int itemId, int time)
    {
        try
        {
            var msg = new Message(-106);
            msg.Writer().WriteShort(itemId);
            msg.Writer().WriteShort(time);
            host.SendMessage(player, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(ItemTimeService), e);
        }
    }

    // Xóa remove item time; giữ nguyên điều kiện, thứ tự và số học của Java (Java: removeItemTime).
    public void RemoveItemTime(Player player, int itemTime) => SendItemTime(player, itemTime, 0);
}
