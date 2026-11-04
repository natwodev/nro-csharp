namespace Nro.Game.Effects.Services;

// Bật và tắt các hiệu ứng "skin" có thời hạn lưu trong EffectSkin, thay singleton gI() Java.
// Nguồn Java: nro/effect/EffectSkinService.java (cổng dịch vụ của lớp chính).
public interface IEffectSkinService
{
    // Bật hiệu ứng băng Drabura kèm lớp phủ và gói hiệu ứng (Java: setDraburaFrost).
    void SetDraburaFrost(Player player, long lastTime, int time);

    // Gỡ hiệu ứng băng Drabura (Java: removeDraburaFrost).
    void RemoveDraburaFrost(Player player);

    // Bật trạng thái làm chậm với mốc và thời lượng (Java: setSlow).
    void SetSlow(Player player, long lastTimeSlow, int timeSlow);

    // Gỡ trạng thái làm chậm (Java: removeSlow).
    void RemoveSlow(Player player);

    // Bật trạng thái sôcôla với mốc và thời lượng (Java: setSocola).
    void SetChocolate(Player player, long lastTimeSocola, int timeSocola);

    // Gỡ trạng thái sôcôla (Java: removeSocola).
    void RemoveChocolate(Player player);

    // Bật Thỏ Đại Ca, nhưng không làm gì khi chỉ số người chơi đã có cờ này (Java: setThoDaiKa).
    void SetRabbitLeader(Player player, long lastTime, int time);

    // Gỡ hiệu ứng Thỏ Đại Ca (Java: removeThoDaiKa).
    void RemoveRabbitLeader(Player player);

    // Bật cuồng nộ Nezuko (Java: setCuongNo).
    void SetRage(Player player, long lastTimeRage, int timeRage);

    // Gỡ cuồng nộ Nezuko (Java: removeCuongNo).
    void RemoveRage(Player player);

    // Bật hiệu ứng Inosuke (Java: setInosuke).
    void SetInosuke(Player player, long lastTimeTanjiro, int timeTanjiro);

    // Gỡ hiệu ứng Inosuke (Java: removeInosuke).
    void RemoveInosuke(Player player);

    // Bật hiệu ứng Tanjiro (Java: setTanjiro).
    void SetTanjiro(Player player, long lastTimeTanjiro, int timeTanjiro);

    // Gỡ hiệu ứng Tanjiro (Java: removeTanjiro).
    void RemoveTanjiro(Player player);

    // Bật hiệu ứng Zenitsu (Java: setZenitsu).
    void SetZenitsu(Player player, long lastTimeZenitsu, int timeZenitsu);

    // Gỡ hiệu ứng Zenitsu (Java: removeZenitsu).
    void RemoveZenitsu(Player player);

    // Bật hiệu ứng InoHashi (Java: setInoHashi).
    void SetInosukeHashibira(Player player, long lastTimeInoHashi, int timeInoHashi);

    // Gỡ hiệu ứng InoHashi (Java: removeInoHashi).
    void RemoveInosukeHashibira(Player player);

    // Gửi hiệu ứng skin lên người chơi bằng gói -124, id hiệu ứng ghi một byte (Java: sendEffectPlayer).
    void SendEffectPlayer(Player plUseSkill, Player plTarget, sbyte toggle, sbyte effect);
}
