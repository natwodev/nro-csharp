using Nro.Data.Db;
using System.Data;

namespace Nro.Data.Repositories.Top;

// Kho truy vấn các bảng xếp hạng, tách dữ liệu ứng viên và chi tiết để sắp xếp tương thích Java trong C#.
// Nguồn Java: jbcd/dao/TopDAO.java (dùng truy vấn có tham số, không phân tích JSON tại PostgreSQL; chỉ đọc items_body của người thắng, và bảo toàn thứ tự hòa theo id).
public sealed class TopRepository : ITopRepository
{
    // SQL Java nối player với account và chỉ giữ account.ban = 0, trừ TOP_THOI_VANG, TOP_NAP và TOP_DUA_NAP.
    private const string MayDamTraiDatSql = "select p.id as pid, p.data_may_dam as k from player p join account a on a.id = p.account_id where a.ban = 0 and p.gender = 0";
    private const string MayDamNamecSql = "select p.id as pid, p.data_may_dam as k from player p join account a on a.id = p.account_id where a.ban = 0 and p.gender = 1";
    private const string MayDamXaydaSql = "select p.id as pid, p.data_may_dam as k from player p join account a on a.id = p.account_id where a.ban = 0 and p.gender = 2";
    private const string DataPointSql = "select p.id as pid, p.data_point as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string PetSql = "select p.id as pid, p.pet as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string VndSql = "select p.id as pid, a.vnd as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string CoinSql = "select p.id as pid, a.coin as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string DaNapSql = "select p.id as pid, a.danap as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string DataInventorySql = "select p.id as pid, p.data_inventory as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string NamekWarSql = "select p.id as pid, p.diem_chien_truong_namek as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string NewYearSql = "select p.id as pid, p.data_event_new_year as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string ChristmasSql = "select p.id as pid, p.data_event_christ_mas as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string VuLanSql = "select p.id as pid, p.data_event_vulan as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string HalloweenSql = "select p.id as pid, p.data_event_halloween as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string Event83Sql = "select p.id as pid, p.data_event_8_3 as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string TrungThuSql = "select p.id as pid, p.data_event_trung_thu as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string HungVuongSql = "select p.id as pid, p.data_event_hung_vuong as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string BlackFridaySql = "select p.id as pid, p.data_event_black_friday as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string PvpSql = "select p.id as pid, p.point_pvp as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string NapSql = "select p.id as pid, a.cash as k from player p join account a on a.id = p.account_id";
    private const string DuaNapSql = "select p.id as pid, a.danap as k from player p join account a on a.id = p.account_id where a.danap >= 100000";

    private const string TaskSql = "select p.id as pid, p.data_task as k, p.last_time_login_game as login from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string WhisSql = "select p.id as pid, p.data_luyentap as k from player p join account a on a.id = p.account_id where a.ban = 0";
    private const string VdstSql = "select p.id as pid, p.vodaisinhtu as k from player p join account a on a.id = p.account_id where a.ban = 0";

    // strpos tương đương LIKE '%"[457,%' nhưng không có ký tự đại diện.
    private const string ThoiVangSql = "select p.id as pid, p.items_bag as bag, p.items_box as box from player p join account a on a.id = p.account_id where strpos(p.items_box, '\"[457,') > 0 or strpos(p.items_bag, '\"[457,') > 0";

    private const string DetailSql =
        "select p.id as pid, p.account_id as aid, p.name as pname, p.gender as gender, p.items_body as body, p.last_time_login_game as login from player p where p.id = any(?)";

    private const int DefaultLimit = 100;

