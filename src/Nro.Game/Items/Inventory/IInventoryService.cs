namespace Nro.Game.Items.Inventory;

// Cổng khai báo nghiệp vụ kho đồ của người chơi.
// Nguồn Java: nro/inventory/InventoryService.java (interface phụ của lớp chính; giữ nguyên hợp đồng thao tác và các điều kiện Java.)

// Port of the public surface of nro/inventory/InventoryService.java (the Java singleton InventoryService.gI() becomes this injectable service). Names are the Java names with the first letter upper-cased;
// the implementation is InventoryService, which documents the quirks of each method.
// Threading (decision D1): every method is called from the tick of the zone that contains the player. The clan-box methods lock the clan's list because the clan box is shared between members.
public interface IInventoryService
{
    // ---- search ----

    // Có bình hút năng lượng (template 1911) trong túi của người chơi thật. (Java: findItemBinhHutNangLuong)
    bool HasEnergyAbsorbingBottle(Player player);

    // Java findItem(list, tempId): the first real item with that template id, or null.
    Item? FindItem(List<Item> list, int tempId);

    // Java findItemEvent(list, TypeEvent): the first real item whose template has that event type, or null.
    Item? FindItemEvent(List<Item> list, int typeEvent);

    // Kiểm tra item có option theo id. (Java: haveOption)
    bool HasOption(Item it, int idOption);

    // Java findItemByOption(player, idOption): the first bag item with that option.
    Item? FindItemByOption(Player player, int idOption);

    // Java removeItem(player, item): empties the bag slot that holds the item and sends the bag.
    void RemoveItem(Player player, Item item);

    // Java findItemBody.
    Item? FindItemBody(Player player, int tempId);

    // Java findItemBodyEvent.
    Item? FindItemBodyEvent(Player player, int typeEvent);

    // Java findItemBag.
    Item? FindItemBag(Player player, int tempId);

    // Java findItemBagEvent.
    Item? FindItemBagEvent(Player player, int typeEvent);

    // Java findItemBox.
    Item? FindItemBox(Player player, int tempId);

    // Java findItemBoxEvent.
    Item? FindItemBoxEvent(Player player, int typeEvent);

    // Java findItemBoxCollection.
    Item? FindItemBoxCollection(Player player, int tempId);

    // Java findItemBoxCollectionEvent.
    Item? FindItemBoxCollectionEvent(Player player, int typeEvent);

    // Java findItemBoxClan.
    Item? FindItemBoxClan(Player player, int tempId);

    // Kiểm tra template tồn tại trong danh sách. (Java: isExistItem)
    bool ItemExists(List<Item> list, int tempId);

    // Kiểm tra template tồn tại trên người. (Java: isExistItemBody)
    bool BodyItemExists(Player player, int tempId);

    // Kiểm tra template tồn tại trong túi. (Java: isExistItemBag)
    bool BagItemExists(Player player, int tempId);

    // Kiểm tra template tồn tại trong rương. (Java: isExistItemBox)
    bool BoxItemExists(Player player, int tempId);

    // Kiểm tra template tồn tại trong rương sưu tầm. (Java: isExistItemBoxCollection)
    bool CollectionBoxItemExists(Player player, int tempId);

    // Kiểm tra template tồn tại trong rương bang hội. (Java: isExistItemBoxClan)
    bool ClanBoxItemExists(Player player, int tempId);

    // Java findItem(player, id): the item is in the bag or the box.
    bool FindItem(Player player, int id);

    // ---- copy ----

    // Java copyList: a deep copy (every item is copied).
    List<Item> CopyList(List<Item> source);

    // Java copyItemsBody.
    List<Item> CopyItemsBody(Player player);

    // Java copyItemsBag.
    List<Item> CopyItemsBag(Player player);

    // Java copyItemsBox.
    List<Item> CopyItemsBox(Player player);

    // Java copyItemsBoxCollection.
    List<Item> CopyItemsBoxCollection(Player player);

