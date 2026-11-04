using Nro.Core.Templates;

namespace Nro.Game.World.Maps.Ports;

// Khai báo phân loại và tra cứu map mà Java đặt trong MapService.gI() và các bảng tĩnh Manager.
// Nguồn Java: nro/map/Map.java (lớp phụ thay MapService và Manager; các hàm thuần theo id map và dữ liệu tĩnh đã nạp, I-02/J-01 sở hữu dữ liệu, mô hình chỉ truy vấn).
public interface IMapRules
{
    // Kiểm tra map riêng tư (Java: isMapRiengTu).
    bool IsMapRiengTu(int mapId);

    // Kiểm tra map Địa Ngục (Java: isMapDiaNguc).
    bool IsMapDiaNguc(int mapId);

    // Kiểm tra map Ngũ Hành Sơn (Java: isMapNguHanhSon).
    bool IsMapNguHanhSon(int mapId);

    // Kiểm tra map sự kiện Hùng Vương (Java: isMapHungVuongEvent).
    bool IsMapHungVuongEvent(int mapId);

    // Kiểm tra map khí gas hủy diệt (Java: isMapKhiGasHuyDiet).
    bool IsMapKhiGasHuyDiet(int mapId);

    // Kiểm tra map phó bản (Java: isMapPhoBan).
    bool IsMapPhoBan(int mapId);

    // Kiểm tra map không rơi vật phẩm (Java: isMapNotDropItems).
    bool IsMapNotDropItems(int mapId);

    // Kiểm tra map sự kiện Giáng sinh (Java: isMapChristMasEvent).
    bool IsMapChristMasEvent(int mapId);

    // Kiểm tra map nâng sức khỏe (Java: isMapUpSKH).
    bool IsMapUpSKH(int mapId);

    // Kiểm tra map Sau Làng (Java: isMapSauLang).
    bool IsMapSauLang(int mapId);

    // Kiểm tra map quái bang hội phần 1 (Java: isMapMobBangHoi_Part1).
    bool IsMapMobBangHoiPart1(int mapId);

    // Kiểm tra map Kong đối đầu Godzila (Java: isMapKongvsGodzila).
    bool IsMapKongvsGodzila(int mapId);

    // Kiểm tra map băng (Java: isMapBang).
    bool IsMapBang(int mapId);

    // Kiểm tra map bản đồ kho báu (Java: isMapBanDoKhoBau).
    bool IsMapBanDoKhoBau(int mapId);

    // Kiểm tra map làng (Java: isMapLang).
    bool IsMapLang(int mapId);

    // Kiểm tra map hành tinh ngục tù (Java: isMapHanhTinhNgucTu).
    bool IsMapHanhTinhNgucTu(int mapId);

    // Kiểm tra map tương lai (Java: isMapTuongLai).
    bool IsMapTuongLai(int mapId);

    // Kiểm tra map lạnh (Java: isMapCold).
    bool IsMapCold(int mapId);

    // Kiểm tra map Hirudegarn (Java: isMapHirudegarn).
    bool IsMapHirudegarn(int mapId);

    // Kiểm tra map bình hút năng lượng (Java: isMapBinhHutNangLuong).
    bool IsMapBinhHutNangLuong(int mapId);

    // Kiểm tra map Potara (Java: isMapPotara).
    bool IsMapPotara(int mapId);

    // Kiểm tra map doanh trại (Java: isMapDoanhTrai).
    bool IsMapDoanhTrai(int mapId);

    // Kiểm tra map ngoại tuyến (Java: isMapOffline).
    bool IsMapOffline(int mapId);

    // Kiểm tra map bang hội (Java: isMapBangHoi).
    bool IsMapBangHoi(int mapId);

    // Tra map theo id; trả null khi không biết (Java: MapService.getMapById).
    Map? GetMapById(int mapId);

    // Tra mẫu quái theo id; trả null khi không biết (Java: Manager.getMobTemplateByTemp).
    MobTemplate? GetMobTemplate(int mobTempId);

    // Lấy tên hành tinh (Java: Service.get_HanhTinh).
    string GetPlanetName(sbyte planetId);

    // Lấy số zone cố định cho loại phó bản từ BlackBallWar, MajinBuu14H/12H, RedRibbonHQ, TreasureUnderSea, SnakeWay, DestronGas hoặc BossOfTheGangs; null với loại khác, còn map ngoại tuyến/bang hội luôn 1 zone do Map tự xử lý.
    int? GetFixedZoneCount(sbyte mapType);
}
