namespace Nro.Game.Players.Ports;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 2144–2446).
// Cổng các dịch vụ combat ngoài aggregate Player. Zone actor sở hữu luồng gọi nên không tái tạo synchronized Java ở đây.
public interface IPlayerCombatGateway
{
    // Cờ Manager.readInt dùng khi cắt CrisGH.
    bool ReadInt { get; }

    // Kiểm tra sự kiện Halloween (Java: EventManager.HALLOWEEN).
    bool IsHalloweenEvent { get; }

    // Đặt hiệu ứng Halloween qua EffectSkillService (Java: setIsHalloween).
    void SetHalloween(Player player, int outfitId, int durationMilliseconds);

    // Kiểm tra map Mabư 12 giờ qua MapService (Java: isMapMaBu12H).
    bool IsMabuTwelveHourMap(Player player);

    // Hồi HP qua PlayerService (Java: hoiPhuc).
    void RestoreHp(Player player, long hp);

    // Kiểm tra chiêu đang chọn là chưởng qua SkillUtil (Java: isUseSkillChuong).
    bool IsBlastSkill(Player player);

    // Phá khiên qua EffectSkillService (Java: breakShield).
    void BreakShield(Player player);

    // Ghi sát thương phản từ khiên vào IDMark (Java: iDMark.setDamePST).
    void SetShieldReflectDamage(Player player, long damage);

    // Kiểm tra bùa bất tử qua Charms (Java: charms.tdBatTu > System.currentTimeMillis).
    bool IsImmortalCharmActive(Player player, long nowMilliseconds);

    // Kiểm tra map giữ Mabư 128 (Java: zone.map.mapId == 128).
    bool IsMabuHoldMap(Player player);

    // Kiểm tra hai người chơi cùng bang qua Clan (Java: plAtt.clan == clan).
    bool AreSameClan(Player first, Player second);

    // Gửi chat riêng qua Service (Java: chatJustForMe).
    void SendPrivateChat(Player sender, Player receiver, string text);

    // Kiểm tra map phó bản qua MapService (Java: isMapPhoBan).
    bool IsDungeonMap(Player player);

    // Gửi thông báo qua Service (Java: sendThongBao).
    void SendNotification(Player player, string text);

    // Cộng sát thương nhận ở map 51 (Java: totalDamageTaken).
    void RecordDamageTakenOnMap51(Player player, double damage);

    // Xử lý chết tại Đại hội võ thuật 23; true khi đã xử lý (Java: The23rdMartialArtCongress.die).
    bool HandleMartialArtCongressDeath(Player player);

    // Kiểm tra cải trang Fide đại ca (Java: setClothes.ctFideDaiCa).
    bool HasFriezaLeaderCostume(Player player);

    // Kiểm tra cải trang đàn em Fide (Java: setClothes.ctDanEmFide).
    bool HasFriezaMinionCostume(Player player);

    // Lấy cấp Broly của bộ trang phục (Java: setClothes.broly).
    int GetBrolyLevel(Player player);

    // Kiểm tra điều kiện nhiệm vụ rơi đuôi khỉ (Java: playerTask.taskMain.id >= 19).
    bool CanDropMonkeyTail(Player victim, Player attacker);

    // Thả vật phẩm chiến đấu vào zone (Java: new ItemMap + Service.dropItemMap).
    void DropCombatItem(Player victim, int itemTemplateId, int quantity, Player owner);

    // Đánh dấu lastTimeDropTail cần được persistence boundary lưu; không truy cập DB trong Player (Java: ConnectDB.executeUpdate).
    void MarkTailDropTimeDirty(Player player);

    // Kiểm tra map được phép rơi vật phẩm khi chết (Java: !isHome && !isMapLuyenTap && !isMapWar).
    bool CanDropDeathItems(Player player);

    // Xử lý chết do bom ở boundary combat (Java: setBom).
    void TriggerBombDeath(Player victim, Player? attacker);

    // Xử lý chết thường ở boundary combat (Java: setDie).
    void TriggerPlayerDeath(Player victim, Player? attacker);
}
