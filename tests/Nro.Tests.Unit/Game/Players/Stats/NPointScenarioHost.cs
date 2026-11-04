using System.Reflection;
using System.Text.Json;

using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Game;
using Nro.Tests.Unit.Game.Items;
using Nro.Tests.Unit.Game.Items.Time;

namespace Nro.Tests.Unit.Game.Players.Stats;

/// <summary>
/// Builds the objects of one scenario of <c>npoint-characterization.json</c> (the same inputs the Java harness gave the original NPoint) and acts as the <see cref="Nro.Game.Players.Stats.Ports.INPointHost"/>
/// for them: it fills the facts from the scenario values, records every outbound call as the same text the Java test doubles logged, and keeps the few values NPoint writes back.
/// It derives from the production <see cref="PlayerNPointHost"/>, so what the production host reads from the Player shell (flags, cards, ids, the pet's NPoint) is exercised too.
/// </summary>
internal sealed class NPointScenarioHost : PlayerNPointHost
{
    /// <summary>Scenario data of one player.</summary>
    internal sealed class Store
    {
        public Store(Player player, string kind)
        {
            Player = player;
            Kind = kind;
        }

        public Player Player { get; }

        public string Kind { get; }

        /// <summary>The collaborator values of this player (what the host reports in <see cref="NPointFacts"/>); the groups are copied into each snapshot.</summary>
        public NPointFacts Facts { get; } = new();

        public sbyte PetStatus { get; set; }
        public sbyte PetType { get; set; }
        public int LocX { get; set; }
        public int LocY { get; set; }
        public bool IsCharging { get; set; }
        public int CountCharging { get; set; }
        public bool HaveEffectSkill { get; set; }
        public List<(int X, int Y, long Hp, bool Die)> Mobs { get; } = [];
        public List<(long Id, int X, int Y, long Hp, bool Die)> Humans { get; } = [];
    }

    private readonly Dictionary<Player, Store> stores = [];
    private readonly Dictionary<Player, Player> masters = [];

    public List<string> Log { get; } = [];

    public bool ReadInt { get; set; } = true;
    public int RateExp { get; set; } = 10;
    public bool WomansDay { get; set; }
    public int WdHp { get; set; }
    public int WdMp { get; set; }
    public int WdDame { get; set; }
    public Dictionary<int, (int Value, bool Expired)> Attributes { get; } = [];
    public List<long> Whis { get; } = [];
    public Dictionary<int, PowerLimitInfo> Limits { get; } = [];

    public Store StoreOf(Player player) => stores[player];

    /// <summary>Creates the player (and its pet / master) from the scenario spec. <paramref name="makeNPoint"/> wires the clock and the host into each NPoint.</summary>
    public Player Build(JsonElement spec, Player? masterOverride, Action<NPoint> makeNPoint)
    {
        string kind = spec.GetProperty("kind").GetString()!;
        Player player;
        if (kind == "detu")
        {
            Player master = masterOverride ?? (spec.TryGetProperty("master", out JsonElement m) && m.ValueKind == JsonValueKind.Object ? Build(m, null, makeNPoint) : NewBare(makeNPoint));
            player = new Pet();
            masters[player] = master;
        }
        else if (kind == "phanthan")
        {
            Player master = spec.TryGetProperty("master", out JsonElement m) && m.ValueKind == JsonValueKind.Object ? Build(m, null, makeNPoint) : NewBare(makeNPoint);
            player = new BodyClone();
            masters[player] = master;
        }
        else
        {
            player = new Player();
        }

        Store store = new(player, kind);
        stores[player] = store;
        player.NPoint = new NPoint(player);
        makeNPoint(player.NPoint);

        foreach (JsonElement kv in spec.GetProperty("sets").EnumerateArray())
        {
            ApplySet(store, kv[0].GetString()!, kv[1]);
        }

        foreach (JsonElement it in spec.GetProperty("body").EnumerateArray())
        {
            store.Facts.Inventory.ItemsBody.Add(BuildItem(it));
        }

        foreach (JsonElement it in spec.GetProperty("bag").EnumerateArray())
        {
            store.Facts.Inventory.ItemsBag.Add(BuildItem(it));
        }

        if (spec.TryGetProperty("trainArmor", out JsonElement ta) && ta.ValueKind == JsonValueKind.Object)
        {
            store.Facts.Inventory.TrainingArmor = BuildItem(ta);
        }

        foreach (JsonElement c in spec.GetProperty("cards").EnumerateArray())
        {
            NPointCard card = new() { Used = (sbyte)c.GetProperty("used").GetInt32(), Level = (sbyte)c.GetProperty("level").GetInt32() };
            foreach (JsonElement o in c.GetProperty("opts").EnumerateArray())
            {
                card.Options.Add(new NPointCardOption { Id = o[0].GetInt32(), Param = o[1].GetInt32(), Active = (sbyte)o[2].GetInt32() });
            }

            store.Facts.Cards.Add(card);
        }

        foreach (JsonElement b in spec.GetProperty("badge").EnumerateArray())
        {
            store.Facts.BadgeOptions.Add(new NPointOption(b[0].GetInt32(), b[1].GetInt32()));
        }

        foreach (JsonElement mob in spec.GetProperty("mobs").EnumerateArray())
        {
            if (store.Facts.Zone is null)
            {
                store.Facts.Zone = new NPointZoneFacts();
            }

            store.Mobs.Add((mob[0].GetInt32(), mob[1].GetInt32(), mob[2].GetInt64(), mob[3].GetBoolean()));
        }

        foreach (JsonElement h in spec.GetProperty("humans").EnumerateArray())
        {
            if (store.Facts.Zone is null)
            {
                store.Facts.Zone = new NPointZoneFacts();
            }

            // the Java harness builds a stub player whose hp is the given hp, or 0 when it is dead (isDie() is hp <= 0)
            long hp = h[4].GetBoolean() ? 0 : h[3].GetInt64();
            store.Humans.Add((h[0].GetInt64(), h[1].GetInt32(), h[2].GetInt32(), hp, h[4].GetBoolean()));
        }

        if (spec.TryGetProperty("detu", out JsonElement petSpec) && petSpec.ValueKind == JsonValueKind.Object)
        {
            player.Pet = (Pet)Build(petSpec, player, makeNPoint);
        }

        return player;
    }

