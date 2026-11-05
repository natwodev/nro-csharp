namespace Nro.Network.Packets.Players;

// Snapshot đăng nhập ME_LOAD_ALL, tách khỏi Player để codec thuần và kiểm thử byte-exact.
// Nguồn Java: nro/services/Service.java (dòng 860–958 trong bảng I-02; bản SRC hiện có 479–583).
public sealed record CharacterLoadPacket(
    long CharacterId, int TaskMainId, sbyte Gender, short Head, string Name, sbyte TypePk, long Power,
    IReadOnlyList<short> SkillIds, long Gold, int Ruby, int Gem,
    IReadOnlyList<CharacterLoadItem> BodyItems, IReadOnlyList<CharacterLoadItem> BagItems, IReadOnlyList<CharacterLoadItem> BoxItems,
    IReadOnlyList<CharacterLoadHeadAvatar> HeadAvatars, bool IsFusion, int DeltaTime, bool IsNewMember,
    short Aura, sbyte FrontEffect, int Hat);
