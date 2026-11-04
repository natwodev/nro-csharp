namespace Nro.Game.Effects.Services;

// Cung cấp hiệu ứng bản đồ gồm lớp phủ đóng băng và gói hiệu ứng bản đồ 113, thay singleton gI() của Java.
// Nguồn Java: nro/effect/EffectMapService.java (cổng dịch vụ của lớp chính).
public interface IEffectMapService
{
    // Gửi gói 113 cho một người chơi; giữ Java ghi id ở byte thứ nhất và thứ ba, đồng thời bỏ qua loop (Java: sendEffectMapToPlayer).
    void SendEffectMapToPlayer(Player player, int id, int layer, int loop, int x, int y, int delay);

    // Gửi lớp phủ đóng băng, gói -128 hiệu ứng 202, cho cả zone (Java: sendCharEffect_HoaBang).
    void SendFrozenCharacterEffect(Player player);

    // Gỡ lớp phủ đóng băng; Java gửi cho người chơi và mọi người đang trực tuyến (Java: RemoveFrozenCharacterEffect).
    void RemoveFrozenCharacterEffect(Player player);

    // Gửi gói 113 đến mọi người chơi trong zone (Java: sendEffectMapToAllInMap).
    void SendEffectMapToAllInMap(Zone zone, int id, int layer, int loop, int x, int y, int delay);

    // Gửi gói 113 đến zone của người chơi chỉ định (Java: sendEffectMapToAllInMap).
    void SendEffectMapToAllInMap(Player player, int id, int layer, int loop, int x, int y, int delay);
}
