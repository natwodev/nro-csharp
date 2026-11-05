using Nro.Game.Items.Core;

namespace Nro.Game.Players.Ports;

// Cổng các service runtime còn lại của vòng đời Player; Zone sở hữu luồng gọi, không tạo Service singleton.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 861–1458).
public interface IPlayerRuntimeServices
{
    // Lấy giờ địa phương của game để giới hạn map Hirudegarn (Java: Calendar.HOUR_OF_DAY).
    int CurrentHour { get; }

    // Kiểm tra map nhà (Java: MapService.isHome).
    bool IsHomeMap(int mapId);

    // Kiểm tra map chiến tranh ngọc rồng đen (Java: MapService.isMapBlackBallWar).
    bool IsBlackBallWarMap(int mapId);

    // Kiểm tra map chiến tranh (Java: MapService.isMapWar).
    bool IsWarMap(int mapId);

    // Kiểm tra map ngoại tuyến (Java: MapService.isMapOffline).
    bool IsOfflineMap(int mapId);

    // Kiểm tra map Mabư 14 giờ (Java: MapService.isMapMabu14H).
    bool IsMabuFourteenHourMap(int mapId);

    // Đẩy component MagicTree chưa có Update C# hoàn chỉnh (Java: magicTree.update).
    void UpdateMagicTree(Player player);

    // Đẩy trạng thái thời hạn vật phẩm (Java: itemTime.update).
    void UpdateItemTime(Player player);

    // Đẩy chỉ số người chơi (Java: nPoint.update).
    void UpdateNPoint(Player player);

    // Đẩy hiệu ứng kỹ năng (Java: effectSkill.update).
    void UpdateEffectSkill(Player player);

    // Đẩy Đệ Trứng (Java: DeTrung.update).
    void UpdateEggMob(Player player);

    // Đẩy hiệu ứng skin (Java: effectSkin.update).
    void UpdateEffectSkin(Player player);

    // Chuyển chỉ số nhiệm vụ đầu game và gửi lại nhiệm vụ (Java: playerTask.taskMain.index, TaskService.sendTaskMain).
    void AdvanceStarterTask(Player player);

    // Kiểm tra nhiệm vụ đầu game 0_0 hoặc 0_1 (Java: TaskService.getIdTask).
    bool IsStarterTask(Player player);

    // Ngắt phiên khi player bị cấm hoặc bất hoạt (Java: Client.kickSession).
    void Disconnect(Player player);

    // Kiểm tra cấm đã qua năm giây (Java: iDMark.isBan, Util.canDoWithTime).
    bool ShouldDisconnectBannedPlayer(Player player, long nowMilliseconds);

    // Đẩy Phân Thân chưa có vòng đời riêng (Java: PhanThan.update).
    void UpdateBodyClone(Player player);

    // Đẩy Sai Ba Men chưa có vòng đời riêng (Java: Saibamen.update).
    void UpdateSaiBaMen(Player player);

    // Đẩy hợp thể chưa có vòng đời riêng (Java: fusion.update).
    void UpdateFusion(Player player);

    // Đẩy thú cưng đệ tử chưa có vòng đời riêng (Java: Detu.update).
    void UpdatePet(Player player);

    // Đẩy thú cưng đi theo chưa có vòng đời riêng (Java: PetFollow.update).
    void UpdatePetFollow(Player player);

    // Đẩy Đường Tăng chưa có vòng đời riêng (Java: Duongtang.update).
    void UpdateTangMonk(Player player);

    // Đẩy vệ tinh chưa có vòng đời riêng (Java: satellite.update).
    void UpdateSatellite(Player player);

    // Đẩy trạng thái vật phẩm rơi chưa có vòng đời riêng (Java: dropItem.update).
    void UpdateDropItems(Player player);

    // Gửi trứng Mabư ở map nhà (Java: mabuEgg.sendMabuEgg).
    void SendMabuEgg(Player player);

    // Gửi cây dưa hấu ở map nhà (Java: duahau.sendDuaHau).
    void SendWatermelon(Player player);

    // Cộng tiến độ thành tựu chuyên cần (Java: achievement.done(HOAT_DONG_CHAM_CHI, 1000)).
    void AdvanceDedicatedAchievement(Player player, int milliseconds);

