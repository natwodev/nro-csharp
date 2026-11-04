using Nro.Core.IO;

namespace Nro.Game.Effects.Ports;

// Phát gói tin đã hoàn chỉnh đến client, tách mã hiệu ứng khỏi socket và session; ISession.Send sao chép payload khi gọi nên bên gọi được Cleanup Message ngay sau đó.
// Nguồn Java: nro/effect/EffectSkillService.java và nro/effect/EffectSkinService.java (cổng C# thay các lệnh Service.sendMessAll* và Player.sendMessage; triển khai không được ném lỗi khi người chơi là bot, đệ tử hoặc đã mất session vì Java bỏ qua im lặng).
public interface IEffectBroadcaster
{
    // Gửi gói chỉ cho người chơi này (Java: player.sendMessage).
    void SendToPlayer(Player player, Message message);

    // Gửi gói cho mọi người chơi trong zone của người chơi, gồm chính họ (Java: Service.sendMessAllPlayerInMap).
    void SendToZoneOf(Player player, Message message);

    // Gửi gói cho mọi người chơi trong zone chỉ định (Java: Service.sendMessAllPlayerInMap).
    void SendToZone(Zone zone, Message message);

    // Gửi gói cho mọi người chơi trong zone của quái (Java: Service.sendMessAllPlayerInMap).
    void SendToZoneOfMob(IEffectMobTarget mob, Message message);

    // Gửi gói cho zone của người chơi nhưng loại chính người đó (Java: Service.sendMessAnotherNotMeInMap).
    void SendToZoneExcept(Player player, Message message);

    // Gửi gói cho toàn bộ người chơi đang trực tuyến (Java: Service.sendMessAllPlayer).
    void SendToAll(Message message);
}
