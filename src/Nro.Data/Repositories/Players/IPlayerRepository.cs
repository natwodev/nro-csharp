using Nro.Data.Db;
using Nro.Data.Models.Players;

namespace Nro.Data.Repositories.Players;

// Cung cấp hợp đồng lưu nguyên vẹn dòng player; việc ánh xạ sang đối tượng game thuộc Nro.Game.
// Nguồn Java: jbcd/dao/PlayerDAO.java (giữ JSON dạng văn bản để không làm thay đổi dữ liệu; repository là DI thay cho DAO static).
public interface IPlayerRepository
{
    // Tìm người chơi theo id (Java: getPlayer).
    Task<PlayerPersistenceRecord?> FindByIdAsync(int playerId, CancellationToken cancellationToken = default);

    // Tìm người chơi theo tên (Java: getPlayerByName).
    Task<PlayerPersistenceRecord?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

    // Tìm người chơi theo id tài khoản (Java: getPlayerByAccountId).
    Task<PlayerPersistenceRecord?> FindByAccountIdAsync(int accountId, CancellationToken cancellationToken = default);

    // Tạo người chơi cùng số dư tài khoản (Java: createNewPlayer).
    Task<PlayerPersistenceRecord> CreateAsync(PlayerPersistenceRecord player, PlayerAccountBalance accountBalance, CancellationToken cancellationToken = default);

    // Lưu các cột vật lý đã thay đổi của người chơi (Java: updatePlayer).
    Task SaveAsync(PlayerPersistenceRecord player, CancellationToken cancellationToken = default);
}
