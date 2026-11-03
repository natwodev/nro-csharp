namespace Nro.Core.Consts;

/// <summary>
/// Port of <c>Utils/FormatStyle.java</c>: the suffix style of <see cref="Util.FormatNumber"/>. The member order is the Java order (the ordinals are 0 and 1).
/// </summary>
public enum FormatStyle
{
    /// <summary>Vietnamese words: <c>Nghìn</c>, <c>Triệu</c>, <c>Tỷ</c>.</summary>
    VIETNAMESE,

    /// <summary>Letters: <c>K</c>, <c>M</c>, <c>B</c>.</summary>
    KMB,
}
