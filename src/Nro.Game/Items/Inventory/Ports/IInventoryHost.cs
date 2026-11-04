namespace Nro.Game.Items.Inventory.Ports;

// Cổng truy cập dữ liệu người chơi và tác động thế giới mà InventoryService cần từ các mô-đun khác.
// Nguồn Java: nro/inventory/InventoryService.java (interface phụ của lớp chính; chạy trong tick zone và giữ các quy ước tham chiếu sống của Java).

// The reads and the world actions InventoryService needs from classes that do not exist yet or belong to other tracks (Player logic, NPoint, Fusion,
// Charms, IDMark, Clan, Client, ChangeMapService, the black-ball and Namek-ball wars, DatabaseUpdater). The Java class reached all of them through
// public fields and singletons; with this seam the inventory logic is plain list manipulation and can be tested with a fake.
// Threading (decision D1): called from the tick of the zone that contains the player; the methods that look at ANOTHER player
// (GetPlayerById) return a live reference and the caller only reads fields of it.
public interface IInventoryHost
{
    // ---- reads about a player ----

    // Java player.isPl(): a real, human-controlled character (not a pet, boss, bot or clone).
    bool IsPl(Player player);

    // Java player.getHead() (the head sprite after cải trang, transformations and fusion).
    short GetHead(Player player);

    // Java ((Detu) player).master. pet must be a disciple; any other player gives InvalidCastException, as the Java cast threw
    // ClassCastException (the caller relies on this in one rare path, see InventoryService.PutItemBody).
    Player GetPetMaster(Player pet);

    // Java player.nPoint.power.
    long GetPower(Player player);

    // Java player.fusion.typeFusion; compare with ConstPlayer.NON_FUSION.
    sbyte GetTypeFusion(Player player);

    // Java player.iDMark.getShopOpen().tagName, or null when the player has no shop open.
    string? GetOpenShopTagName(Player player);

    // Java player.getSession().userId: the account id of the player's session.
    // The player has no session (Java: NullPointerException).
    int GetSessionUserId(Player player);

    // ---- effects on a player ----

    // Java player.charms.addTimeCharms(itemId, min).
    void AddTimeCharms(Player player, int itemId, int minutes);

    // Java BillEgg.createBillEgg(player).
    void CreateBillEgg(Player player);

    // Java player.nPoint.setFullHpMp().
    void SetFullHpMp(Player player);

    // Java player.nPoint.setFullHp().
    void SetFullHp(Player player);

    // Java player.nPoint.setFullMp().
    void SetFullMp(Player player);

    // The three Java statements DatabaseUpdater.addDaysToTimeUpSkhByPlayerId(player.id, days), then, if the player has a session,
    // DatabaseUpdater.refreshAccountAgeDays(session). Returns the session's timeCreateAcount and accountAgeDays after the refresh (the caller prints the
    // deadline), or null when the player has no session.
    (long TimeCreateAccount, long AccountAgeDays)? AddActivationSetSearchDays(Player player, int days);

    // ---- world ----

    // Java Client.gI().getPlayerByID(id): the online player, or null.
    Player? GetPlayerById(long playerId);

    // Java ChangeMapService.gI().exitMap(player).
    void ExitMap(Player player);

    // Java player.dispose() (release a player object that is leaving the game).
    void DisposePlayer(Player player);

    // Java ItemMapService.gI().isBlackBall(tempId).
    bool IsBlackBall(int tempId);

    // Java ItemMapService.gI().isNamecBall(tempId).
    bool IsNamecBall(int tempId);

    // Java ItemMapService.gI().isNamecBallStone(tempId).
    bool IsNamecBallStone(int tempId);

    // Java BlackBallWarService.gI().pickBlackBall(player, item).
    bool PickBlackBall(Player player, Item item);

    // Java NgocRongNamec.gI().pickNamekBall(player, item).
    bool PickNamekBall(Player player, Item item);

    // ---- clan ----

    // Java clan.itemsBoxClan: the clan's shared box (live list; slots are never removed, only replaced).
    List<Item> GetItemsBoxClan(Clan clan);

    // Java clan.getMembers(), reduced to the member ids (a snapshot, safe to iterate while members join or leave).
    IReadOnlyList<long> GetClanMemberIds(Clan clan);

    // Java clan.updateItemsBoxClanToSQL(clan).
    void UpdateItemsBoxClanToSql(Clan clan);
}
