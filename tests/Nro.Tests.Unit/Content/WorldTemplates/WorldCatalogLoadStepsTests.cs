using Nro.Content.Assets;
using Nro.Content.Catalogs;
using Nro.Content.Loaders;
using Nro.Content.StaticData;
using Nro.Core.Templates;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;
using Nro.Game.World.Maps;

namespace Nro.Tests.Unit.Content.WorldTemplates;

// Nguồn Java: nro/server/Manager.java (dòng 742–855, 981–1067, 1142–1191).
// Kiểm thử J-01E chỉ dùng hàng trong bộ nhớ và tệp tạm, không tích hợp cơ sở dữ liệu. (Java: loadDatabase, loadMap)
public sealed class WorldCatalogLoadStepsTests
{
    // Parse map gồm waypoint/mob/NPC theo các mảng JSON Java. (Java: waypoints, mobs, npcs)
    [Fact]
    public async Task Parses_a_complete_map_template()
    {
        var loader = CreateLoader(Source(Map(0)));

        IStaticCatalogs snapshot = await loader.LoadAsync();

        var map = snapshot.World.GetMap(0);
        Assert.Equal("Test map", map.Name);
        WayPoint waypoint = Assert.Single(map.WayPoints);
        Assert.Equal((short)1, waypoint.MinX);
        Assert.True(waypoint.IsEnter);
        Assert.Equal((short)9, waypoint.GoY);
        Assert.Equal([0], map.MobTemp);
        Assert.Equal([1], map.MobLevel);
        Assert.Equal([100], map.MobHp);
        Assert.Equal([0], map.NpcId);
        Assert.Equal((short)7, map.NpcX[0]);
        map.MobTemp[0] = 99;
        Assert.Equal((sbyte)0, snapshot.World.GetMap(0).MobTemp[0]);
    }

    // NULL avatar của ADO.NET có cùng giá trị 0 như ResultSet.getInt Java. (Java: rs.getInt("avatar"))
    [Fact]
    public async Task Null_avatar_becomes_zero()
    {
        var source = Source(Map(0));
        source.Npcs = [Npc(0, null)];

        IStaticCatalogs snapshot = await CreateLoader(source).LoadAsync();

        Assert.Equal(0, snapshot.World.GetNpc(0).Avatar);
    }

    // Từ chối byte vượt khoảng thay vì ép tràn int của hàng DB. (Java: rs.getByte)
    [Fact]
    public async Task Rejects_signed_byte_overflow()
    {
        var source = Source(Map(0));
        source.Mobs = [new MobTemplateRow { MobTemplateId = 0, Type = 128, Name = "M", Hp = 1 }];

        CatalogLoadException exception = await Assert.ThrowsAsync<CatalogLoadException>(() => CreateLoader(source).LoadAsync());

        Assert.Contains("vượt khoảng Int8", exception.Message, StringComparison.Ordinal);
    }

    // Từ chối int map DB vượt byte Java, không ép tràn tile_id. (Java: rs.getByte("tile_id"))
    [Fact]
    public async Task Rejects_map_byte_overflow()
    {
        MapTemplateRow map = Map(0);
        map.TileId = 128;

        CatalogLoadException exception = await Assert.ThrowsAsync<CatalogLoadException>(() => CreateLoader(Source(map)).LoadAsync());

        Assert.Contains("tile_id=128", exception.Message, StringComparison.Ordinal);
    }

    // Từ chối mob JSON không tồn tại trong danh mục đã nạp. (Java: mapTemplate.mobTemp)
    [Fact]
    public async Task Rejects_missing_mob_foreign_key()
    {
        MapTemplateRow map = Map(0);
        map.Mobs = "[[1,1,100,2,3]]";

        CatalogLoadException exception = await Assert.ThrowsAsync<CatalogLoadException>(() => CreateLoader(Source(map)).LoadAsync());

        Assert.Contains("mob id 1", exception.Message, StringComparison.Ordinal);
    }

    // Từ chối id mẫu quái trùng trước khi công bố snapshot. (Java: MOB_TEMPLATES.add)
    [Fact]
    public async Task Rejects_duplicate_ids()
    {
        var source = Source(Map(0));
        source.Mobs = [Mob(0), Mob(0)];

        CatalogLoadException exception = await Assert.ThrowsAsync<CatalogLoadException>(() => CreateLoader(source).LoadAsync());

        Assert.Contains("trùng", exception.Message, StringComparison.Ordinal);
    }

    // Báo chính xác id map 205 bị thiếu thay vì dịch chỉ số Java im lặng. (Java: MAP_TEMPLATES[index])
    [Fact]
    public async Task Reports_each_missing_catalog_id()
    {
        var source = Source(Map(0));
        source.Maps = [Map(0), Map(2)];

        CatalogGapException exception = await Assert.ThrowsAsync<CatalogGapException>(() => CreateLoader(source).LoadAsync());

        Assert.Equal([1], exception.MissingIds);
        Assert.Contains("1", exception.Message, StringComparison.Ordinal);
    }

