using System.Collections.ObjectModel;
using Nro.Core.Templates;
using Nro.Game.Content;
using Nro.Game.Progress.Achievements;
using Nro.Game.Skills.Attributes;
using Nro.Game.Skills.Intrinsics;
using Nro.Game.Skills.Power;

namespace Nro.Content.Catalogs;

// Danh mục tiến trình đóng băng dữ liệu sau khi validate. Nguồn Java: nro/server/Manager.java (dòng 199–202, 531–563, 593–599).
public sealed class ProgressionCatalog : IProgressionCatalog
{
    private readonly IReadOnlyList<AttributeTemplate> attributes;
    private readonly IReadOnlyList<PowerLimit> powerLimits;
    private readonly IReadOnlyList<Caption> captions;
    private readonly IReadOnlyList<Intrinsic> intrinsics;
    private readonly IReadOnlyList<AchievementTemplate> achievements;
    private readonly IReadOnlyDictionary<int, AttributeTemplate> attributesById;
    private readonly IReadOnlyDictionary<int, Caption> captionsById;
    private readonly IReadOnlyList<Intrinsic> earth;
    private readonly IReadOnlyList<Intrinsic> namek;
    private readonly IReadOnlyList<Intrinsic> saiyan;

    // Đóng băng tất cả collection và phân nội tại theo gender Java. (Java: INTRINSIC_TD/NM/XD)
    public ProgressionCatalog(IEnumerable<AttributeTemplate> attributes, IEnumerable<PowerLimit> powerLimits, IEnumerable<Caption> captions, IEnumerable<Intrinsic> intrinsics, IEnumerable<AchievementTemplate> achievements)
    {
        ArgumentNullException.ThrowIfNull(attributes);
        ArgumentNullException.ThrowIfNull(powerLimits);
        ArgumentNullException.ThrowIfNull(captions);
        ArgumentNullException.ThrowIfNull(intrinsics);
        ArgumentNullException.ThrowIfNull(achievements);
        this.attributes = Array.AsReadOnly(attributes.ToArray());
        this.powerLimits = Array.AsReadOnly(powerLimits.Select(Clone).ToArray());
        this.captions = Array.AsReadOnly(captions.Select(Clone).ToArray());
        this.intrinsics = Array.AsReadOnly(intrinsics.Select(Clone).ToArray());
        this.achievements = Array.AsReadOnly(achievements.Select(Clone).ToArray());
        attributesById = new ReadOnlyDictionary<int, AttributeTemplate>(this.attributes.ToDictionary(value => value.Id));
        captionsById = new ReadOnlyDictionary<int, Caption>(this.captions.ToDictionary(value => value.Id));
        earth = Array.AsReadOnly(this.intrinsics.Where(value => value.Gender is 0 or < 0 or > 2).Select(Clone).ToArray());
        namek = Array.AsReadOnly(this.intrinsics.Where(value => value.Gender is 1 or < 0 or > 2).Select(Clone).ToArray());
        saiyan = Array.AsReadOnly(this.intrinsics.Where(value => value.Gender is 2 or < 0 or > 2).Select(Clone).ToArray());
    }

    public IReadOnlyList<AttributeTemplate> AttributeTemplates => attributes;
    public IReadOnlyList<PowerLimit> PowerLimits => Array.AsReadOnly(powerLimits.Select(Clone).ToArray());
    public IReadOnlyList<Caption> Captions => Array.AsReadOnly(captions.Select(Clone).ToArray());
    public IReadOnlyList<Intrinsic> Intrinsics => Array.AsReadOnly(intrinsics.Select(Clone).ToArray());
    public IReadOnlyList<AchievementTemplate> Achievements => Array.AsReadOnly(achievements.Select(Clone).ToArray());

    public AttributeTemplate GetAttributeTemplate(int id) => attributesById.TryGetValue(id, out AttributeTemplate? value)
        ? value
        : throw new KeyNotFoundException($"Không có mẫu thuộc tính id {id}.");

    public PowerLimit GetPowerLimit(int index) => (uint)index < (uint)powerLimits.Count
        ? Clone(powerLimits[index])
        : throw new ArgumentOutOfRangeException(nameof(index), index, "Không có mốc giới hạn sức mạnh tại chỉ mục đã cho.");

    public Caption? FindCaption(int id) => captionsById.TryGetValue(id, out Caption? value) ? Clone(value) : null;

    // Java CaptionManager.findLevel(level) gọi captions.get(level); cột caption.id thật bắt đầu từ 1.
    public Caption GetCaptionAtLevel(int index) => (uint)index < (uint)captions.Count
        ? Clone(captions[index])
        : throw new ArgumentOutOfRangeException(nameof(index), index, "Không có danh hiệu tại cấp đã cho.");

    public IReadOnlyList<Intrinsic> GetIntrinsics(sbyte gender)
    {
        IReadOnlyList<Intrinsic> selected = gender switch { 0 => earth, 1 => namek, _ => saiyan };
        return Array.AsReadOnly(selected.Select(Clone).ToArray());
    }

    public AchievementTemplate GetAchievement(int index) => (uint)index < (uint)achievements.Count
        ? Clone(achievements[index])
        : throw new ArgumentOutOfRangeException(nameof(index), index, "Không có thành tựu tại chỉ mục đã cho.");

    private static PowerLimit Clone(PowerLimit value) => new(value.Id, value.Power, value.Hp, value.Mp, value.Damage, value.Defense, value.Critical);
    private static Caption Clone(Caption value) => new(value.Id, value.Earth, value.Saiya, value.Namek, value.Power);
    private static Intrinsic Clone(Intrinsic value) => new()
    {
        Id = value.Id, Name = value.Name, ParamFrom1 = value.ParamFrom1, ParamTo1 = value.ParamTo1, ParamFrom2 = value.ParamFrom2, ParamTo2 = value.ParamTo2, Icon = value.Icon, Gender = value.Gender, Param1 = value.Param1, Param2 = value.Param2,
    };
    private static AchievementTemplate Clone(AchievementTemplate value) => new(value.Info1, value.Info2, value.Money, value.MaxCount);
}
