using Nro.Data.Models.Shop;
using Nro.Data.Repositories.Shop;

namespace Nro.Tests.Integration.Db.Shop;

public sealed class ShopRepositoryTests : IClassFixture<ShopRepositoryFixture>, IAsyncLifetime
{
    private readonly ShopRepositoryFixture _fixture;

    public ShopRepositoryTests(ShopRepositoryFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Load_builds_the_ordered_shop_tree_with_java_narrowing_and_null_text()
    {
        await _fixture.RunAsync("insert into shop(id,npc_id,tag_name,type_shop) values (200,3,null,258),(100,-1,'second',257)");
        await _fixture.RunAsync("insert into tab_shop(id,shop_id,name) values (30,100,'Later<>Tab'),(10,100,'First<>Tab<>Again'),(20,200,'Other')");
        await _fixture.RunAsync("insert into item_shop(id,tab_id,temp_id,is_new,is_sell,type_sell,cost,costgold,icon_spec,create_time) values (30,10,32768,1,true,258,30,300,3000,'2026-01-01 00:00:00+00'),(20,10,5,0,true,1,20,200,2000,'2026-01-02 00:00:00+00'),(40,10,9,1,false,9,40,400,4000,'2026-01-03 00:00:00+00')");
        await _fixture.RunAsync("insert into item_shop_option(id,item_shop_id,option_id,param) values (90,20,9,900),(10,20,1,100),(5,30,7,700)");

        IReadOnlyList<ShopDefinition> shops = await new ShopRepository(_fixture.Db).LoadAsync();

        Assert.Equal(new[] { 100, 200 }, shops.Select(shop => shop.Id));
        Assert.Equal((sbyte)-1, shops[0].NpcId);
        Assert.Equal((sbyte)1, shops[0].TypeShop);
        Assert.Equal("second", shops[0].TagName);
        Assert.Equal("null", shops[1].TagName);
        Assert.Equal((sbyte)2, shops[1].TypeShop);

        Assert.Equal(new[] { 10, 30 }, shops[0].Tabs.Select(tab => tab.Id));
        Assert.Equal("First\nTab\nAgain", shops[0].Tabs[0].Name);
        Assert.Equal("Later\nTab", shops[0].Tabs[1].Name);

        IReadOnlyList<ShopItemDefinition> items = shops[0].Tabs[0].Items;
        Assert.Equal(new[] { 20, 30 }, items.Select(item => item.Id));
        Assert.False(items[0].IsNew);
        Assert.True(items[1].IsNew);
        Assert.Equal((short)-32768, items[1].TemplateId);
        Assert.Equal((sbyte)2, items[1].TypeSell);
        Assert.Equal(new[] { new ShopItemOptionDefinition(1, 100), new ShopItemOptionDefinition(9, 900) }, items[0].Options);
        Assert.Equal(new[] { new ShopItemOptionDefinition(7, 700) }, items[1].Options);
    }
}

public sealed class ShopRepositoryFixture : TestDatabase
{
    protected override async Task SeedAsync()
    {
        await RunAsync("create table shop(id integer primary key, npc_id integer not null, tag_name text null, type_shop integer not null)");
        await RunAsync("create table tab_shop(id integer primary key, shop_id integer not null, name text not null)");
        await RunAsync("create table item_shop(id integer primary key, tab_id integer not null, temp_id integer not null, is_new smallint not null, is_sell boolean not null, type_sell integer not null, cost integer not null, costgold integer not null, icon_spec integer not null, create_time timestamptz not null)");
        await RunAsync("create table item_shop_option(id integer primary key, item_shop_id integer not null, option_id integer not null, param integer not null)");
    }

    public Task ResetAsync() => RunAsync("delete from item_shop_option; delete from item_shop; delete from tab_shop; delete from shop");
}
