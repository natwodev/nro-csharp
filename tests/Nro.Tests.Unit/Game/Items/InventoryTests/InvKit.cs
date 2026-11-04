using System.Runtime.CompilerServices;
using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Templates;
using Nro.Data.Db;
using Nro.Data.Repositories.Gift;
using Nro.Game;

namespace Nro.Tests.Unit.Game.Items.InventoryTests;

/// <summary>
/// Everything an inventory test needs, built fresh for each test (no shared mutable state): a catalog where template id N is at position N, fakes for the four seams of
/// <see cref="InventoryService"/>, a player with an empty bag/body/box and the service itself.
/// </summary>
internal sealed class InvKit
{
    public const int BagSlots = 20;
    public const int BodySlots = 12;
    public const int BoxSlots = 10;

    public InvKit(IRandomProvider? random = null, TimeProvider? time = null)
    {
        var templates = new List<ItemTemplate>();
        for (int i = 0; i < 2600; i++)
        {
            templates.Add(new ItemTemplate { Id = (short)i, Type = 99, Name = "item" + i });
        }

        var options = new List<ItemOptionTemplate>();
        for (int i = 0; i < 300; i++)
        {
            options.Add(new ItemOptionTemplate(i, "option" + i + " #", 0));
        }

        Catalog = new ItemCatalog(templates, options);
        Factory = new FakeItemFactory(Catalog);
        Notifier = new FakeNotifier();
        Host = new FakeHost();
        GiftRepo = new FakeGiftCodeRepository();
        Usage = new FakeUsageTracker();
        Random = random ?? new SeededRandomProvider(12345);
        Service = new InventoryService(Factory, Notifier, Host, Random, GiftRepo, Usage, time);
        Player = NewPlayer(1);
    }

    public ItemCatalog Catalog { get; }

    public FakeItemFactory Factory { get; }

    public FakeNotifier Notifier { get; }

    public FakeHost Host { get; }

    public FakeGiftCodeRepository GiftRepo { get; }

    public FakeUsageTracker Usage { get; }

    public IRandomProvider Random { get; }

    public InventoryService Service { get; }

    public Player Player { get; }

    public Inventory Inv => Player.Inventory;

    /// <summary>A player with empty bag, body, box and collection box.</summary>
    public Player NewPlayer(long id) => Prepare(new Player { Id = id });

    /// <summary>A disciple (a <see cref="Pet"/> made without running its constructor); <see cref="FakeHost.PetMaster"/> is not set.</summary>
    public Pet NewPet(long id)
    {
        Pet pet = Raw<Pet>();
        pet.Id = id;
        pet.IsPet = true;
        Prepare(pet);
        return pet;
    }

    /// <summary>A following pet (a <see cref="PetFollow"/> made without running its constructor).</summary>
    public PetFollow NewFollow(long id)
    {
        PetFollow pet = Raw<PetFollow>();
        pet.Id = id;
        Prepare(pet);
        return pet;
    }

    private Player Prepare(Player p)
    {
        p.Inventory = new Inventory();
        Fill(p.Inventory.ItemsBag, BagSlots);
        Fill(p.Inventory.ItemsBody, BodySlots);
        Fill(p.Inventory.ItemsBox, BoxSlots);
        Fill(p.Inventory.ItemsBoxCollection, BoxSlots);
        return p;
    }

    public void Fill(List<Item> list, int count)
    {
        list.Clear();
        for (int i = 0; i < count; i++)
        {
            list.Add(Empty());
        }
    }

    public Item Empty() => new(Catalog, 0);

    /// <summary>Sets the type (and optionally more) of template <paramref name="id"/> and returns that template.</summary>
    public ItemTemplate Define(int id, sbyte type = 99, bool upToUp = false, sbyte gender = 3, int strRequire = 0, sbyte level = 0, string? name = null, int typeEvent = 0, short part = 0)
    {
        ItemTemplate t = Catalog.GetTemplate(id);
        t.Type = type;
        t.IsUpToUp = upToUp;
        t.Gender = gender;
        t.StrRequire = strRequire;
        t.Level = level;
        t.TypeEvent = typeEvent;
        t.Part = part;
        if (name != null)
        {
            t.Name = name;
        }

        return t;
    }

    public ItemOption Opt(int optionId, int param) => new(Catalog.GetItemOptionTemplate(optionId), param);

