namespace Nro.Game.Items.Inventory;

// Lưu tiền tệ và các danh sách vật phẩm của một nhân vật hoặc đệ tử.
// Nguồn Java: nro/inventory/Inventory.java (giữ ngữ nghĩa số nguyên, giới hạn tiền và quy ước danh sách không null đã chuyển đổi.)

// Port of nro/inventory/Inventory.java: the money and the item lists of one character (or of one disciple).
// MONEY SEMANTICS (kept exactly as in Java, because the numbers are written to the database and to the client):
// Gold is a long; Gem, Ruby and VipExperience are int and WRAP around in two's complement, like Java
// int (the code below is unchecked on purpose): AddGem adds first and compares with the limit afterwards, so 2_000_000_000 + 2_000_000_000 becomes
// -294_967_296, which is not above the limit and is stored as is. Callers that need a clamp must check before they add (InventoryService.AddItemBag does it in long).
// The Subtract* methods do not look at the sign: they can drive a balance below zero. The caller checks first.
// AddGold takes an int and clamps to LIMIT_GOLD (2 * 10^12); AddGem, AddRuby and AddVipExperience clamp to 2 * 10^9.
// Java dispose() set the seven lists to null. Here (rule of the Player shell, 05b) the lists are never null: Dispose disposes the items and empties the lists, and a disposed
// inventory is simply an empty one.
// Threading (decision D1): an inventory is touched only from the tick of the zone that contains its owner. The one structure shared between players, the clan box, is not here (it belongs
// to the clan) and is locked by InventoryService.
public class Inventory
{
    // Java LIMIT_GOLD: 2,000,000,000,000.
    public const long LIMIT_GOLD = 2_000_000_000_000L;

    // Giới hạn vàng thứ hai: 2,000,000,000; Java không dùng, giữ cho bảng hằng. (Java: LIMIT_GOLD2)
    public const long LIMIT_GOLD_2 = 2_000_000_000L;

    // Java LIMIT_RUBY.
    public const int LIMIT_RUBY = 2_000_000_000;

    // Java LIMIT_GEM.
    public const int LIMIT_GEM = 2_000_000_000;

    // Giới hạn kinh nghiệm VIP. (Java: LIMIT_EXP_VIP)
    public const int LIMIT_VIP_EXPERIENCE = 2_000_000_000;

    // Java MAX_ITEMS_BAG.
    public const int MAX_ITEMS_BAG = 100;

    // Java MAX_ITEMS_BOX.
    public const int MAX_ITEMS_BOX = 100;

    // Giáp tập luyện hoặc null. (Java: trainArmor)
    public Item? TrainingArmor { get; set; }

    // Các mã quà tặng người chơi đã dùng. (Java: giftCode)
    public List<string> GiftCodes { get; set; } = [];

    // Java itemsBody: the worn equipment (fixed slots, an empty slot is an item without template).
    public List<Item> ItemsBody { get; set; } = [];

    // Java itemsBag: the bag.
    public List<Item> ItemsBag { get; set; } = [];

    // Java itemsBox: the storage box.
    public List<Item> ItemsBox { get; set; } = [];

    // Java itemsBoxCollection: the collection box.
    public List<Item> ItemsBoxCollection { get; set; } = [];

    // Vật phẩm trong rương vòng quay may mắn. (Java: itemsBoxCrackBall)
    public List<Item> ItemsBoxLuckyRound { get; set; } = [];

    // Vật phẩm trong hòm thư. (Java: itemsMailBox)
    public List<Item> ItemsMailbox { get; set; } = [];

    // Vật phẩm đã bán. (Java: itemsDaBan)
    public List<Item> SoldItems { get; set; } = [];

    // Java gold (64 bits).
    public long Gold { get; set; }

    // Java gem (32 bits, wraps).
    public int Gem { get; set; }

    // Java ruby (32 bits, wraps).
    public int Ruby { get; set; }

    // Kinh nghiệm VIP (32 bit, tràn theo Java). (Java: Exp_Vip)
    public int VipExperience { get; set; }

    // Java Inventory(): all lists empty, all money 0.
    public Inventory()
    {
    }

    // Java getGemAndRuby(): gem + ruby in 32 bits (wraps).
    public int GetGemAndRuby() => unchecked(Gem + Ruby);

