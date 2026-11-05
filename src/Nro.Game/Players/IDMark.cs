using Nro.Core.Consts;
using Nro.Game.Players.Ports;
using Nro.Game.World.Maps;

namespace Nro.Game.Players;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/IDMark.java (dòng 1–141).
public sealed class IDMark : IDisposable
{
    // Mã vật phẩm đang hiện trên đầu (Java: idItemUpTop).
    public int TopItemId { get; set; }

    // Kiểu chuyển bản đồ (Java: typeChangeMap).
    public int MapChangeType { get; set; }

    // Chỉ số menu NPC hiện tại (Java: indexMenu).
    public int MenuIndex { get; set; }

    // Kiểu ô nhập đang mở (Java: typeInput).
    public int InputType { get; set; }

    // Kiểu vòng quay may mắn (Java: typeLuckyRound).
    public sbyte LuckyRoundType { get; set; }

    // Id người chơi thách đấu (Java: idPlayThachDau).
    public long ChallengePlayerId { get; set; }

    // Vàng thách đấu (Java: goldThachDau).
    public int ChallengeGold { get; set; }

    // Id nhân vật đã hạ, mặc định -9999 (Java: killCharId).
    public long KillCharacterId { get; set; } = -9999;

    // Id kẻ địch (Java: idEnemy).
    public long EnemyId { get; set; }

    // Cửa hàng đang mở qua cổng nhỏ vì Shop chưa được port (Java: shopOpen).
    public IPlayerShop? OpenShop { get; set; }

    // Tag cửa hàng đang mở (Java: tagNameShop).
    public string? ShopTagName { get; set; }

    // Id tàu vũ trụ (Java: idSpaceShip).
    public sbyte SpaceShipId { get; set; }

    // Sát thương phản sát thương (Java: damePST).
    public long ReflectDamage { get; set; }

    // Mốc cấm gần nhất (Java: lastTimeBan).
    public long LastBanTime { get; set; }

    // Cờ bị cấm (Java: isBan).
    public bool IsBanned { get; set; }

    // Cờ đồng ý giao dịch (Java: acpTrade).
    public bool IsTradeAccepted { get; set; }

    // Id người chơi giao dịch, mặc định -1 (Java: playerTradeId).
    public int TradePlayerId { get; set; } = -1;

    // Người chơi giao dịch (Java: playerTrade).
    public Player? TradePlayer { get; set; }

    // Mốc giao dịch gần nhất (Java: lastTimeTrade).
    public long LastTradeTime { get; set; }

    // Mốc thông báo giữ ngọc đen gần nhất (Java: lastTimeNotifyTimeHoldBlackBall).
    public long LastBlackBallHoldNotificationTime { get; set; }

    // Mốc bắt đầu giữ ngọc đen (Java: lastTimeHoldBlackBall).
    public long LastBlackBallHoldTime { get; set; }

    // Id tạm ngọc đen đang giữ, mặc định -1 (Java: tempIdBlackBallHold).
    public int TemporaryBlackBallHoldId { get; set; } = -1;

    // Cờ đang giữ ngọc đen (Java: holdBlackBall).
    public bool HoldsBlackBall { get; set; }

    // Id tạm ngọc Namek đang giữ, mặc định -1 (Java: tempIdNamecBallHold).
    public int TemporaryNamekBallHoldId { get; set; } = -1;

    // Cờ đang giữ ngọc Namek (Java: holdNamecBall).
    public bool HoldsNamekBall { get; set; }

    // Trạng thái Tranh Ngọc Namếc, mặc định -1 (Java: isTranhNgoc).
    public sbyte NamekWarState { get; set; } = -1;

    // Cờ đã tải toàn bộ dữ liệu người chơi (Java: loadedAllDataPlayer).
    public bool IsAllPlayerDataLoaded { get; set; }

    // Mốc đổi cờ gần nhất (Java: lastTimeChangeFlag).
    public long LastFlagChangeTime { get; set; }

    // Giá trị MBV chưa rõ nghĩa (Java: mbv).
    public int Mbv { get; set; }

    // Kiểu rồng thần, mặc định -1 (Java: shenronType).
    public int ShenronType { get; set; } = -1;

    // Cờ đi tới tương lai (Java: gotoFuture).
    public bool GoesToFuture { get; set; }

    // Mốc đi tới tương lai (Java: lastTimeGoToFuture).
    public long LastFutureTravelTime { get; set; }

    // Zone khí gas hủy diệt qua kiểu Zone đã port (Java: zoneKhiGasHuyDiet).
    public Zone? DestronGasZone { get; set; }

    // Tọa độ X map khí gas hủy diệt (Java: xMapKhiGasHuyDiet).
    public int DestronGasMapX { get; set; }

