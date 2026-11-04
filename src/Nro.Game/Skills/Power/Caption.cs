namespace Nro.Game.Skills.Power;

// Lưu danh hiệu theo mốc sức mạnh với văn bản riêng cho từng hành tinh.
// Nguồn Java: nro/power/Caption.java (Lombok được thay bằng constructor và thuộc tính C#).
public class Caption
{
    public int Id { get; set; }

    public string Earth { get; set; }

    public string Saiya { get; set; }

    public string Namek { get; set; }

    public long Power { get; set; }

    public Caption(int id, string earth, string saiya, string namek, long power)
    {
        Id = id;
        Earth = earth;
        Saiya = saiya;
        Namek = namek;
        Power = power;
    }

    // Chọn Namek cho 1, Saiyan cho 2, Trái Đất cho 0 hay giá trị lạ (Java: getCaption).
    public string GetCaption(int planet)
    {
        string caption = Earth;
        if (planet == 1)
        {
            caption = Namek;
        }
        else if (planet == 2)
        {
            caption = Saiya;
        }

        return caption;
    }
}
    // Mã danh hiệu, ba văn bản theo hệ và ngưỡng sức mạnh tối thiểu.
    // Tạo danh hiệu với toàn bộ văn bản và ngưỡng.
