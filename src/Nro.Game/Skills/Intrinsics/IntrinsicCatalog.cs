using System.Collections.ObjectModel;
using Nro.Core.Consts;
using Nro.Data.Db;

namespace Nro.Game.Skills.Intrinsics;

// Catalog các nội tại được nạp từ PostgreSQL và phân theo hệ.
// Nguồn Java: Manager (ORDER BY id làm rõ thứ tự MySQL vốn ngầm định; vị trí 0 là không có nội tại; GetByte giữ tràn id trên 127 thành âm).
public sealed class IntrinsicCatalog : IIntrinsicCatalog
{
    private const string SelectIntrinsicsSql = "select id, name, param_from_1, param_to_1, param_from_2, param_to_2, icon, gender from intrinsic order by id";

    private Snapshot snapshot = Snapshot.Empty;

    // Toàn bộ nội tại theo thứ tự id trong snapshot hiện hành.
    public IReadOnlyList<Intrinsic> Intrinsics => Volatile.Read(ref snapshot).All;

    // Lấy danh sách Trái Đất, Namek hoặc Saiyan; mọi giá trị khác chọn Saiyan như Java.
    public IReadOnlyList<Intrinsic> GetIntrinsics(sbyte playerGender)
    {
        Snapshot current = Volatile.Read(ref snapshot);
        switch (playerGender)
        {
            case 0:
                return current.Earth;
            case 1:
                return current.Namek;
            default:
                return current.Saiyan;
        }
    }

    // Trả bản sao nội tại đầu tiên theo id để không sửa mẫu catalog (Java: getIntrinsicById).
    public Intrinsic? GetIntrinsicById(int id)
    {
        foreach (Intrinsic intrinsic in Intrinsics)
        {
            if (intrinsic.Id == id)
            {
                return new Intrinsic(intrinsic);
            }
        }

        return null;
    }

    // Nạp atomically toàn bộ snapshot; gender 3/khác thêm vào cả ba hệ (Java: Manager).
    public async Task LoadAsync(INroSqlExecutor database, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        IResultSnapshot rs = await database.ExecuteQueryAsync(SelectIntrinsicsSql, cancellationToken);
        var all = new List<Intrinsic>();
        var earth = new List<Intrinsic>();
        var namek = new List<Intrinsic>();
        var saiyan = new List<Intrinsic>();
        while (rs.Next())
        {
            var intrinsic = new Intrinsic
            {
                Id = rs.GetByte("id"),
                Name = rs.GetString("name"),
                ParamFrom1 = rs.GetShort("param_from_1"),
                ParamTo1 = rs.GetShort("param_to_1"),
                ParamFrom2 = rs.GetShort("param_from_2"),
                ParamTo2 = rs.GetShort("param_to_2"),
                Icon = rs.GetShort("icon"),
                Gender = rs.GetByte("gender"),
            };
            switch (intrinsic.Gender)
            {
                case ConstPlayer.EARTHLING:
                    earth.Add(intrinsic);
                    break;
                case ConstPlayer.NAMEKIAN:
                    namek.Add(intrinsic);
                    break;
                case ConstPlayer.SAIYAN:
                    saiyan.Add(intrinsic);
                    break;
                default:
                    earth.Add(intrinsic);
                    namek.Add(intrinsic);
                    saiyan.Add(intrinsic);
                    break;
            }

            all.Add(intrinsic);
        }

        Volatile.Write(
            ref snapshot,
            new Snapshot(new ReadOnlyCollection<Intrinsic>(all), new ReadOnlyCollection<Intrinsic>(earth), new ReadOnlyCollection<Intrinsic>(namek), new ReadOnlyCollection<Intrinsic>(saiyan)));
    }

    private sealed record Snapshot(IReadOnlyList<Intrinsic> All, IReadOnlyList<Intrinsic> Earth, IReadOnlyList<Intrinsic> Namek, IReadOnlyList<Intrinsic> Saiyan)
    {
        public static readonly Snapshot Empty = new([], [], [], []);
    }
}
