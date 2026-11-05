using Nro.Content.Catalogs;
using Nro.Content.Loaders;
using Nro.Core.Content;
using Nro.Core.Templates;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;

namespace Nro.Tests.Unit.Content.CommerceBadgesNotificationsRadar;

// Kiểm thử J-01D bằng hàng trong bộ nhớ, không mở DB. Nguồn Java: nro/server/Manager.java (dòng 723–810, 953–994).
public sealed class CommerceBadgeNotificationRadarCatalogStepsTests
{
    [Fact]
    public async Task Loads_immutable_shop_tree_badges_notifications_and_nullable_radar()
    {
        var loader = new StaticCatalogLoader(CommerceBadgeNotificationRadarCatalogSteps.Create(ValidRows(), Items()));

        IStaticCatalogs snapshot = await loader.LoadAsync();

        Assert.Equal([10, 41], snapshot.Shops.GetByNpc(5)!.Tabs.Select(tab => tab.Id));
        Assert.Equal([8, 7], snapshot.Shops.Get(2).Tabs[0].Items.Select(item => item.Id));
        Assert.Equal([8, 7], snapshot.Shops.Get(2).Tabs[1].Items.Select(item => item.Id));
        Assert.Equal([0], snapshot.Shops.Get(2).Tabs[0].Items[1].Options.Select(option => option.OptionId));
        Assert.Equal([5], snapshot.Badges.Templates[0].Options.Select(option => option.Param));
        Assert.Equal("Mới<>Nội dung", snapshot.Notifications.Latest[0].WireText);
        Assert.Equal(-1, snapshot.Radar.Get(3).IconId);
        Assert.Equal(1, snapshot.Radar.Get(3).Template);
        Assert.Equal(-1, snapshot.Radar.Get(3).AuraId);
    }

    [Fact]
    public async Task Rejects_missing_shop_template_and_option_foreign_keys()
    {
        CommerceRows missingTemplate = ValidRows() with { ShopItems = [new ItemShopRow { ItemShopId = 7, TabId = 10, TempId = 9, IsSell = true }] };
        CatalogLoadException templateError = await Assert.ThrowsAsync<CatalogLoadException>(() => new StaticCatalogLoader(CommerceBadgeNotificationRadarCatalogSteps.Create(missingTemplate, Items())).LoadAsync());
        Assert.Contains("temp_id", templateError.Message, StringComparison.Ordinal);

        CommerceRows missingOption = ValidRows() with { ShopItemOptions = [new ItemShopOptionRow { ItemShopOptionId = 11, ItemShopId = 7, OptionId = 9, Param = 5 }] };
        CatalogLoadException optionError = await Assert.ThrowsAsync<CatalogLoadException>(() => new StaticCatalogLoader(CommerceBadgeNotificationRadarCatalogSteps.Create(missingOption, Items())).LoadAsync());
        Assert.Contains("option_id", optionError.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Rejects_invalid_badge_and_radar_json()
    {
        CommerceRows badBadge = ValidRows() with { Badges = [new DataBadgesRow { DataBadgesId = 1, IdItem = 0, Name = "B", Options = "{" }] };
        CatalogLoadException badgeError = await Assert.ThrowsAsync<CatalogLoadException>(() => new StaticCatalogLoader(CommerceBadgeNotificationRadarCatalogSteps.Create(badBadge, Items())).LoadAsync());
        Assert.Equal(BadgesCatalogLoadStep.StepName, badgeError.StepName);

        CommerceRows badRadar = ValidRows() with { RadarCards = [new RadarRow { RadarId = 3, Body = "[]", Options = "[{}]" }] };
        CatalogLoadException radarError = await Assert.ThrowsAsync<CatalogLoadException>(() => new StaticCatalogLoader(CommerceBadgeNotificationRadarCatalogSteps.Create(badRadar, Items())).LoadAsync());
        Assert.Equal(RadarCatalogLoadStep.StepName, radarError.StepName);
    }

    private static CommerceRows ValidRows() => new()
    {
        Shops = [new ShopRow { ShopId = 2, NpcId = 5, TagName = "shop", TypeShop = 1 }],
        ShopTabs = [new TabShopRow { TabShopId = 10, ShopId = 2, Name = "A<>B" }, new TabShopRow { TabShopId = 41, ShopId = 2, Name = "C" }],
        ShopItems = [new ItemShopRow { ItemShopId = 8, TabId = 10, TempId = 0, IsSell = true, IsNew = 1, Cost = 20, Costgold = 1, IconSpec = 2, TypeSell = 0, CreateTime = new DateTime(2026, 1, 2) }, new ItemShopRow { ItemShopId = 7, TabId = 10, TempId = 0, IsSell = true, IsNew = 1, Cost = 20, Costgold = 1, IconSpec = 2, TypeSell = 0, CreateTime = new DateTime(2026, 1, 1) }],
        ShopItemOptions = [new ItemShopOptionRow { ItemShopOptionId = 11, ItemShopId = 7, OptionId = 0, Param = 5 }, new ItemShopOptionRow { ItemShopOptionId = 12, ItemShopId = 8, OptionId = 0, Param = 6 }],
        BadgeTasks = [new TaskBadgesTemplateRow { TaskBadgesTemplateId = 1, Name = "T", MaxCount = 3, IdBadgesReward = 4 }],
        Badges = [new DataBadgesRow { DataBadgesId = 1, IdEffect = 4, IdItem = 0, Name = "B", Options = "[{\"id\":\"0\",\"param\":\"5\"}]" }],
        Notifications = [new NotifyRow { NotifyId = 2, Name = "Mới", Text = "Nội dung" }, new NotifyRow { NotifyId = 1, Name = "Cũ", Text = "Nội dung cũ" }],
        RadarCards = [new RadarRow { RadarId = 3, Body = "[]", Options = "[]" }],
    };

    private static IItemCatalog Items() => new Nro.Content.Catalogs.ItemCatalog([new ItemTemplate { Id = 0 }], [new ItemOptionTemplate(0, "o", 0)], [], new Dictionary<short, int>());
}
