using System.Reflection;
using Nro.Core.Consts;
using Nro.Core.IO;
using Nro.Game;
using Nro.Tests.Unit.Helpers;
using Nro.Tests.Unit.Game.Items.Core;

namespace Nro.Tests.Unit.Game.Items.Time;

public class ItemTimeServiceTests
{
    private sealed class Rig
    {
        public Rig()
        {
            Clock = ItemTestKit.Time();
            Host = new RecordingHost();
            Player = ItemTestKit.NewPlayer();
            Player.ItemTime = new ItemTime(Player, ItemTestKit.Clock(Clock), Host);
            Service = new ItemTimeService(Host, ItemTestKit.Clock(Clock));
        }

        public ManualTimeProvider Clock { get; }

        public RecordingHost Host { get; }

        public Player Player { get; }

        public ItemTimeService Service { get; }

        public long Now => Clock.GetUtcNow().ToUnixTimeMilliseconds();

        public ItemTime Time => Player.ItemTime;
    }

    private static PropertyInfo Prop(string javaName) => typeof(ItemTime).GetProperty(ItemTimeSpec.CSharpName(javaName))!;

    private static int DurationOf(string constantName) => Convert.ToInt32(typeof(ItemTime).GetField(ItemTimeSpec.CSharpConstantName(constantName))!.GetRawConstantValue());

    private static short ReadShort(byte[] data, int offset) => (short)((data[offset] << 8) | data[offset + 1]);

    public static IEnumerable<object[]> SendAllRows() => ItemTimeSpec.SendAll.Select(r => new object[] { r.Flag, r.Last, r.Duration, r.Icon });

    public static IEnumerable<object[]> ClearRows() => ItemTimeSpec.Clear.Select(r => new object[] { r.Flag, r.Icon });

    // ----- wire format -------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Item_time_message_is_minus_106_with_two_shorts()
    {
        var rig = new Rig();

        rig.Service.SendItemTime(rig.Player, 4547, 30);

        (_, sbyte command, byte[] data) = Assert.Single(rig.Host.Messages);
        Assert.Equal(-106, command);
        Assert.Equal(new byte[] { 0x11, 0xC3, 0x00, 0x1E }, data);
    }

    [Fact]
    public void Item_time_values_wrap_to_16_bits_like_write_short()
    {
        var rig = new Rig();

        rig.Service.SendItemTime(rig.Player, 70000, -100);

        Assert.Equal(new byte[] { 0x11, 0x70, 0xFF, 0x9C }, rig.Host.Messages.Single().Data);
    }

    [Fact]
    public void Removing_an_item_time_sends_zero_seconds()
    {
        var rig = new Rig();

        rig.Service.RemoveItemTime(rig.Player, 2755);

        Assert.Equal(new byte[] { 0x0A, 0xC3, 0x00, 0x00 }, rig.Host.Messages.Single().Data);
    }

    [Fact]
    public void Text_time_message_is_65_with_id_utf_text_and_seconds()
    {
        var rig = new Rig();

        rig.Service.SendTextTime(rig.Player, 3, "ab", 258);

        (_, sbyte command, byte[] data) = Assert.Single(rig.Host.Messages);
        Assert.Equal(65, command);
        Assert.Equal(new byte[] { 3, 0, 2, (byte)'a', (byte)'b', 1, 2 }, data);
    }

    [Fact]
    public void Text_time_keeps_vietnamese_text_in_modified_utf8()
    {
        var rig = new Rig();

        rig.Service.SendTextXinbato(rig.Player);

        byte[] data = rig.Host.Messages.Single().Data;
        var reader = new Message(65, data);
        Assert.Equal(1, reader.ReadByte());
        Assert.Equal("Tìm nước cho Xinbatô ở đảo Kame hoặc đảo Guru", reader.ReadUTF());
        Assert.Equal(30, reader.ReadShort());
    }

    [Theory]
    [InlineData(0, ItemTime.BARRACKS)]
    [InlineData(1, ItemTime.TREASURE_UNDER_SEA)]
    [InlineData(2, ItemTime.DESTRON_GAS)]
    [InlineData(3, ItemTime.SNAKE_WAY)]
    [InlineData(4, ItemTime.CLAN_BOSS_MAP)]
    public void Removing_a_text_sends_an_empty_text_with_zero_seconds(int which, sbyte id)
    {
        var rig = new Rig();
        Action<Player>[] removers =
        [
            rig.Service.RemoveTextBarracks,
            rig.Service.RemoveTextTreasureUnderSea,
            rig.Service.RemoveTextDestronGas,
            rig.Service.RemoveTextSnakeWay,
            rig.Service.RemoveTextClanBossMap,
        ];

        removers[which](rig.Player);

        Assert.Equal(new byte[] { (byte)id, 0, 0, 0, 0 }, rig.Host.Messages.Single().Data);
    }

