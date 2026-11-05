namespace Nro.Game.Cards;

// Chỉ số của thẻ radar bất biến sau khi nạp. Nguồn Java: nro/card/OptionCard.java (dòng 7–24).
public sealed record RadarOption(int Id, int Param, sbyte Active);
