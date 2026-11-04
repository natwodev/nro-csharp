namespace Nro.Data.Models.Event;

// Dữ liệu hàng hung_vuong của bảng event.
// Nguồn Java: jbcd/dao/EventDAO.java (Java: loadHungVuongEvent; ReceiveMelonSeed NULL khi khóa thiếu hoặc không đọc được để nơi gọi giữ giá trị hiện tại).
public sealed record HungVuongEventData(long? ReceiveMelonSeed);
