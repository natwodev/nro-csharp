using System.Globalization;
using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Consts;
using Nro.Core.IO;
using Nro.Core.Templates;

namespace Nro.Game.Progress.Achievements;

// Dịch vụ UI, trao thưởng và đếm thành tựu. Khác Java: không singleton/static Manager, phụ thuộc được truyền vào; Message luôn Cleanup trong finally thay vì nuốt IOException; overload hạ quái nhận ba số để kiểm thử; Achievement của Player không null.
// Nguồn Java: nro/achievement/AchievementService.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).











public sealed class AchievementService : IAchievementService
{
    private const sbyte AchievementCommand = -76;
    private const short GoldBarItemTemplateId = 457;

    private readonly IAchievementCatalog catalog;
    private readonly IAchievementHost host;

    // Nhận catalog và dịch vụ game thay cho singleton Java.
    public AchievementService(IAchievementCatalog catalog, IAchievementHost host)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(host);
        this.catalog = catalog;
        this.host = host;
    }


    // Gửi danh sách thành tựu và luôn dọn Message trong finally. (Java: openAchievementUI)
    public void OpenAchievementUI(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        var msg = new Message(AchievementCommand);
        try
        {
            IReadOnlyList<AchievementTemplate> templates = catalog.Templates;
            msg.Writer().WriteByte(0);
            msg.Writer().WriteByte(templates.Count);
            for (int i = 0; i < templates.Count; i++)
            {
                AchievementTemplate at = templates[i];
                msg.Writer().WriteUTF(at.Info1);
                msg.Writer().WriteUTF(Regex(player, at.Info2) + " (" + NumberToString(player.Achievement.GetCompleted(i)) + "/" + NumberToString(at.MaxCount) + ")");
                msg.Writer().WriteShort(at.Money);
                msg.Writer().WriteBoolean(player.Achievement.IsFinish(i, at.MaxCount));
                msg.Writer().WriteBoolean(player.Achievement.IsRecieve(i));
            }

            host.SendMessage(player, msg);
        }
        finally
        {
            msg.Cleanup();
        }
    }


    // Kiểm tra bảo vệ, điều kiện, chỗ túi rồi trao hồng ngọc và thỏi vàng. (Java: confirmAchievement)
    public void ConfirmAchievement(Player player, sbyte select)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (player.IsAccountProtected)
        {
            host.SendNotification(player, "Chức năng bảo vệ đã được bật. Bạn vui lòng kiểm tra lại");
            return;
        }

        if (!player.Achievement.CanReward(select, catalog))
        {
            host.SendNotification(player, "Không thể thực hiện");
            return;
        }

        if (host.GetCountEmptyBag(player) > 0)
        {
            int money = catalog.GetTemplate(select).Money;
            player.Achievement.Reward(select);
            host.AddRuby(player, money);
            Item item = host.CreateNewItem(GoldBarItemTemplateId, money);
            host.AddItemBag(player, item);
            host.SendItemBag(player);
            host.SendMoney(player);
            host.SendNotification(player, "Bạn vừa nhận được " + money.ToString(CultureInfo.InvariantCulture) + " hồng ngọc");
            host.SendNotification(player, "Bạn vừa nhận được " + money.ToString(CultureInfo.InvariantCulture) + " thỏi vàng");
        }
        else
        {
            host.SendNotification(player, "Cần tối thiểu 1 ô trống hành trang để nhận thưởng");
            return;
        }

        var msg = new Message(AchievementCommand);
        try
        {
            msg.Writer().WriteByte(1);
            msg.Writer().WriteByte(select);
            host.SendMessage(player, msg);
        }
        finally
        {
            msg.Cleanup();
        }
    }


    // Định dạng nhỏ bằng invariant, lớn hơn 10000 theo kiểu Việt Nam. (Java: numberToString)
    public string NumberToString(long num) => num <= 10000 ? num.ToString(CultureInfo.InvariantCulture) : Util.FormatNumber(num, FormatStyle.VIETNAMESE);


    // Thay biến chủng tộc và NPC theo giới tính người chơi. (Java: regex)
    public string Regex(Player player, string text)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(text);
        int gen = player.Gender;
        string withRace = Util.Replace(text, "%1", gen == 0 ? "Siêu nhân" : gen == 1 ? "Siêu Namếc" : "Siêu Xayda");
        return Util.Replace(withRace, "%2", gen == 0 ? "Bunma" : gen == 1 ? "Dende" : "Appule");
    }


    // Cập nhật thành tựu; nạp lần đầu gán trực tiếp tổng đã nạp, trường hợp khác cộng một. (Java: checkDoneTask)
    public void CheckDoneTask(Player player, int aId)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (host.IsPl(player))
        {
            switch (aId)
            {
                case ConstAchievement.FIRST_GEM_TOP_UP:
                    player.Achievement.DoneNotAdd(aId, host.GetSessionPaidAmount(player));
                    break;
                default:
                    player.Achievement.Done(aId, 1);
                    break;
            }
        }
    }


    // Đếm thành tựu diệt siêu quái, thiện xạ và luyện tập theo thuộc tính quái. (Java: checkDoneTaskKillMob)
    public void CheckDoneTaskKillMob(Player player, Mob mob)
    {
        ArgumentNullException.ThrowIfNull(mob);
        CheckDoneTaskKillMob(player, mob.LvMob, mob.Type, mob.TempId);
    }


    // Đếm thành tựu diệt siêu quái, thiện xạ và luyện tập theo thuộc tính quái. (Java: checkDoneTaskKillMob)
    public void CheckDoneTaskKillMob(Player player, int mobLevel, int mobType, int mobTemplateId)
    {
        if (mobLevel > 0)
        {
            CheckDoneTask(player, ConstAchievement.SUPER_MONSTER_SLAYER);
        }

        if (mobType == 4)
        {
            CheckDoneTask(player, ConstAchievement.SHARPSHOOTER);
        }

        if (mobTemplateId == 0)
        {
            CheckDoneTask(player, ConstAchievement.METHODICAL_TRAINING);
        }
    }


    // Đếm thành tựu dùng kỹ năng theo nhóm id kỹ năng Java. (Java: checkDoneTaskUseSkill)
    public void CheckDoneTaskUseSkill(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (host.IsPl(player))
        {
            SkillTemplate template = player.PlayerSkill.SkillSelect?.Template ?? throw new InvalidOperationException("The player has no selected skill");
            switch (template.Id)
            {
                case Skill.KAMEJOKO:
                case Skill.MASENKO:
                case Skill.ANTOMIC:
                    {
                        CheckDoneTask(player, ConstAchievement.KI_BLAST_MASTER);
                        break;
                    }

                case Skill.DRAGON:
                case Skill.DEMON:
                case Skill.GALICK:
                case Skill.LIEN_HOAN:
                case Skill.KAIOKEN:
                case Skill.DICH_CHUYEN_TUC_THOI:
                    {
                        break;
                    }

                default:
                    CheckDoneTask(player, ConstAchievement.SKILL_MASTER);
                    break;
            }
        }
    }


    // Đếm quãng bay sau chia nguyên cho 10 và chỉ khi nhỏ hơn 10. (Java: checkDoneTaskFly)
    public void CheckDoneTaskFly(Player player, int length)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (host.IsPl(player))
        {
            length = Math.Abs(length / 10);
            if (length < 10)
            {
                player.Achievement.Done(ConstAchievement.SKILLED_FLYER, length);
            }
        }
    }
}
