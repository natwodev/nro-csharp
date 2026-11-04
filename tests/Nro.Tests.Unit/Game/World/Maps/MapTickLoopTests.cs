using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Game;

namespace Nro.Tests.Unit.Game.World.Maps;

public class MapTickLoopTests
{
    private sealed class OrderMob : Mob
    {
        private readonly List<string> _log;
        private readonly string _name;
        private readonly bool _throw;

        public OrderMob(MapRuntime rt, List<string> log, string name, bool shouldThrow = false)
            : base(rt)
        {
            _log = log;
            _name = name;
            _throw = shouldThrow;
        }

        public int Updates { get; private set; }

        public Action? OnUpdate { get; set; }

        public override void Update()
        {
            Updates++;
            _log.Add(_name);
            OnUpdate?.Invoke();
            if (_throw)
            {
                throw new InvalidOperationException("mob failed");
            }
        }
    }

    private static OrderMob AddMob(Zone zone, List<string> log, string name, bool shouldThrow = false)
    {
        OrderMob mob = new(zone.Runtime, log, name, shouldThrow) { Zone = zone };
        zone.Mobs.Add(mob);
        return mob;
    }

    [Fact]
    public void TickOnce_TicksTheZonesInOrder_CommandsBeforeUpdates()
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g, 1, zones: 3);
        List<string> log = [];
        for (int i = 0; i < 3; i++)
        {
            Zone z = map.Zones[i];
            AddMob(z, log, "mob" + i);
            int index = i;
            z.Post(_ => log.Add("cmd" + index));
        }

        MapTickLoop loop = new(map);
        Assert.Equal(0, loop.TickOnce());
        Assert.Equal(["cmd0", "mob0", "cmd1", "mob1", "cmd2", "mob2"], log);
        Assert.Equal(1, loop.Turns);
    }

    [Fact]
    public void TickOnce_CountsAZoneThatRefusesToTick_AndTheOtherZonesStillRun()
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g, 1, zones: 2);
        List<string> log = [];
        AddMob(map.Zones[1], log, "mob1");
        int nestedFailures = -1;
        map.Zones[0].Post(z => nestedFailures = new MapTickLoop(z).TickOnce());
        MapTickLoop loop = new(map);
        Assert.Equal(0, loop.TickOnce());
        Assert.Equal(1, nestedFailures);
        Assert.Equal(["mob1"], log);
    }

    [Fact]
    public void TickOnce_ALoopOverOneZoneTicksOnlyThatZone()
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g, 1, zones: 2);
        List<string> log = [];
        AddMob(map.Zones[0], log, "mob0");
        AddMob(map.Zones[1], log, "mob1");
        new MapTickLoop(map.Zones[1]).TickOnce();
        Assert.Equal(["mob1"], log);
    }

    [Fact]
    public void Update_AFailingPhaseOrMobDoesNotStopTheZone_Over100Turns()
    {
        FakeGame g = new() { TranhNgocFailure = new InvalidOperationException("war service down") };
        Map map = MapTestKit.BuildMap(g, 1, zones: 2);
        List<string> log = [];
        OrderMob good = AddMob(map.Zones[0], log, "good");
        good.Id = 1;
        OrderMob bad = AddMob(map.Zones[0], log, "bad", true);
        bad.Id = 2;
        OrderMob other = AddMob(map.Zones[1], log, "other");
        other.Id = 3;
        Player p = MapTestKit.NewPlayerIn(g, map.Zones[0], 7);
        MapTickLoop loop = new(map);
        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(0, loop.TickOnce());
        }

        Assert.Equal((100, 100, 100), (good.Updates, bad.Updates, other.Updates));
        Assert.Equal(100, g.Calls.Count(c => c == "updatePlayer:7"));
        Assert.Equal(100, loop.Turns);
        Assert.NotNull(p);
    }

    [Fact]
    public async Task ConcurrentAddAndRemoveRequests_During1000Ticks_NeverBreakTheZone()
    {
        FakeGame g = new(seed: 99) { RecordPlayerUpdates = false };
        Zone zone = MapTestKit.BuildZone(g, maxPlayer: 100_000);
        for (int i = 0; i < 3; i++)
        {
            MapTestKit.NewMob(zone, i, 25, 1000);
        }

        MapTickLoop loop = new(zone);
        int producers = 4;
        int perProducer = 250;
        Player[][] mine = new Player[producers][];
        for (int t = 0; t < producers; t++)
        {
            long baseId = t * 100_000L;
            mine[t] = Enumerable.Range(0, perProducer).Select(i => g.NewPlayer(baseId + i + 1, i, 144, zone)).ToArray();
        }

        using CancellationTokenSource stop = new();
        Task[] producerTasks = Enumerable.Range(0, producers).Select(t => Task.Run(() =>
        {
            Player[] players = mine[t];
            for (int i = 0; i < players.Length; i++)
            {
                zone.RequestAddPlayer(players[i]);
                if (i % 3 == 0)
                {
                    zone.RequestRemovePlayer(players[i]);
                }

                if (i % 5 == 0)
                {
                    zone.Post(z => _ = new ItemMap(z, 20, 1, 5, 144, -1));
                }
            }
        })).ToArray();

        // readers on another thread enumerate every list the whole time: no InvalidOperationException may ever come out of them
        Task reader = Task.Run(() =>
        {
            long n = 0;
            while (!stop.IsCancellationRequested)
            {
                foreach (Player p in zone.Players)
                {
                    n += p.Id & 1;
                }

                foreach (Player p in zone.Humanoids)
                {
                    n += p.Id & 1;
                }

                foreach (ItemMap it in zone.Items)
                {
                    n += it.X & 1;
                }

                foreach (Mob m in zone.Mobs)
                {
                    n += m.Id & 1;
                }

                n += zone.GetNumOfPlayers();
            }

            return n;
        });

        // this task is the zone's single writer
        int failedTicks = await Task.Run(() =>
        {
            int failed = 0;
            for (int tick = 0; tick < 1000; tick++)
            {
                failed += loop.TickOnce();
            }

            return failed;
        });

        await Task.WhenAll(producerTasks);
        loop.TickOnce();
        await stop.CancelAsync();
        await reader;

        Assert.Equal(0, failedTicks);
        int removedPerProducer = (perProducer + 2) / 3;
        int itemsPerProducer = (perProducer + 4) / 5;
        Assert.Equal(producers * (perProducer - removedPerProducer), zone.Players.Count);
        Assert.Equal(zone.Players.Count, zone.Humanoids.Count);
        Assert.Equal(zone.Players.Count, zone.NotBosses.Count);
        Assert.Equal(producers * itemsPerProducer, zone.Items.Count);
        Assert.Equal(0, zone.PendingCommandCount);
        Assert.Equal(1001, loop.Turns);
    }

    [Fact]
    public async Task RunAsync_TicksUntilCancelled_AndEndsWithoutException()
    {
        FakeGame g = new();
        MapRuntime runtime = new(g, g, g, g, g, g, new SeededRandomProvider(1), TimeProvider.System, true, g.DataRoot);
        Map map = new(runtime, 7, "real", 0, 0, 0, 0, 0, MapTestKit.FlatTiles(), [2], 1, 5, [], [], 0);
        List<string> log = [];
        OrderMob mob = AddMob(map.Zones[0], log, "real");
        MapTickLoop loop = new(map, TimeSpan.FromMilliseconds(5), TimeSpan.FromMilliseconds(1));
        using CancellationTokenSource cts = new();
        Task run = loop.RunAsync(cts.Token);
        await WaitUntil(() => mob.Updates >= 5);
        await cts.CancelAsync();
        await run;
        Assert.True(run.IsCompletedSuccessfully);
        int after = mob.Updates;
        await Task.Delay(40);
        Assert.Equal(after, mob.Updates);
    }

    [Fact]
    public async Task RunAsync_AlreadyCancelled_DoesNotTick()
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g);
        List<string> log = [];
        OrderMob mob = AddMob(map.Zones[0], log, "m");
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();
        await new MapTickLoop(map).RunAsync(cts.Token);
        Assert.Equal(0, mob.Updates);
    }

    [Fact]
    public async Task RunAsync_WaitsThePeriodMinusTheTimeTheTurnTook_ButNeverLessThanTheMinimum()
    {
        FakeGame g = new();
        TimerTimeProvider time = new(DateTimeOffset.FromUnixTimeMilliseconds(1_700_000_000_000));
        MapRuntime runtime = new(g, g, g, g, g, g, new SeededRandomProvider(1), time, true, g.DataRoot);
        Map map = new(runtime, 7, "fake", 0, 0, 0, 0, 0, MapTestKit.FlatTiles(), [2], 1, 5, [], [], 0);
        List<string> log = [];
        OrderMob mob = AddMob(map.Zones[0], log, "m");
        TimeSpan turnTakes = TimeSpan.Zero;
        mob.OnUpdate = () => time.Advance(turnTakes);
        MapTickLoop loop = new(map);
        using CancellationTokenSource cts = new();

        // the first turn runs inside the call and takes no time: the next one is due after the whole period
        Task run = loop.RunAsync(cts.Token);
        await WaitUntil(() => mob.Updates == 1 && time.PendingDelays.Count == 1);
        Assert.Equal([TimeSpan.FromMilliseconds(1000)], time.PendingDelays);

        // a turn that takes 300 ms is followed by a 700 ms wait
        turnTakes = TimeSpan.FromMilliseconds(300);
        time.Advance(TimeSpan.FromMilliseconds(1000));
        await WaitUntil(() => mob.Updates == 2 && time.PendingDelays.Count == 1);
        Assert.Equal([TimeSpan.FromMilliseconds(700)], time.PendingDelays);

        // a turn longer than the period is followed by the minimum wait of 10 ms
        turnTakes = TimeSpan.FromMilliseconds(2000);
        time.Advance(TimeSpan.FromMilliseconds(700));
        await WaitUntil(() => mob.Updates == 3 && time.PendingDelays.Count == 1);
        Assert.Equal([TimeSpan.FromMilliseconds(10)], time.PendingDelays);

        // the wait is cut short by cancellation
        await cts.CancelAsync();
        await run;
        Assert.Equal(3, mob.Updates);
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        for (int i = 0; i < 600 && !condition(); i++)
        {
            await Task.Delay(5);
        }

        Assert.True(condition());
    }

    [Fact]
    public void Constructor_RejectsBadPeriods_AndDefaultsMatchJava()
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g);
        Assert.Throws<ArgumentOutOfRangeException>(() => new MapTickLoop(map, TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => new MapTickLoop(map, TimeSpan.FromSeconds(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new MapTickLoop(map, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(-1)));
        MapTickLoop loop = new(map);
        Assert.Equal(TimeSpan.FromMilliseconds(1000), loop.Period);
        Assert.Equal(TimeSpan.FromMilliseconds(10), loop.MinimumDelay);
    }
}
