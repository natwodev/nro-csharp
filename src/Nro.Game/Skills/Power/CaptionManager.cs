using System.Collections.ObjectModel;
using Nro.Data.Db;

namespace Nro.Game.Skills.Power;

// Nạp và tra cứu danh hiệu theo sức mạnh.
// Nguồn Java: nro/power/CaptionManager.java (không dùng singleton; copy-on-write/snapshot an toàn đa luồng; ORDER BY id; lỗi DB ném ra và nạp toàn bộ hoặc không nạp).
public sealed class CaptionManager : ICaptionManager
{
    private const string SelectCaptionsSql = "select id, earth, saiya, namek, power from caption order by id";

    private readonly object gate = new();
    private ReadOnlyCollection<Caption> captions = new(Array.Empty<Caption>());

    // Snapshot bất biến danh hiệu theo thứ tự nạp.
    public IReadOnlyList<Caption> Captions => Volatile.Read(ref captions);

    // Nạp nối thêm caption theo id; lỗi DB không tạo thay đổi một phần (Java: load).
    public async Task LoadAsync(INroSqlExecutor database, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        IResultSnapshot rs = await database.ExecuteQueryAsync(SelectCaptionsSql, cancellationToken);
        var loaded = new List<Caption>();
        while (rs.Next())
        {
            int id = rs.GetShort("id");
            string earth = rs.GetString("earth");
            string saiya = rs.GetString("saiya");
            string namek = rs.GetString("namek");
            long power = rs.GetLong("power");
            loaded.Add(new Caption(id, earth, saiya, namek, power));
        }

        lock (gate)
        {
            captions = new ReadOnlyCollection<Caption>([.. captions, .. loaded]);
        }
    }

    // Thêm danh hiệu bằng copy-on-write (Java: add).
    public void Add(Caption caption)
    {
        ArgumentNullException.ThrowIfNull(caption);
        lock (gate)
        {
            captions = new ReadOnlyCollection<Caption>([.. captions, caption]);
        }
    }

    // Xóa tham chiếu danh hiệu đầu tiên bằng copy-on-write (Java: remove).
    public bool Remove(Caption caption)
    {
        lock (gate)
        {
            int index = -1;
            for (int i = 0; i < captions.Count; i++)
            {
                if (ReferenceEquals(captions[i], caption))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                return false;
            }

            var copy = new List<Caption>(captions);
            copy.RemoveAt(index);
            captions = new ReadOnlyCollection<Caption>(copy);
            return true;
        }
    }

    // Tìm danh hiệu đầu tiên theo id (Java: find).
    public Caption? Find(int id)
    {
        foreach (Caption caption in Captions)
        {
            if (caption.Id == id)
            {
                return caption;
            }
        }

        return null;
    }

    // Lấy danh hiệu theo vị trí zero-based, ngoài phạm vi trả null (Java: findLevel).
    public Caption? FindLevel(int level)
    {
        IReadOnlyList<Caption> snapshot = Captions;
        return level >= 0 && level < snapshot.Count ? snapshot[level] : null;
    }

    // Quét ngược để lấy vị trí cuối có ngưỡng không vượt power; danh sách rỗng/không khớp trả 0 (Java: getLevel).
    public int GetLevel(long power)
    {
        IReadOnlyList<Caption> snapshot = Captions;
        int level = 0;
        for (int i = snapshot.Count - 1; i >= 0; i--)
        {
            long p = snapshot[i].Power;
            if (power >= p)
            {
                level = i;
                break;
            }
        }

        return level;
    }
}
