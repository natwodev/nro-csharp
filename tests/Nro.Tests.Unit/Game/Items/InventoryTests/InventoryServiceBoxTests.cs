using Nro.Game;

namespace Nro.Tests.Unit.Game.Items.InventoryTests;

/// <summary>Moving items between the bag, the body, the box, the collection box and the clan box.</summary>
public class InventoryServiceBoxTests
{
    // ------------------------------------------------------------------ box -> body or bag

    [Fact]
    public void ItemBoxToBodyOrBag_armor_goes_onto_an_empty_body_slot_and_the_box_gets_the_empty_item()
    {
        var k = new InvKit();
        k.Define(10, 0, gender: 0, strRequire: 100);
        Item armor = k.Put(k.Inv.ItemsBox, 3, k.Item(10));

        k.Service.ItemBoxToBodyOrBag(k.Player, 3);

        Assert.Same(armor, k.Inv.ItemsBody[0]);
        Assert.False(k.Inv.ItemsBox[3].IsNotNullItem());
        Assert.Equal(0, k.CountReal(k.Inv.ItemsBag));
        Assert.Equal(new sbyte[] { -37, -35, -35 }, k.Notifier.Packets.Select(p => p.Command));
        Assert.Contains("Point", k.Notifier.Calls);
    }

    [Fact]
    public void ItemBoxToBodyOrBag_type_32_uses_body_slot_6()
    {
        var k = new InvKit();
        k.Define(10, 32);
        Item item = k.Put(k.Inv.ItemsBox, 0, k.Item(10));

        k.Service.ItemBoxToBodyOrBag(k.Player, 0);

        Assert.Same(item, k.Inv.ItemsBody[6]);
    }

    [Theory]
    [InlineData(1, 0, 100, 1000L)]
    [InlineData(0, 0, 1000, 999L)]
    public void ItemBoxToBodyOrBag_armor_that_cannot_be_worn_goes_to_the_bag(int gender, int playerGender, int strRequire, long power)
    {
        var k = new InvKit();
        k.Define(10, 0, gender: (sbyte)gender, strRequire: strRequire);
        k.Player.Gender = (sbyte)playerGender;
        k.Host.PowerValue = power;
        k.Put(k.Inv.ItemsBox, 3, k.Item(10));

        k.Service.ItemBoxToBodyOrBag(k.Player, 3);

        Assert.False(k.Inv.ItemsBody[0].IsNotNullItem());
        Assert.Equal(10, k.Inv.ItemsBag[0].Template!.Id);
        Assert.False(k.Inv.ItemsBox[3].IsNotNullItem());
    }

    [Fact]
    public void ItemBoxToBodyOrBag_armor_goes_to_the_bag_when_the_body_slot_is_taken()
    {
        var k = new InvKit();
        k.Define(10, 0);
        k.Put(k.Inv.ItemsBody, 0, k.Item(11));
        Item armor = k.Put(k.Inv.ItemsBox, 3, k.Item(10));

        k.Service.ItemBoxToBodyOrBag(k.Player, 3);

        Assert.Equal(11, k.Inv.ItemsBody[0].Template!.Id);
        Assert.Equal(10, k.Inv.ItemsBag[0].Template!.Id);
        Assert.False(k.Inv.ItemsBox[3].IsNotNullItem());
        Assert.Equal(0, armor.Quantity);
    }

    [Fact]
    public void ItemBoxToBodyOrBag_money_items_added_to_the_wallet_stay_in_the_box()
    {
        var k = new InvKit();
        k.Define(190, 9);
        Item gold = k.Put(k.Inv.ItemsBox, 0, k.Item(190, 500));

        k.Service.ItemBoxToBodyOrBag(k.Player, 0);

        Assert.Equal(500, k.Inv.Gold);
        Assert.Same(gold, k.Inv.ItemsBox[0]);
        Assert.Equal(500, gold.Quantity);
    }

