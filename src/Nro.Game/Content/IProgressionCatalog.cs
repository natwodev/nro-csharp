using Nro.Core.Templates;
using Nro.Game.Progress.Achievements;
using Nro.Game.Skills.Attributes;
using Nro.Game.Skills.Intrinsics;
using Nro.Game.Skills.Power;

namespace Nro.Game.Content;

// Danh mục chỉ đọc cho thuộc tính, sức mạnh, nội tại và thành tựu. Nguồn Java: nro/server/Manager.java (dòng 199–202, 531–563, 593–599).
public interface IProgressionCatalog
{
    // Các mẫu thuộc tính toàn máy chủ. (Java: AttributeTemplateManager.list)
    IReadOnlyList<AttributeTemplate> AttributeTemplates { get; }

    // Các mốc giới hạn sức mạnh theo chỉ mục. (Java: PowerLimitManager.powers)
    IReadOnlyList<PowerLimit> PowerLimits { get; }

    // Danh hiệu theo thứ tự nạp; id của phần tử không phải chỉ số. (Java: CaptionManager.load/findLevel/getLevel)
    IReadOnlyList<Caption> Captions { get; }

    // Toàn bộ nội tại theo id. (Java: INTRINSICS)
    IReadOnlyList<Intrinsic> Intrinsics { get; }

    // Thành tựu theo chỉ mục lưu của người chơi. (Java: ACHIEVEMENT_TEMPLATE)
    IReadOnlyList<AchievementTemplate> Achievements { get; }

    // Lấy mẫu thuộc tính theo id. (Java: AttributeTemplateManager.find)
    AttributeTemplate GetAttributeTemplate(int id);

    // Lấy mốc giới hạn sức mạnh theo chỉ mục. (Java: PowerLimitManager.get)
    PowerLimit GetPowerLimit(int index);

    // Tìm danh hiệu theo id. (Java: CaptionManager.find)
    Caption? FindCaption(int id);

    // Lấy danh hiệu theo cấp/vị trí danh sách, không suy diễn index từ cột id 1-based. (Java: CaptionManager.findLevel)
    Caption GetCaptionAtLevel(int index);

    // Lấy danh sách nội tại cho giới tính; giá trị khác 0/1 chọn Saiyan như Java. (Java: INTRINSIC_TD/NM/XD)
    IReadOnlyList<Intrinsic> GetIntrinsics(sbyte gender);

    // Lấy thành tựu theo chỉ mục. (Java: ACHIEVEMENT_TEMPLATE.get)
    AchievementTemplate GetAchievement(int index);
}
