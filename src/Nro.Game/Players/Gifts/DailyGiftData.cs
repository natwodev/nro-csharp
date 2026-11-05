namespace Nro.Game.Players.Gifts;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/DailyGift/DailyGiftData.java (dòng 3–17).
public class DailyGiftData
{
    // Lưu mã phần quà hằng ngày, mặc định chưa gán là -1 (Java: id).
    public sbyte Id { get; set; } = -1;

    // Lưu trạng thái đã nhận phần quà (Java: daNhan).
    public bool Received { get; set; }

    // Khởi tạo dữ liệu phần quà với mã -1 và cờ chưa nhận (Java: DailyGiftData).
    public DailyGiftData()
    {
        Id = -1;
        Received = false;
    }

    // Tạo JSON dạng chuỗi với đúng tên khóa và kiểu chuỗi của Java (Java: toString).
    public override string ToString() => $"{{\"id\":\"{Id}\",\"daNhan\":\"{(Received ? "true" : "false")}\"}}";
}