    private Player NewBare(Action<NPoint> makeNPoint)
    {
        Player p = new();
        stores[p] = new Store(p, "player");
        p.NPoint = new NPoint(p);
        makeNPoint(p.NPoint);
        return p;
    }

    internal static NPointItem BuildItem(JsonElement spec)
    {
        if (spec.TryGetProperty("empty", out _))
        {
            return new NPointItem { IsNotNullItem = false };
        }

        NPointItem item = new() { TemplateId = spec.GetProperty("id").GetInt32(), TemplateType = spec.GetProperty("type").GetInt32() };
        item.TrainArmorPercent = TrainArmorPercent(item.TemplateId);
        JsonElement opts = spec.GetProperty("opts");
        if (opts.ValueKind == JsonValueKind.Null)
        {
            item.ItemOptions = null;
        }
        else
        {
            foreach (JsonElement o in opts.EnumerateArray())
            {
                item.ItemOptions!.Add(new NPointOption(o[0].GetInt32(), o[1].GetInt32()));
            }
        }

        return item;
    }

    /// <summary>Java <c>ItemService.getPercentTrainArmor</c> (ItemService belongs to track D; the table is repeated here for the scenarios).</summary>
    private static int TrainArmorPercent(int templateId) => templateId switch
    {
        529 or 534 => 10,
        530 or 535 => 20,
        531 or 536 => 30,
        1797 => 40,
        _ => 0,
    };

    // ---- scenario sets ----

    public void ApplySet(Store store, string path, JsonElement v)
    {
        int dot = path.IndexOf('.', StringComparison.Ordinal);
        string group = path[..dot];
        string field = path[(dot + 1)..];
        NPointFacts f = store.Facts;
        switch (group)
        {
            case "p":
                SetCSharpProperty(store.Player, CSharpPlayerPropertyName(field), v);
                break;
            case "npoint":
                SetProperty(store.Player.NPoint, field, v);
                break;
            case "it":
                SetProperty(f.ItemTime, ItemTimeSpec.CSharpName(field), v);
                break;
            case "es":
                if (field == "haveEffectSkill")
                {
                    store.HaveEffectSkill = v.GetBoolean();
                }
                else if (field == "isCharging")
                {
                    store.IsCharging = v.GetBoolean();
                }
                else if (field == "countCharging")
                {
                    store.CountCharging = v.GetInt32();
                }
                else
                {
                    SetProperty(f.EffectSkill, field, v);
                }

                break;
            case "ek":
                SetProperty(f.EffectSkin, field, v);
                break;
            case "sc":
                SetProperty(f.SetClothes, CSharpSetClothesPropertyName(field), v);
                break;
            case "ch":
                SetProperty(f.Charms, field, v);
                break;
            case "clan":
                f.Clan ??= new NPointClanFacts();
                SetProperty(f.Clan, field, v);
                break;
            case "sat":
                f.Player.SatelliteIsIntelligent = v.GetBoolean();
                break;
            case "fb":
                f.Player.UseFishSkewer = v.GetBoolean();
                break;
            case "inv":
                switch (field)
                {
                    case "rongNhi":
                        f.Inventory.HasRongNhi = v.GetBoolean();
                        break;
                    case "hpBox":
                        f.Inventory.HpItemsInBox = v.GetInt32();
                        break;
                    case "mpBox":
                        f.Inventory.MpItemsInBox = v.GetInt32();
                        break;
                    case "dameBox":
                        f.Inventory.DamageItemsInBox = v.GetInt32();
                        break;
                    case "critBox":
                        f.Inventory.CritItemsInBox = v.GetInt32();
                        break;
                    default:
                        throw new InvalidOperationException("unknown inventory setting " + field);
                }

                break;
            case "intr":
                SetProperty(f.Intrinsic, field, v);
                break;
            case "skill":
                f.SkillSelect ??= new NPointSkillFacts();
                SetProperty(f.SkillSelect, field == "id" ? "templateId" : field, v);
                break;
            case "fusion":
                f.TypeFusion = (sbyte)v.GetInt32();
                break;
            case "pf":
                f.FusionDamage = v.GetInt32();
                break;
            case "bb":
                f.BlackBallTimeOut[int.Parse(field, System.Globalization.CultureInfo.InvariantCulture)] = v.GetInt64();
                break;
            case "zone":
                f.Zone ??= new NPointZoneFacts();
                SetProperty(f.Zone, field == "type" ? "mapType" : field, v);
                break;
            case "session":
                f.Player.HasVipPoint = v.GetInt32() > 0;
                break;
            case "loc":
                if (field == "x")
                {
                    store.LocX = v.GetInt32();
                }
                else
                {
                    store.LocY = v.GetInt32();
                }

                break;
            case "dt":
                if (field == "typeDeTu")
                {
                    store.PetType = (sbyte)v.GetInt32();
                }
                else if (field == "status")
                {
                    store.PetStatus = (sbyte)v.GetInt32();
                }
                else
                {
                    throw new InvalidOperationException("unknown detu setting " + field);
                }

                break;
            default:
                throw new InvalidOperationException("unknown group " + group);
        }
    }

