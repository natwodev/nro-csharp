using System.Reflection;
using Nro.Game;
using Nro.Tests.Unit.Helpers;
using Nro.Tests.Unit.Game.Items.Core;

namespace Nro.Tests.Unit.Game.Items.Time;

public class ItemTimeTests
{
    private static (ItemTime Time, Player Player, RecordingHost Host, ManualTimeProvider Clock) Make()
    {
        ManualTimeProvider clock = ItemTestKit.Time();
        var host = new RecordingHost();
        Player player = ItemTestKit.NewPlayer();
        var time = new ItemTime(player, ItemTestKit.Clock(clock), host);
        player.ItemTime = time;
        return (time, player, host, clock);
    }

    private static PropertyInfo Prop(string javaName)
    {
        string csharpName = ItemTimeSpec.CSharpName(javaName);
        return typeof(ItemTime).GetProperty(csharpName) ?? throw new InvalidOperationException("missing property " + csharpName + " (Java: " + javaName + ")");
    }

    private static long Duration(ItemTime time, string durationName)
        => durationName == "timeTDLT" ? time.AutoPlayDurationMs : Convert.ToInt64(typeof(ItemTime).GetField(ItemTimeSpec.CSharpConstantName(durationName))!.GetRawConstantValue());

    public static IEnumerable<object[]> ExpiryRows() => ItemTimeSpec.Expiry.Select(r => new object[] { r.Flag, r.Last, r.Duration, r.Calls });

    // ----- shape of the class (constants and fields against the Java source) ---------------------------------------------------------------------------

    public static IEnumerable<object[]> ConstantRows() => ItemTimeSpec.Constants.Select(c => new object[] { c.Name, c.JavaType, c.Value });

    [Theory]
    [MemberData(nameof(ConstantRows))]
    public void Every_java_constant_exists_with_its_value(string name, string javaType, int value)
    {
        FieldInfo field = typeof(ItemTime).GetField(ItemTimeSpec.CSharpConstantName(name))!;

        Assert.NotNull(field);
        Assert.True(field.IsLiteral);
        Assert.Equal(javaType == "byte" ? typeof(sbyte) : typeof(int), field.FieldType);
        Assert.Equal(value, Convert.ToInt32(field.GetRawConstantValue()));
    }

    public static IEnumerable<object[]> FieldRows() => ItemTimeSpec.Fields.Select(f => new object[] { f.Name, f.JavaType });

    [Theory]
    [MemberData(nameof(FieldRows))]
    public void Every_java_field_exists_as_a_read_write_property_of_the_right_type(string name, string javaType)
    {
        PropertyInfo property = Prop(name);

        Assert.Equal(javaType switch { "boolean" => typeof(bool), "long" => typeof(long), _ => typeof(int) }, property.PropertyType);
        Assert.True(property.CanRead && property.CanWrite);
    }

    [Fact]
    public void The_tables_cover_the_known_sizes()
    {
        Assert.Equal(62, ItemTimeSpec.Constants.Length);
        Assert.Equal(171, ItemTimeSpec.Fields.Length);
        Assert.Equal(84, ItemTimeSpec.Expiry.Length);
    }

    [Fact]
    public void A_new_item_time_has_every_flag_off_and_every_time_zero()
    {
        (ItemTime time, _, _, _) = Make();

        foreach ((string name, _) in ItemTimeSpec.Fields)
        {
            Assert.Equal(Activator.CreateInstance(Prop(name).PropertyType), Prop(name).GetValue(time));
        }
    }

    [Fact]
    public void Text_ids_for_the_dungeon_countdowns()
    {
        Assert.Equal(0, ItemTime.BARRACKS);
        Assert.Equal(1, ItemTime.TREASURE_UNDER_SEA);
        Assert.Equal(2, ItemTime.DESTRON_GAS);
        Assert.Equal(3, ItemTime.SNAKE_WAY);
        Assert.Equal(7, ItemTime.CLAN_BOSS_MAP);
    }