    /// <summary>A real item of template <paramref name="id"/> with the given quantity and options (pairs of option id and parameter).</summary>
    public Item Item(int id, int quantity = 1, params (int OptionId, int Param)[] options)
    {
        var item = new Item(Catalog, 0, (short)id) { Quantity = quantity };
        foreach ((int optionId, int param) in options)
        {
            item.ItemOptions.Add(Opt(optionId, param));
        }

        return item;
    }

    /// <summary>Puts <paramref name="item"/> in slot <paramref name="index"/> of <paramref name="list"/> and returns it.</summary>
    public Item Put(List<Item> list, int index, Item item)
    {
        list[index] = item;
        return item;
    }

    public int CountReal(List<Item> list) => list.Count(i => i.IsNotNullItem());

    public IReadOnlyList<int> Ids(List<Item> list) => list.Select(i => i.IsNotNullItem() ? (int)i.Template!.Id : -1).ToList();

    public static T Raw<T>() => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

    public static byte[] Bytes(Message m) => m.GetData();
}

internal sealed class FakeItemFactory : IInventoryItemFactory
{
    private readonly ItemCatalog catalog;

    public FakeItemFactory(ItemCatalog catalog)
    {
        this.catalog = catalog;
    }

    public int NullCreated { get; private set; }

    public Item CreateItemNull()
    {
        NullCreated++;
        return new Item(catalog, 0);
    }

    public Item CopyItem(Item item)
    {
        Item it = new(catalog, 0)
        {
            Template = item.Template,
            Info = item.Info,
            Content = item.Content,
            Quantity = item.Quantity,
            CreateTime = item.CreateTime,
        };
        foreach (ItemOption io in item.ItemOptions)
        {
            it.ItemOptions.Add(new ItemOption(io));
        }

        return it;
    }

    public Item CreateNewItem(short tempId) => new(catalog, 0, tempId) { Quantity = 1, Content = "content" + tempId, Info = "info" + tempId };

    public Item NewItem(short tempId) => new(catalog, 0, tempId);

    public ItemOption CreateItemOption(int templateId, int param) => new(catalog.GetItemOptionTemplate(templateId), param);
}

internal sealed class FakeNotifier : IInventoryNotifier
{
    public List<string> Calls { get; } = [];

    public List<(Player Player, sbyte Command, byte[] Data)> Packets { get; } = [];

    /// <summary>The ids of the players that got a SendNotification, SendNotificationOk or SendAdminNotification, in order.</summary>
    public List<long> NoticeTargets { get; } = [];

    public IEnumerable<string> Texts(string kind) => Calls.Where(c => c.StartsWith(kind + ":", StringComparison.Ordinal)).Select(c => c[(kind.Length + 1)..]);

    public void SendMessage(Player player, Message message) => Packets.Add((player, message.Command, message.GetData()));

    public void SendNotification(Player player, string text)
    {
        NoticeTargets.Add(player.Id);
        Calls.Add("SendNotification:" + text);
    }

    public void SendNotificationOk(Player player, string text)
    {
        NoticeTargets.Add(player.Id);
        Calls.Add("SendNotificationOk:" + text);
    }

    public void SendAdminNotification(Player player, string text)
    {
        NoticeTargets.Add(player.Id);
        Calls.Add("SendAdminNotification:" + text);
    }

    public void SendMoney(Player player) => Calls.Add("SendMoney");

    public void Point(Player player) => Calls.Add("Point");

    public void SendCostume(Player player) => Calls.Add("SendCostume:" + player.Id);

    public void SendFlagBag(Player player) => Calls.Add("SendFlagBag");

    public void SendTrueSonOfHeaven(Player player, int itemTemplateId) => Calls.Add("SendTrueSonOfHeaven:" + itemTemplateId);

    public void RemoveTitle(Player player) => Calls.Add("RemoveTitle");

    public void RemovePlayerEffect(Player player, int idEff) => Calls.Add("RemovePlayerEffect:" + idEff);

    public void ShowInfoPet(Player player) => Calls.Add("ShowInfoPet");

    public void AddPotential(Player player, sbyte type, long amount) => Calls.Add("AddPotential:" + type + ":" + amount);

    public void SendInfoHpMp(Player player) => Calls.Add("SendInfoHpMp");
}

internal sealed class FakeHost : IInventoryHost
{
    public List<string> Calls { get; } = [];

    public bool Pl { get; set; } = true;

    public short Head { get; set; } = 5;

    public Player? PetMaster { get; set; }

    public long PowerValue { get; set; } = long.MaxValue;

    public Dictionary<Player, long> PowerOf { get; } = [];

