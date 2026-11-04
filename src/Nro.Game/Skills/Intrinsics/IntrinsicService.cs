using System.Globalization;
using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Consts;
using Nro.Core.IO;

namespace Nro.Game.Skills.Intrinsics;

// Điều phối xem, mở và quay nội tại của người chơi.
// Nguồn Java: nro/intrinsic/IntrinsicService.java (catalog/random/host được tiêm thay singleton; Message luôn Cleanup trong finally thay nuốt lỗi; DoiNoiTai gọi chung ChangeIntrinsic; giữ chỉ mục giá không kiểm biên, số Việt, tính int, ngưỡng 10 tỷ và quy tắc đếm thường/VIP).
public sealed class IntrinsicService : IIntrinsicService
{
    private static readonly int[] CostOpen = [10, 20, 40, 80, 160, 320, 640, 1280];

    private const long PowerRequired = 10000000000L;
    private const int VipCost = 200_000_000;
    private const sbyte InfoCommand = 112;

    private readonly IIntrinsicCatalog catalog;
    private readonly IIntrinsicHost host;
    private readonly RandomUtil random;

    public IntrinsicService(IIntrinsicCatalog catalog, IIntrinsicHost host, RandomUtil random)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(random);
        this.catalog = catalog;
        this.host = host;
        this.random = random;
    }

    // Lấy danh sách nội tại theo hệ (Java: getIntrinsics).
    public IReadOnlyList<Intrinsic> GetIntrinsics(sbyte playerGender) => catalog.GetIntrinsics(playerGender);

    // Lấy bản sao nội tại theo id (Java: getIntrinsicById).
    public Intrinsic? GetIntrinsicById(int id) => catalog.GetIntrinsicById(id);

    // Gửi gói 112/0 có icon và tên nội tại, luôn Cleanup Message (Java: sendInfoIntrinsic).
    public void SendInfoIntrinsic(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        var msg = new Message(InfoCommand);
        try
        {
            msg.Writer().WriteByte(0);
            msg.Writer().WriteShort(player.PlayerIntrinsic.Intrinsic.Icon);
            msg.Writer().WriteUTF(player.PlayerIntrinsic.Intrinsic.GetName());
            host.SendMessage(player, msg);
        }
        finally
        {
            msg.Cleanup();
        }
    }

    // Gửi gói 112/1 liệt kê nội tại hệ người chơi, bỏ entry 0 không có nội tại (Java: showAllIntrinsic).
    public void ShowAllIntrinsic(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        IReadOnlyList<Intrinsic> listIntrinsic = GetIntrinsics(player.Gender);
        var msg = new Message(InfoCommand);
        try
        {
            msg.Writer().WriteByte(1);
            msg.Writer().WriteByte(1); // Số tab.
            msg.Writer().WriteUTF("Nội tại");
            msg.Writer().WriteByte(listIntrinsic.Count - 1);
            for (int i = 1; i < listIntrinsic.Count; i++)
            {
                msg.Writer().WriteShort(listIntrinsic[i].Icon);
                msg.Writer().WriteUTF(listIntrinsic[i].GetDescription());
            }

            host.SendMessage(player, msg);
        }
        finally
        {
            msg.Cleanup();
        }
    }

    // Mở menu NPC để xem hoặc mở nội tại (Java: showMenu).
    public void ShowMenu(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        host.CreateMenuConMeo(
            player,
            ConstNpc.INTRINSIC,
            -1,
            "Nội tại là một kỹ năng bị động hỗ trợ đặc biệt\nBạn có muốn mở hoặc thay đổi nội tại không?",
            "Xem\ntất cả\nNội Tại",
            "Mở\nNội Tại",
            "Mở VIP",
            "Từ chối");
    }

    // Hiển thị xác nhận giá quay thường theo CountOpen (Java: showConfirmOpen).
    public void ShowConfirmOpen(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        host.CreateMenuConMeo(
            player,
            ConstNpc.CONFIRM_OPEN_INTRINSIC,
            -1,
            "Bạn muốn đổi Nội Tại khác\nvới giá là " + CostOpen[player.PlayerIntrinsic.CountOpen].ToString(CultureInfo.InvariantCulture) + " Tr vàng ?",
            "Mở\nNội Tại",
            "Từ chối");
    }

    // Hiển thị xác nhận giá mở VIP cố định (Java: showConfirmOpenVip).
    public void ShowConfirmOpenVip(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        host.CreateMenuConMeo(
            player,
            ConstNpc.CONFIRM_OPEN_INTRINSIC_VIP,
            -1,
            "Bạn có muốn mở Nội Tại VIP\nvới giá là 200 triệu vàng\nvà tái lập giá vàng quay lại ban đầu không?",
            "Mở\nNội VIP",
            "Từ chối");
    }

    // Quay nội tại miễn phí qua luồng dùng chung (Java: DoiNoiTai).
    public void DoiNoiTai(Player player) => ChangeIntrinsic(player);

    // Mở thường: cần 10 tỷ sức mạnh, trừ vàng int và tăng CountOpen đến giá cuối (Java: open).
    public void Open(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (host.GetPower(player) >= PowerRequired)
        {
            int goldRequire = unchecked(CostOpen[player.PlayerIntrinsic.CountOpen] * 1000000);
            long gold = host.GetGold(player);
            if (gold >= goldRequire)
            {
                host.SetGold(player, gold - goldRequire);
                host.SendInfoHpMpMoney(player);
                ChangeIntrinsic(player);
                if (player.PlayerIntrinsic.CountOpen < CostOpen.Length - 1)
                {
                    player.PlayerIntrinsic.CountOpen++;
                }
            }
            else
            {
                host.SendNotification(player, "Bạn không đủ vàng, còn thiếu " + Util.FormatNumber(goldRequire - gold, FormatStyle.VIETNAMESE) + " vàng nữa");
            }
        }
        else
        {
            host.SendNotification(player, "Yêu cầu sức mạnh tối thiểu 10 tỷ");
        }
    }

    // Mở VIP: cần 10 tỷ sức mạnh, trừ 200 triệu vàng và đặt CountOpen về 0 (Java: openVip).
    public void OpenVip(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (host.GetPower(player) >= PowerRequired)
        {
            long gold = host.GetGold(player);
            if (gold >= VipCost)
            {
                host.SetGold(player, gold - VipCost);
                host.SendInfoHpMpMoney(player);
                ChangeIntrinsic(player);
                player.PlayerIntrinsic.CountOpen = 0;
            }
            else
            {
                host.SendNotification(player, "Bạn không đủ vàng, còn thiếu " + Util.FormatNumber(VipCost - gold, FormatStyle.VIETNAMESE) + " vàng nữa");
            }
        }
        else
        {
            host.SendNotification(player, "Yêu cầu sức mạnh tối thiểu 10 tỷ");
        }
    }

    // Chọn ngẫu nhiên nội tại từ vị trí 1, quay hai tham số, báo tên trước " [" rồi gửi thông tin mới (Java: changeIntrinsic).
    private void ChangeIntrinsic(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        IReadOnlyList<Intrinsic> listIntrinsic = GetIntrinsics(player.Gender);
        var rolled = new Intrinsic(listIntrinsic[random.NextInt(1, listIntrinsic.Count - 1)]);
        player.PlayerIntrinsic.Intrinsic = rolled;
        rolled.Param1 = unchecked((short)random.NextInt(rolled.ParamFrom1, rolled.ParamTo1));
        rolled.Param2 = unchecked((short)random.NextInt(rolled.ParamFrom2, rolled.ParamTo2));
        string name = rolled.GetName();
        host.SendNotification(player, "Bạn nhận được Nội tại:\n" + name.Substring(0, name.IndexOf(" [", StringComparison.Ordinal)));
        SendInfoIntrinsic(player);
    }
}
    // Tạo service với catalog, cầu nối game và nguồn ngẫu nhiên; thứ tự quay giữ Java: nội tại, tham số một, tham số hai.
