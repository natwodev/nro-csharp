
namespace Nro.Game.Progress.Tasks;

// Cầu nối ItemService cho việc tạo, sao chép vật phẩm KOL; dùng khi nạp catalog.
// Nguồn Java: nro/task/KolTask.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).





public sealed class KolTaskItemFactory : IKolTaskItemFactory
{
    private readonly ItemService items;

    // Nhận ItemService thật để chuyển tiếp ba thao tác vật phẩm KOL.
    public KolTaskItemFactory(ItemService items)
    {
        ArgumentNullException.ThrowIfNull(items);
        this.items = items;
    }


    // Tạo vật phẩm khóa giao dịch cho phần thưởng KOL. (Java: createNewItemLock)
    public Item CreateNewItemLock(int itemTemplateId, int quantity) => items.CreateNewItemLock(itemTemplateId, quantity);


    // Tạo tùy chọn vật phẩm từ mã template và tham số. (Java: new ItemOption)
    public ItemOption CreateOption(int optionTemplateId, int param) => items.CreateOption(optionTemplateId, param);


    // Sao chép độc lập vật phẩm để trao cho người chơi. (Java: copyItem)
    public Item CopyItem(Item item) => items.CopyItem(item);
}