    [Theory]
    [InlineData(true, (byte)1)]
    [InlineData(false, (byte)0)]
    public void Auto_play_flag_message_is_minus_116(bool on, byte expected)
    {
        var rig = new Rig();
        rig.Time.IsAutoPlayEnabled = on;

        rig.Service.SendCanAutoPlay(rig.Player);

        (_, sbyte command, byte[] data) = Assert.Single(rig.Host.Messages);
        Assert.Equal(-116, command);
        Assert.Equal(new[] { expected }, data);
    }

    [Fact]
    public void Constructor_rejects_a_null_host()
    {
        Assert.Throws<ArgumentNullException>(() => new ItemTimeService(null!));
    }

    [Fact]
    public void A_host_that_throws_on_send_is_logged_not_propagated()
    {
        var rig = new Rig();
        var failing = new ThrowingHost();
        var service = new ItemTimeService(failing, ItemTestKit.Clock(rig.Clock));

        service.SendItemTime(rig.Player, 1, 1);
        service.SendTextTime(rig.Player, 0, "x", 1);
        service.SendCanAutoPlay(rig.Player);

        Assert.Equal(3, failing.Attempts);
    }

    private sealed class ThrowingHost : IItemHost
    {
        public int Attempts { get; private set; }

        public void SendMessage(Player player, Message message)
        {
            Attempts++;
            throw new InvalidOperationException("send failed");
        }

        public void SendPoint(Player player) => throw new InvalidOperationException("point failed");

        public void SendNotification(Player player, string text)
        {
        }

        public void SendNotificationFromAdmin(Player player, string text)
        {
        }

        public void SendItemBag(Player player)
        {
        }

        public int GetCountEmptyBag(Player player) => 0;

        public void AddItemBag(Player player, Item item)
        {
        }

        public void SubQuantityItemsBag(Player player, Item? item, int quantity)
        {
        }

        public Item? FindBagItem(Player player, int templateId) => null;

        public IReadOnlyList<Item> GetCombineItems(Player player) => [];

        public void RemoveMonkeyTail(Player player)
        {
        }

        public void IncreasePowerLimit(Player player)
        {
        }

        public ExternalItemTimes GetExternalItemTimes(Player player) => ExternalItemTimes.None;
    }

    // ----- sendAllItemTime ---------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Nothing_is_sent_when_no_effect_is_running()
    {
        var rig = new Rig();

        rig.Service.SendAllItemTime(rig.Player);

        Assert.Empty(rig.Host.Messages);
    }

    [Theory]
    [MemberData(nameof(SendAllRows))]
    public void Each_running_effect_sends_its_icon_and_the_remaining_seconds(string flag, string last, string durationName, int icon)
    {
        var rig = new Rig();
        Prop(flag).SetValue(rig.Time, true);
        Prop(last).SetValue(rig.Time, rig.Now - 100_000);

        rig.Service.SendAllItemTime(rig.Player);

        (_, sbyte command, byte[] data) = Assert.Single(rig.Host.Messages);
        Assert.Equal(-106, command);
        Assert.Equal((short)icon, ReadShort(data, 0));
        Assert.Equal((short)((DurationOf(durationName) - 100_000) / 1000), ReadShort(data, 2));
    }

    [Fact]
    public void Effects_are_reported_in_java_order()
    {
        var rig = new Rig();
        rig.Time.IsUseBloodTonic = true;
        rig.Time.LastTimeBloodTonic = rig.Now;
        rig.Time.IsUseMonkeyTailPowerPotential = true;
        rig.Time.LastTimeUseMonkeyTailPowerPotential = rig.Now;
        rig.Time.IsUseScanner = true;
        rig.Time.LastTimeUseScanner = rig.Now;

        rig.Service.SendAllItemTime(rig.Player);

        Assert.Equal(new short[] { 5072, 2755, 2758 }, rig.Host.Messages.Select(m => ReadShort(m.Data, 0)).ToArray());
    }