    public sbyte TypeFusion { get; set; }

    public string? ShopTag { get; set; }

    public bool ShopTagThrows { get; set; }

    public int UserId { get; set; } = 77;

    public Dictionary<long, Player> Online { get; } = [];

    public HashSet<int> BlackBalls { get; } = [];

    public HashSet<int> NamecBalls { get; } = [];

    public HashSet<int> NamecStones { get; } = [];

    public bool PickResult { get; set; } = true;

    public List<Item> ClanBox { get; } = [];

    public List<long> MemberIds { get; } = [];

    public (long, long)? SkhResult { get; set; }

    public bool IsPl(Player player) => Pl;

    public short GetHead(Player player) => Head;

    public Player GetPetMaster(Player pet) => PetMaster ?? throw new InvalidCastException("not a disciple");

    public long GetPower(Player player) => PowerOf.TryGetValue(player, out long p) ? p : PowerValue;

    public sbyte GetTypeFusion(Player player) => TypeFusion;

    public string? GetOpenShopTagName(Player player) => ShopTagThrows ? throw new InvalidOperationException("no shop") : ShopTag;

    public int GetSessionUserId(Player player) => UserId >= 0 ? UserId : throw new InvalidOperationException("no session");

    public void AddTimeCharms(Player player, int itemId, int minutes) => Calls.Add("AddTimeCharms:" + itemId + ":" + minutes);

    public void CreateBillEgg(Player player) => Calls.Add("CreateBillEgg");

    public void SetFullHpMp(Player player) => Calls.Add("SetFullHpMp");

    public void SetFullHp(Player player) => Calls.Add("SetFullHp");

    public void SetFullMp(Player player) => Calls.Add("SetFullMp");

    public (long TimeCreateAccount, long AccountAgeDays)? AddActivationSetSearchDays(Player player, int days)
    {
        Calls.Add("AddActivationSetSearchDays:" + days);
        return SkhResult;
    }

    public Player? GetPlayerById(long playerId) => Online.GetValueOrDefault(playerId);

    public void ExitMap(Player player) => Calls.Add("ExitMap:" + player.Id);

    public void DisposePlayer(Player player) => Calls.Add("DisposePlayer:" + player.Id);

    public bool IsBlackBall(int tempId) => BlackBalls.Contains(tempId);

    public bool IsNamecBall(int tempId) => NamecBalls.Contains(tempId);

    public bool IsNamecBallStone(int tempId) => NamecStones.Contains(tempId);

    public bool PickBlackBall(Player player, Item item)
    {
        Calls.Add("PickBlackBall");
        return PickResult;
    }

    public bool PickNamekBall(Player player, Item item)
    {
        Calls.Add("PickNamekBall");
        return PickResult;
    }

    public List<Item> GetItemsBoxClan(Clan clan) => ClanBox;

    public IReadOnlyList<long> GetClanMemberIds(Clan clan) => MemberIds;

    public void UpdateItemsBoxClanToSql(Clan clan) => Calls.Add("UpdateItemsBoxClanToSql");
}

internal sealed class FakeGiftCodeRepository : IGiftCodeRepository
{
    public Dictionary<string, int> CountLeft { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<string> Calls { get; } = [];

    public List<(int UserId, long PlayerId, string Code, DateTime When)> Saved { get; } = [];

    public Task<int?> GetCountLeftAsync(string code, CancellationToken cancellationToken = default)
    {
        Calls.Add("Get:" + code);
        return Task.FromResult<int?>(CountLeft.TryGetValue(code, out int n) ? n : null);
    }

    public Task<int> SetCountLeftAsync(string code, int countLeft, CancellationToken cancellationToken = default)
    {
        Calls.Add("Set:" + code + ":" + countLeft);
        if (CountLeft.ContainsKey(code))
        {
            CountLeft[code] = countLeft;
            return Task.FromResult(1);
        }

        return Task.FromResult(0);
    }

    public Task SaveUseAsync(int userId, long playerId, string code, DateTime whenVietnamTime, CancellationToken cancellationToken = default)
    {
        Calls.Add("Save:" + code);
        Saved.Add((userId, playerId, code, whenVietnamTime));
        return Task.CompletedTask;
    }
}

internal sealed class FakeUsageTracker : IGiftCodeUsageTracker
{
    public List<(int PlayerId, string Code)> Used { get; } = [];

    public void MarkUsed(int playerId, string code) => Used.Add((playerId, code));
}
