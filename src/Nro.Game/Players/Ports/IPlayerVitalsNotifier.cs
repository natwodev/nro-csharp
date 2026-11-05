namespace Nro.Game.Players.Ports;

// Cổng gói HP/MP và tiền của PlayerService, tách khỏi các thông báo DailyGift ngoài phạm vi I-01.
// Nguồn Java: nro/services/PlayerService.java (dòng 114–177).
public interface IPlayerVitalsNotifier
{
    // Gửi HP hiện tại bằng sub-command 5 (Java: sendInfoHp).
    void SendCurrentHp(Player player, long hp);

    // Gửi MP hiện tại bằng sub-command 6 (Java: sendInfoMp).
    void SendCurrentMp(Player player, long mp);

    // Gửi tiền, HP và MP hiện tại bằng sub-command 4 (Java: sendInfoHpMpMoney).
    void SendInfoHpMpMoney(Player player);
}
