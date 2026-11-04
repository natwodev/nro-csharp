namespace Nro.Game.Items.Inventory;

// Truy vấn ô vật phẩm, số lượng và điều kiện sử dụng trong kho đồ.
// Nguồn Java: nro/inventory/InventoryService.java (partial phụ của lớp chính; giữ quy ước null, chỉ số và thứ tự duyệt Java.)

// Sections "Kiem tra danh sach con cho trong" and the lookups of InventoryService.java, set bonuses and collection box bonuses.
public sealed partial class InventoryService
{
    // ------------------------------------------------------------------ empty slots and indexes

    public sbyte GetCountEmptyBag(Player player) => GetCountEmptyListItem(player.Inventory.ItemsBag);

    public sbyte GetCountEmptyBody(Player player) => GetCountEmptyListItem(player.Inventory.ItemsBody);

    // Returns an sbyte like the Java byte: the count wraps to -128 at the 128th empty slot (a bag is 100 slots plus a few expansions, so it does not happen in practice).
    public sbyte GetCountEmptyListItem(List<Item> list)
    {
        sbyte count = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].IsNotNullItem())
            {
                count = unchecked((sbyte)(count + 1));
            }
        }

        return count;
    }

    // The position of it (by reference) in the bag, or -1. The Java loop variable was a byte, which cannot reach a bag size above 127; the same limit is kept (a larger bag throws ArgumentOutOfRangeException once the index wraps negative).
    public sbyte GetIndexBag(Player pl, Item it)
    {
        List<Item> bag = pl.Inventory.ItemsBag;
        for (sbyte i = 0; i < bag.Count; i = unchecked((sbyte)(i + 1)))
        {
            Item item = bag[i];
            if (item != null && ReferenceEquals(it, item))
            {
                return i;
            }
        }

        return -1;
    }

    public int GetIndexItem(Player player, List<Item> list, Item item)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], item))
            {
                return i;
            }
        }

        return -1;
    }

    public int GetIndexItemBag(Player player, Item item) => GetIndexItem(player, player.Inventory.ItemsBag, item);

    public int GetIndexItemBody(Player player, Item item) => GetIndexItem(player, player.Inventory.ItemsBody, item);

    public int GetIndexItemBox(Player player, Item item) => GetIndexItem(player, player.Inventory.ItemsBox, item);

    public int GetIndexItemBoxCollection(Player player, Item item) => GetIndexItem(player, player.Inventory.ItemsBoxCollection, item);

    public int GetIndexItemBoxClan(Player player, Item item)
    {
        List<Item> box = ClanBox(player);
        lock (box)
        {
            return GetIndexItem(player, box, item);
        }
    }

    // ------------------------------------------------------------------ option parameters

    // The parameter of option idoption on the FIRST bag item with template itemID that has it, 0 if none. (Java kept scanning other items of the same template if the first had no such option.)
    // Lấy param option của item trong túi. (Java: getParam)
    public int GetBagItemOptionParameter(Player player, int idoption, int itemID)
    {
        List<Item> bag = player.Inventory.ItemsBag;
        for (int i = 0; i < bag.Count; i++)
        {
            Item it = bag[i];
            if (it != null && it.ItemOptions != null && it.IsNotNullItem() && Tpl(it).Id == itemID)
            {
                foreach (ItemOption iop in it.ItemOptions)
                {
                    if (iop.OptionTemplate.Id == idoption)
                    {
                        return iop.Param;
                    }
                }
            }
        }

        return 0;
    }

    // Looks at the first bag item with template itemID only (the loop stops there even if it has no such option), subtracts param from option idoption and removes the
    // item (and disposes it) when the option drops to 0 or below. Note it dereferences the template of every bag item it passes: an empty slot before the match throws InvalidOperationException (Java NPE) -
    // the bag is kept packed by SortItems, so real items come first.
    // Trừ param option của item trong túi. (Java: subParamItemsBag)
    public void SubtractBagItemOptionParameter(Player player, int itemID, int idoption, int param)
    {
        Item? itemRemove = null;
        List<Item> bag = player.Inventory.ItemsBag;
        for (int i = 0; i < bag.Count; i++)
        {
            Item it = bag[i];
            if (it != null && Tpl(it).Id == itemID)
            {
                foreach (ItemOption op in it.ItemOptions)
                {
                    if (op != null && op.OptionTemplate.Id == idoption)
                    {
                        op.Param = unchecked(op.Param - param);
                        if (op.Param <= 0)
                        {
                            itemRemove = it;
                        }

                        break;
                    }
                }

                break;
            }
        }

        if (itemRemove != null)
        {
            RemoveItem(player.Inventory.ItemsBag, itemRemove);
        }
    }

    // Option thuộc nhóm nâng cấp được. (Java: optionCanUpgrade)
    public bool CanUpgradeOption(int id) => id == 0 || id == 22 || id == 23 || id == 14 || id == 27 || id == 28 || id == 47;

    // ------------------------------------------------------------------ the "has item X" family

    private static bool AnyWithId(List<Item> list, int templateId)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Item item = list[i];
            if (item.IsNotNullItem() && Tpl(item).Id == templateId)
            {
                return true;
            }
        }

        return false;
    }

    private static bool AnyWithIdIn(List<Item> list, int fromId, int toId)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Item item = list[i];
            if (item.IsNotNullItem() && Tpl(item).Id >= fromId && Tpl(item).Id <= toId)
            {
                return true;
            }
        }

        return false;
    }

    // Đúng khi người chơi không có rương gỗ. (Java: finditemWoodChest)
    public bool HasNoWoodChest(Player player) =>
        !AnyWithId(player.Inventory.ItemsBag, 570) && !AnyWithId(player.Inventory.ItemsBox, 570);

    // Có cành mai template 1185. (Java: findItemTheVoCuc)
    public bool HasApricotBranch(Player player) =>
        host.IsPl(player) && (AnyWithId(player.Inventory.ItemsBag, 1185) || AnyWithId(player.Inventory.ItemsBox, 1185));

    // Có phiếu giảm giá thường. (Java: findItemPhieuGiamGiaThuong)
    public bool HasStandardDiscountCoupon(Player player) => AnyWithId(player.Inventory.ItemsBag, 459);

    // Có Pet Cerberus đang trang bị. (Java: findItemPetCeberus)
    public bool HasCerberusPet(Player player) => AnyWithId(player.Inventory.ItemsBody, 1654);

    // Có phiếu giảm giá VIP. (Java: findItemPhieuGiamGiaVip)
    public bool HasVipDiscountCoupon(Player player) => AnyWithId(player.Inventory.ItemsBag, 721);

    // Có bông tai Porata cấp 2. (Java: findItemBongTaiCap2)
    public bool HasLevelTwoPorataEarrings(Player player) => AnyWithId(player.Inventory.ItemsBag, 921) || AnyWithId(player.Inventory.ItemsBox, 921);

    // Có cải trang Quy Lão Kame. (Java: findItemSkinQuyLaoKame)
    public bool HasKameElderCostume(Player player) =>
        AnyWithId(player.Inventory.ItemsBody, 710) || AnyWithId(player.Inventory.ItemsBag, 710) || AnyWithId(player.Inventory.ItemsBox, 710);

    // Có cải trang Drabura Frost. (Java: findItemSkinDraburaFrost)
    public bool HasDraburaFrostCostume(Player player) => AnyWithId(player.Inventory.ItemsBody, 1205);

    // Có đủ bảy Rồng Nhí trong túi. (Java: findItemRongNhi)
    public bool HasAllBabyDragons(Player player)
    {
        short[] needed = [1872, 1873, 1874, 1875, 1876, 1877, 1878];
        var found = new HashSet<short>();
        List<Item> bag = player.Inventory.ItemsBag;
        for (int i = 0; i < bag.Count; i++)
        {
            Item item = bag[i];
            if (item.IsNotNullItem() && Array.IndexOf(needed, Tpl(item).Id) >= 0)
            {
                found.Add(Tpl(item).Id);
            }
        }

        return found.Count == needed.Length;
    }

    // Có trang bị Namếc (isGender = 1) trên người. (Java: findItemGender)
    public bool HasNamekianEquipment(Player player)
    {
        List<Item> body = player.Inventory.ItemsBody;
        for (int i = 0; i < body.Count; i++)
        {
            Item item = body[i];
            if (item != null && item.IsNotNullItem() && Tpl(item).IsGender == 1)
            {
                return true;
            }
        }

        return false;
    }

    // Có Nhẫn thời không sai lệch. (Java: findItemNTK)
    public bool HasTimeSpaceDistortionRing(Player player) =>
        host.IsPl(player) && (AnyWithId(player.Inventory.ItemsBag, 992) || AnyWithId(player.Inventory.ItemsBox, 992));

    // Có cải trang Hit. (Java: findItemTVC)
    public bool HasHitCostume(Player player) =>
        host.IsPl(player) && (AnyWithId(player.Inventory.ItemsBag, 2077) || AnyWithId(player.Inventory.ItemsBox, 2077));

    // Có tất/vớ Giáng sinh. (Java: findItemTatVoGiangSinh)
    public bool HasChristmasStocking(Player player) => AnyWithId(player.Inventory.ItemsBag, 649);

    // Có Vòng Kim Cô. (Java: findItemVongKimCo)
    public bool HasGoldenHeadband(Player player) => AnyWithId(player.Inventory.ItemsBag, 543);

    // Có đủ năm món bộ Thần Linh. (Java: fullSetThan)
    public bool HasFullDivineSet(Player player)
    {
        for (int i = 0; i < 5; i++)
        {
            Item item = player.Inventory.ItemsBody[i];
            if (item == null || item.Template == null || item.Template.Level != 13)
            {
                return false;
            }
        }

        return true;
    }

    // Có trang bị Thần Linh. (Java: findItemThanLinh)
    public bool HasDivineItem(Player player) => AnyWithIdIn(player.Inventory.ItemsBody, 555, 567);

    // Có avatar đeo khẩu trang. (Java: findAvatarKhauTrang)
    public bool HasFaceMaskAvatar(Player player) => AnyWithIdIn(player.Inventory.ItemsBody, 761, 763);

    // Có nón Noel. (Java: findNonNoel)
    public bool HasChristmasHat(Player player) => AnyWithIdIn(player.Inventory.ItemsBody, 386, 394);

    // Có bông tuyết. (Java: findBongTuyet)
    public bool HasSnowflake(Player player) => AnyWithId(player.Inventory.ItemsBody, 745);

    // Có bình phép. (Java: findBinhPhep)
    public bool HasMagicBottle(Player player) => player != null && AnyWithId(player.Inventory.ItemsBag, 1259);

    // Có giỏ đựng ngọc bí. (Java: findGioDungNgocBi)
    public bool HasMysticOrbBasket(Player player) => AnyWithId(player.Inventory.ItemsBag, 1348);

    // Có kiếm gỗ. (Java: findItemKiemGo)
    public bool HasWoodenSword(Player player) => AnyWithId(player.Inventory.ItemsBag, 1304);

    // Có một chồng 99 thức ăn dùng cho Bill. (Java: x99ThucAn)
    public bool HasNinetyNineBillFoodItems(Player player)
    {
        List<Item> bag = player.Inventory.ItemsBag;
        for (int i = 0; i < bag.Count; i++)
        {
            Item it = bag[i];
            if (it != null && it.Template != null && it.Template.Id >= 663 && it.Template.Id <= 667 && it.Quantity >= 99)
            {
                return true;
            }
        }

        return false;
    }

    // Đủ điều kiện mở shop Beerus. (Java: canOpenBillShop)
    public bool CanOpenBeerusShop(Player player) => HasFullDivineSet(player) && HasNinetyNineBillFoodItems(player);

    // True when the player has a clan, the clan has members, and EVERY member is online, in the same zone as player and wears one of the lantern ("long den") items on the body. The members' inventories
    // belong to other zones' ticks, so they are only read, with index loops that cannot throw while another thread replaces a slot (D1); the answer can be a moment stale.
    // Mọi thành viên bang cùng zone và có lồng đèn. (Java: areAllMembersInSameZoneAndHaveLongDenItem)
    public bool AreAllMembersInSameZoneAndHaveLantern(Player player)
    {
        if (player == null || player.Zone == null || player.Clan == null)
        {
            return false;
        }

        Zone currentZone = player.Zone;
        IReadOnlyList<long> memberIds = host.GetClanMemberIds(player.Clan);
        if (memberIds.Count == 0)
        {
            return false;
        }

        int[] lanternTemplateIds = [467, 468, 469, 470, 471, 800, 801, 802, 803, 804, 1047, 1303, 1675];
        for (int m = 0; m < memberIds.Count; m++)
        {
            Player? member = host.GetPlayerById(memberIds[m]);
            if (member == null || member.Zone == null || !member.Zone.Equals(currentZone))
            {
                return false;
            }

            bool hasLantern = false;
            List<Item> body = member.Inventory.ItemsBody;
            for (int i = 0; i < body.Count && !hasLantern; i++)
            {
                Item item = body[i];
                if (item != null && item.IsNotNullItem())
                {
                    hasLantern = Array.IndexOf(lanternTemplateIds, (int)Tpl(item).Id) >= 0;
                }
            }

            if (!hasLantern)
            {
                return false;
            }
        }

        return true;
    }

    // ------------------------------------------------------------------ set bonuses

    // Counts the worn items that carry option setOptionId (once per item); with exactly 5 of them the bonus option applyId of every worn item is SET to 1, with any other
    // count it is set to 0 (Item.GetOptionParam(id, param) is a setter that does nothing for an item that lacks the option). The test is == 5, not &gt;= 5.
    public void UpdateSet(Player player, int setOptionId, int applyId)
    {
        List<Item> body = player.Inventory.ItemsBody;
        int count = 0;
        for (int i = 0; i < body.Count; i++)
        {
            Item item = body[i];
            if (item == null || !item.IsNotNullItem())
            {
                continue;
            }

            foreach (ItemOption io in item.ItemOptions)
            {
                if (io.OptionTemplate.Id == setOptionId)
                {
                    count++;
                    break;
                }
            }
        }

        for (int i = 0; i < body.Count; i++)
        {
            Item item = body[i];
            if (item == null || !item.IsNotNullItem())
            {
                continue;
            }

            item.GetOptionParam(applyId, count == 5 ? 1 : 0);
        }
    }

    // After an item left the body: when fewer than 5 pieces of the set remain on player, zeroes the bonus option on every worn item; ALWAYS zeroes it on every item of the player's bag, of
    // pl's bag and of plbox's box (so a set piece carried away never keeps a live bonus). pl and plbox may be null.
    public void ResetSet(Player player, Player? pl, Player? plbox, int setOptionId, int applyId)
    {
        List<Item> body = player.Inventory.ItemsBody;
        int count = 0;
        for (int i = 0; i < body.Count; i++)
        {
            Item item = body[i];
            if (item == null || !item.IsNotNullItem())
            {
                continue;
            }

            foreach (ItemOption io in item.ItemOptions)
            {
                if (io.OptionTemplate.Id == setOptionId)
                {
                    count++;
                    break;
                }
            }
        }

        for (int i = 0; i < body.Count; i++)
        {
            Item item = body[i];
            if (item == null || !item.IsNotNullItem())
            {
                continue;
            }

            if (count < 5)
            {
                item.GetOptionParam(applyId, 0);
            }
        }

        ZeroOption(player.Inventory.ItemsBag, applyId);
        if (pl != null)
        {
            ZeroOption(pl.Inventory.ItemsBag, applyId);
        }

        if (plbox != null)
        {
            ZeroOption(plbox.Inventory.ItemsBox, applyId);
        }
    }

    private static void ZeroOption(List<Item> list, int applyId)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Item item = list[i];
            if (item == null || !item.IsNotNullItem())
            {
                continue;
            }

            item.GetOptionParam(applyId, 0);
        }
    }

    private static int CountSetPieces(List<Item> body, int startId, int endId)
    {
        int count = 0;
        for (int i = 0; i < body.Count; i++)
        {
            Item item = body[i];
            if (item == null || !item.IsNotNullItem())
            {
                continue;
            }

            foreach (ItemOption io in item.ItemOptions)
            {
                if (io.OptionTemplate.Id >= startId && io.OptionTemplate.Id <= endId)
                {
                    count++;
                    break;
                }
            }
        }

        return count;
    }

    // The "new" sets: pieces are the worn items with any option id in [startId, endId]; the three bonus options are set to 1 from 2, 4 and 5 pieces, otherwise 0.
    public void UpdateSetNew(Player player, int startId, int endId, int[] applyIds)
    {
        List<Item> body = player.Inventory.ItemsBody;
        int count = CountSetPieces(body, startId, endId);
        for (int i = 0; i < body.Count; i++)
        {
            Item item = body[i];
            if (item == null || !item.IsNotNullItem())
            {
                continue;
            }

            item.GetOptionParam(applyIds[0], count >= 2 ? 1 : 0);
            item.GetOptionParam(applyIds[1], count >= 4 ? 1 : 0);
            item.GetOptionParam(applyIds[2], count >= 5 ? 1 : 0);
        }
    }

    // Counterpart of ResetSet for the "new" sets: bonuses whose threshold (2, 4, 5) is no longer met are zeroed on the body; all three are zeroed on the bags and the box.
    public void ResetSetNew(Player player, Player? pl, Player? plbox, int startId, int endId, int[] applyIds)
    {
        List<Item> body = player.Inventory.ItemsBody;
        int count = CountSetPieces(body, startId, endId);
        for (int i = 0; i < body.Count; i++)
        {
            Item item = body[i];
            if (item == null || !item.IsNotNullItem())
            {
                continue;
            }

            if (count < 2)
            {
                item.GetOptionParam(applyIds[0], 0);
            }

            if (count < 4)
            {
                item.GetOptionParam(applyIds[1], 0);
            }

            if (count < 5)
            {
                item.GetOptionParam(applyIds[2], 0);
            }
        }

        foreach (int id in applyIds)
        {
            ZeroOption(player.Inventory.ItemsBag, id);
        }

        if (pl != null)
        {
            foreach (int id in applyIds)
            {
                ZeroOption(pl.Inventory.ItemsBag, id);
            }
        }

        if (plbox != null)
        {
            foreach (int id in applyIds)
            {
                ZeroOption(plbox.Inventory.ItemsBox, id);
            }
        }
    }

    // ------------------------------------------------------------------ collection box bonuses

    private static int CountBoxCollection(Player player)
    {
        int count = 0;
        List<Item> box = player.Inventory.ItemsBoxCollection;
        for (int i = 0; i < box.Count; i++)
        {
            Item item = box[i];
            if (item != null && item.IsNotNullItem())
            {
                count++;
            }
        }

        return count;
    }

    public void CountItemsInBoxCollection(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        notifier.SendNotification(player, "Trong rương sưu tầm hiện có " + CountBoxCollection(player) + " vật phẩm");
    }

    // Phần trăm thưởng HP từ rương sưu tầm. (Java: HpItemsInBoxCollection)
    public int GetCollectionBoxHealthBonusPercent(Player player)
    {
        if (player == null)
        {
            return 0;
        }

        int count = CountBoxCollection(player);
        if (count >= 120)
        {
            return 30;
        }

        if (count >= 100)
        {
            return 20;
        }

        if (count >= 80)
        {
            return 15;
        }

        if (count >= 60)
        {
            return 10;
        }

        if (count >= 40)
        {
            return 5;
        }

        if (count >= 30)
        {
            return 4;
        }

        if (count >= 20)
        {
            return 2;
        }

        if (count >= 10)
        {
            return 1;
        }

        return 0;
    }

    // Phần trăm thưởng KI từ rương sưu tầm. (Java: MpItemsInBoxCollection)
    public int GetCollectionBoxManaBonusPercent(Player player)
    {
        if (player == null)
        {
            return 0;
        }

        int count = CountBoxCollection(player);
        if (count >= 120)
        {
            return 35;
        }

        if (count >= 100)
        {
            return 30;
        }

        if (count >= 80)
        {
            return 24;
        }

        if (count >= 60)
        {
            return 18;
        }

        if (count >= 50)
        {
            return 12;
        }

        if (count >= 35)
        {
            return 7;
        }

        if (count >= 20)
        {
            return 4;
        }

        if (count >= 10)
        {
            return 2;
        }

        if (count >= 5)
        {
            return 1;
        }

        return 0;
    }

    // Phần trăm thưởng sát thương từ rương sưu tầm. (Java: DamageItemsInBoxCollection)
    public int GetCollectionBoxDamageBonusPercent(Player player)
    {
        if (player == null)
        {
            return 0;
        }

        int count = CountBoxCollection(player);
        if (count >= 120)
        {
            return 25;
        }

        if (count >= 100)
        {
            return 18;
        }

        if (count >= 90)
        {
            return 15;
        }

        if (count >= 70)
        {
            return 7;
        }

        if (count >= 50)
        {
            return 5;
        }

        if (count >= 30)
        {
            return 2;
        }

        if (count >= 15)
        {
            return 1;
        }

        return 0;
    }

    // Phần trăm thưởng chí mạng từ rương sưu tầm. (Java: CritItemsInBoxCollection)
    public int GetCollectionBoxCriticalChanceBonusPercent(Player player)
    {
        if (player == null)
        {
            return 0;
        }

        int count = CountBoxCollection(player);
        if (count >= 120)
        {
            return 5;
        }

        if (count >= 100)
        {
            return 4;
        }

        if (count >= 70)
        {
            return 3;
        }

        if (count >= 40)
        {
            return 2;
        }

        if (count >= 20)
        {
            return 1;
        }

        return 0;
    }
}
