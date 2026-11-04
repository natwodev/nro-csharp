using Nro.Data.Db;

namespace Nro.Game.Progress.Badges;

// Hợp đồng đọc catalog huy hiệu đã nạp, tách dịch vụ khỏi dữ liệu tĩnh Java.
// Nguồn Java: nro/badges/BadgesService.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).





public interface IBadgesCatalog
{

    IReadOnlyList<BagesTemplate> BagesTemplates { get; }


    IReadOnlyList<BadgesTaskTemplate> TaskTemplates { get; }






    Task LoadAsync(INroSqlExecutor database, ItemCatalog itemCatalog, CancellationToken cancellationToken = default);


    int FindIdItemByIdIdEffect(int idEffect);


    int FineIdEffectbyIdItem(int idItem);





    IReadOnlyList<int> ListEffect(Player player);





    IReadOnlyList<ItemOption> SendListItemOption(Player player);
}
