using Nro.Core.Consts;
using Nro.Core.Templates;

namespace Nro.Game.Progress.Achievements;

// Tiến độ thành tựu của một người chơi. Khác Java: catalog được truyền vào, MagicTree lấy từ nguồn tùy chọn (không có thì 0), Dispose chỉ xóa danh sách và giữ owner. GetCompleted cố ý ghi lại sức mạnh/cấp cây cho các chỉ mục 0,1,2,16; thành tựu chuyên cần đổi mili giây sang giờ bằng chia nguyên.
// Nguồn Java: nro/achievement/Achievement.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).











public class Achievement
{
    private const long MillisecondsPerHour = 60 * 60 * 1000;

    // Người chơi sở hữu trạng thái này.
    public Player Player { get; }


    // Nguồn số liệu của các lớp chưa port; null nghĩa là không có.
    public IAchievementProgressSource? ProgressSource { get; set; }


    // Danh sách tiến độ thành tựu theo chỉ mục template.
    public List<AchievementQuest> AchievementList { get; } = [];

    // Gắn trạng thái thành tựu với người chơi sở hữu.
    public Achievement(Player player)
    {
        Player = player;
    }


    // Thêm một bản ghi tiến độ thành tựu.
    public void Add(AchievementQuest achievement)
    {
        AchievementList.Add(achievement);
    }


    // Lấy tiến độ tại chỉ mục, hoặc null khi ngoài danh sách.
    public AchievementQuest? Get(int index) => index >= 0 && index < AchievementList.Count ? AchievementList[index] : null;





    // Trả tiến độ hiển thị và cập nhật các chỉ mục lấy số liệu sống theo quy tắc Java. (Java: getCompleted)
    public long GetCompleted(int index)
    {
        AchievementQuest? aq = Get(index);
        if (aq is not null)
        {
            switch (index)
            {
                case 0:
                case 1:
                case 16:
                    {
                        aq.Completed = Player.NPoint.Power;
                        break;
                    }

                case 2:
                    {
                        aq.Completed = ProgressSource?.GetMagicTreeLevel(Player) ?? 0;
                        break;
                    }

                case ConstAchievement.DEDICATED_PLAYER:
                    {
                        return aq.Completed / MillisecondsPerHour;
                    }
            }

            return aq.Completed;
        }

        return 0;
    }


    // Kiểm tra đã nhận thưởng hoặc đã đủ tiến độ. (Java: isFinish)
    public bool IsFinish(int index, long maxCount)
    {
        AchievementQuest? aq = Get(index);
        return aq is not null && (aq.IsRecieve || GetCompleted(index) >= maxCount);
    }


    // Kiểm tra cờ đã nhận thưởng theo chính tả Java.
    public bool IsRecieve(int index)
    {
        AchievementQuest? aq = Get(index);
        return aq is not null && aq.IsRecieve;
    }






    // Chỉ cho nhận khi quest tồn tại, chưa nhận và đạt ngưỡng; tra template trước như Java nên chỉ mục sai vẫn ném lỗi. (Java: canReward)
    public bool CanReward(int index, IAchievementCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        AchievementQuest? aq = Get(index);
        AchievementTemplate at = catalog.GetTemplate(index);
        return aq is not null && !aq.IsRecieve && GetCompleted(index) >= at.MaxCount;
    }





    // Cộng tiến độ bằng arithmetic unchecked như Java; bỏ qua chỉ mục ngoài danh sách. (Java: done)
    public void Done(int index, long completed)
    {
        if (index >= 0 && index < AchievementList.Count)
        {
            AchievementList[index] = new AchievementQuest(unchecked(AchievementList[index].Completed + completed), AchievementList[index].IsRecieve);
        }
    }


    // Gán tiến độ mới, giữ cờ đã nhận và bỏ qua chỉ mục ngoài danh sách. (Java: doneNotAdd)
    public void DoneNotAdd(int index, long completed)
    {
        if (index >= 0 && index < AchievementList.Count)
        {
            AchievementList[index] = new AchievementQuest(completed, AchievementList[index].IsRecieve);
        }
    }


    // Đánh dấu quest đã nhận thưởng nếu chỉ mục hợp lệ. (Java: reward)
    public void Reward(int index)
    {
        if (index >= 0 && index < AchievementList.Count)
        {
            AchievementList[index] = new AchievementQuest(AchievementList[index].Completed, true);
        }
    }


    // Dọn trạng thái theo quy tắc thành phần không bị gán null. (Java: dispose)
    public void Dispose()
    {
        AchievementList.Clear();
    }
}
