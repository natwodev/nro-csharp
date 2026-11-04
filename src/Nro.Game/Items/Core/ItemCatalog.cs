using Nro.Core.Templates;

namespace Nro.Game.Items.Core;

// Lưu các bảng mẫu vật phẩm, tùy chọn và mục cửa hàng chỉ đọc để tra cứu theo đúng thứ tự tải.
// Nguồn Java: Manager.ITEM_TEMPLATES, ITEM_OPTION_TEMPLATES, SHOPS (dữ liệu được dựng một lần rồi chỉ đọc nên chia sẻ đa luồng không cần khóa; tra theo vị trí như Java, vị trí sai kể cả -1 ném ArgumentOutOfRangeException thay IndexOutOfBoundsException).
public sealed class ItemCatalog
{
    private readonly ItemTemplate[] itemTemplates;
    private readonly ItemOptionTemplate[] itemOptionTemplates;
    private readonly ShopItemEntry[] shopItems;

    // Thực hiện thành viên; giữ nguyên điều kiện, thứ tự và số học của Java.
    public ItemCatalog(IEnumerable<ItemTemplate> itemTemplates, IEnumerable<ItemOptionTemplate> itemOptionTemplates, IEnumerable<ShopItemEntry>? shopItems = null)
    {
        ArgumentNullException.ThrowIfNull(itemTemplates);
        ArgumentNullException.ThrowIfNull(itemOptionTemplates);
        this.itemTemplates = itemTemplates.ToArray();
        this.itemOptionTemplates = itemOptionTemplates.ToArray();
        this.shopItems = shopItems?.ToArray() ?? [];
    }

    // Java Manager.ITEM_TEMPLATES. (Java: Manager.ITEM_TEMPLATES).
    public IReadOnlyList<ItemTemplate> ItemTemplates => itemTemplates;

    // Java Manager.ITEM_OPTION_TEMPLATES. (Java: Manager.ITEM_OPTION_TEMPLATES).
    public IReadOnlyList<ItemOptionTemplate> ItemOptionTemplates => itemOptionTemplates;

    // Every shop item in Java iteration order (see ).
    public IReadOnlyList<ShopItemEntry> ShopItems => shopItems;

    // Lấy get template; giữ nguyên điều kiện, thứ tự và số học của Java (Java: ItemService.getTemplate).
    public ItemTemplate GetTemplate(int id)
    {
        if ((uint)id >= (uint)itemTemplates.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(id), id, "no item template at this position (Java: IndexOutOfBoundsException)");
        }

        return itemTemplates[id];
    }

    // Lấy get item option template; giữ nguyên điều kiện, thứ tự và số học của Java (Java: ItemService.getItemOptionTemplate).
    public ItemOptionTemplate GetItemOptionTemplate(int id)
    {
        if ((uint)id >= (uint)itemOptionTemplates.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(id), id, "no item option template at this position (Java: IndexOutOfBoundsException)");
        }

        return itemOptionTemplates[id];
    }

    // Lấy get item id by icon; giữ nguyên điều kiện, thứ tự và số học của Java (Java: ItemService.getItemIdByIcon).
    public short GetItemIdByIcon(short iconId)
    {
        for (int j = 0; j < itemTemplates.Length; j++)
        {
            if (itemTemplates[j].IconID == iconId)
            {
                return itemTemplates[j].Id;
            }
        }

        return -1;
    }

    // Lấy get option param item shop; giữ nguyên điều kiện, thứ tự và số học của Java (Java: ItemService.getOptionParamItemShop).
    public int GetOptionParamItemShop(short id, int optionId)
    {
        foreach (ShopItemEntry itemShop in shopItems)
        {
            if (itemShop.Template.Id != id)
            {
                continue;
            }

            foreach (ItemOption itemOption in itemShop.Options)
            {
                if (itemOption.OptionTemplate.Id == optionId)
                {
                    return itemOption.Param;
                }
            }
        }

        return -1;
    }
}
