using Nro.Data.Db;

namespace Nro.Game.Progress.Tasks;

// Hợp đồng truy cập các bảng nhiệm vụ Java Manager sau khi nạp; chỉ TaskMain cần sao sâu cho mỗi người chơi, các template còn lại bất biến và dùng chung.
// Nguồn Java: nro/task/Manager.java và nro/task/TaskService.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).









public interface ITaskCatalog
{

    IReadOnlyList<TaskMain> TaskMains { get; }


    IReadOnlyList<SideTaskTemplate> SideTaskTemplates { get; }


    IReadOnlyList<KolTaskTemplate> KolTaskTemplates { get; }


    IReadOnlyList<EventTaskTemplate> EventTaskTemplates { get; }





    Task LoadAsync(INroSqlExecutor database, IKolTaskItemFactory kolItems, CancellationToken cancellationToken = default);





    TaskMain? CreateTaskMainCopy(int id);



    SideTaskTemplate? GetSideTaskTemplateById(int id);



    EventTaskTemplate? GetEventTaskTemplateById(int id);





    KolTaskTemplate? GetKolTaskTemplateById(int id);
}
