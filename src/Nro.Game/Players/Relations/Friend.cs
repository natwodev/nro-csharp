namespace Nro.Game.Players.Relations;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Friend.java (dòng 4–15).
public class Friend
{
    // Lưu mã người bạn (Java: id).
    public int Id { get; set; }

    // Lưu tên người bạn; Java không khởi tạo nên có thể null (Java: name).
    public string? Name { get; set; }

    // Lưu mã đầu trang phục (Java: head).
    public short Head { get; set; }

    // Lưu mã thân trang phục (Java: body).
    public short Body { get; set; }

    // Lưu mã chân trang phục (Java: leg).
    public short Leg { get; set; }

    // Lưu trạng thái túi/cờ giao diện byte có dấu như Java (Java: bag).
    public sbyte Bag { get; set; }

    // Lưu sức mạnh người bạn (Java: power).
    public long Power { get; set; }

    // Lưu trạng thái trực tuyến (Java: online).
    public bool Online { get; set; }
}
