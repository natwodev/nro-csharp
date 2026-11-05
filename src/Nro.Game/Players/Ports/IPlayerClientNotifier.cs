namespace Nro.Game.Players.Ports;

// Cổng gửi thông báo client của Player, thay DailyGiftService/ItemTimeService/Service singleton.
// Nguồn Java: nro/player/Player.java (dòng 2093–2141).
public interface IPlayerClientNotifier
{
    // Kiểm tra phần quà hằng ngày còn nhận được hay không (Java: DailyGiftService.checkDailyGift).
    bool CanReceiveDailyGift(Player player, sbyte dailyGiftId);

    // Gửi thông báo có thời hạn tới client (Java: ItemTimeService.sendTextTime).
    void SendTimedText(Player player, sbyte textId, string text, sbyte seconds);

    // Gửi gói sub-command 5 chứa HP hiện tại; adapter tạo và dọn Message (Java: Service.messageSubCommand(5)).
    void SendCurrentHp(Player player, long hp);

    // Gửi gói sub-command 6 chứa MP hiện tại; adapter tạo và dọn Message (Java: Service.messageSubCommand(6)).
    void SendCurrentMp(Player player, long mp);

    // Ghi nhận lỗi gửi gói để giữ nhánh Logger.logException của Java ngoài lõi Player (Java: Logger.logException).
    void ReportNotificationFailure(Player player, Exception exception);
}