    // Ánh xạ tên trường Java trong corpus đối chiếu sang property C# đã đổi tên.
    internal static string CSharpPropertyName(string javaName)
    {
        string pascalName = char.ToUpperInvariant(javaName[0]) + javaName[1..];
        if (javaName == "wearingVoHinh")
        {
            return "WearingInvisibilityArmor";
        }

        if (javaName == "isMapVohinh")
        {
            return "IsInvisibleMap";
        }

        return pascalName switch
        {
            "IsCrit" => "IsCritical",
            "IsCrit100" => "IsGuaranteedCritical",
            "IsCritTele" => "IsTeleportCritical",
            "DameAfter" => "DamageAfter",
            "TiemNang" => "Potential",
            "Hpg" => "BaseHp",
            "Mpg" => "BaseMp",
            "Dameg" => "BaseDamage",
            "Dame" => "Damage",
            "Defg" => "BaseDefense",
            "Def" => "Defense",
            "Critg" => "BaseCriticalChance",
            "Crit" => "CriticalChance",
            "KhangTDHS" => "IsSolarFlareResistant",
            "KhangHoaXuong" => "IsBoneTransformationImmune",
            "IsTacDungHopThe" => "IsFusionActive",
            "TlSpeed" => "SpeedPercent",
            "TlMayman" => "LuckPercent",
            "HpAdd" => "HpBonus",
            "MpAdd" => "MpBonus",
            "DameAdd" => "DamageBonus",
            "DefAdd" => "DefenseBonus",
            "CritAdd" => "CriticalChanceBonus",
            "HpHoiAdd" => "HpRegenBonus",
            "MpHoiAdd" => "MpRegenBonus",
            "TlDameCrit" => "CriticalDamagePercents",
            "TlSDCM" => "CriticalDamagePercent",
            "TlHp" => "HpPercents",
            "TlMp" => "MpPercents",
            "TlDef" => "DefensePercents",
            "TlDame" => "DamagePercents",
            "TlDameAttMob" => "MobAttackDamagePercents",
            "HpHoi" => "HpRegen",
            "MpHoi" => "MpRegen",
            "MpHoiCute" => "CuteMpRegen",
            "TlHpHoi" => "HpRegenPercent",
            "TlMpHoi" => "MpRegenPercent",
            "TlHpHoiBanthan_DongMinh" => "SelfAndAllyHpRegenPercent",
            "TlHpHoiBanThanVaDongDoi" => "SelfAndTeamHpRegenPercent",
            "TlMpHoiBanThanVaDongDoi" => "SelfAndTeamMpRegenPercent",
            "TlHutHp" => "HpAbsorptionPercent",
            "TlHutMp" => "MpAbsorptionPercent",
            "TlHutHpMob" => "MobHpAbsorptionPercent",
            "TlHutHpMpXQ" => "AroundHpMpAbsorptionPercent",
            "TlFixStun" => "ControlResistancePercent",
            "TlCuteAddame" => "CuteDamagePercent",
            "TlPST" => "ReflectDamagePercent",
            "TlTNSM" => "PowerPotentialPercents",
            "TlGold" => "GoldPercent",
            "TlNeDon" => "DodgePercent",
            "TlBom" => "BombPercent",
            "TlGiap" => "ArmorPercent",
            "Tlxgcc" => "MeleeArmorPenetrationPercent",
            "Tlxgc" => "RangedArmorPenetrationPercent",
            "Tlchinhxac" => "AccuracyPercent",
            "Tlstc" => "TrueDamagePercent",
            "TlCongDonSD" => "StackingDamagePercent",
            "TlSubDamePercenMp20" => "LowMpDamageReductionPercent",
            "TlTanCongTocTraiDat" => "EarthlingAttackPercent",
            "TlTanCongTocNamec" => "NamekianAttackPercent",
            "TlTanCongTocXayda" => "SaiyanAttackPercent",
            "TlGiamSatThuongTraiDat" => "EarthlingDamageReductionPercent",
            "TlGiamSatThuongNamec" => "NamekianDamageReductionPercent",
            "TlGiamSatThuongXayda" => "SaiyanDamageReductionPercent",
            "TlDameClan" => "ClanDamagePercent",
            "TlHpClan" => "ClanHpPercent",
            "TlMpClan" => "ClanMpPercent",
            "TlDameClanAdd" => "ClanDamageBonusPercent",
            "TlHpClanAdd" => "ClanHpBonusPercent",
            "TlMpClanAdd" => "ClanMpBonusPercent",
            "CongTiemNangSucManhToanBangHoi" => "ClanPowerPotentialBonus",
            "TlDameBoss" => "BossDamagePercent",
            "TlDameMobRun" => "RunningMobDamagePercent",
            "TlDameMobMonkey" => "MonkeyMobDamagePercent",
            "TlDameMobFly" => "FlyingMobDamagePercent",
            "IsThoDaiCa" => "IsRabbitLeader",
            "IsThoDaiKa" => "IsRabbitLeader",
            "XHPKI" => "HpKiMultiplier",
            "XDame" => "DamageMultiplier",
            "IsXChuong" => "IsBlastDamageMultiplierReady",
            "TiLeHPHuytSao" => "HpWhistlePercent",
            "IsDameBuff" => "IsDamageBuff",
            "TileDameBuff" => "DamageBuffPercent",
            "Iskefla" => "IsKefla",
            "Iscumber" => "IsCumber",
            "Iscumber2" => "IsCumber2",
            "IsSUcManhBocPha" => "IsPowerBurst",
            "IsBongTuyet" => "IsSnowflake",
            "TlTNSMPet" => "PetPowerPotentialPercent",
            "XChuong" => "BlastDamageMultiplier",
            "SetTinhAn" => "StarSealSetCount",
            "SetNhatAn" => "NhatSealSetCount",
            "SetNguyetAn" => "NguyetSealSetCount",
            "TlSexyDame" => "SexyDamagePercent",
            "TlCoolDame" => "CoolDamagePercent",
            "TlSubSD" => "DamageReductionPercent",
            "TlSubHP" => "HpReductionPercent",
            "TlSubMP" => "MpReductionPercent",
            "VoHieuChuong" => "BlastNullification",
            "IsKhongLanh" => "IsColdImmune",
            "IsTinhAn" => "HasStarSeal",
            "IsNhatAn" => "HasNhatSeal",
            "IsNguyetAn" => "HasNguyetSeal",
            "IsHoaDa" => "IsPetrified",
            "IsDoSPL" => "HasCrystalStarEquipment",
            "IsThoBulma" => "IsRabbitBulma",
            "IsBunmaTocMau" => "IsBulmaColoredHair",
            "IsTiecBaiBien" => "IsBeachParty",
            "TlHpGiamODo" => "PercentDirtyHpLoss",
            "Cong20ExpKhiAttackMob" => "Grants20ExperienceOnMobAttack",
            "LevelBT" => "EarringLevel",
            "IsTanHinh" => "IsInvisible",
            "IsHoaBiNgoXungQuanh" => "IsPumpkinAura",
            "IsUseCuongNo" => "IsUseRage",
            "IsUseCuongNo2" => "IsUseRage2",
            "LastTimeCuongNo" => "LastTimeRage",
            "LastTimeCuongNo2" => "LastTimeRage2",
            "TdDeTu" => "PetTraining",
            "TdDeTu2" => "PetTraining2",
            "TdDeTu3" => "PetTraining3",
            "TdDeTu4" => "PetTraining4",
            "TdDeTu5" => "PetTraining5",
            "TdDeTu7" => "PetTraining7",
            "TdDeTu10" => "PetTraining10",
            "TdDeTu20" => "PetTraining20",
            "BuaTriTue" => "WisdomCharmExpiry",
            "IsMapBanDoKhoBau" => "IsTreasureUnderSeaMap",
            "IsMapDoanhTrai" => "IsBarracksMap",
            "IsTnsmDividedMap" => "IsPowerPotentialDividedMap",
            _ => pascalName,
        };
    }

