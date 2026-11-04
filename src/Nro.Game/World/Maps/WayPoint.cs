namespace Nro.Game.World.Maps;

// Mô tả cửa chuyển bản đồ: vùng kích hoạt, trạng thái và tọa độ/bản đồ đích.
// Nguồn Java: nro/map/WayPoint.java (giữ kiểu short của Java cho các tọa độ và chỉ số).
public class WayPoint
{
    // Biên trái của vùng cửa.
    public short MinX { get; set; }

    // Biên trên của vùng cửa.
    public short MinY { get; set; }

    // Biên phải của vùng cửa.
    public short MaxX { get; set; }

    // Biên dưới của vùng cửa.
    public short MaxY { get; set; }

    // Cho biết đây là cửa đi vào.
    public bool IsEnter { get; set; }

    // Cho biết cửa dẫn tới bản đồ ngoại tuyến.
    public bool IsOffline { get; set; }

    // Tên hiển thị của cửa.
    public string Name { get; set; } = "";

    // Id bản đồ đích.
    public int GoMap { get; set; }

    // Tọa độ X xuất hiện tại bản đồ đích.
    public short GoX { get; set; }

    // Tọa độ Y xuất hiện tại bản đồ đích.
    public short GoY { get; set; }
}
