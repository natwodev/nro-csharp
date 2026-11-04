namespace Nro.Game.Players.Stats;

// Chứa các công thức thuần từ Utils/SkillUtil mà NPoint gọi.
// Nguồn Java: Utils/SkillUtil.java và nro/player/NPoint.java (SkillUtil chưa được chuyển; sẽ thay bằng lời gọi dịch vụ khi có, các công thức đều được kiểm chứng đối chiếu Java).

public static class NPointSkillMath
{
    // Tính phần trăm phân thân: 10 khi chưa chọn kỹ năng, nếu không bằng cấp kỹ năng nhân 10.
    public static int GetClonePercent(NPointSkillFacts? skillSelect) => skillSelect is null ? 10 : unchecked(skillSelect.Point * 10);

    // Tính phần trăm HP khi hóa khỉ, giữ phép nhân unchecked như Java.
    public static int GetPercentHpMonkey(int level) => unchecked((level + 3) * 10);

    // Tính phần trăm sát thương khi hóa khỉ, giữ phép cộng unchecked như Java.
    public static int GetMonkeyDamagePercent(int level) => unchecked(level + 3);

    // Tính phần trăm sát thương của Sức mạnh bộc phá, giữ phép tính unchecked như Java.
    public static int GetPowerBurstDamagePercent(int level) => unchecked((level + 1) * 5);

    // Tính tầm đánh Quả cầu kênh khí theo cấp kỹ năng, giữ phép tính unchecked như Java.
    public static int GetEnergyBallRange(int level) => unchecked(350 + (level * 30));

    // Tính phần trăm nạp năng lượng theo cấp kỹ năng, giữ phép cộng unchecked như Java.
    public static int GetPercentCharge(int level) => unchecked(level + 3);
}
