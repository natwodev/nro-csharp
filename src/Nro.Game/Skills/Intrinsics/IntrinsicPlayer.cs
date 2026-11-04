
namespace Nro.Game.Skills.Intrinsics;

// Lưu nội tại của người chơi và số lần đã trả phí để quay lại.
// Nguồn Java: nro/intrinsic/IntrinsicPlayer.java (nhận catalog thay singleton; constructor trần và thiếu id 0 dùng Intrinsic rỗng; Dispose giữ Intrinsic khác null theo quy tắc Player shell).
public class IntrinsicPlayer
{
    // Số lần quay đã trả phí, đồng thời là chỉ mục bảng giá; service giới hạn tối đa (Java: countOpen).
    public sbyte CountOpen { get; set; }

    public Intrinsic Intrinsic { get; set; }

    public IntrinsicPlayer()
    {
        Intrinsic = new Intrinsic();
    }

    public IntrinsicPlayer(IIntrinsicCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        Intrinsic = catalog.GetIntrinsicById(0) ?? new Intrinsic();
    }

    // Không đặt nội tại thành null như Java vì thành phần do Player tạo phải luôn khác null (Java: dispose).
    public void Dispose()
    {
    }
}
    // Nội tại hiện tại, luôn khác null.
    // Tạo nội tại rỗng id 0 cho Player shell.
    // Tạo nội tại từ catalog; thiếu entry id 0 thì dùng nội tại rỗng.
