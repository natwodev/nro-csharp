namespace Nro.Game.Progress.Tasks;

// Hợp đồng ba thao tác ItemService mà KOL cần, để catalog và kiểm thử không phụ thuộc singleton Java.
// Nguồn Java: nro/task/KolTask.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).




public interface IKolTaskItemFactory
{

    Item CreateNewItemLock(int itemTemplateId, int quantity);


    ItemOption CreateOption(int optionTemplateId, int param);


    Item CopyItem(Item item);
}
