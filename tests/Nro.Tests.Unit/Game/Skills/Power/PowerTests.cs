using Nro.Game;
using Nro.Tests.Unit.Game.Support;

namespace Nro.Tests.Unit.Game.Skills.Power;

public class CaptionTests
{
    private static readonly Caption Sample = new(3, "earth text", "saiya text", "namek text", 1000);

    [Theory]
    [InlineData(0, "earth text")]
    [InlineData(1, "namek text")]
    [InlineData(2, "saiya text")]
    [InlineData(3, "earth text")]
    [InlineData(-1, "earth text")]
    [InlineData(100, "earth text")]
    public void GetCaption_picks_the_text_of_the_planet(int planet, string expected)
    {
        Assert.Equal(expected, Sample.GetCaption(planet));
    }

    [Fact]
    public void Properties_are_settable_like_the_lombok_setters()
    {
        var caption = new Caption(1, "a", "b", "c", 5);

        caption.Id = 9;
        caption.Power = long.MaxValue;
        caption.Earth = "x";

        Assert.Equal(9, caption.Id);
        Assert.Equal(long.MaxValue, caption.Power);
        Assert.Equal("x", caption.GetCaption(0));
    }
}

public class CaptionManagerTests
{
    private static readonly string[] Columns = ["id", "earth", "saiya", "namek", "power"];

    private static CaptionManager WithPowers(params long[] powers)
    {
        var manager = new CaptionManager();
        for (int i = 0; i < powers.Length; i++)
        {
            manager.Add(new Caption(i + 10, "e" + i, "s" + i, "n" + i, powers[i]));
        }

        return manager;
    }

    [Fact]
    public void A_new_manager_is_empty_and_level_zero()
    {
        var manager = new CaptionManager();

        Assert.Empty(manager.Captions);
        Assert.Equal(0, manager.GetLevel(123456));
        Assert.Null(manager.FindLevel(0));
        Assert.Null(manager.Find(1));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(999, 0)]
    [InlineData(1000, 1)]
    [InlineData(4999, 1)]
    [InlineData(5000, 2)]
    [InlineData(long.MaxValue, 3)]
    public void GetLevel_is_the_highest_caption_the_power_reaches(long power, int expected)
    {
        CaptionManager manager = WithPowers(0, 1000, 5000, 20000);

        Assert.Equal(expected, manager.GetLevel(power));
    }

    [Fact]
    public void GetLevel_scans_from_the_end_so_an_unsorted_table_gives_the_last_position_that_fits()
    {
        CaptionManager manager = WithPowers(500, 100, 900, 50);

        Assert.Equal(3, manager.GetLevel(60));   // position 3 (power 50) fits and is scanned first
        Assert.Equal(3, manager.GetLevel(1000)); // same: the last position that fits
        Assert.Equal(0, manager.GetLevel(10));   // nothing fits: level 0
    }

    [Fact]
    public void Find_and_FindLevel_use_the_id_and_the_position()
    {
        CaptionManager manager = WithPowers(0, 100, 200);

        Assert.Equal(11, manager.Find(11)!.Id);
        Assert.Null(manager.Find(1));
        Assert.Equal(12, manager.FindLevel(2)!.Id);
        Assert.Null(manager.FindLevel(3));
        Assert.Null(manager.FindLevel(-1));
    }

    [Fact]
    public void Remove_takes_out_that_object_only()
    {
        CaptionManager manager = WithPowers(0, 100);
        Caption second = manager.Captions[1];
        var lookAlike = new Caption(second.Id, second.Earth, second.Saiya, second.Namek, second.Power);

        Assert.False(manager.Remove(lookAlike));
        Assert.True(manager.Remove(second));
        Assert.False(manager.Remove(second));
        Assert.Single(manager.Captions);
    }

    [Fact]
    public void A_list_already_returned_does_not_change_when_the_manager_changes()
    {
        CaptionManager manager = WithPowers(0, 100);
        IReadOnlyList<Caption> before = manager.Captions;

        manager.Add(new Caption(99, "", "", "", 5));

        Assert.Equal(2, before.Count);
        Assert.Equal(3, manager.Captions.Count);
    }

