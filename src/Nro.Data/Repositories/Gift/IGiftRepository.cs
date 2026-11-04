using Nro.Data.Db;
using Nro.Data.Models.Gift;
namespace Nro.Data.Repositories.Gift;

// Cung cấp hợp đồng lưu hai quà đăng nhập đầu tiên của người chơi.
// Nguồn Java: jbcd/dao/GiftDAO.java (thay trạng thái static bằng repository bất đồng bộ và lưu có giao dịch).
public interface IGiftRepository
{
    // Tải trạng thái quà, hoặc tạo mới khi chưa có (Java: loadGiftPlayer, createGiftPlayer).
    Task<GiftPlayerState> LoadOrCreateAsync(int playerId, CancellationToken cancellationToken = default);

    // Lưu trạng thái quà của người chơi (Java: saveGiftPlayer).
    Task SaveAsync(int playerId, GiftPlayerState gift, CancellationToken cancellationToken = default);
}
