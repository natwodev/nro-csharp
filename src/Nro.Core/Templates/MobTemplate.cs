namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/MobTemplate.java</c> (a mob template, loaded from the mob_template table). Java's <c>byte</c> fields
/// are signed, so they are <see cref="sbyte"/>. <c>percent_gold</c> becomes <c>Percent_gold</c> (only the first letter is capitalized).
/// <c>name</c> is null in Java until the loader assigns it; C# uses an empty string (see the null decision in <see cref="FlagBag"/>).
/// </summary>
public class MobTemplate
{
    public int Id { get; set; }

    public sbyte Type { get; set; }

    public string Name { get; set; } = "";

    public int Hp { get; set; }

    public sbyte RangeMove { get; set; }

    public sbyte Speed { get; set; }

    public sbyte DartType { get; set; }

    public sbyte PercentDame { get; set; }

    public sbyte PercentTiemNang { get; set; }

    public sbyte Percent_gold { get; set; }
}
