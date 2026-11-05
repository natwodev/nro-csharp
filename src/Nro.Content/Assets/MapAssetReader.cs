using System.Buffers.Binary;
using System.Globalization;
using Nro.Game.Content;

namespace Nro.Content.Assets;

// Nguồn Java: nro/server/Manager.java (dòng 1142–1191).
// Đọc asset tile từ gốc cấu hình đã chuẩn hóa; đường dẫn asset chỉ ghép từ các segment hằng và map id hợp lệ. (Java: readTileIndexTileType, readTileMap)
public sealed class MapAssetReader : IMapAssetReader
{
    private readonly string dataRoot;
    private readonly IWorldCatalog world;

    // Tạo reader với gốc dữ liệu tuyệt đối do cấu hình máy chủ cung cấp, không nhận đường dẫn asset từ caller. (Java: data/map)
    public MapAssetReader(string dataRoot, IWorldCatalog world)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataRoot);
        ArgumentNullException.ThrowIfNull(world);
        if (!Path.IsPathFullyQualified(dataRoot) || HasParentSegment(dataRoot))
        {
            throw new ArgumentException("Gốc asset phải là đường dẫn tuyệt đối không chứa '..'.", nameof(dataRoot));
        }

        this.dataRoot = Path.GetFullPath(dataRoot);
        this.world = world;
    }

    // Đọc tile_set_info bằng định dạng DataInputStream big-endian của Java. (Java: readTileIndexTileType)
    public TileSetAsset ReadTileSet()
    {
        ReadOnlySpan<byte> bytes = File.ReadAllBytes(TileSetInfoPath());
        int offset = 0;
        int mapCount = ReadSignedByte(bytes, ref offset, "numTileMap");
        if (mapCount < 0) throw new InvalidDataException("numTileMap không được âm.");
        var maps = new List<TileSetMapAsset>(mapCount);
        for (int mapIndex = 0; mapIndex < mapCount; mapIndex++)
        {
            int typeCount = ReadSignedByte(bytes, ref offset, $"numTileOfMap[{mapIndex}]");
            if (typeCount < 0) throw new InvalidDataException($"numTileOfMap[{mapIndex}] không được âm.");
            var types = new List<TileTypeIndexAsset>(typeCount);
            for (int typeIndex = 0; typeIndex < typeCount; typeIndex++)
            {
                int type = ReadInt32BigEndian(bytes, ref offset, $"tileType[{mapIndex},{typeIndex}]");
                int indexCount = ReadSignedByte(bytes, ref offset, $"numIndex[{mapIndex},{typeIndex}]");
                if (indexCount < 0) throw new InvalidDataException($"numIndex[{mapIndex},{typeIndex}] không được âm.");
                var indices = new int[indexCount];
                for (int index = 0; index < indexCount; index++) indices[index] = ReadSignedByte(bytes, ref offset, $"typeIndex[{mapIndex},{typeIndex},{index}]");
                types.Add(new TileTypeIndexAsset(type, indices));
            }

            maps.Add(new TileSetMapAsset(types));
        }

        return new TileSetAsset(maps);
    }

    // Đọc tile_map_data/mapId sau khi kiểm map id thuộc catalog; map id không bao giờ là một chuỗi đường dẫn. (Java: readTileMap)
    public TileMapAsset ReadMap(int mapId)
    {
        _ = world.GetMap(mapId);
        ReadOnlySpan<byte> bytes = File.ReadAllBytes(TileMapPath(mapId));
        int offset = 0;
        int width = ReadSignedByte(bytes, ref offset, "width");
        int height = ReadSignedByte(bytes, ref offset, "height");
        if (width <= 0 || height <= 0) throw new InvalidDataException($"Map id {mapId} có kích thước tile không hợp lệ {width}x{height}.");
        var rows = new int[height][];
        for (int y = 0; y < height; y++)
        {
            rows[y] = new int[width];
            for (int x = 0; x < width; x++) rows[y][x] = ReadSignedByte(bytes, ref offset, $"tile[{x},{y}]");
        }

        return new TileMapAsset(width, height, rows);
    }

    // Tạo đường dẫn tile_set_info chỉ từ segment hằng. (Java: data/map/tile_set_info)
    private string TileSetInfoPath() => Path.Combine(dataRoot, "data", "map", "tile_set_info");

    // Tạo tên tệp map từ int đã xác thực, không nhận hay ghép chuỗi caller cung cấp. (Java: data/map/tile_map_data/{mapId})
    private string TileMapPath(int mapId) => Path.Combine(dataRoot, "data", "map", "tile_map_data", mapId.ToString(CultureInfo.InvariantCulture));

    // Cấm parent segment trong gốc cấu hình trước chuẩn hóa. (Java: FileInputStream)
    private static bool HasParentSegment(string path) => path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.None).Any(segment => String.Equals(segment, "..", StringComparison.Ordinal));

    // Đọc byte có dấu theo DataInputStream.readByte. (Java: readByte)
    private static int ReadSignedByte(ReadOnlySpan<byte> bytes, ref int offset, string field)
    {
        if ((uint)offset >= (uint)bytes.Length) throw new EndOfStreamException($"Thiếu dữ liệu tại {field}.");
        return (sbyte)bytes[offset++];
    }

    // Đọc int big-endian theo DataInputStream.readInt. (Java: readInt)
    private static int ReadInt32BigEndian(ReadOnlySpan<byte> bytes, ref int offset, string field)
    {
        if (bytes.Length - offset < sizeof(int)) throw new EndOfStreamException($"Thiếu dữ liệu tại {field}.");
        int value = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(offset, sizeof(int)));
        offset += sizeof(int);
        return value;
    }
}
