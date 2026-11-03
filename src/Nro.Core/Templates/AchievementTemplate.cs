namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/AchievementTemplate.java</c>: describes one achievement (loaded from the achievement table).
/// Java has only one four-parameter constructor (no default constructor).
/// </summary>
public class AchievementTemplate
{
    public string Info1 { get; set; }

    public string Info2 { get; set; }

    public int Money { get; set; }

    public long MaxCount { get; set; }

    public AchievementTemplate(string info1, string info2, int money, long maxCount)
    {
        Info1 = info1;
        Info2 = info2;
        Money = money;
        MaxCount = maxCount;
    }
}
