using Nro.Data.Db;
using Nro.Data.Repositories.Players;

namespace Nro.Game.Players.Persistence;

// Tải, tạo và lưu người chơi qua DI thay cho luồng static cũ.
// Nguồn Java: jbcd/data/GodGK.java (DI thay static GodGK; PlayerDAO đảm nhiệm repository).


public sealed class PlayerPersistenceService : IPlayerPersistenceService
{
    private readonly IPlayerRepository _players;
    private readonly PlayerPersistenceMapper _mapper;

    // Khởi tạo PlayerPersistenceService.

    public PlayerPersistenceService(IPlayerRepository players, PlayerPersistenceMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(mapper);
        _players = players;
        _mapper = mapper;
    }

    // Tải player theo Id qua repository và mapper.

    public async Task<Player?> LoadByIdAsync(int playerId, long nowMs, CancellationToken cancellationToken = default)
    {
        var row = await _players.FindByIdAsync(playerId, cancellationToken);
        return row is null ? null : _mapper.Load(row, nowMs);
    }

    // Tải player theo tên qua repository và mapper.

    public async Task<Player?> LoadByNameAsync(string name, long nowMs, CancellationToken cancellationToken = default)
    {
        var row = await _players.FindByNameAsync(name, cancellationToken);
        return row is null ? null : _mapper.Load(row, nowMs);
    }

    // Kiểm tra tên, tạo row, lưu số dư ban đầu 0 rồi nạp Player.

    public async Task<Player> CreateNewAsync(int accountId, string name, sbyte gender, short head, long nowMs, CancellationToken cancellationToken = default)
    {
        PlayerCharacterName.EnsureValidStoredName(name);
        var draft = _mapper.CreateNew(accountId, name, gender, head, nowMs);
        var saved = await _players.CreateAsync(draft, new PlayerAccountBalance(accountId, 0, 0, 0), cancellationToken);
        return _mapper.Load(saved, nowMs);
    }

    // Ánh xạ Player rồi lưu qua repository.

    public async Task SaveAsync(Player player, CancellationToken cancellationToken = default)
    {
        var row = _mapper.Save(player);
        await _players.SaveAsync(row, cancellationToken);
    }
}