    // Chuẩn hóa hệ số hiệu ứng skin ngoài chiến tranh (Java: effectSkin.xHPKI/xDame, nPoint.calPoint, Service.point).
    void NormalizeEffectSkinBonuses(Player player, bool isBlackBallWarMap);

    // Sửa trạng thái chiến tranh ngọc rồng đen (Java: fixBlackBallWar).
    void FixBlackBallWar(Player player);

    // Trao thưởng Siêu Hạng sau nửa đêm (Java: superRank.reward).
    void RewardSuperRank(Player player);

    // Kiểm tra mốc thưởng Siêu Hạng đã qua nửa đêm (Java: Util.isAfterMidnight).
    bool IsAfterMidnightForSuperRank(Player player, long nowMilliseconds);

    // Hồi sinh tự động (Java: Service.hsChar).
    void AutoRespawn(Player player);

    // Thông báo điểm máy đấm đã tính (Java: Service.sendThongBao).
    void SendPunchingMachineScore(Player player, long score);

    // Nạp trạng thái thời hạn của vật phẩm (Java: ItemService.loadItemTimeStatus).
    void LoadItemTimeStatus(Player player, Item item);

    // Kiểm tra vật phẩm quá hạn (Java: ItemService.isOutOfDateTime).
    bool IsOutOfDateTime(Item item);

    // Xóa vật phẩm quá hạn và gửi thông báo (Java: InventoryService.removeItem, Service.sendThongBao).
    void RemoveExpiredItem(Player player, Item item);

    // Đưa người chơi rời Hirudegarn ngoài giờ cho phép (Java: ChangeMapService.changeMapNonSpaceship).
    void LeaveHirudegarn(Player player, int x, int y);

    // Hồi phục bằng Siêu Thần Thủy (Java: PlayerService.hoiPhuc).
    void RestoreSuperDivineWater(Player player);

    // Kiểm tra đang dùng hồi phục Siêu Cấp (Java: itemTime.isUseHoiSieuCap).
    bool IsUsingSuperRegen(Player player);

    // Cập nhật Ngũ Hành Sơn (Java: NguHanhSon.update).
    void UpdateNguHanhSon(Player player);

    // Đồng bộ đếm nhiệm vụ phụ (Java: TaskService.sendUpdateCountSubTask).
    void SendSubTaskUpdate(Player player);

    // Cập nhật nhiệm vụ huy hiệu (Java: BadgesTaskService.updateDoneTask).
    void UpdateBadgeTasks(Player player);

    // Gửi nhắc quà hằng ngày (Java: sendTextTimeDaiLyGift).
    void SendDailyGiftReminders(Player player, long nowMilliseconds);

    // Cập nhật Tranh Ngọc (Java: TranhNgoc.update).
    void UpdateNamekWar(Player player);

    // Đếm nhiệm vụ bang hội (Java: ClanService.checkDoneTaskJoinClan).
    void CheckClanTask(Player player);

    // Hoàn tất hệ số buff kinh nghiệm bang hội (Java: clan.CongTiemNangSucManhToanBangHoi = 1).
    void ResetExpiredClanExperienceBuff(Player player, long nowMilliseconds);

    // Ghi nhận đăng nhập còn hoạt động (Java: PlayerService.updatePlayerLastTimeLoginGame).
    void UpdateLastLogin(Player player);

    // Kiểm tra người chơi có Bình Phép (Java: InventoryService.findBinhPhep).
    bool HasMagicBottle(Player player);

    // Tiêu thụ Hồn ma Lích Tên và gửi túi/thông báo (Java: InventoryService.subQuantityItemsBag/sendItemBag, Service.sendThongBao).
    void ReleaseGhostSoul(Player player, Item ghostSoul);

    // Tự cập nhật quyên góp nước boss thường (Java: BossNomalService.CheckDonateWater_AutoUpdated).
    void UpdateAutomaticWaterDonation(Player player);

    // Cập nhật Siêu Thần Thủy (Java: SuperDivineWater.update).
    void UpdateSuperDivineWater(Player player);

    // Cập nhật Mabư 12 giờ (Java: MajinBuu12H.update).
    void UpdateMabuTwelveHour(Player player);

    // Cập nhật sự kiện Quá Khứ (Java: QuaKhu.update).
    void UpdatePastEvent(Player player);

    // Gửi chat Khụ khụ (Java: Service.chat).
    void SendCoughChat(Player player);

