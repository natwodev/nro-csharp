using Nro.Core.Templates;
using Nro.Game.Content;
using Nro.Game.World.Maps;

namespace Nro.Content.Catalogs;

// Nguồn Java: nro/server/Manager.java (dòng 742–855).
// Danh mục thế giới đóng băng bằng sao chép sâu; Get luôn trả bản sao để snapshot không bị sửa từ bên ngoài. (Java: MOB_TEMPLATES, NPC_TEMPLATES, MAP_TEMPLATES)
public sealed class WorldCatalog : IWorldCatalog
{
    private readonly MobTemplate[] mobs;
    private readonly NpcTemplate[] npcs;
    private readonly MapTemplate[] maps;

    // Đóng danh mục từ các mảng đã xác thực liên tục theo id. (Java: ArrayList.add, MAP_TEMPLATES[y++])
    public WorldCatalog(IEnumerable<MobTemplate> mobs, IEnumerable<NpcTemplate> npcs, IEnumerable<MapTemplate> maps)
    {
        ArgumentNullException.ThrowIfNull(mobs);
        ArgumentNullException.ThrowIfNull(npcs);
        ArgumentNullException.ThrowIfNull(maps);
        this.mobs = mobs.Select(Clone).ToArray();
        this.npcs = npcs.Select(Clone).ToArray();
        this.maps = maps.Select(Clone).ToArray();
    }

    // Số mẫu quái. (Java: MOB_TEMPLATES.size)
    public int MobCount => mobs.Length;

    // Số mẫu NPC. (Java: NPC_TEMPLATES.size)
    public int NpcCount => npcs.Length;

    // Số mẫu map. (Java: MAP_TEMPLATES.length)
    public int MapCount => maps.Length;

    // Lấy bản sao mẫu quái tại id. (Java: MOB_TEMPLATES.get)
    public MobTemplate GetMob(int id) => (uint)id < (uint)mobs.Length
        ? Clone(mobs[id])
        : throw new ArgumentOutOfRangeException(nameof(id), id, "Không có mẫu quái tại id đã cho.");

    // Lấy bản sao mẫu NPC tại id. (Java: NPC_TEMPLATES.get)
    public NpcTemplate GetNpc(int id) => (uint)id < (uint)npcs.Length
        ? Clone(npcs[id])
        : throw new ArgumentOutOfRangeException(nameof(id), id, "Không có mẫu NPC tại id đã cho.");

    // Lấy bản sao mẫu map tại id. (Java: MAP_TEMPLATES[index])
    public MapTemplate GetMap(int id) => (uint)id < (uint)maps.Length
        ? Clone(maps[id])
        : throw new ArgumentOutOfRangeException(nameof(id), id, "Không có mẫu bản đồ tại id đã cho.");

    // Sao chép mẫu quái để giữ snapshot bất biến. (Java: MobTemplate)
    private static MobTemplate Clone(MobTemplate value) => new()
    {
        Id = value.Id, Type = value.Type, Name = value.Name, Hp = value.Hp, RangeMove = value.RangeMove, Speed = value.Speed, DartType = value.DartType, PercentDame = value.PercentDame, PercentPotential = value.PercentPotential, Percent_gold = value.Percent_gold,
    };

    // Sao chép mẫu NPC để giữ snapshot bất biến. (Java: NpcTemplate)
    private static NpcTemplate Clone(NpcTemplate value) => new()
    {
        Id = value.Id, Name = value.Name, Head = value.Head, Body = value.Body, Leg = value.Leg, Avatar = value.Avatar,
    };

    // Sao chép cấu trúc map và mọi mảng JSON để giữ snapshot bất biến. (Java: MapTemplate)
    private static MapTemplate Clone(MapTemplate value) => new()
    {
        Id = value.Id, Name = value.Name, Type = value.Type, PlanetId = value.PlanetId, BgType = value.BgType, TileId = value.TileId, BgId = value.BgId, GenderType = value.GenderType, Zones = value.Zones, MaxPlayerPerZone = value.MaxPlayerPerZone,
        WayPoints = value.WayPoints.Select(Clone).ToList(), MobTemp = value.MobTemp.ToArray(), MobLevel = value.MobLevel.ToArray(), MobHp = value.MobHp.ToArray(), MobX = value.MobX.ToArray(), MobY = value.MobY.ToArray(), NpcId = value.NpcId.ToArray(), NpcX = value.NpcX.ToArray(), NpcY = value.NpcY.ToArray(), EffectMaps = value.EffectMaps.Select(effect => new EffectMap { Key = effect.Key, Value = effect.Value }).ToList(),
    };

    // Sao chép cửa map để giữ snapshot bất biến. (Java: WayPoint)
    private static WayPoint Clone(WayPoint value) => new()
    {
        MinX = value.MinX, MinY = value.MinY, MaxX = value.MaxX, MaxY = value.MaxY, IsEnter = value.IsEnter, IsOffline = value.IsOffline, Name = value.Name, GoMap = value.GoMap, GoX = value.GoX, GoY = value.GoY,
    };
}
