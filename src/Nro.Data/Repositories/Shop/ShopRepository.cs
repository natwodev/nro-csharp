using Nro.Data.Db;

namespace Nro.Data.Repositories.Shop;

// Tải cấu trúc chỉ đọc cửa hàng, tab, vật phẩm và tùy chọn từ cơ sở dữ liệu.
// Nguồn Java: jbcd/dao/ShopDAO.java (chỉ dựng cây dữ liệu; mẫu vật phẩm vẫn là trách nhiệm Nro.Game, các truy vấn cố định có tham số).
public sealed class ShopRepository : IShopRepository
{
    private const string ShopsSql = "select id, npc_id, tag_name, type_shop from shop order by npc_id asc";
    private const string TabsSql = "select id, name from tab_shop where shop_id = ? order by id";
    private const string ItemsSql = "select id, temp_id, is_new, cost, costgold, icon_spec, type_sell from item_shop where is_sell = true and tab_id = ? order by create_time desc";
    private const string OptionsSql = "select option_id, param from item_shop_option where item_shop_id = ? order by id";
    private readonly NroDatabase _database;

    // Tạo kho cửa hàng với truy cập cơ sở dữ liệu.
    public ShopRepository(NroDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    // Tải toàn bộ cây cửa hàng theo thứ tự truy vấn đã quy định (Java: loadShop).
    public async Task<IReadOnlyList<ShopDefinition>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var shops = new List<ShopDefinition>();
        using IResultSnapshot shopRows = await _database.ExecuteQueryAsync(ShopsSql, cancellationToken);
        while (shopRows.Next())
        {
            int id = shopRows.GetInt("id");
            shops.Add(new ShopDefinition(id, shopRows.GetByte("npc_id"), shopRows.GetString("tag_name"), shopRows.GetByte("type_shop"), await LoadTabsAsync(id, cancellationToken)));
        }

        return shops;
    }

    private async Task<IReadOnlyList<ShopTabDefinition>> LoadTabsAsync(int shopId, CancellationToken cancellationToken)
    {
        var tabs = new List<ShopTabDefinition>();
        using IResultSnapshot tabRows = await _database.ExecuteQueryAsync(TabsSql, [shopId], cancellationToken);
        while (tabRows.Next())
        {
            int id = tabRows.GetInt("id");
            tabs.Add(new ShopTabDefinition(id, tabRows.GetString("name").Replace("<>", "\n", StringComparison.Ordinal), await LoadItemsAsync(id, cancellationToken)));
        }

        return tabs;
    }

    private async Task<IReadOnlyList<ShopItemDefinition>> LoadItemsAsync(int tabId, CancellationToken cancellationToken)
    {
        var items = new List<ShopItemDefinition>();
        using IResultSnapshot itemRows = await _database.ExecuteQueryAsync(ItemsSql, [tabId], cancellationToken);
        while (itemRows.Next())
        {
            int id = itemRows.GetInt("id");
            items.Add(new ShopItemDefinition(id, itemRows.GetShort("temp_id"), itemRows.GetBoolean("is_new"), itemRows.GetInt("cost"), itemRows.GetInt("costgold"), itemRows.GetInt("icon_spec"), itemRows.GetByte("type_sell"), await LoadOptionsAsync(id, cancellationToken)));
        }

        return items;
    }

    private async Task<IReadOnlyList<ShopItemOptionDefinition>> LoadOptionsAsync(int itemId, CancellationToken cancellationToken)
    {
        var options = new List<ShopItemOptionDefinition>();
        using IResultSnapshot optionRows = await _database.ExecuteQueryAsync(OptionsSql, [itemId], cancellationToken);
        while (optionRows.Next())
        {
            options.Add(new ShopItemOptionDefinition(optionRows.GetInt("option_id"), optionRows.GetInt("param")));
        }

        return options;
    }
}
