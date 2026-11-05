namespace Nro.Network.Packets.Players;

// Dữ liệu tiền và chỉ số hiện tại khi vào/gọi refresh client.
// Nguồn Java: nro/services/PlayerService.java (dòng 154–177).
public sealed record PlayerInfoHpMpMoneyPacket(long Gold, int Gem, long Hp, long Mp, int Ruby);
