namespace Nro.Game.Skills;

// Hợp đồng bảng hệ số sát thương theo kỹ năng.
// Nguồn Java: nro/config/SkillDamageConfig.java (lớp chính SkillDamageConfig thay HashMap static bằng đối tượng tiêm được, an toàn đa luồng).
public interface ISkillDamageConfig
{
    // Lấy hệ số hoặc 1,0 khi chưa cấu hình (Java: getMultiplier).
    double GetMultiplier(sbyte skillId);

    // Đặt/thay hệ số không kiểm miền giá trị như Java (Java: setMultiplier).
    void SetMultiplier(sbyte skillId, double value);
}
