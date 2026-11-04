namespace Nro.Core.Templates;

// Lưu danh sách dữ liệu của một bộ ô bản đồ.
// Nguồn Java: nro/template/TileSetInfo.java (giữ tên TileSetInfoData theo trường tileSetInfoData của Java dù trùng tên lớp; kiểu List khác kiểu phần tử và Java chỉ có hàm tạo một tham số).
public class TileSetInfo
{
    // Danh sách dữ liệu bộ ô (Java: tileSetInfoData)
    public List<TileSetInfoData> TileSetInfoData { get; set; }

    public TileSetInfo(List<TileSetInfoData> tileSetInfoData)
    {
        TileSetInfoData = tileSetInfoData;
    }
}
