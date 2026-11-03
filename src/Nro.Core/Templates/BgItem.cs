namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/BgItem.java</c> (a map background decoration). Java <c>byte layer</c> is signed, so it is
/// <see cref="sbyte"/>; <c>idImage</c>, <c>dx</c>, <c>dy</c> are <c>short</c>.
/// </summary>
public class BgItem
{
    public int Id { get; set; }

    public short IdImage { get; set; }

    public short Dx { get; set; }

    public short Dy { get; set; }

    public sbyte Layer { get; set; }
}
