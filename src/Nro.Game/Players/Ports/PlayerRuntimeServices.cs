using Nro.Game.Items.Core;

namespace Nro.Game.Players.Ports;

// Base adapter cho IPlayerRuntimeServices; production override các side effect đã ghép, test chỉ override nhánh cần kiểm.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 861–1458).
public abstract class PlayerRuntimeServices : IPlayerRuntimeServices
{
    // Giờ game mặc định, adapter production phải override khi dùng Hirudegarn (Java: Calendar.HOUR_OF_DAY).
    public virtual int CurrentHour => 0;
    // Mặc định predicate map là false cho adapter chưa ghép (Java: MapService).
    public virtual bool IsHomeMap(int mapId) => false;
    // Mặc định predicate map là false cho adapter chưa ghép (Java: MapService.isMapBlackBallWar).
    public virtual bool IsBlackBallWarMap(int mapId) => false;
    // Mặc định predicate map là false cho adapter chưa ghép (Java: MapService.isMapWar).
    public virtual bool IsWarMap(int mapId) => false;
    // Mặc định predicate map là false cho adapter chưa ghép (Java: MapService.isMapOffline).
    public virtual bool IsOfflineMap(int mapId) => false;
    // Mặc định predicate map là false cho adapter chưa ghép (Java: MapService.isMapMabu14H).
    public virtual bool IsMabuFourteenHourMap(int mapId) => false;
    // Không làm gì khi MagicTree chưa ghép (Java: magicTree.update).
    public virtual void UpdateMagicTree(Player player) { }
    // Không làm gì khi ItemTime host chưa ghép (Java: itemTime.update).
    public virtual void UpdateItemTime(Player player) { }
    // Không làm gì khi NPoint host chưa ghép (Java: nPoint.update).
    public virtual void UpdateNPoint(Player player) { }
    // Không làm gì khi EffectSkill environment chưa ghép (Java: effectSkill.update).
    public virtual void UpdateEffectSkill(Player player) { }
    // Không làm gì khi EggMob runtime chưa ghép (Java: DeTrung.update).
    public virtual void UpdateEggMob(Player player) { }
    // Không làm gì khi EffectSkin environment chưa ghép (Java: effectSkin.update).
    public virtual void UpdateEffectSkin(Player player) { }
    // Không làm gì khi TaskService chưa ghép (Java: TaskService.sendTaskMain).
    public virtual void AdvanceStarterTask(Player player) { }
    // Mặc định không phải nhiệm vụ đầu game khi TaskService chưa ghép (Java: TaskService.getIdTask).
    public virtual bool IsStarterTask(Player player) => false;
    // Không làm gì khi Client chưa ghép (Java: Client.kickSession).
    public virtual void Disconnect(Player player) { }
    // Mặc định không ngắt khi IDMark chưa ghép (Java: iDMark.isBan).
    public virtual bool ShouldDisconnectBannedPlayer(Player player, long nowMilliseconds) => false;
    // Không làm gì khi PhanThan chưa ghép (Java: PhanThan.update).
    public virtual void UpdateBodyClone(Player player) { }
    // Không làm gì khi Saibamen chưa ghép (Java: Saibamen.update).
    public virtual void UpdateSaiBaMen(Player player) { }
    // Không làm gì khi Fusion chưa ghép (Java: fusion.update).
    public virtual void UpdateFusion(Player player) { }
    // Không làm gì khi Detu chưa ghép (Java: Detu.update).
    public virtual void UpdatePet(Player player) { }
    // Không làm gì khi PetFollow chưa ghép (Java: PetFollow.update).
    public virtual void UpdatePetFollow(Player player) { }
    // Không làm gì khi Duongtang chưa ghép (Java: Duongtang.update).
    public virtual void UpdateTangMonk(Player player) { }
    // Không làm gì khi Satellite chưa ghép (Java: satellite.update).
    public virtual void UpdateSatellite(Player player) { }
    // Không làm gì khi DropItem chưa ghép (Java: dropItem.update).
    public virtual void UpdateDropItems(Player player) { }
    // Không làm gì khi MabuEgg chưa ghép (Java: mabuEgg.sendMabuEgg).
    public virtual void SendMabuEgg(Player player) { }
    // Không làm gì khi MelonPlant chưa ghép (Java: duahau.sendDuaHau).
    public virtual void SendWatermelon(Player player) { }
    // Không làm gì khi Achievement service chưa ghép (Java: achievement.done).
    public virtual void AdvanceDedicatedAchievement(Player player, int milliseconds) { }
    // Không làm gì khi EffectSkin gateway chưa ghép (Java: effectSkin xHPKI/xDame).
    public virtual void NormalizeEffectSkinBonuses(Player player, bool isBlackBallWarMap) { }
    // Không làm gì khi BlackBall gateway chưa ghép (Java: fixBlackBallWar).
    public virtual void FixBlackBallWar(Player player) { }
    // Không làm gì khi SuperRank chưa ghép (Java: superRank.reward).
    public virtual void RewardSuperRank(Player player) { }
    // Mặc định chưa qua nửa đêm khi SuperRank chưa ghép (Java: Util.isAfterMidnight).
    public virtual bool IsAfterMidnightForSuperRank(Player player, long nowMilliseconds) => false;
    // Không làm gì khi hồi sinh service chưa ghép (Java: Service.hsChar).
    public virtual void AutoRespawn(Player player) { }
    // Không làm gì khi notifier chưa ghép (Java: Service.sendThongBao).
    public virtual void SendPunchingMachineScore(Player player, long score) { }
    // Không làm gì khi ItemService chưa ghép (Java: ItemService.loadItemTimeStatus).
    public virtual void LoadItemTimeStatus(Player player, Item item) { }
    // Mặc định vật phẩm chưa quá hạn khi ItemService chưa ghép (Java: ItemService.isOutOfDateTime).
    public virtual bool IsOutOfDateTime(Item item) => false;
    // Không làm gì khi InventoryService chưa ghép (Java: InventoryService.removeItem).
    public virtual void RemoveExpiredItem(Player player, Item item) { }
    // Không làm gì khi ChangeMapService chưa ghép (Java: changeMapNonSpaceship).
    public virtual void LeaveHirudegarn(Player player, int x, int y) { }
    // Không làm gì khi PlayerService chưa ghép (Java: hoiPhuc).
    public virtual void RestoreSuperDivineWater(Player player) { }
    // Mặc định không dùng hồi phục Siêu Cấp khi ItemTime mapping chưa ghép (Java: isUseHoiSieuCap).
    public virtual bool IsUsingSuperRegen(Player player) => false;
    // Không làm gì khi NguHanhSon chưa ghép (Java: NguHanhSon.update).
    public virtual void UpdateNguHanhSon(Player player) { }
    // Không làm gì khi TaskService chưa ghép (Java: sendUpdateCountSubTask).
    public virtual void SendSubTaskUpdate(Player player) { }
    // Không làm gì khi BadgesTaskService chưa ghép (Java: updateDoneTask).
    public virtual void UpdateBadgeTasks(Player player) { }
    // Không làm gì khi daily gift notifier chưa ghép (Java: sendTextTimeDaiLyGift).
    public virtual void SendDailyGiftReminders(Player player, long nowMilliseconds) { }
    // Không làm gì khi TranhNgoc chưa ghép (Java: TranhNgoc.update).
    public virtual void UpdateNamekWar(Player player) { }
    // Không làm gì khi ClanService chưa ghép (Java: checkDoneTaskJoinClan).
    public virtual void CheckClanTask(Player player) { }
    // Không làm gì khi Clan chưa ghép (Java: CongTiemNangSucManhToanBangHoi).
    public virtual void ResetExpiredClanExperienceBuff(Player player, long nowMilliseconds) { }
    // Không làm gì khi persistence boundary chưa ghép (Java: updatePlayerLastTimeLoginGame).
    public virtual void UpdateLastLogin(Player player) { }
    // Mặc định chưa có Bình Phép khi InventoryService chưa ghép (Java: findBinhPhep).
    public virtual bool HasMagicBottle(Player player) => false;
    // Không làm gì khi InventoryService chưa ghép (Java: subQuantityItemsBag).
    public virtual void ReleaseGhostSoul(Player player, Item ghostSoul) { }
    // Không làm gì khi BossNomalService chưa ghép (Java: CheckDonateWater_AutoUpdated).
    public virtual void UpdateAutomaticWaterDonation(Player player) { }
    // Không làm gì khi SuperDivineWater chưa ghép (Java: SuperDivineWater.update).
    public virtual void UpdateSuperDivineWater(Player player) { }
    // Không làm gì khi MajinBuu12H chưa ghép (Java: MajinBuu12H.update).
    public virtual void UpdateMabuTwelveHour(Player player) { }
    // Không làm gì khi QuaKhu chưa ghép (Java: QuaKhu.update).
    public virtual void UpdatePastEvent(Player player) { }
    // Không làm gì khi chat notifier chưa ghép (Java: Service.chat).
    public virtual void SendCoughChat(Player player) { }
    // Mặc định chưa hoàn tất SnakeWay khi dungeon chưa ghép (Java: allMobsDead).
    public virtual bool IsSnakeWayCompleted(Player player) => false;
    // Không làm gì khi ChangeMapService chưa ghép (Java: changeMapYardrat).
    public virtual void MoveToSnakeWayExit(Player player, int x, int y) { }
    // Mặc định không có hạng Whis khi manager chưa ghép (Java: getPlayerWhis).
    public virtual int GetWhisRank(Player player) => 0;
    // Không làm gì khi effect notifier chưa ghép (Java: addEffectChar).
    public virtual void AddWhisEffect(Player player, int effectId) { }
    // Không làm gì khi EffectSkillService chưa ghép (Java: setIsMonkeyTrungThu).
    public virtual void ActivateMoonFestivalMonkey(Player player) { }
    // Không làm gì khi EffectSkillService chưa ghép (Java: removeMabuHold).
    public virtual void RemoveMabuHold(Player player) { }
    // Không làm gì khi death gateway chưa ghép (Java: setDie).
    public virtual void SetDie(Player player) { }
    // Không làm gì khi Mabư 14 giờ chưa ghép (Java: isPhuHoMapMabu).
    public virtual void UpdateMabuFourteenHourProtection(Player player, bool isMabuFourteenHourMap) { }
    // Không làm gì khi PlayerService chưa ghép (Java: sendInfoHp).
    public virtual void SendInfoHp(Player player) { }
    // Không làm gì khi PlayerService chưa ghép (Java: sendInfoMp).
    public virtual void SendInfoMp(Player player) { }
    // Không làm gì khi Service chưa ghép (Java: sendBadgesPlayer).
    public virtual void SendBadge(Player player, int badgeId) { }
    // Không làm gì khi Service chưa ghép (Java: point).
    public virtual void SendPoint(Player player) { }
    // Mặc định không gần nhau khi location gateway chưa ghép (Java: Util.getDistance).
    public virtual int GetDistance(Player first, Player second) => int.MaxValue;
    // Mặc định không cùng bang khi Clan chưa ghép (Java: clan.equals).
    public virtual bool AreSameClan(Player first, Player second) => false;
    // Mặc định không có buff bang khi NPoint clan chưa ghép (Java: tlHpClan/tlDameClan/tlMpClan).
    public virtual void GetClanBonuses(Player player, out int hp, out int damage, out int mp) => (hp, damage, mp) = (0, 0, 0);
    // Mặc định không có Ông già Noel khi boss catalog chưa ghép (Java: BossID.ONG_GIA_NOEL).
    public virtual bool IsSantaBoss(Player player) => false;
    // Không làm gì khi NpcService chưa ghép (Java: createMenuConMeo).
    public virtual void OpenSantaReindeerMenu(Player player) { }
    // Không làm gì khi boss notifier chưa ghép (Java: sendThongBao).
    public virtual void SendBossEventNotification(Player player, string text) { }
    // Không làm gì khi IDMark chưa ghép (Java: iDMark).
    public virtual void UpdateTravelMarkers(Player player, long nowMilliseconds, IPlayerRandom random) { }
    // Không làm gì khi TrapMap chưa ghép (Java: trap.doPlayer).
    public virtual void ApplyTrap(Player player) { }
    // Không làm gì khi DetuService chưa ghép (Java: PetFollow).
    public virtual void SynchronizePetFollow(Player player) { }
    // Không làm gì khi costume effect service chưa ghép (Java: cumberdown/paindown/kefladown).
    public virtual void UpdateTransformationCostume(Player player) { }
    // Không làm gì khi EffectSkillService chưa ghép (Java: setChibi).
    public virtual void SetChibi(Player player, int durationMilliseconds) { }
    // Không làm gì khi EffectSkillService chưa ghép (Java: removeChibi).
    public virtual void RemoveChibi(Player player) { }
    // Không làm gì khi chat notifier chưa ghép (Java: Service.chat).
    public virtual void SendMabuHelpChat(Player player) { }
    // Không làm gì khi logger chưa ghép (Java: Logger.logException).
    public virtual void ReportUpdateFailure(Player player, Exception exception) { }
}
