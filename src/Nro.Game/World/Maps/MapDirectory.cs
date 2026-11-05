using Nro.Game.Services.Maps;
using Nro.Game.World.Maps.Ports;

namespace Nro.Game.World.Maps;

// Danh mục Map runtime đã nạp, chỉ tra cứu trong bộ nhớ và kiểm tra dữ liệu bắt buộc.
// Nguồn Java: nro/server/Manager.java (MAPS); nro/services/MapService.java (dòng 310–317).
public sealed class MapDirectory : IMapDirectory
{
    // Bảng tra cứu id sau khi host đã dựng Map từ catalog/tài nguyên. (Java: Manager.MAPS)
    private readonly IReadOnlyDictionary<int, Map> _byId;

    // Kiểm tra trùng id và map 205 bắt buộc ngay lúc danh mục được dựng. (Quyết định dự án: dump thiếu 205 phải fail-fast)
    public MapDirectory(IReadOnlyList<Map> maps)
    {
        ArgumentNullException.ThrowIfNull(maps);
        Maps = maps;
        var byId = new Dictionary<int, Map>(maps.Count);
        foreach (Map map in maps)
        {
            if (!byId.TryAdd(map.MapId, map))
            {
                throw new InvalidOperationException($"Danh mục map có map id trùng: {map.MapId}.");
            }
        }

        if (!byId.ContainsKey(MapIds.REQUIRED_PAST_MAP))
        {
            throw new InvalidOperationException($"Thiếu map bắt buộc {MapIds.REQUIRED_PAST_MAP} trong dump; không thể khởi động cho tới khi dữ liệu map được sửa.");
        }

        _byId = byId;
    }

    // Toàn bộ map theo thứ tự lúc host dựng. (Java: Manager.MAPS)
    public IReadOnlyList<Map> Maps { get; }

    // Tra map runtime theo id. (Java: MapService.getMapById)
    public Map? GetMapById(int mapId) => _byId.GetValueOrDefault(mapId);
}
