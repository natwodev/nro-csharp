namespace Nro.Game.Players.Ports;

// Ngữ cảnh xác định cho một tick Player: thời gian, ngẫu nhiên và các side effect runtime đều do Zone/host cấp.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 848–1458).
public sealed class PlayerRuntimeContext
{
    // Đồng hồ epoch mili giây của tick (Java: System.currentTimeMillis).
    public required IPlayerClock Clock { get; init; }

    // Nguồn ngẫu nhiên xác định được cho tick (Java: Util.isTrue, Util.nextInt).
    public required IPlayerRandom Random { get; init; }

    // Cổng các service/map/event chưa port hoàn chỉnh (Java: Service, MapService, ClanService, NPC/Boss).
    public required IPlayerRuntimeServices Services { get; init; }

    // Cổng companion của Zone hiện tại; null cho tới khi track I ghép adapter Zone/MapService/Combat.
    public IPlayerCompanionGateway? CompanionGateway { get; init; }
}
