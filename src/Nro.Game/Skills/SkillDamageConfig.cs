using System.Collections.Concurrent;

namespace Nro.Game.Skills;

// Bảng hệ số sát thương riêng cho từng kỹ năng; mặc định 1,0 khi không có cấu hình.
// Nguồn Java: nro/config/SkillDamageConfig.java (HashMap static được thay bằng ConcurrentDictionary theo instance để đọc/ghi đa luồng an toàn; các instance không dùng chung thay đổi).
public sealed class SkillDamageConfig : ISkillDamageConfig
{
    private readonly ConcurrentDictionary<sbyte, double> multipliers = new();

    public SkillDamageConfig()
    {
        // Hệ Trái Đất.
        multipliers[Skill.DRAGON] = 1.0;
        multipliers[Skill.KAMEJOKO] = 0.8;
        multipliers[Skill.KAIOKEN] = 1.2;
        multipliers[Skill.DICH_CHUYEN_TUC_THOI] = 0.8;

        // Hệ Saiyan.
        multipliers[Skill.GALICK] = 1.2;
        multipliers[Skill.ANTOMIC] = 1.5;

        // Hệ Namek.
        multipliers[Skill.DEMON] = 1.0;
        multipliers[Skill.MASENKO] = 1.0;
        multipliers[Skill.LIEN_HOAN] = 1.1;

        // Kỹ năng đặc biệt.
        multipliers[Skill.TU_SAT] = 3.0;
        multipliers[Skill.QUA_CAU_KENH_KHI] = 2.5;
        multipliers[Skill.MAKANKOSAPPO] = 3.0;
    }

    // Trả hệ số sát thương của kỹ năng, hoặc 1,0 khi chưa cấu hình (Java: getMultiplier).
    public double GetMultiplier(sbyte skillId) => multipliers.TryGetValue(skillId, out double value) ? value : 1.0;

    // Thêm hoặc thay hệ số sát thương mà không giới hạn miền giá trị như Java (Java: setMultiplier).
    public void SetMultiplier(sbyte skillId, double value) => multipliers[skillId] = value;
}
