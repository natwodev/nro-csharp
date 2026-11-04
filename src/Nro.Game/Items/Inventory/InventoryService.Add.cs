using System.Globalization;
using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Items.Inventory;

// Bổ sung vật phẩm vào túi, hòm và các vị trí kho đồ.
// Nguồn Java: nro/inventory/InventoryService.java (partial phụ của lớp chính; giữ thuật toán gộp ô và kiểm tra sức chứa Java.)

// Sections "Them vat pham vao danh sach", "Kiem tra dieu kien vat pham" and the gift code method of InventoryService.java.
public sealed partial class InventoryService
{
    // Java MAX_STACK_TV: the largest stack of gold bars (item 457) in one slot.
    public const int MaxStackGoldBar = 100000;

    // The largest stack of an ordinary stackable item (isUpToUp).
    public const int MaxStack = 9999;

    // ------------------------------------------------------------------ gift code

    // The three SQL statements of the Java method (SELECT, UPDATE ... LIMIT 1, INSERT) are parameterised and go through IGiftCodeRepository; the Java concatenated the code
    // the player typed into the text (plan decision D2). Behaviour kept from Java, quirks included:
    // Nothing at all happens when the code is not in the table.
    // Gold (-1) is min(gold + quantity, 2,000,000,000): a player with MORE than 2e9 gold loses the difference. Gem (-2) and ruby (-3, -10) are min(x + quantity, 200,000,000) in 32-bit arithmetic. -6 and -7 (titles) are disabled.
    // Every other key is an item id (cast to short). The item gets option as its option list (the SAME list object for all items, and AddItemList adds option 73 to it when it is empty),
    // is added to the bag, the bag is sent, the in-memory counter is told, and count_left is set to the value read at the start MINUS ONE, once per item (so a code with three items still lowers the stored count by one only).
    // Item 457 (gold bar) is added with quantity 0 and the lock option 30, so it adds nothing unless the player already has a locked stack.
    // The order of detail is the order of the Java HashMap key set, which the caller supplies.
    public async Task AddItemGiftCodeToPlayerAsync(Player p, IReadOnlyList<KeyValuePair<int, int>> detail, List<ItemOption> option, string code, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(p);
        ArgumentNullException.ThrowIfNull(detail);
        ArgumentNullException.ThrowIfNull(option);
        ArgumentNullException.ThrowIfNull(code);

        string textGift = "|7|Giftcode: " + code + "\n" + "|6|Phần Thưởng Của Bạn Là :\b";
        int? countLeft = await giftCodes.GetCountLeftAsync(code, cancellationToken);
        if (countLeft is null)
        {
            return;
        }

        foreach (KeyValuePair<int, int> entry in detail)
        {
            int idItem = entry.Key;
            int quantity = entry.Value;
            string quantityText = quantity.ToString(CultureInfo.InvariantCulture);
            switch (idItem)
            {
                case -1:
                    p.Inventory.Gold = Math.Min(p.Inventory.Gold + (long)quantity, 2_000_000_000L);
                    textGift += quantityText + " vàng\b";
                    break;
                case -2:
                    p.Inventory.Gem = Math.Min(unchecked(p.Inventory.Gem + quantity), 200_000_000);
                    textGift += quantityText + " ngọc\b";
                    break;
                case -3:
                case -10:
                    p.Inventory.Ruby = Math.Min(unchecked(p.Inventory.Ruby + quantity), 200_000_000);
                    textGift += quantityText + " ngọc khóa\b";
                    break;
                case -6:
                case -7:
                    break;
                default:
                    short templateId = unchecked((short)idItem);
                    Item itemGift = items.NewItem(templateId);
                    if (Tpl(itemGift).Id == 457 && (Tpl(itemGift).Type == 12 || (Tpl(itemGift).Type >= 0 && Tpl(itemGift).Type <= 5)))
                    {
                        itemGift.ItemOptions.Add(items.CreateItemOption(30, 0));
                    }
                    else
                    {
                        itemGift.ItemOptions = option;
                        itemGift.Quantity = quantity;
                    }

                    AddItemBag(p, itemGift);
                    SendItemBag(p);
                    textGift += "x" + quantityText + " " + Tpl(itemGift).Name + "\b";
                    giftCodeUsage.MarkUsed(unchecked((int)p.Id), code);
                    await giftCodes.SetCountLeftAsync(code, unchecked(countLeft.Value - 1), cancellationToken);
                    break;
            }
        }

        DateTime nowVietnam = time.GetUtcNow().ToOffset(VietnamTime.Offset).DateTime;
        await giftCodes.SaveUseAsync(host.GetSessionUserId(p), p.Id, code, nowVietnam, cancellationToken);
        notifier.SendAdminNotification(p, textGift);
    }

