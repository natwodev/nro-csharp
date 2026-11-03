namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/FlagBag.java</c> (a back-worn flag). Java leaves <c>name</c> and <c>iconEffect</c> null until the
/// loader (<c>Manager</c>) assigns them from the database; nowhere in the Java code are these two fields null-checked
/// (<c>FlagBagService</c> reads <c>iconEffect.length</c> directly). Therefore C# uses neutral values: an empty string and an empty array.
/// </summary>
public class FlagBag
{
    public int Id { get; set; }

    public short IconId { get; set; }

    public short[] IconEffect { get; set; } = [];

    public string Name { get; set; } = "";

    public int Gold { get; set; }

    public int Gem { get; set; }
}
