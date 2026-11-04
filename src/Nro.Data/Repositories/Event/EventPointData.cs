using Nro.Data.Db;
namespace Nro.Data.Repositories.Event;

// Dữ liệu điểm sự kiện và mốc thưởng kinh nghiệm cuối của Tết âm lịch hoặc Giáng sinh.
// Nguồn Java: jbcd/dao/EventDAO.java (giữ hai giá trị EventDAO.loadEvent chuyển cho nơi gọi).
public sealed record EventPointData(int EventPoint, int LastExpRewardStage);
