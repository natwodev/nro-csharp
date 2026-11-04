namespace Nro.Game.World.Mobs.Ports;

// Ảnh chụp các dữ kiện player mà quy tắc rơi đồ cần.
// Nguồn Java: player.nPoint.*, player.itemTime.*, player.getSession() và player.clan.
public sealed record MobLootPlayerView
{
    // Giới tính để chọn vật phẩm theo hệ.
    public sbyte Gender { get; init; }

    // Sức mạnh hiện có của player.
    public long Power { get; init; }

    // Tỷ lệ may mắn. (Java: nPoint.tlMayman)
    public double LuckPercent { get; init; }

    // Phần trăm vàng cộng thêm. (Java: nPoint.tlGold)
    public double GoldPercent { get; init; }

    // Cho biết đang trang bị đồ Sao Pha Lê (Java: isDoSPL).
    public bool HasCrystalStarEquipment { get; init; }

    // Cho biết phiên chơi đang hoạt động.
    public bool IsActive { get; init; }

    // Vé tháng còn hiệu lực. (Java: session.vethang == 1 && session.vethangExpire > now)
    public bool MonthlyCardActive { get; init; }

    // Vé tuần còn hiệu lực. (Java: session.vetuan == 1 && session.vetuanExpire > now)
    public bool WeeklyCardActive { get; init; }

    // Cho biết đang dùng cỏ bốn lá.
    public bool UseCoBonLa { get; init; }

    // Cho biết đang dùng máy dò thường (Java: isUseMayDo).
    public bool UseScanner { get; init; }

    // Cho biết đang dùng máy dò siêu hóa (Java: isUseMayDoSieuHoa).
    public bool UseSuperTransformationScanner { get; init; }

    // Cho biết đang dùng máy dò linh hồn (Java: isUseMayDoLinhHon).
    public bool UseSoulScanner { get; init; }

    // Cho biết đang dùng máy dò ngọc bí (Java: isUseMayDoNgocBi).
    public bool UseMysticOrbScanner { get; init; }

    // Bang có bản đồ kho báu cấp lớn hơn 0. (Java: player.clan != null && player.clan.BanDoKhoBau.level > 0)
    public bool HasTreasureMapClan { get; init; }
}

// Cổng tồn kho, vật phẩm và sự kiện mà quy tắc rơi của Mob và MobDrop cần; mô hình chỉ quay ngẫu nhiên còn host quản vật phẩm.
// Nguồn Java: InventoryService.gI(), ItemService.gI(), player.itemEvent và player.event.
public interface IMobLootHost
{
    // Lấy ảnh chụp dữ kiện rơi đồ của player.
    MobLootPlayerView GetLootView(Player player);

    // Tìm điều kiện vật phẩm Noel. (Java: InventoryService.findNonNoel)
    bool HasChristmasHat(Player player);

    // Kiểm tra còn được rơi kẹo Giáng Sinh và tăng bộ đếm nếu được. (Java: player.itemEvent.canDropKeoGiangSinh)
    bool CanDropChristmasCandy(Player player, int limit);

    // Kiểm tra còn được rơi tất vô Giáng Sinh. (Java: player.itemEvent.canDropTatVoGiangSinh)
    bool CanDropTatVoGiangSinh(Player player, int limit);

    // Kiểm tra còn được rơi bình nước sự kiện. (Java: player.itemEvent.canDropBinhNuoc)
    bool CanDropBinhNuoc(Player player, int limit);

    // Cộng điểm sự kiện Ngũ Hành Sơn. (Java: player.event.addEventPointNHS)
    void AddEventPointNHS(Player player, int points);

    // Tìm vật phẩm theo giới tính trong túi. (Java: InventoryService.findItemGender)
    bool HasNamekianEquipment(Player player);

    // Đếm món mặc khác null thuộc bộ yêu cầu. (Java: duyệt player.inventory.itemsBody)
    int CountBodySetPieces(Player player, BodySetKind kind);

    // Tìm bình hút năng lượng. (Java: InventoryService.findItemBinhHutNangLuong)
    bool HasEnergyAbsorbingBottle(Player player);

    // Trả giá trị tùy chọn 262 của từng vật phẩm template 1911 theo thứ tự túi. (Java: duyệt player.inventory.itemsBag)
    IReadOnlyList<int> GetBinhHutNangLuongParams(Player player);

    // Tăng tùy chọn 262 của vật phẩm tại index thêm 1, chặn 3000 và gửi túi. (Java: addOptionParam và sendItemBag)
    void IncreaseBinhHutNangLuong(Player player, int index);

    // Tạo các tùy chọn có sẵn của ItemService.createItemSetKichHoat(itemId, 1), cùng template itemId (Java: createSetKichHoatOptions).
    List<ItemMapOption> CreateActivationSetOptions(int itemId);

    // Lấy người quyết định bộ đếm rơi bình nước: chủ của đệ tử hoặc chính player. (Java: player.isDeTu ? ((Detu) player).master : player)
    Player GetBinhNuocOwner(Player player);
}
