using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Consts;

namespace Nro.Game.Items.Inventory;

// Trang bị, tháo và xử lý tác động của vật phẩm trên nhân vật.
// Nguồn Java: nro/inventory/InventoryService.java (partial phụ của lớp chính; giữ thứ tự cập nhật chỉ số và ngoại lệ nghiệp vụ Java.)

// Section "Thao tac thao mac item" of InventoryService.java: moving items between body, bag, boxes, clan box and the disciple.
public sealed partial class InventoryService
{
    // The set-bonus pairs Java listed (in this order) after every equip or unequip: (option that marks a piece of the set, option that carries the bonus).
    private static readonly (int SetOptionId, int ApplyId)[] SetPairs =
    [
        (127, 139), (128, 140), (129, 141), (130, 142), (131, 143), (132, 144), (133, 136), (134, 137), (135, 138),
        (233, 234), (250, 253), (251, 254), (252, 255), (263, 264), (265, 266), (267, 268),
    ];

    // The "new" set groups: the range of option ids that mark a piece, and the three bonus options (2, 4 and 5 pieces).
    private static readonly (int Start, int End, int[] Apply)[] SetNewGroups =
    [
        (241, 244, [242, 243, 244]),
        (237, 240, [238, 239, 240]),
        (245, 248, [246, 247, 248]),
    ];

    // The 16 updateSet and 3 updateSetNew calls Java repeated after each equip.
    private void UpdateAllSets(Player player)
    {
        foreach ((int setOptionId, int applyId) in SetPairs)
        {
            UpdateSet(player, setOptionId, applyId);
        }

        foreach ((int start, int end, int[] apply) in SetNewGroups)
        {
            UpdateSetNew(player, start, end, apply);
        }
    }

    // The 16 resetSet and 3 resetSetNew calls Java repeated after each unequip.
    private void ResetAllSets(Player player, Player? pl, Player? plbox)
    {
        foreach ((int setOptionId, int applyId) in SetPairs)
        {
            ResetSet(player, pl, plbox, setOptionId, applyId);
        }

        foreach ((int start, int end, int[] apply) in SetNewGroups)
        {
            ResetSetNew(player, pl, plbox, start, end, apply);
        }
    }

    private Item PutItemBag(Player player, Item item) => PutItemList(player.Inventory.ItemsBag, item);

    private Item PutItemBox(Player player, Item item) => PutItemList(player.Inventory.ItemsBox, item);

    private Item PutItemBoxCollection(Player player, Item item) => PutItemList(player.Inventory.ItemsBoxCollection, item);

    private Item PutItemBoxClan(Player player, Item item)
    {
        List<Item> box = ClanBox(player);
        lock (box)
        {
            return PutItemList(box, item);
        }
    }

