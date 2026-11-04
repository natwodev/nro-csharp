namespace Nro.Game.World.Maps;

// Lưu id và tham số một tùy chọn của vật phẩm rơi trên mặt đất.
// Nguồn Java: nro/map/ItemMap.java (lớp phụ giữ id được luật rơi đọc để mô hình bản đồ độc lập catalog; bộ nhặt đổi lại ItemOption thật khi tạo Item).
public sealed class ItemMapOption
{
    // Tạo tùy chọn từ id mẫu và tham số.
    public ItemMapOption(int id, int param)
    {
        Id = id;
        Param = param;
    }

    // Id mẫu tùy chọn (Java: optionTemplate.id).
    public int Id { get; }

    // Giá trị tham số tùy chọn (Java: param).
    public int Param { get; set; }
}