    // Java copyItemsBoxClan.
    List<Item> CopyItemsBoxClan(Player player);

    // ---- throw and remove ----

    // Java throwItem(player, where, index): discards a worn (0) or bag (1) item.
    void ThrowItem(Player player, int where, int index);

    // Java removeItem(items, index): puts a new empty item in the slot (the old item is not disposed).
    void RemoveItem(List<Item> list, int index);

    // Java removeItem(items, item): empties the slot that holds the item (by reference) and disposes the item.
    void RemoveItem(List<Item> list, Item item);

    // Java removeItemBag(player, index).
    void RemoveItemBag(Player player, int index);

    // Java removeItemBag(player, item).
    void RemoveItemBag(Player player, Item item);

    // Java removeItemBody.
    void RemoveItemBody(Player player, int index);

    // Java removeItemPetBody: the disciple's worn slot.
    void RemoveItemPetBody(Player player, int index);

    // Java removeItemBox.
    void RemoveItemBox(Player player, int index);

    // Java removeItemBoxCollection.
    void RemoveItemBoxCollection(Player player, int index);

    // Java removeItemBoxClan.
    void RemoveItemBoxClan(Player player, int index);

    // ---- subtract quantity ----

    // Java subQuantityItemsBag.
    void SubQuantityItemsBag(Player player, Item item, int quantity);

    // Java subQuantityItemsBody.
    void SubQuantityItemsBody(Player player, Item item, int quantity);

    // Java subQuantityItemsBox.
    void SubQuantityItemsBox(Player player, Item item, int quantity);

    // Java subQuantityItemsBoxCollection.
    void SubQuantityItemsBoxCollection(Player player, Item item, int quantity);

    // Java subQuantityItemsBoxClan.
    void SubQuantityItemsBoxClan(Player player, Item item, int quantity);

    // Java subQuantityItem: subtracts from the stack and removes the item when it reaches 0.
    void SubQuantityItem(List<Item> list, Item item, int quantity);

    // ---- sort ----

    // Java sortItems: moves real items in front of empty slots by swapping (not stable).
    void SortItems(List<Item> list);

    // Dồn item có số lượng dương lên đầu và điền null ở cuối. (Java: sortItemv2)
    void CompactItemsWithPositiveQuantity(List<Item> list);

    // ---- move between places ----

    // Java itemBagToBody.
    void ItemBagToBody(Player player, int index);

    // Java itemBodyToBag.
    void ItemBodyToBag(Player player, int index);

    // Java itemBagToPetBody.
    void ItemBagToPetBody(Player player, int index);

    // Java itemPetBodyToBag.
    void ItemPetBodyToBag(Player player, int index);

    // Java itemBoxToBodyOrBag.
    void ItemBoxToBodyOrBag(Player player, int index);

    // Java itemBoxCollectionToBodyOrBag.
    void ItemBoxCollectionToBodyOrBag(Player player, int index);

    // Java itemBagToBox.
    void ItemBagToBox(Player player, int index);

    // Java itemBagToBoxCollection.
    void ItemBagToBoxCollection(Player player, int index);

    // Java itemBodyToBox.
    void ItemBodyToBox(Player player, int index);

    // Java itemBodyToBoxCollection.
    void ItemBodyToBoxCollection(Player player, int index);

    // Java sendItemBoxClanToAll: refreshes the clan box of every online member.
    void SendItemBoxClanToAll(Clan clan);

    // Java itemBoxClanToBodyOrBag.
    void ItemBoxClanToBodyOrBag(Player player, int index);

    // Java itemBagToBoxClan.
    void ItemBagToBoxClan(Player player, int index);

    // Java itemBodyToBoxClan.
    void ItemBodyToBoxClan(Player player, int index);

    // ---- packets ----

    // Java sendItemBag: sorts the bag and sends cmd -36.
    void SendItemBag(Player player);

    // Java sendItemBody: cmd -37, then the chan menh effect, the look and the flag bag.
    void SendItemBody(Player player);

