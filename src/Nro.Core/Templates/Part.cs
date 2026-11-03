namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/Part.java</c> (a character image part). Java's default constructor initializes
/// <c>partDetails = new ArrayList()</c>, so the list is never null after the object is created.
/// </summary>
public class Part
{
    public int Id { get; set; }

    public int Type { get; set; }

    public List<PartDetail> PartDetails { get; set; }

    public Part()
    {
        PartDetails = new List<PartDetail>();
    }
}
