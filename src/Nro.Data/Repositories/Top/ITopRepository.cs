using Nro.Data.Db;
using Nro.Data.Models.Top;
using Nro.Core.Enums;
namespace Nro.Data.Repositories.Top;

// Cung cấp hợp đồng tải bảng xếp hạng TopDAO trên PostgreSQL.
// Nguồn Java: jbcd/dao/TopDAO.java (mỗi lượt đọc là snapshot REPEATABLE READ; JSON văn bản được đọc bằng C# để dữ liệu lỗi không làm hỏng truy vấn; giá trị thiếu đứng cuối, số âm tràn unsigned đứng đầu, hoà luôn theo player.id tăng dần để ổn định khác Java; bốn TOP_SK, TOP_NHS, TOP_DC, TOP_3_WHIS không port vì MariaDB gốc cũng lỗi và không được gọi).
public interface ITopRepository
{
    // Lấy bảng xếp hạng một giá trị theo hạng đã chọn.
    Task<IReadOnlyList<TopEntry>> GetTopAsync(TopRanking ranking, CancellationToken cancellationToken = default);

    // Lấy TOP_NV theo nhiệm vụ, nhiệm vụ phụ, tiến độ giảm dần rồi thời điểm đăng nhập tăng dần.
    Task<IReadOnlyList<TopTaskEntry>> GetTopNvAsync(CancellationToken cancellationToken = default);

    // Lấy TOP_WHIS với training_data[5] lớn hơn 0, giảm dần rồi training_data[6] tăng dần, tối đa 10 dòng (Java: data_luyentap).
    Task<IReadOnlyList<TopWhisEntry>> GetTopWhisAsync(CancellationToken cancellationToken = default);

    // Lấy TOP_VDST với deathmatch_arena_data[3] lớn hơn 0, giảm dần, tối đa 20 dòng (Java: vodaisinhtu).
    Task<IReadOnlyList<TopVdstEntry>> GetTopVdstAsync(CancellationToken cancellationToken = default);

    // Lấy TOP_THOI_VANG theo thỏi vàng vật phẩm 457 trong túi và rương, không lọc ban như Java, tối đa 20 dòng.
    Task<IReadOnlyList<TopGoldBarEntry>> GetTopThoiVangAsync(CancellationToken cancellationToken = default);
}