    // Player dùng property C# đã đổi tên; corpus scenario vẫn giữ chính tả Java.
    private static string CSharpPlayerPropertyName(string javaName)
    {
        if (javaName.StartsWith("nhanKeoHayBiGheoNpc_", StringComparison.Ordinal)
            && int.TryParse(javaName.AsSpan("nhanKeoHayBiGheoNpc_".Length), out int candyIndex) && candyIndex is >= 1 and <= 15)
            return $"HalloweenNpcCandyOutcome_{candyIndex}";
        if (javaName.StartsWith("nhanLiXiForNPC_", StringComparison.Ordinal)
            && int.TryParse(javaName.AsSpan("nhanLiXiForNPC_".Length), out int luckyMoneyIndex) && luckyMoneyIndex is >= 1 and <= 25)
            return $"LuckyMoneyReceivedFromNpc_{luckyMoneyIndex}";

        string pascalName = char.ToUpperInvariant(javaName[0]) + javaName[1..];
        return pascalName switch
        {
            "HienThiHopThe" => "FusionDisplayState", "Resetdame" => "ResetDamage", "LastTimeDame" => "LastDamageTime", "Dametong" => "TotalDamage",
            "LastTimeDameMayDam" => "LastPunchingMachineDamageTime", "TotalDameMayDam" => "PunchingMachineTotalDamage",
            "TlDameAdd" => "DamageBonusPercent", "TlDameClanAdd" => "ClanDamageBonusPercent", "TlHpClanAdd" => "ClanHpBonusPercent", "TlMpClanAdd" => "ClanMpBonusPercent",
            "TrangThaiLichSuGd" => "TransactionHistoryStatus", "TopMayDamTraiDat" => "EarthlingPunchingMachineRank", "TopMayDamNamec" => "NamekianPunchingMachineRank", "TopMayDamXayda" => "SaiyanPunchingMachineRank",
            "DiemDanhBangHoi" => "ClanCheckInCount", "VeTangNgoc_SoLuongNgoc" => "GiftGemVoucherCount", "Player_NhanNgoc" => "GemRecipient", "VeTangHongNgoc_SoLuongHongNgoc" => "GiftRubyVoucherCount", "Player_NhanHongNgoc" => "RubyRecipient",
            "RestartNauBanh" => "RestartCakeCooking", "Khisukien" => "IsEventGasActive", "Lasttimekhisukien" => "LastEventGasTime", "LastTimeCallKhi" => "LastGasCallTime", "PartDanhHieu" => "TitlePart",
            "IsDeTu" => "IsPet", "IsDuongTang" => "IsTangMonk", "IsPhanThan" => "IsBodyClone", "IsNguoiYeu" => "IsLover", "IsConOne" => "IsChildOne", "IsConTwo" => "IsChildTwo", "IsConThree" => "IsChildThree",
            "NhapNgocRongTimes" => "DragonBallInsertCount", "CheckTrongNgay" => "DailyCheckTimestamp",
            "DuaTopTangLiXi" => "GiveLuckyMoneyRankingScore", "DuaTopMoLiXi" => "OpenLuckyMoneyRankingScore", "DuaTopBanPhaoHoa" => "LaunchFireworksRankingScore", "DuaTopBanPhaoHoaVIP" => "LaunchVipFireworksRankingScore",
            "DuaTopTrangTriCayNoel" => "DecorateChristmasTreeRankingScore", "DuaTopCheTaoNguoiTuyet" => "CraftSnowmanRankingScore", "DuaTopCheTaoNguoiTuyetBangGia" => "CraftFrozenSnowmanRankingScore", "DuaTopDotDiem" => "BurnMatchesRankingScore",
            "DuaTopPhaoHoaVuLan" => "LaunchVuLanFirecrackersRankingScore", "DuaTopHoaDang" => "FlowerLanternRankingScore", "DuaTopHoaDangCoLoiChuc" => "WishLanternRankingScore",
            "DuaTopMoHopMaQuy" => "OpenGhostGiftBoxRankingScore", "DuaTopThiepHalloween" => "HalloweenCardRankingScore", "DuaTopMoThiep83" => "OpenWomensDayCardRankingScore", "DuaTopTangBongHoaHong" => "GiveRosesRankingScore",
            "DuaTopLamBanhTrungThu" => "MakeMoonCakeRankingScore", "DuaTopMoHopTrungThuDacBiet" => "OpenSpecialMoonCakeRankingScore", "UocMienPhi" => "FreeWishCount",
            "NhanQuaHungVuongFree" => "ReceivedHungKingFreeGift", "LastTimeNauBanhHungVuong" => "LastHungKingCakeCookingTime", "TypeBanhDangNau" => "CookingCakeType",
            "DuaTopMoHopQuaGioTo" => "OpenHungKingGiftBoxRankingScore", "DuaTopDangBanhHungVuong" => "OfferHungKingCakesRankingScore", "DuaTopMoTrungRongVang" => "OpenGoldenDragonEggRankingScore", "DuaTopDoiDuaHau" => "ExchangeWatermelonRankingScore",
            "DuaTopMoHopBlackFriday" => "OpenBlackFridayBoxRankingScore", "DuaTopMuaSamBlackFriday" => "BlackFridayShoppingRankingScore", "DuaTopMoHop20_10" => "OpenWomensDayGiftBoxRankingScore", "DuaTopMoThiepChuc" => "OpenGreetingCardRankingScore", "DuaTopTangThiepChuc" => "GiveGreetingCardRankingScore",
            "HoTongDuongTang" => "IsEscortingTangMonk", "MapHoTong" => "TangMonkEscortMapId", "LastTimeDuongTang" => "LastTangMonkTime",
            "IsHoldNamecBallTranhDoat" => "IsHoldingContestedNamekBall", "TempIdNamecBallHoldTranhDoat" => "ContestedNamekBallTemplateId",
            "ThongBaoChangeMap" => "ShowMapChangeNotification", "TextThongBaoChangeMap" => "MapChangeNotificationText", "ThongBaoThua" => "ShowLossNotification", "TextThongBaoThua" => "LossNotificationText",
            "ThoiVangVoDaiSinhTu" => "DeathmatchGoldTime", "BinhChonHatMit" => "JackfruitVote", "BinhChonPlayer" => "PlayerVote", "ZoneBinhChon" => "VotingZone",
            "IsBo" => "IsFather", "IsMe" => "IsMother", "IsPhuHoMapMabu" => "IsMabuMapProtected",
            "DoesNotAttackBo" => "FatherDoesNotAttack", "LastTimePlayerNotAttackBo" => "LastFatherAttackBlockPlayerTime", "TimeNotAttackBo" => "FatherAttackBlockDuration", "TemporaryEnemiesBo" => "FatherTemporaryEnemies", "JustRevivedBo" => "FatherJustRevived", "LastTimeRevivedBo" => "LastFatherRevivedTime",
            "DoesNotAttackMe" => "MotherDoesNotAttack", "LastTimePlayerNotAttackMe" => "LastMotherAttackBlockPlayerTime", "TimeNotAttackMe" => "MotherAttackBlockDuration", "TemporaryEnemiesMe" => "MotherTemporaryEnemies", "JustRevivedMe" => "MotherJustRevived", "LastTimeRevivedMe" => "LastMotherRevivedTime",
            "DoesNotAttackNguoiYeu" => "LoverDoesNotAttack", "LastTimePlayerNotAttackNguoiYeu" => "LastLoverAttackBlockPlayerTime", "TimeNotAttackNguoiYeu" => "LoverAttackBlockDuration", "TemporaryEnemiesNguoiYeu" => "LoverTemporaryEnemies", "JustRevivedNguoiYeu" => "LoverJustRevived", "LastTimeRevivedNguoiYeu" => "LastLoverRevivedTime",
            "DoesNotAttackConone" => "ChildOneDoesNotAttack", "LastTimePlayerNotAttackConone" => "LastChildOneAttackBlockPlayerTime", "TimeNotAttackConone" => "ChildOneAttackBlockDuration", "TemporaryEnemiesConone" => "ChildOneTemporaryEnemies", "JustRevivedConone" => "ChildOneJustRevived", "LastTimeRevivedConone" => "LastChildOneRevivedTime",
            "DoesNotAttackContwo" => "ChildTwoDoesNotAttack", "LastTimePlayerNotAttackContwo" => "LastChildTwoAttackBlockPlayerTime", "TimeNotAttackContwo" => "ChildTwoAttackBlockDuration", "TemporaryEnemiesContwo" => "ChildTwoTemporaryEnemies", "JustRevivedContwo" => "ChildTwoJustRevived", "LastTimeRevivedContwo" => "LastChildTwoRevivedTime",
            "DoesNotAttackConthree" => "ChildThreeDoesNotAttack", "LastTimePlayerNotAttackConthree" => "LastChildThreeAttackBlockPlayerTime", "TimeNotAttackConthree" => "ChildThreeAttackBlockDuration", "TemporaryEnemiesConthree" => "ChildThreeTemporaryEnemies", "JustRevivedConthree" => "ChildThreeJustRevived", "LastTimeRevivedConthree" => "LastChildThreeRevivedTime",
            "THE_TUAN" => "WeeklyCard", "LASTTIME_THE_TUAN" => "LastWeeklyCardTime",
            "THE_THANG" => "MonthlyCard", "LASTTIME_THE_THANG" => "LastMonthlyCardTime",
            "THE_NAM" => "YearlyCard", "LASTTIME_THE_NAM" => "LastYearlyCardTime",
            "THE_CHI_TON" => "SupremeCard", "LASTTIME_THE_CHI_TON" => "LastSupremeCardTime",
            "IsUseDanhHieu_ThienTu" => "UsesEmperorTitle", "LastTimeDanhHieu_ThienTu" => "LastEmperorTitleTime",
            "IsUseDanhHieu_2" => "UsesTitle_2", "LastTimeDanhHieu_2" => "LastTitleTime_2",
            "IsUseDanhHieu_3" => "UsesTitle_3", "LastTimeDanhHieu_3" => "LastTitleTime_3",
            "IsUseDanhHieu_4" => "UsesTitle_4", "LastTimeDanhHieu_4" => "LastTitleTime_4",
            "Isbienhinh" => "TransformationState", "Baovetaikhoan" => "IsAccountProtected",
            _ => pascalName,
        };
    }