    // ----- update(): every one of the 84 expiry checks --------------------------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(ExpiryRows))]
    public void An_effect_stays_on_until_strictly_more_than_its_duration_has_passed(string flag, string last, string durationName, string calls)
    {
        Assert.NotNull(calls);
        (ItemTime time, _, RecordingHost host, ManualTimeProvider clock) = Make();
        time.AutoPlayDurationMs = 60_000;
        long duration = Duration(time, durationName);
        long now = clock.GetUtcNow().ToUnixTimeMilliseconds();
        Prop(flag).SetValue(time, true);
        Prop(last).SetValue(time, now - duration);

        time.Update();

        Assert.True((bool)Prop(flag).GetValue(time)!, flag + " must still be on at exactly its duration");
        Assert.Empty(host.Calls);
    }

    [Theory]
    [MemberData(nameof(ExpiryRows))]
    public void An_effect_switches_off_one_millisecond_after_its_duration_with_its_side_effects(string flag, string last, string durationName, string calls)
    {
        (ItemTime time, _, RecordingHost host, ManualTimeProvider clock) = Make();
        time.AutoPlayDurationMs = 60_000;
        long duration = Duration(time, durationName);
        long now = clock.GetUtcNow().ToUnixTimeMilliseconds();
        Prop(flag).SetValue(time, true);
        Prop(last).SetValue(time, now - duration - 1);

        time.Update();

        Assert.False((bool)Prop(flag).GetValue(time)!, flag + " must be off");
        Assert.Equal(calls, string.Join(",", host.Calls));
    }

    [Theory]
    [MemberData(nameof(ExpiryRows))]
    public void An_effect_that_is_off_is_never_touched(string flag, string last, string durationName, string calls)
    {
        Assert.NotNull(durationName);
        Assert.NotNull(calls);
        (ItemTime time, _, RecordingHost host, _) = Make();
        Prop(last).SetValue(time, -1_000_000_000L);

        time.Update();

        Assert.False((bool)Prop(flag).GetValue(time)!);
        Assert.Empty(host.Calls);
    }

    [Fact]
    public void Update_checks_in_java_order_so_the_side_effects_come_in_that_order()
    {
        (ItemTime time, _, RecordingHost host, ManualTimeProvider clock) = Make();
        long old = clock.GetUtcNow().ToUnixTimeMilliseconds() - 100_000_000;
        time.IsUseStandardInvisibilityPotion = true;
        time.LastTimeStandardInvisibilityPotion = old;
        time.HasMonkeyTail = true;
        time.MonkeyTailStartTime = old;
        time.IsUseMonkeyTailPowerPotential = true;
        time.LastTimeUseMonkeyTailPowerPotential = old;

        time.Update();

        Assert.Equal(new[] { "point", "duoikhi", "point", "point", "notify" }, host.Calls);
    }

    [Fact]
    public void Update_leaves_effects_that_are_still_running_alone_while_ending_the_others()
    {
        (ItemTime time, _, RecordingHost host, ManualTimeProvider clock) = Make();
        long now = clock.GetUtcNow().ToUnixTimeMilliseconds();
        time.IsUseBloodTonic = true;
        time.LastTimeBloodTonic = now - 100_000;
        time.IsUseEnergyTonic = true;
        time.LastTimeEnergyTonic = now - 700_000;

        time.Update();

        Assert.True(time.IsUseBloodTonic);
        Assert.False(time.IsUseEnergyTonic);
        Assert.Equal(new[] { "point" }, host.Calls);
    }

    [Fact]
    public void Update_follows_the_clock_it_was_given()
    {
        (ItemTime time, _, RecordingHost host, ManualTimeProvider clock) = Make();
        time.IsEatMeal = true;
        time.LastTimeEatMeal = clock.GetUtcNow().ToUnixTimeMilliseconds();

        time.Update();
        Assert.True(time.IsEatMeal);

        clock.Advance(TimeSpan.FromMilliseconds(ItemTime.TIME_EAT_MEAL + 1));
        time.Update();

        Assert.False(time.IsEatMeal);
        Assert.Equal(new[] { "point" }, host.Calls);
    }

