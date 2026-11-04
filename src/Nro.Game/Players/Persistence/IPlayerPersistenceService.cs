namespace Nro.Game.Players.Persistence;

// Định nghĩa dịch vụ lưu/tải người chơi cho ứng dụng.
// Nguồn Java: jbcd/data/GodGK.java (interface C# phụ trợ cho lớp chính PlayerPersistenceService, thay điểm vào tĩnh GodGK/PlayerDAO).


public interface IPlayerPersistenceService
{
    // Thao tác lưu trữ người chơi của dịch vụ (Java: GodGK/PlayerDAO).

    Task<Player?> LoadByIdAsync(int playerId, long nowMs, CancellationToken cancellationToken = default);
    // Thao tác lưu trữ người chơi của dịch vụ (Java: GodGK/PlayerDAO).

    Task<Player?> LoadByNameAsync(string name, long nowMs, CancellationToken cancellationToken = default);
    // Thao tác lưu trữ người chơi của dịch vụ (Java: GodGK/PlayerDAO).

    Task<Player> CreateNewAsync(int accountId, string name, sbyte gender, short head, long nowMs, CancellationToken cancellationToken = default);
    // Thao tác lưu trữ người chơi của dịch vụ (Java: GodGK/PlayerDAO).

    Task SaveAsync(Player player, CancellationToken cancellationToken = default);
}
