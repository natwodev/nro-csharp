using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Templates;

namespace Nro.Game.Items.Core;

// Tạo, sao chép và quay vật phẩm, tùy chọn rơi, phần thưởng bộ và xử lý hạn dùng.
// Nguồn Java: models/Item/ItemService.java (không dùng singleton: catalog, random, đồng hồ và host được tiêm; RandomUtil giữ vị trí rút ngẫu nhiên; ItemMap được thay bằng RolledDrop; phần thưởng bộ dùng bảng dữ liệu/ClaimSetReward và vẫn quay trước khi kiểm tra túi; danh sách tùy chọn shop trả bản sao thay vì đối tượng gốc Java).
public class ItemService
{
    private readonly ItemCatalog catalog;
    private readonly RandomUtil random;
    private readonly VietnamClock clock;
    private readonly IItemHost host;

    // Thực hiện thành viên; giữ nguyên điều kiện, thứ tự và số học của Java.
    public ItemService(ItemCatalog catalog, RandomUtil random, VietnamClock clock, IItemHost? host = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(clock);
        this.catalog = catalog;
        this.random = random;
        this.clock = clock;
        this.host = host ?? NullItemHost.Instance;
    }

    // Thực hiện catalog; giữ nguyên điều kiện, thứ tự và số học của Java.
    public ItemCatalog Catalog => catalog;

    // Tạo create option; giữ nguyên điều kiện, thứ tự và số học của Java (Java: IndexOutOfBoundsException).
    public ItemOption CreateOption(int tempId, int param) => new(catalog.GetItemOptionTemplate(tempId), param);

    // Thực hiện new item; giữ nguyên điều kiện, thứ tự và số học của Java (Java: new Item().
    private Item NewItem() => new(catalog, clock.CurrentTimeMillis());

    // Java getItemOptionTemplate(id). (Java: getItemOptionTemplate(id)).
    public ItemOptionTemplate GetItemOptionTemplate(int id) => catalog.GetItemOptionTemplate(id);

    // Java getTemplate(id). (Java: getTemplate(id)).
    public ItemTemplate GetTemplate(int id) => catalog.GetTemplate(id);

    // Lấy get item id by icon; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getItemIdByIcon).
    public short GetItemIdByIcon(short iconId) => catalog.GetItemIdByIcon(iconId);

    // ---------------------------------------------------------------------------------------------------------------------------------------------------
    // Vật phẩm Thần Linh rơi ngẫu nhiên (randDoTL / randDoTLBoss / getDefaultOptionTL)
    // ---------------------------------------------------------------------------------------------------------------------------------------------------

    // Chọn ngẫu nhiên rand do tlboss; giữ nguyên điều kiện, thứ tự và số học của Java (Java: randDoTLBoss).
    public RolledDrop RandDoTLBoss()
    {
        short idTempTL;
        short[] ao = [555, 557, 559];
        short[] quan = [556, 558, 560];
        short[] gang = [562, 564, 566];
        short[] giay = [563, 565, 567];
        short[] nhan = [561];
        short[] options = [86, 87];
        if (random.IsTrueInt(10, 100))
        {
            idTempTL = nhan[0];
        }
        else if (random.IsTrueInt(25, 100))
        {
            idTempTL = gang[random.NextInt(3)];
        }
        else if (random.IsTrueInt(45, 100))
        {
            idTempTL = quan[random.NextInt(3)];
        }
        else if (random.IsTrueInt(75, 100))
        {
            idTempTL = ao[random.NextInt(3)];
        }
        else
        {
            idTempTL = giay[random.NextInt(3)];
        }

        int tiLe = random.NextInt(100, 115);
        var itemoptions = new List<ItemOption>();
        int chiso;
        switch (idTempTL)
        {
            case 555:
                itemoptions.Add(CreateOption(47, 800 * tiLe / 100));
                if (tiLe > 100)
                {
                    itemoptions.Add(CreateOption(207, tiLe - 100));
                }

                break;
            case 557:
                itemoptions.Add(CreateOption(47, 850 * tiLe / 100));
                if (tiLe > 100)
                {
                    itemoptions.Add(CreateOption(207, tiLe - 100));
                }

                break;
            case 559:
                itemoptions.Add(CreateOption(47, 900 * tiLe / 100));
                if (tiLe > 100)
                {
                    itemoptions.Add(CreateOption(207, tiLe - 100));
                }

                break;
            case 556:
                chiso = 52000 * tiLe / 100;
                itemoptions.Add(CreateOption(22, chiso / 1000));
                itemoptions.Add(CreateOption(27, chiso / 20));
                if (tiLe > 100)
                {
                    itemoptions.Add(CreateOption(207, tiLe - 100));
                }

                break;
            case 558:
                chiso = 50000 * tiLe / 100;
                itemoptions.Add(CreateOption(22, chiso / 1000));
                itemoptions.Add(CreateOption(27, chiso / 20));
                if (tiLe > 100)
                {
                    itemoptions.Add(CreateOption(207, tiLe - 100));
                }

                break;
            case 560:
                chiso = 48000 * tiLe / 100;
                itemoptions.Add(CreateOption(22, chiso / 1000));
                itemoptions.Add(CreateOption(27, chiso / 20));
                if (tiLe > 100)
                {
                    itemoptions.Add(CreateOption(207, tiLe - 100));
                }

                break;
            case 562:
                itemoptions.Add(CreateOption(0, 4400 * tiLe / 100));
                if (tiLe > 100)
                {
                    itemoptions.Add(CreateOption(207, tiLe - 100));
                }

                break;
            case 564:
                itemoptions.Add(CreateOption(0, 4300 * tiLe / 100));
                if (tiLe > 100)
                {
                    itemoptions.Add(CreateOption(207, tiLe - 100));
                }

                break;
            case 566:
                itemoptions.Add(CreateOption(0, 4500 * tiLe / 100));
                if (tiLe > 100)
                {
                    itemoptions.Add(CreateOption(207, tiLe - 100));
                }

                break;
            case 563:
                chiso = 48000 * tiLe / 100;
                itemoptions.Add(CreateOption(23, chiso / 1000));
                itemoptions.Add(CreateOption(28, chiso / 20));
                if (tiLe > 100)
                {
                    itemoptions.Add(CreateOption(207, tiLe - 100));
                }

                break;
            case 565:
                chiso = 50000 * tiLe / 100;
                itemoptions.Add(CreateOption(23, chiso / 1000));
                itemoptions.Add(CreateOption(28, chiso / 20));
                if (tiLe > 100)
                {
                    itemoptions.Add(CreateOption(207, tiLe - 100));
                }

                break;
            case 567:
                chiso = 46000 * tiLe / 100;
                itemoptions.Add(CreateOption(23, chiso / 1000));
                itemoptions.Add(CreateOption(28, chiso * 150 / 1000));
                if (tiLe > 100)
                {
                    itemoptions.Add(CreateOption(207, tiLe - 100));
                }

                break;
            case 561:
                itemoptions.Add(CreateOption(14, 14 * tiLe / 100));
                break;
            default:
                break;
        }

        if (random.IsTrueInt(30, 100))
        {
            itemoptions.Add(CreateOption(options[random.NextInt(options.Length)], 0));
        }

        itemoptions.Add(CreateOption(21, random.NextInt(15, 17)));
        return new RolledDrop(idTempTL, itemoptions);
    }

