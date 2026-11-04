using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.IO;

namespace Nro.Game.Items.Inventory;

// Tạo và gửi các gói dữ liệu đồng bộ kho đồ cho khách.
// Nguồn Java: nro/inventory/InventoryService.java (partial phụ của lớp chính; giữ mã gói, thứ tự trường và dữ liệu Java.)

// Section "Gui danh sach item cho nguoi choi" of InventoryService.java: the inventory packets.
//
// Wire format (docs/PROTOCOL.md, cmd -35 box, -36 bag, -37 body):
//   -36: byte 0, byte n, n * item                 -37: byte 0, short head, byte n, n * item
//   -35: byte 0, byte n, n * item   or   byte 1 (open/close the box window)
//   item: short -1 (empty slot)   or   short templateId, int quantity, UTF info, UTF content, byte nOptions, nOptions * (int optionId, int param)
// The option is written as (int, int) although the client reads (ubyte, ushort): known mismatch D2 in PROTOCOL.md, the server side is kept as it is.
public sealed partial class InventoryService
{
    // Command of the bag packet.
    public const int CmdBag = -36;

    // Command of the body (worn equipment) packet.
    public const int CmdBody = -37;

    // Command of the box packets (storage box, collection box, clan box; the same command, only the content differs).
    public const int CmdBox = -35;

    private static void WriteItem(Message msg, Item item)
    {
        if (!item.IsNotNullItem())
        {
            msg.Writer().WriteShort(-1);
            return;
        }

        msg.Writer().WriteShort(Tpl(item).Id);
        msg.Writer().WriteInt(item.Quantity);
        msg.Writer().WriteUTF(item.GetInfo());
        msg.Writer().WriteUTF(item.GetContent());
        msg.Writer().WriteByte(item.ItemOptions.Count);
        for (int j = 0; j < item.ItemOptions.Count; j++)
        {
            msg.Writer().WriteInt(item.ItemOptions[j].OptionTemplate.Id);
            msg.Writer().WriteInt(item.ItemOptions[j].Param);
        }
    }

    // Builds and sends one "list of items" packet (-35 or -36); any exception is logged and nothing is sent, like the Java catch (Exception e).
    private void SendItemList(Player player, int command, List<Item> list)
    {
        try
        {
            var msg = new Message(command);
            msg.Writer().WriteByte(0);
            msg.Writer().WriteByte(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                WriteItem(msg, list[i]);
            }

            notifier.SendMessage(player, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(InventoryService), e);
        }
    }

    private void SendOpenBox(Player player)
    {
        try
        {
            var msg = new Message(CmdBox);
            msg.Writer().WriteByte(1);
            notifier.SendMessage(player, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(InventoryService), e);
        }
    }

    // Sorts the bag first (SortItems), so the client always sees the items packed at the front.
    public void SendItemBag(Player player)
    {
        SortItems(player.Inventory.ItemsBag);
        SendItemList(player, CmdBag, player.Inventory.ItemsBag);
    }

    // After the packet it also sends the "chan menh" effect when slot 11 holds an item of type 39, then the look and the flag bag. Slot 11 is read OUTSIDE the try block (Java did the same), so a body with
    // fewer than 12 slots throws ArgumentOutOfRangeException from here.
    public void SendItemBody(Player player)
    {
        try
        {
            var msg = new Message(CmdBody);
            msg.Writer().WriteByte(0);
            msg.Writer().WriteShort(host.GetHead(player));
            msg.Writer().WriteByte(player.Inventory.ItemsBody.Count);
            foreach (Item item in player.Inventory.ItemsBody)
            {
                WriteItem(msg, item);
            }

            notifier.SendMessage(player, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(InventoryService), e);
        }

        Item chanMenh = player.Inventory.ItemsBody[11];
        if (chanMenh != null && chanMenh.IsNotNullItem() && Tpl(chanMenh).Type == 39)
        {
            notifier.SendTrueSonOfHeaven(player, Tpl(chanMenh).Id);
        }

        notifier.SendCostume(player);
        notifier.SendFlagBag(player);
    }

    public void SendItemBox(Player player)
    {
        SendItemList(player, CmdBox, player.Inventory.ItemsBox);
        OpenBox(player);
    }

    public void OpenBox(Player player) => SendOpenBox(player);

    public void SendItemBoxCollection(Player player)
    {
        SendItemList(player, CmdBox, player.Inventory.ItemsBoxCollection);
        OpenBox(player);
    }

    public void OpenBoxCollection(Player player) => SendOpenBox(player);

    public void SendItemBoxClan(Player player)
    {
        if (player == null || player.Clan == null)
        {
            return;
        }

        SendClanList(player);
        OpenBoxClan(player);
    }

    public void SendItemClan(Player player)
    {
        if (player == null || player.Clan == null)
        {
            return;
        }

        SendClanList(player);
    }

    private void SendClanList(Player player)
    {
        List<Item> box = ClanBox(player);
        lock (box)
        {
            SendItemList(player, CmdBox, box);
        }
    }

    public void OpenBoxClan(Player player)
    {
        if (player == null || player.Clan == null)
        {
            return;
        }

        SendOpenBox(player);
    }
}
