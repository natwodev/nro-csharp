namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/ArrHead2Frames.java</c>. Java initializes <c>frames = new ArrayList()</c> right at the field declaration,
/// so the list is never null after the object is created.
/// </summary>
public class ArrHead2Frames
{
    public List<int> Frames { get; set; } = new();
}