    // Java sendItemBox: cmd -35 with the box, then opens the box window.
    void SendItemBox(Player player);

    // Java openBox: cmd -35 with the byte 1.
    void OpenBox(Player player);

    // Java sendItemBoxCollection.
    void SendItemBoxCollection(Player player);

    // Java openBoxCollection.
    void OpenBoxCollection(Player player);

    // Java sendItemBoxClan.
    void SendItemBoxClan(Player player);

    // Java sendItemClan: like SendItemBoxClan without opening the window.
    void SendItemClan(Player player);

    // Java openBoxClan.
    void OpenBoxClan(Player player);

    // ---- add ----

    // Java addItemBox.
    bool AddItemBox(Player player, Item item);

    // Java addItemBoxCollection.
    bool AddItemBoxCollection(Player player, Item item);

    // Java addItemBoxClan.
    bool AddItemBoxClan(Player player, Item item);

    // Java addItemList(items, itemAdd): stacks or places itemAdd in list; true when it all fitted (its quantity is then 0).
    // notify is the player who gets the "full" notice for gold bars (Java sent it to a null static field, see the implementation).
    bool AddItemList(List<Item> list, Item itemAdd, Player? notify = null);

    // Java addItemBag: the full pick-up/receive path (war balls, special items, money, expansions, tickets, then the bag).
    bool AddItemBag(Player player, Item item);

    // Java addItemGiftCodeToPlayer(p, giftcode, code): detail is giftcode.detail (key = item id or a negative money code, value = quantity, in the iteration order of the Java HashMap),
    // option is giftcode.option.
    Task AddItemGiftCodeToPlayerAsync(Player p, IReadOnlyList<KeyValuePair<int, int>> detail, List<ItemOption> option, string code, CancellationToken cancellationToken = default);

    // ---- counts and queries ----

    // Java getCountEmptyBag (a Java byte).
    sbyte GetCountEmptyBag(Player player);

    // Java getCountEmptyBody.
    sbyte GetCountEmptyBody(Player player);

    // Java getCountEmptyListItem.
    sbyte GetCountEmptyListItem(List<Item> list);

    // Java getIndexBag: the bag position of the item, or -1.
    sbyte GetIndexBag(Player pl, Item it);

    // Đúng khi người chơi KHÔNG có rương gỗ (template 570) trong túi hoặc rương. (Java: finditemWoodChest)
    bool HasNoWoodChest(Player player);

    // Lấy param của option trên item trong túi. (Java: getParam)
    int GetBagItemOptionParameter(Player player, int idoption, int itemID);

    // Trừ param option của item trong túi. (Java: subParamItemsBag)
    void SubtractBagItemOptionParameter(Player player, int itemID, int idoption, int param);

    // Có cành mai template 1185. (Java: findItemTheVoCuc)
    bool HasApricotBranch(Player player);

    // Có phiếu giảm giá thường. (Java: findItemPhieuGiamGiaThuong)
    bool HasStandardDiscountCoupon(Player player);

    // Có Pet Cerberus đang trang bị. (Java: findItemPetCeberus)
    bool HasCerberusPet(Player player);

    // Có phiếu giảm giá VIP. (Java: findItemPhieuGiamGiaVip)
    bool HasVipDiscountCoupon(Player player);

    // Có bông tai Porata cấp 2. (Java: findItemBongTaiCap2)
    bool HasLevelTwoPorataEarrings(Player player);

    // Có cải trang Quy Lão Kame. (Java: findItemSkinQuyLaoKame)
    bool HasKameElderCostume(Player player);

    // Có cải trang Drabura Frost. (Java: findItemSkinDraburaFrost)
    bool HasDraburaFrostCostume(Player player);

    // Có đủ bảy Rồng Nhí template 1872-1878 trong túi. (Java: findItemRongNhi)
    bool HasAllBabyDragons(Player player);

    // Có trang bị Namếc (isGender = 1) trên người. (Java: findItemGender)
    bool HasNamekianEquipment(Player player);

