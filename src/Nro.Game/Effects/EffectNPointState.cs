namespace Nro.Game.Effects;

// Chứa bản sao chỉ đọc các trường NPoint mà hiệu ứng cần; adapter của IEffectPlayerAccess.GetNPoint điền từ NPoint thật vì lớp chính còn là placeholder của track khác.
// Nguồn Java: nro/player/NPoint.java (DTO phụ trợ; Hp và HpMax giữ long vì công thức phụ thuộc độ rộng số học Java).
public sealed record EffectNPointState
{
    // HP hiện tại (Java: hp).
    public long Hp { get; init; }

    // HP tối đa (Java: hpMax).
    public long HpMax { get; init; }

    // KI hiện tại (Java: mp).
    public long Mp { get; init; }

    // KI tối đa (Java: mpMax).
    public long MpMax { get; init; }

    // Tỷ lệ hồi HP cho bản thân và đồng minh (Java: tlHpHoiBanthan_DongMinh).
    public int SelfAndAllyHpRegenPercent { get; init; }

    // Cho biết đang có hiệu ứng Hóa Bí Ngô xung quanh (Java: isHoaBiNgoXungQuanh).
    public bool IsPumpkinAura { get; init; }

    // Cho biết đang có hiệu ứng Founder (Java: isFounder).
    public bool IsFounder { get; init; }

    // Cho biết đang có hiệu ứng Xinbato (Java: isXinbato).
    public bool IsXinbato { get; init; }

    // Tỷ lệ sát thương Sexy (Java: tlSexyDame).
    public int SexyDamagePercent { get; init; }

    // Cho biết có trạng thái Thỏ Bulma (Java: isThoBulma).
    public bool IsRabbitBulma { get; init; }

    // Tỷ lệ sát thương Cool (Java: tlCoolDame).
    public int CoolDamagePercent { get; init; }

    // Tỷ lệ sát thương Cute (Java: tlCuteAddame).
    public int CuteDamagePercent { get; init; }

    // Cho biết đang tàng hình (Java: isTanHinh).
    public bool IsInvisible { get; init; }

    // Cho biết đang hóa đá (Java: isHoaDa).
    public bool IsPetrified { get; init; }

    // Cho biết đang mặc trang phục BuiBui (Java: wearingBuiBui).
    public bool WearingBuiBui { get; init; }

    // Cho biết đang mặc trang phục Mabu (Java: wearingMabu).
    public bool WearingMabu { get; init; }

    // Giá trị X Chưởng (Java: xChuong).
    public int BlastDamageMultiplier { get; init; }

    // Cho biết đang mặc giáp vô hình (Java: wearingVoHinh).
    public bool WearingInvisibilityArmor { get; init; }

    // Cho biết đang mặc giáp luyện tập (Java: wearingTrainArmor).
    public bool WearingTrainArmor { get; init; }

    // Cho biết có kháng hóa xương (Java: khangHoaXuong).
    public bool IsBoneTransformationImmune { get; init; }

    // Tỷ lệ hút HP/KI xung quanh (Java: tlHutHpMpXQ).
    public int AroundHpMpAbsorptionPercent { get; init; }

    // Tỷ lệ HP giảm ở độc (Java: tlHpGiamODo).
    public int PercentDirtyHpLoss { get; init; }

    // Cho biết có trạng thái Thỏ Đại Ca (Java: isThoDaiCa).
    public bool IsRabbitLeader { get; init; }
}
