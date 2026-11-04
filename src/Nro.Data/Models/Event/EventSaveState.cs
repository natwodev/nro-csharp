namespace Nro.Data.Models.Event;

// Trạng thái sự kiện cần lưu gồm phần trăm chỉ số, điểm và mốc thưởng cùng thời điểm nhận hạt dưa.
// Nguồn Java: jbcd/dao/EventDAO.java (Java giữ các trường static trong EventDAO; C# truyền trạng thái vào rõ ràng).
public sealed record EventSaveState(
    int DamePercent,
    int HpPercent,
    int MpPercent,
    int WomensDayEventPoint,
    int WomensDayLastExpRewardStage,
    int LunaNewYearEventPoint,
    int LunaNewYearLastExpRewardStage,
    int ChristmasEventPoint,
    int ChristmasLastExpRewardStage,
    long ReceiveMelonSeed);
