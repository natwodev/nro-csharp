namespace Nro.Game.Skills;

// Lưu một cấp kỹ năng và, khi gắn cho người chơi, các mốc thời gian hồi chiêu riêng của họ.
// Nguồn Java: nro/skill/Skill.java (các hằng byte dùng sbyte; Dispose chỉ bỏ tham chiếu mẫu vì GC không cần giải phóng thủ công).
public class Skill
{
    // Tầm đánh của các nhóm chiêu đấm và chưởng trong dữ liệu Java.
    public const int RANGE_ATTACK_CHIEU_DAM = 100;
    public const int RANGE_ATTACK_CHIEU_CHUONG = 300;

    public const sbyte DEMON = 2;
    public const sbyte MASENKO = 3;
    public const sbyte TRI_THUONG = 7;
    public const sbyte MAKANKOSAPPO = 11;
    public const sbyte DE_TRUNG = 12;
    public const sbyte LIEN_HOAN = 17;
    public const sbyte SOCOLA = 18;

    public const sbyte GALICK = 4;
    public const sbyte ANTOMIC = 5;
    public const sbyte TAI_TAO_NANG_LUONG = 8;
    public const sbyte BIEN_KHI = 13;
    public const sbyte TU_SAT = 14;
    public const sbyte HUYT_SAO = 21;
    public const sbyte TROI = 23;

    public const sbyte DRAGON = 0;
    public const sbyte KAMEJOKO = 1;
    public const sbyte THAI_DUONG_HA_SAN = 6;
    public const sbyte KAIOKEN = 9;
    public const sbyte QUA_CAU_KENH_KHI = 10;
    public const sbyte DICH_CHUYEN_TUC_THOI = 20;
    public const sbyte THOI_MIEN = 22;

    public const sbyte KHIEN_NANG_LUONG = 19;

    public const sbyte SUPER_KAME = 24;
    public const sbyte LIEN_HOAN_CHUONG = 25;
    public const sbyte MA_PHONG_BA = 26;

    // Mẫu kỹ năng dùng chung, null ở đối tượng trần và sau Dispose (Java: template).
    public SkillTemplate? Template { get; set; }

    // Mã cấp kỹ năng trong bảng dữ liệu.
    public short SkillId { get; set; }

    // Điểm nâng của cấp kỹ năng.
    public int Point { get; set; }

    // Sức mạnh tối thiểu để sử dụng.
    public long PowRequire { get; set; }

    // Thời gian hồi chiêu.
    public int CoolDown { get; set; }

    // Lần dùng kỹ năng gần nhất.
    public long LastTimeUseThisSkill { get; set; }

    // Lần bot dùng kỹ năng gần nhất.
    public long LastTimeUseThisSkillbot { get; set; }

    // Độ lệch ngang của kỹ năng.
    public int Dx { get; set; }

    // Độ lệch dọc của kỹ năng.
    public int Dy { get; set; }

    // Số mục tiêu tối đa có thể giao chiến.
    public int MaxFight { get; set; }

    // Mana tiêu hao.
    public int ManaUse { get; set; }

    // Sát thương cơ sở.
    public short Damage { get; set; }

    // Cấp hiện thời của kỹ năng đặc biệt.
    public short CurrLevel { get; set; }

    // Thông tin thêm, null đến khi nạp; có thể mang nguyên văn chuỗi "null" của cột info (Java: moreInfo).
    public string? MoreInfo { get; set; }

    // Giá mua cấp kỹ năng.
    public short Price { get; set; }

    // Tạo kỹ năng trần để trình nạp điền dữ liệu.
    public Skill()
    {
    }

    // Sao chép dữ liệu và dùng chung Template, nhưng cố ý không chép CurrLevel và LastTimeUseThisSkillbot nên chúng bằng 0 trong bản sao (quirk Java).
    public Skill(Skill skill)
    {
        ArgumentNullException.ThrowIfNull(skill);
        SkillId = skill.SkillId;
        Point = skill.Point;
        PowRequire = skill.PowRequire;
        CoolDown = skill.CoolDown;
        LastTimeUseThisSkill = skill.LastTimeUseThisSkill;
        Dx = skill.Dx;
        Dy = skill.Dy;
        MaxFight = skill.MaxFight;
        ManaUse = skill.ManaUse;
        Damage = skill.Damage;
        MoreInfo = skill.MoreInfo;
        Price = skill.Price;
        Template = skill.Template;
    }

    // Bỏ tham chiếu mẫu để đối tượng không còn giữ dữ liệu dùng chung (Java: dispose).
    public void Dispose()
    {
        Template = null;
    }
}
