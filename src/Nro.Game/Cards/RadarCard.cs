namespace Nro.Game.Cards;

// Thẻ radar bất biến sau khi nạp từ dữ liệu tĩnh. Nguồn Java: nro/card/RadarCard.java (dòng 10–39).
public sealed record RadarCard(
    int Id,
    short IconId,
    sbyte Rank,
    sbyte Max,
    sbyte Type,
    short Template,
    short Head,
    short Body,
    short Leg,
    short Bag,
    string Name,
    string Info,
    IReadOnlyList<RadarOption> Options,
    short AuraId);