    [Fact]
    public void An_effect_past_its_end_but_not_yet_switched_off_reports_a_negative_time_as_a_wrapped_short()
    {
        var rig = new Rig();
        rig.Time.IsUseBloodTonic = true;
        rig.Time.LastTimeBloodTonic = rig.Now - 700_000;

        rig.Service.SendAllItemTime(rig.Player);

        Assert.Equal(-100, ReadShort(rig.Host.Messages.Single().Data, 2));
    }

    [Theory]
    [InlineData(0, 3790)]
    [InlineData(2, 3790)]
    [InlineData(1, 3901)]
    public void Fusion_timer_uses_the_icon_of_the_race(int gender, int icon)
    {
        var rig = new Rig();
        rig.Player.Gender = (sbyte)gender;
        rig.Host.External = new ExternalItemTimes { FusionType = ConstPlayer.FUSION_DANCE, FusionLastTime = rig.Now - 100_000, FusionDurationMs = 600_000 };

        rig.Service.SendAllItemTime(rig.Player);

        byte[] data = rig.Host.Messages.Single().Data;
        Assert.Equal((short)icon, ReadShort(data, 0));
        Assert.Equal(500, ReadShort(data, 2));
    }

    [Fact]
    public void Other_fusion_types_send_nothing()
    {
        var rig = new Rig();
        rig.Host.External = new ExternalItemTimes { FusionType = 3, FusionLastTime = rig.Now, FusionDurationMs = 600_000 };

        rig.Service.SendAllItemTime(rig.Player);

        Assert.Empty(rig.Host.Messages);
    }

    [Fact]
    public void Effect_skill_timers_are_sent_while_their_start_time_is_not_negative()
    {
        var rig = new Rig();
        rig.Host.External = new ExternalItemTimes
        {
            IsCumber = true,
            LastTimeCumber = rig.Now - 60_000,
            IsCumber2 = true,
            LastTimeCumber2 = -1,
            IsPain = true,
            LastTimeUpPain = 0,
            IsKefla = true,
            LastTimeKefla = rig.Now - 100_000,
        };

        rig.Service.SendAllItemTime(rig.Player);

        List<(short Icon, short Seconds)> sent = rig.Host.Messages.Select(m => (ReadShort(m.Data, 0), ReadShort(m.Data, 2))).ToList();
        Assert.Equal(3, sent.Count);
        Assert.Equal(30907, sent[0].Icon);
        Assert.Equal(120, sent[0].Seconds);
        Assert.Equal(25265, sent[1].Icon);
        Assert.Equal(13769, sent[2].Icon);
        Assert.Equal(200, sent[2].Seconds);
    }

    [Fact]
    public void The_meal_uses_its_own_icon_and_auto_play_sends_its_length()
    {
        var rig = new Rig();
        rig.Time.IsEatMeal = true;
        rig.Time.IconMeal = 4242;
        rig.Time.LastTimeEatMeal = rig.Now - 100_000;
        rig.Time.IsAutoPlayEnabled = true;
        rig.Time.AutoPlayDurationMs = 180_000;

        rig.Service.SendAllItemTime(rig.Player);

        List<byte[]> data = rig.Host.Messages.Select(m => m.Data).ToList();
        Assert.Equal(2, data.Count);
        Assert.Equal(4242, ReadShort(data[0], 0));
        Assert.Equal(500, ReadShort(data[0], 2));
        Assert.Equal(4387, ReadShort(data[1], 0));
        Assert.Equal(180, ReadShort(data[1], 2));
    }

    [Fact]
    public void Both_hongdao_style_flags_that_share_an_icon_send_one_message_each()
    {
        var rig = new Rig();
        rig.Time.IsUseKamejoko = true;
        rig.Time.LastTimeUseKamejoko = rig.Now;
        rig.Time.IsUseKamejoko2 = true;
        rig.Time.LastTimeUseKamejoko2 = rig.Now;

        rig.Service.SendAllItemTime(rig.Player);

        Assert.Equal(2, rig.Host.Messages.Count);
        Assert.All(rig.Host.Messages, m => Assert.Equal(30867, ReadShort(m.Data, 0)));
    }

    // ----- dungeon texts --------------------------------------------------------------------------------------------------------------------------------

