using Nro.Content.Catalogs;
using Nro.Content.StaticData;
using Nro.Core.Templates;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;
using Nro.Game.Skills.Attributes;
using Nro.Game.Skills.Intrinsics;
using Nro.Game.Skills.Power;

namespace Nro.Content.Loaders;

// Nạp các bảng tiến trình tĩnh vào một catalog duy nhất. Nguồn Java: nro/server/Manager.java (dòng 199–202, 531–563, 593–599).
public sealed class ProgressionCatalogLoadStep : ICatalogLoadStep
{
    public const string StepName = "progression_catalog";
    private readonly IStaticRowSource source;

    public ProgressionCatalogLoadStep(IStaticRowSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        this.source = source;
    }

    public string Name => StepName;
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<AttributeTemplateRow> attributeRows = await source.ReadAttributeTemplatesAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<PowerLimitRow> powerRows = await source.ReadPowerLimitsAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<CaptionRow> captionRows = await source.ReadCaptionsAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<IntrinsicRow> intrinsicRows = await source.ReadIntrinsicsAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<AchievementTemplateRow> achievementRows = await source.ReadAchievementTemplatesAsync(cancellationToken).ConfigureAwait(false);
        var attributeIds = new HashSet<int>();
        var powerIds = new HashSet<int>();
        var captionIds = new HashSet<int>();
        var intrinsicIds = new HashSet<int>();
        var achievementIds = new HashSet<int>();
        var attributes = new List<AttributeTemplate>(attributeRows.Count);
        var powers = new List<PowerLimit>(powerRows.Count);
        var captions = new List<Caption>(captionRows.Count);
        var intrinsics = new List<Intrinsic>(intrinsicRows.Count);
        var achievements = new List<AchievementTemplate>(achievementRows.Count);
        foreach (AttributeTemplateRow row in attributeRows)
        {
            StaticCatalogValidation.Unique(attributeIds, row.AttributeTemplateId, StepName);
            attributes.Add(new AttributeTemplate(row.AttributeTemplateId, row.Name));
        }

        foreach (PowerLimitRow row in powerRows)
        {
            StaticCatalogValidation.Unique(powerIds, row.PowerLimitId, StepName);
            powers.Add(new PowerLimit(StaticCatalogValidation.Short(row.PowerLimitId, StepName, "id", row.PowerLimitId), row.Power, row.Hp, row.Mp, row.Damage, row.Defense, row.Critical));
        }

        StaticCatalogValidation.ContinuousZeroBasedIds(powers.Select(value => value.Id), StepName);
        foreach (CaptionRow row in captionRows)
        {
            StaticCatalogValidation.Unique(captionIds, row.CaptionId, StepName);
            captions.Add(new Caption(StaticCatalogValidation.Short(row.CaptionId, StepName, "id", row.CaptionId), row.Earth, row.Saiya, row.Namek, row.Power));
        }

        StaticCatalogValidation.ContinuousZeroBasedIds(captions.Select(value => value.Id), StepName);
        foreach (IntrinsicRow row in intrinsicRows)
        {
            StaticCatalogValidation.Unique(intrinsicIds, row.IntrinsicId, StepName);
            intrinsics.Add(new Intrinsic
            {
                Id = StaticCatalogValidation.SByte(row.IntrinsicId, StepName, "id", row.IntrinsicId),
                Name = row.Name,
                ParamFrom1 = StaticCatalogValidation.Short(row.ParamFrom1, StepName, "param_from_1", row.IntrinsicId),
                ParamTo1 = StaticCatalogValidation.Short(row.ParamTo1, StepName, "param_to_1", row.IntrinsicId),
                ParamFrom2 = StaticCatalogValidation.Short(row.ParamFrom2, StepName, "param_from_2", row.IntrinsicId),
                ParamTo2 = StaticCatalogValidation.Short(row.ParamTo2, StepName, "param_to_2", row.IntrinsicId),
                Icon = StaticCatalogValidation.Short(row.Icon, StepName, "icon", row.IntrinsicId),
                Gender = StaticCatalogValidation.SByte(row.Gender, StepName, "gender", row.IntrinsicId),
            });
        }

        foreach (AchievementTemplateRow row in achievementRows)
        {
            StaticCatalogValidation.Unique(achievementIds, row.AchievementTemplateId, StepName);
            achievements.Add(new AchievementTemplate(row.Info1, row.Info2, row.Money, row.MaxCount));
        }

        StaticCatalogValidation.ContinuousIds(achievementRows.Select(row => row.AchievementTemplateId), StepName);

        builder.SetProgression(new ProgressionCatalog(attributes, powers, captions, intrinsics, achievements));
        return new CatalogStepResult(attributeRows.Count + powerRows.Count + captionRows.Count + intrinsicRows.Count + achievementRows.Count);
    }
}
