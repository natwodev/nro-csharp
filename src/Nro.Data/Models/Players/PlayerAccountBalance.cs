namespace Nro.Data.Models.Players;

// Gom các giá trị tài khoản được khởi tạo cùng lúc với người chơi mới.
// Nguồn Java: jbcd/dao/PlayerDAO.java (cập nhật số dư tài khoản trước khi chèn người chơi trong cùng giao dịch).
// Id tài khoản liên kết với hàng người chơi mới.
// Số dư ví VND ban đầu.
// Tổng tiền đã nạp ban đầu.
// Số dư coin ban đầu.
public sealed record PlayerAccountBalance(int AccountId, int Vnd, int TotalTopUp, int Coin);
