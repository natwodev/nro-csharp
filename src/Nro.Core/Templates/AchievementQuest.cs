namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/AchievementQuest.java</c>: a player's progress on one achievement.
/// Java has only one two-parameter constructor (no default constructor), so the same applies here.
/// </summary>
public class AchievementQuest
{
    public long Completed { get; set; }

    /// <summary>The Java name is <c>isRecieve</c> (misspelled); kept as is so it can be found with grep.</summary>
    public bool IsRecieve { get; set; }

    public AchievementQuest(long completed, bool isRecieve)
    {
        Completed = completed;
        IsRecieve = isRecieve;
    }
}
