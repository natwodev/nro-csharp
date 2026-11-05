using Nro.Content.StaticData;
using Nro.Core.Templates;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nguồn Java: nro/server/Manager.java (dòng 761–774).
// Nạp mẫu NPC, mô phỏng getInt trả 0 khi avatar SQL là NULL. (Java: NPC_TEMPLATES)
public sealed class NpcTemplateCatalogLoadStep : ICatalogLoadStep
{
    // Tên bước và khóa ExpectedCounts. (Java: npc_template)
    public const string StepName = "npc-template";
    private readonly IStaticRowSource source;
    private readonly WorldCatalogLoadState state;

    // Tạo bước nạp từ nguồn hàng và trạng thái J-01E cục bộ. (Java: npc_template)
    internal NpcTemplateCatalogLoadStep(IStaticRowSource source, WorldCatalogLoadState state)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
        this.state = state ?? throw new ArgumentNullException(nameof(state));
    }

    // Tên bước nạp. (Java: npc_template)
    public string Name => StepName;

    // Mẫu NPC không phụ thuộc catalog khác. (Java: loadDatabase)
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    // Chuyển hàng dữ liệu thành mẫu NPC đã xác thực. (Java: rs.getByte, rs.getShort, rs.getInt)
    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<NpcTemplateRow> rows = await source.ReadNpcTemplatesAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var npcs = new List<NpcTemplate>(rows.Count);
        foreach (NpcTemplateRow row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StaticCatalogValidation.Unique(ids, row.NpcTemplateId, StepName);
            int id = StaticCatalogValidation.SByte(row.NpcTemplateId, StepName, "id", row.NpcTemplateId);
            npcs.Add(new NpcTemplate
            {
                Id = id, Name = row.Name ?? String.Empty, Head = StaticCatalogValidation.Short(row.Head, StepName, "head", row.NpcTemplateId), Body = StaticCatalogValidation.Short(row.Body, StepName, "body", row.NpcTemplateId), Leg = StaticCatalogValidation.Short(row.Leg, StepName, "leg", row.NpcTemplateId), Avatar = row.Avatar ?? 0,
            });
        }

        StaticCatalogValidation.ContinuousZeroBasedIds(ids, StepName);
        state.Npcs = npcs.OrderBy(npc => npc.Id).ToArray();
        return new CatalogStepResult(npcs.Count);
    }
}
