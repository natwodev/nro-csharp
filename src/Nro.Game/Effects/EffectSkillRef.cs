namespace Nro.Game.Effects;

// Tham chiếu bất biến gồm id thực thể kỹ năng và cấp kỹ năng để hiệu ứng đọc; là value type nên bản chụp không thể đổi dưới lúc đọc.
// Nguồn Java: nro/skill/Skill.java (DTO phụ trợ cho EffectSkill).
// Id thực thể kỹ năng, không phải id template; Java dùng short (Java: skillId).
// Cấp kỹ năng (Java: point).
public readonly record struct EffectSkillRef(short SkillId, int Point);