    // Khóa corpus giữ nguyên tên Java; chỉ ánh xạ khi đặt vào các property SetClothes đã đổi tên.
    private static string CSharpSetClothesPropertyName(string javaName) => javaName switch
    {
        "ctHaiTac" => "CostumePirate",
        "ctBunmaTocMau" => "CostumeBulmaColoredHair",
        "ctTiecbaiBien" => "CostumeBeachParty",
        "ctNezuko" => "CostumeNezuko",
        "ctTanjiro" => "CostumeTanjiro",
        "ctInoHashi" => "CostumeInosukeHashibira",
        "ctInosuke" => "CostumeInosuke",
        "ctZenitsu" => "CostumeZenitsu",
        "ctFideDaiCa" => "CostumeFriezaBoss",
        "ctDanEmFide" => "CostumeFriezaMinion",
        _ => javaName,
    };

    internal static void SetProperty(object target, string javaName, JsonElement v)
    {
        SetCSharpProperty(target, CSharpPropertyName(javaName), v);
    }

    private static void SetCSharpProperty(object target, string name, JsonElement v)
    {
        PropertyInfo prop = target.GetType().GetProperty(name) ?? throw new InvalidOperationException($"{target.GetType().Name} has no property {name}");
        Type t = prop.PropertyType;
        object value;
        if (t == typeof(bool))
        {
            value = v.GetBoolean();
        }
        else if (t.IsEnum)
        {
            value = Enum.Parse(t, v.GetString()!);
        }
        else if (t == typeof(string))
        {
            value = v.GetString()!;
        }
        else
        {
            value = Convert.ChangeType(v.GetInt64(), t, System.Globalization.CultureInfo.InvariantCulture);
        }

        prop.SetValue(target, value);
    }