    // Tọa độ Y map khí gas hủy diệt (Java: yMapKhiGasHuyDiet).
    public int DestronGasMapY { get; set; }

    // Cờ đi tới khí gas hủy diệt (Java: goToKGHD).
    public bool GoesToDestronGas { get; set; }

    // Mốc đi tới khí gas hủy diệt (Java: lastTimeGoToKGHD).
    public long LastDestronGasTravelTime { get; set; }

    // Mốc đổi zone gần nhất (Java: lastTimeChangeZone).
    public long LastZoneChangeTime { get; set; }

    // Mốc chat toàn cục gần nhất (Java: lastTimeChatGlobal).
    public long LastGlobalChatTime { get; set; }

    // Mốc chat riêng gần nhất (Java: lastTimeChatPrivate).
    public long LastPrivateChatTime { get; set; }

    // Mốc nhặt vật phẩm gần nhất (Java: lastTimePickItem).
    public long LastItemPickTime { get; set; }

    // Cờ đi tới Bản Đồ Kho Báu (Java: goToBDKB).
    public bool GoesToTreasureUnderSea { get; set; }

    // Mốc đi tới Bản Đồ Kho Báu (Java: lastTimeGoToBDKB).
    public long LastTreasureUnderSeaTravelTime { get; set; }

    // Mốc dính bẫy Bản Đồ Kho Báu (Java: lastTimeAnXienTrapBDKB).
    public long LastTreasureUnderSeaTrapTime { get; set; }

    // Cờ đi du hành thời gian (Java: GoToDuHanhThoiGian).
    public bool GoesToTimeTravel { get; set; }

    // Mốc đi du hành thời gian (Java: LastTimeDuHanhThoiGian).
    public long LastTimeTravelTime { get; set; }

    // Cờ trở về thời gian lần một (Java: GoToTroVeThoiGian).
    public bool ReturnsFromTimeTravel { get; set; }

    // Mốc trở về thời gian lần một (Java: LastTimeTroVeThoiGian).
    public long LastTimeTravelReturnTime { get; set; }

    // Cờ trở về thời gian lần hai (Java: GoToTroVeThoiGian2).
    public bool ReturnsFromTimeTravelSecond { get; set; }

    // Mốc trở về thời gian lần hai (Java: LastTimeTroVeThoiGian2).
    public long LastSecondTimeTravelReturnTime { get; set; }

    // Cờ đi về nhà (Java: goToHome).
    public bool GoesHome { get; set; }

    // Mốc đi về nhà (Java: lastTimeGoToHome).
    public long LastHomeTravelTime { get; set; }

    // NPC được chọn qua cổng nhỏ vì Npc chưa được port (Java: npcChose).
    public IPlayerNpc? ChosenNpc { get; set; }

    // Loại thẻ (Java: loaiThe).
    public sbyte CardType { get; set; }

    // Mốc trả thù gần nhất (Java: lastTimeRevenge).
    public long LastRevengeTime { get; set; }

    // Cờ giao dịch World Peace (Java: transactionWP).
    public bool IsWorldPeaceTransaction { get; set; }

    // Cờ giao dịch World Versus Peace (Java: transactionWVP).
    public bool IsWorldVersusPeaceTransaction { get; set; }

    // Giá trị OTT chưa rõ nghĩa (Java: ott).
    public int Ott { get; set; }

    // Kiểu menu (Java: menuType).
    public int MenuType { get; set; }

    // Id NPC đã mở menu (Java: npcId).
    public sbyte NpcId { get; set; }

    // Trả trạng thái Tranh Ngọc, tương đương Java getTranhNgoc (Java: getTranhNgoc).
    public sbyte GetNamekWarState() => NamekWarState;

    // Đặt trạng thái Tranh Ngọc, tương đương Java setTranhNgoc (Java: setTranhNgoc).
    public void SetNamekWarState(sbyte state) => NamekWarState = state;

    // Trả trạng thái Tranh Ngọc, accessor Java trùng nghĩa (Java: getIsTranhNgoc).
    public sbyte GetIsNamekWarState() => NamekWarState;

    // Đặt trạng thái Tranh Ngọc, accessor Java trùng nghĩa (Java: setIsTranhNgoc).
    public void SetIsNamekWarState(sbyte state) => NamekWarState = state;

    // Kiểm tra đang ở menu gốc của NPC (Java: isBaseMenu).
    public bool IsBaseMenu() => MenuIndex == ConstNpc.BASE_MENU;

    // Bỏ các tham chiếu đối tượng như Java; shop được dispose trước khi xóa (Java: dispose).
    public void Dispose()
    {
        OpenShop?.Dispose();
        OpenShop = null;
        ChosenNpc = null;
        ShopTagName = null;
        TradePlayer = null;
        DestronGasZone = null;
    }
}
