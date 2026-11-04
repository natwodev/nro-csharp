namespace Nro.Game.Effects.Helpers;

// Chứa các cờ công thức hiệu ứng, do host gán một lần lúc khởi động và dùng chung trong một máy chủ nhưng không static để các máy chủ trong cùng tiến trình kiểm thử không dùng lẫn nhau.
// Nguồn Java: nro/effect/EffectSkill.java (lớp phụ trợ C# cho Manager.readInt).
public sealed class EffectSettings
{
    // Tương ứng Manager.readInt: khi đúng, mọi lượng HP hiệu ứng tính được bị giới hạn int.MaxValue rồi cắt về int trong Util.CrisGH; Java lấy giá trị mặc định từ cấu hình máy chủ.
    public bool ReadInt { get; set; }
}
