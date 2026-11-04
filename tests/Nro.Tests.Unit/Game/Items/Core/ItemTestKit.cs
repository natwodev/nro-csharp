using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Templates;
using Nro.Game;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.Items.Core;

/// <summary>
/// Shared fixtures of the item model tests: a SYNTHETIC catalog (no real game rows), a fake host that records what the item code asks the game to do, and a clock the test can move.
/// </summary>
internal static class ItemTestKit
{
    /// <summary>Position count of the synthetic option table (ids 0 to 299; id 300 and above, and -1, are outside).</summary>
    public const int OptionCount = 300;

    /// <summary>Position count of the synthetic item table.</summary>
    public const int TemplateCount = 1900;

    /// <summary>1 January 2025 00:00 Vietnam time as epoch milliseconds (an arbitrary but fixed start).</summary>
    public static readonly long T0 = ManualTimeProvider.Vn(2025, 1, 1).ToUnixTimeMilliseconds();

    /// <summary>
    /// The option texts are "Opt{id} #" so a test can read the id and the parameter back from <see cref="ItemOption.GetOptionString"/>.
    /// Item templates: id = position; the default type is 99; <paramref name="configure"/> may adjust a template (type, level, gender).
    /// </summary>
    public static ItemCatalog Catalog(Action<ItemTemplate>? configure = null, IEnumerable<ShopItemEntry>? shop = null)
    {
        var options = new List<ItemOptionTemplate>();
        for (int i = 0; i < OptionCount; i++)
        {
            options.Add(new ItemOptionTemplate(i, "Opt" + i + " #", 0));
        }

        var templates = new List<ItemTemplate>();
        for (int i = 0; i < TemplateCount; i++)
        {
            var t = new ItemTemplate { Id = (short)i, Type = 99, Name = "Item" + i, StrRequire = i * 10, IconID = (short)(i + 1000) };
            configure?.Invoke(t);
            templates.Add(t);
        }

        return new ItemCatalog(templates, options, shop);
    }

    public static ManualTimeProvider Time(long? epochMs = null) => new(DateTimeOffset.FromUnixTimeMilliseconds(epochMs ?? T0));

    public static VietnamClock Clock(ManualTimeProvider time) => new(time);

    public static ItemService Service(ItemCatalog catalog, IRandomProvider random, ManualTimeProvider time, IItemHost? host = null)
        => new(catalog, new RandomUtil(random), Clock(time), host);

    public static ItemOption Opt(ItemCatalog catalog, int id, int param) => new(catalog.GetItemOptionTemplate(id), param);

    public static Item MakeItem(ItemCatalog catalog, short templateId, long createTime = 0, params (int Id, int Param)[] options)
    {
        var item = new Item(catalog, createTime, templateId) { Quantity = 1 };
        foreach ((int id, int param) in options)
        {
            item.ItemOptions.Add(Opt(catalog, id, param));
        }

        return item;
    }

    /// <summary>"id:param" for each option, to compare option lists in one assertion.</summary>
    public static string Dump(IEnumerable<ItemOption> options) => string.Join(",", options.Select(o => o.OptionTemplate.Id + ":" + o.Param));

    public static Player NewPlayer() => new();
}

/// <summary>Records every call the item code makes on the host.</summary>
internal sealed class RecordingHost : IItemHost
{
    public List<(Player Player, sbyte Command, byte[] Data)> Messages { get; } = [];

    public List<string> Notifications { get; } = [];

    public List<string> AdminNotifications { get; } = [];

    public List<string> Calls { get; } = [];

    public List<Item> BagAdds { get; } = [];

    public List<(Item? Item, int Quantity)> BagSubs { get; } = [];

    public int EmptyBagSlots { get; set; } = 10;

    public Dictionary<int, Item> Bag { get; } = [];

    public List<Item> CombineItems { get; } = [];

    public ExternalItemTimes External { get; set; } = ExternalItemTimes.None;

    public void SendMessage(Player player, Message message)
    {
        Messages.Add((player, message.Command, message.GetData()));
        Calls.Add("msg" + message.Command);
    }

    public void SendPoint(Player player) => Calls.Add("point");

    public void SendNotification(Player player, string text)
    {
        Notifications.Add(text);
        Calls.Add("notify");
    }

    public void SendNotificationFromAdmin(Player player, string text)
    {
        AdminNotifications.Add(text);
        Calls.Add("notifyAdmin");
    }

    public void SendItemBag(Player player) => Calls.Add("bag");

    public int GetCountEmptyBag(Player player) => EmptyBagSlots;

    public void AddItemBag(Player player, Item item)
    {
        BagAdds.Add(item);
        Calls.Add("add");
    }

    public void SubQuantityItemsBag(Player player, Item? item, int quantity)
    {
        BagSubs.Add((item, quantity));
        Calls.Add("sub");
    }

    public Item? FindBagItem(Player player, int templateId) => Bag.TryGetValue(templateId, out Item? item) ? item : null;

    public IReadOnlyList<Item> GetCombineItems(Player player) => CombineItems;

    public void RemoveMonkeyTail(Player player) => Calls.Add("duoikhi");

    public void IncreasePowerLimit(Player player) => Calls.Add("powerlimit");

    public ExternalItemTimes GetExternalItemTimes(Player player) => External;
}