    private static ClanDungeonTimes Clan(long now, long openAgoMs) => new()
    {
        LastTimeOpenBarracks = now - openAgoMs,
        BarracksDurationMs = 1_800_000,
        LastTimeOpenTreasureUnderSea = now - openAgoMs,
        TreasureUnderSeaDurationMs = 1_800_000,
        LastTimeOpenSnakeWay = now - openAgoMs,
        SnakeWayDurationMs = 1_800_000,
        LastTimeOpenDestronGas = now - openAgoMs,
        DestronGasDurationMs = 1_800_000,
        LastTimeOpenBossOfTheGangs = now - openAgoMs,
        BossOfTheGangsDurationMs = 1_800_000,
        BossClanRound = 4,
    };

    private static (string Text, int Seconds, sbyte Id) ReadText(byte[] data)
    {
        var m = new Message(65, data);
        sbyte id = m.ReadByte();
        string text = m.ReadUTF();
        short seconds = m.ReadShort();
        return (text, seconds, id);
    }

    [Fact]
    public void Dungeon_texts_are_sent_in_java_order_with_the_seconds_left()
    {
        var rig = new Rig();
        rig.Host.External = new ExternalItemTimes { Clan = Clan(rig.Now, 100_000) };

        rig.Service.SendAllItemTime(rig.Player);

        List<(string Text, int Seconds, sbyte Id)> texts = rig.Host.Messages.Select(m => ReadText(m.Data)).ToList();
        Assert.Equal(
            new[] { (ItemTime.TREASURE_UNDER_SEA, "Hang kho báu:"), (ItemTime.BARRACKS, "Trại độc nhãn:"), (ItemTime.SNAKE_WAY, "Con đường rắn độc:"), (ItemTime.DESTRON_GAS, "Khí gas hủy diệt:") },
            texts.Select(t => (t.Id, t.Text)).ToArray());
        Assert.All(texts, t => Assert.Equal(1700, t.Seconds));
    }

    [Theory]
    [InlineData(0, 1800)]
    [InlineData(1000, 1799)]
    [InlineData(1_800_000, 0)]
    public void The_countdown_is_sent_at_the_edges_of_its_valid_range(long openedAgoMs, int expectedSeconds)
    {
        var rig = new Rig();
        rig.Host.External = new ExternalItemTimes { Clan = Clan(rig.Now, openedAgoMs) };

        rig.Service.SendTextBarracks(rig.Player);

        Assert.Equal(expectedSeconds, ReadText(rig.Host.Messages.Single().Data).Seconds);
    }

    [Theory]
    [InlineData(1_801_000)]
    [InlineData(-2_000)]
    public void The_countdown_is_skipped_when_over_or_more_than_1800_seconds_remain(long openedAgoMs)
    {
        var rig = new Rig();
        rig.Host.External = new ExternalItemTimes { Clan = Clan(rig.Now, openedAgoMs) };

        rig.Service.SendTextBarracks(rig.Player);
        rig.Service.SendTextTreasureUnderSea(rig.Player);
        rig.Service.SendTextSnakeWay(rig.Player);
        rig.Service.SendTextDestronGas(rig.Player);
        rig.Service.SendTextClanBossMap(rig.Player);

        Assert.Empty(rig.Host.Messages);
    }

    [Fact]
    public void A_countdown_that_was_never_opened_is_not_sent()
    {
        var rig = new Rig();
        rig.Host.External = new ExternalItemTimes { Clan = new ClanDungeonTimes() };

        rig.Service.SendTextBarracks(rig.Player);
        rig.Service.SendTextTreasureUnderSea(rig.Player);
        rig.Service.SendTextSnakeWay(rig.Player);
        rig.Service.SendTextDestronGas(rig.Player);
        rig.Service.SendTextClanBossMap(rig.Player);
        rig.Service.SendTextTimePickBarracks(rig.Player);

        Assert.Empty(rig.Host.Messages);
    }

    [Fact]
    public void A_player_without_a_clan_gets_no_dungeon_text()
    {
        var rig = new Rig();

        rig.Service.SendTextBarracks(rig.Player);
        rig.Service.SendTextTreasureUnderSea(rig.Player);
        rig.Service.SendTextSnakeWay(rig.Player);
        rig.Service.SendTextDestronGas(rig.Player);
        rig.Service.SendTextClanBossMap(rig.Player);
        rig.Service.SendTextTimePickBarracks(rig.Player);

        Assert.Empty(rig.Host.Messages);
    }

    [Fact]
    public void Doanh_trai_text_is_skipped_once_the_clan_has_gone_in()
    {
        var rig = new Rig();
        ClanDungeonTimes clan = Clan(rig.Now, 1000);
        rig.Host.External = new ExternalItemTimes { Clan = new ClanDungeonTimes { HaveGoneBarracks = true, LastTimeOpenBarracks = clan.LastTimeOpenBarracks, BarracksDurationMs = 1_800_000 } };

        rig.Service.SendTextBarracks(rig.Player);

        Assert.Empty(rig.Host.Messages);
    }

