namespace Nro.Game.Effects.Ports;

// Mô tả vật phẩm tối thiểu mà hiệu ứng cần đọc.
// Nguồn Java: nro/models/item/Item.java (cổng phụ trợ cho lớp chính do track khác sở hữu).
public interface IEffectItem
{
    // Đúng khi ô có vật phẩm; ô trống vẫn là đối tượng "null item" không template và trả false (Java: isNotNullItem).
    bool IsNotNullItem { get; }

    // Id template, chỉ có nghĩa khi IsNotNullItem đúng (Java: template.id).
    int TemplateId { get; }

    // Các tùy chọn vật phẩm; có thể null và từng phần tử cũng có thể null như trường itemOptions của Java.
    IReadOnlyList<IEffectItemOption?>? Options { get; }
}
