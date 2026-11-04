namespace Nro.Game.Effects.Ports;

// Mô tả tùy chọn vật phẩm gồm id template và tham số có thể thay đổi.
// Nguồn Java: nro/models/item/ItemOption.java (cổng phụ trợ cho lớp chính do track khác sở hữu).
public interface IEffectItemOption
{
    // Id template tùy chọn (Java: optionTemplate.id).
    int OptionId { get; }

    // Tham số hiệu ứng sửa tại chỗ, như thưởng trang phục hoặc thời gian giáp luyện tập (Java: param).
    int Param { get; set; }
}
