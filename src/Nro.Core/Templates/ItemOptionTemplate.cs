namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/ItemOptionTemplate.java</c>. Java has two constructors: a default one (empty; the loader assigns
/// each field) and a three-parameter one. <c>name</c> is null in Java's default constructor; C# uses an empty string
/// (see the null decision in <see cref="FlagBag"/>).
/// </summary>
public class ItemOptionTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int Type { get; set; }

    public ItemOptionTemplate()
    {
    }

    public ItemOptionTemplate(int id, string name, int type)
    {
        Id = id;
        Name = name;
        Type = type;
    }
}
