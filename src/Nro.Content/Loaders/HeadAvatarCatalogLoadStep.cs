using Nro.Core.Templates;
using Nro.Content.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp avatar đầu vào trạng thái cục bộ. Nguồn Java: nro/server/Manager.java (dòng 560–569).
public sealed class HeadAvatarCatalogLoadStep : ICatalogLoadStep
{
    // Tên bước và khóa ExpectedCounts. (Java: head_avatar)
    public const string StepName = "head_avatar";
    private readonly IStaticRowSource source;
    private readonly AppearanceItemCatalogLoadState state;

    // Tạo bước nạp từ nguồn hàng được tiêm. (Java: head_avatar)
    internal HeadAvatarCatalogLoadStep(IStaticRowSource source, AppearanceItemCatalogLoadState state)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(state);
        this.source = source;
        this.state = state;
    }

    // Tên bước. (Java: head_avatar)
    public string Name => StepName;

    // Avatar đầu không phụ thuộc catalog khác khi đọc. (Java: loadDatabase)
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    // Chuyển các hàng avatar và kiểm id đầu không trùng. (Java: HEAD_AVATARS)
    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<Nro.Data.Entities.StaticData.HeadAvatarRow> rows = await source.ReadHeadAvatarsAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var avatars = new List<HeadAvatar>(rows.Count);
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StaticCatalogValidation.Unique(ids, row.HeadId, StepName);
            avatars.Add(new HeadAvatar(row.HeadId, row.AvatarId));
        }

        state.HeadAvatars = avatars.AsReadOnly();
        return new CatalogStepResult(avatars.Count);
    }
}
