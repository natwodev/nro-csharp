namespace Nro.Game.Items.Time;

// Gom các bộ đếm ngoài ItemTime để ItemTimeService thông báo thời gian hợp thể, kỹ năng hiệu ứng và phó bản bang.
// Nguồn Java: models/Item/ItemTimeService.java, player.fusion, player.effectSkill, player.clan (host lấy từ đối tượng thật; các thời lượng hằng Java được mang vào đây theo mili giây).
public sealed class ExternalItemTimes
{
    // Trạng thái rỗng khi người chơi không hợp thể, không có hiệu ứng và không thuộc bang.
    public static ExternalItemTimes None { get; } = new();

    // Kiểu hợp thể hiện tại (Java: player.fusion.typeFusion).
    public sbyte FusionType { get; init; }

    // Mốc bắt đầu hợp thể theo mili giây epoch (Java: player.fusion.lastTimeFusion).
    public long FusionLastTime { get; init; }

    // Thời lượng hợp thể theo mili giây (Java: Fusion.TIME_FUSION).
    public long FusionDurationMs { get; init; }

    // Cờ hiệu ứng Cumber thứ nhất (Java: player.effectSkill.iscumber).
    public bool IsCumber { get; init; }

    // Mốc bắt đầu hiệu ứng Cumber thứ nhất (Java: player.effectSkill.lastTimeUpcumber).
    public long LastTimeCumber { get; init; }

    // Cờ hiệu ứng Cumber thứ hai (Java: player.effectSkill.iscumber2).
    public bool IsCumber2 { get; init; }

    // Mốc bắt đầu hiệu ứng Cumber thứ hai (Java: player.effectSkill.lastTimeUpcumber2).
    public long LastTimeCumber2 { get; init; }

    // Cờ hiệu ứng đau (Java: player.effectSkill.isPain).
    public bool IsPain { get; init; }

    // Mốc bắt đầu hiệu ứng đau (Java: player.effectSkill.lastTimeUpPain).
    public long LastTimeUpPain { get; init; }

    // Cờ hiệu ứng Kefla (Java: player.effectSkill.iskefla).
    public bool IsKefla { get; init; }

    // Mốc bắt đầu hiệu ứng Kefla (Java: player.effectSkill.lastTimeUpkefla).
    public long LastTimeKefla { get; init; }

    // Các bộ đếm phó bản bang, hoặc null khi người chơi chưa có bang.
    public ClanDungeonTimes? Clan { get; init; }
}