    private static void CopyProperties(object source, object destination)
    {
        foreach (PropertyInfo p in source.GetType().GetProperties())
        {
            if (p.CanRead && p.CanWrite)
            {
                p.SetValue(destination, p.GetValue(source));
            }
        }
    }

    // ---- reads ----

    protected override void CaptureCollaborators(NPoint npoint, NPointFacts facts)
    {
        Player player = npoint.Player;
        Store store = stores[player];
        NPointFacts s = store.Facts;
        CopyProperties(s.ItemTime, facts.ItemTime);
        CopyProperties(s.EffectSkill, facts.EffectSkill);
        CopyProperties(s.EffectSkin, facts.EffectSkin);
        CopyProperties(s.SetClothes, facts.SetClothes);
        CopyProperties(s.Charms, facts.Charms);
        CopyProperties(s.Inventory, facts.Inventory);
        facts.Zone = s.Zone;
        facts.Clan = s.Clan;
        facts.TypeFusion = s.TypeFusion;
        facts.FusionDamage = s.FusionDamage;
        facts.BlackBallTimeOut = s.BlackBallTimeOut;
        facts.Cards = s.Cards;
        facts.BadgeOptions = s.BadgeOptions;
        facts.Intrinsic = s.Intrinsic;
        facts.SkillSelect = s.SkillSelect;
        facts.Player.SatelliteIsIntelligent = s.Player.SatelliteIsIntelligent;
        facts.Player.UseFishSkewer = s.Player.UseFishSkewer;
        facts.Player.HasVipPoint = s.Player.HasVipPoint;
        facts.Player.PetStatus = store.PetStatus;
        facts.Player.PetType = store.PetType;

        NPointServerFacts server = facts.Server;
        server.ReadInt = ReadInt;
        server.RateExpServer = RateExp;
        server.InternationalWomansDay = WomansDay;
        server.WomansDayHp = WdHp;
        server.WomansDayMp = WdMp;
        server.WomansDayDame = WdDame;
        FillAttribute(4, v => server.AttributeHpActive = v, v => server.AttributeHpValue = v);
        FillAttribute(3, v => server.AttributeKiActive = v, v => server.AttributeKiValue = v);
        FillAttribute(5, v => server.AttributeSucDanhActive = v, v => server.AttributeSucDanhValue = v);
        FillAttribute(1, v => server.AttributeTnsmActive = v, v => server.AttributeTnsmValue = v);
        server.WhisPlayerIds = [.. Whis];

        // the pet of this player: the numbers were read from its NPoint by the production host, the rest comes from the pet's scenario
        if (player.Pet is not null && facts.Pet is not null)
        {
            Store petStore = stores[player.Pet];
            facts.Pet.PetType = petStore.PetType;
            List<NPointItem> petBody = petStore.Facts.Inventory.ItemsBody;
            facts.Pet.BodyItemCount = petBody.Count;
            facts.Pet.BodySkin = petBody.Count > 5 ? petBody[5] : null;
        }

        // this player is a pet or a clone: the data of its master
        if (masters.TryGetValue(player, out Player? master))
        {
            Store ms = stores[master];
            NPointMasterFacts mf = new()
            {
                HpMax = master.NPoint.HpMax,
                MpMax = master.NPoint.MpMax,
                Damage = master.NPoint.Damage,
                PetPowerPotentialPercent = master.NPoint.PetPowerPotentialPercent,
                ItemTimeIsEatMeal = ms.Facts.ItemTime.IsEatMeal,
                ItemTimeIsUsePetPowerPotentialCharm = ms.Facts.ItemTime.IsUsePetPowerPotentialCharm,
                ItemTimeIsBoneDragon = ms.Facts.ItemTime.IsBoneDragon,
                WeeklyCard = master.WeeklyCard,
                LastWeeklyCardTime = master.LastWeeklyCardTime,
                MonthlyCard = master.MonthlyCard,
                LastMonthlyCardTime = master.LastMonthlyCardTime,
                YearlyCard = master.YearlyCard,
                LastYearlyCardTime = master.LastYearlyCardTime,
                SupremeCard = master.SupremeCard,
                LastSupremeCardTime = master.LastSupremeCardTime,
            };
            CopyProperties(ms.Facts.Charms, mf.Charms);
            facts.Master = mf;
        }
    }

