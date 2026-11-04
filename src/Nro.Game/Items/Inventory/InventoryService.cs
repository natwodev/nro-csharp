using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Util;
using Nro.Core.Templates;
using Nro.Data.Db;
using Nro.Data.Repositories.Gift;

namespace Nro.Game.Items.Inventory;

// Điều phối các thao tác chính trên kho đồ của người chơi.
// Nguồn Java: nro/inventory/InventoryService.java (lớp chính được chuyển đổi; giữ thứ tự và điều kiện nghiệp vụ Java.)

// Port of nro/inventory/InventoryService.java (2,627 lines): find, add, remove, stack, move (bag, body, box, collection box, clan box, disciple), set-bonus bookkeeping and the three
// inventory packets (-35 box, -36 bag, -37 body). It is split over several files (InventoryService.*.cs) by the section markers the Java file had; the section marker methods
// (__________________..., empty private methods) are not ported.
// WHAT CHANGED FROM JAVA, all on purpose:
// No singleton: InventoryService.gI() is replaced by constructor injection (IInventoryService). The calls the Java class made on Service.gI(), PlayerService.gI(),
// ItemService.gI(), Client.gI(), ChangeMapService.gI() and friends go through four small seams (IInventoryItemFactory, IInventoryNotifier,
// IInventoryHost, IGiftCodeUsageTracker); the random numbers come from IRandomProvider (Java used a private new Random() and Collections.shuffle) and the clock
// from a TimeProvider.
// Safe iteration (D1): every loop that can change the list it walks is an index loop, because a List{T} enumerator throws when the list is modified (Java ArrayList.set was allowed).
// The clan box is the only list that several players (on different zone ticks) share; every operation on it holds lock (list).
// A Java NullPointerException for an item or player part that is missing becomes InvalidOperationException (see Item).
// The gift-code SQL is parameterised and asynchronous (plan decision D2, see AddItemGiftCodeToPlayerAsync).
// One bug fix: the "bag full" notice for gold bars (Thoi vang) was sent to Manager.player, a static field nobody ever assigns, so it was a logged NullPointerException and the player saw nothing. It now goes to the player who tried to add the item.
// Everything else keeps the Java behaviour, including its quirks (they are listed on the methods): SortItems is not a stable sort, AddItemList can stack part of an item and still return false,
// int arithmetic wraps, and a stack of 9999 is the cap for stackable items.
public sealed partial class InventoryService : IInventoryService
{
    private readonly IInventoryItemFactory items;
    private readonly IInventoryNotifier notifier;
    private readonly IInventoryHost host;
    private readonly RandomUtil random;
    private readonly IRandomProvider randomProvider;
    private readonly IGiftCodeRepository giftCodes;
    private readonly IGiftCodeUsageTracker giftCodeUsage;
    private readonly TimeProvider time;

    // Item factory (Java ItemService.gI()).
    // Messages and refreshes sent to the client (Java Service.gI(), PlayerService.gI()).
    // Reads and world actions that belong to other classes.
    // Source of random numbers.
    // Gift-code tables.
    // In-memory gift-code bookkeeping.
    // Clock; the system clock when omitted.
    public InventoryService(
        IInventoryItemFactory items,
        IInventoryNotifier notifier,
        IInventoryHost host,
        IRandomProvider randomProvider,
        IGiftCodeRepository giftCodes,
        IGiftCodeUsageTracker giftCodeUsage,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(notifier);
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(randomProvider);
        ArgumentNullException.ThrowIfNull(giftCodes);
        ArgumentNullException.ThrowIfNull(giftCodeUsage);
        this.items = items;
        this.notifier = notifier;
        this.host = host;
        this.randomProvider = randomProvider;
        random = new RandomUtil(randomProvider);
        this.giftCodes = giftCodes;
        this.giftCodeUsage = giftCodeUsage;
        time = timeProvider ?? TimeProvider.System;
    }

    // The template of a real item; an empty slot gives InvalidOperationException (Java: NullPointerException on item.template).
    private static ItemTemplate Tpl(Item item) =>
        item.Template ?? throw new InvalidOperationException("this item has no template (Java: NullPointerException)");

    private static Clan RequireClan(Player player) =>
        player.Clan ?? throw new InvalidOperationException("the player has no clan (Java: NullPointerException)");

    private List<Item> ClanBox(Player player) => host.GetItemsBoxClan(RequireClan(player));

    // ------------------------------------------------------------------ search