    // ------------------------------------------------------------------ add

    // Java addItemSpecial: items that are used up the moment they enter the bag (charms, food, a few boxes) instead of occupying a slot. Returns true when the item was consumed.
    private bool AddItemSpecial(Player player, Item item)
    {
        if (Tpl(item).Type == 13)
        {
            int min = 0;
            try
            {
                string? tagShopBua = host.GetOpenShopTagName(player);
                min = tagShopBua switch
                {
                    "BUA_1H" => 60,
                    "BUA_DETU" => 60,
                    "BUA_8H" => 60 * 8,
                    "BUA_1M" => 60 * 24 * 30,
                    _ => 0,
                };
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(InventoryService), e);
            }

            host.AddTimeCharms(player, Tpl(item).Id, min);
            return true;
        }

        switch (Tpl(item).Id)
        {
            case 2500:
                if (player.BillEgg == null)
                {
                    host.CreateBillEgg(player);
                }

                return true;
            case 453:
                player.HaveTennisSpaceShip = true;
                return true;
            case 74:
                host.SetFullHpMp(player);
                notifier.SendInfoHpMp(player);
                return true;
            case 191:
                host.SetFullHp(player);
                notifier.SendInfoHpMp(player);
                return true;
            case 192:
                host.SetFullMp(player);
                notifier.SendInfoHpMp(player);
                return true;
            case 516:
            case 670:
                host.SetFullHpMp(player);
                notifier.AddPotential(player, 2, random.NextInt(500000, 1000000));
                notifier.SendInfoHpMp(player);
                return true;
            case 1784:
                AddActivationSetSearchDays(player, 7);
                return true;
            case 1798:
                AddActivationSetSearchDays(player, 15);
                return true;
            case 1706:
                GiveStack(player, 220, 71, 0, 99);
                return true;
            case 1707:
                GiveStack(player, 221, 70, 0, 99);
                return true;
            case 1708:
                GiveStack(player, 222, 69, 0, 99);
                return true;
            case 1709:
                GiveStack(player, 223, 68, 0, 99);
                return true;
            case 1710:
                GiveStack(player, 224, 67, 0, 99);
                return true;
            case 1703:
                GiveStack(player, 595, 2, 512, 30);
                return true;
            case 1704:
                GiveStack(player, 1796, 2, 1024, 30);
                return true;
        }

