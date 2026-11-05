using Nro.Game.Items.Core;

namespace Nro.Game.Players.Ports;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 2731–3091).
// Cổng side effect khi chết/hủy Player; adapter gọi world, PVP, war và IInventoryService thay các singleton Java.
public interface IPlayerDeathGateway
{
    // Kiểm tra map chiến tranh để quyết định trừ vàng (Java: MapService.isMapWar).
    bool IsWarMap(Player player);

    // Đồng bộ tiền người chơi sau khi trừ vàng (Java: Service.sendMoney).
    void SendMoney(Player player);

    // Tạo và thả tiền theo template vào map hiện tại (Java: new ItemMap + Service.dropItemMap).
    void DropGold(Player player, int itemTemplateId, int amount);

    // Đồng bộ chỉ số sau khi gỡ hệ số hiệu ứng (Java: Service.point).
    void SendPoint(Player player);

    // Gỡ hiệu ứng kỹ năng phải xóa khi chết qua EffectSkillService đã ghép môi trường (Java: effectSkill.removeSkillEffectWhenDie).
    void RemoveSkillEffectsWhenDie(Player player);

    // Gửi trạng thái nhân vật chết (Java: Service.charDie).
    void SendCharacterDeath(Player player);

    // Đăng ký kẻ thù social của nạn nhân (Java: FriendAndEnemyService.addEnemy).
    void AddEnemy(Player victim, Player attacker);

    // Kiểm tra map không phải map 140 trước khi xử lý thua PVP (Java: zone.map.mapId != 140).
    bool CanResolvePvpDeath(Player player);

    // Xử lý người thua PVP chết (Java: pvp.lose(..., TYPE_LOSE_PVP.DEAD)).
    void ResolvePvpDeath(Player player);

    // Thả ngọc Black Ball đang giữ (Java: BlackBallWarService.dropBlackBall).
    void DropBlackBall(Player player);

    // Thả ngọc rồng Namếc đang giữ (Java: NgocRongNamec.dropNamekBall).
    void DropNamekBall(Player player);

    // Thả ngọc Namếc tranh đoạt đang cầm (Java: TranhNgocService.dropBall).
    void DropContestedNamekBall(Player player);

    // Gửi cập nhật trạng thái nhấc ngọc tranh đoạt (Java: TranhNgocService.sendUpdateLift).
    void SendContestedNamekBallLiftUpdate(Player player);

    // Trả vật phẩm giao dịch vào túi qua IInventoryService trước khi dọn (Java: InventoryService.addItemBag).
    void ReturnTradeItem(Player player, Item item);

    // Giải phóng trận PVP khi Player bị hủy (Java: pvp.dispose).
    void DisposePvp(Player player);
}
