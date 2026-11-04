namespace Nro.Data.Models.Event;

// Dữ liệu hàng international_womens_day của bảng event gồm phần trăm chỉ số, điểm và mốc thưởng.
// Nguồn Java: jbcd/dao/EventDAO.java (Java: loadInternationalWomensDayEvent; thuộc tính NULL khi khóa thiếu hoặc đọc lỗi tại/trước nó để nơi gọi giữ giá trị hiện tại).
public sealed record InternationalWomensDayEventData(int? DamePercent, int? HpPercent, int? MpPercent, int? EventPoint, int? LastExpRewardStage);
