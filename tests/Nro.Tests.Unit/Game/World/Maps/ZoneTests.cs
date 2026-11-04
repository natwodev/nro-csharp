using Nro.Core.Consts;
using Nro.Core.IO;
using Nro.Game;

namespace Nro.Tests.Unit.Game.World.Maps;

public class ZoneTests
{
    private sealed class ProbeMob : Mob
    {
        private readonly List<string> _log;
        private readonly bool _throw;

        public ProbeMob(MapRuntime rt, List<string> log, bool shouldThrow)
            : base(rt)
        {
            _log = log;
            _throw = shouldThrow;
        }

        public override void Update()
        {
            _log.Add("mob" + Id);
            if (_throw)
            {
                throw new InvalidOperationException("boom");
            }
        }
    }

    // ---- membership ------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void AddPlayer_PutsEachKindInTheRightLists()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player normal = g.NewPlayer(1);
        Player boss = g.NewPlayer(2);
        boss.IsBoss = true;
        Player disciple = g.NewPlayer(3);
        disciple.IsPet = true;
        Player follow = g.NewPlayer(4);
        follow.IsPetFollow = true;
        Player npc = g.NewPlayer(5);
        g.State(npc).NonInteractiveNpc = true;
        foreach (Player p in new[] { normal, boss, disciple, follow, npc })
        {
            z.AddPlayer(p);
        }