    [Fact]
    public void Boss_round_text_names_the_round_before_the_current_one()
    {
        var rig = new Rig();
        rig.Host.External = new ExternalItemTimes { Clan = Clan(rig.Now, 100_000) };

        rig.Service.SendTextClanBossMap(rig.Player);

        (string text, int seconds, sbyte id) = ReadText(rig.Host.Messages.Single().Data);
        Assert.Equal(ItemTime.CLAN_BOSS_MAP, id);
        Assert.Equal("Hạ Boss Bang Hội [lần thứ 3] thời gian: ", text);
        Assert.Equal(1700, seconds);
    }

    [Fact]
    public void Pick_countdown_needs_a_doanh_trai_that_is_picking()
    {
        var rig = new Rig();
        rig.Host.External = new ExternalItemTimes
        {
            Clan = new ClanDungeonTimes { HasBarracks = true, BarracksIsTimePicking = true, BarracksLastTimePick = rig.Now - 20_000, BarracksPickDurationMs = 60_000 },
        };

        rig.Service.SendTextTimePickBarracks(rig.Player);

        (string text, int seconds, sbyte id) = ReadText(rig.Host.Messages.Single().Data);
        Assert.Equal(ItemTime.BARRACKS, id);
        Assert.Equal("Trại độc nhãn:", text);
        Assert.Equal(40, seconds);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Pick_countdown_is_silent_without_a_doanh_trai_or_when_not_picking(bool hasDoanhTrai, bool picking)
    {
        var rig = new Rig();
        rig.Host.External = new ExternalItemTimes { Clan = new ClanDungeonTimes { HasBarracks = hasDoanhTrai, BarracksIsTimePicking = picking, BarracksPickDurationMs = 60_000 } };

        rig.Service.SendTextTimePickBarracks(rig.Player);

        Assert.Empty(rig.Host.Messages);
    }

    // ----- clearAllItemTime ---------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Clearing_sends_82_removals_then_the_stats_and_switches_the_flags_off()
    {
        var rig = new Rig();
        foreach ((string flag, _) in ItemTimeSpec.Clear)
        {
            Prop(flag).SetValue(rig.Time, true);
        }

        rig.Service.ClearAllItemTime(rig.Player);

        Assert.Equal(82, rig.Host.Messages.Count);
        Assert.All(rig.Host.Messages, m => Assert.Equal(0, ReadShort(m.Data, 2)));
        Assert.Equal("point", rig.Host.Calls.Last());
        Assert.Equal(1, rig.Host.Calls.Count(c => c == "point"));
        foreach ((string flag, _) in ItemTimeSpec.Clear)
        {
            Assert.False((bool)Prop(flag).GetValue(rig.Time)!, flag);
        }
    }

    [Fact]
    public void Clearing_removes_the_icons_in_java_order()
    {
        var rig = new Rig();

        rig.Service.ClearAllItemTime(rig.Player);

        Assert.Equal(ItemTimeSpec.Clear.Select(c => (short)c.Icon).ToArray(), rig.Host.Messages.Select(m => ReadShort(m.Data, 0)).ToArray());
    }

    [Theory]
    [MemberData(nameof(ClearRows))]
    public void Clearing_one_flag_switches_it_off_and_removes_its_icon(string flag, int icon)
    {
        var rig = new Rig();
        Prop(flag).SetValue(rig.Time, true);

        rig.Service.ClearAllItemTime(rig.Player);

        Assert.False((bool)Prop(flag).GetValue(rig.Time)!);
        Assert.Contains(rig.Host.Messages, m => ReadShort(m.Data, 0) == icon);
    }

    [Fact]
    public void Clearing_leaves_the_meal_and_the_plain_use_flag_which_java_never_cleared()
    {
        var rig = new Rig();
        rig.Time.IsEatMeal = true;
        rig.Time.IsActive = true;

        rig.Service.ClearAllItemTime(rig.Player);

        Assert.True(rig.Time.IsEatMeal);
        Assert.True(rig.Time.IsActive);
    }

    [Fact]
    public void Clearing_logs_a_failure_and_stops_instead_of_throwing()
    {
        var rig = new Rig();
        var failing = new ThrowingHost();
        var service = new ItemTimeService(failing, ItemTestKit.Clock(rig.Clock));

        service.ClearAllItemTime(rig.Player);

        Assert.True(failing.Attempts >= 1);
    }

    // ----- auto play item ---------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Turning_auto_play_on_takes_the_minutes_from_the_item_and_tells_the_client()
    {
        var rig = new Rig();
        Item item = ItemTestKit.MakeItem(ItemTestKit.Catalog(), 5, 0, (1, 3));

        rig.Service.TurnOnAutoPlay(rig.Player, item);

        Assert.True(rig.Time.IsAutoPlayEnabled);
        Assert.Equal(180_000, rig.Time.AutoPlayDurationMs);
        Assert.Equal(rig.Now, rig.Time.LastTimeAutoPlay);
        Assert.Equal(0, item.GetOptionParam(1));
        Assert.Equal(new[] { "msg-116", "msg-106", "bag" }, rig.Host.Calls);
        Assert.Equal(new byte[] { 1 }, rig.Host.Messages[0].Data);
        Assert.Equal(180, ReadShort(rig.Host.Messages[1].Data, 2));
        Assert.Equal(4387, ReadShort(rig.Host.Messages[1].Data, 0));
    }

    [Fact]
    public void Turning_auto_play_on_with_an_item_that_has_no_minutes_gives_zero_length()
    {
        var rig = new Rig();

        rig.Service.TurnOnAutoPlay(rig.Player, ItemTestKit.MakeItem(ItemTestKit.Catalog(), 5));

        Assert.True(rig.Time.IsAutoPlayEnabled);
        Assert.Equal(0, rig.Time.AutoPlayDurationMs);
    }

    [Fact]
    public void Auto_play_length_wraps_in_32_bits_for_a_huge_minute_count()
    {
        var rig = new Rig();
        Item item = ItemTestKit.MakeItem(ItemTestKit.Catalog(), 5, 0, (1, 100_000));

        rig.Service.TurnOnAutoPlay(rig.Player, item);

        Assert.Equal(unchecked(100_000 * 60 * 1000), rig.Time.AutoPlayDurationMs);
    }

    [Fact]
    public void Turning_auto_play_off_gives_back_the_whole_minutes_not_used()
    {
        var rig = new Rig();
        Item item = ItemTestKit.MakeItem(ItemTestKit.Catalog(), 5, 0, (1, 3));
        rig.Service.TurnOnAutoPlay(rig.Player, item);
        rig.Clock.Advance(TimeSpan.FromSeconds(61));
        rig.Host.Messages.Clear();
        rig.Host.Calls.Clear();

        rig.Service.TurnOffAutoPlay(rig.Player, item);

        // (180000 - 61000) / 60 / 1000 = 1
        Assert.False(rig.Time.IsAutoPlayEnabled);
        Assert.Equal(1, item.GetOptionParam(1));
        Assert.Equal(new[] { "msg-116", "msg-106", "bag" }, rig.Host.Calls);
        Assert.Equal(new byte[] { 0 }, rig.Host.Messages[0].Data);
        Assert.Equal(0, ReadShort(rig.Host.Messages[1].Data, 2));
    }

    [Fact]
    public void Turning_auto_play_off_after_it_ran_out_subtracts_minutes_because_the_cast_keeps_the_sign()
    {
        var rig = new Rig();
        Item item = ItemTestKit.MakeItem(ItemTestKit.Catalog(), 5, 0, (1, 1));
        rig.Service.TurnOnAutoPlay(rig.Player, item);
        rig.Clock.Advance(TimeSpan.FromMinutes(5));

        rig.Service.TurnOffAutoPlay(rig.Player, item);

        // (60000 - 300000) / 60 / 1000 = -4
        Assert.Equal(-4, item.GetOptionParam(1));
    }

    [Fact]
    public void Auto_play_calls_reject_null_arguments()
    {
        var rig = new Rig();

        Assert.Throws<ArgumentNullException>(() => rig.Service.TurnOnAutoPlay(null!, new Item()));
        Assert.Throws<ArgumentNullException>(() => rig.Service.TurnOnAutoPlay(rig.Player, null!));
        Assert.Throws<ArgumentNullException>(() => rig.Service.TurnOffAutoPlay(rig.Player, null!));
        Assert.Throws<ArgumentNullException>(() => rig.Service.SendAllItemTime(null!));
        Assert.Throws<ArgumentNullException>(() => rig.Service.ClearAllItemTime(null!));
    }
}
