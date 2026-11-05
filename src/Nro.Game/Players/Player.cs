
namespace Nro.Game.Players;

// Lưu vỏ dữ liệu của người chơi; logic update, dispose, bị thương và tấn công còn cho H-01. Danh sách không khóa vì chỉ tick zone chạm vào theo D1.
// Nguồn Java: nro/player/Player.java (tên property viết hoa chữ đầu, byte Java là sbyte, Timestamp/Date là DateTime? UTC; component luôn khác null; Runnable/run/start, timer/task và lichSuGiaoDichDangXem được bỏ có chủ đích).


public partial class Player
{
    internal const long TIME_TRUNG_THU = 120000;

    internal static long LAST_TIME_UPDATE_LOGIN { get; set; }

    internal static readonly short[][] idOutfitFusion =
    [
        [380, 381, 382], [383, 384, 385], [391, 392, 393],
        [870, 871, 872], [873, 874, 875], [867, 868, 869],
        [1939, 1942, 1943], [1831, 1834, 1835], [1826, 1829, 1830],
        [1765, 1766, 1767], [1771, 1772, 1773], [1768, 1769, 1770],
        [1774, 1775, 1776], [1780, 1781, 1782], [1777, 1778, 1779],
    ];

    internal static readonly short[][] idOutfitCaiTrangFusion =
    [
        [566, 567, 568], [570, 571, 572], [563, 564, 565],
        [630, 631, 632], [633, 634, 635], [627, 628, 629],
    ];

    // Lưu trạng thái idOutfitHalloween (Java: idOutfitHalloween).

    public static readonly short[][][] idOutfitHalloween =
    [
        [[654, 655, 656], [654, 655, 656], [654, 655, 656]],
        [[651, 652, 653], [651, 652, 653], [651, 652, 653]],
        [[545, 548, 549], [547, 548, 549], [546, 548, 549]],
        [[760, 761, 762], [760, 761, 762], [760, 761, 762]],
    ];

    // Lưu trạng thái idOutfitMafuba (Java: idOutfitMafuba).

    public static readonly short[][] idOutfitMafuba = [[1686, 1687, 1688], [-1, -1, -1], [1218, 1219, 1220]];

    // Lưu trạng thái idOutfitGod (Java: idOutfitGod).

    public static readonly short[][] idOutfitGod = [[-1, 472, 473], [-1, 476, 477], [-1, 474, 475]];

    internal static readonly short[][] idOutFitSuperEarth =
    [
        [1436, 1437, 1438],
        [1436, 1437, 1438],
        [1442, 1437, 1438],
        [1440, 1437, 1438],
        [1439, 1437, 1438],
        [1441, 1437, 1438],
    ];

    internal static readonly short[][] idOutFitSuperNamec =
    [
        [1430, 1431, 1432],
        [1443, 1431, 1432],
        [1444, 1431, 1432],
        [1445, 1431, 1432],
        [1446, 1431, 1432],
        [1447, 1431, 1432],
    ];

    internal static readonly short[][] idOutFitSuperSaiyan =
    [
        [1433, 1434, 1435],
        [1433, 1434, 1435],
        [1448, 1434, 1435],
        [1449, 1434, 1435],
        [1450, 1434, 1435],
        [1451, 1434, 1435],
    ];

    internal static readonly sbyte[][] idAuraSuper =
    [
        [20, 21, 22, 23, 24, 25],
        [26, 27, 28, 29, 30, 31],
        [32, 33, 34, 35, 36, 37],
    ];

    // Lưu trạng thái FusionDisplayState (Java: hienThiHopThe).

    public int FusionDisplayState { get; set; } = 1;

    // Lưu trạng thái LastTimeEatPea (Java: lastTimeEatPea).

    public long LastTimeEatPea { get; set; }

    // Lưu trạng thái Session (Java: session).

    public MySession? Session { get; set; }

    // Lưu trạng thái Saving (Java: saving).

    public bool Saving { get; set; }

    // Lưu trạng thái Id (Java: id).

    public long Id { get; set; }

    // Lưu trạng thái Name (Java: name).

    public string Name { get; set; } = "";

    // Lưu trạng thái Gender (Java: gender).

    public sbyte Gender { get; set; }

    // Lưu trạng thái IsNewMember (Java: isNewMember).

    public bool IsNewMember { get; set; }

    // Lưu trạng thái AccountCreatedDays (Java: accountCreatedDays).

    public long AccountCreatedDays { get; set; }

    // Lưu trạng thái Head (Java: head).

    public short Head { get; set; }

    // Lưu trạng thái DeltaTime (Java: deltaTime).

    public int DeltaTime { get; set; }

    // Lưu trạng thái TypePk (Java: typePk).

    public sbyte TypePk { get; set; }

    // Lưu trạng thái CFlag (Java: cFlag).

    public sbyte CFlag { get; set; }

    // Lưu trạng thái HaveTennisSpaceShip (Java: haveTennisSpaceShip).

    public bool HaveTennisSpaceShip { get; set; }
    private long ResetDateOut { get; set; }

    // Lưu trạng thái ResetDamage (Java: resetdame).

    public bool ResetDamage { get; set; }

    // Lưu trạng thái LastDamageTime (Java: lastTimeDame).

    public long LastDamageTime { get; set; }

    // Lưu trạng thái TotalDamage (Java: dametong).

    public long TotalDamage { get; set; }

    // Lưu trạng thái LastPunchingMachineDamageTime (Java: lastTimeDameMayDam).

    public long LastPunchingMachineDamageTime { get; set; }

    // Lưu trạng thái PunchingMachineTotalDamage (Java: totalDameMayDam).

    public long PunchingMachineTotalDamage { get; set; }

    // Lưu trạng thái Lastimelogin (Java: lastimelogin).

    public DateTime? Lastimelogin { get; set; }

    // Lưu trạng thái FirstTimeLogin (Java: firstTimeLogin).

    public DateTime? FirstTimeLogin { get; set; }

    // Lưu trạng thái LastTimeLoginGame (Java: lastTimeLoginGame).

    public DateTime? LastTimeLoginGame { get; set; }

    // Lưu trạng thái LastTimeAttack (Java: lastTimeAttack).

    public long LastTimeAttack { get; set; }

    // Lưu trạng thái DamageBonusPercent (Java: tlDameAdd).

    public int DamageBonusPercent { get; set; }

    // Lưu trạng thái ClanDamageBonusPercent (Java: tlDameClanAdd).

    public int ClanDamageBonusPercent { get; set; }

    // Lưu trạng thái ClanHpBonusPercent (Java: tlHpClanAdd).

    public int ClanHpBonusPercent { get; set; }

    // Lưu trạng thái ClanMpBonusPercent (Java: tlMpClanAdd).

    public int ClanMpBonusPercent { get; set; }

    // Lưu trạng thái CanGeFirstTimeLogin (Java: canGeFirstTimeLogin).

    public bool CanGeFirstTimeLogin { get; set; }

    // Lưu trạng thái TempItemIndex (Java: tempItemIndex).

    public int TempItemIndex { get; set; } = -1;

    // Lưu trạng thái ItemThrow_Drop (Java: itemThrow_Drop).

    public Item? ItemThrow_Drop { get; set; }

    // Lưu trạng thái Item_ChangePet (Java: item_ChangePet).

    public Item? Item_ChangePet { get; set; }

    // Lưu trạng thái TransactionHistoryStatus (Java: trangThaiLichSuGd).

    public int TransactionHistoryStatus { get; set; }

    // Lưu trạng thái EarthlingPunchingMachineRank (Java: topMayDamTraiDat).

    public long EarthlingPunchingMachineRank { get; set; }

    // Lưu trạng thái NamekianPunchingMachineRank (Java: topMayDamNamec).

    public long NamekianPunchingMachineRank { get; set; }

    // Lưu trạng thái SaiyanPunchingMachineRank (Java: topMayDamXayda).

    public long SaiyanPunchingMachineRank { get; set; }

    // Lưu trạng thái NumUseSkill (Java: numUseSkill).

    public int NumUseSkill { get; set; }

    // Lưu trạng thái LastTimeHoiskill (Java: lastTimeHoiskill).

    public long LastTimeHoiskill { get; set; }

