namespace Nro.Game.Skills;

// Mô tả mẫu kỹ năng dùng chung cho các cấp kỹ năng; sau khi nạp chỉ được đọc vì bản sao của người chơi dùng chung mẫu.
// Nguồn Java: nro/template/SkillTemplate.java (đặt ở Nro.Game thay vì Nro.Core vì tham chiếu hai chiều với Skill).
public class SkillTemplate
{
    // Mã mẫu kỹ năng kiểu byte có dấu của Java.
    public sbyte Id { get; set; }

    // Mã lớp nhân vật sở hữu kỹ năng.
    public int ClassId { get; set; }

    // Tên hiển thị của kỹ năng.
    public string Name { get; set; } = "";

    // Số điểm tối đa có thể nâng.
    public int MaxPoint { get; set; }

    // Kiểu tiêu hao mana khi thi triển.
    public int ManaUseType { get; set; }

    // Loại kỹ năng theo dữ liệu gốc.
    public int Type { get; set; }

    // Mã biểu tượng trên giao diện.
    public int IconId { get; set; }

    // Các dòng mô tả theo cấp.
    public string[]? Description { get; set; }

    // Mảng skills của Java chưa từng được nạp; giữ lại để tương thích, còn Skillss mới được sử dụng.
    public Skill[]? Skills { get; set; }

    // Các cấp của kỹ năng theo thứ tự nạp (Java: skillss).
    public List<Skill> Skillss { get; set; } = [];

    // Thông tin sát thương bổ sung.
    public string? DamInfo { get; set; }
}
