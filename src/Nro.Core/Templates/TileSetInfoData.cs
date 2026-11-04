namespace Nro.Core.Templates;

// Lưu loại và dữ liệu số nguyên của một mục trong bộ ô bản đồ.
// Nguồn Java: nro/template/TileSetInfoData.java (giữ nguyên tham chiếu mảng data được truyền vào, không sao chép; Java chỉ có hàm tạo hai tham số).
public class TileSetInfoData
{
    // Loại dữ liệu (Java: type)
    public int Type { get; set; }

    // Mảng dữ liệu, giữ nguyên tham chiếu được truyền vào (Java: data)
    public int[] Data { get; set; }

    public TileSetInfoData(int type, int[] data)
    {
        Type = type;
        Data = data;
    }
}
