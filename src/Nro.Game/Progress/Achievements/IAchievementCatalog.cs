using Nro.Core.Templates;
using Nro.Data.Db;

namespace Nro.Game.Progress.Achievements;

// Hợp đồng catalog template thành tựu được nạp theo id; vị trí là chỉ mục lưu của người chơi nên thứ tự là dữ liệu.
// Nguồn Java: nro/achievement/Achievement.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).


public interface IAchievementCatalog
{

    IReadOnlyList<AchievementTemplate> Templates { get; }



    AchievementTemplate GetTemplate(int index);





    Task LoadAsync(INroSqlExecutor database, CancellationToken cancellationToken = default);
}