    [Fact]
    public void ItemBoxToBodyOrBag_checks_index_and_free_bag_slot()
    {
        var k = new InvKit();
        k.Service.ItemBoxToBodyOrBag(k.Player, -1);
        Assert.Equal(new[] { "SendNotification:Không thể thực hiện" }, k.Notifier.Calls);

        k.Notifier.Calls.Clear();
        for (int i = 0; i < k.Inv.ItemsBag.Count; i++)
        {
            k.Put(k.Inv.ItemsBag, i, k.Item(100 + i));
        }

        k.Put(k.Inv.ItemsBox, 0, k.Item(10));
        k.Service.ItemBoxToBodyOrBag(k.Player, 0);
        Assert.Equal(new[] { "SendNotification:Hành trang của bạn không đủ chỗ trống" }, k.Notifier.Calls);
        Assert.True(k.Inv.ItemsBox[0].IsNotNullItem());
    }

    [Fact]
    public void ItemBoxToBodyOrBag_empty_box_slot_does_nothing_and_bad_index_throws()
    {
        var k = new InvKit();
        k.Service.ItemBoxToBodyOrBag(k.Player, 2);
        Assert.Empty(k.Notifier.Packets);
        Assert.Throws<ArgumentOutOfRangeException>(() => k.Service.ItemBoxToBodyOrBag(k.Player, 99));
    }

    [Fact]
    public void ItemBoxCollectionToBodyOrBag_works_like_the_box_but_without_the_set_update()
    {
        var k = new InvKit();
        for (int slot = 0; slot < 4; slot++)
        {
            k.Define(20 + slot, (sbyte)slot);
            k.Put(k.Inv.ItemsBody, slot, k.Item(20 + slot, 1, (127, 1), (139, 0)));
        }

        k.Define(24, 4);
        Item fifth = k.Put(k.Inv.ItemsBoxCollection, 1, k.Item(24, 1, (127, 1), (139, 0)));

        k.Service.ItemBoxCollectionToBodyOrBag(k.Player, 1);

        Assert.Same(fifth, k.Inv.ItemsBody[4]);
        Assert.False(k.Inv.ItemsBoxCollection[1].IsNotNullItem());
        Assert.All(k.Inv.ItemsBody.Take(5), i => Assert.Equal(0, i.GetOptionParam(139)));
        Assert.Equal(-35, k.Notifier.Packets[^2].Command);
    }

    [Fact]
    public void ItemBoxCollectionToBodyOrBag_negative_index_and_full_bag_and_plain_item()
    {
        var k = new InvKit();
        k.Service.ItemBoxCollectionToBodyOrBag(k.Player, -3);
        Assert.Equal("SendNotification:Không thể thực hiện", k.Notifier.Calls.Single());

        k.Notifier.Calls.Clear();
        k.Put(k.Inv.ItemsBoxCollection, 0, k.Item(50));
        k.Service.ItemBoxCollectionToBodyOrBag(k.Player, 0);
        Assert.Equal(50, k.Inv.ItemsBag[0].Template!.Id);
        Assert.False(k.Inv.ItemsBoxCollection[0].IsNotNullItem());

        for (int i = 0; i < k.Inv.ItemsBag.Count; i++)
        {
            k.Put(k.Inv.ItemsBag, i, k.Item(100 + i));
        }

        k.Put(k.Inv.ItemsBoxCollection, 1, k.Item(51));
        k.Notifier.Calls.Clear();
        k.Service.ItemBoxCollectionToBodyOrBag(k.Player, 1);
        Assert.Equal("SendNotification:Hành trang của bạn không đủ chỗ trống", k.Notifier.Calls.Single());
    }

    // ------------------------------------------------------------------ bag -> box

