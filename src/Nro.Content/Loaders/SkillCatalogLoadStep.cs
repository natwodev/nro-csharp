using System.Text.Json;
using Nro.Content.Catalogs;
using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;
using Nro.Game.Skills;

namespace Nro.Content.Loaders;

// Nạp class/template/cấp kỹ năng từ JSON sau khi khóa thứ tự truy vấn. Nguồn Java: nro/server/Manager.java (dòng 437–488).
public sealed class SkillCatalogLoadStep : ICatalogLoadStep
{
    public const string StepName = "skill_template";
    private readonly IStaticRowSource source;

    public SkillCatalogLoadStep(IStaticRowSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        this.source = source;
    }

    public string Name => StepName;
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<SkillTemplateRow> rows = await source.ReadSkillTemplatesAsync(cancellationToken).ConfigureAwait(false);
        var classes = new Dictionary<int, NClass>();
        var templateIds = new HashSet<(int ClassId, int TemplateId)>();
        foreach (SkillTemplateRow row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int classId = StaticCatalogValidation.SByte(row.NclassId, StepName, "nclass_id", row.SkillTemplateId);
            int templateId = StaticCatalogValidation.SByte(row.SkillTemplateId, StepName, "id", row.SkillTemplateId);
            if (!templateIds.Add((classId, templateId)))
            {
                throw new CatalogLoadException(StepName, $"Template kỹ năng lớp {classId} id {templateId} bị trùng.");
            }

            if (!classes.TryGetValue(classId, out NClass? nclass))
            {
                nclass = new NClass { ClassId = classId, Name = ClassName(classId) };
                classes.Add(classId, nclass);
            }

            var template = new SkillTemplate
            {
                ClassId = classId,
                Id = StaticCatalogValidation.SByte(row.SkillTemplateId, StepName, "id", row.SkillTemplateId),
                Name = row.Name,
                MaxPoint = StaticCatalogValidation.SByte(row.MaxPoint, StepName, "max_point", row.SkillTemplateId),
                ManaUseType = StaticCatalogValidation.SByte(row.ManaUseType, StepName, "mana_use_type", row.SkillTemplateId),
                Type = StaticCatalogValidation.SByte(row.Type, StepName, "type", row.SkillTemplateId),
                IconId = StaticCatalogValidation.Short(row.IconId, StepName, "icon_id", row.SkillTemplateId),
                DamInfo = row.DamInfo,
            };
            foreach (Skill skill in SkillLevels(row.Skills, template, row.SkillTemplateId))
            {
                template.Skillss.Add(skill);
            }

            template.Skills = template.Skillss.ToArray();
            nclass.SkillTemplatess.Add(template);
        }

        StaticCatalogValidation.ContinuousZeroBasedIds(classes.Keys.OrderBy(value => value), StepName);
        builder.SetSkills(new SkillCatalog(classes.Values));
        return new CatalogStepResult(rows.Count);
    }

    private static string ClassName(int id) => id switch { 0 => "Trái Đất", 1 => "Namếc", _ => "Xayda" };

    private static IReadOnlyList<Skill> SkillLevels(string json, SkillTemplate template, int rowId)
    {
        try
        {
            string normalized = json.Replace("[\"", "[", StringComparison.Ordinal).Replace("\"[", "[", StringComparison.Ordinal).Replace("\"]", "]", StringComparison.Ordinal).Replace("]\"", "]", StringComparison.Ordinal).Replace("}\",\"{", "},{", StringComparison.Ordinal);
            using JsonDocument document = JsonDocument.Parse(normalized);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new CatalogLoadException(StepName, $"Hàng id {rowId} có skills phải là mảng JSON.");
            }

            var levels = new List<Skill>(document.RootElement.GetArrayLength());
            foreach (JsonElement value in document.RootElement.EnumerateArray())
            {
                if (value.ValueKind != JsonValueKind.Object)
                {
                    throw new CatalogLoadException(StepName, $"Hàng id {rowId} có cấp kỹ năng không phải object JSON.");
                }

                levels.Add(new Skill
                {
                    Template = template,
                    SkillId = Short(value, "id", rowId),
                    Point = SByte(value, "point", rowId),
                    PowRequire = Long(value, "power_require", rowId),
                    ManaUse = Int(value, "mana_use", rowId),
                    CoolDown = Int(value, "cool_down", rowId),
                    Dx = Int(value, "dx", rowId),
                    Dy = Int(value, "dy", rowId),
                    MaxFight = Int(value, "max_fight", rowId),
                    Damage = Short(value, "damage", rowId),
                    Price = Short(value, "price", rowId),
                    MoreInfo = Info(value, rowId),
                });
            }

            return levels;
        }
        catch (JsonException exception)
        {
            throw new CatalogLoadException(StepName, $"Hàng id {rowId} có skills JSON không hợp lệ.", exception);
        }
    }

    private static int Int(JsonElement parent, string name, int rowId) => Property(parent, name, rowId).TryGetInt32(out int value) ? value : throw Invalid(rowId, name, "Int32");
    private static long Long(JsonElement parent, string name, int rowId) => Property(parent, name, rowId).TryGetInt64(out long value) ? value : throw Invalid(rowId, name, "Int64");
    private static short Short(JsonElement parent, string name, int rowId) => StaticCatalogValidation.Short(Int(parent, name, rowId), StepName, name, rowId);
    private static sbyte SByte(JsonElement parent, string name, int rowId) => StaticCatalogValidation.SByte(Int(parent, name, rowId), StepName, name, rowId);
    private static string Info(JsonElement parent, int rowId)
    {
        JsonElement value = Property(parent, "info", rowId);
        return value.ValueKind == JsonValueKind.String ? value.GetString() ?? "null" : value.GetRawText();
    }
    private static JsonElement Property(JsonElement parent, string name, int rowId) => parent.TryGetProperty(name, out JsonElement value) ? value : throw new CatalogLoadException(StepName, $"Hàng id {rowId} thiếu skills.{name}.");
    private static CatalogLoadException Invalid(int rowId, string name, string type) => new(StepName, $"Hàng id {rowId} có skills.{name} phải là {type} JSON.");
}
