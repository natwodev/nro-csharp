using Nro.Data.Db;

namespace Nro.Game.Skills.Intrinsics;

// Hợp đồng catalog các bảng nội tại của Manager Java.
// Nguồn Java: Manager và IntrinsicService (mẫu nạp một lần chỉ đọc; bên gọi phải tạo bản sao trước khi thay đổi).
public interface IIntrinsicCatalog
{
    // Mọi dòng theo id (Java: Manager.INTRINSICS).
    IReadOnlyList<Intrinsic> Intrinsics { get; }

    // Danh sách Trái Đất, Namek hoặc Saiyan; vị trí 0 là entry không có nên chỉ quay từ 1 (Java: getIntrinsics).
    IReadOnlyList<Intrinsic> GetIntrinsics(sbyte playerGender);

    // Bản sao nội tại đầu tiên theo id với tham số quay bằng 0, không có trả null (Java: getIntrinsicById).
    Intrinsic? GetIntrinsicById(int id);

    // Nạp intrinsic theo id, phân 0/1/2 vào một hệ, giá trị khác vào cả ba; thay snapshot toàn bộ hoặc không thay gì (Java: Manager).
    Task LoadAsync(INroSqlExecutor database, CancellationToken cancellationToken = default);
}