        return false;
    }

    private void AddActivationSetSearchDays(Player player, int days)
    {
        (long TimeCreateAccount, long AccountAgeDays)? session = host.AddActivationSetSearchDays(player, days);
        if (session is { } s)
        {
            notifier.SendNotification(player, "Thời gian tìm set kích hoạt đến " + TimeUtil.GetDeadline(s.TimeCreateAccount, s.AccountAgeDays));
        }
    }

    // The "open this box to receive a stack" items of AddItemSpecial: the gift is added to the bag and the bag is sent; a full bag silently loses it (Java).
    private void GiveStack(Player player, short templateId, int optionId, int optionParam, int quantity)
    {
        Item item = items.CreateNewItem(templateId);
        item.AddOptionParam(optionId, optionParam);
        item.Quantity = quantity;
        AddItemBag(player, item);
        SendItemBag(player);
    }

    public bool AddItemBox(Player player, Item item) => AddItemList(player.Inventory.ItemsBox, item, player);

    public bool AddItemBoxCollection(Player player, Item item) => AddItemList(player.Inventory.ItemsBoxCollection, item, player);

    public bool AddItemBoxClan(Player player, Item item)
    {
        List<Item> box = ClanBox(player);
        lock (box)
        {
            return AddItemList(box, item, player);
        }
    }

    // The core of the inventory. On success the quantity of itemAdd is left at 0 (its content moved into the list: either added to an existing stack or copied into an empty slot), and the caller
    // treats Quantity == 0 as "fully moved". In order:
    // An item without options gets option 73 (so every stored item has at least one).
    // "Incremental" items (the first option is id 1 or 31, e.g. auto-training books and gift tickets): if the list already has the same template, the new parameter is ADDED to the matching option of the existing item
    // and the new item disappears (quantity 0, return true) EVEN IF the existing item has no such option (the value is then lost).
    // Gold bars (457) stack to MaxStackGoldBar per slot, locked (option 30) and unlocked bars never mix, the rest goes to empty slots, one slot per MaxStackGoldBar. If it does not all fit the
    // stacks are already filled (partial add), the player is told the bag is full and the result is false.
    // Stackable items (IsUpToUp) fill the existing stacks of the same template that hold fewer than MaxStack (but stacks of 100,000,000 or more are skipped; item 1705 has no cap),
    // the rest goes to the first empty slot as a copy. A partial fill that finds no empty slot returns false although the existing stacks have grown.
    // Everything else goes into the first empty slot as a copy; no empty slot gives false.
    // Overflow: it.Quantity += itemAdd.Quantity for item 1705 is plain 32-bit arithmetic and wraps.
    // Deviation: Java sent the gold-bar "bag full" notice to the static field Manager.player, which is always null (nothing assigns it), so the message was lost with a logged NullPointerException. Here it goes to
    // notify, the player who is adding the item (null: no notice, e.g. when a caller has no player).
    public bool AddItemList(List<Item> list, Item itemAdd, Player? notify = null)
    {
        if (itemAdd.ItemOptions.Count == 0)
        {
            itemAdd.ItemOptions.Add(items.CreateItemOption(73, 0));
        }

        (int optionId, int optionParam) = IsItemIncrementalOption(itemAdd);
        if (optionId != -1)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Item it = list[i];
                if (it.IsNotNullItem() && Tpl(it).Id == Tpl(itemAdd).Id)
                {
                    foreach (ItemOption io in it.ItemOptions)
                    {
                        if (io.OptionTemplate.Id == optionId)
                        {
                            io.Param = unchecked(io.Param + optionParam);
                        }
                    }

                    itemAdd.Quantity = 0;
                    return true;
                }
            }
        }

        if (Tpl(itemAdd).Id == 457)
        {
            bool isLocked = HasOptionTemplateId(itemAdd, 30);
            for (int i = 0; i < list.Count; i++)
            {
                Item it = list[i];
                if (it != null && it.IsNotNullItem() && Tpl(it).Id == 457)
                {
                    bool otherLocked = HasOptionTemplateId(it, 30);
                    if (isLocked == otherLocked)
                    {
                        int space = MaxStackGoldBar - it.Quantity;
                        if (space > 0)
                        {
                            int add = Math.Min(space, itemAdd.Quantity);
                            it.Quantity = unchecked(it.Quantity + add);
                            itemAdd.Quantity = unchecked(itemAdd.Quantity - add);
                            if (itemAdd.Quantity <= 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            if (itemAdd.Quantity > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Item slot = list[i];
                    if (slot == null || !slot.IsNotNullItem())
                    {
                        Item newItem = items.CopyItem(itemAdd);
                        newItem.Quantity = Math.Min(itemAdd.Quantity, MaxStackGoldBar);
                        if (isLocked && !HasOptionTemplateId(newItem, 30))
                        {
                            newItem.ItemOptions.Add(items.CreateItemOption(30, 0));
                        }

                        list[i] = newItem;
                        itemAdd.Quantity -= newItem.Quantity;
                        if (itemAdd.Quantity <= 0)
                        {
                            return true;
                        }
                    }
                }
            }

            if (notify != null)
            {
                notifier.SendNotification(notify, "Hành trang đã đầy, không thể nhận thêm Thỏi vàng");
            }

            return false;
        }

        if (Tpl(itemAdd).IsUpToUp)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Item it = list[i];
                if (!it.IsNotNullItem() || Tpl(it).Id != Tpl(itemAdd).Id || it.Quantity >= 100_000_000)
                {
                    continue;
                }

                if (Tpl(itemAdd).Id == 1705)
                {
                    it.Quantity = unchecked(it.Quantity + itemAdd.Quantity);
                    itemAdd.Quantity = 0;
                    return true;
                }

                if (it.Quantity < MaxStack)
                {
                    int add = MaxStack - it.Quantity;
                    if (itemAdd.Quantity <= add)
                    {
                        it.Quantity += itemAdd.Quantity;
                        itemAdd.Quantity = 0;
                        return true;
                    }

                    it.Quantity = MaxStack;
                    itemAdd.Quantity -= add;
                }
            }
        }

        if (itemAdd.Quantity > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Item slot = list[i];
                if (slot == null || !slot.IsNotNullItem())
                {
                    list[i] = items.CopyItem(itemAdd);
                    itemAdd.Quantity = 0;
                    return true;
                }
            }
        }

        return false;
    }

    // Java checkListsEqual: the two option lists have the same length and, position by position, the same option id and parameter.
    public static bool CheckListsEqual(List<ItemOption> list1, List<ItemOption> list2)
    {
        if (list1.Count != list2.Count)
        {
            return false;
        }

        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i].OptionTemplate.Id != list2[i].OptionTemplate.Id || list1[i].Param != list2[i].Param)
            {
                return false;
            }
        }

        return true;
    }

    // Order of the Java checks: black ball, Namek ball (both handled by the war services, whose result is returned as is), the "special" items consumed at once, money (types 9 gold, 10 gem, 34 ruby),
    // bag/box expansion items, the two gift tickets, then AddItemList on the bag. Money: gold is added only if the total stays within Inventory.LIMIT_GOLD (otherwise a notice and false);
    // gem and ruby are added in 64 bits and clamped to int.MaxValue (so the limit is 2,147,483,647 here, not Inventory.LIMIT_GEM). The money item itself keeps its quantity.
    // Item 988 never succeeds (its test goldLimit &lt; 2e12 is always false).
    public bool AddItemBag(Player player, Item item)
    {
        int templateId = Tpl(item).Id;
        if (host.IsBlackBall(templateId))
        {
            return host.PickBlackBall(player, item);
        }

        if (host.IsNamecBall(templateId) || host.IsNamecBallStone(templateId))
        {
            return host.PickNamekBall(player, item);
        }

        if (AddItemSpecial(player, item))
        {
            return true;
        }

        switch (Tpl(item).Type)
        {
            case 9:
                if (player.Inventory.Gold + item.Quantity <= player.Inventory.GetGoldLimit())
                {
                    player.Inventory.Gold += item.Quantity;
                    notifier.SendMoney(player);
                    return true;
                }

                notifier.SendNotification(player, "Vàng sau khi nhặt quá giới hạn cho phép");
                return false;
            case 10:
                long gem = (long)player.Inventory.Gem + item.Quantity;
                if (gem > int.MaxValue)
                {
                    gem = int.MaxValue;
                }

                player.Inventory.Gem = unchecked((int)gem);
                notifier.SendMoney(player);
                return true;
            case 34:
                long ruby = (long)player.Inventory.Ruby + item.Quantity;
                if (ruby > int.MaxValue)
                {
                    ruby = int.MaxValue;
                }

                player.Inventory.Ruby = unchecked((int)ruby);
                notifier.SendMoney(player);
                return true;
        }

        switch (Tpl(item).Id)
        {
            case 1627:
            case 517:
                player.Inventory.ItemsBag.Add(items.CreateItemNull());
                notifier.SendNotificationOk(player, "Hành trang của bạn đã được mở rộng thêm 1 ô");
                return true;
            case 518:
                player.Inventory.ItemsBox.Add(items.CreateItemNull());
                notifier.SendNotificationOk(player, "Rương đồ của bạn đã được mở rộng thêm 1 ô");
                return true;
            case 988:
                if (player.Inventory.GetGoldLimit() < 2_000_000_000_000L)
                {
                    notifier.SendNotification(player, "Giới hạn vàng của bạn đã tăng lên 100Tr");
                    return true;
                }

                notifier.SendNotificationOk(player, "Giới hạn vàng của bạn đã đạt tối đa");
                return false;
        }

        if (Tpl(item).Id == 718 && MergeGiftTicket(player, item, 718))
        {
            return true;
        }

        if (Tpl(item).Id == 1788 && MergeGiftTicket(player, item, 1788))
        {
            return true;
        }

        return AddItemList(player.Inventory.ItemsBag, item, player);
    }

    // The gift tickets (718 gem ticket, 1788 ruby ticket) keep their balance in option 31: a new ticket is folded into the first ticket in the bag that has the option, capped at 30,000.
    // Returns true when it was folded (the new ticket's quantity becomes 0); false when the bag has no ticket yet (the new one is added by AddItemList). The new ticket gets option 31 with 0 first.
    private bool MergeGiftTicket(Player player, Item item, int templateId)
    {
        if (!item.HaveOption(31))
        {
            item.AddOptionParam(31, 0);
        }

        List<Item> bag = player.Inventory.ItemsBag;
        for (int i = 0; i < bag.Count; i++)
        {
            Item it = bag[i];
            if (it != null && it.IsNotNullItem() && Tpl(it).Id == templateId)
            {
                foreach (ItemOption io in it.ItemOptions)
                {
                    if (io.OptionTemplate.Id == 31)
                    {
                        int total = unchecked(io.Param + item.Quantity);
                        io.Param = Math.Min(total, 30_000);
                        item.Quantity = 0;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // Java hasOptionTemplateId: whether the item has an option with that template id (option 30 is the "locked" mark).
    private static bool HasOptionTemplateId(Item item, int optionTemplateId)
    {
        if (item == null || item.ItemOptions == null)
        {
            return false;
        }

        foreach (ItemOption io in item.ItemOptions)
        {
            if (io.OptionTemplate.Id == optionTemplateId)
            {
                return true;
            }
        }

        return false;
    }

    // Java isItemIncrementalOption: the id and parameter of the first option that is 1 or 31, otherwise (-1, -1).
    private static (int OptionId, int Param) IsItemIncrementalOption(Item item)
    {
        foreach (ItemOption io in item.ItemOptions)
        {
            switch (io.OptionTemplate.Id)
            {
                case 1:
                case 31:
                    return (io.OptionTemplate.Id, io.Param);
            }
        }

        return (-1, -1);
    }

    // Java removeAndAddOptionTemplate (item option 210, "random options"): removes option 210 (its parameter is how many options to roll, default 1), then adds that many options with ids taken from a
    // shuffled list of 17 (one id each; when more are rolled than ids exist the extra ones are drawn at random with repetition) and a parameter range that depends on the id.
    // Java used Collections.shuffle and new Random() next to Util.nextInt; here everything draws from the one IRandomProvider, in the same order
    // (shuffle first, then per option: id if needed, then parameter). The shuffle is the JDK algorithm (for i = size; i &gt; 1; i--: swap(i - 1, nextInt(i))).
    private void RemoveAndAddOptionTemplate(Item item)
    {
        int[] randomIds = [50, 77, 103, 14, 5, 94, 97, 108, 95, 96, 101, 204, 80, 81, 45, 46, 197];
        int count = 1;
        ItemOption? option210 = null;
        foreach (ItemOption option in item.ItemOptions)
        {
            if (option.OptionTemplate.Id == 210)
            {
                option210 = option;
                break;
            }
        }

        if (option210 != null)
        {
            count = option210.Param;
            item.ItemOptions.Remove(option210);
        }

        List<int> availableIds = [.. randomIds];
        for (int i = availableIds.Count; i > 1; i--)
        {
            int j = randomProvider.NextInt(i);
            (availableIds[i - 1], availableIds[j]) = (availableIds[j], availableIds[i - 1]);
        }

        for (int i = 0; i < count; i++)
        {
            int newId = i < availableIds.Count ? availableIds[i] : randomIds[randomProvider.NextInt(randomIds.Length)];
            int newParam;
            switch (newId)
            {
                case 50:
                case 77:
                case 103:
                case 108:
                case 45:
                case 46:
                case 197:
                    newParam = random.NextInt(5, 10);
                    break;
                case 94:
                case 97:
                case 204:
                case 14:
                    newParam = random.NextInt(7, 18);
                    break;
                case 5:
                case 80:
                case 81:
                case 95:
                case 96:
                    newParam = random.NextInt(10, 20);
                    break;
                default:
                    newParam = random.NextInt(15, 30);
                    break;
            }

            item.ItemOptions.Add(items.CreateItemOption(newId, newParam));
        }
    }

    // Java removeAndAddOptionTemplateHSD (item option 231): removes option 231 and, with probability 90/100, adds option 93 (expiry, in days) with a value drawn from {1, 3, 5, 7, 14, 30}.
    // Draw order: the 90% test, then the day count.
    private void RemoveAndAddOptionTemplateHSD(Item item)
    {
        int[] randomIds = [1, 3, 5, 7, 14, 30];
        ItemOption? option231 = null;
        foreach (ItemOption option in item.ItemOptions)
        {
            if (option.OptionTemplate.Id == 231)
            {
                option231 = option;
                break;
            }
        }

        if (option231 != null)
        {
            item.ItemOptions.Remove(option231);
        }

        if (random.IsTrueInt(90, 100))
        {
            int newId = 93;
            int newParam = randomIds[randomProvider.NextInt(randomIds.Length)];
            item.ItemOptions.Add(items.CreateItemOption(newId, newParam));
        }
    }
}