        Assert.Equal(5, z.Humanoids.Count);
        Assert.Equal([normal, disciple], z.NotBosses.ToArray());
        Assert.Equal([normal], z.Players.ToArray());
        Assert.Equal([boss], z.Bosses.ToArray());
        Assert.Equal([npc], z.NonInteractiveNPCs.ToArray());
        Assert.Equal(1, z.GetNumOfPlayers());
        Assert.Equal(1, z.GetNumOfBoss());
    }

    [Fact]
    public void AddPlayer_Null_IsIgnored_AndTwiceDoesNotDuplicate()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        z.AddPlayer(null);
        Player p = g.NewPlayer(1);
        z.AddPlayer(p);
        z.AddPlayer(p);
        Assert.Single(z.Humanoids);
        Assert.Single(z.NotBosses);
        Assert.Single(z.Players);
    }

    [Fact]
    public void RemovePlayer_RemovesFromEveryList_AndUnknownPlayerIsHarmless()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player p = g.NewPlayer(1);
        Player boss = g.NewPlayer(2);
        boss.IsBoss = true;
        z.AddPlayer(p);
        z.AddPlayer(boss);
        z.RemovePlayer(p);
        z.RemovePlayer(boss);
        z.RemovePlayer(g.NewPlayer(3));
        Assert.Empty(z.Humanoids);
        Assert.Empty(z.NotBosses);
        Assert.Empty(z.Players);
        Assert.Empty(z.Bosses);
    }

    [Fact]
    public void IsFullPlayer_Boundary()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g, maxPlayer: 2);
        Assert.False(z.IsFullPlayer());
        z.AddPlayer(g.NewPlayer(1));
        Assert.False(z.IsFullPlayer());
        z.AddPlayer(g.NewPlayer(2));
        Assert.True(z.IsFullPlayer());
    }

    [Fact]
    public void BossLookups()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player boss = g.NewPlayer(-61);
        boss.IsBoss = true;
        Assert.True(z.IsBossCanJoin(boss));
        z.AddPlayer(boss);
        Assert.False(z.IsBossCanJoin(boss));
        Assert.True(z.IsTrongTaiDeoCoTrongKhu());
        Player referee = g.NewPlayer(-1000000, zone: z);
        z.AddPlayer(referee);
        Assert.False(z.IsTrongTaiDeoCoTrongKhu());
        Assert.Same(referee, z.FindPlayerByID(-1000000));
        Assert.Same(boss, z.GetPlayerInMap(-61));
        Assert.Null(z.GetPlayerInMap(77));
        Assert.Null(z.GetPlayerInMapOffline(referee, -61));
    }

    [Fact]
    public void GetNewMobId_ReturnsTheSmallestFreeId()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Assert.Equal(0, z.GetNewMobId());
        MapTestKit.NewMob(z, 0);
        MapTestKit.NewMob(z, 1);
        MapTestKit.NewMob(z, 3);
        Assert.Equal(2, z.GetNewMobId());
    }

    [Fact]
    public void GetRandomPlayerInMap_SkipsInvisibleAndMabuHeld_AndIsDeterministicForASeed()
    {
        FakeGame g = new(seed: 5);
        Zone z = MapTestKit.BuildZone(g);
        Assert.Null(z.GetRandomPlayerInMap());
        Player hidden = MapTestKit.NewPlayerIn(g, z, 1);
        g.State(hidden).ViewTransform = v => v with { IsInvisible = true };
        Player held = MapTestKit.NewPlayerIn(g, z, 2);
        held.IsMabuHold = true;
        Assert.Null(z.GetRandomPlayerInMap());
        Player ok = MapTestKit.NewPlayerIn(g, z, 3);
        Assert.Same(ok, z.GetRandomPlayerInMap());
    }

    // ---- items on the ground ----------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void AddItem_PutsTheNewestFirst_IgnoresNullAndDuplicates()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        ItemMap first = new(z, 10, 1, 5, 5, -1);
        ItemMap second = new(z, 11, 1, 6, 6, -1);
        z.AddItem(first);
        z.AddItem(null);
        Assert.Equal([second, first], z.Items.ToArray());
        Assert.Same(second, z.GetItemMapByItemMapId(second.ItemMapId));
        Assert.Same(first, z.GetItemMapByTempId(10));
        Assert.Null(z.GetItemMapByTempId(999));
        z.RemoveItemMap(first);
        Assert.Single(z.Items);
    }

    [Fact]
    public void GetItemMapsForPlayer_HidesTaskItemsUntilTheTaskIsReached()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player p = g.NewPlayer(1);
        _ = new ItemMap(z, 78, 1, 0, 0, -1);
        _ = new ItemMap(z, 74, 1, 0, 0, -1);
        _ = new ItemMap(z, 20, 1, 0, 0, -1);
        g.TaskIds[p] = ConstTask.TASK_3_0 - 1;
        Assert.Equal([20], z.GetItemMapsForPlayer(p).Select(i => (int)i.ItemTemplate!.Id));
        g.TaskIds[p] = ConstTask.TASK_3_0;
        Assert.Equal([20, 74], z.GetItemMapsForPlayer(p).Select(i => (int)i.ItemTemplate!.Id).OrderBy(i => i));
        g.TaskIds[p] = ConstTask.TASK_3_1;
        Assert.Equal([20, 74, 78], z.GetItemMapsForPlayer(p).Select(i => (int)i.ItemTemplate!.Id).OrderBy(i => i));
    }

    [Fact]
    public void FindItemMapByPlayer_MatchesOwnerAndTemplate()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player p = MapTestKit.NewPlayerIn(g, z, 7);
        _ = new ItemMap(z, 20, 1, 0, 0, 7);
        Assert.True(z.FindItemMapByPlayer(p, 20));
        Assert.False(z.FindItemMapByPlayer(p, 21));
    }

    [Fact]
    public void PickItem_OwnerPicks_ItemLeavesTheGround_AndTaskChecksRun()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player p = MapTestKit.NewPlayerIn(g, z, 5);
        ItemMap item = new(z, 20, 1, 0, 0, 5);
        z.PickItem(p, item.ItemMapId);
        Assert.Empty(z.Items);
        Assert.Contains($"tryPick:5:{item.ItemMapId}", g.Calls);
        Assert.Contains("onPicked:5", g.Calls);
    }

    [Fact]
    public void PickItem_AnybodyCanPickAnUnownedItem_ButNotAnotherPlayersItem()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player p = MapTestKit.NewPlayerIn(g, z, 5);
        ItemMap free = new(z, 20, 1, 0, 0, -1);
        ItemMap others = new(z, 21, 1, 0, 0, 6);
        z.PickItem(p, others.ItemMapId);
        Assert.Contains("thongbao:5:Không thể nhặt vật phẩm của người khác", g.Calls);
        Assert.DoesNotContain(g.Calls, c => c.StartsWith("tryPick", StringComparison.Ordinal));
        Assert.Contains(others, z.Items);
        z.PickItem(p, free.ItemMapId);
        Assert.DoesNotContain(free, z.Items);
    }

    [Fact]
    public void PickItem_OwnerIdEncodedAbove100Million_IsDecodedAsJavaDid()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player p = MapTestKit.NewPlayerIn(g, z, 10);
        ItemMap item = new(z, 20, 1, 0, 0, 999_999_990L);
        z.PickItem(p, item.ItemMapId);
        Assert.DoesNotContain(item, z.Items);
    }

    [Theory]
    [InlineData(ItemPickupResult.Refused, false, false)]
    [InlineData(ItemPickupResult.BagFull, true, false)]
    public void PickItem_RefusedOrBagFull_LeavesTheItem(ItemPickupResult result, bool expectFullBagNotice, bool expectOnPicked)
    {
        FakeGame g = new() { PickResult = result };
        Zone z = MapTestKit.BuildZone(g);
        Player p = MapTestKit.NewPlayerIn(g, z, 5);
        ItemMap item = new(z, 20, 1, 0, 0, -1);
        z.PickItem(p, item.ItemMapId);
        Assert.Contains(item, z.Items);
        Assert.Equal(expectFullBagNotice, g.Calls.Contains("thongbao:5:Hành trang không còn chỗ trống, không thể nhặt thêm"));
        Assert.Equal(expectOnPicked, g.Calls.Contains("onPicked:5"));
    }

    [Fact]
    public void PickItem_BagFullForABall_NoNoticeButTaskChecksStillRun()
    {
        FakeGame g = new() { PickResult = ItemPickupResult.BagFull };
        g.SetMapSet("NamecBall", 353);
        Zone z = MapTestKit.BuildZone(g);
        Player p = MapTestKit.NewPlayerIn(g, z, 5);
        ItemMap ball = new(z, 353, 1, 0, 0, -1);
        z.PickItem(p, ball.ItemMapId);
        Assert.DoesNotContain(g.Calls, c => c.StartsWith("thongbao", StringComparison.Ordinal));
        Assert.Contains("onPicked:5", g.Calls);
    }

    [Fact]
    public void PickItem_SatelliteMissingAndPickedUpItems_AreRefusedSilentlyOrWithNotice()
    {
        FakeGame g = new();
        g.ItemTypes[900] = 22;
        Zone z = MapTestKit.BuildZone(g);
        Player p = MapTestKit.NewPlayerIn(g, z, 5);
        ItemMap satellite = new(z, 900, 1, 0, 0, -1);
        z.PickItem(p, satellite.ItemMapId);
        Assert.Empty(g.Calls);
        z.PickItem(p, 424242);
        Assert.Contains("thongbao:5:Không thể thực hiện", g.Calls);
        ItemMap done = new(z, 20, 1, 0, 0, -1) { IsPickedUp = true };
        g.Calls.Clear();
        z.PickItem(p, done.ItemMapId);
        Assert.Contains("thongbao:5:Không thể thực hiện", g.Calls);
    }

    [Fact]
    public void PickItem_TaskItemsOnTheirMapsStayOnTheGround()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g, mapId: 21);
        Player p = MapTestKit.NewPlayerIn(g, z, 5);
        ItemMap eaten = z.GetItemMapByTempId(74)!;
        z.PickItem(p, eaten.ItemMapId);
        Assert.Contains(eaten, z.Items);
        Assert.Contains("onPicked:5", g.Calls);
    }

    [Fact]
    public void PickItem_NamekWarBallGoesToTheWarService()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player p = MapTestKit.NewPlayerIn(g, z, 5);
        ItemMap ball = new(z, 353, 1, 0, 0, -1) { IsContestedNamekBall = true };
        z.PickItem(p, ball.ItemMapId);
        Assert.Contains("namekWarPick:5", g.Calls);
        Assert.Contains(ball, z.Items);
    }

    // ---- actor behaviour -------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Commands_RunInOrder_AtTheNextTick_AndACommandThatThrowsDoesNotStopTheOthers()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        List<int> ran = [];
        z.Post(_ => ran.Add(1));
        z.Post(_ => throw new InvalidOperationException("bad command"));
        z.Post(zone => zone.Post(_ => ran.Add(3)));
        z.Post(_ => ran.Add(2));
        Assert.Empty(ran);
        Assert.Equal(4, z.PendingCommandCount);
        z.Tick();
        Assert.Equal([1, 2], ran);
        Assert.Equal(1, z.PendingCommandCount);
        z.Tick();
        Assert.Equal([1, 2, 3], ran);
    }

    [Fact]
    public void RequestAddAndRemove_TakeEffectOnTheTick()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player p = g.NewPlayer(1);
        z.RequestAddPlayer(p);
        Assert.Empty(z.Players);
        z.Tick();
        Assert.Single(z.Players);
        z.RequestRemovePlayer(p);
        z.Tick();
        Assert.Empty(z.Players);
        ItemMap item = new(z, 20, 1, 0, 0, -1);
        z.RemoveItemMap(item);
        z.RequestAddItem(item);
        z.Tick();
        Assert.Single(z.Items);
    }

    [Fact]
    public void Tick_IsRefusedWhileAnotherTickIsRunning()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Exception? inner = null;
        z.Post(zone =>
        {
            try
            {
                zone.Tick();
            }
            catch (InvalidOperationException e)
            {
                inner = e;
            }
        });
        z.Tick();
        Assert.NotNull(inner);
        Assert.Contains("already being ticked", inner!.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Update_OrderIsMobsItemsNpcsThenPlayers()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        List<string> order = [];
        ProbeMob mob = new(g.Runtime, order, false) { Id = 1, Zone = z };
        z.Mobs.Add(mob);
        _ = new ItemMap(z, 20, 1, 0, 0, -1);
        g.Time.Advance(TimeSpan.FromMilliseconds(50_001));
        Player npc = g.NewPlayer(8, zone: z);
        g.State(npc).NonInteractiveNpc = true;
        z.AddPlayer(npc);
        Player pl = MapTestKit.NewPlayerIn(g, z, 9);
        z.Update();
        List<string> events = [.. order, .. g.Calls.Where(c => c.StartsWith("removeItem", StringComparison.Ordinal) || c.StartsWith("updatePlayer", StringComparison.Ordinal))];
        Assert.Equal(["mob1", "removeItem:20", "updatePlayer:8", "updatePlayer:9"], events);
        Assert.NotNull(pl);
    }

    [Fact]
    public void Update_AFailingMobDoesNotStopTheOtherMobsOrTheRestOfTheZone()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        List<string> order = [];
        z.Mobs.Add(new ProbeMob(g.Runtime, order, false) { Id = 1, Zone = z });
        z.Mobs.Add(new ProbeMob(g.Runtime, order, true) { Id = 2, Zone = z });
        z.Mobs.Add(new ProbeMob(g.Runtime, order, false) { Id = 3, Zone = z });
        MapTestKit.NewPlayerIn(g, z, 9);
        z.Update();
        Assert.Equal(["mob3", "mob2", "mob1"], order);
        Assert.Contains("updatePlayer:9", g.Calls);
    }

    [Fact]
    public void Update_UpdatesOnlyRealPlayers()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player a = MapTestKit.NewPlayerIn(g, z, 1);
        Player b = MapTestKit.NewPlayerIn(g, z, 2);
        z.AddPlayer(a);
        g.State(a).IsPl = true;
        g.State(b).IsPl = false;
        z.Update();
        Assert.Contains("updatePlayer:1", g.Calls);
        Assert.DoesNotContain("updatePlayer:2", g.Calls);
    }

    [Fact]
    public void Update_RemovesAnItemThatLostItsTemplate()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        ItemMap broken = new(z, 20, 1, 0, 0, -1);
        broken.Dispose();
        z.Items.Insert(0, broken);
        z.Update();
        Assert.DoesNotContain(broken, z.Items);
    }

    [Fact]
    public void Update_RemovingThePlayersWhileTheyAreUpdated_NeverThrows()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        for (int i = 0; i < 20; i++)
        {
            MapTestKit.NewPlayerIn(g, z, i);
        }

        // a player removes everybody (also itself) from the zone while the zone loops over them
        z.Post(zone =>
        {
            foreach (Player p in zone.Players)
            {
                zone.RemovePlayer(p);
            }
        });
        z.Tick();
        Assert.Empty(z.Players);
    }

    // ---- Namek war -------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void NamekWar_WarEndedByTheServer_ResetsTheZone()
    {
        FakeGame g = new() { NamekWar = false };
        Zone z = MapTestKit.BuildZone(g);
        z.IsNamekWarZoneStarted = true;
        z.PointCadic = 3;
        z.PointFide = 2;
        z.AddPlayersCadic(g.NewPlayer(1));
        z.Update();
        Assert.False(z.IsNamekWarZoneStarted);
        Assert.Equal(0, z.PointCadic);
        Assert.Equal(0, z.PointFide);
        Assert.Empty(z.PlayersCadic);
    }

    [Fact]
    public void NamekWar_SevenPointsWinImmediately_AndTheTimeoutPicksTheHigherScore()
    {
        FakeGame g = new() { NamekWar = true };
        Zone z = MapTestKit.BuildZone(g);
        z.IsNamekWarZoneStarted = true;
        z.LastNamekWarStartTime = g.Runtime.Now;
        z.LastTimeDropBall = g.Runtime.Now;
        z.PointFide = 7;
        z.PointCadic = 1;
        z.AddPlayersCadic(g.NewPlayer(1));
        z.AddplayersFide(g.NewPlayer(2));
        z.Update();
        Assert.False(z.IsNamekWarZoneStarted);
        Assert.Equal(["namekWarEnd:1:True", "namekWarEnd:0:False", "namekWarPrize:1:1:7", "namekWarPrize:0:1:1"], g.Calls.Where(c => c.StartsWith("namekWar", StringComparison.Ordinal)).ToArray());

        g.Calls.Clear();
        z.IsNamekWarZoneStarted = true;
        z.LastNamekWarStartTime = g.Runtime.Now;
        z.PointCadic = 4;
        z.PointFide = 2;
        g.Time.Advance(TimeSpan.FromMilliseconds(ConstNamekWar.TIME + 1));
        z.Update();
        Assert.Contains("namekWarEnd:1:False", g.Calls);
        Assert.Contains("namekWarEnd:0:True", g.Calls);
        Assert.Equal(0, z.PointCadic);

        g.Calls.Clear();
        z.IsNamekWarZoneStarted = true;
        z.LastNamekWarStartTime = g.Runtime.Now;
        z.PointCadic = 2;
        z.PointFide = 2;
        g.Time.Advance(TimeSpan.FromMilliseconds(ConstNamekWar.TIME + 1));
        z.Update();
        Assert.Equal(["namekWarEnd:2:True", "namekWarEnd:2:False"], g.Calls.Where(c => c.StartsWith("namekWar", StringComparison.Ordinal)).ToArray());
    }

    [Fact]
    public void NamekWar_DropsABallAfterTheDelay_WhenNobodyHasIt()
    {
        FakeGame g = new() { NamekWar = true };
        Zone z = MapTestKit.BuildZone(g);
        z.IsNamekWarZoneStarted = true;
        z.LastNamekWarStartTime = g.Runtime.Now;
        z.LastTimeDropBall = g.Runtime.Now;
        z.Update();
        Assert.Empty(z.Items);
        g.Time.Advance(TimeSpan.FromMilliseconds(ConstNamekWar.LAST_TIME_DROP_BALL + 1));
        z.Update();
        ItemMap ball = Assert.Single(z.Items);
        Assert.True(ball.IsContestedNamekBall);
        Assert.InRange((int)ball.ItemTemplate!.Id, 353, 359);
        Assert.Equal(g.Runtime.Now, z.LastTimeDropBall);
    }

    [Fact]
    public void NamekWar_PlayersAreNotAddedTwice()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player p = g.NewPlayer(1);
        z.AddPlayersCadic(p);
        z.AddPlayersCadic(p);
        z.AddplayersFide(p);
        z.AddplayersFide(p);
        Assert.Single(z.PlayersCadic);
        Assert.Single(z.PlayersFide);
        z.RemoveplayersCadic(p);
        z.RemoveplayersFide(p);
        z.RemoveplayersFide(p);
        Assert.Empty(z.PlayersCadic);
        Assert.Empty(z.PlayersFide);
    }

    // ---- traps, big boss info, packets -----------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void IsInTrap_UsesAnInclusiveRectangle()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g, mapId: 135);
        TrapMap trap = Assert.Single(z.TrapMaps);
        Assert.Equal((260, 960, 740, 72, 49), (trap.X, trap.Y, trap.W, trap.H, trap.EffectId));
        Player inside = g.NewPlayer(1, 260, 960);
        Player edge = g.NewPlayer(2, 1000, 1032);
        Player outside = g.NewPlayer(3, 1001, 1032);
        Assert.Same(trap, z.IsInTrap(inside));
        Assert.Same(trap, z.IsInTrap(edge));
        Assert.Null(z.IsInTrap(outside));
    }

    [Fact]
    public void SendBigBoss_ReportsTheStagesOfALivingHirudegarn()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player p = g.NewPlayer(1);
        Mob hiru = MapTestKit.NewMob(z, 0, ConstMob.HIRUDEGARN);
        hiru.LvMob = 2;
        z.SendBigBoss(p);
        Assert.Equal(["bigboss2p:1:6", "bigboss2p:1:5"], g.Calls);
        g.Calls.Clear();
        hiru.Point.Hp = 0;
        z.SendBigBoss(p);
        Assert.Empty(g.Calls);
    }

    [Fact]
    public void LoadPlayers_FollowsTheMapKind()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player a = MapTestKit.NewPlayerIn(g, z, 1);
        Player b = MapTestKit.NewPlayerIn(g, z, 2);
        Player c = MapTestKit.NewPlayerIn(g, z, 3);
        z.LoadMeToAnother(a);
        Assert.Equal(["info:2<-1", "info:3<-1"], g.Calls.OrderBy(x => x, StringComparer.Ordinal));
        g.Calls.Clear();
        z.LoadAnotherToMe(a);
        Assert.Equal(["info:1<-2", "info:1<-3"], g.Calls.OrderBy(x => x, StringComparer.Ordinal));
        Assert.NotNull(b);
        Assert.NotNull(c);
    }

    [Fact]
    public void LoadPlayers_ClanMapOnlyReachesTheClanMates()
    {
        FakeGame g = new();
        g.SetMapSet("BangHoi", 1);
        Zone z = MapTestKit.BuildZone(g);
        Player a = MapTestKit.NewPlayerIn(g, z, 1);
        Player b = MapTestKit.NewPlayerIn(g, z, 2);
        b.Name = a.Name;
        MapTestKit.NewPlayerIn(g, z, 3);
        z.LoadMeToAnother(a);
        Assert.Equal(["info:2<-1"], g.Calls);
    }

    [Fact]
    public void LoadBoss_ExchangesInfoWithTheOtherBosses()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player b1 = g.NewPlayer(-1);
        b1.IsBoss = true;
        Player b2 = g.NewPlayer(-2);
        b2.IsBoss = true;
        z.AddPlayer(b1);
        z.AddPlayer(b2);
        z.LoadBoss(b1);
        Assert.Equal(["info:-1<--2", "info:-2<--1"], g.Calls);
    }

    [Fact]
    public void MapInfo_WritesTheDocumentedLayout()
    {
        FakeGame g = new();
        WayPoint wp = new() { MinX = 1, MinY = 2, MaxX = 3, MaxY = 4, IsEnter = true, IsOffline = false, Name = "Door" };
        Map map = MapTestKit.BuildMap(g, mapId: 148, wayPoints: [wp]);
        Zone z = map.Zones[0];
        Mob mob = MapTestKit.NewMob(z, 4, 25, 5000, 70, 80);
        mob.Level = 7;
        _ = new ItemMap(z, 20, 1, 11, 12, 77);
        Player p = g.NewPlayer(1, 33, 44, z);
        z.MapInfo(p);

        SentPacket packet = Assert.Single(g.PacketsOf(-24));
        Assert.Equal("player:1", packet.Target);
        Message m = new(-24, packet.Data);
        Assert.Equal(148, m.ReadUnsignedByte());
        Assert.Equal(1, m.ReadByte());
        Assert.Equal(2, m.ReadByte());
        Assert.Equal(3, m.ReadByte());
        Assert.Equal(0, m.ReadByte());
        Assert.Equal("Map148", m.ReadUTF());
        Assert.Equal(0, m.ReadByte());
        Assert.Equal(33, m.ReadShort());
        Assert.Equal(44, m.ReadShort());
        Assert.Equal(1, m.ReadByte());
        Assert.Equal((1, 2, 3, 4), (m.ReadShort(), m.ReadShort(), m.ReadShort(), m.ReadShort()));
        Assert.True(m.ReadBoolean());
        Assert.False(m.ReadBoolean());
        Assert.Equal("Door", m.ReadUTF());
        Assert.Equal(1, m.ReadByte());
        for (int i = 0; i < 5; i++)
        {
            Assert.False(m.ReadBoolean());
        }

        Assert.Equal(25, m.ReadByte());
        Assert.Equal(0, m.ReadByte());
        Assert.Equal(5000, m.ReadInt());
        Assert.Equal(7, m.ReadByte());
        Assert.Equal(5000, m.ReadInt());
        Assert.Equal(70, m.ReadShort());
        Assert.Equal(80, m.ReadShort());
        Assert.Equal(5, m.ReadByte());
        Assert.Equal(0, m.ReadByte());
        Assert.False(m.ReadBoolean());
        Assert.Equal(0, m.ReadByte());
        Assert.Equal(1, m.ReadByte());
        Assert.Equal((1, 100, 200, 5, 7), ((int)m.ReadByte(), (int)m.ReadShort(), (int)m.ReadShort(), (int)m.ReadByte(), (int)m.ReadShort()));
        Assert.Equal(1, m.ReadByte());
        Assert.Equal(0, m.ReadShort());
        Assert.Equal(20, m.ReadShort());
        Assert.Equal(11, m.ReadShort());
        Assert.Equal(12, m.ReadShort());
        Assert.Equal(77, m.ReadInt());
        Assert.Equal(0, m.ReadShort());
        Assert.Equal(0, m.ReadShort());
        Assert.Equal(4, m.ReadByte());
        Assert.Equal(3, m.ReadByte());
        Assert.Equal(1, m.ReadByte());
    }

    [Fact]
    public void MapInfo_AppendsTheBackgroundAndEffectFilesWhenTheyExist_AndHidesDeadBigBosses()
    {
        string root = Path.Combine(Path.GetTempPath(), "nro-mapinfo-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "map", "item_bg_map_data"));
            Directory.CreateDirectory(Path.Combine(root, "map", "eff_map"));
            Directory.CreateDirectory(Path.Combine(root, "map", "eff_map_christmas"));
            File.WriteAllBytes(Path.Combine(root, "map", "item_bg_map_data", "1"), [0xAA, 0xBB]);
            File.WriteAllBytes(Path.Combine(root, "map", "eff_map", "1"), [0xCC]);
            File.WriteAllBytes(Path.Combine(root, "map", "eff_map_christmas", "1"), [0xDD, 0xEE]);
            FakeGame g = new(dataRoot: root);
            Zone z = MapTestKit.BuildZone(g);
            Mob deadBoss = MapTestKit.NewMob(z, 0, ConstMob.PIANO);
            deadBoss.Point.Hp = 0;
            Player p = g.NewPlayer(1, 0, 0, z);
            z.MapInfo(p);
            byte[] normal = Assert.Single(g.PacketsOf(-24)).Data;
            string hex = Convert.ToHexString(normal);
            Assert.Contains("AABBCC", hex, StringComparison.Ordinal);
            g.Packets.Clear();
            g.Christmas = true;
            z.MapInfo(p);
            Assert.Contains("AABBDDEE", Convert.ToHexString(Assert.Single(g.PacketsOf(-24)).Data), StringComparison.Ordinal);

            // the dead PIANO (a big boss other than template 70) is not listed: the mob count byte follows the way point count (0)
            Message m = new(-24, normal);
            m.ReadByte();
            m.ReadByte();
            m.ReadByte();
            m.ReadByte();
            m.ReadByte();
            m.ReadUTF();
            m.ReadByte();
            m.ReadShort();
            m.ReadShort();
            Assert.Equal(0, m.ReadByte());
            Assert.Equal(0, m.ReadByte());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