    private void FillAttribute(int templateId, Action<bool> active, Action<int> value)
    {
        if (Attributes.TryGetValue(templateId, out (int Value, bool Expired) a) && !a.Expired)
        {
            active(true);
            value(a.Value);
        }
    }

    public override void Commit(NPoint npoint, NPointFacts facts)
    {
        NPointFacts s = stores[npoint.Player].Facts;
        s.SetClothes.Worldcup = facts.SetClothes.Worldcup;
        s.Inventory.TrainingArmor = facts.Inventory.TrainingArmor;
    }

    public override NPointAttackFacts CaptureAttack(NPoint npoint)
    {
        Player player = npoint.Player;
        Store store = stores[player];
        NPointFacts s = store.Facts;
        NPointAttackFacts af = new() { ReadInt = ReadInt, IsPet = player.IsPet };
        if (s.SkillSelect is null)
        {
            throw new NullReferenceException("player.playerSkill.skillSelect is null");
        }

        af.SkillSelect = s.SkillSelect;
        af.Intrinsic = s.Intrinsic;
        CopyProperties(s.SetClothes, af.SetClothes);
        CopyProperties(s.EffectSkill, af.EffectSkill);
        CopyProperties(s.EffectSkin, af.EffectSkin);
        if (player.IsPet && masters.TryGetValue(player, out Player? master))
        {
            af.MasterPetCharmTraining = stores[master].Facts.Charms.PetTraining;
        }

        if (s.SkillSelect.TemplateId == Skill.QUA_CAU_KENH_KHI)
        {
            if (s.Zone is null)
            {
                throw new NullReferenceException("player.zone is null");
            }

            int range = NPointSkillMath.GetEnergyBallRange(s.SkillSelect.Point);
            foreach ((int x, int y, long hp, bool die) in store.Mobs)
            {
                if (!die && Util.GetDistance(store.LocX, store.LocY, x, y) <= range)
                {
                    af.AroundMobHp += hp;
                }
            }

            foreach ((long id, int x, int y, long hp, bool _) in store.Humans)
            {
                if (hp > 0 && player.Id != id && Util.GetDistance(store.LocX, store.LocY, x, y) <= range)
                {
                    af.AroundPlayerHp += hp;
                }
            }
        }

        return af;
    }

