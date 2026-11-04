namespace Nro.Game.Effects;

// Lưu cờ hoạt ảnh túi cờ (vật phẩm lưng) đang bật của người chơi; tám cờ loại trừ nhau theo quy ước và chỉ được chạm trong tick zone nên không cần khóa.
// Nguồn Java: nro/effect/EffectFlagBag.java (UseItem gọi Reset trước khi bật cờ; lớp chỉ chứa dữ liệu, không tự cưỡng chế tính loại trừ).
public class EffectFlagBag
{
    // Cho biết túi cờ vỏ ốc đang bật. (Java: useVoOc)
    public bool UseSnailShell { get; set; }

    // Cho biết túi cờ cây kem đang bật. (Java: useCayKem)
    public bool UseIceCream { get; set; }

    // Cho biết túi cờ cá heo đang bật. (Java: useCaHeo)
    public bool UseDolphin { get; set; }

    // Cho biết túi cờ con diều đang bật. (Java: useConDieu)
    public bool UseKite { get; set; }

    // Cho biết túi cờ diều rồng đang bật. (Java: useDieuRong)
    public bool UseDragonKite { get; set; }

    // Cho biết túi cờ mèo mun đang bật. (Java: useMeoMun)
    public bool UseBlackCat { get; set; }

    // Cho biết túi cờ xiên cá đang bật. (Java: useXienCa)
    public bool UseFishSkewer { get; set; }

    // Cho biết túi cờ phóng lợn đang bật. (Java: usePhongHeo)
    public bool UsePigLauncher { get; set; }

    // Tắt cả tám cờ (Java: reset); Java gán useVoOc hai lần nhưng C# gán một lần với kết quả tương đương.
    public void Reset()
    {
        UseSnailShell = false;
        UseIceCream = false;
        UseDolphin = false;
        UseKite = false;
        UseDragonKite = false;
        UseBlackCat = false;
        UseFishSkewer = false;
        UsePigLauncher = false;
    }

    // Giữ phương thức rỗng dispose của Java để Player gọi; khác Java, Player không gán thành phần này null nên cờ còn giá trị nhưng không còn được đọc sau dispose (Java: dispose).
    public void Dispose()
    {
    }
}