    // Reload dựng snapshot mới và để snapshot cũ giữ map cũ. (Java: loadMap)
    [Fact]
    public async Task Reload_does_not_mutate_the_previous_snapshot()
    {
        var source = Source(Map(0, "before"));
        StaticCatalogLoader loader = CreateLoader(source);
        IStaticCatalogs before = await loader.LoadAsync();
        source.Maps = [Map(0, "after")];

        IStaticCatalogs after = await loader.LoadAsync();

        Assert.NotSame(before, after);
        Assert.Equal("before", before.World.GetMap(0).Name);
        Assert.Equal("after", after.World.GetMap(0).Name);
    }

    // Chặn map id không thuộc catalog và gốc chứa parent segment trước bất kỳ I/O asset nào. (Java: tile_map_data/{mapId})
    [Fact]
    public void Map_asset_reader_rejects_unknown_id_and_parent_segment()
    {
        var catalog = new WorldCatalog([new MobTemplate { Id = 0 }], [new NpcTemplate { Id = 0 }], [new Nro.Game.World.Maps.MapTemplate { Id = 0 }]);
        string root = Path.GetTempPath();
        var reader = new MapAssetReader(root, catalog);

        Assert.Throws<ArgumentOutOfRangeException>(() => reader.ReadMap(1));
        Assert.Throws<ArgumentException>(() => new MapAssetReader(Path.Combine(root, "..", "assets"), catalog));
    }

    // Đọc tile binary Java và chỉ trả các collection không thể sửa. (Java: readTileIndexTileType, readTileMap)
    [Fact]
    public void Map_asset_reader_returns_immutable_tile_data()
    {
        string root = Path.Combine(Path.GetTempPath(), "nro-j01e-" + Guid.NewGuid().ToString("N"));
        string mapDirectory = Path.Combine(root, "data", "map", "tile_map_data");
        Directory.CreateDirectory(mapDirectory);
        try
        {
            File.WriteAllBytes(Path.Combine(root, "data", "map", "tile_set_info"), [1, 1, 0, 0, 0, 2, 2, 255, 4]);
            File.WriteAllBytes(Path.Combine(mapDirectory, "0"), [2, 1, 255, 4]);
            var catalog = new WorldCatalog([new MobTemplate { Id = 0 }], [new NpcTemplate { Id = 0 }], [new MapTemplate { Id = 0 }]);
            var reader = new MapAssetReader(root, catalog);

            TileSetAsset tileSet = reader.ReadTileSet();
            TileMapAsset map = reader.ReadMap(0);

            Assert.Equal(2, tileSet.Maps[0].Types[0].Type);
            Assert.Equal([-1, 4], tileSet.Maps[0].Types[0].Indices);
            Assert.Equal(-1, map.GetTile(0, 0));
            Assert.Equal(4, map.GetTile(1, 0));
            Assert.Throws<NotSupportedException>(() => ((IList<int>)map.Rows[0])[0] = 99);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    // Tạo loader world không áp ExpectedCounts để unit test được dữ liệu tối thiểu. (Java: loadDatabase)
    private static StaticCatalogLoader CreateLoader(WorldRowSource source) => new(WorldCatalogSteps.Create(source));

    // Tạo nguồn có đủ mob/NPC cho map mẫu. (Java: mob_template, npc_template, map_template)
    private static WorldRowSource Source(params MapTemplateRow[] maps) => new()
    {
        Mobs = [Mob(0)], Npcs = [Npc(0, 3)], Maps = maps,
    };

    // Tạo hàng quái nhỏ nhất. (Java: mob_template)
    private static MobTemplateRow Mob(int id) => new() { MobTemplateId = id, Type = 1, Name = "M", Hp = 100, RangeMove = 1, Speed = 2, DartType = 3, DamagePercent = 4, PercentPotential = 5, PercentGold = 6 };

    // Tạo hàng NPC nhỏ nhất. (Java: npc_template)
    private static NpcTemplateRow Npc(int id, int? avatar) => new() { NpcTemplateId = id, Name = "N", Head = 1, Body = -1, Leg = -1, Avatar = avatar };

    // Tạo hàng map với đủ JSON waypoint/mob/NPC; String JSON lồng mô phỏng String.valueOf Java. (Java: map_template)
    private static MapTemplateRow Map(int id, string name = "Test map") => new()
    {
        MapTemplateId = id, Name = name, Zones = 1, MaxPlayer = 10, Data = "[]", Type = 0, PlanetId = 0, BgType = 0, TileId = 0, BgId = -1, GenderType = 0,
        Waypoints = "[\"[\"Gate\",1,2,3,4,1,0,0,8,9]\"]", Mobs = "[\"[0,1,100,2,3]\"]", Npcs = "[\"[0,7,8]\"]",
    };

}