    [Fact]
    public void Auto_play_end_sends_the_flag_off_message()
    {
        (ItemTime time, _, RecordingHost host, ManualTimeProvider clock) = Make();
        time.IsAutoPlayEnabled = true;
        time.AutoPlayDurationMs = 1000;
        time.LastTimeAutoPlay = clock.GetUtcNow().ToUnixTimeMilliseconds() - 1001;

        time.Update();

        (_, sbyte command, byte[] data) = Assert.Single(host.Messages);
        Assert.Equal(-116, command);
        Assert.Equal(new byte[] { 0 }, data);
    }

    [Fact]
    public void Auto_play_with_a_zero_length_ends_on_the_first_millisecond()
    {
        (ItemTime time, _, RecordingHost host, ManualTimeProvider clock) = Make();
        time.IsAutoPlayEnabled = true;
        time.AutoPlayDurationMs = 0;
        time.LastTimeAutoPlay = clock.GetUtcNow().ToUnixTimeMilliseconds();

        time.Update();
        Assert.True(time.IsAutoPlayEnabled);
        clock.Advance(TimeSpan.FromMilliseconds(1));
        time.Update();

        Assert.False(time.IsAutoPlayEnabled);
    }

    [Fact]
    public void Open_power_flag_goes_off_after_the_power_limit_step_and_notice()
    {
        (ItemTime time, _, RecordingHost host, ManualTimeProvider clock) = Make();
        time.IsOpenPower = true;
        time.LastTimeOpenPower = clock.GetUtcNow().ToUnixTimeMilliseconds() - ItemTime.TIME_OPEN_POWER - 1;

        time.Update();

        Assert.False(time.IsOpenPower);
        Assert.Equal(new[] { "powerlimit", "notify" }, host.Calls);
        Assert.Equal("Giới hạn sức mạnh của bạn đã được tăng lên 1 bậc", host.Notifications.Single());
    }

    [Fact]
    public void The_invisibility_potions_each_send_their_own_text()
    {
        (ItemTime time, _, RecordingHost host, ManualTimeProvider clock) = Make();
        long old = clock.GetUtcNow().ToUnixTimeMilliseconds() - 100_000_000;
        time.IsUseStandardInvisibilityPotion = true;
        time.LastTimeStandardInvisibilityPotion = old;
        time.IsUseEnhancedInvisibilityPotion = true;
        time.LastTimeEnhancedInvisibilityPotion = old;

        time.Update();

        Assert.Equal(new[] { "Thuốc Tàng Hình Thường Đã Hết Tác Dụng!", "Thuốc Tàng Hình Cải Tiến Đã Hết Tác Dụng!" }, host.Notifications);
    }

    [Fact]
    public void The_default_host_and_clock_make_a_bare_item_time_usable_in_the_player_shell()
    {
        var player = new Player();

        player.ItemTime.IsUseBloodTonic = true;
        player.ItemTime.LastTimeBloodTonic = 1;
        player.ItemTime.Update();

        Assert.False(player.ItemTime.IsUseBloodTonic);
        Assert.Same(player, player.ItemTime.Player);
    }

    [Fact]
    public void Dispose_does_not_change_anything()
    {
        (ItemTime time, Player player, _, _) = Make();
        time.IsUseBloodTonic = true;

        time.Dispose();

        Assert.True(time.IsUseBloodTonic);
        Assert.Same(player, time.Player);
    }

    [Fact]
    public void Can_do_with_time_is_strictly_greater()
    {
        (ItemTime time, _, _, ManualTimeProvider clock) = Make();
        long now = clock.GetUtcNow().ToUnixTimeMilliseconds();

        Assert.False(time.CanDoWithTime(now - 1000, 1000));
        Assert.True(time.CanDoWithTime(now - 1001, 1000));
        Assert.False(time.CanDoWithTime(now + 5, 0));
        Assert.True(time.CanDoWithTime(0, 1000));
    }
}