    // Lưu trạng thái ClanCheckInCount (Java: diemDanhBangHoi).

    public int ClanCheckInCount { get; set; }

    // Lưu trạng thái GiftGemVoucherCount (Java: veTangNgoc_SoLuongNgoc).

    public int GiftGemVoucherCount { get; set; }

    // Lưu trạng thái GemRecipient (Java: player_NhanNgoc).

    public Player? GemRecipient { get; set; }

    // Lưu trạng thái GiftRubyVoucherCount (Java: veTangHongNgoc_SoLuongHongNgoc).

    public int GiftRubyVoucherCount { get; set; }

    // Lưu trạng thái RubyRecipient (Java: player_NhanHongNgoc).

    public Player? RubyRecipient { get; set; }

    // Lưu trạng thái BetAmount (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: cuoc).

    public int BetAmount { get; set; }

    // Lưu trạng thái RubyWin (Java: rubyWin).

    public int RubyWin { get; set; }

    // Lưu trạng thái NameClan (Java: nameClan).

    public string? NameClan { get; set; }

    // Lưu trạng thái LastTimeDropTail (Java: lastTimeDropTail).

    public long LastTimeDropTail { get; set; }

    // Lưu trạng thái LastTimeChangeBadges (Java: lastTimeChangeBadges).

    public long LastTimeChangeBadges { get; set; }

    // Lưu trạng thái MapTransitionTime (Java: mapTransitionTime).

    public long MapTransitionTime { get; set; }
    private int WrongPasswordAttempts { get; set; }

    // Lưu trạng thái RestartCakeCooking (Java: restartNauBanh). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public sbyte RestartCakeCooking { get; set; }

    // Lưu trạng thái IsEventGasActive (Java: khisukien). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public bool IsEventGasActive { get; set; }

    // Lưu trạng thái LastGasCallTime (Java: lastTimeCallKhi). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public long LastGasCallTime { get; set; }

    // Lưu trạng thái LastEventGasTime (Java: lasttimekhisukien). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public long LastEventGasTime { get; set; }
    private long LastTimeBom { get; set; }

    // Lưu trạng thái TitlePart (Java: partDanhHieu).

    public int TitlePart { get; set; }

    // Lưu trạng thái Titleitem (Java: titleitem).

    public bool Titleitem { get; set; }

    // Lưu trạng thái PointRank (Java: pointRank).

    public sbyte PointRank { get; set; }

    // Lưu trạng thái TypeTabPet (Java: typeTabPet).

    public sbyte TypeTabPet { get; set; }

    // Lưu trạng thái TypeBox (Java: typeBox).

    public sbyte TypeBox { get; set; }

    // Lưu trạng thái TypeVip (Java: typeVip).

    public int TypeVip { get; set; } = -1;

    // Lưu trạng thái IsPlayer (Java: isPlayer).

    public bool IsPlayer { get; set; }

    // Lưu trạng thái IsPet (Java: isDeTu).

    public bool IsPet { get; set; }

    // Lưu trạng thái IsFather (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: isBo).

    public bool IsFather { get; set; }

    // Lưu trạng thái IsMother (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: isMe).

    public bool IsMother { get; set; }

    // Lưu trạng thái IsPetFollow (Java: isPetFollow).

    public bool IsPetFollow { get; set; }

    // Lưu trạng thái IsTangMonk (Java: isDuongTang).

    public bool IsTangMonk { get; set; }

    // Lưu trạng thái IsBoss (Java: isBoss).

    public bool IsBoss { get; set; }

    // Lưu trạng thái IsLover (Java: isNguoiYeu).

    public bool IsLover { get; set; }

    // Lưu trạng thái IsChildOne (Java: isConOne).

    public bool IsChildOne { get; set; }

    // Lưu trạng thái IsChildTwo (Java: isConTwo).

    public bool IsChildTwo { get; set; }

    // Lưu trạng thái IsChildThree (Java: isConThree).

    public bool IsChildThree { get; set; }

    // Lưu trạng thái IsBodyClone (Java: isPhanThan).

    public bool IsBodyClone { get; set; }

    // Lưu trạng thái DragonBallInsertCount (Java: nhapNgocRongTimes).

    public int DragonBallInsertCount { get; set; } = 1;

    // Lưu trạng thái HalloweenNpcCandyOutcome_1 (Java: nhanKeoHayBiGheoNpc_1). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_1 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_2 (Java: nhanKeoHayBiGheoNpc_2). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_2 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_3 (Java: nhanKeoHayBiGheoNpc_3). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_3 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_4 (Java: nhanKeoHayBiGheoNpc_4). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_4 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_5 (Java: nhanKeoHayBiGheoNpc_5). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_5 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_6 (Java: nhanKeoHayBiGheoNpc_6). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_6 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_7 (Java: nhanKeoHayBiGheoNpc_7). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_7 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_8 (Java: nhanKeoHayBiGheoNpc_8). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_8 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_9 (Java: nhanKeoHayBiGheoNpc_9). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_9 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_10 (Java: nhanKeoHayBiGheoNpc_10). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_10 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_11 (Java: nhanKeoHayBiGheoNpc_11). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_11 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_12 (Java: nhanKeoHayBiGheoNpc_12). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_12 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_13 (Java: nhanKeoHayBiGheoNpc_13). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_13 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_14 (Java: nhanKeoHayBiGheoNpc_14). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_14 { get; set; }

    // Lưu trạng thái HalloweenNpcCandyOutcome_15 (Java: nhanKeoHayBiGheoNpc_15). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int HalloweenNpcCandyOutcome_15 { get; set; }

    // Lưu trạng thái DailyCheckTimestamp (Java: checkTrongNgay). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public long DailyCheckTimestamp { get; set; }

    // Lưu trạng thái GiveLuckyMoneyRankingScore (Java: duaTopTangLiXi).

    public int GiveLuckyMoneyRankingScore { get; set; }

    // Lưu trạng thái OpenLuckyMoneyRankingScore (Java: duaTopMoLiXi).

    public int OpenLuckyMoneyRankingScore { get; set; }

    // Lưu trạng thái LaunchFireworksRankingScore (Java: duaTopBanPhaoHoa).

    public int LaunchFireworksRankingScore { get; set; }

    // Lưu trạng thái LaunchVipFireworksRankingScore (Java: duaTopBanPhaoHoaVIP).

    public int LaunchVipFireworksRankingScore { get; set; }

    // Lưu trạng thái DecorateChristmasTreeRankingScore (Java: duaTopTrangTriCayNoel).

    public int DecorateChristmasTreeRankingScore { get; set; }

    // Lưu trạng thái CraftSnowmanRankingScore (Java: duaTopCheTaoNguoiTuyet).

    public int CraftSnowmanRankingScore { get; set; }

    // Lưu trạng thái CraftFrozenSnowmanRankingScore (Java: duaTopCheTaoNguoiTuyetBangGia).

    public int CraftFrozenSnowmanRankingScore { get; set; }

    // Lưu trạng thái BurnMatchesRankingScore (Java: duaTopDotDiem).

    public int BurnMatchesRankingScore { get; set; }

    // Lưu trạng thái LaunchVuLanFirecrackersRankingScore (Java: duaTopPhaoHoaVuLan).

    public int LaunchVuLanFirecrackersRankingScore { get; set; }

    // Lưu trạng thái FlowerLanternRankingScore (Java: duaTopHoaDang).

    public int FlowerLanternRankingScore { get; set; }

    // Lưu trạng thái WishLanternRankingScore (Java: duaTopHoaDangCoLoiChuc).

    public int WishLanternRankingScore { get; set; }

    // Lưu trạng thái OpenGhostGiftBoxRankingScore (Java: duaTopMoHopMaQuy).

    public int OpenGhostGiftBoxRankingScore { get; set; }

    // Lưu trạng thái HalloweenCardRankingScore (Java: duaTopThiepHalloween).

    public int HalloweenCardRankingScore { get; set; }

    // Lưu trạng thái OpenWomensDayCardRankingScore (Java: duaTopMoThiep83).

    public int OpenWomensDayCardRankingScore { get; set; }

    // Lưu trạng thái GiveRosesRankingScore (Java: duaTopTangBongHoaHong).

