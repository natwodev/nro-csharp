using Nro.Game;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.Items.InventoryTests;

/// <summary><c>addItemGiftCodeToPlayer</c> with a fake gift-code table (the real SQL is covered by <see cref="GiftCodeRepositoryTests"/>).</summary>
public class InventoryServiceGiftCodeTests
{
    private const string Header = "|7|Giftcode: ABC\n|6|Phần Thưởng Của Bạn Là :\b";

    private static KeyValuePair<int, int> E(int id, int quantity) => new(id, quantity);

    private static InvKit Kit(int countLeft = 10)
    {
        var clock = new ManualTimeProvider(ManualTimeProvider.Vn(2026, 11, 3, 10, 30, 15));
        var k = new InvKit(time: clock);
        k.GiftRepo.CountLeft["ABC"] = countLeft;
        return k;
    }

    [Fact]
    public async Task Unknown_code_does_nothing_at_all()
    {
        InvKit k = Kit();

        await k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [E(-1, 100)], [], "NOPE");

        Assert.Equal(new[] { "Get:NOPE" }, k.GiftRepo.Calls);
        Assert.Empty(k.Notifier.Calls);
        Assert.Equal(0, k.Inv.Gold);
    }

    [Fact]
    public async Task Money_rewards_are_added_with_their_own_caps_and_listed_in_the_message()
    {
        InvKit k = Kit();
        k.Inv.Gold = 1_000;
        k.Inv.Gem = 7;
        k.Inv.Ruby = 5;

        await k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [E(-1, 100), E(-2, 20), E(-3, 30), E(-10, 40)], [], "ABC");

        Assert.Equal(1_100, k.Inv.Gold);
        Assert.Equal(27, k.Inv.Gem);
        Assert.Equal(75, k.Inv.Ruby);
        Assert.Equal(
            new[] { "SendAdminNotification:" + Header + "100 vàng\b20 ngọc\b30 ngọc khóa\b40 ngọc khóa\b" },
            k.Notifier.Calls);
    }

    [Theory]
    [InlineData(1_000_000_000L, 1_500_000_000, 2_000_000_000L)]
    [InlineData(3_000_000_000L, 1, 2_000_000_000L)]
    [InlineData(0L, 2_000_000_000, 2_000_000_000L)]
    [InlineData(0L, int.MaxValue, 2_000_000_000L)]
    [InlineData(1_999_999_999L, 1, 2_000_000_000L)]
    public async Task Gold_reward_is_capped_at_two_billion_and_can_take_gold_away(long start, int reward, long expected)
    {
        InvKit k = Kit();
        k.Inv.Gold = start;

        await k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [E(-1, reward)], [], "ABC");

        Assert.Equal(expected, k.Inv.Gold);
    }

    [Theory]
    [InlineData(150_000_000, 100_000_000, 200_000_000)]
    [InlineData(0, 200_000_001, 200_000_000)]
    [InlineData(199_999_999, 1, 200_000_000)]
    [InlineData(2_000_000_000, 1, 200_000_000)]
    [InlineData(2_000_000_000, 147_483_647, 200_000_000)]
    public async Task Gem_and_ruby_rewards_are_capped_at_200_million_in_32_bit_arithmetic(int start, int reward, int expected)
    {
        InvKit k = Kit();
        k.Inv.Gem = start;
        k.Inv.Ruby = start;

        await k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [E(-2, reward), E(-3, reward)], [], "ABC");

        Assert.Equal(expected, k.Inv.Gem);
        Assert.Equal(expected, k.Inv.Ruby);
    }

    [Fact]
    public async Task Gem_reward_that_overflows_32_bits_wraps_to_a_negative_balance()
    {
        InvKit k = Kit();
        k.Inv.Gem = int.MaxValue;

        await k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [E(-2, 5)], [], "ABC");

        // int.MaxValue + 5 wraps to a negative number, which is below 200,000,000, so Math.min keeps it
        Assert.Equal(unchecked(int.MaxValue + 5), k.Inv.Gem);
        Assert.True(k.Inv.Gem < 0);
    }

    [Fact]
    public async Task Title_rewards_minus_6_and_minus_7_are_disabled()
    {
        InvKit k = Kit();

        await k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [E(-6, 1), E(-7, 1)], [], "ABC");

        Assert.Equal(new[] { "SendAdminNotification:" + Header }, k.Notifier.Calls);
        Assert.Equal(new[] { "Get:ABC", "Save:ABC" }, k.GiftRepo.Calls);
    }

    [Fact]
    public async Task Item_rewards_go_to_the_bag_with_the_code_options_and_lower_the_stored_count_once_per_item()
    {
        InvKit k = Kit(countLeft: 10);
        k.Define(10, name: "Cai Ao");
        k.Define(11, name: "Cai Quan");
        List<ItemOption> options = [k.Opt(4, 12)];

        await k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [E(10, 3), E(11, 5)], options, "ABC");

        Assert.Equal(10, k.Inv.ItemsBag[0].Template!.Id);
        Assert.Equal(3, k.Inv.ItemsBag[0].Quantity);
        Assert.Equal(12, k.Inv.ItemsBag[0].ItemOptions[0].Param);
        Assert.Equal(11, k.Inv.ItemsBag[1].Template!.Id);
        Assert.Equal(5, k.Inv.ItemsBag[1].Quantity);
        Assert.Equal(new[] { "Get:ABC", "Set:ABC:9", "Set:ABC:9", "Save:ABC" }, k.GiftRepo.Calls);
        Assert.Equal(9, k.GiftRepo.CountLeft["ABC"]);
        Assert.Equal(new[] { (1, "ABC"), (1, "ABC") }, k.Usage.Used.Select(u => (u.PlayerId, u.Code)));
        Assert.Equal("SendAdminNotification:" + Header + "x3 Cai Ao\bx5 Cai Quan\b", k.Notifier.Calls[^1]);
        Assert.Equal(2, k.Notifier.Packets.Count(p => p.Command == -36));
    }

    [Fact]
    public async Task Item_reward_shares_the_code_option_list_and_an_empty_list_gets_option_73()
    {
        InvKit k = Kit();
        List<ItemOption> options = [];

        await k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [E(10, 1)], options, "ABC");

        // Java assigned the code's own list to the item, and addItemList appended the filler option to it
        Assert.Single(options);
        Assert.Equal(73, options[0].OptionTemplate.Id);
        Assert.Equal(73, k.Inv.ItemsBag[0].ItemOptions[0].OptionTemplate.Id);
    }

    [Fact]
    public async Task Item_reward_with_a_full_bag_is_still_counted_and_listed()
    {
        InvKit k = Kit();
        for (int i = 0; i < k.Inv.ItemsBag.Count; i++)
        {
            k.Put(k.Inv.ItemsBag, i, k.Item(100 + i));
        }

        await k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [E(10, 1)], [], "ABC");

        Assert.DoesNotContain(k.Inv.ItemsBag, i => i.Template!.Id == 10);
        Assert.Equal(new[] { "Get:ABC", "Set:ABC:9", "Save:ABC" }, k.GiftRepo.Calls);
        Assert.Contains("x1 item10\b", k.Notifier.Calls[^1]);
    }

    [Fact]
    public async Task Item_reward_of_type_12_gold_bar_gets_the_lock_option_and_quantity_zero()
    {
        InvKit k = Kit();
        k.Define(457, 12, name: "Thoi vang");
        k.Put(k.Inv.ItemsBag, 0, k.Item(457, 5, (30, 0)));

        await k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [E(457, 50)], [], "ABC");

        // quantity stays 0 in this branch (Java never set it), so the locked stack in the bag is what accepts the "add"
        Assert.Equal(5, k.Inv.ItemsBag[0].Quantity);
        Assert.Equal(1, k.CountReal(k.Inv.ItemsBag));
        Assert.Contains("x50 Thoi vang\b", k.Notifier.Calls[^1]);
    }

    [Fact]
    public async Task The_use_is_saved_with_the_account_the_player_and_the_vietnam_time()
    {
        InvKit k = Kit();
        k.Host.UserId = 4242;

        await k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [E(-1, 1)], [], "ABC");

        var saved = Assert.Single(k.GiftRepo.Saved);
        Assert.Equal(4242, saved.UserId);
        Assert.Equal(1, saved.PlayerId);
        Assert.Equal("ABC", saved.Code);
        Assert.Equal(new DateTime(2026, 11, 3, 10, 30, 15), saved.When);
    }

    [Fact]
    public async Task Arguments_are_checked()
    {
        InvKit k = Kit();
        await Assert.ThrowsAsync<ArgumentNullException>(() => k.Service.AddItemGiftCodeToPlayerAsync(null!, [], [], "ABC"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => k.Service.AddItemGiftCodeToPlayerAsync(k.Player, null!, [], "ABC"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [], null!, "ABC"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [], [], null!));
    }

    [Fact]
    public async Task A_player_without_a_session_gets_the_rewards_but_the_save_and_the_message_fail()
    {
        InvKit k = Kit();
        k.Host.UserId = -1;

        await Assert.ThrowsAsync<InvalidOperationException>(() => k.Service.AddItemGiftCodeToPlayerAsync(k.Player, [E(-1, 7)], [], "ABC"));

        Assert.Equal(7, k.Inv.Gold);
        Assert.Empty(k.GiftRepo.Saved);
        Assert.Empty(k.Notifier.Calls);
    }
}