    // Có Nhẫn thời không sai lệch. (Java: findItemNTK)
    bool HasTimeSpaceDistortionRing(Player player);

    // Có cải trang Hit. (Java: findItemTVC)
    bool HasHitCostume(Player player);

    // Có tất/vớ Giáng sinh. (Java: findItemTatVoGiangSinh)
    bool HasChristmasStocking(Player player);

    // Có Vòng Kim Cô. (Java: findItemVongKimCo)
    bool HasGoldenHeadband(Player player);

    // Có đủ năm món bộ Thần Linh (level 13). (Java: fullSetThan)
    bool HasFullDivineSet(Player player);

    // Có trang bị Thần Linh. (Java: findItemThanLinh)
    bool HasDivineItem(Player player);

    // Có avatar đeo khẩu trang. (Java: findAvatarKhauTrang)
    bool HasFaceMaskAvatar(Player player);

    // Có nón Noel. (Java: findNonNoel)
    bool HasChristmasHat(Player player);

    // Có bông tuyết. (Java: findBongTuyet)
    bool HasSnowflake(Player player);

    // Có bình phép. (Java: findBinhPhep)
    bool HasMagicBottle(Player player);

    // Có giỏ đựng ngọc bí. (Java: findGioDungNgocBi)
    bool HasMysticOrbBasket(Player player);

    // Mọi thành viên bang cùng zone và có lồng đèn. (Java: areAllMembersInSameZoneAndHaveLongDenItem)
    bool AreAllMembersInSameZoneAndHaveLantern(Player player);

    // Có kiếm gỗ. (Java: findItemKiemGo)
    bool HasWoodenSword(Player player);

    // Có một chồng 99 thức ăn dùng cho Bill. (Java: x99ThucAn)
    bool HasNinetyNineBillFoodItems(Player player);

    // Đủ điều kiện mở shop Beerus. (Java: canOpenBillShop)
    bool CanOpenBeerusShop(Player player);

    // Option thuộc nhóm nâng cấp được. (Java: optionCanUpgrade)
    bool CanUpgradeOption(int id);

    // Java getIndexItem (by reference).
    int GetIndexItem(Player player, List<Item> list, Item item);

    // Java getIndexItemBag.
    int GetIndexItemBag(Player player, Item item);

    // Java getIndexItemBody.
    int GetIndexItemBody(Player player, Item item);

    // Java getIndexItemBox.
    int GetIndexItemBox(Player player, Item item);

    // Java getIndexItemBoxCollection.
    int GetIndexItemBoxCollection(Player player, Item item);

    // Java getIndexItemBoxClan.
    int GetIndexItemBoxClan(Player player, Item item);

    // ---- set bonuses ----

    // Java updateSet.
    void UpdateSet(Player player, int setOptionId, int applyId);

    // Java resetSet.
    void ResetSet(Player player, Player? pl, Player? plbox, int setOptionId, int applyId);

    // Java updateSetNew.
    void UpdateSetNew(Player player, int startId, int endId, int[] applyIds);

    // Java resetSetNew.
    void ResetSetNew(Player player, Player? pl, Player? plbox, int startId, int endId, int[] applyIds);

    // ---- collection box ----

    // Java countItemsInBoxCollection: tells the player how many items the collection box holds.
    void CountItemsInBoxCollection(Player player);

    // Phần trăm thưởng HP từ rương sưu tầm. (Java: HpItemsInBoxCollection)
    int GetCollectionBoxHealthBonusPercent(Player player);

    // Phần trăm thưởng KI từ rương sưu tầm. (Java: MpItemsInBoxCollection)
    int GetCollectionBoxManaBonusPercent(Player player);

    // Phần trăm thưởng sát thương từ rương sưu tầm. (Java: DamageItemsInBoxCollection)
    int GetCollectionBoxDamageBonusPercent(Player player);

    // Phần trăm thưởng chí mạng từ rương sưu tầm. (Java: CritItemsInBoxCollection)
    int GetCollectionBoxCriticalChanceBonusPercent(Player player);
}
