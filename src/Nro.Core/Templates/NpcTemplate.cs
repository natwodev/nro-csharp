namespace Nro.Core.Templates;

// Mẫu NPC, được nạp từ bảng npc_template.
// Nguồn Java: nro/template/NpcTemplate.java (tên mặc định chuỗi rỗng thay cho null của Java).
public class NpcTemplate
{
    // Mã mẫu NPC (Java: id)
    public int Id { get; set; }

    // Tên NPC, mặc định chuỗi rỗng thay cho null của Java (Java: name)
    public string Name { get; set; } = "";

    // Mã đầu của NPC (Java: head)
    public int Head { get; set; }

    // Mã thân của NPC (Java: body)
    public int Body { get; set; }

    // Mã chân của NPC (Java: leg)
    public int Leg { get; set; }

    // Mã ảnh đại diện của NPC (Java: avatar)
    public int Avatar { get; set; }
}