    public override void CommitAttack(NPoint npoint, NPointAttackFacts facts)
    {
        NPointEffectSkinFacts es = stores[npoint.Player].Facts.EffectSkin;
        es.IsBlastDamageMultiplierReady = facts.EffectSkin.IsBlastDamageMultiplierReady;
        es.IsBlastDamageMultiplierApplied = facts.EffectSkin.IsBlastDamageMultiplierApplied;
        es.LastTimeBlastDamageMultiplier = facts.EffectSkin.LastTimeBlastDamageMultiplier;
    }

    public override NPointUpdateFacts CaptureUpdate(NPoint npoint)
    {
        Store store = stores[npoint.Player];
        NPointSkillFacts? skill = store.Facts.SkillSelect;
        return new NPointUpdateFacts
        {
            HasEffectSkill = true,
            IsCharging = store.IsCharging,
            CountCharging = store.CountCharging,
            IsHaveEffectSkill = store.HaveEffectSkill,
            HasSkillSelect = skill is not null,
            SkillSelectPoint = skill?.Point ?? 0,
            ReadInt = ReadInt,
        };
    }

    public override PowerLimitInfo? GetPowerLimit(int limitPower) => Limits.TryGetValue(limitPower, out PowerLimitInfo? l) ? l : null;

    // ---- writes (the text is what the Java test doubles logged) ----

    public override void SetDie(Player player) => Log.Add("setDie");

    public override void RestoreHpMp(Player player, long hp, long mp) => Log.Add($"hoiPhuc({hp},{mp})");

    public override void SendCurrentStamina(Player player) => Log.Add("sendCurrentStamina");

    public override void Chat(Player player, string text) => Log.Add($"chat({text})");

    public override void StopCharge(Player player)
    {
        Log.Add("stopCharge");
        Store store = stores[player];
        store.IsCharging = false;
        store.CountCharging = 0;
    }

    public override int IncrementCountCharging(Player player) => ++stores[player].CountCharging;

    public override void ClearCharging(Player player)
    {
        Store store = stores[player];
        store.IsCharging = false;
        store.CountCharging = 0;
    }

    public override void SendNotification(Player player, string text) => Log.Add($"sendNotification({text})");

    public override void SendNotificationOk(Player player, string text) => Log.Add($"sendNotificationOk({text})");

    public override void SendMoney(Player player) => Log.Add("sendMoney");

    public override void SendPoint(Player player) => Log.Add("point");

    public override void SendBasePetInfo(Player player) => Log.Add("InfoPetGoc");

    public override void ShowInfoPet(Player player) => Log.Add("showInfoPet");

    public override void CheckDoneTaskPower(Player player, long power) => Log.Add($"checkDoneTaskPower({power})");

    public override void CheckPotentialUseTaskCompletion(Player player) => Log.Add("checkDoneTaskUseTiemNang");
}
