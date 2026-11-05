using Nro.Content.Catalogs;
using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;
using Nro.Game.World.Maps;

namespace Nro.Content.Loaders;

// Nguồn Java: nro/server/Manager.java (dòng 776–855, 981–1067).
// Nạp map JSON sau mob/NPC; mob thiếu được runtime Java bỏ qua, NPC thiếu vẫn lỗi khi dựng. (Java: MAP_TEMPLATES, loadMap)
public sealed class MapTemplateCatalogLoadStep : ICatalogLoadStep
{
    // Tên bước và khóa ExpectedCounts. (Java: map_template)
    public const string StepName = "map-template";
    private readonly IStaticRowSource source;
    private readonly WorldCatalogLoadState state;

    // Tạo bước nạp từ nguồn hàng và trạng thái J-01E cục bộ. (Java: map_template)
    internal MapTemplateCatalogLoadStep(IStaticRowSource source, WorldCatalogLoadState state)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
        this.state = state ?? throw new ArgumentNullException(nameof(state));
    }

    // Tên bước nạp. (Java: map_template)
    public string Name => StepName;

    // Map cần mob và NPC để đối chiếu hành vi khởi tạo runtime Java. (Java: mobTemp, npcId)
    public IReadOnlyCollection<string> Dependencies { get; } = [MobTemplateCatalogLoadStep.StepName, NpcTemplateCatalogLoadStep.StepName];

    // Dựng WorldCatalog mới, không sửa catalog/snapshot đã công bố. (Java: loadMap)
    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<Nro.Core.Templates.MobTemplate> mobs = state.Mobs ?? throw Missing(MobTemplateCatalogLoadStep.StepName);
        IReadOnlyList<Nro.Core.Templates.NpcTemplate> npcs = state.Npcs ?? throw Missing(NpcTemplateCatalogLoadStep.StepName);
        IReadOnlyList<MapTemplateRow> rows = await source.ReadMapTemplatesAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var maps = new List<MapTemplate>(rows.Count);
        var mobIds = mobs.Select(mob => mob.Id).ToHashSet();
        var npcIds = npcs.Select(npc => npc.Id).ToHashSet();
        int missingMobReferenceCount = 0;

        foreach (MapTemplateRow row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StaticCatalogValidation.Unique(ids, row.MapTemplateId, StepName);
            MapTemplate map = CreateMap(row, mobIds, npcIds, out int missingMobs);
            missingMobReferenceCount += missingMobs;
            maps.Add(map);
        }

        StaticCatalogValidation.NonNegativeIds(ids, StepName, "được tra cứu theo mapId");
        builder.SetWorld(new WorldCatalog(mobs, npcs, maps.OrderBy(map => map.Id)));
        return new CatalogStepResult(maps.Count, Warnings(missingMobReferenceCount));
    }

    // Chuyển một dòng map sang kiểu miền và kiểm mọi byte/short Java. initMob Java bỏ qua template mob không tìm thấy.
    private static MapTemplate CreateMap(MapTemplateRow row, IReadOnlySet<int> mobIds, IReadOnlySet<int> npcIds, out int missingMobs)
    {
        (sbyte[] mobTemp, sbyte[] mobLevel, int[] mobHp, short[] mobX, short[] mobY) = WorldCatalogJson.Mobs(row.Mobs, row.MapTemplateId);
        (sbyte[] npcId, short[] npcX, short[] npcY) = WorldCatalogJson.Npcs(row.Npcs, row.MapTemplateId);
        missingMobs = 0;
        foreach (sbyte id in mobTemp)
        {
            if (!mobIds.Contains(id))
            {
                missingMobs++;
            }
        }

        foreach (sbyte id in npcId)
        {
            if (!npcIds.Contains(id))
            {
                throw new CatalogLoadException(StepName, $"Map id {row.MapTemplateId} tham chiếu NPC id {id} không có trong catalog.");
            }
        }

        return new MapTemplate
        {
            Id = row.MapTemplateId, Name = row.Name ?? String.Empty, Type = StaticCatalogValidation.SByte(row.Type, StepName, "type", row.MapTemplateId), PlanetId = StaticCatalogValidation.SByte(row.PlanetId, StepName, "planet_id", row.MapTemplateId), BgType = StaticCatalogValidation.SByte(row.BgType, StepName, "bg_type", row.MapTemplateId), TileId = StaticCatalogValidation.SByte(row.TileId, StepName, "tile_id", row.MapTemplateId), BgId = StaticCatalogValidation.SByte(row.BgId, StepName, "bg_id", row.MapTemplateId), Zones = StaticCatalogValidation.SByte(row.Zones, StepName, "zones", row.MapTemplateId), MaxPlayerPerZone = StaticCatalogValidation.SByte(row.MaxPlayer, StepName, "max_player", row.MapTemplateId), GenderType = StaticCatalogValidation.SByte(row.GenderType, StepName, "genderType", row.MapTemplateId), WayPoints = WorldCatalogJson.WayPoints(row.Waypoints, row.MapTemplateId), MobTemp = mobTemp, MobLevel = mobLevel, MobHp = mobHp, MobX = mobX, MobY = mobY, NpcId = npcId, NpcX = npcX, NpcY = npcY,
        };
    }

    private static IEnumerable<string> Warnings(int missingMobReferenceCount)
    {
        if (missingMobReferenceCount != 0)
        {
            yield return $"{StepName}: giữ {missingMobReferenceCount} tham chiếu mob không tồn tại (Java initMob bỏ qua các mob này ở runtime)";
        }
    }

    // Báo phụ thuộc nội bộ bị mất thay vì nạp vào snapshot dở dang. (Java: loadMap)
    private static CatalogLoadException Missing(string stepName) => new(StepName, $"Thiếu kết quả bắt buộc của bước {stepName}.");
}
