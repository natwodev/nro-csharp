namespace Nro.Game.Progress.Badges;

// Điều kiện huy hiệu bất biến của catalog tĩnh. Nguồn Java: nro/task/BadgesTaskTemplate.java (dòng 1–20).
public sealed record BadgeTaskTemplate(int Id, string Name, int Count, int BadgeRewardId);