    public int GiveRosesRankingScore { get; set; }

    // Lưu trạng thái MakeMoonCakeRankingScore (Java: duaTopLamBanhTrungThu).

    public int MakeMoonCakeRankingScore { get; set; }

    // Lưu trạng thái OpenSpecialMoonCakeRankingScore (Java: duaTopMoHopTrungThuDacBiet).

    public int OpenSpecialMoonCakeRankingScore { get; set; }

    // Lưu trạng thái FreeWishCount (Java: uocMienPhi).

    public int FreeWishCount { get; set; }

    // Lưu trạng thái ReceivedHungKingFreeGift (Java: nhanQuaHungVuongFree).

    public int ReceivedHungKingFreeGift { get; set; }

    // Lưu trạng thái LastHungKingCakeCookingTime (Java: lastTimeNauBanhHungVuong).

    public long LastHungKingCakeCookingTime { get; set; }

    // Lưu trạng thái CookingCakeType (Java: typeBanhDangNau).

    public int CookingCakeType { get; set; }

    // Lưu trạng thái OpenHungKingGiftBoxRankingScore (Java: duaTopMoHopQuaGioTo).

    public int OpenHungKingGiftBoxRankingScore { get; set; }

    // Lưu trạng thái OfferHungKingCakesRankingScore (Java: duaTopDangBanhHungVuong).

    public int OfferHungKingCakesRankingScore { get; set; }

    // Lưu trạng thái OpenGoldenDragonEggRankingScore (Java: duaTopMoTrungRongVang).

    public int OpenGoldenDragonEggRankingScore { get; set; }

    // Lưu trạng thái ExchangeWatermelonRankingScore (Java: duaTopDoiDuaHau).

    public int ExchangeWatermelonRankingScore { get; set; }

    // Lưu trạng thái OpenBlackFridayBoxRankingScore (Java: duaTopMoHopBlackFriday). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int OpenBlackFridayBoxRankingScore { get; set; }

    // Lưu trạng thái BlackFridayShoppingRankingScore (Java: duaTopMuaSamBlackFriday). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int BlackFridayShoppingRankingScore { get; set; }

    // Lưu trạng thái OpenWomensDayGiftBoxRankingScore (Java: duaTopMoHop20_10). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int OpenWomensDayGiftBoxRankingScore { get; set; }

    // Lưu trạng thái OpenGreetingCardRankingScore (Java: duaTopMoThiepChuc). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int OpenGreetingCardRankingScore { get; set; }

    // Lưu trạng thái GiveGreetingCardRankingScore (Java: duaTopTangThiepChuc). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int GiveGreetingCardRankingScore { get; set; }

    // Lưu trạng thái IsEscortingTangMonk (Java: hoTongDuongTang).

    public bool IsEscortingTangMonk { get; set; }

    // Lưu trạng thái TangMonkEscortMapId (Java: mapHoTong).

    public int TangMonkEscortMapId { get; set; }

    // Lưu trạng thái LastTangMonkTime (Java: lastTimeDuongTang).

