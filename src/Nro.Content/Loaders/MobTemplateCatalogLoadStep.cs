using Nro.Content.StaticData;
using Nro.Core.Templates;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nguồn Java: nro/server/Manager.java (dòng 742–759).
// Nạp mẫu quái và giữ byte có dấu Java bằng sbyte. (Java: MOB_TEMPLATES)
public sealed class MobTemplateCatalogLoadStep : ICatalogLoadStep
{
    // Tên bước và khóa ExpectedCounts. (Java: mob_template)
    public const string StepName = "mob-template";
    private readonly IStaticRowSource source;
    private readonly WorldCatalogLoadState state;

    // Tạo bước nạp từ nguồn hàng và trạng thái J-01E cục bộ. (Java: mob_template)
    internal MobTemplateCatalogLoadStep(IStaticRowSource source, WorldCatalogLoadState state)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
        this.state = state ?? throw new ArgumentNullException(nameof(state));
    }

    // Tên bước nạp. (Java: mob_template)
    public string Name => StepName;

    // Mẫu quái không phụ thuộc catalog khác. (Java: loadDatabase)
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    // Chuyển hàng dữ liệu thành mẫu quái đã xác thực. (Java: rs.getByte)
    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<MobTemplateRow> rows = await source.ReadMobTemplatesAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var mobs = new List<MobTemplate>(rows.Count);
        foreach (MobTemplateRow row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StaticCatalogValidation.Unique(ids, row.MobTemplateId, StepName);
            mobs.Add(new MobTemplate
            {
                Id = row.MobTemplateId, Type = StaticCatalogValidation.SByte(row.Type, StepName, "type", row.MobTemplateId), Name = row.Name ?? String.Empty, Hp = row.Hp, RangeMove = StaticCatalogValidation.SByte(row.RangeMove, StepName, "range_move", row.MobTemplateId), Speed = StaticCatalogValidation.SByte(row.Speed, StepName, "speed", row.MobTemplateId), DartType = StaticCatalogValidation.SByte(row.DartType, StepName, "dart_type", row.MobTemplateId), PercentDame = StaticCatalogValidation.SByte(row.DamagePercent, StepName, "percent_dame", row.MobTemplateId), PercentPotential = StaticCatalogValidation.SByte(row.PercentPotential, StepName, "percent_tiem_nang", row.MobTemplateId), Percent_gold = StaticCatalogValidation.SByte(row.PercentGold, StepName, "percent_gold", row.MobTemplateId),
            });
        }

        StaticCatalogValidation.ContinuousZeroBasedIds(ids, StepName);
        state.Mobs = mobs.OrderBy(mob => mob.Id).ToArray();
        return new CatalogStepResult(mobs.Count);
    }
}