    [Fact]
    public void ItemBagToBox_moves_the_item_sorts_the_bag_and_sends_both_lists()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(50));
        k.Put(k.Inv.ItemsBag, 1, k.Item(51));
        k.Put(k.Inv.ItemsBag, 2, k.Item(52));

        k.Service.ItemBagToBox(k.Player, 0);

        Assert.Equal(50, k.Inv.ItemsBox[0].Template!.Id);
        Assert.Equal(new[] { 52, 51, -1 }, k.Ids(k.Inv.ItemsBag).Take(3));
        Assert.Equal(new sbyte[] { -36, -35, -35 }, k.Notifier.Packets.Select(p => p.Command));
    }

    [Theory]
    [InlineData(570)]
    [InlineData(543)]
    public void ItemBagToBox_refuses_the_wood_chest_and_the_gold_ring(int id)
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(id));

        k.Service.ItemBagToBox(k.Player, 0);

        Assert.True(k.Inv.ItemsBag[0].IsNotNullItem());
        Assert.Equal(new[] { "SendNotification:Không thể cất vật phẩm này vào rương" }, k.Notifier.Calls);
    }

    [Fact]
    public void ItemBagToBox_full_box_changes_nothing_and_sends_nothing()
    {
        var k = new InvKit();
        for (int i = 0; i < k.Inv.ItemsBox.Count; i++)
        {
            k.Put(k.Inv.ItemsBox, i, k.Item(100 + i));
        }

        k.Put(k.Inv.ItemsBag, 0, k.Item(50));
        k.Service.ItemBagToBox(k.Player, 0);

        Assert.True(k.Inv.ItemsBag[0].IsNotNullItem());
        Assert.Empty(k.Notifier.Packets);
    }

    [Fact]
    public void ItemBagToBox_stackable_items_merge_into_the_box_stack_and_the_bag_slot_is_emptied()
    {
        var k = new InvKit();
        k.Define(60, upToUp: true);
        k.Put(k.Inv.ItemsBox, 0, k.Item(60, 10));
        k.Put(k.Inv.ItemsBag, 0, k.Item(60, 5));

        k.Service.ItemBagToBox(k.Player, 0);

        Assert.Equal(15, k.Inv.ItemsBox[0].Quantity);
        Assert.Equal(0, k.CountReal(k.Inv.ItemsBag));
    }

    [Fact]
    public void ItemBagToBox_negative_index_empty_slot_and_bad_index()
    {
        var k = new InvKit();
        k.Service.ItemBagToBox(k.Player, -1);
        Assert.Equal("SendNotification:Không thể thực hiện", k.Notifier.Calls.Single());
        k.Notifier.Calls.Clear();

        k.Service.ItemBagToBox(k.Player, 5);
        Assert.Empty(k.Notifier.Calls);
        Assert.Throws<ArgumentOutOfRangeException>(() => k.Service.ItemBagToBox(k.Player, 500));
    }

    [Fact]
    public void ItemBagToBoxCollection_only_takes_collection_items_without_expiry()
    {
        var k = new InvKit();
        k.Define(60, 5);
        k.Define(61, 99);
        Item ok = k.Put(k.Inv.ItemsBag, 0, k.Item(60));
        k.Put(k.Inv.ItemsBag, 1, k.Item(61));
        k.Put(k.Inv.ItemsBag, 2, k.Item(60, 1, (93, 3)));

        k.Service.ItemBagToBoxCollection(k.Player, 1);
        Assert.Equal("SendNotification:Không thể cất vật phẩm này vào rương", k.Notifier.Calls.Single());

        k.Notifier.Calls.Clear();
        k.Service.ItemBagToBoxCollection(k.Player, 2);
        Assert.Equal("SendNotification:Không thể cất vật phẩm có hạn sử dụng", k.Notifier.Calls.Single());

        k.Notifier.Calls.Clear();
        k.Service.ItemBagToBoxCollection(k.Player, 0);
        Assert.Equal(60, k.Inv.ItemsBoxCollection[0].Template!.Id);
        Assert.DoesNotContain(ok, k.Inv.ItemsBag);
        Assert.Equal(new sbyte[] { -36, -35, -35 }, k.Notifier.Packets.Select(p => p.Command));
    }

    [Fact]
    public void ItemBagToBoxCollection_negative_index_and_full_collection_box()
    {
        var k = new InvKit();
        k.Service.ItemBagToBoxCollection(k.Player, -1);
        Assert.Equal("SendNotification:Không thể thực hiện", k.Notifier.Calls.Single());

        k.Notifier.Calls.Clear();
        k.Define(60, 5);
        for (int i = 0; i < k.Inv.ItemsBoxCollection.Count; i++)
        {
            k.Put(k.Inv.ItemsBoxCollection, i, k.Item(100 + i));
        }

        k.Put(k.Inv.ItemsBag, 0, k.Item(60));
        k.Service.ItemBagToBoxCollection(k.Player, 0);
        Assert.True(k.Inv.ItemsBag[0].IsNotNullItem());
        Assert.Empty(k.Notifier.Packets);
    }

    // ------------------------------------------------------------------ body -> box

    [Fact]
    public void ItemBodyToBox_moves_the_worn_item_into_the_box()
    {
        var k = new InvKit();
        k.Define(10, 0);
        Item item = k.Put(k.Inv.ItemsBody, 0, k.Item(10));

        k.Service.ItemBodyToBox(k.Player, 0);

        Assert.False(k.Inv.ItemsBody[0].IsNotNullItem());
        Assert.Same(item, k.Inv.ItemsBox[0]);
        Assert.Contains("Point", k.Notifier.Calls);
        Assert.Equal(new sbyte[] { -37, -35, -35 }, k.Notifier.Packets.Select(p => p.Command));
    }

    [Fact]
    public void ItemBodyToBox_with_a_full_box_leaves_the_item_on_the_body_but_still_refreshes()
    {
        var k = new InvKit();
        for (int i = 0; i < k.Inv.ItemsBox.Count; i++)
        {
            k.Put(k.Inv.ItemsBox, i, k.Item(100 + i));
        }

        Item item = k.Put(k.Inv.ItemsBody, 0, k.Item(10));
        k.Service.ItemBodyToBox(k.Player, 0);

        Assert.Same(item, k.Inv.ItemsBody[0]);
        Assert.Contains("Point", k.Notifier.Calls);
    }

    [Fact]
    public void ItemBodyToBox_empty_slot_does_nothing()
    {
        var k = new InvKit();
        k.Service.ItemBodyToBox(k.Player, 0);
        Assert.Empty(k.Notifier.Calls);
        Assert.Throws<ArgumentOutOfRangeException>(() => k.Service.ItemBodyToBox(k.Player, 99));
    }

    [Fact]
    public void ItemBodyToBoxCollection_only_takes_collection_items_without_expiry()
    {
        var k = new InvKit();
        k.Define(60, 5);
        k.Define(61, 99);
        Item ok = k.Put(k.Inv.ItemsBody, 0, k.Item(60));
        k.Put(k.Inv.ItemsBody, 1, k.Item(61));
        k.Put(k.Inv.ItemsBody, 2, k.Item(60, 1, (93, 3)));

        k.Service.ItemBodyToBoxCollection(k.Player, 1);
        Assert.Equal("SendNotification:Không thể cất vật phẩm này vào rương", k.Notifier.Calls.Single());
        k.Notifier.Calls.Clear();
        k.Service.ItemBodyToBoxCollection(k.Player, 2);
        Assert.Equal("SendNotification:Không thể cất vật phẩm có hạn sử dụng", k.Notifier.Calls.Single());

        k.Notifier.Calls.Clear();
        k.Service.ItemBodyToBoxCollection(k.Player, 0);
        Assert.Same(ok, k.Inv.ItemsBoxCollection[0]);
        Assert.False(k.Inv.ItemsBody[0].IsNotNullItem());
        Assert.Contains("Point", k.Notifier.Calls);
    }

    // ------------------------------------------------------------------ clan box

    private static InvKit WithClan(int boxSlots = 5)
    {
        var k = new InvKit();
        k.Player.Clan = InvKit.Raw<Clan>();
        for (int i = 0; i < boxSlots; i++)
        {
            k.Host.ClanBox.Add(k.Empty());
        }

        return k;
    }

    [Fact]
    public void Clan_methods_do_nothing_without_a_clan()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(50));
        k.Put(k.Inv.ItemsBody, 0, k.Item(50));

        k.Service.ItemBoxClanToBodyOrBag(k.Player, 0);
        k.Service.ItemBagToBoxClan(k.Player, 0);
        k.Service.ItemBodyToBoxClan(k.Player, 0);
        k.Service.SendItemBoxClan(k.Player);
        k.Service.SendItemClan(k.Player);
        k.Service.OpenBoxClan(k.Player);

        Assert.Empty(k.Notifier.Packets);
        Assert.Empty(k.Host.Calls);
        Assert.True(k.Inv.ItemsBag[0].IsNotNullItem());
    }

    [Fact]
    public void ItemBagToBoxClan_moves_the_item_saves_the_box_and_tells_every_online_member()
    {
        InvKit k = WithClan();
        Player member = k.NewPlayer(2);
        member.Clan = k.Player.Clan;
        k.Host.Online[2] = member;
        k.Host.MemberIds.AddRange(new long[] { 1, 2, 3 });
        k.Put(k.Inv.ItemsBag, 0, k.Item(50));

        k.Service.ItemBagToBoxClan(k.Player, 0);

        Assert.Equal(50, k.Host.ClanBox[0].Template!.Id);
        Assert.False(k.Inv.ItemsBag[0].IsNotNullItem());
        Assert.Equal(new[] { "UpdateItemsBoxClanToSql" }, k.Host.Calls);
        // bag, box of the mover (+ open window), then the box of each member that is online (member 3 is offline; the mover is not online in the fake, so only member 2 gets it)
        Assert.Equal(new sbyte[] { -36, -35, -35, -35 }, k.Notifier.Packets.Select(p => p.Command));
        Assert.Equal(2, k.Notifier.Packets[^1].Player.Id);
    }

    [Theory]
    [InlineData(570)]
    [InlineData(543)]
    public void ItemBagToBoxClan_refuses_the_wood_chest_and_the_gold_ring(int id)
    {
        InvKit k = WithClan();
        k.Put(k.Inv.ItemsBag, 0, k.Item(id));

        k.Service.ItemBagToBoxClan(k.Player, 0);

        Assert.Equal("SendNotification:Không thể cất vật phẩm này vào rương", k.Notifier.Calls.Single());
        Assert.False(k.Host.ClanBox[0].IsNotNullItem());
        Assert.Empty(k.Host.Calls);
    }

    [Fact]
    public void ItemBagToBoxClan_refuses_locked_items()
    {
        InvKit k = WithClan();
        k.Put(k.Inv.ItemsBag, 0, k.Item(50, 1, (30, 0)));

        k.Service.ItemBagToBoxClan(k.Player, 0);

        Assert.Equal("SendNotification:Không thể cất vật phẩm này vào rương", k.Notifier.Calls.Single());
        Assert.True(k.Inv.ItemsBag[0].IsNotNullItem());
    }

    [Fact]
    public void ItemBagToBoxClan_negative_index_returns_before_saving()
    {
        InvKit k = WithClan();
        k.Service.ItemBagToBoxClan(k.Player, -1);
        Assert.Equal("SendNotification:Không thể thực hiện", k.Notifier.Calls.Single());
        Assert.Empty(k.Host.Calls);
    }

    [Fact]
    public void ItemBagToBoxClan_with_a_full_clan_box_still_saves_and_notifies_the_members()
    {
        InvKit k = WithClan(2);
        k.Host.ClanBox[0] = k.Item(100);
        k.Host.ClanBox[1] = k.Item(101);
        k.Put(k.Inv.ItemsBag, 0, k.Item(50));

        k.Service.ItemBagToBoxClan(k.Player, 0);

        Assert.True(k.Inv.ItemsBag[0].IsNotNullItem());
        Assert.Equal(new[] { "UpdateItemsBoxClanToSql" }, k.Host.Calls);
    }

    [Fact]
    public void ItemBoxClanToBodyOrBag_armor_goes_on_the_body_and_other_items_go_to_the_bag()
    {
        InvKit k = WithClan();
        k.Define(10, 0);
        Item armor = k.Host.ClanBox[0] = k.Item(10);
        Item other = k.Host.ClanBox[1] = k.Item(50);

        k.Service.ItemBoxClanToBodyOrBag(k.Player, 0);
        Assert.Same(armor, k.Inv.ItemsBody[0]);
        Assert.False(k.Host.ClanBox[0].IsNotNullItem());

        k.Service.ItemBoxClanToBodyOrBag(k.Player, 1);
        Assert.Equal(50, k.Inv.ItemsBag[0].Template!.Id);
        Assert.False(k.Host.ClanBox[1].IsNotNullItem());
        Assert.Equal(0, other.Quantity);
        Assert.Equal(2, k.Host.Calls.Count(c => c == "UpdateItemsBoxClanToSql"));
    }

    [Fact]
    public void ItemBoxClanToBodyOrBag_checks_index_and_bag_space_without_saving()
    {
        InvKit k = WithClan();
        k.Service.ItemBoxClanToBodyOrBag(k.Player, -1);
        Assert.Equal("SendNotification:Không thể thực hiện", k.Notifier.Calls.Single());

        k.Notifier.Calls.Clear();
        for (int i = 0; i < k.Inv.ItemsBag.Count; i++)
        {
            k.Put(k.Inv.ItemsBag, i, k.Item(100 + i));
        }

        k.Host.ClanBox[0] = k.Item(50);
        k.Service.ItemBoxClanToBodyOrBag(k.Player, 0);
        Assert.Equal("SendNotification:Hành trang của bạn không đủ chỗ trống", k.Notifier.Calls.Single());
        Assert.Empty(k.Host.Calls);
    }

    [Fact]
    public void ItemBoxClanToBodyOrBag_empty_clan_slot_still_saves_and_refreshes()
    {
        InvKit k = WithClan();

        k.Service.ItemBoxClanToBodyOrBag(k.Player, 2);

        Assert.Equal(new[] { "UpdateItemsBoxClanToSql" }, k.Host.Calls);
        Assert.Contains(k.Notifier.Packets, p => p.Command == -35);
    }

    [Fact]
    public void ItemBodyToBoxClan_moves_the_worn_item_into_the_clan_box()
    {
        InvKit k = WithClan();
        Item item = k.Put(k.Inv.ItemsBody, 0, k.Item(10));

        k.Service.ItemBodyToBoxClan(k.Player, 0);

        Assert.Same(item, k.Host.ClanBox[0]);
        Assert.False(k.Inv.ItemsBody[0].IsNotNullItem());
        Assert.Equal(new[] { "UpdateItemsBoxClanToSql" }, k.Host.Calls);
        Assert.Contains("Point", k.Notifier.Calls);
    }

    [Fact]
    public void ItemBodyToBoxClan_empty_slot_still_saves()
    {
        InvKit k = WithClan();
        k.Service.ItemBodyToBoxClan(k.Player, 4);
        Assert.Equal(new[] { "UpdateItemsBoxClanToSql" }, k.Host.Calls);
        Assert.DoesNotContain("Point", k.Notifier.Calls);
    }

    [Fact]
    public void SendItemBoxClanToAll_skips_offline_members()
    {
        InvKit k = WithClan();
        Player m1 = k.NewPlayer(11);
        Player m2 = k.NewPlayer(12);
        k.Player.Clan = m1.Clan = m2.Clan = InvKit.Raw<Clan>();
        k.Host.Online[11] = m1;
        k.Host.Online[12] = m2;
        k.Host.MemberIds.AddRange(new long[] { 11, 99, 12 });

        k.Service.SendItemBoxClanToAll(k.Player.Clan);

        Assert.Equal(new long[] { 11, 12 }, k.Notifier.Packets.Select(p => p.Player.Id));
        Assert.All(k.Notifier.Packets, p => Assert.Equal(-35, p.Command));
    }

    [Fact]
    public async Task Clan_box_operations_from_several_threads_never_lose_or_duplicate_an_item()
    {
        InvKit k = WithClan(40);
        int players = 4;
        int perPlayer = 10;
        var kits = new List<(Player P, InvKit K)>();
        for (int i = 0; i < players; i++)
        {
            Player p = k.NewPlayer(100 + i);
            p.Clan = k.Player.Clan;
            for (int j = 0; j < perPlayer; j++)
            {
                k.Put(p.Inventory.ItemsBag, j, k.Item(1000 + (i * perPlayer) + j));
            }

            kits.Add((p, k));
        }

        // every player puts its own 10 items into the shared box; one InventoryService (own fakes) per player, all on the same list
        var tasks = kits.Select(t => Task.Run(() =>
        {
            var local = new InvKit();
            local.Host.ClanBox.Clear();
            var service = new InventoryService(local.Factory, local.Notifier, new SharedBoxHost(k.Host.ClanBox), local.Random, local.GiftRepo, local.Usage);
            for (int j = 0; j < perPlayer; j++)
            {
                service.ItemBagToBoxClan(t.P, 0);
            }
        }));
        await Task.WhenAll(tasks);

        List<int> ids = k.Host.ClanBox.Where(i => i.IsNotNullItem()).Select(i => (int)i.Template!.Id).OrderBy(i => i).ToList();
        Assert.Equal(Enumerable.Range(1000, players * perPlayer), ids);
    }

    private sealed class SharedBoxHost : Nro.Game.Items.Inventory.Ports.IInventoryHost
    {
        private readonly List<Item> box;

        public SharedBoxHost(List<Item> box)
        {
            this.box = box;
        }

        public bool IsPl(Player player) => true;

        public short GetHead(Player player) => 0;

        public Player GetPetMaster(Player pet) => throw new InvalidCastException();

        public long GetPower(Player player) => long.MaxValue;

        public sbyte GetTypeFusion(Player player) => 0;

        public string? GetOpenShopTagName(Player player) => null;

        public int GetSessionUserId(Player player) => 0;

        public void AddTimeCharms(Player player, int itemId, int minutes)
        {
        }

        public void CreateBillEgg(Player player)
        {
        }

        public void SetFullHpMp(Player player)
        {
        }

        public void SetFullHp(Player player)
        {
        }

        public void SetFullMp(Player player)
        {
        }

        public (long TimeCreateAccount, long AccountAgeDays)? AddActivationSetSearchDays(Player player, int days) => null;

        public Player? GetPlayerById(long playerId) => null;

        public void ExitMap(Player player)
        {
        }

        public void DisposePlayer(Player player)
        {
        }

        public bool IsBlackBall(int tempId) => false;

        public bool IsNamecBall(int tempId) => false;

        public bool IsNamecBallStone(int tempId) => false;

        public bool PickBlackBall(Player player, Item item) => false;

        public bool PickNamekBall(Player player, Item item) => false;

        public List<Item> GetItemsBoxClan(Clan clan) => box;

        public IReadOnlyList<long> GetClanMemberIds(Clan clan) => [];

        public void UpdateItemsBoxClanToSql(Clan clan)
        {
        }
    }
}
