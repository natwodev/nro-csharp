namespace Nro.Data.Models.Gift;

// Gom hai cờ quà tân thủ một lần của một người chơi từ bảng gift.
// Nguồn Java: jbcd/dao/GiftDAO.java (chỉ biểu diễn trạng thái; việc đọc và ghi do GiftRepository thực hiện).
// Cờ đã nhận quà vàng tân thủ.
// Cờ đã nhận quà ngọc tân thủ.
public sealed record GiftPlayerState(bool GoldTanThu, bool GemTanThu);
