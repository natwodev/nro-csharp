using System.Globalization;
using System.Text;
using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Progress.Tasks;

// Chuỗi nhiệm vụ KOL của riêng người chơi. Khác Java: chuyển nhiệm vụ không tăng Id của template dùng chung; hoàn tất nhiệm vụ cuối trả về không có template thay vì IndexOutOfBoundsException. Phần trăm giữ phép nhân int 32-bit nên có thể tràn như Java.
// Nguồn Java: nro/task/KolTask.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).
















public class KolTask
{

    // Template đang áp dụng; null khi chưa có.
    public KolTaskTemplate? Template { get; set; }

    // Tiến độ hoặc số lượng hiện có.
    public int Count { get; set; }




    // Chuyển sang id KOL kế tiếp mà không sửa template dùng chung và đặt lại tiến độ. (Java: next)
    public void Next(ITaskCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        if (Template is not null)
        {
            Template = catalog.GetKolTaskTemplateById(Template.Id + 1);
        }

        Count = 0;
    }


    // Tăng một bước chỉ khi đang có template KOL. (Java: addCount)
    public void AddCount()
    {
        if (Template is not null)
        {
            Count++;
        }
    }


    // Kiểm tra tiến độ đã đạt ngưỡng của template hiện tại. (Java: isDone)
    public bool IsDone() => Template is not null && Count >= Template.MaxCount;






    // Nộp KOL theo đúng thứ tự Java: kiểm tra hoàn thành/chỗ trống, chép từng thưởng, gửi túi rồi chuyển bước. (Java: receive)
    public void Receive(Player player, IKolTaskHost host, ITaskCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(catalog);
        KolTaskTemplate? template = Template;
        if (template is null)
        {
            return;
        }

        if (!IsDone())
        {
            host.SendNotification(player, "Bạn chưa hoàn thành nhiệm vụ");
            return;
        }

        if (host.GetCountEmptyBag(player) < template.Rewards.Count - 1)
        {
            host.SendNotification(player, "Bạn cần trống " + (template.Rewards.Count - 1).ToString(CultureInfo.InvariantCulture) + " ô hành trang!");
            return;
        }

        foreach (Item item in template.Rewards)
        {
            host.AddItemBag(player, host.CopyItem(item));
            host.SendNotification(player, "Bạn nhận được " + item.Quantity.ToString(CultureInfo.InvariantCulture) + " " + item.Name());
        }

        host.SendItemBag(player);
        Next(catalog);
    }





    // Chỉ tăng cho vật phẩm 1836 ở KOL 0 hoặc 1837 ở KOL 1. (Java: checkDonePickItem)
    public void CheckDonePickItem(int pickedItemTemplateId)
    {
        if (Template is not null)
        {
            switch (Template.Id)
            {
                case 0:
                    {
                        if (pickedItemTemplateId == 1836)
                        {
                            Count++;
                        }

                        break;
                    }

                case 1:
                    {
                        if (pickedItemTemplateId == 1837)
                        {
                            Count++;
                        }

                        break;
                    }
            }
        }
    }


    // Thay %1 bằng số cần làm, hoặc báo đã hết nhiệm vụ. (Java: getTaskInfo)
    public string GetTaskInfo()
    {
        if (Template is not null)
        {
            return Util.Replace(Template.Info, "%1", Template.MaxCount.ToString(CultureInfo.InvariantCulture));
        }

        return "Đã hoàn thành hết nhiệm vụ.";
    }





    // Ghép chuỗi phần thưởng, giữ định dạng số Việt Nam và số ngày hết hạn. (Java: getRewardsInfo)
    public string GetRewardsInfo()
    {
        var text = new StringBuilder("Thưởng ");
        if (Template is null)
        {
            return text.ToString();
        }

        IReadOnlyList<Item> rewards = Template.Rewards;
        for (int i = 0; i < rewards.Count; i++)
        {
            Item item = rewards[i];
            text.Append(Util.Format(item.Quantity)).Append(' ').Append(item.Name());
            ItemOption? io = item.ItemOptions.FirstOrDefault(o => o.HaveExpiryDate());
            if (io is not null)
            {
                text.Append(" (").Append(io.Param.ToString(CultureInfo.InvariantCulture)).Append(" ngày)");
            }

            if (i < rewards.Count - 1)
            {
                text.Append(", ");
            }
        }

        return text.ToString();
    }


    // Tính phần trăm; giữ phép nhân int 32-bit có thể tràn như Java. (Java: getPercentProcess)
    public int GetPercentProcess()
    {
        if (Template is null || Template.MaxCount <= 0)
        {
            return 0;
        }

        if (Count >= Template.MaxCount)
        {
            return 100;
        }

        return unchecked(Count * 100) / Template.MaxCount;
    }


    // Dọn trạng thái theo quy tắc thành phần không bị gán null. (Java: dispose)
    public void Dispose()
    {
        Template = null;
    }
}
