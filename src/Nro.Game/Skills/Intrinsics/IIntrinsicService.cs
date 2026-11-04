namespace Nro.Game.Skills.Intrinsics;

// Hợp đồng dịch vụ nội tại.
// Nguồn Java: nro/intrinsic/IntrinsicService.java (lớp chính thay singleton IntrinsicService.gI()).
public interface IIntrinsicService
{
    // Lấy nội tại theo hệ (Java: getIntrinsics).
    IReadOnlyList<Intrinsic> GetIntrinsics(sbyte playerGender);

    // Lấy bản sao nội tại theo id, không có trả null (Java: getIntrinsicById).
    Intrinsic? GetIntrinsicById(int id);

    // Gửi gói 112/0 gồm icon và tên nội tại hiện tại (Java: sendInfoIntrinsic).
    void SendInfoIntrinsic(Player player);

    // Gửi gói 112/1 liệt kê mọi nội tại của hệ người chơi (Java: showAllIntrinsic).
    void ShowAllIntrinsic(Player player);

    // Hiển thị menu nội tại (Java: showMenu).
    void ShowMenu(Player player);

    // Hiển thị xác nhận mở nội tại thường (Java: showConfirmOpen).
    void ShowConfirmOpen(Player player);

    // Hiển thị xác nhận mở nội tại VIP (Java: showConfirmOpenVip).
    void ShowConfirmOpenVip(Player player);

    // Quay nội tại ngẫu nhiên không tính phí (Java: DoiNoiTai).
    void DoiNoiTai(Player player);

    // Trả giá theo số lần quay hiện tại rồi quay nội tại mới (Java: open).
    void Open(Player player);

    // Trả 200 triệu vàng, quay nội tại và đặt lại số lần quay (Java: openVip).
    void OpenVip(Player player);
}
