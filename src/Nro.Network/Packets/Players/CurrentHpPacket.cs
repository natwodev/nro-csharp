namespace Nro.Network.Packets.Players;

// Dữ liệu HP hiện tại của người chơi.
// Nguồn Java: nro/services/PlayerService.java (dòng 114–124).
public sealed record CurrentHpPacket(long Hp);
