using Nro.Game.Skills;

namespace Nro.Game.Content;

// Danh mục kỹ năng chỉ đọc theo lớp. Nguồn Java: nro/server/Manager.java (dòng 439–488).
public interface ISkillCatalog
{
    // Các lớp kỹ năng theo nclass_id. (Java: NCLASS)
    IReadOnlyList<NClass> Classes { get; }

    // Lấy lớp theo mã định danh. (Java: NCLASS)
    NClass GetClass(int classId);

    // Lấy template kỹ năng trong lớp đã chỉ rõ. (Java: NClass.getSkillTemplate)
    SkillTemplate GetTemplate(int classId, int templateId);
}