    [Fact]
    public async Task LoadAsync_reads_the_rows_in_id_order_and_appends()
    {
        var db = new FakeSqlExecutor().Table("from caption", Columns,
            new object?[] { 1, "e1", "s1", "n1", 0L },
            new object?[] { 2, "e2", "s2", "n2", 1500L });
        CaptionManager manager = WithPowers(7);

        await manager.LoadAsync(db);

        Assert.Equal(3, manager.Captions.Count);
        Assert.Equal(["e0", "e1", "e2"], manager.Captions.Select(c => c.Earth));
        Assert.Equal(1500L, manager.Captions[2].Power);
        Assert.Contains("order by id", db.QueryLog.Single());
    }

    [Fact]
    public async Task LoadAsync_failure_changes_nothing_and_is_reported()
    {
        var db = new FakeSqlExecutor();
        db.FailQueryContaining = ("from caption", new InvalidOperationException("db down"));
        CaptionManager manager = WithPowers(7);

        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.LoadAsync(db));

        Assert.Single(manager.Captions);
    }

    [Fact]
    public async Task Concurrent_readers_and_writers_never_see_a_broken_list()
    {
        var manager = new CaptionManager();
        var writers = Enumerable.Range(0, 4).Select(w => Task.Run(() =>
        {
            for (int i = 0; i < 500; i++)
            {
                var caption = new Caption(i, "e", "s", "n", i);
                manager.Add(caption);
                manager.Remove(caption);
            }
        }));
        var readers = Enumerable.Range(0, 4).Select(r => Task.Run(() =>
        {
            for (int i = 0; i < 2000; i++)
            {
                _ = manager.GetLevel(i);
                foreach (Caption c in manager.Captions)
                {
                    _ = c.Power;
                }
            }
        }));

        await Task.WhenAll(writers.Concat(readers));

        Assert.Empty(manager.Captions);
    }
}

public class PowerLimitManagerTests
{
    private static readonly string[] Columns = ["id", "power", "hp", "mp", "damage", "defense", "critical"];

    [Fact]
    public void Get_is_by_position_and_null_outside_the_list()
    {
        var manager = new PowerLimitManager();
        manager.Add(new PowerLimit(7, 100, 1, 2, 3, 4, 5));
        manager.Add(new PowerLimit(3, 200, 1, 2, 3, 4, 5));

        Assert.Equal(7, manager.Get(0)!.Id);
        Assert.Equal(3, manager.Get(1)!.Id);
        Assert.Null(manager.Get(2));
        Assert.Null(manager.Get(-1));
    }

    [Fact]
    public async Task LoadAsync_reads_every_column_with_the_java_types()
    {
        var db = new FakeSqlExecutor().Table("from power_limit", Columns,
            new object?[] { 1, 17_000_000_000L, 2_000_000_000, 2_100_000_000, 123, 45, 6 });
        var manager = new PowerLimitManager();

        await manager.LoadAsync(db);

        PowerLimit limit = manager.Get(0)!;
        Assert.Equal(1, limit.Id);
        Assert.Equal(17_000_000_000L, limit.Power);
        Assert.Equal(2_000_000_000L, limit.Hp);
        Assert.Equal(2_100_000_000L, limit.Mp);
        Assert.Equal(123L, limit.Damage);
        Assert.Equal(45, limit.Defense);
        Assert.Equal(6, limit.Critical);
        Assert.Contains("order by id", db.QueryLog.Single());
    }

    [Fact]
    public async Task LoadAsync_failure_changes_nothing()
    {
        var db = new FakeSqlExecutor();
        db.FailQueryContaining = ("power_limit", new InvalidOperationException("db down"));
        var manager = new PowerLimitManager();
        manager.Add(new PowerLimit(1, 1, 1, 1, 1, 1, 1));

        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.LoadAsync(db));

        Assert.Single(manager.Powers);
    }

    [Fact]
    public void Remove_takes_out_that_object_only()
    {
        var manager = new PowerLimitManager();
        var limit = new PowerLimit(1, 1, 1, 1, 1, 1, 1);
        manager.Add(limit);

        Assert.False(manager.Remove(new PowerLimit(1, 1, 1, 1, 1, 1, 1)));
        Assert.True(manager.Remove(limit));
        Assert.Empty(manager.Powers);
    }

    [Fact]
    public async Task Null_arguments_are_rejected()
    {
        var manager = new PowerLimitManager();
        Assert.Throws<ArgumentNullException>(() => manager.Add(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => manager.LoadAsync(null!));
    }
}