    private static readonly Dictionary<TopRanking, RankingSpec> Specs = new()
    {
        [TopRanking.PunchingMachineEarthling] = new(MayDamTraiDatSql, KeyKind.JsonIndexUnquoted, 0, DefaultLimit),
        [TopRanking.PunchingMachineNamekian] = new(MayDamNamecSql, KeyKind.JsonIndexUnquoted, 1, DefaultLimit),
        [TopRanking.PunchingMachineSaiyan] = new(MayDamXaydaSql, KeyKind.JsonIndexUnquoted, 2, DefaultLimit),
        [TopRanking.Power] = new(DataPointSql, KeyKind.JsonIndex, 1, DefaultLimit),
        [TopRanking.Hp] = new(DataPointSql, KeyKind.JsonIndex, 14, DefaultLimit),
        [TopRanking.Ki] = new(DataPointSql, KeyKind.JsonIndex, 15, DefaultLimit),
        [TopRanking.Damage] = new(DataPointSql, KeyKind.JsonIndex, 13, DefaultLimit),
        [TopRanking.PetPower] = new(PetSql, KeyKind.NestedJsonIndex, 1, DefaultLimit),
        [TopRanking.PetHp] = new(PetSql, KeyKind.NestedJsonIndex, 10, DefaultLimit),
        [TopRanking.PetKi] = new(PetSql, KeyKind.NestedJsonIndex, 11, DefaultLimit),
        [TopRanking.PetDamage] = new(PetSql, KeyKind.NestedJsonIndex, 12, DefaultLimit),
        [TopRanking.Vnd] = new(VndSql, KeyKind.SignedColumn, 0, DefaultLimit),
        [TopRanking.Coin] = new(CoinSql, KeyKind.SignedColumn, 0, DefaultLimit),
        [TopRanking.Ruby] = new(DataInventorySql, KeyKind.JsonIndex, 1, DefaultLimit),
        [TopRanking.TotalTopUp] = new(DaNapSql, KeyKind.SignedColumn, 0, DefaultLimit),
        [TopRanking.NamekWar] = new(NamekWarSql, KeyKind.SignedColumn, 0, DefaultLimit),
        [TopRanking.OpenLuckyMoney] = new(NewYearSql, KeyKind.JsonIndexUnquoted, 0, DefaultLimit),
        [TopRanking.GiveLuckyMoney] = new(NewYearSql, KeyKind.JsonIndexUnquoted, 1, DefaultLimit),
        [TopRanking.LaunchFireworks] = new(NewYearSql, KeyKind.JsonIndexUnquoted, 2, DefaultLimit),
        [TopRanking.LaunchFireworksVip] = new(NewYearSql, KeyKind.JsonIndexUnquoted, 3, DefaultLimit),
        [TopRanking.DecorateChristmasTree] = new(ChristmasSql, KeyKind.JsonIndexUnquoted, 0, DefaultLimit),
        [TopRanking.CraftSnowman] = new(ChristmasSql, KeyKind.JsonIndexUnquoted, 1, DefaultLimit),
        [TopRanking.CraftFrozenSnowman] = new(ChristmasSql, KeyKind.JsonIndexUnquoted, 2, DefaultLimit),
        [TopRanking.BurnMatches] = new(ChristmasSql, KeyKind.JsonIndexUnquoted, 3, DefaultLimit),
        [TopRanking.BurnVuLanFirecrackers] = new(VuLanSql, KeyKind.JsonIndexUnquoted, 0, DefaultLimit),
        [TopRanking.FlowerLantern] = new(VuLanSql, KeyKind.JsonIndexUnquoted, 1, DefaultLimit),
        [TopRanking.FlowerLanternWish] = new(VuLanSql, KeyKind.JsonIndexUnquoted, 2, DefaultLimit),
        [TopRanking.OpenGhostGiftBox] = new(HalloweenSql, KeyKind.JsonIndexUnquoted, 0, DefaultLimit),
        [TopRanking.HalloweenCard] = new(HalloweenSql, KeyKind.JsonIndexUnquoted, 1, DefaultLimit),
        [TopRanking.OpenWomensDayCard] = new(Event83Sql, KeyKind.JsonIndexUnquoted, 0, DefaultLimit),
        [TopRanking.GiveRoses] = new(Event83Sql, KeyKind.JsonIndexUnquoted, 1, DefaultLimit),
        [TopRanking.MakeMoonCake] = new(TrungThuSql, KeyKind.JsonIndexUnquoted, 0, DefaultLimit),
        [TopRanking.OpenSpecialMoonCake] = new(TrungThuSql, KeyKind.JsonIndexUnquoted, 1, DefaultLimit),
        [TopRanking.OpenGoldenEgg] = new(HungVuongSql, KeyKind.JsonIndexUnquoted, 0, DefaultLimit),
        [TopRanking.OpenHungKingGiftBox] = new(HungVuongSql, KeyKind.JsonIndexUnquoted, 3, DefaultLimit),
        [TopRanking.OfferCakes] = new(HungVuongSql, KeyKind.JsonIndexUnquoted, 4, DefaultLimit),
        [TopRanking.ExchangeWatermelon] = new(HungVuongSql, KeyKind.JsonIndexUnquoted, 5, DefaultLimit),
        [TopRanking.OpenBlackFridayBox] = new(BlackFridaySql, KeyKind.JsonIndexUnquoted, 0, DefaultLimit),
        [TopRanking.BlackFridayShopping] = new(BlackFridaySql, KeyKind.JsonIndexUnquoted, 1, DefaultLimit),
        [TopRanking.Pvp] = new(PvpSql, KeyKind.UnsignedColumn, 0, DefaultLimit),
        [TopRanking.TopUp] = new(NapSql, KeyKind.UnsignedColumn, 0, 20),
        [TopRanking.TopUpRace] = new(DuaNapSql, KeyKind.SignedColumn, 0, 10),
    };

