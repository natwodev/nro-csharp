namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/NpcTemplate.java</c> (an NPC template, loaded from the npc_template table).
/// <c>name</c> is null in Java until the loader assigns it; C# uses an empty string (see the null decision in <see cref="FlagBag"/>).
/// </summary>
public class NpcTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int Head { get; set; }

    public int Body { get; set; }

    public int Leg { get; set; }

    public int Avatar { get; set; }
}
