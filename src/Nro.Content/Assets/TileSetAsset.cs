using System.Collections.ObjectModel;

namespace Nro.Content.Assets;

// Nguồn Java: nro/server/Manager.java (dòng 1142–1169).
// Tập tile set bất biến, chứa các map tile theo thứ tự byte của Java. (Java: tileIndexTileType)
public sealed class TileSetAsset
{
    // Đóng danh sách tile set bằng bản sao để asset không đổi sau khi trả về. (Java: tileIndexTileType)
    public TileSetAsset(IEnumerable<TileSetMapAsset> maps)
    {
        ArgumentNullException.ThrowIfNull(maps);
        Maps = new ReadOnlyCollection<TileSetMapAsset>(maps.ToArray());
    }

    // Các tile set theo chỉ số map tile. (Java: tileMapId)
    public IReadOnlyList<TileSetMapAsset> Maps { get; }
}
