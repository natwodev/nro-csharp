using System.Collections.ObjectModel;
using Nro.Data.Db;

namespace Nro.Game.Skills.Power;

// Nạp và tra cứu các mốc giới hạn sức mạnh.
// Sẽ thay bằng Nro.Content ở J-01F.
// Nguồn Java: nro/power/PowerLimitManager.java (không dùng singleton; copy-on-write/snapshot an toàn đa luồng; ORDER BY id là bắt buộc vì Get theo vị trí; lỗi DB ném ra và nạp atomically).
public sealed class PowerLimitManager : IPowerLimitManager
{
    private const string SelectPowerLimitsSql = "select id, power, hp, mp, damage, defense, critical from power_limit order by id";

    private readonly object gate = new();
    private ReadOnlyCollection<PowerLimit> powers = new(Array.Empty<PowerLimit>());

    // Snapshot bất biến các mốc giới hạn theo thứ tự nạp.
    public IReadOnlyList<PowerLimit> Powers => Volatile.Read(ref powers);

    // Nạp nối thêm power_limit theo id; lỗi DB không thay đổi một phần (Java: load).
    public async Task LoadAsync(INroSqlExecutor database, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        IResultSnapshot rs = await database.ExecuteQueryAsync(SelectPowerLimitsSql, cancellationToken);
        var loaded = new List<PowerLimit>();
        while (rs.Next())
        {
            int id = rs.GetShort("id");
            long power = rs.GetLong("power");
            long hp = rs.GetInt("hp");
            long mp = rs.GetInt("mp");
            long damage = rs.GetInt("damage");
            int defense = rs.GetInt("defense");
            int critical = rs.GetInt("critical");
            loaded.Add(new PowerLimit(id, power, hp, mp, damage, defense, critical));
        }

        lock (gate)
        {
            powers = new ReadOnlyCollection<PowerLimit>([.. powers, .. loaded]);
        }
    }

    // Thêm mốc bằng copy-on-write (Java: add).
    public void Add(PowerLimit powerLimit)
    {
        ArgumentNullException.ThrowIfNull(powerLimit);
        lock (gate)
        {
            powers = new ReadOnlyCollection<PowerLimit>([.. powers, powerLimit]);
        }
    }

    // Xóa tham chiếu mốc đầu tiên bằng copy-on-write (Java: remove).
    public bool Remove(PowerLimit powerLimit)
    {
        lock (gate)
        {
            int index = -1;
            for (int i = 0; i < powers.Count; i++)
            {
                if (ReferenceEquals(powers[i], powerLimit))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                return false;
            }

            var copy = new List<PowerLimit>(powers);
            copy.RemoveAt(index);
            powers = new ReadOnlyCollection<PowerLimit>(copy);
            return true;
        }
    }

    // Lấy mốc theo vị trí, không phải id; chỉ số âm/ngoài phạm vi trả null (Java: get).
    public PowerLimit? Get(int index)
    {
        IReadOnlyList<PowerLimit> snapshot = Powers;
        if (index < 0 || index >= snapshot.Count)
        {
            return null;
        }

        return snapshot[index];
    }
}
