namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/PartDetail.java</c>. <c>dx</c> and <c>dy</c> are signed Java <c>byte</c> values (the offset can be negative),
/// so they are <see cref="sbyte"/>. Java has only one three-parameter constructor (no default constructor).
/// </summary>
public class PartDetail
{
    public short IconId { get; set; }

    public sbyte Dx { get; set; }

    public sbyte Dy { get; set; }

    public PartDetail(short iconId, sbyte dx, sbyte dy)
    {
        IconId = iconId;
        Dx = dx;
        Dy = dy;
    }
}
