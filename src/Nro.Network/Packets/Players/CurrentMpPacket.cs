namespace Nro.Network.Packets.Players;

// Dữ liệu MP hiện tại của người chơi.
// Nguồn Java: nro/services/PlayerService.java (dòng 126–136).
public sealed record CurrentMpPacket(long Mp);