    // Chọn ngẫu nhiên rand do tl; giữ nguyên điều kiện, thứ tự và số học của Java (Java: randDoTL).
    public RolledDrop RandDoTL()
    {
        short idTempTL;
        short type;
        short[] ao = [555, 557, 559];
        short[] quan = [556, 558, 560];
        short[] gang = [562, 564, 566];
        short[] giay = [563, 565, 567];
        short[] nhan = [561];
        short[] options = [30, 34, 35, 36, 86, 87, 208];
        if (random.IsTrueInt(10, 100))
        {
            idTempTL = nhan[0];
            type = 4;
        }
        else if (random.IsTrueInt(30, 100))
        {
            idTempTL = gang[random.NextInt(3)];
            type = 2;
        }
        else if (random.IsTrueInt(50, 100))
        {
            idTempTL = quan[random.NextInt(3)];
            type = 1;
        }
        else if (random.IsTrueInt(70, 100))
        {
            idTempTL = ao[random.NextInt(3)];
            type = 0;
        }
        else
        {
            idTempTL = giay[random.NextInt(3)];
            type = 3;
        }

        int tiLe = random.NextInt(100, 115);
        var itemoptions = new List<ItemOption>();
        int chiso;
        switch (type)
        {
            case 0:
                itemoptions.Add(CreateOption(47, random.NextInt(800, 900) * tiLe / 100));
                break;
            case 1:
                chiso = random.NextInt(46000, 49000) * tiLe / 100;
                itemoptions.Add(CreateOption(22, chiso / 1000));
                itemoptions.Add(CreateOption(27, chiso * 125 / 1000));
                break;
            case 2:
                itemoptions.Add(CreateOption(0, random.NextInt(4300, 4500) * tiLe / 100));
                break;
            case 3:
                chiso = random.NextInt(46000, 49000) * tiLe / 100;
                itemoptions.Add(CreateOption(23, chiso / 1000));
                itemoptions.Add(CreateOption(28, chiso * 125 / 1000));
                break;
            case 4:
                itemoptions.Add(CreateOption(14, random.NextInt(14, 17) * tiLe / 100));
                break;
        }

        if (random.IsTrueInt(90, 100))
        {
            itemoptions.Add(CreateOption(options[random.NextInt(options.Length)], 0));
        }

        itemoptions.Add(CreateOption(21, random.NextInt(15, 20)));
        return new RolledDrop(idTempTL, itemoptions);
    }

