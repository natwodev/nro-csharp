using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Effects;

// Gom các phụ thuộc máy chủ mà EffectSkill và EffectSkin cần, thay singleton tĩnh Java bằng môi trường dùng chung không có trạng thái theo người chơi.
// Nguồn Java: nro/effect/EffectSkill.java và nro/effect/EffectSkin.java (lớp ghép nối C#; factory H-01 phải gán cùng một thể hiện vào Environment của từng hiệu ứng).
public sealed class EffectEnvironment
{
    // Cung cấp thời điểm hiện tại cho mọi phép kiểm tra thời gian.
    public required VietnamClock Clock { get; init; }

    // Cung cấp ngẫu nhiên thay cho Util.nextInt và Util.isTrue của Java.
    public required RandomUtil Random { get; init; }

    // Nhận ngoại lệ bị nuốt thay cho e.printStackTrace và Logger.logException của Java.
    public required GameLogger Logger { get; init; }

    // Chứa cờ công thức Manager.readInt của Java.
    public required EffectSettings Settings { get; init; }

    // Gửi gói tin hiệu ứng.
    public required IEffectBroadcaster Broadcaster { get; init; }

    // Gửi lệnh làm mới client thay cho Service, PlayerService và ItemTimeService của Java.
    public required IEffectClientNotifier Notifier { get; init; }

    // Truy cập mô hình người chơi.
    public required IEffectPlayerAccess Players { get; init; }

    // Truy cập túi đồ.
    public required IEffectInventoryAccess Inventory { get; init; }

    // Áp dụng quy tắc bản đồ và kỹ năng.
    public required IEffectRules Rules { get; init; }

    // Dịch vụ hiệu ứng bản đồ.
    public required IEffectMapService MapEffects { get; init; }

    // Dịch vụ hiệu ứng kỹ năng.
    public required IEffectSkillService Skills { get; init; }

    // Dịch vụ hiệu ứng trang phục.
    public required IEffectSkinService Skins { get; init; }

    // Tạo ba dịch vụ từ các cổng đã cho và trả môi trường đã nối; dùng cho host và kiểm thử, chỉ ghép tay khi cần thay dịch vụ.
    public static EffectEnvironment Create(
        VietnamClock clock,
        RandomUtil random,
        GameLogger logger,
        EffectSettings settings,
        IEffectScheduler scheduler,
        IEffectBroadcaster broadcaster,
        IEffectClientNotifier notifier,
        IEffectPlayerAccess players,
        IEffectInventoryAccess inventory,
        IEffectRules rules)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(broadcaster);
        ArgumentNullException.ThrowIfNull(notifier);
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(rules);

        var mapEffects = new EffectMapService(broadcaster, logger);
        var skills = new EffectSkillService(clock, random, logger, settings, scheduler, broadcaster, notifier, players, rules);
        var skins = new EffectSkinService(logger, mapEffects, skills, broadcaster, notifier, players);
        return new EffectEnvironment
        {
            Clock = clock,
            Random = random,
            Logger = logger,
            Settings = settings,
            Broadcaster = broadcaster,
            Notifier = notifier,
            Players = players,
            Inventory = inventory,
            Rules = rules,
            MapEffects = mapEffects,
            Skills = skills,
            Skins = skins,
        };
    }
}
