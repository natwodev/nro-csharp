using System.Collections.ObjectModel;

namespace Nro.Content.Assets;

// Nguồn Java: nro/server/Manager.java (dòng 1150–1163).
// Nhóm index bất biến của một loại tile. (Java: TileSetInfoData)
public sealed class TileTypeIndexAsset
{
    // Đóng các index bằng bản sao để asset không bị sửa bên ngoài. (Java: data)
    public TileTypeIndexAsset(int type, IEnumerable<int> indices)
    {
        ArgumentNullException.ThrowIfNull(indices);
        Type = type;
        Indices = new ReadOnlyCollection<int>(indices.ToArray());
    }

    // Mã loại tile. (Java: tileType)
    public int Type { get; }

    // Các index có loại tile này. (Java: typeIndex)
    public IReadOnlyList<int> Indices { get; }
}
