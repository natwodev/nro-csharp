using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Skills;

// Đại diện một lớp nhân vật và các mẫu kỹ năng của lớp theo thứ tự nạp nclass_id, slot.
// Nguồn Java: nro/skill/NClass.java (giữ nguyên thứ tự dữ liệu từ trình nạp).
public class NClass
{
    // Mã lớp nhân vật.
    public int ClassId { get; set; }

    // Tên lớp nhân vật.
    public string Name { get; set; } = "";

    // Danh sách mẫu kỹ năng của lớp (Java: skillTemplatess).
    public List<SkillTemplate> SkillTemplatess { get; set; } = [];

    // Tìm mẫu đầu tiên có mã yêu cầu, không có thì trả null (Java: getSkillTemplate).
    public SkillTemplate? GetSkillTemplate(int tempId)
    {
        foreach (SkillTemplate skillTemplate in SkillTemplatess)
        {
            if (skillTemplate.Id == tempId)
            {
                return skillTemplate;
            }
        }

        return null;
    }

    // Tìm mẫu đầu tiên có tên không dấu, viết hoa chứa chuỗi tìm; chuỗi rỗng khớp phần tử đầu tiên. Dùng ToUpperInvariant thay locale máy Java, khác biệt chỉ ảnh hưởng ký tự ngoài tên kỹ năng.
    public SkillTemplate? GetSkillTemplateByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        string wanted = Util.RemoveAccent(name).ToUpperInvariant();
        foreach (SkillTemplate skillTemplate in SkillTemplatess)
        {
            if (Util.RemoveAccent(skillTemplate.Name).ToUpperInvariant().Contains(wanted, StringComparison.Ordinal))
            {
                return skillTemplate;
            }
        }

        return null;
    }
}