    private readonly NroDatabase _database;

    // Tạo kho xếp hạng từ truy cập cơ sở dữ liệu.
    public TopRepository(NroDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    private enum KeyKind
    {
        // Giá trị JSON ép sang số không dấu.
        JsonIndex,

        // Giá trị JSON bỏ dấu nháy rồi ép sang số không dấu.
        JsonIndexUnquoted,

        // Giá trị JSON lồng của player.pet, với $[1] là mảng JSON dạng chuỗi.
        NestedJsonIndex,

        // Cột số nguyên thường, sắp xếp có dấu.
        SignedColumn,

        // Cột ép sang số không dấu; số âm quấn và đứng trên mọi số dương.
        UnsignedColumn,
    }

    // Trả bảng xếp hạng một giá trị theo quy tắc sắp xếp tương thích Java.
    public Task<IReadOnlyList<TopEntry>> GetTopAsync(TopRanking ranking, CancellationToken cancellationToken = default)
    {
        if (!Specs.TryGetValue(ranking, out var spec))
        {
            throw new ArgumentOutOfRangeException(nameof(ranking), ranking, "Unknown ranking.");
        }

        return _database.InTransactionAsync<IReadOnlyList<TopEntry>>(
            async transaction =>
            {
                var candidates = new List<Candidate>();
                using (var rows = await transaction.ExecuteQueryAsync(spec.CandidateSql, cancellationToken))
                {
                    while (rows.Next())
                    {
                        var (key, value) = ReadKey(spec, rows.GetObject("k"));
                        candidates.Add(new Candidate(ReadInt(rows.GetObject("pid")), key, value));
                    }
                }

                candidates.Sort(static (x, y) =>
                {
                    var c = CompareDescending(x.Key, y.Key);
                    return c != 0 ? c : x.PlayerId.CompareTo(y.PlayerId);
                });
                var winners = candidates.Take(spec.Limit).ToList();
                var details = await ReadDetailsAsync(transaction, winners.Select(w => w.PlayerId), cancellationToken);
                return winners
                    .Select(w =>
                    {
                        var d = details[w.PlayerId];
                        return new TopEntry(d.PlayerId, d.Name, d.Gender, d.ItemsBody, d.LastTimeLoginGame, w.Value);
                    })
                    .ToList();
            },
            cancellationToken,
            IsolationLevel.RepeatableRead);
    }

    // Trả bảng xếp hạng nhiệm vụ chính.
    public Task<IReadOnlyList<TopTaskEntry>> GetTopNvAsync(CancellationToken cancellationToken = default)
    {
        return _database.InTransactionAsync<IReadOnlyList<TopTaskEntry>>(
            async transaction =>
            {
                var candidates = new List<TaskCandidate>();
                using (var rows = await transaction.ExecuteQueryAsync(TaskSql, cancellationToken))
                {
                    while (rows.Next())
                    {
                        var json = rows.GetObject("k") as string;
                        candidates.Add(new TaskCandidate(
                            ReadInt(rows.GetObject("pid")),
                            MariaDbCompat.JsonIndexToUnsigned(json, 0, unquote: false),
                            MariaDbCompat.JsonIndexToUnsigned(json, 1, unquote: false),
                            MariaDbCompat.JsonIndexToUnsigned(json, 2, unquote: false),
                            rows.GetObject("login") as DateTime?));
                    }
                }

                // Sắp xếp nv, subnv, progress giảm dần, thời điểm đăng nhập tăng dần rồi đến id người chơi.
                candidates.Sort(static (x, y) =>
                {
                    var c = CompareDescending(Widen(x.Task), Widen(y.Task));
                    c = c != 0 ? c : CompareDescending(Widen(x.SubTask), Widen(y.SubTask));
                    c = c != 0 ? c : CompareDescending(Widen(x.Progress), Widen(y.Progress));
                    c = c != 0 ? c : Nullable.Compare(x.Login, y.Login);
                    return c != 0 ? c : x.PlayerId.CompareTo(y.PlayerId);
                });
                var winners = candidates.Take(DefaultLimit).ToList();
                var details = await ReadDetailsAsync(transaction, winners.Select(w => w.PlayerId), cancellationToken);
                return winners
                    .Select(w =>
                    {
                        var d = details[w.PlayerId];
                        return new TopTaskEntry(d.PlayerId, d.Name, d.Gender, d.ItemsBody, d.LastTimeLoginGame, AsValue(w.Task), AsValue(w.SubTask), AsValue(w.Progress));
                    })
                    .ToList();
            },
            cancellationToken,
            IsolationLevel.RepeatableRead);
    }

    // Trả bảng xếp hạng thử thách luyện tập Whis.
    public Task<IReadOnlyList<TopWhisEntry>> GetTopWhisAsync(CancellationToken cancellationToken = default)
    {
        return _database.InTransactionAsync<IReadOnlyList<TopWhisEntry>>(
            async transaction =>
            {
                var candidates = new List<WhisCandidate>();
                using (var rows = await transaction.ExecuteQueryAsync(WhisSql, cancellationToken))
                {
                    while (rows.Next())
                    {
                        var json = rows.GetObject("k") as string;
                        var top = MariaDbCompat.JsonIndexToUnsigned(json, 5, unquote: false);
                        if (top is > 0)
                        {
                            // Điều kiện CAST(... '$[5]' ...) > 0 loại cả NULL lẫn 0.
                            candidates.Add(new WhisCandidate(ReadInt(rows.GetObject("pid")), top.Value, MariaDbCompat.JsonIndexToUnsigned(json, 6, unquote: false)));
                        }
                    }
                }

                // Sắp xếp top giảm dần, time tăng dần; NULL nhỏ nhất nên đứng đầu, sau đó đến id người chơi.
                candidates.Sort(static (x, y) =>
                {
                    var c = y.Top.CompareTo(x.Top);
                    c = c != 0 ? c : Nullable.Compare(x.Time, y.Time);
                    return c != 0 ? c : x.PlayerId.CompareTo(y.PlayerId);
                });
                var winners = candidates.Take(10).ToList();
                var details = await ReadDetailsAsync(transaction, winners.Select(w => w.PlayerId), cancellationToken);
                return winners
                    .Select(w =>
                    {
                        var d = details[w.PlayerId];
                        return new TopWhisEntry(d.PlayerId, d.Name, d.Gender, d.ItemsBody, d.LastTimeLoginGame, AsValue(w.Top), AsValue(w.Time));
                    })
                    .ToList();
            },
            cancellationToken,
            IsolationLevel.RepeatableRead);
    }

    // Trả bảng xếp hạng Võ đài sinh tử.
    public Task<IReadOnlyList<TopVdstEntry>> GetTopVdstAsync(CancellationToken cancellationToken = default)
    {
        return _database.InTransactionAsync<IReadOnlyList<TopVdstEntry>>(
            async transaction =>
            {
                var candidates = new List<VdstCandidate>();
                using (var rows = await transaction.ExecuteQueryAsync(VdstSql, cancellationToken))
                {
                    while (rows.Next())
                    {
                        var json = rows.GetObject("k") as string;
                        var time = MariaDbCompat.JsonIndexToUnsigned(json, 3, unquote: false);
                        if (time is > 0)
                        {
                            candidates.Add(new VdstCandidate(ReadInt(rows.GetObject("pid")), time.Value, MariaDbCompat.JsonIndexToUnsigned(json, 2, unquote: false)));
                        }
                    }
                }

                candidates.Sort(static (x, y) =>
                {
                    var c = y.Time.CompareTo(x.Time);
                    return c != 0 ? c : x.PlayerId.CompareTo(y.PlayerId);
                });
                var winners = candidates.Take(20).ToList();
                var details = await ReadDetailsAsync(transaction, winners.Select(w => w.PlayerId), cancellationToken);
                return winners
                    .Select(w =>
                    {
                        var d = details[w.PlayerId];
                        return new TopVdstEntry(d.PlayerId, d.Name, d.Gender, d.ItemsBody, AsValue(w.Field2), AsValue(w.Time));
                    })
                    .ToList();
            },
            cancellationToken,
            IsolationLevel.RepeatableRead);
    }

    // Trả bảng xếp hạng thỏi vàng trong túi và rương.
    public Task<IReadOnlyList<TopGoldBarEntry>> GetTopThoiVangAsync(CancellationToken cancellationToken = default)
    {
        return _database.InTransactionAsync<IReadOnlyList<TopGoldBarEntry>>(
            async transaction =>
            {
                var candidates = new List<ThoiVangCandidate>();
                using (var rows = await transaction.ExecuteQueryAsync(ThoiVangSql, cancellationToken))
                {
                    while (rows.Next())
                    {
                        var bag = MariaDbCompat.GoldBarQuantity(rows.GetObject("bag") as string ?? string.Empty);
                        var box = MariaDbCompat.GoldBarQuantity(rows.GetObject("box") as string ?? string.Empty);
                        candidates.Add(new ThoiVangCandidate(ReadInt(rows.GetObject("pid")), bag, box));
                    }
                }

                candidates.Sort(static (x, y) =>
                {
                    var c = y.Sum.CompareTo(x.Sum);
                    return c != 0 ? c : x.PlayerId.CompareTo(y.PlayerId);
                });
                var winners = candidates.Take(20).ToList();
                var details = await ReadDetailsAsync(transaction, winners.Select(w => w.PlayerId), cancellationToken);
                return winners
                    .Select(w =>
                    {
                        var d = details[w.PlayerId];
                        var total = w.Sum > ulong.MaxValue ? ulong.MaxValue : (ulong)w.Sum;
                        return new TopGoldBarEntry(d.PlayerId, d.AccountId, d.Name, d.Gender, d.ItemsBody, w.Bag, w.Box, total);
                    })
                    .ToList();
            },
            cancellationToken,
            IsolationLevel.RepeatableRead);
    }

    private static async Task<Dictionary<int, Detail>> ReadDetailsAsync(NroTransaction transaction, IEnumerable<int> ids, CancellationToken cancellationToken)
    {
        var idArray = ids.ToArray();
        var details = new Dictionary<int, Detail>(idArray.Length);
        if (idArray.Length == 0)
        {
            return details;
        }

        using var rows = await transaction.ExecuteQueryAsync(DetailSql, [idArray], cancellationToken);
        while (rows.Next())
        {
            var id = ReadInt(rows.GetObject("pid"));
            details[id] = new Detail(
                id,
                ReadInt(rows.GetObject("aid")),
                rows.GetObject("pname") as string ?? string.Empty,
                ReadInt(rows.GetObject("gender")),
                rows.GetObject("body") as string ?? string.Empty,
                rows.GetObject("login") as DateTime?);
        }

        return details;
    }

    // Đọc khóa sắp xếp; NULL SQL đứng cuối khi giảm dần và giá trị trả về theo getLong của Java.
    private static (Int128? Key, long Value) ReadKey(RankingSpec spec, object? raw)
    {
        switch (spec.Kind)
        {
            case KeyKind.SignedColumn:
                return raw is null ? (null, 0) : (ReadLong(raw), ReadLong(raw));
            case KeyKind.UnsignedColumn:
                if (raw is null)
                {
                    return (null, 0);
                }

                var wrapped = unchecked((ulong)ReadLong(raw));
                return (wrapped, AsValue(wrapped));
            default:
                var json = raw as string;
                var number = spec.Kind switch
                {
                    KeyKind.JsonIndex => MariaDbCompat.JsonIndexToUnsigned(json, spec.Index, unquote: false),
                    KeyKind.JsonIndexUnquoted => MariaDbCompat.JsonIndexToUnsigned(json, spec.Index, unquote: true),
                    _ => MariaDbCompat.NestedJsonIndexToUnsigned(json, 1, spec.Index),
                };
                return (Widen(number), AsValue(number));
        }
    }

    private static Int128? Widen(ulong? value) => value is { } number ? number : null;

    // Đổi cột không dấu theo getLong Java: NULL là 0 và giữ mẫu bit không dấu.
    private static long AsValue(ulong? value) => value is { } number ? unchecked((long)number) : 0;

    private static int CompareDescending(Int128? x, Int128? y)
    {
        if (x is null)
        {
            return y is null ? 0 : 1; // NULL đứng cuối.
        }

        return y is null ? -1 : y.Value.CompareTo(x.Value);
    }

    private static int ReadInt(object? value) => value switch
    {
        int number => number,
        long number => checked((int)number),
        short number => number,
        _ => throw new InvalidCastException("Expected an integer column."),
    };

    private static long ReadLong(object value) => value switch
    {
        int number => number,
        long number => number,
        short number => number,
        _ => throw new InvalidCastException("Expected an integer column."),
    };

    private sealed record RankingSpec(string CandidateSql, KeyKind Kind, int Index, int Limit);

    private sealed record Candidate(int PlayerId, Int128? Key, long Value);

    private sealed record TaskCandidate(int PlayerId, ulong? Task, ulong? SubTask, ulong? Progress, DateTime? Login);

    private sealed record WhisCandidate(int PlayerId, ulong Top, ulong? Time);

    private sealed record VdstCandidate(int PlayerId, ulong Time, ulong? Field2);

    private sealed record ThoiVangCandidate(int PlayerId, ulong Bag, ulong Box)
    {
        // Tổng thỏi vàng dùng UInt128 để tránh tràn trước khi bão hòa khi trả kết quả.
        public UInt128 Sum => (UInt128)Bag + Box;
    }

    private sealed record Detail(int PlayerId, int AccountId, string Name, int Gender, string ItemsBody, DateTime? LastTimeLoginGame);
}
