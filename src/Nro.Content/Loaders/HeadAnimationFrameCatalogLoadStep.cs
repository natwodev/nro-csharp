using Nro.Core.Templates;
using Nro.Content.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp frame hoạt ảnh đầu theo id liên tục để giữ hợp đồng tra vị trí. Nguồn Java: nro/server/Manager.java (dòng 590–600).
public sealed class HeadAnimationFrameCatalogLoadStep : ICatalogLoadStep
{
    // Tên bước và khóa ExpectedCounts. (Java: array_head_2_frames)
    public const string StepName = "head_animation_frame";
    private readonly IStaticRowSource source;
    private readonly AppearanceItemCatalogLoadState state;

    // Tạo bước nạp từ nguồn hàng được tiêm. (Java: array_head_2_frames)
    internal HeadAnimationFrameCatalogLoadStep(IStaticRowSource source, AppearanceItemCatalogLoadState state)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(state);
        this.source = source;
        this.state = state;
    }

    // Tên bước. (Java: array_head_2_frames)
    public string Name => StepName;

    // Frame đầu không phụ thuộc catalog khác khi đọc. (Java: loadDatabase)
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    // Parse data JSON thành danh sách int và kiểm id liên tục. (Java: ARR_HEAD_2_FRAMES)
    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<Nro.Data.Entities.StaticData.HeadAnimationFrameRow> rows = await source.ReadHeadAnimationFramesAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var frames = new List<ArrHead2Frames>(rows.Count);
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StaticCatalogValidation.Unique(ids, row.HeadAnimationFrameId, StepName);
            frames.Add(new ArrHead2Frames { Frames = StaticCatalogJson.Frames(row.Data, StepName, row.HeadAnimationFrameId) });
        }

        StaticCatalogValidation.ContinuousIds(rows.Select(row => row.HeadAnimationFrameId), StepName);
        state.HeadFrames = frames.AsReadOnly();
        return new CatalogStepResult(frames.Count);
    }
}