    // Kiểm tra bang/dungeon Con Đường Rắn Độc đã đủ điều kiện chuyển map (Java: clan.ConDuongRanDoc.allMobsDead).
    bool IsSnakeWayCompleted(Player player);

    // Chuyển người chơi sang map 144 của Con Đường Rắn Độc (Java: ChangeMapService.changeMapYardrat).
    void MoveToSnakeWayExit(Player player, int x, int y);

    // Lấy thứ hạng Whis đủ điều kiện hiệu ứng; trả 0 nếu danh sách Java chưa đạt 5 (Java: TopKillWhisManager.getList, getPlayerWhis).
    int GetWhisRank(Player player);

    // Thêm hiệu ứng Whis cho hạng 1–3 (Java: Service.addEffectChar).
    void AddWhisEffect(Player player, int effectId);

    // Bật hiệu ứng Đuôi Khỉ Trung Thu và đồng bộ các gói liên quan (Java: EffectSkillService.setIsMonkeyTrungThu, Service/PlayerService).
    void ActivateMoonFestivalMonkey(Player player);

    // Gỡ giữ Mabư (Java: EffectSkillService.removeMabuHold).
    void RemoveMabuHold(Player player);

    // Gây chết sau khi hết HP vì giữ Mabư (Java: setDie).
    void SetDie(Player player);

    // Gỡ bảo hộ map Mabư 14 giờ khi rời map (Java: isPhuHoMapMabu, nPoint.calPoint, Service.point).
    void UpdateMabuFourteenHourProtection(Player player, bool isMabuFourteenHourMap);

    // Đồng bộ HP hiện tại (Java: PlayerService.sendInfoHp).
    void SendInfoHp(Player player);

    // Đồng bộ MP hiện tại (Java: PlayerService.sendInfoMp).
    void SendInfoMp(Player player);

    // Đồng bộ huy hiệu đang đeo (Java: Service.sendBadgesPlayer).
    void SendBadge(Player player, int badgeId);

    // Đồng bộ chỉ số người chơi (Java: Service.point).
    void SendPoint(Player player);

    // Lấy khoảng cách giữa hai người chơi (Java: Util.getDistance).
    int GetDistance(Player first, Player second);

    // Kiểm tra hai người chơi cùng bang (Java: clan.equals).
    bool AreSameClan(Player first, Player second);

    // Lấy ba hệ số cộng bang từ NPoint (Java: nPoint.tlHpClan/tlDameClan/tlMpClan).
    void GetClanBonuses(Player player, out int hp, out int damage, out int mp);

    // Kiểm tra Boss Ông già Noel (Java: bossPlayer.id == BossID.ONG_GIA_NOEL).
    bool IsSantaBoss(Player player);

    // Mở menu giao tuần lộc của Ông già Noel (Java: NpcService.createMenuConMeo).
    void OpenSantaReindeerMenu(Player player);

    // Gửi thông báo sự kiện boss (Java: Service.sendThongBao).
    void SendBossEventNotification(Player player, string text);

    // Cập nhật các cờ IDMark/chuyển map/thời gian bất hoạt chưa có kiểu C# (Java: iDMark và location.lastTimeplayerMove).
    void UpdateTravelMarkers(Player player, long nowMilliseconds, IPlayerRandom random);

    // Áp dụng bẫy map cho người chơi (Java: TrapMap.doPlayer).
    void ApplyTrap(Player player);

    // Đồng bộ tạo hoặc hủy PetFollow theo ô body 7 (Java: DetuService.PetFollow, PetFollow.dispose).
    void SynchronizePetFollow(Player player);

    // Gỡ cải trang biến hình không còn mặc (Java: EffectSkillService.cumberdown*/paindown/kefladown).
    void UpdateTransformationCostume(Player player);

    // Đặt Chibi sáu trăm giây (Java: EffectSkillService.setChibi).
    void SetChibi(Player player, int durationMilliseconds);

    // Gỡ Chibi tại map cấm (Java: EffectSkillService.removeChibi).
    void RemoveChibi(Player player);

    // Chat cầu cứu khi bị giữ Mabư (Java: Service.chat).
    void SendMabuHelpChat(Player player);

    // Báo lỗi update ở boundary thay Logger singleton (Java: Logger.logException).
    void ReportUpdateFailure(Player player, Exception exception);
}
