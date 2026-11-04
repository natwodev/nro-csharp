using Nro.Game;

namespace Nro.Tests.Unit.Game.World.Maps;

public class ItemMapTests
{
    [Fact]
    public void Constructors_AssignIncreasingIds_AndRegisterTheItemInItsZone()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        ItemMap a = new(z, 20, 3, 10, 20, -1);
        ItemMap b = new(z, 21, 1, [new ItemMapOption(30, 0)], 1, 2, 9);
        Assert.Equal((0, 1), (a.ItemMapId, b.ItemMapId));
        Assert.Equal([b, a], z.Items.ToArray());
        Assert.Equal(3, a.Quantity);
        Assert.Equal(g.Runtime.Now, a.CreateTime);
        Assert.Equal(-1, a.PlayerId);
        Assert.Equal(-1, a.ClanId);
        Assert.Equal(-1, a.TypeHaveBallTranhDoat);
        Assert.Single(b.Options);
        Assert.Empty(a.Options);
    }

    [Fact]
    public void Constructor_WithATemplate_KeepsThatTemplate()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Nro.Core.Templates.ItemTemplate template = g.GetItemTemplate(77)!;
        ItemMap it = new(z, template, 1, 0, 0, 5);
        Assert.Same(template, it.ItemTemplate);
    }

    [Fact]
    public void Constructor_UnknownTemplate_Throws()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Assert.Throws<InvalidOperationException>(() => new ItemMap(z, -5, 1, 0, 0, 5));
    }

    [Fact]
    public void ItemIdCounter_WrapsToZeroAtTwoBillion()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        z.CountItemAppeaerd = 1_999_999_999;
        ItemMap last = new(z, 20, 1, 0, 0, -1);
        Assert.Equal(1_999_999_999, last.ItemMapId);
        Assert.Equal(0, z.CountItemAppeaerd);
        Assert.Equal(0, new ItemMap(z, 20, 1, 0, 0, -1).ItemMapId);
    }

    [Theory]
    [InlineData(-1L, -1L)]
    [InlineData(5L, 5L)]
    [InlineData(-5L, 5L)]
    [InlineData(long.MinValue, long.MinValue)]
    public void OwnerId_IsStoredAsItsAbsoluteValue_ExceptMinusOne(long given, long stored)
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Assert.Equal(stored, new ItemMap(z, 20, 1, 0, 0, given).PlayerId);
    }

    [Fact]
    public void CopyConstructor_ReusesIdAndOptions_ResetsTheCreationTime_AndJoinsTheZone()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        ItemMap original = new(z, 20, 4, [new ItemMapOption(1, 2)], 7, 8, 3);
        g.Time.Advance(TimeSpan.FromSeconds(10));
        ItemMap copy = new(original);
        Assert.Equal(original.ItemMapId, copy.ItemMapId);
        Assert.Same(original.Options, copy.Options);
        Assert.Equal((4, 7, 8, 3L), (copy.Quantity, copy.X, copy.Y, copy.PlayerId));
        Assert.Equal(original.CreateTime + 10_000, copy.CreateTime);
        Assert.Contains(copy, z.Items);
    }

    [Fact]
    public void Options_AddGetSetSub()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        ItemMap it = new(z, 20, 1, 0, 0, -1);
        Assert.Equal(0, it.GetOptionParam(74));
        it.AddOptionParam(74, 5);
        it.AddOptionParam(74, 3);
        Assert.Equal(8, it.GetOptionParam(74));
        it.SubOptionParam(74, 2);
        Assert.Equal(6, it.GetOptionParam(74));
        it.SetOptionParam(74, 40);
        Assert.Equal(40, it.GetOptionParam(74));
        it.SetOptionParam(999, 1);
        it.SubOptionParam(999, 1);
        Assert.Single(it.Options);
    }

    [Fact]
    public void Update_OwnerProtectionEndsAfter45Seconds_AndTheItemIsRemovedAfter50()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        ItemMap it = new(z, 20, 1, 0, 0, 7);
        g.Time.Advance(TimeSpan.FromMilliseconds(45_000));
        it.Update();
        Assert.Equal(7, it.PlayerId);
        g.Time.Advance(TimeSpan.FromMilliseconds(1));
        it.Update();
        Assert.Equal(-1, it.PlayerId);
        Assert.Empty(g.Calls);
        g.Time.Advance(TimeSpan.FromMilliseconds(4_999));
        it.Update();
        Assert.Empty(g.Calls);
        g.Time.Advance(TimeSpan.FromMilliseconds(1));
        it.Update();
        Assert.Equal(["removeItem:20"], g.Calls);
        Assert.Empty(z.Items);
    }

    [Fact]
    public void Update_Exceptions_NamecBallsTaskMapsAndSpecialItemsLiveLonger()
    {
        FakeGame g = new();
        g.SetMapSet("NamecBall", 355);
        g.SetMapSet("Barracks", 90);
        Zone z = MapTestKit.BuildZone(g, 1);
        ItemMap namec = new(z, 355, 1, 0, 0, -1);
        ItemMap id78 = new(z, 78, 1, 0, 0, -1);
        ItemMap id726 = new(z, 726, 1, 0, 0, 4);
        g.Time.Advance(TimeSpan.FromMilliseconds(100_000));
        namec.Update();
        id78.Update();
        id726.Update();
        Assert.Equal(4, id726.PlayerId);
        Assert.DoesNotContain(g.Calls, c => c.StartsWith("removeItem:355", StringComparison.Ordinal));
        Assert.DoesNotContain("removeItem:78", g.Calls);
        g.Time.Advance(TimeSpan.FromMilliseconds(80_001));
        namec.Update();
        id78.Update();
        Assert.DoesNotContain(g.Calls, c => c.StartsWith("removeItem:355", StringComparison.Ordinal));
        Assert.DoesNotContain("removeItem:78", g.Calls);

        FakeGame g2 = new();
        Zone task = MapTestKit.BuildZone(g2, 22);
        ItemMap kept = new(task, 20, 1, 0, 0, -1);
        g2.Time.Advance(TimeSpan.FromMilliseconds(500_000));
        kept.Update();
        Assert.DoesNotContain("removeItem:20", g2.Calls);

        FakeGame g3 = new();
        g3.SetMapSet("Barracks", 90);
        Zone dt = MapTestKit.BuildZone(g3, 90);
        ItemMap ball = new(dt, 15, 1, 0, 0, -1);
        g3.Time.Advance(TimeSpan.FromMilliseconds(60_000));
        ball.Update();
        Assert.DoesNotContain("removeItem:15", g3.Calls);
        g3.Time.Advance(TimeSpan.FromMilliseconds(130_000));
        ball.Update();
        Assert.DoesNotContain("removeItem:15", g3.Calls);
    }

    [Fact]
    public void Update_Item726OfThe1vs1Map_IsRemovedWhenItsOwnerLeft()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g, 52);
        MapTestKit.NewPlayerIn(g, z, 4);
        ItemMap stays = new(z, 726, 1, 0, 0, 4);
        ItemMap goes = new(z, 726, 1, 0, 0, 5);
        stays.Update();
        goes.Update();
        Assert.Equal(["removeItem:726"], g.Calls);
        Assert.Contains(stays, z.Items);
        Assert.DoesNotContain(goes, z.Items);
    }

    [Fact]
    public void Update_Item460OfThe123456789Owner_IsRemovedAfter5Seconds()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        ItemMap it = new(z, 460, 1, 0, 0, 123456789);
        g.Time.Advance(TimeSpan.FromMilliseconds(5_000));
        it.Update();
        Assert.Empty(g.Calls);
        g.Time.Advance(TimeSpan.FromMilliseconds(1));
        it.Update();
        Assert.Equal(["removeItem:460"], g.Calls);
    }

    [Fact]
    public void Update_BlackBall_FollowsTheFirstPlayerEveryTenSeconds_AndNeverExpires()
    {
        FakeGame g = new();
        g.SetMapSet("BlackBall", 16);
        Zone z = MapTestKit.BuildZone(g);
        Player p = MapTestKit.NewPlayerIn(g, z, 1, 200, 100);
        ItemMap ball = new(z, 16, 1, 0, 0, -1);
        Assert.True(ball.IsBlackBall);
        ball.Update();
        Assert.Equal(0, ball.X);
        g.Time.Advance(TimeSpan.FromMilliseconds(10_001));
        ball.Update();
        Assert.Equal((200, 144), (ball.X, ball.Y));
        Assert.Equal(["itemDisappear:16", "drop:16x1@200,144"], g.Calls);
        g.Calls.Clear();
        g.Time.Advance(TimeSpan.FromMilliseconds(1_000_000));
        g.State(p).X = 90;
        ball.Update();
        Assert.Equal(90, ball.X);
        Assert.DoesNotContain("removeItem:16", g.Calls);
    }

    [Fact]
    public void Update_BlackBall_WithNoPlayerOrAPlayerInAnotherZone_DoesNothing()
    {
        FakeGame g = new();
        g.SetMapSet("BlackBall", 16);
        Zone z = MapTestKit.BuildZone(g);
        ItemMap ball = new(z, 16, 1, 0, 0, -1);
        g.Time.Advance(TimeSpan.FromMilliseconds(20_000));
        ball.Update();
        Assert.Empty(g.Calls);
        Player stranger = g.NewPlayer(1, 50, 50, MapTestKit.BuildZone(g, 2));
        z.Players.Add(stranger);
        ball.Update();
        Assert.Empty(g.Calls);
    }

    [Fact]
    public void Update_Satellite_BuffsOnlyNearbyLivingOwnersWithinTheRange()
    {
        FakeGame g = new();
        g.ItemTypes[343] = 22;
        Zone z = MapTestKit.BuildZone(g);
        Player owner = MapTestKit.NewPlayerIn(g, z, 1, 100, 100);
        Player far = MapTestKit.NewPlayerIn(g, z, 2, 900, 100);
        Player other = MapTestKit.NewPlayerIn(g, z, 3, 100, 100);
        ItemMap sat = new(z, 343, 1, 100, 100, 1);
        sat.Update();
        Assert.Equal(["satellite:1:343"], g.Calls);
        g.Calls.Clear();
        g.State(owner).IsDie = true;
        sat.Update();
        Assert.Empty(g.Calls);
        Assert.NotNull(far);
        Assert.NotNull(other);
    }

    [Fact]
    public void Dispose_DetachesZoneAndTemplate()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        ItemMap it = new(z, 20, 1, 0, 0, -1);
        it.Dispose();
        Assert.Null(it.Zone);
        Assert.False(it.IsNotNullItem());
        it.Update();
        Assert.Empty(g.Calls);
    }
}