    // The four Java putItemBag/Box/BoxCollection/BoxClan: puts item in the first empty slot and returns a new empty item (what the source slot now holds);
    // when the list has no empty slot returns item itself (the source slot keeps it).
    private Item PutItemList(List<Item> list, Item item)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].IsNotNullItem())
            {
                list[i] = item;
                return items.CreateItemNull();
            }
        }

        return item;
    }

    // Who gets the "cannot wear" notices: a disciple's master, otherwise the player.
    private Player NoticeTarget(Player player) => player.IsDeTu ? host.GetPetMaster(player) : player;

    // Java putItemBody: tries to wear item; returns the item that left the slot (to put in the source slot) or item itself when it could not be worn
    // (a notice has been sent). Equipment types 0-5 go to the slot of the same number; 32 to 6, 21 to 7, 11 to 8, 23 and 24 to 9, 35 to 10, 39 to 11. Types 70-75 pass the first check and then
    // have no slot (index -1), so itemsBody[-1] throws ArgumentOutOfRangeException exactly as Java threw IndexOutOfBoundsException.
    private Item PutItemBody(Player player, Item item)
    {
        Item sItem = item;
        if (!item.IsNotNullItem())
        {
            return sItem;
        }

        switch (Tpl(item).Type)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 32:
            case 23:
            case 24:
            case 11:
            case 21:
            case 35:
            case 70:
            case 71:
            case 72:
            case 73:
            case 74:
            case 75:
            case 39:
                break;
            default:
                notifier.SendNotificationOk(NoticeTarget(player), "Trang bị không phù hợp!");
                return sItem;
        }

        if (Tpl(item).Gender < 3 && Tpl(item).Gender != player.Gender)
        {
            notifier.SendNotificationOk(NoticeTarget(player), "Trang bị không phù hợp!");
            return sItem;
        }

        long powerRequire = Tpl(item).StrRequire;
        foreach (ItemOption io in item.ItemOptions)
        {
            if (io.OptionTemplate.Id == 21)
            {
                powerRequire = io.Param * 1_000_000_000L;
                break;
            }
        }

        if (host.GetPower(player) < powerRequire)
        {
            notifier.SendNotificationOk(NoticeTarget(player), "Sức mạnh không đủ yêu cầu!");
            return sItem;
        }

        if (item.IsItemTemplateToPlayer() && player.IsDeTu)
        {
            notifier.SendNotification(NoticeTarget(player), "Không thể cho đệ tử mặc Cải Trang này");
            return sItem;
        }

        if (item.HaveOption(38) && host.GetTypeFusion(player) == ConstPlayer.NON_FUSION)
        {
            notifier.SendNotification(NoticeTarget(player), player.IsDeTu ? "Không thể cho đệ tử sử dụng" : "Chỉ khi hợp thể mới sử dụng được");
            return sItem;
        }

        if (item.IsItemTemplateToPet() && !player.IsDeTu)
        {
            notifier.SendNotification(player, "Chỉ sử dụng được cho đệ tử");
            return sItem;
        }

        int index = -1;
        switch (Tpl(item).Type)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                index = Tpl(item).Type;
                break;
            case 32:
                index = 6;
                break;
            case 21:
                index = 7;
                break;
            case 11:
                index = 8;
                break;
            case 23:
            case 24:
                index = 9;
                break;
            case 35:
                index = 10;
                break;
            case 39:
                index = 11;
                break;
        }

        List<Item> body = player.Inventory.ItemsBody;
        if (index >= body.Count)
        {
            // Java sent both messages to ((Detu) player).master, so a non-disciple here throws ClassCastException; GetPetMaster keeps that.
            if (player.IsDeTu)
            {
                notifier.SendNotification(host.GetPetMaster(player), "Đệ tử không thể sử dụng vật phẩm này !");
            }
            else
            {
                notifier.SendNotification(host.GetPetMaster(player), "Không thể sử dụng vật phẩm này !");
            }

            return sItem;
        }

        sItem = body[index];
        if (index == 8 && sItem.IsNotNullItem())
        {
            notifier.RemovePlayerEffect(player, Tpl(sItem).Part);
        }

        if (item.HaveOption(210))
        {
            RemoveAndAddOptionTemplate(item);
        }

        if (item.HaveOption(231))
        {
            RemoveAndAddOptionTemplateHSD(item);
        }

        body[index] = item;
        return sItem;
    }

    public void ItemBagToBody(Player player, int index)
    {
        if (index < 0)
        {
            notifier.SendNotification(player, "Không thể thực hiện");
            return;
        }

        Item item = player.Inventory.ItemsBag[index];
        if (item.IsNotNullItem())
        {
            player.Inventory.ItemsBag[index] = PutItemBody(player, item);
            if (Tpl(item).Type == 39 && Tpl(item).Id >= 2002 && Tpl(item).Id <= 2010)
            {
                notifier.RemoveTitle(player);
                notifier.SendTrueSonOfHeaven(player, Tpl(item).Id);
            }

            UpdateAllSets(player);
            SendItemBag(player);
            SendItemBody(player);
            notifier.Point(player);
            notifier.SendCostume(player);
            notifier.SendFlagBag(player);
        }
    }

    public void ItemBodyToBag(Player player, int index)
    {
        Item item = player.Inventory.ItemsBody[index];
        if (GetCountEmptyBag(player) == 0)
        {
            notifier.SendNotification(player, "Hành trang của bạn không đủ chỗ trống");
            return;
        }

        if (item.IsNotNullItem())
        {
            if (index == 7 && !player.IsDeTu && player.PetFollow != null)
            {
                host.ExitMap(player.PetFollow);
                host.DisposePlayer(player.PetFollow);
                player.PetFollow = null;
            }

            if (Tpl(item).Type == 39)
            {
                notifier.RemoveTitle(player);
            }

            player.Inventory.ItemsBody[index] = PutItemBag(player, item);
            ResetAllSets(player, player, player);
            SendItemBag(player);
            SendItemBody(player);
            if ((index == 8 || index == 12) && item.IsNotNullItem())
            {
                notifier.RemovePlayerEffect(player, Tpl(item).Part);
            }

            notifier.Point(player);
            notifier.SendCostume(player);
            notifier.SendFlagBag(player);
        }
    }

    // Any exception is logged and answered with "Không thể thực hiện" (Java catch (Exception E)). The disciple needs 1,500,000 power to wear anything. The Java code reads the disciple's
    // power from player.Detu.nPoint.power; here IInventoryHost.GetPower.
    public void ItemBagToPetBody(Player player, int index)
    {
        try
        {
            Player? pet = player.Detu;
            if (pet != null && host.GetPower(pet) >= 1_500_000)
            {
                Item item = player.Inventory.ItemsBag[index];
                if (item.IsNotNullItem())
                {
                    Item itemSwap = PutItemBody(pet, item);
                    player.Inventory.ItemsBag[index] = itemSwap;
                    UpdateAllSets(pet);
                    SendItemBag(player);
                    SendItemBody(player);
                    if (!ReferenceEquals(itemSwap, item))
                    {
                        notifier.Point(player);
                        player.TypeTabPet = 0;
                        notifier.ShowInfoPet(player);
                    }

                    notifier.SendCostume(pet);
                    notifier.SendCostume(player);
                }
            }
            else
            {
                notifier.SendNotification(player, "Đệ tử phải đạt 1tr5 sức mạnh mới có thể mặc");
            }
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(InventoryService), e);
            notifier.SendNotification(player, "Không thể thực hiện");
        }
    }

    public void ItemPetBodyToBag(Player player, int index)
    {
        if (GetCountEmptyBag(player) == 0)
        {
            notifier.SendNotification(player, "Hành trang của bạn không đủ chỗ trống");
            return;
        }

        // Java: switch (player.typeTabPet) { case 0: petPl = player.Detu; default: petPl = player.Detu; }
        Player petPl = player.Detu ?? throw new InvalidOperationException("the player has no disciple (Java: NullPointerException)");
        Item item = petPl.Inventory.ItemsBody[index];
        if (item.IsNotNullItem())
        {
            petPl.Inventory.ItemsBody[index] = PutItemBag(player, item);
            ResetAllSets(petPl, player, player);
            SendItemBag(player);
            SendItemBody(player);
            notifier.Point(player);
            notifier.SendCostume(petPl);
            notifier.SendCostume(player);
            if (player.TypeTabPet == 0)
            {
                notifier.ShowInfoPet(player);
            }
        }
    }

    // Whether a box item can be put straight on the empty body slot of its type: right gender (3 = any) and enough power (option 21 overrides the template requirement, in billions).
    private bool CanWearFromBox(Player player, Item item)
    {
        if (Tpl(item).Gender != player.Gender && Tpl(item).Gender != 3)
        {
            return false;
        }

        long powerRequire = Tpl(item).StrRequire;
        foreach (ItemOption io in item.ItemOptions)
        {
            if (io.OptionTemplate.Id == 21)
            {
                powerRequire = io.Param * 1_000_000_000L;
                break;
            }
        }

        return powerRequire <= host.GetPower(player);
    }

    private static int BodySlotOf(Item item) => item.Template!.Type == 32 ? 6 : item.Template.Type;

    // An armour piece (types 0-5, 32) goes onto its EMPTY body slot when gender and power allow (the box slot then holds the empty item that was on the body); otherwise, or for any other item, it is added
    // to the bag with AddItemBag. If AddItemBag returns true without emptying the item (money, expansions, specials) the box slot keeps the item: Java behaviour, not changed.
    public void ItemBoxToBodyOrBag(Player player, int index)
    {
        if (index < 0)
        {
            notifier.SendNotification(player, "Không thể thực hiện");
            return;
        }

        if (GetCountEmptyBag(player) == 0)
        {
            notifier.SendNotification(player, "Hành trang của bạn không đủ chỗ trống");
            return;
        }

        Item item = player.Inventory.ItemsBox[index];
        if (item.IsNotNullItem())
        {
            bool done = false;
            if ((Tpl(item).Type >= 0 && Tpl(item).Type <= 5) || Tpl(item).Type == 32)
            {
                Item itemBody = player.Inventory.ItemsBody[BodySlotOf(item)];
                if (!itemBody.IsNotNullItem() && CanWearFromBox(player, item))
                {
                    player.Inventory.ItemsBody[BodySlotOf(item)] = item;
                    player.Inventory.ItemsBox[index] = itemBody;
                    UpdateAllSets(player);
                    done = true;
                    SendItemBody(player);
                    notifier.Point(player);
                    notifier.SendCostume(player);
                }
            }

            if (!done && AddItemBag(player, item))
            {
                if (item.Quantity == 0)
                {
                    player.Inventory.ItemsBox[index] = items.CreateItemNull();
                }

                SendItemBag(player);
            }

            SendItemBox(player);
        }
    }

    // Like ItemBoxToBodyOrBag but for the collection box, and without the set-bonus update (Java did not call it here).
    public void ItemBoxCollectionToBodyOrBag(Player player, int index)
    {
        if (index < 0)
        {
            notifier.SendNotification(player, "Không thể thực hiện");
            return;
        }

        if (GetCountEmptyBag(player) == 0)
        {
            notifier.SendNotification(player, "Hành trang của bạn không đủ chỗ trống");
            return;
        }

        Item item = player.Inventory.ItemsBoxCollection[index];
        if (item.IsNotNullItem())
        {
            bool done = false;
            if ((Tpl(item).Type >= 0 && Tpl(item).Type <= 5) || Tpl(item).Type == 32)
            {
                Item itemBody = player.Inventory.ItemsBody[BodySlotOf(item)];
                if (!itemBody.IsNotNullItem() && CanWearFromBox(player, item))
                {
                    player.Inventory.ItemsBody[BodySlotOf(item)] = item;
                    player.Inventory.ItemsBoxCollection[index] = itemBody;
                    done = true;
                    SendItemBody(player);
                    notifier.Point(player);
                    notifier.SendCostume(player);
                }
            }

            if (!done && AddItemBag(player, item))
            {
                if (item.Quantity == 0)
                {
                    player.Inventory.ItemsBoxCollection[index] = items.CreateItemNull();
                }

                SendItemBag(player);
            }

            SendItemBoxCollection(player);
        }
    }

    public void ItemBagToBox(Player player, int index)
    {
        if (index < 0)
        {
            notifier.SendNotification(player, "Không thể thực hiện");
            return;
        }

        Item item = player.Inventory.ItemsBag[index];
        if (item != null && item.IsNotNullItem())
        {
            if (Tpl(item).Id == 570 || Tpl(item).Id == 543)
            {
                notifier.SendNotification(player, "Không thể cất vật phẩm này vào rương");
                return;
            }

            if (AddItemBox(player, item))
            {
                if (item.Quantity == 0)
                {
                    player.Inventory.ItemsBag[index] = items.CreateItemNull();
                }

                SortItems(player.Inventory.ItemsBag);
                ResetAllSets(player, player, player);
                SendItemBag(player);
                SendItemBox(player);
            }
        }
    }

    public void ItemBagToBoxCollection(Player player, int index)
    {
        if (index < 0)
        {
            notifier.SendNotification(player, "Không thể thực hiện");
            return;
        }

        Item item = player.Inventory.ItemsBag[index];
        if (item != null && item.IsNotNullItem())
        {
            if (!item.IsItemCollection())
            {
                notifier.SendNotification(player, "Không thể cất vật phẩm này vào rương");
                return;
            }

            if (item.IsTrangBiHSD())
            {
                notifier.SendNotification(player, "Không thể cất vật phẩm có hạn sử dụng");
                return;
            }

            if (AddItemBoxCollection(player, item))
            {
                if (item.Quantity == 0)
                {
                    player.Inventory.ItemsBag[index] = items.CreateItemNull();
                }

                SortItems(player.Inventory.ItemsBag);
                SendItemBag(player);
                SendItemBoxCollection(player);
            }
        }
    }

    // The item goes to the first empty box slot; when the box is full it stays on the body (PutItemBox returns it) but the sets are reset and the packets still go out, as in Java. Java sorted the BAG here, not the box.
    public void ItemBodyToBox(Player player, int index)
    {
        Item item = player.Inventory.ItemsBody[index];
        if (item.IsNotNullItem())
        {
            player.Inventory.ItemsBody[index] = PutItemBox(player, item);
            SortItems(player.Inventory.ItemsBag);
            ResetAllSets(player, player, player);
            SendItemBody(player);
            SendItemBox(player);
            notifier.Point(player);
            notifier.SendCostume(player);
        }
    }

    public void ItemBodyToBoxCollection(Player player, int index)
    {
        Item item = player.Inventory.ItemsBody[index];
        if (item.IsNotNullItem())
        {
            if (!item.IsItemCollection())
            {
                notifier.SendNotification(player, "Không thể cất vật phẩm này vào rương");
                return;
            }

            if (item.IsTrangBiHSD())
            {
                notifier.SendNotification(player, "Không thể cất vật phẩm có hạn sử dụng");
                return;
            }

            player.Inventory.ItemsBody[index] = PutItemBoxCollection(player, item);
            SortItems(player.Inventory.ItemsBag);
            SendItemBody(player);
            SendItemBoxCollection(player);
            notifier.Point(player);
            notifier.SendCostume(player);
        }
    }

    // ---- clan box ----

    public void SendItemBoxClanToAll(Clan clan)
    {
        IReadOnlyList<long> memberIds = host.GetClanMemberIds(clan);
        for (int i = 0; i < memberIds.Count; i++)
        {
            Player? pl = host.GetPlayerById(memberIds[i]);
            if (pl != null)
            {
                SendItemClan(pl);
            }
        }
    }

    // Holds lock on the clan's box list while it moves the item (D1: members on other zone ticks use the same box). The database save and the packets to the members run after the lock is released.
    public void ItemBoxClanToBodyOrBag(Player player, int index)
    {
        if (player == null || player.Clan == null)
        {
            return;
        }

        if (index < 0)
        {
            notifier.SendNotification(player, "Không thể thực hiện");
            return;
        }

        if (GetCountEmptyBag(player) == 0)
        {
            notifier.SendNotification(player, "Hành trang của bạn không đủ chỗ trống");
            return;
        }

        List<Item> box = ClanBox(player);
        lock (box)
        {
            Item item = box[index];
            if (item != null && item.IsNotNullItem())
            {
                bool done = false;
                if ((Tpl(item).Type >= 0 && Tpl(item).Type <= 5) || Tpl(item).Type == 32)
                {
                    Item itemBody = player.Inventory.ItemsBody[BodySlotOf(item)];
                    if (!itemBody.IsNotNullItem() && CanWearFromBox(player, item))
                    {
                        player.Inventory.ItemsBody[BodySlotOf(item)] = item;
                        box[index] = itemBody;
                        done = true;
                        SendItemBody(player);
                        notifier.Point(player);
                        notifier.SendCostume(player);
                    }
                }

                if (!done && AddItemBag(player, item))
                {
                    if (item.Quantity == 0)
                    {
                        box[index] = items.CreateItemNull();
                    }

                    SendItemBag(player);
                }
            }
        }

        host.UpdateItemsBoxClanToSql(player.Clan);
        SendItemBoxClan(player);
        SendItemBoxClanToAll(player.Clan);
    }

    public void ItemBagToBoxClan(Player player, int index)
    {
        if (player == null || player.Clan == null)
        {
            return;
        }

        if (index < 0)
        {
            notifier.SendNotification(player, "Không thể thực hiện");
            return;
        }

        Item item = player.Inventory.ItemsBag[index];
        if (item != null && item.IsNotNullItem())
        {
            if (Tpl(item).Id == 570 || Tpl(item).Id == 543)
            {
                notifier.SendNotification(player, "Không thể cất vật phẩm này vào rương");
                return;
            }

            if (item.ItemOptions != null)
            {
                foreach (ItemOption io in item.ItemOptions)
                {
                    if (io != null && io.OptionTemplate.Id == 30)
                    {
                        notifier.SendNotification(player, "Không thể cất vật phẩm này vào rương");
                        return;
                    }
                }
            }

            if (AddItemBoxClan(player, item))
            {
                if (item.Quantity == 0)
                {
                    player.Inventory.ItemsBag[index] = items.CreateItemNull();
                }

                SortItems(player.Inventory.ItemsBag);
                SendItemBag(player);
                SendItemBoxClan(player);
            }
        }

        host.UpdateItemsBoxClanToSql(player.Clan);
        SendItemBoxClanToAll(player.Clan);
    }

    public void ItemBodyToBoxClan(Player player, int index)
    {
        if (player == null || player.Clan == null)
        {
            return;
        }

        Item item = player.Inventory.ItemsBody[index];
        if (item.IsNotNullItem())
        {
            player.Inventory.ItemsBody[index] = PutItemBoxClan(player, item);
            SortItems(player.Inventory.ItemsBag);
            SendItemBody(player);
            SendItemBoxClan(player);
            notifier.Point(player);
            notifier.SendCostume(player);
        }

        host.UpdateItemsBoxClanToSql(player.Clan);
        SendItemBoxClanToAll(player.Clan);
    }
}
