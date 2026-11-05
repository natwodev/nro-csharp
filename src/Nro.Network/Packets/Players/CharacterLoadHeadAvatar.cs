namespace Nro.Network.Packets.Players;

// Một cặp đầu và avatar của ME_LOAD_ALL. (Java: DataGame.sendHeadAvatar)
// Nguồn Java: nro/services/Service.java (dòng 860–958 trong bảng I-02; bản SRC hiện có 479–583).
public sealed record CharacterLoadHeadAvatar(int HeadId, int AvatarId);
