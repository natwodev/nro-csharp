namespace Nro.Game.Progress.Achievements;

// Thành phần tiến độ được port, giữ nguyên logic và dữ liệu C# hiện có.
// Nguồn Java: nro/achievement/AchievementService.java (interface phụ thay singleton AchievementService.gI(); mã và thứ tự C# được bảo toàn).


public interface IAchievementService
{

    void OpenAchievementUI(Player player);


    void ConfirmAchievement(Player player, sbyte select);


    string NumberToString(long num);


    string Regex(Player player, string text);


    void CheckDoneTask(Player player, int aId);


    void CheckDoneTaskKillMob(Player player, Mob mob);





    void CheckDoneTaskKillMob(Player player, int mobLevel, int mobType, int mobTemplateId);


    void CheckDoneTaskUseSkill(Player player);


    void CheckDoneTaskFly(Player player, int length);
}
