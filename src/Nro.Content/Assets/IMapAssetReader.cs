namespace Nro.Content.Assets;

// Nguồn Java: nro/server/Manager.java (dòng 1142–1191).
// Cổng đọc asset map từ gốc cấu hình; chỉ nhận map id số đã có trong WorldCatalog. (Java: readTileIndexTileType, readTileMap)
public interface IMapAssetReader
{
    // Đọc định nghĩa type/index của mọi tile set. (Java: tile_set_info)
    TileSetAsset ReadTileSet();

    // Đọc ma trận tile cho một map trong catalog. (Java: tile_map_data/{mapId})
    TileMapAsset ReadMap(int mapId);
}
