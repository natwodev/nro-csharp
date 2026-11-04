namespace Nro.Game.World.Maps;

// Lưu một cặp khóa/giá trị cấu hình hiệu ứng bản đồ.
// Nguồn Java: nro/map/EffectMap.java (getter/setter Lombok được thay bằng thuộc tính C#).
public class EffectMap
{
    // Khóa nhận diện hiệu ứng.
    public string? Key { get; set; }

    // Giá trị cấu hình hiệu ứng.
    public string? Value { get; set; }
}
