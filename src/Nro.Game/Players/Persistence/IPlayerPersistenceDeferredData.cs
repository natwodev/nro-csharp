using Nro.Data.Repositories.Players;

namespace Nro.Game.Players.Persistence;

// Cung cấp dữ liệu cột hoãn xử lý mà vẫn giữ nguyên lexeme để lưu lại không mất dữ liệu.
// Nguồn Java: jbcd/dao/PlayerDAO.java (lớp phụ trợ cho PlayerPersistenceMapper; DEFERRED thuộc track sau được bảo toàn).


public interface IPlayerPersistenceDeferredData
{
    // Liệt kê các cột còn hoãn xử lý để lưu lại nguyên vẹn.

    IReadOnlySet<string> DeferredColumns { get; }

    // Lấy lexeme thô của cột hoãn xử lý.

    object? GetRawColumn(string column);
}

internal sealed class PlayerPersistenceDeferredData : IPlayerPersistenceDeferredData
{
    private readonly PlayerPersistenceRecord row;

    // Khởi tạo trạng thái hoãn xử lý từ row và tập cột được bảo toàn.
    public PlayerPersistenceDeferredData(PlayerPersistenceRecord row, IReadOnlySet<string> deferredColumns)
    {
        this.row = row;
        DeferredColumns = deferredColumns;
    }

    // Lưu trạng thái DeferredColumns (Java: deferredColumns).

    public IReadOnlySet<string> DeferredColumns { get; }

    // Thực hiện GetRawColumn (Java: getRawColumn).

    public object? GetRawColumn(string column)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(column);
        if (!DeferredColumns.Contains(column))
        {
            throw new ArgumentOutOfRangeException(nameof(column), column, "The column is hydrated by PlayerPersistenceMapper.");
        }

        return row[column];
    }
}
