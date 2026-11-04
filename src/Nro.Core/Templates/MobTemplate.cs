namespace Nro.Core.Templates;

// Mẫu quái, được nạp từ bảng mob_template; các byte có dấu của Java được giữ bằng sbyte.
// Nguồn Java: nro/template/MobTemplate.java (tên mặc định chuỗi rỗng thay cho null của Java và chỉ viết hoa ký tự đầu của percent_gold).
public class MobTemplate
{
    // Mã mẫu quái (Java: id)
    public int Id { get; set; }

    // Kiểu quái (Java: type)
    public sbyte Type { get; set; }

    // Tên quái, mặc định chuỗi rỗng thay cho null của Java (Java: name)
    public string Name { get; set; } = "";

    // Máu cơ bản của quái (Java: hp)
    public int Hp { get; set; }

    // Phạm vi di chuyển (Java: rangeMove)
    public sbyte RangeMove { get; set; }

    // Tốc độ di chuyển (Java: speed)
    public sbyte Speed { get; set; }

    // Kiểu phi tiêu/tấn công xa (Java: dartType)
    public sbyte DartType { get; set; }

    // Phần trăm sát thương (Java: percentDame)
    public sbyte PercentDame { get; set; }

    // Phần trăm tiềm năng (Java: percentTiemNang)
    public sbyte PercentPotential { get; set; }

    // Phần trăm vàng nhận được (Java: percent_gold)
    public sbyte Percent_gold { get; set; }
}
