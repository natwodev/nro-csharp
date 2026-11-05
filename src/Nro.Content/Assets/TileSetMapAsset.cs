using System.Collections.ObjectModel;

namespace Nro.Content.Assets;

// Nguồn Java: nro/server/Manager.java (dòng 1148–1163).
// Một tile set bất biến với các nhóm type/index. (Java: numTileOfMap)
public sealed class TileSetMapAsset
{
    // Đóng nhóm type/index bằng bản sao để asset không bị sửa bên ngoài. (Java: tileType, typeIndex)
    public TileSetMapAsset(IEnumerable<TileTypeIndexAsset> types)
    {
        ArgumentNullException.ThrowIfNull(types);
        Types = new ReadOnlyCollection<TileTypeIndexAsset>(types.ToArray());
    }

    // Các nhóm index theo loại tile. (Java: tileType)
    public IReadOnlyList<TileTypeIndexAsset> Types { get; }
}
