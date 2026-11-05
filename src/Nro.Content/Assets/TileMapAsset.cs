using System.Collections.ObjectModel;

namespace Nro.Content.Assets;

// Nguồn Java: nro/server/Manager.java (dòng 1175–1191).
// Ma trận tile bất biến, giữ giá trị byte có dấu Java dưới dạng int. (Java: tileMap)
public sealed class TileMapAsset
{
    private readonly ReadOnlyCollection<ReadOnlyCollection<int>> rows;

    // Đóng ma trận bằng sao chép sâu để dữ liệu tile không thể bị sửa. (Java: tileMap)
    public TileMapAsset(int width, int height, IEnumerable<IEnumerable<int>> rows)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(rows);
        var copied = rows.Select(row => new ReadOnlyCollection<int>((row ?? throw new ArgumentException("Hàng tile không được null.", nameof(rows))).ToArray())).ToArray();
        if (copied.Length != height || copied.Any(row => row.Count != width))
        {
            throw new ArgumentException("Kích thước ma trận tile không khớp width/height.", nameof(rows));
        }

        Width = width;
        Height = height;
        this.rows = new ReadOnlyCollection<ReadOnlyCollection<int>>(copied);
    }

    // Rộng map theo ô. (Java: w)
    public int Width { get; }

    // Cao map theo ô. (Java: h)
    public int Height { get; }

    // Các hàng tile chỉ đọc. (Java: tileMap)
    public IReadOnlyList<IReadOnlyList<int>> Rows => rows;

    // Lấy tile theo tọa độ ô. (Java: tileMap[y][x])
    public int GetTile(int x, int y) => (uint)x < (uint)Width && (uint)y < (uint)Height
        ? rows[y][x]
        : throw new ArgumentOutOfRangeException((uint)x >= (uint)Width ? nameof(x) : nameof(y));
}
