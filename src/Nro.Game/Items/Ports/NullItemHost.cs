using Nro.Core.IO;

namespace Nro.Game.Items.Ports;

// Cài đặt IItemHost rỗng cho các lớp vật phẩm khi chưa có dịch vụ trò chơi thật hoặc khi kiểm thử trạng thái.
// Nguồn Java: Service, InventoryService, EffectSkillService (lớp phụ trợ C#; không gửi gói, túi luôn đầy, không tìm thấy vật phẩm và không giữ trạng thái).
public sealed class NullItemHost : IItemHost
{
    private NullItemHost()
    {
    }

    // Thực hiện instance; giữ nguyên điều kiện, thứ tự và số học của Java.
    public static NullItemHost Instance { get; } = new();

    // Cung cấp dữ liệu hoặc thao tác của lớp.
    public void SendMessage(Player player, Message message)
    {
    }

    // Cung cấp dữ liệu hoặc thao tác của lớp.
    public void SendPoint(Player player)
    {
    }

    // Cung cấp dữ liệu hoặc thao tác của lớp.
    public void SendNotification(Player player, string text)
    {
    }

    // Cung cấp dữ liệu hoặc thao tác của lớp.
    public void SendNotificationFromAdmin(Player player, string text)
    {
    }

    // Cung cấp dữ liệu hoặc thao tác của lớp.
    public void SendItemBag(Player player)
    {
    }

    // Cung cấp dữ liệu hoặc thao tác của lớp.
    public int GetCountEmptyBag(Player player) => 0;

    // Cung cấp dữ liệu hoặc thao tác của lớp.
    public void AddItemBag(Player player, Item item)
    {
    }

    // Cung cấp dữ liệu hoặc thao tác của lớp.
    public void SubQuantityItemsBag(Player player, Item? item, int quantity)
    {
    }

    // Cung cấp dữ liệu hoặc thao tác của lớp.
    public Item? FindBagItem(Player player, int templateId) => null;

    // Cung cấp dữ liệu hoặc thao tác của lớp.
    public IReadOnlyList<Item> GetCombineItems(Player player) => [];

    // Cung cấp dữ liệu hoặc thao tác của lớp.
    public void RemoveMonkeyTail(Player player)
    {
    }

    // Cung cấp dữ liệu hoặc thao tác của lớp.
    public void IncreasePowerLimit(Player player)
    {
    }

    // Cung cấp dữ liệu hoặc thao tác của lớp.
    public ExternalItemTimes GetExternalItemTimes(Player player) => ExternalItemTimes.None;
}
