namespace Nro.Game.World.Maps;

// Lưu một dòng map_template cùng cửa, quái, NPC và hiệu ứng bản đồ.
// Nguồn Java: nro/template/MapTemplate.java (đặt trong Nro.Game vì giữ WayPoint và EffectMap; byte Java dùng sbyte C#).
public class MapTemplate
{
    // Id bản đồ.
    public int Id { get; set; }

    // Tên bản đồ.
    public string Name { get; set; } = "";

    // Loại bản đồ.
    public sbyte Type { get; set; }

    // Id hành tinh.
    public sbyte PlanetId { get; set; }

    // Kiểu nền.
    public sbyte BgType { get; set; }

    // Id ô nền.
    public sbyte TileId { get; set; }

    // Id ảnh nền.
    public sbyte BgId { get; set; }

    // Kiểu giới tính được phép.
    public sbyte GenderType { get; set; }

    // Số khu mặc định.
    public sbyte Zones { get; set; }

    // Số người tối đa mỗi khu.
    public sbyte MaxPlayerPerZone { get; set; }

    // Danh sách cửa chuyển bản đồ.
    public List<WayPoint> WayPoints { get; set; } = [];

    // Id mẫu quái theo vị trí.
    public sbyte[] MobTemp { get; set; } = [];

    // Cấp quái theo vị trí.
    public sbyte[] MobLevel { get; set; } = [];

    // Máu quái theo vị trí.
    public int[] MobHp { get; set; } = [];

    // Tọa độ X quái theo vị trí.
    public short[] MobX { get; set; } = [];

    // Tọa độ Y quái theo vị trí.
    public short[] MobY { get; set; } = [];

    // Id mẫu NPC theo vị trí.
    public sbyte[] NpcId { get; set; } = [];

    // Tọa độ X NPC theo vị trí.
    public short[] NpcX { get; set; } = [];

    // Tọa độ Y NPC theo vị trí.
    public short[] NpcY { get; set; } = [];

    // Danh sách hiệu ứng bản đồ.
    public List<EffectMap> EffectMaps { get; set; } = [];
}
