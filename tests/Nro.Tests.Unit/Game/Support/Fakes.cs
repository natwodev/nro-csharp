using Nro.Core.IO;
using Nro.Core.Templates;
using Nro.Game;
using Nro.Tests.Shared.Game.Support;

namespace Nro.Tests.Unit.Game.Support;

internal sealed class FakeKolHost : FakeKolItems, IKolTaskHost
{
    public int EmptyBagSlots { get; set; } = 100;

    public List<string> Notifications { get; } = [];

    public List<Item> Bag { get; } = [];

    public int ItemBagSent { get; private set; }

    public void SendNotification(Player player, string text) => Notifications.Add(text);

    public int GetCountEmptyBag(Player player) => EmptyBagSlots;

    public void AddItemBag(Player player, Item item) => Bag.Add(item);

    public void SendItemBag(Player player) => ItemBagSent++;
}

internal sealed class FakeSkillSpecialHost : ISkillSpecialHost
{
    public bool UsingNcd { get; set; }

    public bool Pl { get; set; } = true;

    public List<short> LevelsSent { get; } = [];

    public int Updates { get; private set; }

    public bool IsUsingNcd(Player player) => UsingNcd;

    public bool IsPl(Player player) => Pl;

    public void SendCurrLevelSpecial(Player player, Skill skill) => LevelsSent.Add(skill.CurrLevel);

    public void UpdateSkillSpecial(Player player) => Updates++;
}

internal sealed class FakeAchievementHost : IAchievementHost
{
    public bool Pl { get; set; } = true;

    public long PaidAmount { get; set; }

    public int EmptyBagSlots { get; set; } = 5;

    public List<string> Notifications { get; } = [];

    public List<int> Commands { get; } = [];

    public List<byte[]> MessageData { get; } = [];

    public List<Item> Bag { get; } = [];

    public int RubyAdded { get; private set; }

    public int ItemBagSent { get; private set; }

    public int MoneySent { get; private set; }

    public bool IsPl(Player player) => Pl;

    public long GetSessionPaidAmount(Player player) => PaidAmount;

    public int GetCountEmptyBag(Player player) => EmptyBagSlots;

    public Item CreateNewItem(short itemTemplateId, int quantity) => new() { Template = new ItemTemplate { Id = itemTemplateId, Name = "item" + itemTemplateId }, Quantity = quantity };

    public void AddItemBag(Player player, Item item) => Bag.Add(item);

    public void SendItemBag(Player player) => ItemBagSent++;

    public void AddRuby(Player player, int amount) => RubyAdded += amount;

    public void SendMoney(Player player) => MoneySent++;

    public void SendNotification(Player player, string text) => Notifications.Add(text);

    public void SendMessage(Player player, Message message)
    {
        Commands.Add(message.Command);
        MessageData.Add(message.GetData());
    }
}

internal sealed class FakeIntrinsicHost : IIntrinsicHost
{
    public long Power { get; set; } = 20_000_000_000L;

    public long Gold { get; set; }

    public List<string> Notifications { get; } = [];

    public List<(int Command, byte[] Data)> Messages { get; } = [];

    public List<(int MenuId, int Avatar, string Text, string[] Options)> Menus { get; } = [];

    public int MoneyInfoSent { get; private set; }

    public long GetPower(Player player) => Power;

    public long GetGold(Player player) => Gold;

    public void SetGold(Player player, long gold) => Gold = gold;

    public void SendInfoHpMpMoney(Player player) => MoneyInfoSent++;

    public void SendNotification(Player player, string text) => Notifications.Add(text);

    public void SendMessage(Player player, Message message) => Messages.Add((message.Command, message.GetData()));

    public void CreateMenuConMeo(Player player, int indexMenu, int avatar, string text, params string[] select) => Menus.Add((indexMenu, avatar, text, select));
}
