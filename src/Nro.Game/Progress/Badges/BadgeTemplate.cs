using Nro.Game.Items.Core;

namespace Nro.Game.Progress.Badges;

// Mẫu huy hiệu bất biến cùng các option đã resolve. Nguồn Java: nro/player_badges/BagesTemplate.java (dòng 1–30).
public sealed record BadgeTemplate(int Id, int EffectId, int ItemId, string Name, IReadOnlyList<ItemOption> Options);
