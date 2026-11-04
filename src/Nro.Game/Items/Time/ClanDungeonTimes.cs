namespace Nro.Game.Items.Time;

// Thời điểm, trạng thái và thời lượng các phó bản bang mà ItemTimeService dùng để thông báo cho người chơi.
// Nguồn Java: models/Item/ItemTimeService.java, nro/clan/Clan.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác).
public sealed class ClanDungeonTimes
{
    // Bang đã vào phó bản Doanh trại độc nhãn hay chưa (Java: haveGoneDoanhTrai)
    public bool HaveGoneBarracks { get; init; }

    // Thời điểm (mili giây) mở phó bản Doanh trại độc nhãn của bang (Java: lastTimeOpenDoanhTrai)
    public long LastTimeOpenBarracks { get; init; }

    // Thời lượng (mili giây) phó bản Doanh trại độc nhãn (Java: RedRibbonHQ.TIME_DOANH_TRAI)
    public int BarracksDurationMs { get; init; }

    // Bang có phó bản Doanh trại độc nhãn đang hoạt động hay không (Java: doanhTrai != null)
    public bool HasBarracks { get; init; }

    // Phó bản Doanh trại độc nhãn đang trong thời gian nhận thưởng hay không (Java: doanhTrai.isTimePicking)
    public bool BarracksIsTimePicking { get; init; }

    // Thời điểm (mili giây) bắt đầu thời gian nhận thưởng Doanh trại độc nhãn (Java: doanhTrai.lastTimePick)
    public long BarracksLastTimePick { get; init; }

    // Thời lượng (mili giây) nhận thưởng Doanh trại độc nhãn (Java: RedRibbonHQ.TIME_PICK_DOANH_TRAI)
    public int BarracksPickDurationMs { get; init; }

    // Thời điểm (mili giây) mở phó bản Bản đồ kho báu của bang (Java: lastTimeOpenBanDoKhoBau)
    public long LastTimeOpenTreasureUnderSea { get; init; }

    // Thời lượng (mili giây) phó bản Bản đồ kho báu (Java: TreasureUnderSea.TIME_BAN_DO_KHO_BAU)
    public int TreasureUnderSeaDurationMs { get; init; }

    // Thời điểm (mili giây) mở phó bản Con đường rắn độc của bang (Java: lastTimeOpenConDuongRanDoc)
    public long LastTimeOpenSnakeWay { get; init; }

    // Thời lượng (mili giây) phó bản Con đường rắn độc (Java: SnakeWay.TIME_CON_DUONG_RAN_DOC)
    public int SnakeWayDurationMs { get; init; }

    // Thời điểm (mili giây) mở phó bản Khí gas hủy diệt của bang (Java: lastTimeOpenKhiGasHuyDiet)
    public long LastTimeOpenDestronGas { get; init; }

    // Thời lượng (mili giây) phó bản Khí gas hủy diệt (Java: DestronGas.TIME_KHI_GAS_HUY_DIET)
    public int DestronGasDurationMs { get; init; }

    // Thời điểm (mili giây) mở bản đồ Boss bang hội (Java: lastTimeOpenBossOfTheGangs)
    public long LastTimeOpenBossOfTheGangs { get; init; }

    // Thời lượng (mili giây) bản đồ Boss bang hội (Java: BossOfTheGangs.TIME_MAP_BOSS_BANG_HOI)
    public int BossOfTheGangsDurationMs { get; init; }

    // Vòng Boss bang hội hiện tại (Java: boss_clan_round)
    public int BossClanRound { get; init; }
}
