namespace Nro.Game.Items.Core;

// Cung cấp các phép nhận diện thuần theo id cho vật phẩm rơi trên bản đồ.
// Nguồn Java: models/Item/ItemMapService.java (PARTIAL: chưa chuyển pickItem, ba hàm xóa/gửi item map và findItemMapByPlayer vì còn cần Zone, ItemMap, IDMark, Service; quy tắc pick Java là hút đồ hoặc quá 1000 ms; lớp tĩnh không trạng thái thay singleton Java).
public static class ItemMapService
{
    // Kiểm tra bảy ngọc rồng, id mẫu 372 đến 378 (Java: isBlackBall).
    public static bool IsBlackBall(int tempId) => tempId >= 372 && tempId <= 378;

    // Kiểm tra ngọc rồng Namec, id mẫu 353 đến 360 (Java: isNamecBall).
    public static bool IsNamecBall(int tempId) => tempId >= 353 && tempId <= 360;

    // Kiểm tra đá ngọc rồng Namec, id mẫu 362 (Java: isNamecBallStone).
    public static bool IsNamecBallStone(int tempId) => tempId == 362;
}
