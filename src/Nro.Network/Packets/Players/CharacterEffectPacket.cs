namespace Nro.Network.Packets.Players;

// Dữ liệu hiệu ứng nhân vật sub 0, giữ các ép short/byte Java ở codec.
// Nguồn Java: nro/services/Service.java (dòng 119–145).
public sealed record CharacterEffectPacket(int CharacterId, int EffectId, int Layer, int Loop, int LoopCount, int Stand);
