using System.Collections.ObjectModel;
using Nro.Data.Models.Shop;
using Nro.Game.Content;

namespace Nro.Content.Catalogs;

// Danh mục cây cửa hàng đóng băng sau khi kiểm ràng buộc. Nguồn Java: jbcd/dao/ShopDAO.java (dòng 20–132).
public sealed class ShopCatalog : IShopCatalog
{
    private readonly IReadOnlyList<ShopDefinition> shops;
    private readonly IReadOnlyDictionary<int, ShopDefinition> byId;
    private readonly IReadOnlyDictionary<int, ShopDefinition> byNpc;

    // Đóng băng toàn bộ cây shop để không chia sẻ collection mutable. (Java: SHOPS)
    public ShopCatalog(IEnumerable<ShopDefinition> shops)
    {
        ArgumentNullException.ThrowIfNull(shops);
        this.shops = Array.AsReadOnly(shops.Select(Clone).ToArray());
        byId = new ReadOnlyDictionary<int, ShopDefinition>(this.shops.ToDictionary(value => value.Id));
        byNpc = new ReadOnlyDictionary<int, ShopDefinition>(this.shops.GroupBy(value => (int)value.NpcId).ToDictionary(group => group.Key, group => group.First()));
    }

    public IReadOnlyList<ShopDefinition> Shops => shops;

    public ShopDefinition? GetByNpc(int npcId) => byNpc.TryGetValue(npcId, out ShopDefinition? value) ? value : null;

    public ShopDefinition Get(int id) => byId.TryGetValue(id, out ShopDefinition? value)
        ? value
        : throw new KeyNotFoundException($"Không có cửa hàng id {id}.");

    private static ShopDefinition Clone(ShopDefinition shop) => new(shop.Id, shop.NpcId, shop.TagName, shop.TypeShop,
        Array.AsReadOnly(shop.Tabs.Select(tab => new ShopTabDefinition(tab.Id, tab.Name,
            Array.AsReadOnly(tab.Items.Select(item => new ShopItemDefinition(item.Id, item.TemplateId, item.IsNew, item.Cost, item.CostGold, item.IconSpec, item.TypeSell,
                Array.AsReadOnly(item.Options.Select(option => new ShopItemOptionDefinition(option.OptionId, option.Param)).ToArray()))).ToArray()))).ToArray()));
}
