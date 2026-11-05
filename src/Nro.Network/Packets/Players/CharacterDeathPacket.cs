namespace Nro.Network.Packets.Players;

// Dữ liệu hai gói chết riêng của chính người chơi và người chơi khác. (Java: Service.charDie)
// Nguồn Java: nro/services/Service.java (dòng 1213–1239 trong bảng I-02; bản SRC hiện có 761–789).
public sealed record CharacterDeathPacket(long CharacterId, int X, int Y);
