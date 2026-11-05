using Nro.Core.Templates;

namespace Nro.Game.Players.Ports;

// Cổng các gói điểm/đăng nhập/chết của Service; Game không phụ thuộc Network codec hay Service singleton.
// Nguồn Java: nro/services/Service.java (dòng 798–987, 1181–1290 trong bảng I-02).
public interface IPlayerCharacterPacketNotifier
{
    // Gửi ME_LOAD_POINT cho người chơi không phải pet/boss/new pet. (Java: point)
    void SendPlayerPoint(Player player);

    // Gửi ME_LOAD_ALL với catalog đầu/avatar đã nạp. (Java: player)
    void SendCharacterLoad(Player player, IReadOnlyList<HeadAvatar> headAvatars);

    // Gửi ME_DIE cho chính chủ và phát bản khác cho zone. (Java: charDie)
    void SendCharacterDeath(Player player);
}