    // Lấy get default option tl; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getDefaultOptionTL).
    public List<ItemOption> GetDefaultOptionTL(int itemId)
    {
        var options = new List<ItemOption>();
        int tiLe = random.NextInt(100, 115);
        int cs;
        switch (itemId)
        {
            case 555:
                options.Add(CreateOption(47, 800 * tiLe / 100));
                break;
            case 557:
                options.Add(CreateOption(47, 850 * tiLe / 100));
                break;
            case 559:
                options.Add(CreateOption(47, 900 * tiLe / 100));
                break;
            case 556:
                cs = 52000 * tiLe / 100;
                options.Add(CreateOption(22, cs / 1000));
                options.Add(CreateOption(27, cs / 20));
                break;
            case 558:
                cs = 50000 * tiLe / 100;
                options.Add(CreateOption(22, cs / 1000));
                options.Add(CreateOption(27, cs / 20));
                break;
            case 560:
                cs = 48000 * tiLe / 100;
                options.Add(CreateOption(22, cs / 1000));
                options.Add(CreateOption(27, cs / 20));
                break;
            case 562:
                options.Add(CreateOption(0, 4400 * tiLe / 100));
                break;
            case 564:
                options.Add(CreateOption(0, 4300 * tiLe / 100));
                break;
            case 566:
                options.Add(CreateOption(0, 4500 * tiLe / 100));
                break;
            case 563:
                cs = 48000 * tiLe / 100;
                options.Add(CreateOption(23, cs / 1000));
                options.Add(CreateOption(28, cs / 20));
                break;
            case 565:
                cs = 50000 * tiLe / 100;
                options.Add(CreateOption(23, cs / 1000));
                options.Add(CreateOption(28, cs / 20));
                break;
            case 567:
                cs = 46000 * tiLe / 100;
                options.Add(CreateOption(23, cs / 1000));
                options.Add(CreateOption(28, cs * 150 / 1000));
                break;
            case 561:
                options.Add(CreateOption(14, 14 * tiLe / 100));
                break;
        }

        if (random.IsTrueInt(30, 100))
        {
            options.Add(CreateOption(random.NextInt(86, 87), 0));
        }

        options.Add(CreateOption(21, random.NextInt(15, 40)));
        return options;
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------
    // Tạo và sao chép vật phẩm
    // ---------------------------------------------------------------------------------------------------------------------------------------------------

    // an empty item (no template). (Java: createItemNull()).
    public Item CreateItemNull() => NewItem();

    // Xóa remove and add option template hsd; giữ nguyên điều kiện, thứ tự và số học của Java (Java: removeAndAddOptionTemplateHSD).
    private void RemoveAndAddOptionTemplateHSD(List<ItemOption> itemOptions, int removeId)
    {
        int id = 0;
        int param = 0;
        int[] randomDays = [1, 3, 5, 7, 14, 30];
        int chiSo = random.NextInt(randomDays.Length);
        bool shouldExecute = false;
        switch (removeId)
        {
            case 231:
                id = 93;
                param = randomDays[chiSo];
                shouldExecute = true;
                break;
            default:
                break;
        }

        if (shouldExecute && itemOptions.Any(io => io.OptionTemplate.Id == removeId))
        {
            itemOptions.RemoveAll(io => io.OptionTemplate.Id == removeId);
            if (random.IsTrueInt(random.NextInt(86, 90), random.NextInt(96, 100)))
            {
                itemOptions.Add(new ItemOption(CreateOption(id, param)));
            }
        }
    }

    // Xóa remove option template; giữ nguyên điều kiện, thứ tự và số học của Java (Java: removeOptionTemplate).
    private static void RemoveOptionTemplate(List<ItemOption> itemOptions, int removeId)
    {
        bool shouldExecute = false;
        switch (removeId)
        {
            case 76:
                shouldExecute = true;
                break;
            case 164:
                shouldExecute = true;
                break;
            case 249:
                shouldExecute = true;
                break;
            default:
                break;
        }

        if (shouldExecute && itemOptions.Any(io => io.OptionTemplate.Id == removeId))
        {
            itemOptions.RemoveAll(io => io.OptionTemplate.Id == removeId);
        }
    }

    // Xóa remove and add option template; giữ nguyên điều kiện, thứ tự và số học của Java (Java: removeAndAddOptionTemplate).
    private void RemoveAndAddOptionTemplate(List<ItemOption> itemOptions, int removeId)
    {
        if (removeId != 210)
        {
            return;
        }

        ItemOption? option210 = itemOptions.FirstOrDefault(io => io.OptionTemplate.Id == 210);
        if (option210 == null)
        {
            return;
        }

        int paramCount = option210.Param;
        int[] randomValues = [50, 77, 103, 14, 5, 94, 97, 108, 95, 96, 101, 204, 80, 81, 45, 46, 197];
        int[] availableIds = (int[])randomValues.Clone();
        random.ShuffleArray(availableIds);
        var newOptions = new List<ItemOption>();
        for (int j = 0; j < paramCount; j++)
        {
            int id;
            if (j < availableIds.Length)
            {
                id = availableIds[j];
            }
            else
            {
                id = randomValues[random.NextInt(randomValues.Length)];
            }

            int param;
            switch (id)
            {
                case 50:
                case 77:
                case 103:
                case 108:
                case 45:
                case 46:
                case 197:
                    param = random.NextInt(5, 10);
                    break;
                case 94:
                case 97:
                case 204:
                case 14:
                    param = random.NextInt(7, 18);
                    break;
                case 5:
                case 80:
                case 81:
                case 95:
                case 96:
                    param = random.NextInt(10, 20);
                    break;
                default:
                    param = random.NextInt(15, 30);
                    break;
            }

            newOptions.Add(CreateOption(id, param));
        }

        itemOptions.RemoveAll(io => io.OptionTemplate.Id == removeId);
        itemOptions.AddRange(newOptions);
    }

    // Tạo create item from item shop; giữ nguyên điều kiện, thứ tự và số học của Java (Java: createItemFromItemShop).
    public Item CreateItemFromItemShop(ItemTemplate temp, IEnumerable<ItemOption> shopOptions)
    {
        ArgumentNullException.ThrowIfNull(temp);
        ArgumentNullException.ThrowIfNull(shopOptions);
        Item item = NewItem();
        item.Template = temp;
        item.Quantity = 1;
        item.Content = item.GetContent();
        item.Info = item.GetInfo();
        foreach (ItemOption io in shopOptions)
        {
            item.ItemOptions.Add(new ItemOption(io));
            RemoveAndAddOptionTemplate(item.ItemOptions, new ItemOption(io).OptionTemplate.Id);
            RemoveAndAddOptionTemplateHSD(item.ItemOptions, new ItemOption(io).OptionTemplate.Id);
            RemoveOptionTemplate(item.ItemOptions, new ItemOption(io).OptionTemplate.Id);
        }

        return item;
    }

    // Tạo create item from item shop; giữ nguyên điều kiện, thứ tự và số học của Java.
    public Item CreateItemFromItemShop(ShopItemEntry itemShop)
    {
        ArgumentNullException.ThrowIfNull(itemShop);
        return CreateItemFromItemShop(itemShop.Template, itemShop.Options);
    }

    // Sao chép copy item; giữ nguyên điều kiện, thứ tự và số học của Java (Java: copyItem).
    public Item CopyItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var it = new Item(catalog)
        {
            ItemOptions = [],
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

    // one piece. (Java: createNewItem(tempId)).
    public Item CreateNewItem(short tempId) => CreateNewItem(tempId, 1);

    // Tạo create new item; giữ nguyên điều kiện, thứ tự và số học của Java (Java: createNewItem).
    public Item CreateNewItem(short tempId, int quantity)
    {
        Item item = NewItem();
        item.Template = GetTemplate(tempId);
        item.Quantity = quantity;
        item.CreateTime = clock.CurrentTimeMillis();
        item.Content = item.GetContent();
        item.Info = item.GetInfo();
        return item;
    }

    // Java createNewItemLock(tempId). (Java: createNewItemLock(tempId)).
    public Item CreateNewItemLock(int tempId) => CreateNewItemLock(tempId, 1);

    // Tạo create new item lock; giữ nguyên điều kiện, thứ tự và số học của Java (Java: createNewItemLock).
    public Item CreateNewItemLock(int tempId, int quantity)
    {
        Item item = NewItem();
        item.Template = GetTemplate(tempId);
        item.Quantity = quantity;
        item.CreateTime = clock.CurrentTimeMillis();
        item.Content = item.GetContent();
        item.Info = item.GetInfo();
        item.ItemOptions.Add(CreateOption(30, 1));
        return item;
    }

    // Java otpts(tempId). (Java: otpts(tempId)).
    public Item Otpts(short tempId) => Otpts(tempId, 1);

    // Thực hiện otpts; giữ nguyên điều kiện, thứ tự và số học của Java (Java: otpts).
    public Item Otpts(short tempId, int quantity)
    {
        Item item = NewItem();
        item.Template = GetTemplate(tempId);
        item.Quantity = quantity;
        item.CreateTime = clock.CurrentTimeMillis();
        if (item.RequiredTemplate.Type == 0)
        {
            item.ItemOptions.Add(CreateOption(21, 80));
            item.ItemOptions.Add(CreateOption(47, random.NextInt(2000, 2500)));
        }

        if (item.RequiredTemplate.Type == 1)
        {
            item.ItemOptions.Add(CreateOption(21, 80));
            item.ItemOptions.Add(CreateOption(22, random.NextInt(150, 200)));
        }

        if (item.RequiredTemplate.Type == 2)
        {
            item.ItemOptions.Add(CreateOption(21, 80));
            item.ItemOptions.Add(CreateOption(0, random.NextInt(18000, 20000)));
        }

        if (item.RequiredTemplate.Type == 3)
        {
            item.ItemOptions.Add(CreateOption(21, 80));
            item.ItemOptions.Add(CreateOption(23, random.NextInt(150, 200)));
        }

        if (item.RequiredTemplate.Type == 4)
        {
            item.ItemOptions.Add(CreateOption(21, 80));
            item.ItemOptions.Add(CreateOption(14, random.NextInt(20, 25)));
        }

        item.Content = item.GetContent();
        item.Info = item.GetInfo();
        return item;
    }

    // Java otptl(tempId). (Java: otptl(tempId)).
    public Item Otptl(short tempId) => Otptl(tempId, 1);

    // Thực hiện otptl; giữ nguyên điều kiện, thứ tự và số học của Java (Java: otptl).
    public Item Otptl(short tempId, int quantity)
    {
        Item item = NewItem();
        item.Template = GetTemplate(tempId);
        item.Quantity = quantity;
        item.CreateTime = clock.CurrentTimeMillis();
        if (item.RequiredTemplate.Type == 0)
        {
            item.ItemOptions.Add(CreateOption(21, 19));
            item.ItemOptions.Add(CreateOption(47, random.NextInt(800, 1200)));
        }

        if (item.RequiredTemplate.Type == 1)
        {
            item.ItemOptions.Add(CreateOption(21, 19));
            item.ItemOptions.Add(CreateOption(22, random.NextInt(24, 28)));
        }

        if (item.RequiredTemplate.Type == 2)
        {
            item.ItemOptions.Add(CreateOption(21, 19));
            item.ItemOptions.Add(CreateOption(0, random.NextInt(5500, 7800)));
        }

        if (item.RequiredTemplate.Type == 3)
        {
            item.ItemOptions.Add(CreateOption(21, 19));
            item.ItemOptions.Add(CreateOption(23, random.NextInt(23, 29)));
        }

        if (item.RequiredTemplate.Type == 4)
        {
            item.ItemOptions.Add(CreateOption(21, 19));
            item.ItemOptions.Add(CreateOption(14, random.NextInt(1, 14)));
        }

        item.Content = item.GetContent();
        item.Info = item.GetInfo();
        return item;
    }

    // Java otphd(tempId). (Java: otphd(tempId)).
    public Item Otphd(short tempId) => Otphd(tempId, 1);

    // Thực hiện otphd; giữ nguyên điều kiện, thứ tự và số học của Java (Java: otphd).
    public Item Otphd(short tempId, int quantity)
    {
        Item item = NewItem();
        item.Template = GetTemplate(tempId);
        item.Quantity = quantity;
        item.CreateTime = clock.CurrentTimeMillis();
        if (item.RequiredTemplate.Type == 0)
        {
            item.ItemOptions.Add(CreateOption(21, 80));
            item.ItemOptions.Add(CreateOption(47, random.NextInt(1200, 2100)));
        }

        if (item.RequiredTemplate.Type == 1)
        {
            item.ItemOptions.Add(CreateOption(21, 80));
            item.ItemOptions.Add(CreateOption(22, random.NextInt(60, 80)));
        }

        if (item.RequiredTemplate.Type == 2)
        {
            item.ItemOptions.Add(CreateOption(21, 80));
            item.ItemOptions.Add(CreateOption(0, random.NextInt(8500, 11000)));
        }

        if (item.RequiredTemplate.Type == 3)
        {
            item.ItemOptions.Add(CreateOption(21, 80));
            item.ItemOptions.Add(CreateOption(23, random.NextInt(59, 82)));
        }

        if (item.RequiredTemplate.Type == 4)
        {
            item.ItemOptions.Add(CreateOption(21, 80));
            item.ItemOptions.Add(CreateOption(14, random.NextInt(5, 18)));
        }

        item.Content = item.GetContent();
        item.Info = item.GetInfo();
        return item;
    }

    // Tạo create item set kich hoat; giữ nguyên điều kiện, thứ tự và số học của Java (Java: createItemSetKichHoat).
    public Item CreateItemSetKichHoat(int tempId, int quantity)
    {
        Item item = NewItem();
        item.Template = GetTemplate(tempId);
        item.Quantity = quantity;
        item.ItemOptions = CreateItemNull().ItemOptions;
        item.CreateTime = clock.CurrentTimeMillis();
        item.Content = item.GetContent();
        item.Info = item.GetInfo();
        return item;
    }

    // Tạo create item do huy diet; giữ nguyên điều kiện, thứ tự và số học của Java (Java: createItemDoHuyDiet).
    public Item CreateItemDoHuyDiet(int tempId, int quantity)
    {
        Item item = NewItem();
        item.Template = GetTemplate(tempId);
        item.Quantity = quantity;
        item.ItemOptions = CreateItemNull().ItemOptions;
        item.CreateTime = clock.CurrentTimeMillis();
        item.Content = item.GetContent();
        item.Info = item.GetInfo();
        return item;
    }

    // Tạo create item from item map; giữ nguyên điều kiện, thứ tự và số học của Java (Java: createItemFromItemMap).
    public Item CreateItemFromItemMap(short templateId, int quantity, List<ItemOption> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        Item item = CreateNewItem(templateId, quantity);
        item.ItemOptions = options;
        return item;
    }

    // Kiểm tra is item activation; giữ nguyên điều kiện, thứ tự và số học của Java (Java: isItemActivation).
    public bool IsItemActivation(Item item) => false;

    // Lấy get percent train armor; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getPercentTrainArmor).
    public int GetPercentTrainArmor(Item? item)
    {
        if (item == null)
        {
            return 0;
        }

        switch (item.RequiredTemplate.Id)
        {
            case 529:
            case 534:
                return 10;
            case 530:
            case 535:
                return 20;
            case 531:
            case 536:
                return 30;
            case 1797:
                return 40;
            default:
                return 0;
        }
    }

    // Java isTrainArmor(item). (Java: isTrainArmor(item)).
    public bool IsTrainArmor(Item? item)
    {
        if (item == null)
        {
            return false;
        }

        switch (item.RequiredTemplate.Id)
        {
            case 529:
            case 534:
            case 530:
            case 535:
            case 531:
            case 536:
            case 1797:
                return true;
            default:
                return false;
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------
    // Hạn dùng
    // ---------------------------------------------------------------------------------------------------------------------------------------------------

    // Cập nhật trực tiếp các tùy chọn hạn 93/260/261 rồi báo hết hạn; TimeUtil lấy chênh lệch tuyệt đối, giờ modulo 24 và phút modulo 60, CreateTime có thể đổi trong vòng lặp còn tùy chọn hết hạn không đổi mốc, đúng Java (Java: isOutOfDateTime).
    public bool IsOutOfDateTime(Item? item)
    {
        if (item == null)
        {
            return false;
        }

        bool expired = false;
        foreach (ItemOption io in item.ItemOptions)
        {
            long now = clock.CurrentTimeMillis();
            switch (io.OptionTemplate.Id)
            {
                case 93:
                {
                    int dayPassed = (int)TimeUtil.DiffDate(now, item.CreateTime, TimeUtil.DAY);
                    if (dayPassed > 0)
                    {
                        io.Param -= dayPassed;
                        if (io.Param <= 0)
                        {
                            expired = true;
                        }
                        else
                        {
                            item.CreateTime = now;
                        }
                    }

                    break;
                }

                case 260:
                {
                    int hoursPassed = (int)TimeUtil.DiffDate(now, item.CreateTime, TimeUtil.HOUR);
                    if (hoursPassed > 0)
                    {
                        io.Param -= hoursPassed;
                        if (io.Param <= 0)
                        {
                            expired = true;
                        }
                        else
                        {
                            item.CreateTime = now;
                        }
                    }

                    break;
                }

                case 261:
                {
                    int minutesPassed = (int)TimeUtil.DiffDate(now, item.CreateTime, TimeUtil.MINUTE);
                    if (minutesPassed > 0)
                    {
                        io.Param -= minutesPassed;
                        if (io.Param <= 0)
                        {
                            expired = true;
                        }
                        else
                        {
                            item.CreateTime = now;
                        }
                    }

                    break;
                }

                default:
                    break;
            }
        }

        return expired;
    }

    // Cập nhật hạn vật phẩm lúc nạp và gửi lại túi; giữ lỗi đặc thù Java: sau khi hết hạn chỉ gán Item rỗng cho biến cục bộ, các tùy chọn còn lại của vật phẩm thật không còn được cập nhật theo cùng mốc (Java: loadItemTimeStatus).
    public void LoadItemTimeStatus(Player player, Item? item)
    {
        if (item == null)
        {
            return;
        }

        long now = clock.CurrentTimeMillis();
        Item current = item;
        List<ItemOption> options = item.ItemOptions;
        foreach (ItemOption io in options)
        {
            switch (io.OptionTemplate.Id)
            {
                case 93:
                {
                    int dayPassed = (int)TimeUtil.DiffDate(now, current.CreateTime, TimeUtil.DAY);
                    if (dayPassed > 0)
                    {
                        io.Param -= dayPassed;
                        if (io.Param < 0)
                        {
                            io.Param = 0;
                        }

                        current.CreateTime = now;
                    }

                    break;
                }

                case 260:
                {
                    int hoursPassed = (int)TimeUtil.DiffDate(now, current.CreateTime, TimeUtil.HOUR);
                    if (hoursPassed > 0)
                    {
                        io.Param -= hoursPassed;
                        if (io.Param < 0)
                        {
                            io.Param = 0;
                        }

                        current.CreateTime = now;
                    }

                    break;
                }

                case 261:
                {
                    int minutesPassed = (int)TimeUtil.DiffDate(now, current.CreateTime, TimeUtil.MINUTE);
                    if (minutesPassed > 0)
                    {
                        io.Param -= minutesPassed;
                        if (io.Param < 0)
                        {
                            io.Param = 0;
                        }

                        current.CreateTime = now;
                    }

                    break;
                }

                default:
                    break;
            }

            if (IsOutOfDateTime(current))
            {
                current = CreateItemNull();
            }
        }

        host.SendItemBag(player);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------
    // Trang bị kích hoạt bộ, Hủy Diệt và Thiên Sứ
    // ---------------------------------------------------------------------------------------------------------------------------------------------------

    // Chọn ngẫu nhiên random skhid; giữ nguyên điều kiện, thứ tự và số học của Java (Java: randomSKHId).
    public int RandomSKHId(sbyte gender)
    {
        if (gender == 3)
        {
            gender = 2;
        }

        int[][] options =
        [
            [128, 129, 127],
            [131, 132, 130],
            [133, 135, 134],
        ];
        int skhv1 = 25;
        int skhv2 = 35;
        int skhc = 40;
        int skhId = -1;
        int rd = random.NextInt(1, 100);
        if (rd <= skhv1)
        {
            skhId = 0;
        }
        else if (rd <= skhv1 + skhv2)
        {
            skhId = 1;
        }
        else if (rd <= skhv1 + skhv2 + skhc)
        {
            skhId = 2;
        }

        if (gender == 0 && skhId == 0 && random.IsTrueInt(50, 100))
        {
            return 214;
        }

        return options[gender][skhId];
    }

    // Thực hiện item skh; giữ nguyên điều kiện, thứ tự và số học của Java (Java: itemSKH).
    public Item ItemSKH(int itemId, int skhId)
    {
        Item item = CreateItemSetKichHoat(itemId, 1);
        item.ItemOptions.AddRange(GetListOptionItemShop((short)itemId));
        item.ItemOptions.Add(CreateOption(skhId, 1));
        item.ItemOptions.Add(CreateOption(OptionIdSKH(skhId), 1));
        item.ItemOptions.Add(CreateOption(30, 1));
        return item;
    }

    // Thực hiện option id skh; giữ nguyên điều kiện, thứ tự và số học của Java (Java: optionIdSKH).
    public int OptionIdSKH(int skhId)
    {
        switch (skhId)
        {
            case 127:
                return 139;
            case 128:
                return 140;
            case 129:
                return 141;
            case 130:
                return 143;
            case 131:
                return 254;
            case 132:
                return 144;
            case 133:
                return 136;
            case 134:
                return 137;
            case 135:
                return 138;
            case 250:
                return 253;
            case 251:
                return 254;
            case 255:
                return 256;
        }

        return 0;
    }

    // Thực hiện item dhd; giữ nguyên điều kiện, thứ tự và số học của Java (Java: itemDHD).
    public Item ItemDHD(int itemId, int dhdId)
    {
        Item item = CreateItemSetKichHoat(itemId, 1);
        item.ItemOptions.Add(CreateOption(dhdId, 1));
        item.ItemOptions.Add(CreateOption(OptionIdDHD(dhdId), 1));
        item.ItemOptions.Add(CreateOption(30, 1));
        return item;
    }

    // Thực hiện option id dhd; giữ nguyên điều kiện, thứ tự và số học của Java (Java: optionIdDHD).
    public int OptionIdDHD(int skhId)
    {
        switch (skhId)
        {
            case 127:
                return 139;
            case 128:
                return 140;
            case 129:
                return 141;
            case 130:
                return 143;
            case 131:
                return 254;
            case 132:
                return 144;
            case 133:
                return 136;
            case 134:
                return 137;
            case 135:
                return 138;
            case 250:
                return 253;
            case 251:
                return 254;
            case 255:
                return 256;
        }

        return 0;
    }

    // Chọn ngẫu nhiên random cs_dhd; giữ nguyên điều kiện, thứ tự và số học của Java (Java: randomCS_DHD).
    public Item RandomCS_DHD(int itemId, int gender)
    {
        Item it = CreateItemSetKichHoat(itemId, 1);
        int[] ao = [650, 652, 654];
        int[] quan = [651, 653, 655];
        int[] gang = [657, 659, 661];
        int[] giay = [658, 660, 662];
        int nhd = 656;
        if (ao.Contains(itemId))
        {
            it.ItemOptions.Add(CreateOption(47, Util.HighlightsItem(gender == 2, random.NextInt(1001) + 1800)));
        }

        if (quan.Contains(itemId))
        {
            it.ItemOptions.Add(CreateOption(22, Util.HighlightsItem(gender == 0, random.NextInt(16) + 85)));
        }

        if (gang.Contains(itemId))
        {
            it.ItemOptions.Add(CreateOption(0, Util.HighlightsItem(gender == 2, random.NextInt(150) + 8500)));
        }

        if (giay.Contains(itemId))
        {
            it.ItemOptions.Add(CreateOption(23, Util.HighlightsItem(gender == 1, random.NextInt(11) + 80)));
        }

        if (nhd == itemId)
        {
            it.ItemOptions.Add(CreateOption(14, random.NextInt(3) + 17));
        }

        it.ItemOptions.Add(CreateOption(21, 80));
        it.ItemOptions.Add(CreateOption(30, 1));
        return it;
    }

    // Mở open dts; giữ nguyên điều kiện, thứ tự và số học của Java (Java: openDTS).
    public void OpenDTS(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        IReadOnlyList<Item> itemsCombine = host.GetCombineItems(player);
        if (itemsCombine.Count(item => item.RequiredTemplate.Id >= 555 && item.RequiredTemplate.Id <= 567) < 1)
        {
            host.SendNotification(player, "Thiếu đồ thần linh");
            return;
        }

        if (itemsCombine.Count(item => item.RequiredTemplate.Id >= 650 && item.RequiredTemplate.Id <= 662) < 2)
        {
            host.SendNotification(player, "Thiếu đồ hủy diệt");
            return;
        }

        if (itemsCombine.Count != 3)
        {
            host.SendNotification(player, "Thiếu đồ");
            return;
        }

        if (host.GetCountEmptyBag(player) > 0)
        {
            Item itemTL = itemsCombine.First(item => item.RequiredTemplate.Id >= 555 && item.RequiredTemplate.Id <= 567);
            List<Item> itemHDs = itemsCombine.Where(item => item.RequiredTemplate.Id >= 650 && item.RequiredTemplate.Id <= 662).ToList();
            short[][] itemIds =
            [
                [1048, 1051, 1054, 1057, 1060],
                [1049, 1052, 1055, 1058, 1061],
                [1050, 1053, 1056, 1059, 1062],
            ];
            Item itemTS = DoThienSu(itemIds[player.Gender][itemTL.RequiredTemplate.Type], player.Gender);
            host.AddItemBag(player, itemTS);
            host.SubQuantityItemsBag(player, itemTL, 1);
            foreach (Item item in itemHDs)
            {
                host.SubQuantityItemsBag(player, item, 1);
            }

            host.SendItemBag(player);
            host.SendNotification(player, "Bạn đã nhận được " + itemTS.RequiredTemplate.Name);
        }
        else
        {
            host.SendNotification(player, "Bạn phải có ít nhất 1 ô trống hành trang");
        }
    }

    // Thực hiện do thien su; giữ nguyên điều kiện, thứ tự và số học của Java (Java: DoThienSu).
    public Item DoThienSu(int itemId, int gender)
    {
        Item dots = CreateItemSetKichHoat(itemId, 1);
        int[] ao = [1048, 1049, 1050];
        int[] quan = [1051, 1052, 1053];
        int[] gang = [1054, 1055, 1056];
        int[] giay = [1057, 1058, 1059];
        int[] nhan = [1060, 1061, 1062];
        if (ao.Contains(itemId))
        {
            dots.AddOptionParam(47, Util.HighlightsItem(gender == 2, random.NextInt(1201) + 2800));
        }

        if (random.IsTrueInt(80, 100))
        {
            if (quan.Contains(itemId))
            {
                dots.AddOptionParam(22, Util.HighlightsItem(gender == 0, random.NextInt(11) + 120));
            }
        }
        else
        {
            if (quan.Contains(itemId))
            {
                dots.AddOptionParam(22, Util.HighlightsItem(gender == 0, random.NextInt(21) + 130));
            }
        }

        if (random.IsTrueInt(80, 100))
        {
            if (gang.Contains(itemId))
            {
                dots.AddOptionParam(0, Util.HighlightsItem(gender == 2, random.NextInt(651) + 9350));
            }
        }
        else
        {
            if (gang.Contains(itemId))
            {
                dots.AddOptionParam(0, Util.HighlightsItem(gender == 2, random.NextInt(1001) + 10000));
            }
        }

        if (random.IsTrueInt(80, 100))
        {
            if (giay.Contains(itemId))
            {
                dots.AddOptionParam(23, Util.HighlightsItem(gender == 1, random.NextInt(21) + 90));
            }
        }
        else
        {
            if (giay.Contains(itemId))
            {
                dots.AddOptionParam(23, Util.HighlightsItem(gender == 1, random.NextInt(21) + 110));
            }
        }

        if (nhan.Contains(itemId))
        {
            dots.AddOptionParam(14, Util.HighlightsItem(gender == 1, random.NextInt(3) + 18));
        }

        dots.AddOptionParam(21, 120);
        dots.AddOptionParam(30, 1);
        return dots;
    }

    // Lấy get angel item; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getAngelItem).
    public Item GetAngelItem(int gender, int type)
    {
        int tempId = 1048 + (type * 3) + gender;
        Item angelItem = CreateNewItem((short)tempId);
        foreach (ItemOption io in GetListOptionItemShop(14, type == 4 ? 3 : gender, type))
        {
            if (io.IsOptionCanUpgrade())
            {
                int p = JavaNumbers.ToInt(io.Param * 1.2);
                angelItem.ItemOptions.Add(CreateOption(io.OptionTemplate.Id, p));
            }
        }

        int param;
        switch (type)
        {
            case 0:
                param = 62;
                break;
            case 1:
                param = 66;
                break;
            case 2:
                param = 70;
                break;
            case 3:
                param = 64;
                break;
            default:
                param = 68;
                break;
        }

        angelItem.ItemOptions.Add(CreateOption(21, param));
        angelItem.ItemOptions.Add(CreateOption(30, 0));
        return angelItem;
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------
    // Tra cứu cửa hàng
    // ---------------------------------------------------------------------------------------------------------------------------------------------------

    // Lấy get list option item shop; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getListOptionItemShop).
    public List<ItemOption> GetListOptionItemShop(int level, int gender, int type)
    {
        var list = new List<ItemOption>();
        foreach (ShopItemEntry itemShop in catalog.ShopItems)
        {
            if (itemShop.Template.Level == level && itemShop.Template.Gender == gender && itemShop.Template.Type == type && list.Count == 0)
            {
                foreach (ItemOption io in itemShop.Options)
                {
                    list.Add(CreateOption(io.OptionTemplate.Id, io.Param));
                }
            }
        }

        return list;
    }

    // see . (Java: getOptionParamItemShop(id, optionId)).
    public int GetOptionParamItemShop(short id, int optionId) => catalog.GetOptionParamItemShop(id, optionId);

    // Lấy get list option item shop; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getListOptionItemShop).
    public List<ItemOption> GetListOptionItemShop(short id)
    {
        var list = new List<ItemOption>();
        foreach (ShopItemEntry itemShop in catalog.ShopItems)
        {
            if (itemShop.Template.Id == id && list.Count == 0)
            {
                foreach (ItemOption io in itemShop.Options)
                {
                    list.Add(new ItemOption(io));
                }
            }
        }

        return list;
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------
    // Chọn ngẫu nhiên trang bị kích hoạt
    // ---------------------------------------------------------------------------------------------------------------------------------------------------

    private static readonly int[][][] KichHoatItems =
    [
        [[0, 0], [1, 1], [2, 2], [0, 33], [1, 41], [2, 49], [0, 3], [1, 4], [2, 5]],
        [[0, 6], [1, 7], [2, 8], [0, 35], [1, 43], [2, 51], [0, 9], [1, 10], [2, 11]],
        [[0, 21], [1, 22], [2, 23], [0, 24], [1, 46], [2, 53], [0, 37], [1, 25], [2, 26]],
        [[0, 27], [1, 28], [2, 29], [0, 30], [1, 47], [2, 55], [0, 39], [1, 31], [2, 32]],
        [[3, 12], [3, 57], [3, 58], [3, 59]],
    ];

    private static readonly int[][][] KichHoatItemsVip =
    [
        [[0, 0], [1, 1], [2, 2], [0, 33], [1, 41], [2, 49], [0, 3], [1, 4], [2, 5], [0, 34], [0, 136], [0, 137], [0, 138], [0, 139], [0, 230], [0, 231], [0, 232], [0, 233], [1, 42], [1, 152], [1, 153], [1, 154], [1, 155], [1, 234], [1, 235], [1, 236], [1, 237], [2, 50], [2, 168], [2, 169], [2, 170], [2, 171], [2, 238], [2, 239], [2, 240], [2, 241]],
        [[0, 6], [1, 7], [2, 8], [0, 35], [1, 43], [2, 51], [0, 9], [1, 10], [2, 11], [0, 36], [0, 140], [0, 141], [0, 142], [0, 143], [0, 242], [0, 243], [0, 244], [0, 245], [1, 44], [1, 156], [1, 157], [1, 158], [1, 159], [1, 246], [1, 247], [1, 248], [1, 249], [2, 52], [2, 172], [2, 173], [2, 174], [2, 175], [2, 250], [2, 251], [2, 252], [2, 253]],
        [[0, 21], [1, 22], [2, 23], [0, 24], [1, 46], [2, 53], [0, 37], [1, 25], [2, 26], [0, 38], [0, 144], [0, 145], [0, 146], [0, 147], [0, 254], [0, 255], [0, 256], [0, 257], [1, 45], [1, 160], [1, 161], [1, 162], [1, 163], [1, 258], [1, 259], [1, 260], [1, 261], [2, 54], [2, 176], [2, 177], [2, 178], [2, 179], [2, 262], [2, 263], [2, 264], [2, 265]],
        [[0, 27], [1, 28], [2, 29], [0, 30], [1, 47], [2, 55], [0, 39], [1, 31], [2, 32], [0, 40], [0, 148], [0, 149], [0, 150], [0, 151], [0, 266], [0, 267], [0, 268], [0, 269], [1, 48], [1, 164], [1, 165], [1, 166], [1, 167], [1, 270], [1, 271], [1, 272], [1, 273], [2, 56], [2, 180], [2, 181], [2, 182], [2, 183], [2, 274], [2, 275], [2, 276], [2, 277]],
        [[3, 12], [3, 57], [3, 58], [3, 59], [3, 184], [3, 185], [3, 186], [3, 187], [3, 278], [3, 279], [3, 280], [3, 281]],
    ];

    private int PickKichHoat(int[][][] items, int gender)
    {
        for (int j = 0; j < 1; j++)
        {
            int type = random.NextInt(items.Length);
            var candidates = new List<int>();
            foreach (int[] pair in items[type])
            {
                if (pair[0] == gender || pair[0] == 3)
                {
                    candidates.Add(pair[1]);
                }
            }

            if (candidates.Count != 0)
            {
                return candidates[random.NextInt(candidates.Count)];
            }
        }

        return 0;
    }

    // Chọn ngẫu nhiên rand temp item kich hoat; giữ nguyên điều kiện, thứ tự và số học của Java (Java: randTempItemKichHoat).
    public int RandTempItemKichHoat(int gender) => PickKichHoat(KichHoatItems, gender);

    // Chọn ngẫu nhiên rand temp item kich hoat_vip; giữ nguyên điều kiện, thứ tự và số học của Java (Java: randTempItemKichHoat_VIP).
    public int RandTempItemKichHoat_VIP(int gender) => PickKichHoat(KichHoatItemsVip, gender);

    // Chọn ngẫu nhiên rand option item kich hoat; giữ nguyên điều kiện, thứ tự và số học của Java (Java: randOptionItemKichHoat).
    public int[] RandOptionItemKichHoat(int gender)
    {
        int[][] options;
        switch (gender)
        {
            case 0:
                options =
                [
                    [128, 140],
                    [127, 139],
                    [129, 141],
                    [233, 234],
                    [250, 253],
                    [263, 264],
                    [265, 266],
                    [267, 268],
                ];
                break;
            case 1:
                options =
                [
                    [130, 143],
                    [131, 254],
                    [132, 144],
                    [233, 234],
                    [251, 142],
                    [263, 264],
                    [265, 266],
                    [267, 268],
                ];
                break;
            default:
                options =
                [
                    [134, 137],
                    [135, 138],
                    [133, 136],
                    [233, 234],
                    [252, 255],
                    [263, 264],
                    [265, 266],
                    [267, 268],
                ];
                break;
        }

        return options[random.NextInt(options.Length)];
    }

    // Chọn ngẫu nhiên rand option item kich hoat new; giữ nguyên điều kiện, thứ tự và số học của Java (Java: randOptionItemKichHoatNew).
    public int[] RandOptionItemKichHoatNew(int gender)
    {
        int op1;
        int op2;
        int op3;
        int op4;
        switch (gender)
        {
            case 0:
                op1 = 245;
                op2 = 246;
                op3 = 247;
                op4 = 248;
                break;
            case 1:
                op1 = 237;
                op2 = 238;
                op3 = 239;
                op4 = 240;
                break;
            case 2:
                op1 = 241;
                op2 = 242;
                op3 = 243;
                op4 = 244;
                break;
            default:
                op1 = 269;
                op2 = 270;
                op3 = 271;
                op4 = 272;
                break;
        }

        return [op1, op2, op3, op4];
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------
    // Gói phần thưởng (settl* / sethd*)
    // ---------------------------------------------------------------------------------------------------------------------------------------------------

    // Nhận claim set reward; giữ nguyên điều kiện, thứ tự và số học của Java.
    public void ClaimSetReward(Player player, SetRewardDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(definition);
        Item? hq = host.FindBagItem(player, definition.PackItemId);
        int pick = random.NextInt(definition.CandidateIds.Length);
        short pieceId = (short)definition.CandidateIds[pick];
        Item ao = definition.Kind == SetRewardKind.ThanLinh ? Otptl(pieceId) : Otphd(pieceId);
        ao.AddOptionParam(definition.SetOptionId, 0);
        ao.AddOptionParam(definition.BonusOptionId, 0);
        ao.AddOptionParam(30, 0);
        if (host.GetCountEmptyBag(player) > definition.EmptySlotsExclusiveMin)
        {
            host.AddItemBag(player, ao);
            host.SendItemBag(player);
            host.SendNotification(player, definition.Message);
            host.SubQuantityItemsBag(player, hq, 1);
            host.SendItemBag(player);
        }
        else
        {
            host.SendNotification(player, "Bạn phải có ít nhất 1 ô trống hành trang");
        }
    }

    // Nhận claim set reward; giữ nguyên điều kiện, thứ tự và số học của Java.
    public void ClaimSetReward(Player player, string javaMethodName)
    {
        ArgumentNullException.ThrowIfNull(javaMethodName);
        SetRewardDefinition definition = SetRewardDefinition.All.FirstOrDefault(d => d.JavaName == javaMethodName)
            ?? throw new ArgumentException("unknown set reward " + javaMethodName, nameof(javaMethodName));
        ClaimSetReward(player, definition);
    }
}
