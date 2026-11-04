
namespace Nro.Game.Progress.Tasks;

// Template KOL bất biến, phần thưởng được tạo khi nạp để các người chơi chỉ đọc chung; giữ nguyên thứ tự và cấu hình phần thưởng Java.
// Nguồn Java: nro/task/KolTaskTemplate.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).












public sealed class KolTaskTemplate
{
    // Mã định danh.
    public int Id { get; }

    // Nội dung nhiệm vụ.
    public string Info { get; }


    // Số lượng cần đạt.
    public int MaxCount { get; }


    // Danh sách phần thưởng template chỉ đọc.
    public IReadOnlyList<Item> Rewards { get; }

    // Tạo template KOL và danh sách phần thưởng từ factory vật phẩm.
    public KolTaskTemplate(int id, string info, int maxCount, IKolTaskItemFactory items)
    {
        ArgumentNullException.ThrowIfNull(items);
        Id = id;
        Info = info;
        MaxCount = maxCount;
        Rewards = BuildRewards(id, items);
    }

    private static List<Item> BuildRewards(int id, IKolTaskItemFactory items)
    {
        var rewards = new List<Item>();
        switch (id)
        {
            case 0:
                {
                    rewards.Add(items.CreateNewItemLock(1820, 3));
                    break;
                }

            case 1:
                {
                    rewards.Add(items.CreateNewItemLock(1592, 5));
                    rewards.Add(items.CreateNewItemLock(1757, 5));
                    break;
                }

            case 2:
                {
                    rewards.Add(items.CreateNewItemLock(1360, 1));
                    break;
                }

            case 3:
                {
                    Item cerBerus = items.CreateNewItemLock(1654, 1);
                    cerBerus.ItemOptions.Add(items.CreateOption(93, 120));
                    rewards.Add(cerBerus);
                    break;
                }

            case 4:
                {
                    rewards.Add(items.CreateNewItemLock(1808, 1));
                    break;
                }

            case 5:
                {
                    Item caiTrang = items.CreateNewItemLock(1829, 1);
                    caiTrang.ItemOptions.Add(items.CreateOption(50, 25));
                    caiTrang.ItemOptions.Add(items.CreateOption(103, 30));
                    caiTrang.ItemOptions.Add(items.CreateOption(93, 180));
                    rewards.Add(caiTrang);
                    rewards.Add(items.CreateNewItemLock(1592, 5));
                    rewards.Add(items.CreateNewItemLock(1757, 5));
                    break;
                }

            case 6:
                {
                    rewards.Add(items.CreateNewItemLock(1592, 10));
                    rewards.Add(items.CreateNewItemLock(1757, 10));
                    break;
                }
        }

        return rewards;
    }
}
