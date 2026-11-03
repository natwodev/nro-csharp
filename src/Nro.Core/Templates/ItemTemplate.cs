namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/ItemTemplate.java</c> (an item template, loaded from the item_template table).
/// <para>
/// Java <c>byte</c> is signed, so every byte field is <see cref="sbyte"/>. Property names only capitalize the first letter
/// (<c>ruby_sell</c> becomes <c>Ruby_sell</c>, <c>TypeEvent</c> is kept as is).
/// </para>
/// <para>
/// The eleven event flags (<c>LUNNAR_NEW_YEAR</c> through <c>DAY_20_10</c>) are <c>protected</c> instance fields in Java,
/// NOT <c>static</c> and NOT <c>final</c>, with initial values 1 to 11, and they are not used anywhere.
/// Since they are not constants they do not become <c>const</c>; they are kept as <c>protected</c> properties with the exact same initial values.
/// </para>
/// <para>
/// <c>name</c> and <c>description</c> are null in Java's default constructor; C# uses an empty string
/// (see the null decision in <see cref="FlagBag"/>).
/// </para>
/// </summary>
public class ItemTemplate
{
    public short Id { get; set; }

    public sbyte Type { get; set; }

    public sbyte Gender { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public sbyte Level { get; set; } = 0;

    public short IconID { get; set; }

    public short Part { get; set; }

    public bool IsUpToUp { get; set; }

    public int StrRequire { get; set; }

    public int Gold { get; set; }

    public int GoldSell { get; set; }

    public int Gem { get; set; }

    public int GemSell { get; set; }

    public int Ruby { get; set; }

    public int Ruby_sell { get; set; }

    public int Head { get; set; }

    public int Body { get; set; }

    public int Leg { get; set; }

    public int TypeEvent { get; set; }

    protected sbyte LUNNAR_NEW_YEAR { get; set; } = 1;

    protected sbyte CHRISTMAS { get; set; } = 2;

    protected sbyte VU_LAN_FESTIVAL { get; set; } = 3;

    protected sbyte HALLOWEEN { get; set; } = 4;

    protected sbyte INTERNATIONAL_WOMANS_DAY { get; set; } = 5;

    protected sbyte TRUNG_THU { get; set; } = 6;

    protected sbyte HUNG_VUONG { get; set; } = 7;

    protected sbyte BLACK_FRIDAY { get; set; } = 8;

    protected sbyte WORLD_CUP { get; set; } = 9;

    protected sbyte VALENTINE { get; set; } = 10;

    protected sbyte DAY_20_10 { get; set; } = 11;

    public sbyte IsGender { get; set; }

    public ItemTemplate()
    {
    }

    public ItemTemplate(short id, sbyte type, sbyte gender, string name, string description, short iconID, short part, bool isUpToUp, int strRequire, int TypeEvent, sbyte isGender)
    {
        Id = id;
        Type = type;
        Gender = gender;
        Name = name;
        Description = description;
        IconID = iconID;
        Part = part;
        IsUpToUp = isUpToUp;
        StrRequire = strRequire;
        this.TypeEvent = TypeEvent;
        IsGender = isGender;
    }
}
