using Nro.Game.Skills;

namespace Nro.Game.Players.Stats;

// Nguồn Java: Utils/SkillUtil.java (dòng 84–170) và nro/player/NPoint.java.
// Lớp tương thích cho NPoint; mọi công thức ủy quyền SkillUtil để chỉ còn một bản quy tắc.

public static class NPointSkillMath
{
    // Tính phần trăm phân thân từ facts NPoint. (Java: getPercentPhanThan)
    public static int GetClonePercent(NPointSkillFacts? skillSelect) => skillSelect is null ? 10 : SkillUtil.GetClonePercentForPoint(skillSelect.Point);

    // Tính phần trăm HP khi hóa khỉ. (Java: getPercentHpMonkey)
    public static int GetPercentHpMonkey(int level) => SkillUtil.GetPercentHpMonkey(level);

    // Tính phần trăm sát thương khi hóa khỉ. (Java: getPercentDameMonkey)
    public static int GetMonkeyDamagePercent(int level) => SkillUtil.GetMonkeyDamagePercent(level);

    // Tính phần trăm sát thương Sức mạnh bộc phá. (Java: getPercentDameSucManhBocPha)
    public static int GetPowerBurstDamagePercent(int level) => SkillUtil.GetPowerBurstDamagePercent(level);

    // Tính tầm đánh Quả cầu kênh khí theo cấp kỹ năng. (Java: getRangeQCKK)
    public static int GetEnergyBallRange(int level) => SkillUtil.GetEnergyBallRange(level);

    // Tính phần trăm nạp năng lượng theo cấp kỹ năng. (Java: getPercentCharge)
    public static int GetPercentCharge(int level) => SkillUtil.GetPercentCharge(level);
}
