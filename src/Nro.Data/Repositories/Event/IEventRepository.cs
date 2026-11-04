using Nro.Data.Db;
using Nro.Data.Models.Event;
namespace Nro.Data.Repositories.Event;

// Cung cấp hợp đồng lưu trữ sự kiện mùa toàn máy chủ, mỗi sự kiện là một dòng với JSON văn bản.
// Nguồn Java: jbcd/dao/EventDAO.java (trạng thái static thuộc bên gọi; lỗi CSDL được ném lại, lưu bốn dòng trong một giao dịch, tải chọn id nhỏ nhất và JSON lỗi chỉ ghi nhật ký không lộ nội dung).
public interface IEventRepository
{
    // Tải sự kiện Hùng Vương; không có dòng hung_vuong thì trả null (Java: loadHungVuongEvent).
    Task<HungVuongEventData?> LoadHungVuongEventAsync(CancellationToken cancellationToken = default);

    // Tải sự kiện Quốc tế Phụ nữ; không có dòng thì trả null (Java: loadInternationalWomensDayEvent).
    Task<InternationalWomensDayEventData?> LoadInternationalWomensDayEventAsync(CancellationToken cancellationToken = default);

    // Tải điểm và mốc thưởng; thiếu dòng hoặc một trong hai khóa eventPoint, lastExpRewardStage thì trả null (Java: loadEvent).
    Task<EventPointData?> LoadEventAsync(string eventName, CancellationToken cancellationToken = default);

    // Tải sự kiện năm mới âm lịch từ dòng luna_new_year (Java: loadLunaNewYearEvent).
    Task<EventPointData?> LoadLunaNewYearEventAsync(CancellationToken cancellationToken = default);

    // Tải sự kiện Giáng sinh từ dòng christ_mas (Java: loadChristMasEvent).
    Task<EventPointData?> LoadChristMasEventAsync(CancellationToken cancellationToken = default);

    // Lưu bốn dòng sự kiện và cảnh báo từng dòng không khớp (Java: save).
    Task<EventSaveResult> SaveAsync(EventSaveState state, CancellationToken cancellationToken = default);
}