    // Có bình hút năng lượng trong túi. (Java: findItemBinhHutNangLuong)
    public bool HasEnergyAbsorbingBottle(Player player)
    {
        if (host.IsPl(player))
        {
            List<Item> bag = player.Inventory.ItemsBag;
            for (int i = 0; i < bag.Count; i++)
            {
                Item item = bag[i];
                if (item.IsNotNullItem() && Tpl(item).Id == 1911)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Java wrapped the loop in try { } catch (Exception e) { }. The only thing that could throw is a null element (only CompactItemsWithPositiveQuantity makes one), which ended the search with null; that is kept.
    public Item? FindItem(List<Item> list, int tempId)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Item item = list[i];
            if (item is null)
            {
                return null;
            }

            if (item.IsNotNullItem() && Tpl(item).Id == tempId)
            {
                return item;
            }
        }

        return null;
    }

    public Item? FindItemEvent(List<Item> list, int typeEvent)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Item item = list[i];
            if (item is null)
            {
                return null;
            }

            if (item.IsNotNullItem() && Tpl(item).TypeEvent == typeEvent)
            {
                return item;
            }
        }

        return null;
    }

    // Kiểm tra item có option theo id. (Java: haveOption)
    public bool HasOption(Item it, int idOption)
    {
        if (it != null && it.IsNotNullItem())
        {
            for (int i = 0; i < it.ItemOptions.Count; i++)
            {
                ItemOption op = it.ItemOptions[i];
                if (op != null && op.OptionTemplate.Id == idOption)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Item? FindItemByOption(Player player, int idOption)
    {
        if (player == null)
        {
            return null;
        }

        List<Item> bag = player.Inventory.ItemsBag;
        for (int i = 0; i < bag.Count; i++)
        {
            Item it = bag[i];
            if (it != null && HasOption(it, idOption))
            {
                return it;
            }
        }

        return null;
    }

    public void RemoveItem(Player player, Item item)
    {
        if (player == null || item == null)
        {
            return;
        }

        int index = IndexOfReference(player.Inventory.ItemsBag, item);
        if (index != -1)
        {
            player.Inventory.ItemsBag[index] = items.CreateItemNull();
            SendItemBag(player);
        }
    }

    public Item? FindItemBody(Player player, int tempId) => FindItem(player.Inventory.ItemsBody, tempId);

    public Item? FindItemBodyEvent(Player player, int typeEvent) => FindItemEvent(player.Inventory.ItemsBody, typeEvent);

    public Item? FindItemBag(Player player, int tempId) => FindItem(player.Inventory.ItemsBag, tempId);

    public Item? FindItemBagEvent(Player player, int typeEvent) => FindItemEvent(player.Inventory.ItemsBag, typeEvent);

    public Item? FindItemBox(Player player, int tempId) => FindItem(player.Inventory.ItemsBox, tempId);

    public Item? FindItemBoxEvent(Player player, int typeEvent) => FindItemEvent(player.Inventory.ItemsBox, typeEvent);

    public Item? FindItemBoxCollection(Player player, int tempId) => FindItem(player.Inventory.ItemsBoxCollection, tempId);

    public Item? FindItemBoxCollectionEvent(Player player, int typeEvent) => FindItemEvent(player.Inventory.ItemsBoxCollection, typeEvent);

    public Item? FindItemBoxClan(Player player, int tempId)
    {
        List<Item> box = ClanBox(player);
        lock (box)
        {
            return FindItem(box, tempId);
        }
    }

    // Kiểm tra template tồn tại trong danh sách. (Java: isExistItem)
    public bool ItemExists(List<Item> list, int tempId) => FindItem(list, tempId) != null;

    // Kiểm tra template tồn tại trên người. (Java: isExistItemBody)
    public bool BodyItemExists(Player player, int tempId) => ItemExists(player.Inventory.ItemsBody, tempId);

    // Kiểm tra template tồn tại trong túi. (Java: isExistItemBag)
    public bool BagItemExists(Player player, int tempId) => ItemExists(player.Inventory.ItemsBag, tempId);

    // Kiểm tra template tồn tại trong rương. (Java: isExistItemBox)
    public bool BoxItemExists(Player player, int tempId) => ItemExists(player.Inventory.ItemsBox, tempId);

    // Kiểm tra template tồn tại trong rương sưu tầm. (Java: isExistItemBoxCollection)
    public bool CollectionBoxItemExists(Player player, int tempId) => ItemExists(player.Inventory.ItemsBoxCollection, tempId);

    // Kiểm tra template tồn tại trong rương bang hội. (Java: isExistItemBoxClan)
    public bool ClanBoxItemExists(Player player, int tempId)
    {
        List<Item> box = ClanBox(player);
        lock (box)
        {
            return ItemExists(box, tempId);
        }
    }

    public bool FindItem(Player player, int id)
    {
        List<Item> bag = player.Inventory.ItemsBag;
        for (int i = 0; i < bag.Count; i++)
        {
            Item item = bag[i];
            if (item.IsNotNullItem() && Tpl(item).Id == id)
            {
                return true;
            }
        }

        List<Item> box = player.Inventory.ItemsBox;
        for (int i = 0; i < box.Count; i++)
        {
            Item item = box[i];
            if (item.IsNotNullItem() && Tpl(item).Id == id)
            {
                return true;
            }
        }

        return false;
    }

    // ------------------------------------------------------------------ copy

    public List<Item> CopyList(List<Item> source)
    {
        var list = new List<Item>(source.Count);
        for (int i = 0; i < source.Count; i++)
        {
            list.Add(items.CopyItem(source[i]));
        }

        return list;
    }

    public List<Item> CopyItemsBody(Player player) => CopyList(player.Inventory.ItemsBody);

    public List<Item> CopyItemsBag(Player player) => CopyList(player.Inventory.ItemsBag);

    public List<Item> CopyItemsBox(Player player) => CopyList(player.Inventory.ItemsBox);

    public List<Item> CopyItemsBoxCollection(Player player) => CopyList(player.Inventory.ItemsBoxCollection);

    public List<Item> CopyItemsBoxClan(Player player)
    {
        List<Item> box = ClanBox(player);
        lock (box)
        {
            return CopyList(box);
        }
    }

    // ------------------------------------------------------------------ throw and remove

    // where 0 is the worn equipment, 1 the bag, anything else does nothing. Java asked the player to confirm for set-bonus and expiring items through a form, but the call was commented
    // out, so both branches throw the item away at once; the only difference left is that the first branch remembers the item in TempItemIndex/ItemThrow_Drop.
    // An index outside the list throws ArgumentOutOfRangeException (Java IndexOutOfBoundsException); an empty slot throws InvalidOperationException (Java NPE).
    public void ThrowItem(Player player, int where, int index)
    {
        if (where == 0)
        {
            Item itemThrow = player.Inventory.ItemsBody[index];
            if (itemThrow != null)
            {
                if ((!itemThrow.HaveOption(93) && Tpl(itemThrow).Type == 5)
                    || itemThrow.HasActivationSet()
                    || itemThrow.HaveOption(107))
                {
                    player.TempItemIndex = index;
                    player.ItemThrow_Drop = itemThrow;
                    RemoveItemBody(player, index);
                    SendItemBody(player);
                    notifier.SendCostume(player);
                    notifier.SendNotification(player, "Đã vứt bỏ " + Tpl(itemThrow).Name);
                    return;
                }

                RemoveItemBody(player, index);
                SendItemBody(player);
                notifier.SendCostume(player);
                notifier.SendNotification(player, "Đã vứt bỏ " + Tpl(itemThrow).Name);
            }
        }
        else if (where == 1)
        {
            Item itemThrow = player.Inventory.ItemsBag[index];
            if (itemThrow != null)
            {
                if (Tpl(itemThrow).Id == 570 || Tpl(itemThrow).Id == 457)
                {
                    notifier.SendNotification(player, "Không thể vứt vật phẩm này.");
                    return;
                }

                if ((!itemThrow.HaveOption(93) && Tpl(itemThrow).Type == 5)
                    || itemThrow.HasActivationSet()
                    || itemThrow.HaveOption(107))
                {
                    player.TempItemIndex = index;
                    player.ItemThrow_Drop = itemThrow;
                    RemoveItemBag(player, index);
                    SortItems(player.Inventory.ItemsBag);
                    SendItemBag(player);
                    notifier.SendNotification(player, "Đã vứt bỏ " + Tpl(itemThrow).Name);
                    return;
                }

                RemoveItemBag(player, index);
                SortItems(player.Inventory.ItemsBag);
                SendItemBag(player);
                notifier.SendNotification(player, "Đã vứt bỏ " + Tpl(itemThrow).Name);
            }
        }
    }

    // The slot gets a new empty item; the removed item is NOT disposed (callers still read it afterwards, e.g. ThrowItem prints its name).
    public void RemoveItem(List<Item> list, int index) => list[index] = items.CreateItemNull();

    // Matches by reference. The first slot that holds item gets a new empty item and item is disposed (its template becomes null). Does nothing when the item is not in the list.
    public void RemoveItem(List<Item> list, Item item)
    {
        if (item == null)
        {
            return;
        }

        Item empty = items.CreateItemNull();
        for (int i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], item))
            {
                list[i] = empty;
                item.Dispose();
                break;
            }
        }
    }

    public void RemoveItemBag(Player player, int index) => RemoveItem(player.Inventory.ItemsBag, index);

    public void RemoveItemBag(Player player, Item item) => RemoveItem(player.Inventory.ItemsBag, item);

    public void RemoveItemBody(Player player, int index) => RemoveItem(player.Inventory.ItemsBody, index);

    public void RemoveItemPetBody(Player player, int index)
    {
        Player pet = player.Pet ?? throw new InvalidOperationException("the player has no disciple (Java: NullPointerException)");
        RemoveItemBody(pet, index);
    }

    public void RemoveItemBox(Player player, int index) => RemoveItem(player.Inventory.ItemsBox, index);

    public void RemoveItemBoxCollection(Player player, int index) => RemoveItem(player.Inventory.ItemsBoxCollection, index);

    public void RemoveItemBoxClan(Player player, int index)
    {
        List<Item> box = ClanBox(player);
        lock (box)
        {
            RemoveItem(box, index);
        }
    }

    // ------------------------------------------------------------------ subtract quantity

    public void SubQuantityItemsBag(Player player, Item item, int quantity) => SubQuantityItem(player.Inventory.ItemsBag, item, quantity);

    public void SubQuantityItemsBody(Player player, Item item, int quantity) => SubQuantityItem(player.Inventory.ItemsBody, item, quantity);

    public void SubQuantityItemsBox(Player player, Item item, int quantity) => SubQuantityItem(player.Inventory.ItemsBox, item, quantity);

    public void SubQuantityItemsBoxCollection(Player player, Item item, int quantity) => SubQuantityItem(player.Inventory.ItemsBoxCollection, item, quantity);

    public void SubQuantityItemsBoxClan(Player player, Item item, int quantity)
    {
        List<Item> box = ClanBox(player);
        lock (box)
        {
            SubQuantityItem(box, item, quantity);
        }
    }

    // Matches by reference. quantity is a plain int subtraction (no overflow check, a negative quantity adds). When the stack drops to 0 or below the item is removed
    // and disposed (RemoveItem(List{Item}, Item)). The method does not check that enough items are there: the caller does.
    public void SubQuantityItem(List<Item> list, Item item, int quantity)
    {
        if (item == null)
        {
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            Item it = list[i];
            if (ReferenceEquals(item, it))
            {
                it.Quantity = unchecked(it.Quantity - quantity);
                if (it.Quantity <= 0)
                {
                    RemoveItem(list, item);
                }

                break;
            }
        }
    }

    // ------------------------------------------------------------------ sort

    // NOT a stable sort, and not "sorted" in any visible sense: it swaps the first empty slot with the last real item, and repeats until every real item is before every empty slot. The relative order of the items
    // that move changes (an item taken from the end lands in the first gap). Java did this recursively; the loop gives the same result without a recursion depth equal to the number of gaps.
    public void SortItems(List<Item> list)
    {
        while (true)
        {
            int first = -1;
            int last = -1;
            Item? tempFirst = null;
            Item? tempLast = null;
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].IsNotNullItem())
                {
                    first = i;
                    tempFirst = list[i];
                    break;
                }
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].IsNotNullItem())
                {
                    last = i;
                    tempLast = list[i];
                    break;
                }
            }

            if (first != -1 && last != -1 && first < last)
            {
                list[first] = tempLast!;
                list[last] = tempFirst!;
            }
            else
            {
                return;
            }
        }
    }

    // Compacts the items with Quantity &gt; 0 to the front, keeping their order, and fills the rest with null (not with empty items), which every other method here does not expect: nothing in the
    // Java source calls this method, and it is ported only for completeness. Index loop: the Java for-each that calls set is not allowed on a List{T}.
    // Dồn item có số lượng dương lên đầu và điền null ở cuối. (Java: sortItemv2)
    public void CompactItemsWithPositiveQuantity(List<Item> list)
    {
        int index = 0;
        for (int i = 0; i < list.Count; i++)
        {
            Item item = list[i];
            if (item != null && item.Quantity > 0)
            {
                list[index] = item;
                index++;
            }
        }

        for (int i = index; i < list.Count; i++)
        {
            list[i] = null!;
        }
    }

    private static int IndexOfReference(List<Item> list, Item item)
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
}
