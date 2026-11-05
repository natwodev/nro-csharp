using System.Collections.ObjectModel;
using Nro.Game.Content;
using Nro.Game.Skills;

namespace Nro.Content.Catalogs;

// Danh mục kỹ năng đóng băng theo lớp và template. Nguồn Java: nro/server/Manager.java (dòng 437–488).
public sealed class SkillCatalog : ISkillCatalog
{
    private readonly IReadOnlyDictionary<int, NClass> classesById;

    // Sao chép sâu các lớp trước khi đóng để mã gọi không thể sửa snapshot. (Java: NCLASS)
    public SkillCatalog(IEnumerable<NClass> classes)
    {
        ArgumentNullException.ThrowIfNull(classes);
        var copy = new Dictionary<int, NClass>();
        foreach (NClass item in classes)
        {
            if (!copy.TryAdd(item.ClassId, Clone(item)))
            {
                throw new ArgumentException($"Mã lớp kỹ năng bị trùng: {item.ClassId}.", nameof(classes));
            }
        }

        classesById = new ReadOnlyDictionary<int, NClass>(copy);
    }

    // Trả bản sao sâu để giữ catalog bất biến. (Java: NCLASS)
    public IReadOnlyList<NClass> Classes => Array.AsReadOnly(classesById.Values.OrderBy(value => value.ClassId).Select(Clone).ToArray());

    // Lấy lớp bằng mã. (Java: NCLASS)
    public NClass GetClass(int classId) => classesById.TryGetValue(classId, out NClass? value)
        ? Clone(value)
        : throw new KeyNotFoundException($"Không có lớp kỹ năng id {classId}.");

    // Lấy template thuộc lớp, không cho template của lớp khác lẫn vào. (Java: NClass.getSkillTemplate)
    public SkillTemplate GetTemplate(int classId, int templateId)
    {
        NClass value = GetClass(classId);
        return value.GetSkillTemplate(templateId)
            ?? throw new KeyNotFoundException($"Không có template kỹ năng id {templateId} trong lớp {classId}.");
    }

    private static NClass Clone(NClass source)
    {
        var result = new NClass { ClassId = source.ClassId, Name = source.Name };
        foreach (SkillTemplate template in source.SkillTemplatess)
        {
            result.SkillTemplatess.Add(Clone(template));
        }

        return result;
    }

    private static SkillTemplate Clone(SkillTemplate source)
    {
        var result = new SkillTemplate
        {
            Id = source.Id,
            ClassId = source.ClassId,
            Name = source.Name,
            MaxPoint = source.MaxPoint,
            ManaUseType = source.ManaUseType,
            Type = source.Type,
            IconId = source.IconId,
            Description = source.Description?.ToArray(),
            DamInfo = source.DamInfo,
        };
        foreach (Skill skill in source.Skillss)
        {
            result.Skillss.Add(Clone(skill, result));
        }

        result.Skills = result.Skillss.ToArray();
        return result;
    }

    private static Skill Clone(Skill source, SkillTemplate template) => new()
    {
        Template = template,
        SkillId = source.SkillId,
        Point = source.Point,
        PowRequire = source.PowRequire,
        CoolDown = source.CoolDown,
        LastTimeUseThisSkill = source.LastTimeUseThisSkill,
        LastTimeUseThisSkillbot = source.LastTimeUseThisSkillbot,
        Dx = source.Dx,
        Dy = source.Dy,
        MaxFight = source.MaxFight,
        ManaUse = source.ManaUse,
        Damage = source.Damage,
        CurrLevel = source.CurrLevel,
        MoreInfo = source.MoreInfo,
        Price = source.Price,
    };
}