    public long LastTangMonkTime { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_1 (Java: nhanLiXiForNPC_1).

    public int LuckyMoneyReceivedFromNpc_1 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_2 (Java: nhanLiXiForNPC_2).

    public int LuckyMoneyReceivedFromNpc_2 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_3 (Java: nhanLiXiForNPC_3).

    public int LuckyMoneyReceivedFromNpc_3 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_4 (Java: nhanLiXiForNPC_4).

    public int LuckyMoneyReceivedFromNpc_4 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_5 (Java: nhanLiXiForNPC_5).

    public int LuckyMoneyReceivedFromNpc_5 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_6 (Java: nhanLiXiForNPC_6).

    public int LuckyMoneyReceivedFromNpc_6 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_7 (Java: nhanLiXiForNPC_7).

    public int LuckyMoneyReceivedFromNpc_7 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_8 (Java: nhanLiXiForNPC_8).

    public int LuckyMoneyReceivedFromNpc_8 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_9 (Java: nhanLiXiForNPC_9).

    public int LuckyMoneyReceivedFromNpc_9 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_10 (Java: nhanLiXiForNPC_10).

    public int LuckyMoneyReceivedFromNpc_10 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_11 (Java: nhanLiXiForNPC_11).

    public int LuckyMoneyReceivedFromNpc_11 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_12 (Java: nhanLiXiForNPC_12).

    public int LuckyMoneyReceivedFromNpc_12 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_13 (Java: nhanLiXiForNPC_13).

    public int LuckyMoneyReceivedFromNpc_13 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_14 (Java: nhanLiXiForNPC_14).

    public int LuckyMoneyReceivedFromNpc_14 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_15 (Java: nhanLiXiForNPC_15).

    public int LuckyMoneyReceivedFromNpc_15 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_16 (Java: nhanLiXiForNPC_16).

    public int LuckyMoneyReceivedFromNpc_16 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_17 (Java: nhanLiXiForNPC_17).

    public int LuckyMoneyReceivedFromNpc_17 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_18 (Java: nhanLiXiForNPC_18).

    public int LuckyMoneyReceivedFromNpc_18 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_19 (Java: nhanLiXiForNPC_19).

    public int LuckyMoneyReceivedFromNpc_19 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_20 (Java: nhanLiXiForNPC_20).

    public int LuckyMoneyReceivedFromNpc_20 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_21 (Java: nhanLiXiForNPC_21).

    public int LuckyMoneyReceivedFromNpc_21 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_22 (Java: nhanLiXiForNPC_22).

    public int LuckyMoneyReceivedFromNpc_22 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_23 (Java: nhanLiXiForNPC_23).

    public int LuckyMoneyReceivedFromNpc_23 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_24 (Java: nhanLiXiForNPC_24).

    public int LuckyMoneyReceivedFromNpc_24 { get; set; }

    // Lưu trạng thái LuckyMoneyReceivedFromNpc_25 (Java: nhanLiXiForNPC_25).

    public int LuckyMoneyReceivedFromNpc_25 { get; set; }

    // Lưu trạng thái IsHoldingContestedNamekBall (Java: isHoldNamecBallTranhDoat).

    public bool IsHoldingContestedNamekBall { get; set; }

    // Lưu trạng thái ContestedNamekBallTemplateId (Java: tempIdNamecBallHoldTranhDoat).

    public int ContestedNamekBallTemplateId { get; set; } = -1;

    // Lưu trạng thái LastTimePickItem (Java: lastTimePickItem).

    public long LastTimePickItem { get; set; }

    // Lưu trạng thái LastTimeUpdateBallWar (Java: lastTimeUpdateBallWar).

    public long LastTimeUpdateBallWar { get; set; }

    // Lưu trạng thái TotalDamageTaken (Java: totalDamageTaken).

    public long TotalDamageTaken { get; set; }

    // Lưu trạng thái ShowMapChangeNotification (Java: thongBaoChangeMap).

    public bool ShowMapChangeNotification { get; set; }

    // Lưu trạng thái MapChangeNotificationText (Java: textThongBaoChangeMap).

    public string? MapChangeNotificationText { get; set; }

    // Lưu trạng thái ShowLossNotification (Java: thongBaoThua).

    public bool ShowLossNotification { get; set; }

    // Lưu trạng thái LossNotificationText (Java: textThongBaoThua).

    public string? LossNotificationText { get; set; }

    // Lưu trạng thái LevelWoodChest (Java: levelWoodChest).

    public int LevelWoodChest { get; set; }

    // Lưu trạng thái GoldChallenge (Java: goldChallenge).

    public long GoldChallenge { get; set; }

    // Lưu trạng thái RubyChallenge (Java: rubyChallenge).

    public long RubyChallenge { get; set; }

    // Lưu trạng thái LastTimeRewardWoodChest (Java: lastTimeRewardWoodChest).

    public long LastTimeRewardWoodChest { get; set; }

    // Lưu trạng thái ItemsWoodChest (Java: itemsWoodChest).

    public List<Item> ItemsWoodChest { get; set; } = [];

    // Lưu trạng thái IndexWoodChest (Java: indexWoodChest).

    public int IndexWoodChest { get; set; }

    // Lưu trạng thái LastTimePKDHVT23 (Java: lastTimePKDHVT23).

    public long LastTimePKDHVT23 { get; set; }

    // Lưu trạng thái LostByDeath (Java: lostByDeath).

    public bool LostByDeath { get; set; }

    // Lưu trạng thái IsPKDHVT (Java: isPKDHVT).

    public bool IsPKDHVT { get; set; }

    // Lưu trạng thái LastTimePKVoDaiSinhTu (Java: lastTimePKVoDaiSinhTu).

    public long LastTimePKVoDaiSinhTu { get; set; }

    // Lưu trạng thái HaveRewardVDST (Java: haveRewardVDST).

    public bool HaveRewardVDST { get; set; }

    // Lưu trạng thái DeathmatchGoldTime (Java: thoiVangVoDaiSinhTu). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int DeathmatchGoldTime { get; set; }

    // Lưu trạng thái TimePKVDST (Java: timePKVDST).

    public long TimePKVDST { get; set; }

    // Lưu trạng thái JackfruitVote (Java: binhChonHatMit). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int JackfruitVote { get; set; }

    // Lưu trạng thái PlayerVote (Java: binhChonPlayer). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public int PlayerVote { get; set; }

    // Lưu trạng thái VotingZone (Java: zoneBinhChon). (nghĩa chưa chắc, cần chủ dự án duyệt).

    public Zone? VotingZone { get; set; }

    // Lưu trạng thái Mbv (Java: mbv).

    public int Mbv { get; set; }

    // Lưu trạng thái IsAccountProtected (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: baovetaikhoan).

    public bool IsAccountProtected { get; set; }

    // Lưu trạng thái AccountProtectionTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: mbvtime).

    public long AccountProtectionTime { get; set; }

    // Lưu trạng thái Captcha (Java: captcha).

    public string Captcha { get; set; } = "";

    // Lưu trạng thái TimeGohome (Java: timeGohome).

    public int TimeGohome { get; set; }

    // Lưu trạng thái LastUpdateGohomeTime (Java: lastUpdateGohomeTime).

    public long LastUpdateGohomeTime { get; set; }

    // Lưu trạng thái GoHome (Java: goHome).

    public bool GoHome { get; set; }

    // Lưu trạng thái MaBu2H (Java: maBu2H).

    public MajinBuu14H? MaBu2H { get; set; }

    // Lưu trạng thái IsMabuHold (Java: isMabuHold).

    public bool IsMabuHold { get; set; }

    // Lưu trạng thái MaBuHold (Java: maBuHold).

    public MaBuHold? MaBuHold { get; set; }

    // Lưu trạng thái PrecentMabuHold (Java: precentMabuHold).

    public int PrecentMabuHold { get; set; }

    // Lưu trạng thái IsMabuMapProtected (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: isPhuHoMapMabu).

    public bool IsMabuMapProtected { get; set; }

    // Lưu trạng thái IsCopy (Java: isCopy).

    public bool IsCopy { get; set; }

    // Lưu trạng thái LastPkCommesonTime (Java: lastPkCommesonTime).

    public long LastPkCommesonTime { get; set; }

    // Lưu trạng thái SheepSlaughterAttemptsToday (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: timesPerDayCuuSat).

    public int SheepSlaughterAttemptsToday { get; set; }

    // Lưu trạng thái LastSheepSlaughterTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimeCuuSat).

    public long LastSheepSlaughterTime { get; set; }

    // Lưu trạng thái ReceivedVipGoldBoost (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: nhanVangNangVIP).

    public bool ReceivedVipGoldBoost { get; set; }

    // Lưu trạng thái ReceivedVipPetUpgrade (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: nhanDeTuNangVIP).

    public bool ReceivedVipPetUpgrade { get; set; }

    // Lưu trạng thái ReceivedVipActivationSet (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: nhanSKHVIP).

    public bool ReceivedVipActivationSet { get; set; }

    // Lưu trạng thái CanReward_TuanLoc (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: canReward_TuanLoc).

    public bool CanReward_TuanLoc { get; set; }

    // Lưu trạng thái UseVipMap_TuanLoc (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: changeMapVIP_TuanLoc).

    public bool UseVipMap_TuanLoc { get; set; }

    // Lưu trạng thái HasReward_TuanLoc (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: haveReward_TuanLoc).

    public bool HasReward_TuanLoc { get; set; }

    // Lưu trạng thái CanReceiveEventReward (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: canReward).

    public bool CanReceiveEventReward { get; set; }

    // Lưu trạng thái UseVipEventMap (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: changeMapVIP).

    public bool UseVipEventMap { get; set; }

    // Lưu trạng thái HasEventReward (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: haveReward).

    public bool HasEventReward { get; set; }

    // Lưu trạng thái CanReceiveEventReward_MeoDen (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: canReward_MeoDen).

    public bool CanReceiveEventReward_MeoDen { get; set; }

    // Lưu trạng thái UseVipEventMap_MeoDen (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: changeMapVIP_MeoDen).

    public bool UseVipEventMap_MeoDen { get; set; }

    // Lưu trạng thái HasEventReward_MeoDen (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: haveReward_MeoDen).

    public bool HasEventReward_MeoDen { get; set; }

    // Lưu trạng thái CanReceiveEventReward_PiLong (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: canReward_PiLong).

    public bool CanReceiveEventReward_PiLong { get; set; }

    // Lưu trạng thái UseVipEventMap_PiLong (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: changeMapVIP_PiLong).

    public bool UseVipEventMap_PiLong { get; set; }

    // Lưu trạng thái HasEventReward_PiLong (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: haveReward_PiLong).

    public bool HasEventReward_PiLong { get; set; }

    // Lưu trạng thái CanReceiveEventReward_PeNa (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: canReward_PeNa).

    public bool CanReceiveEventReward_PeNa { get; set; }

    // Lưu trạng thái UseVipEventMap_PeNa (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: changeMapVIP_PeNa).

    public bool UseVipEventMap_PeNa { get; set; }

    // Lưu trạng thái HasEventReward_PeNa (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: haveReward_PeNa).

    public bool HasEventReward_PeNa { get; set; }

    // Lưu trạng thái LastTimeJoinDT (Java: lastTimeJoinDT).

    public long LastTimeJoinDT { get; set; }

    // Lưu trạng thái JoinCDRD (Java: joinCDRD).

    public bool JoinCDRD { get; set; }

    // Lưu trạng thái LastSnakeWayJoinTime (Java: lastTimeJoinCDRD).

    public long LastSnakeWayJoinTime { get; set; }

    // Lưu trạng thái HasTalkedToKami (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: talkToThuongDe).

    public bool HasTalkedToKami { get; set; }

    // Lưu trạng thái HasTalkedToKarin (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: talkToThanMeo).

    public bool HasTalkedToKarin { get; set; }

    // Lưu trạng thái TimeChangeMap144 (Java: timeChangeMap144).

    public long TimeChangeMap144 { get; set; }

    // Lưu trạng thái SnakeWayCompletedLevel (Java: levelCDRDDone).

    public int SnakeWayCompletedLevel { get; set; }

    // Lưu trạng thái SnakeWayCompletionTime (Java: timeCDRDDone).

    public long SnakeWayCompletionTime { get; set; }

    // Lưu trạng thái LastSnakeWayRankingUpdate (Java: lastTimeUpdateTopCDRD).

    public long LastSnakeWayRankingUpdate { get; set; }

    // Lưu trạng thái DestronGasCompletedLevel (Java: levelKhiGasDone).

    public int DestronGasCompletedLevel { get; set; }

    // Lưu trạng thái DestronGasCompletionTime (Java: timeKhiGasDone).

    public long DestronGasCompletionTime { get; set; }

    // Lưu trạng thái LastDestronGasRankingUpdate (Java: lastTimeUpdateTopKhiGas).

    public long LastDestronGasRankingUpdate { get; set; }

    // Lưu trạng thái TreasureUnderSeaAttemptsToday (Java: timesPerDayBDKB).

    public int TreasureUnderSeaAttemptsToday { get; set; }

    // Lưu trạng thái LastTimeJoinBDKB (Java: lastTimeJoinBDKB).

    public long LastTimeJoinBDKB { get; set; }

    // Lưu trạng thái TreasureUnderSeaAttemptCount (Java: bdkb_countPerDay).

    public int TreasureUnderSeaAttemptCount { get; set; }

    // Lưu trạng thái LastTreasureUnderSeaJoinTime (Java: bdkb_lastTimeJoin).

    public long LastTreasureUnderSeaJoinTime { get; set; }

    // Lưu trạng thái IsInTreasureUnderSea (Java: bdkb_isJoinBdkb).

    public bool IsInTreasureUnderSea { get; set; }

    // Lưu trạng thái TreasureUnderSeaCompletedLevel (Java: levelBDKBDone).

    public int TreasureUnderSeaCompletedLevel { get; set; }

    // Lưu trạng thái TreasureUnderSeaCompletionTime (Java: timeBDKBDone).

    public long TreasureUnderSeaCompletionTime { get; set; }

    // Lưu trạng thái LastTreasureUnderSeaRankingUpdate (Java: lastTimeUpdateTopBDKB).

    public long LastTreasureUnderSeaRankingUpdate { get; set; }

    // Lưu trạng thái CallBossPocolo (Java: callBossPocolo).

    public bool CallBossPocolo { get; set; }

    // Lưu trạng thái SuperDivineWaterZone (Java: zoneSieuThanhThuy).

    public Zone? SuperDivineWaterZone { get; set; }

    // Lưu trạng thái WonSuperDivineWater (Java: winSTT).

    public bool WonSuperDivineWater { get; set; }

    // Lưu trạng thái LastSuperDivineWaterWinTime (Java: lastTimeWinSTT).

    public long LastSuperDivineWaterWinTime { get; set; }

    // Lưu trạng thái LastSuperDivineWaterUpdate (Java: lastTimeUpdateSTT).

    public long LastSuperDivineWaterUpdate { get; set; }

    // Lưu trạng thái Notify (Java: notify).

    public string? Notify { get; set; }

    // Lưu trạng thái IsOffline (Java: isOffline).

    public bool IsOffline { get; set; }

    // Lưu trạng thái TrainingLevel (Java: levelLuyenTap).

    public int TrainingLevel { get; set; }

    // Lưu trạng thái IsSparring (Java: isThachDau).

    public bool IsSparring { get; set; }

    // Lưu trạng thái TrainingPowerPotential (Java: tnsmLuyenTap).

    public int TrainingPowerPotential { get; set; }

    // Lưu trạng thái IsAutoTrainingRegistered (Java: dangKyTapTuDong).

    public bool IsAutoTrainingRegistered { get; set; }

    // Lưu trạng thái LastTimeOffline (Java: lastTimeOffline).

    public long LastTimeOffline { get; set; }

    // Lưu trạng thái AutoTrainingMapId (Java: mapIdDangTapTuDong).

    public int AutoTrainingMapId { get; set; }

    // Lưu trạng thái LastMapOffline (Java: lastMapOffline).

    public int LastMapOffline { get; set; }

    // Lưu trạng thái LastZoneOffline (Java: lastZoneOffline).

    public int LastZoneOffline { get; set; }

    // Lưu trạng thái LastXOffline (Java: lastXOffline).

    public int LastXOffline { get; set; }

    // Lưu trạng thái AutoTrainingNotification (Java: thongBaoTapTuDong).

    public string? AutoTrainingNotification { get; set; }

    // Lưu trạng thái AutoTrainTeleport (Java: teleTapTuDong).

    public bool AutoTrainTeleport { get; set; }

    // Lưu trạng thái Training (Java: traning).

    public Training Training { get; set; } = null!;

    // Lưu trạng thái DoesNotAttack (Java: doesNotAttack).

    public bool DoesNotAttack { get; set; }

    // Lưu trạng thái LastTimePlayerNotAttack (Java: lastTimePlayerNotAttack).

    public long LastTimePlayerNotAttack { get; set; }

    // Lưu trạng thái TimeNotAttack (Java: timeNotAttack).

    public int TimeNotAttack { get; set; } = 1800000;

    // Lưu trạng thái TemporaryEnemies (Java: temporaryEnemies).

    public List<Player> TemporaryEnemies { get; set; } = [];

    // Lưu trạng thái JustRevived (Java: justRevived).

    public bool JustRevived { get; set; }

    // Lưu trạng thái LastTimeRevived (Java: lastTimeRevived).

    public long LastTimeRevived { get; set; }


    // Lưu trạng thái FatherDoesNotAttack (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: doesNotAttackBo).

    public bool FatherDoesNotAttack { get; set; }

    // Lưu trạng thái LastFatherAttackBlockPlayerTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimePlayerNotAttackBo).

    public long LastFatherAttackBlockPlayerTime { get; set; }

    // Lưu trạng thái FatherAttackBlockDuration (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: timeNotAttackBo).

    public int FatherAttackBlockDuration { get; set; } = 1800000;

    // Lưu trạng thái FatherTemporaryEnemies (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: temporaryEnemiesBo).

    public List<Player> FatherTemporaryEnemies { get; set; } = [];

    // Lưu trạng thái FatherJustRevived (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: justRevivedBo).

    public bool FatherJustRevived { get; set; }

    // Lưu trạng thái LastFatherRevivedTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimeRevivedBo).

    public long LastFatherRevivedTime { get; set; }

    // Lưu trạng thái NameFather (Java: nameFather).

    public string? NameFather { get; set; }

    // Lưu trạng thái MotherDoesNotAttack (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: doesNotAttackMe).

    public bool MotherDoesNotAttack { get; set; }

    // Lưu trạng thái LastMotherAttackBlockPlayerTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimePlayerNotAttackMe).

    public long LastMotherAttackBlockPlayerTime { get; set; }

    // Lưu trạng thái MotherAttackBlockDuration (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: timeNotAttackMe).

    public int MotherAttackBlockDuration { get; set; } = 1800000;

    // Lưu trạng thái MotherTemporaryEnemies (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: temporaryEnemiesMe).

    public List<Player> MotherTemporaryEnemies { get; set; } = [];

    // Lưu trạng thái MotherJustRevived (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: justRevivedMe).

    public bool MotherJustRevived { get; set; }

    // Lưu trạng thái LastMotherRevivedTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimeRevivedMe).

    public long LastMotherRevivedTime { get; set; }

    // Lưu trạng thái NameMother (Java: nameMother).

    public string? NameMother { get; set; }

    // Lưu trạng thái LoverDoesNotAttack (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: doesNotAttackNguoiYeu).

    public bool LoverDoesNotAttack { get; set; }

    // Lưu trạng thái LastLoverAttackBlockPlayerTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimePlayerNotAttackNguoiYeu).

    public long LastLoverAttackBlockPlayerTime { get; set; }

    // Lưu trạng thái LoverAttackBlockDuration (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: timeNotAttackNguoiYeu).

    public int LoverAttackBlockDuration { get; set; } = 1800000;

    // Lưu trạng thái LoverTemporaryEnemies (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: temporaryEnemiesNguoiYeu).

    public List<Player> LoverTemporaryEnemies { get; set; } = [];

    // Lưu trạng thái LoverJustRevived (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: justRevivedNguoiYeu).

    public bool LoverJustRevived { get; set; }

    // Lưu trạng thái LastLoverRevivedTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimeRevivedNguoiYeu).

    public long LastLoverRevivedTime { get; set; }

    // Lưu trạng thái ChildOneDoesNotAttack (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: doesNotAttackConone).

    public bool ChildOneDoesNotAttack { get; set; }

    // Lưu trạng thái LastChildOneAttackBlockPlayerTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimePlayerNotAttackConone).

    public long LastChildOneAttackBlockPlayerTime { get; set; }

    // Lưu trạng thái ChildOneAttackBlockDuration (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: timeNotAttackConone).

    public int ChildOneAttackBlockDuration { get; set; } = 1800000;

    // Lưu trạng thái ChildOneTemporaryEnemies (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: temporaryEnemiesConone).

    public List<Player> ChildOneTemporaryEnemies { get; set; } = [];

    // Lưu trạng thái ChildOneJustRevived (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: justRevivedConone).

    public bool ChildOneJustRevived { get; set; }

    // Lưu trạng thái LastChildOneRevivedTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimeRevivedConone).

    public long LastChildOneRevivedTime { get; set; }

    // Lưu trạng thái ChildTwoDoesNotAttack (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: doesNotAttackContwo).

    public bool ChildTwoDoesNotAttack { get; set; }

    // Lưu trạng thái LastChildTwoAttackBlockPlayerTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimePlayerNotAttackContwo).

    public long LastChildTwoAttackBlockPlayerTime { get; set; }

    // Lưu trạng thái ChildTwoAttackBlockDuration (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: timeNotAttackContwo).

    public int ChildTwoAttackBlockDuration { get; set; } = 1800000;

    // Lưu trạng thái ChildTwoTemporaryEnemies (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: temporaryEnemiesContwo).

    public List<Player> ChildTwoTemporaryEnemies { get; set; } = [];

    // Lưu trạng thái ChildTwoJustRevived (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: justRevivedContwo).

    public bool ChildTwoJustRevived { get; set; }

    // Lưu trạng thái LastChildTwoRevivedTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimeRevivedContwo).

    public long LastChildTwoRevivedTime { get; set; }

    // Lưu trạng thái ChildThreeDoesNotAttack (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: doesNotAttackConthree).

    public bool ChildThreeDoesNotAttack { get; set; }

    // Lưu trạng thái LastChildThreeAttackBlockPlayerTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimePlayerNotAttackConthree).

    public long LastChildThreeAttackBlockPlayerTime { get; set; }

    // Lưu trạng thái ChildThreeAttackBlockDuration (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: timeNotAttackConthree).

    public int ChildThreeAttackBlockDuration { get; set; } = 1800000;

    // Lưu trạng thái ChildThreeTemporaryEnemies (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: temporaryEnemiesConthree).

    public List<Player> ChildThreeTemporaryEnemies { get; set; } = [];

    // Lưu trạng thái ChildThreeJustRevived (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: justRevivedConthree).

    public bool ChildThreeJustRevived { get; set; }

    // Lưu trạng thái LastChildThreeRevivedTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimeRevivedConthree).

    public long LastChildThreeRevivedTime { get; set; }

    // Lưu trạng thái XSend (Java: xSend).

    public int XSend { get; set; }

    // Lưu trạng thái YSend (Java: ySend).

    public int YSend { get; set; }

    // Lưu trạng thái IsFly (Java: isFly).

    public bool IsFly { get; set; }

    // Lưu trạng thái LastTimeShenronAppeared_Halloween (Java: lastTimeShenronAppeared_Halloween).

    public long LastTimeShenronAppeared_Halloween { get; set; }

    // Lưu trạng thái IsShenronAppear_Halloween (Java: isShenronAppear_Halloween).

    public bool IsShenronAppear_Halloween { get; set; }

    // Lưu trạng thái ShenronEvent_Halloween (Java: shenronEvent_Halloween).

    public ShenronHalloweenEvent? ShenronEvent_Halloween { get; set; }

    // Lưu trạng thái LastTimeShenronAppeared_Christmas (Java: lastTimeShenronAppeared_Christmas).

    public long LastTimeShenronAppeared_Christmas { get; set; }

    // Lưu trạng thái IsShenronAppear_Christmas (Java: isShenronAppear_Christmas).

    public bool IsShenronAppear_Christmas { get; set; }

    // Lưu trạng thái ShenronEvent_Christmas (Java: shenronEvent_Christmas).

    public ShenronChristMasEvent? ShenronEvent_Christmas { get; set; }

    // Lưu trạng thái TypeChibi (Java: typeChibi).

    public int TypeChibi { get; set; }

    // Lưu trạng thái LastTimeChibi (Java: lastTimeChibi).

    public long LastTimeChibi { get; set; }

    // Lưu trạng thái LastTimeUpdateChibi (Java: lastTimeUpdateChibi).

    public long LastTimeUpdateChibi { get; set; }

    // Lưu trạng thái IdNRNM (Java: idNRNM).

    public short IdNRNM { get; set; } = -1;

    // Lưu trạng thái IdGo (Java: idGo).

    public short IdGo { get; set; } = -1;

    // Lưu trạng thái LastTimePickNRNM (Java: lastTimePickNRNM).

    public long LastTimePickNRNM { get; set; }

    // Lưu trạng thái IsBot (Java: isBot).

    public bool IsBot { get; set; }
    private long TimeSpawnBot { get; set; }

    // Lưu trạng thái IsBot_New (Java: isBot_New).

    public bool IsBot_New { get; set; }

    // Lưu trạng thái IsBotLogin (Java: isBotLogin).

    public bool IsBotLogin { get; set; }

    // Lưu trạng thái IsBot_Event (Java: isBot_Event).

    public bool IsBot_Event { get; set; }

    // Lưu trạng thái IsBot_Valentine (Java: isBot_Valentine).

    public bool IsBot_Valentine { get; set; }

    // Lưu trạng thái GoldNormar (Java: goldNormar).

    public int GoldNormar { get; set; }

    // Lưu trạng thái GoldVIP (Java: goldVIP).

    public int GoldVIP { get; set; }

    // Lưu trạng thái AutoHP (Java: autoHP).

    public bool AutoHP { get; set; }

    // Lưu trạng thái AutoKI (Java: autoKI).

    public bool AutoKI { get; set; }

    // Lưu trạng thái AutoSD (Java: autoSD).

    public bool AutoSD { get; set; }

    // Lưu trạng thái AutoArmor (Java: autoGiap).

    public bool AutoArmor { get; set; }

    // Lưu trạng thái WeeklyCard (Java: THE_TUAN).

    public int WeeklyCard { get; set; }

    // Lưu trạng thái LastWeeklyCardTime (Java: LASTTIME_THE_TUAN).

    public long LastWeeklyCardTime { get; set; }

    // Lưu trạng thái MonthlyCard (Java: THE_THANG).

    public int MonthlyCard { get; set; }

    // Lưu trạng thái LastMonthlyCardTime (Java: LASTTIME_THE_THANG).

    public long LastMonthlyCardTime { get; set; }

    // Lưu trạng thái YearlyCard (Java: THE_NAM).

    public int YearlyCard { get; set; }

    // Lưu trạng thái LastYearlyCardTime (Java: LASTTIME_THE_NAM).

    public long LastYearlyCardTime { get; set; }

    // Lưu trạng thái SupremeCard (Java: THE_CHI_TON).

    public int SupremeCard { get; set; }

    // Lưu trạng thái LastSupremeCardTime (Java: LASTTIME_THE_CHI_TON).

    public long LastSupremeCardTime { get; set; }

    // Lưu trạng thái CheckInCount (Java: DIEM_DANH).

    public int CheckInCount { get; set; }

    // Lưu trạng thái DailyCheckInMarker (Java: CHECK_DAY_ON_ONE).

    public int DailyCheckInMarker { get; set; }

    // Lưu trạng thái WeeklyCheckInCount (Java: DIEM_DANH_TUAN).

    public int WeeklyCheckInCount { get; set; }

    // Lưu trạng thái WeeklyCheckInMarker (Java: CHECK_DAY_ON_TUAN).

    public int WeeklyCheckInMarker { get; set; }

    // Lưu trạng thái MonthlyCheckInCount (Java: DIEM_DANH_THANG).

    public int MonthlyCheckInCount { get; set; }

    // Lưu trạng thái MonthlyCheckInMarker (Java: CHECK_DAY_ON_THANG).

    public int MonthlyCheckInMarker { get; set; }

    // Lưu trạng thái YearlyCheckInCount (Java: DIEM_DANH_NAM).

    public int YearlyCheckInCount { get; set; }

    // Lưu trạng thái YearlyCheckInMarker (Java: CHECK_DAY_ON_NAM).

    public int YearlyCheckInMarker { get; set; }

    // Lưu trạng thái SupremeCheckInCount (Java: DIEM_DANH_CHI_TON).

    public int SupremeCheckInCount { get; set; }

    // Lưu trạng thái SupremeCheckInMarker (Java: CHECK_DAY_ON_CHI_TON).

    public int SupremeCheckInMarker { get; set; }

    // Lưu trạng thái UsesEmperorTitle (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: isUseDanhHieu_ThienTu).

    public bool UsesEmperorTitle { get; set; }

    // Lưu trạng thái LastEmperorTitleTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimeDanhHieu_ThienTu).

    public long LastEmperorTitleTime { get; set; }

    // Lưu trạng thái UsesTitle_2 (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: isUseDanhHieu_2).

    public bool UsesTitle_2 { get; set; }

    // Lưu trạng thái LastTitleTime_2 (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimeDanhHieu_2).

    public long LastTitleTime_2 { get; set; }

    // Lưu trạng thái UsesTitle_3 (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: isUseDanhHieu_3).

    public bool UsesTitle_3 { get; set; }

    // Lưu trạng thái LastTitleTime_3 (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimeDanhHieu_3).

    public long LastTitleTime_3 { get; set; }

    // Lưu trạng thái UsesTitle_4 (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: isUseDanhHieu_4).

    public bool UsesTitle_4 { get; set; }

    // Lưu trạng thái LastTitleTime_4 (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: lastTimeDanhHieu_4).

    public long LastTitleTime_4 { get; set; }

    // Lưu trạng thái SquareRiceCakeCount (Java: slBanhChung).

    public int SquareRiceCakeCount { get; set; }

    // Lưu trạng thái CylindricalRiceCakeCount (Java: slBanhTet).

    public int CylindricalRiceCakeCount { get; set; }

    // Lưu trạng thái ItemsTradeWVP (Java: itemsTradeWVP).

    public List<Item> ItemsTradeWVP { get; set; } = [];

    // Lưu trạng thái GoldTradeWVP (Java: goldTradeWVP).

    public long GoldTradeWVP { get; set; }

    // Lưu trạng thái TradeWVP (Java: tradeWVP).

    public bool TradeWVP { get; set; }

    // Lưu trạng thái PlIdWVP (Java: plIdWVP).

    public long PlIdWVP { get; set; }

    // Lưu trạng thái LastTransformationTime (Java: lastTimeTranformation).

    public long LastTransformationTime { get; set; }

    // Lưu trạng thái TransformationState (Java: isbienhinh).

    public int TransformationState { get; set; }

    // Lưu trạng thái CountBDKB (Java: countBDKB).

    public sbyte CountBDKB { get; set; }

    // Lưu trạng thái FirstJoinBDKB (Java: firstJoinBDKB).

    public bool FirstJoinBDKB { get; set; }

    // Lưu trạng thái LastimeJoinBDKB (Java: lastimeJoinBDKB).

    public long LastimeJoinBDKB { get; set; }

    // Lưu trạng thái HighBetGold (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: goldTai).

    public int HighBetGold { get; set; }

    // Lưu trạng thái Last_time_dd (Java: last_time_dd).

    public long Last_time_dd { get; set; }

    // Lưu trạng thái LowBetGold (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: goldXiu).

    public int LowBetGold { get; set; }

    // Lưu trạng thái BeforeDispose (Java: beforeDispose).

    public bool BeforeDispose { get; set; }

    // Lưu trạng thái ErrorMap (Java: errorMap).

    public sbyte ErrorMap { get; set; }

    // Lưu trạng thái ErrorLocation (Java: errorLocation).

    public sbyte ErrorLocation { get; set; }

    // Lưu trạng thái ErrorPay (Java: errorPay).

    public sbyte ErrorPay { get; set; }

    // Lưu trạng thái TimeTravelDuration (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: thoigianduhanh).

    public long TimeTravelDuration { get; set; }

    // Lưu trạng thái IsTimeTraveling (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: isthoigianduhanh).

    public bool IsTimeTraveling { get; set; }

    // Lưu trạng thái RentsOil (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: thueOil).

    public bool RentsOil { get; set; }

    // Lưu trạng thái RentsMacki (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: thueMacki).

    public bool RentsMacki { get; set; }

    // Lưu trạng thái RentsGas (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: thueGas).

    public bool RentsGas { get; set; }

    // Lưu trạng thái RentsElectric (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: thueElec).

    public bool RentsElectric { get; set; }

    // Lưu trạng thái IsWukongSealReleased (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: giaiPhongAnNgoKhong).

    public bool IsWukongSealReleased { get; set; }

    // Lưu trạng thái IsBossFlag (Java: isBossFlag).

    public bool IsBossFlag { get; set; }

    // Lưu trạng thái WukongSealReleaseState (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: giaiphongan).

    public sbyte WukongSealReleaseState { get; set; }

    // Lưu trạng thái Solanhotong (Java: solanhotong).

    public short Solanhotong { get; set; }

    // Lưu trạng thái DailyMedicineReceived (Java: nhanThuocTrongNgay).

    public sbyte DailyMedicineReceived { get; set; }

    // Lưu trạng thái DailyMedicineTime (Java: timeTrongNgay).

    public long DailyMedicineTime { get; set; }

    // Lưu trạng thái BossRegeneratesHealth (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: bosshoimau).

    public bool BossRegeneratesHealth { get; set; }

    // Lưu trạng thái Trbne (Java: trbne).

    public sbyte Trbne { get; set; }

    // Lưu trạng thái ArmorAssistLevel (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: trbgiap).

    public sbyte ArmorAssistLevel { get; set; }

    // Lưu trạng thái Actived (Java: actived).

    public int Actived { get; set; }

    // Lưu trạng thái Account_id (Java: account_id).

    public int Account_id { get; set; }

    // Lưu trạng thái ElapsedTimeCounter (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: demthoigian).

    public long ElapsedTimeCounter { get; set; }

    // Lưu trạng thái Timeres (Java: timeres).

    public long Timeres { get; set; }

    // Lưu trạng thái BossFightTime (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: timedanhboss).

    public int BossFightTime { get; set; }

    // Lưu trạng thái BossSpawned (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: bossxuathien).

    public int BossSpawned { get; set; }

    // Lưu trạng thái Pvp (Java: pvp).

    public IPVP? Pvp { get; set; }

    // Lưu trạng thái MaxTime (Java: maxTime).

    public sbyte MaxTime { get; set; } = 30;

    // Lưu trạng thái Type (Java: type).

    public sbyte Type { get; set; }

    // Lưu trạng thái Timeoff (Java: timeoff).

    public long Timeoff { get; set; }

    // Lưu trạng thái Buff (Java: buff).

    public Buff Buff { get; set; } = Buff.NONE;

    // Lưu trạng thái Pointfusion (Java: pointfusion).

    public PointFusion Pointfusion { get; set; } = null!;

    // Lưu trạng thái DailyGiftData (Java: dailyGiftData).

    public List<DailyGiftData> DailyGiftData { get; set; } = [];

    // Lưu trạng thái Event (Java: event).

    public PlayerEvent Event { get; set; } = null!;

    // Lưu trạng thái Saibamen (Java: saibamen).

    public SaiBaMen? Saibamen { get; set; }

    // Lưu trạng thái IsSaibamen (Java: isSaibamen).

    public bool IsSaibamen { get; set; }

    // Lưu trạng thái Badges (Java: badges).

    public Badges Badges { get; set; } = null!;

    // Lưu trạng thái MapIdBeforeLogout (Java: mapIdBeforeLogout).

    public int MapIdBeforeLogout { get; set; }

    // Lưu trạng thái BodyClone (Java: phanThan).

    public BodyClone? BodyClone { get; set; }

    // Lưu trạng thái MapBlackBall (Java: mapBlackBall).

    public List<Zone>? MapBlackBall { get; set; }

    // Lưu trạng thái MapMaBu (Java: mapMaBu).

    public List<Zone>? MapMaBu { get; set; }

    // Lưu trạng thái DataBadges (Java: dataBadges).

    public List<BadgesData> DataBadges { get; set; } = [];

    // Lưu trạng thái DataTaskBadges (Java: dataTaskBadges).

    public List<BadgesTask> DataTaskBadges { get; set; } = [];

    // Lưu trạng thái ItemEvent (Java: itemEvent).

    public ItemEvent ItemEvent { get; set; } = null!;

    // Lưu trạng thái Zone (Java: zone).

    public Zone? Zone { get; set; }

    // Lưu trạng thái MapBeforeCapsule (Java: mapBeforeCapsule).

    public Zone? MapBeforeCapsule { get; set; }

    // Lưu trạng thái MapCapsule (Java: mapCapsule).

    public List<Zone>? MapCapsule { get; set; }

    // Lưu trạng thái Pet (Java: detu).

    public Pet? Pet { get; set; }

    // Lưu trạng thái PetFollow (Java: petFollow).

    public PetFollow? PetFollow { get; set; }

    // Lưu trạng thái TangMonk (Java: duongtang).

    public TangMonk? TangMonk { get; set; }

    // Lưu trạng thái SuperRank (Java: superRank).

    public SuperRank SuperRank { get; set; } = null!;

    // Lưu trạng thái EggMob (Java: deTrung).

    public EggMob? EggMob { get; set; }

    // Lưu trạng thái Location (Java: location).

    public Location Location { get; set; } = null!;

    // Thực hiện SetClothes (Java: setClothes).

    public SetClothes SetClothes { get; set; } = null!;

    // Lưu trạng thái EffectSkill (Java: effectSkill).

    public EffectSkill EffectSkill { get; set; } = null!;

    // Lưu trạng thái MabuEgg (Java: mabuEgg).

    public MabuEgg? MabuEgg { get; set; }

    // Lưu trạng thái BillEgg (Java: billEgg).

    public BillEgg? BillEgg { get; set; }

    // Lưu trạng thái WatermelonPlant (Java: duahau).

    public MelonPlant? WatermelonPlant { get; set; }

    // Lưu trạng thái PlayerTask (Java: playerTask).

    public TaskPlayer PlayerTask { get; set; } = null!;

    // Lưu trạng thái ItemTime (Java: itemTime).

    public ItemTime ItemTime { get; set; } = null!;

    // Lưu trạng thái Fusion (Java: fusion).

    public Fusion Fusion { get; set; } = null!;

    // Lưu trạng thái MagicTree (Java: magicTree).

    public MagicTree? MagicTree { get; set; }

    // Lưu trạng thái PlayerIntrinsic (Java: playerIntrinsic).

    public IntrinsicPlayer PlayerIntrinsic { get; set; } = null!;

    // Lưu trạng thái Inventory (Java: inventory).

    public Inventory Inventory { get; set; } = null!;

    // Lưu trạng thái PlayerSkill (Java: playerSkill).

    public PlayerSkill PlayerSkill { get; set; } = null!;

    // Lưu trạng thái Combine (Java: combine).

    public Combine Combine { get; set; } = null!;

    // Lưu trạng thái IDMark (Java: iDMark).

    public IDMark IDMark { get; set; } = null!;

    // Lưu trạng thái Charms (Java: charms).

    public Charms Charms { get; set; } = null!;

    // Lưu trạng thái EffectSkin (Java: effectSkin).

    public EffectSkin EffectSkin { get; set; } = null!;

    // Lưu trạng thái Gift (Java: gift).

    public Gift Gift { get; set; } = null!;

    // Lưu trạng thái NPoint (Java: nPoint).

    public NPoint NPoint { get; set; } = null!;

    // Lưu trạng thái RewardBlackBall (Java: rewardBlackBall).

    public RewardBlackBall RewardBlackBall { get; set; } = null!;

    // Lưu trạng thái EffectFlagBag (Java: effectFlagBag).

    public EffectFlagBag EffectFlagBag { get; set; } = null!;

    // Lưu trạng thái FightMabu (Java: fightMabu).

    public FightMabu FightMabu { get; set; } = null!;

    // Lưu trạng thái NewSkill (Java: newSkill).

    public NewSkill NewSkill { get; set; } = null!;

    // Lưu trạng thái Achievement (Java: achievement).

    public Achievement Achievement { get; set; } = null!;

    // Lưu trạng thái Clan (Java: clan).

    public Clan? Clan { get; set; }

    // Lưu trạng thái ClanMember (Java: clanMember).

    public ClanMember? ClanMember { get; set; }

    // Lưu trạng thái Satellite (Java: satellite).

    public Satellite Satellite { get; set; } = null!;

    // Lưu trạng thái Friends (Java: friends).

    public List<Friend> Friends { get; set; } = [];

    // Lưu trạng thái Enemies (Java: enemies).

    public List<Enemy> Enemies { get; set; } = [];
    private DropItem DropItem { get; set; } = null!;

    // Lưu trạng thái Cards (Java: cards).

    public List<Card> Cards { get; set; } = [];

    // Lưu trạng thái IsAutoRespawn (Java: isAutoHoiSinh).

    public bool IsAutoRespawn { get; set; }

    // Lưu trạng thái AccountAgeDays (Java: soNgayTaoAcc).

    public int AccountAgeDays { get; set; }

    // Lưu trạng thái Nationality (Java: quocTich).

    public sbyte Nationality { get; set; }

    // Lưu trạng thái TimeChangeZone (Java: timeChangeZone).

    public long TimeChangeZone { get; set; }

    // Lưu trạng thái LastUseOptionTime (Java: lastUseOptionTime).

    public long LastUseOptionTime { get; set; }

    // Lưu trạng thái LastRegenTime (Java: lastTimeHoiPhuc).

    public long LastRegenTime { get; set; }

    // Lưu trạng thái CatGodItemDamageValue (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: ducNTdamethanmeo).

    public float CatGodItemDamageValue { get; set; }

    // Lưu trạng thái TimeChatKhukhu (Java: timeChatKhukhu).

    public long TimeChatKhukhu { get; set; }

    // Lưu trạng thái IdEffChar (Java: idEffChar).

    public List<int> IdEffChar { get; set; } = [];

    // Lưu trạng thái BoughtSkill (Java: boughtSkill).

    public List<int> BoughtSkill { get; set; } = [];

    // Lưu trạng thái LearnSkill (Java: learnSkill).

    public PlayerSkill LearnSkill { get; set; } = null!;

    // Lưu trạng thái GoldChan (Java: goldChan).

    public int GoldChan { get; set; }

    // Lưu trạng thái GoldLe (Java: goldLe).

    public int GoldLe { get; set; }

    // Lưu trạng thái ReceivedGift50k (Java: receivedGift50k).

    public bool ReceivedGift50k { get; set; }

    // Lưu trạng thái ReceivedGift100k (Java: receivedGift100k).

    public bool ReceivedGift100k { get; set; }

    // Lưu trạng thái TopUpMilestones (nghĩa chưa chắc, cần chủ dự án duyệt) (Java: mocnap).

    public int[] TopUpMilestones { get; set; } = [0, 0, 0, 0, 0];

    // Lưu trạng thái LastTimeSendTextTime (Java: lastTimeSendTextTime).

    public long LastTimeSendTextTime { get; set; }

    // Lưu trạng thái LastTimeSendTextTime_2 (Java: lastTimeSendTextTime_2).

    public long LastTimeSendTextTime_2 { get; set; }

    // Khởi tạo shell; nowMs thay thời gian tường Java cho LastUseOptionTime, truyền 0 khi chưa biết.

    public Player(long nowMs = 0)
    {
        LearnSkill = new PlayerSkill();
        LastUseOptionTime = nowMs;
        Location = new Location();
        NPoint = new NPoint(this);
        Inventory = new Inventory();
        PlayerSkill = new PlayerSkill(this);
        SetClothes = new SetClothes(this);
        EffectSkill = new EffectSkill(this);
        Fusion = new Fusion(this);
        PlayerIntrinsic = new IntrinsicPlayer();
        RewardBlackBall = new RewardBlackBall(this);
        EffectFlagBag = new EffectFlagBag();
        FightMabu = new FightMabu(this);
        Satellite = new Satellite();
        IDMark = new IDMark();
        Combine = new Combine();
        PlayerTask = new TaskPlayer();
        ItemTime = new ItemTime(this);
        Charms = new Charms();
        Gift = new Gift(this);
        EffectSkin = new EffectSkin(this);
        NewSkill = new NewSkill(this);
        Achievement = new Achievement(this);
        DropItem = new DropItem(this);
        SuperRank = new SuperRank(this);
        ItemEvent = new ItemEvent(this);
        Badges = new Badges();
        Event = new PlayerEvent(this);
        Training = new Training();
        Pointfusion = new PointFusion(this);
        Buff = Buff.NONE;
    }
}