    // Java getGem().
    public int GetGem() => Gem;

    // Java getRuby().
    public int GetRuby() => Ruby;

    // Lấy kinh nghiệm VIP. (Java: getExpVip)
    public int GetVipExperience() => VipExperience;

    // Java getGold().
    public long GetGold() => Gold;

    // Java getGoldLimit(): always LIMIT_GOLD.
    public long GetGoldLimit() => LIMIT_GOLD;

    // Lấy tham số tùy chọn đầu tiên khớp id hoặc 0. (Java: getParam)
    public int GetItemOptionParameter(Item it, int id)
    {
        foreach (ItemOption op in it.ItemOptions)
        {
            if (op != null && op.OptionTemplate.Id == id)
            {
                return op.Param;
            }
        }

        return 0;
    }

    // Kiểm tra item tại index có tùy chọn id. (Java: haveOption)
    // index is not a position of the list (Java: IndexOutOfBoundsException).
    public bool HasOption(List<Item> l, int index, int id)
    {
        Item it = l[index];
        if (it != null && it.IsNotNullItem())
        {
            foreach (ItemOption op in it.ItemOptions)
            {
                if (op != null && op.OptionTemplate.Id == id)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Trừ gem, có thể tràn hoặc âm. (Java: subGem)
    public void SubtractGem(int num) => Gem = unchecked(Gem - num);

    // Trừ gold kiểu int. (Java: subGold)
    public void SubtractGold(int num) => Gold -= num;

    // Trừ gold kiểu long. (Java: subGold)
    public void SubtractGold(long num) => Gold -= num;

    // Trừ ruby. (Java: subRuby)
    public void SubtractRuby(int num) => Ruby = unchecked(Ruby - num);

    // Trừ kinh nghiệm VIP. (Java: subExpVip)
    public void SubtractVipExperience(int num) => VipExperience = unchecked(VipExperience - num);

    // Trừ gem và ruby: ưu tiên ruby, phần thiếu trừ vào gem. (Java: subGemAndRuby)
    // The gems can go negative if there are not enough (the caller checks GetGemAndRuby first).
    public void SubtractGemAndRuby(int num)
    {
        unchecked
        {
            Ruby -= num;
            if (Ruby < 0)
            {
                Gem += Ruby;
                Ruby = 0;
            }
        }
    }

    // Java addGold(int): adds, then clamps to LIMIT_GOLD.
    public void AddGold(int gold)
    {
        Gold += gold;
        if (Gold > LIMIT_GOLD)
        {
            Gold = LIMIT_GOLD;
        }
    }

    // Java addRuby(int): adds in 32 bits (wraps), then clamps to LIMIT_RUBY.
    public void AddRuby(int ruby)
    {
        Ruby = unchecked(Ruby + ruby);
        if (Ruby > LIMIT_RUBY)
        {
            Ruby = LIMIT_RUBY;
        }
    }

    // Java addGem(int): adds in 32 bits (wraps), then clamps to LIMIT_GEM.
    public void AddGem(int gem)
    {
        Gem = unchecked(Gem + gem);
        if (Gem > LIMIT_GEM)
        {
            Gem = LIMIT_GEM;
        }
    }

    // Cộng kinh nghiệm VIP rồi chặn ở giới hạn. (Java: addExpVip)
    public void AddVipExperience(int vip)
    {
        VipExperience = unchecked(VipExperience + vip);
        if (VipExperience > LIMIT_VIP_EXPERIENCE)
        {
            VipExperience = LIMIT_VIP_EXPERIENCE;
        }
    }

    // Java dispose(): disposes the training armour and every item of the seven lists and empties the lists. Unlike Java the list objects stay (the shell rule: never null),
    // so calling it twice, or using the inventory afterwards, is harmless. Money is untouched.
    public void Dispose()
    {
        TrainingArmor?.Dispose();
        TrainingArmor = null;
        DisposeAndClear(ItemsBody);
        DisposeAndClear(ItemsBag);
        DisposeAndClear(ItemsBox);
        DisposeAndClear(ItemsBoxCollection);
        DisposeAndClear(ItemsBoxLuckyRound);
        DisposeAndClear(ItemsMailbox);
        DisposeAndClear(SoldItems);
    }

    private static void DisposeAndClear(List<Item> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].Dispose();
        }

        items.Clear();
    }
}
