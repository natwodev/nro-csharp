using Nro.Game.Players;
using Nro.Game.Players.Ports;
using Nro.Game.Skills;

namespace Nro.Tests.Unit.Game.Players;

// Characterization các nhánh Player.injured; chưa có Java golden vì dependency graph Player đầy đủ chưa dựng được.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 2144–2478).
public sealed class PlayerCombatTests
{
    // Kiểm tra giáp âm sau xuyên giáp cộng bonus rồi cộng sát thương chuẩn (Java: calculateFinalDamage).
    [Fact]
    public void Calculate_final_damage_preserves_negative_armor_bonus_and_true_damage()
    {
        double actual = MakePlayer().CalculateFinalDamage(100, 50, 80, 20, new FakeCombat());

        Assert.Equal(138, actual);
    }

    // Kiểm tra đòn Player: giáp cap 86, trừ HP và random né chạy trước nhánh sau (Java: injured).
    [Fact]
    public void Player_attack_caps_armor_and_returns_the_actual_damage()
    {
        Player victim = MakePlayer(100, 100);
        victim.NPoint.ArmorPercent = 99;
        victim.NPoint.DodgePercent = 1;
        FakeRandom random = new(99);

        double actual = victim.Injured(MakePlayer(), 100, false, false, new FakeCombat(), random, new FakeClock(10_000));

        Assert.Equal(14, actual);
        Assert.Equal(86, victim.NPoint.Hp);
        Assert.Equal(["next 0/99"], random.Calls);
    }

    // Kiểm tra né trả 0 trước giảm sát thương và không đổi HP (Java: injured).
    [Fact]
    public void Dodge_returns_zero_without_changing_hp()
    {
        Player victim = MakePlayer(100, 100);
        victim.NPoint.DodgePercent = 50;
        FakeRandom random = new(49);

        double actual = victim.Injured(MakePlayer(), 20, false, false, new FakeCombat(), random, new FakeClock(10_000));

        Assert.Equal(0, actual);
        Assert.Equal(100, victim.NPoint.Hp);
        Assert.Equal(["next 0/99"], random.Calls);
    }

    // Kiểm tra attacker thiếu skill không xóa né của defender trước Util.isTrue (Java: injured).
    [Fact]
    public void Attacker_without_a_selected_skill_preserves_defender_dodge()
    {
        Player victim = MakePlayer(100, 100);
        victim.NPoint.DodgePercent = 50;

        double actual = victim.Injured(MakePlayer(), 20, false, false, new FakeCombat(), new FakeRandom(49), new FakeClock(10_000));

        Assert.Equal(0, actual);
        Assert.Equal(100, victim.NPoint.Hp);
    }

    // Kiểm tra khiên giới hạn sát thương thành 1, phá khiên và ghi phản sát thương đúng thứ tự (Java: injured).
    [Fact]
    public void Shield_reduces_player_damage_to_one_and_records_reflect_damage()
    {
        Player victim = MakePlayer(100, 100);
        victim.EffectSkill.IsShielding = true;
        victim.NPoint.DodgePercent = 1;
        FakeCombat combat = new();

        double actual = victim.Injured(MakePlayer(), 1_000, false, false, combat, new FakeRandom(99), new FakeClock(10_000));

        Assert.Equal(1, actual);
        Assert.Equal(99, victim.NPoint.Hp);
        Assert.Equal(["break-shield", "reflect 1"], combat.Calls);
    }

    // Kiểm tra quái đánh khi bất tử chừa một HP và không handoff chết (Java: injured).
    [Fact]
    public void Mob_attack_respects_immortal_charm()
    {
        Player victim = MakePlayer(10, 10);
        victim.NPoint.DodgePercent = 1;
        FakeCombat combat = new() { Immortal = true };

        double actual = victim.Injured(null, 10, false, true, combat, new FakeRandom(99), new FakeClock(10_000));

        Assert.Equal(9, actual);
        Assert.Equal(1, victim.NPoint.Hp);
        Assert.DoesNotContain(combat.Calls, value => value.StartsWith("death", StringComparison.Ordinal));
    }

    // Kiểm tra boss tắt ẩn thân và ghi thời điểm qua clock injected (Java: injured).
    [Fact]
    public void Boss_attack_reveals_the_defender()
    {
        Player victim = MakePlayer(100, 100);
        victim.EffectSkin.IsInvisible = true;
        victim.NPoint.DodgePercent = 1;
        Player boss = MakePlayer();
        boss.IsBoss = true;

        victim.Injured(boss, 1, false, false, new FakeCombat(), new FakeRandom(99), new FakeClock(77));

        Assert.False(victim.EffectSkin.IsInvisible);
        Assert.Equal(77, victim.EffectSkin.LastTimeInvisible);
    }

    // Kiểm tra grace period hồi sinh chặn sau random né, đúng thứ tự Java (Java: injured).
    [Fact]
    public void Revive_grace_period_blocks_damage_after_the_dodge_roll()
    {
        Player victim = MakePlayer(100, 100);
        victim.LastTimeRevived = 9_000;
        victim.NPoint.DodgePercent = 1;
        FakeRandom random = new(99);

        double actual = victim.Injured(MakePlayer(), 20, false, false, new FakeCombat(), random, new FakeClock(10_000));

        Assert.Equal(0, actual);
        Assert.Equal(100, victim.NPoint.Hp);
        Assert.Equal(["next 0/99"], random.Calls);
    }

    // Kiểm tra rơi đuôi khỉ đánh dấu persistence dirty thay DB rồi handoff chết (Java: injured).
    [Fact]
    public void Monkey_tail_drop_marks_persistence_dirty_and_orders_gateway_calls()
    {
        Player victim = MakePlayer(10, 10);
        victim.EffectSkill.IsMonkey = true;
        victim.NPoint.DodgePercent = 1;
        victim.NPoint.BombPercent = 1;
        Player attacker = MakePlayer();
        attacker.Gender = 0;
        attacker.PlayerSkill.SkillSelect = new Skill { Template = new SkillTemplate { Id = Skill.QUA_CAU_KENH_KHI } };
        FakeCombat combat = new() { CanDropTail = true };
        FakeRandom random = new(99, 99);

        victim.Injured(attacker, 20, false, false, combat, random, new FakeClock(200_000));

        Assert.Equal(200_000, victim.LastTimeDropTail);
        Assert.Equal(
        [
            "drop 579", "tail-dirty", "notify Rớt đuôi khỉ kìa",
            "notify Bạn đã bị hạ và rớt đuôi khỉ, trong vòng 1 giờ sẽ không rớt đuôi khỉ nữa", "death",
        ], combat.Calls);
        Assert.Equal(["next 0/99", "next 0/99"], random.Calls);
    }

    // Kiểm tra setTemporaryEnemies thêm một lần vào bảy list, không tạo bản sao (Java: setTemporaryEnemies).
    [Fact]
    public void Temporary_enemies_are_added_once_to_every_family_list()
    {
        Player victim = MakePlayer();
        Player attacker = MakePlayer();

        victim.SetTemporaryEnemies(attacker);
        victim.SetTemporaryEnemies(attacker);

        Assert.All(
        [
            victim.TemporaryEnemies, victim.FatherTemporaryEnemies, victim.MotherTemporaryEnemies, victim.LoverTemporaryEnemies,
            victim.ChildOneTemporaryEnemies, victim.ChildTwoTemporaryEnemies, victim.ChildThreeTemporaryEnemies,
        ], list => Assert.Equal([attacker], list));
    }

    // Tạo Player tối thiểu với component combat do constructor khởi tạo (Java: Player()).
    private static Player MakePlayer(long hp = 1_000, long hpMax = 1_000)
    {
        Player player = new() { IsPlayer = true };
        player.NPoint.Hp = hp;
        player.NPoint.HpMax = hpMax;
        player.NPoint.Mp = 100;
        player.NPoint.MpMax = 100;
        return player;
    }

    // Fake clock trả một mốc bất biến (Java: System.currentTimeMillis).
    private sealed class FakeClock(long nowMilliseconds) : IPlayerClock
    {
        // Trả mili giây epoch đã fixture (Java: System.currentTimeMillis).
        public long NowMilliseconds { get; } = nowMilliseconds;
    }

    // Fake random tiêu thụ kết quả đúng thứ tự Java gọi (Java: Util.isTrue/nextInt).
    private sealed class FakeRandom(params int[] results) : IPlayerRandom
    {
        // Ghi thứ tự random để assertion (Java: Util.isTrue/nextInt).
        public List<string> Calls { get; } = [];

        // Chứa kết quả kế tiếp của nextInt (Java: Util.nextInt).
        private readonly Queue<int> values = new(results);

        // Trả kết quả nextInt đã fixture và ghi đoạn đóng (Java: Util.nextInt).
        public int NextInt(int minInclusive, int maxInclusive)
        {
            Calls.Add($"next {minInclusive}/{maxInclusive}");
            return values.Count > 0 ? values.Dequeue() : minInclusive;
        }
    }

    // Fake cổng world/service ghi thứ tự side effect combat (Java: Service, PlayerService, MapService và event).
    private sealed class FakeCombat : IPlayerCombatGateway
    {
        // Ghi mọi side effect combat để assertion (Java: singleton services).
        public List<string> Calls { get; } = [];

        // Bật fixture bùa bất tử (Java: charms.tdBatTu).
        public bool Immortal { get; init; }

        // Bật fixture task đuôi khỉ (Java: taskMain.id >= 19).
        public bool CanDropTail { get; init; }

        // Giữ cắt CrisGH theo long cho golden số nhỏ (Java: Manager.readInt).
        public bool ReadInt => false;

        // Mặc định sự kiện Halloween tắt (Java: EventManager.HALLOWEEN).
        public bool IsHalloweenEvent => false;

        // Ghi đặt Halloween (Java: EffectSkillService.setIsHalloween).
        public void SetHalloween(Player player, int outfitId, int durationMilliseconds) => Calls.Add($"halloween {outfitId}");

        // Mặc định không ở Mabư 12 giờ (Java: MapService.isMapMaBu12H).
        public bool IsMabuTwelveHourMap(Player player) => false;

        // Ghi hồi HP (Java: PlayerService.hoiPhuc).
        public void RestoreHp(Player player, long hp) => Calls.Add($"heal {hp}");

        // Mặc định selected skill không phải chưởng (Java: SkillUtil.isUseSkillChuong).
        public bool IsBlastSkill(Player player) => false;

        // Ghi phá khiên (Java: EffectSkillService.breakShield).
        public void BreakShield(Player player) => Calls.Add("break-shield");

        // Ghi sát thương phản khiên (Java: iDMark.setDamePST).
        public void SetShieldReflectDamage(Player player, long damage) => Calls.Add($"reflect {damage}");

        // Trả fixture bùa bất tử (Java: charms.tdBatTu).
        public bool IsImmortalCharmActive(Player player, long nowMilliseconds) => Immortal;

        // Mặc định không ở map giữ Mabư (Java: map 128).
        public bool IsMabuHoldMap(Player player) => false;

        // Mặc định không cùng bang (Java: clan reference equality).
        public bool AreSameClan(Player first, Player second) => false;

        // Ghi chat riêng (Java: Service.chatJustForMe).
        public void SendPrivateChat(Player sender, Player receiver, string text) => Calls.Add($"chat {text}");

        // Mặc định không ở phó bản (Java: MapService.isMapPhoBan).
        public bool IsDungeonMap(Player player) => false;

        // Ghi thông báo (Java: Service.sendThongBao).
        public void SendNotification(Player player, string text) => Calls.Add($"notify {text}");

        // Không ghi map 51 trong fixture này (Java: totalDamageTaken).
        public void RecordDamageTakenOnMap51(Player player, double damage) { }

        // Mặc định không ở Đại hội võ thuật 23 (Java: The23rdMartialArtCongress.die).
        public bool HandleMartialArtCongressDeath(Player player) => false;

        // Mặc định không có costume Fide leader (Java: setClothes.ctFideDaiCa).
        public bool HasFriezaLeaderCostume(Player player) => false;

        // Mặc định không có costume Fide minion (Java: setClothes.ctDanEmFide).
        public bool HasFriezaMinionCostume(Player player) => false;

        // Mặc định không có cấp Broly 5 (Java: setClothes.broly).
        public int GetBrolyLevel(Player player) => 0;

        // Trả fixture điều kiện task đuôi khỉ (Java: taskMain.id >= 19).
        public bool CanDropMonkeyTail(Player victim, Player attacker) => CanDropTail;

        // Ghi vật phẩm rơi (Java: Service.dropItemMap).
        public void DropCombatItem(Player victim, int itemTemplateId, int quantity, Player owner) => Calls.Add($"drop {itemTemplateId}");

        // Ghi marker persistence thay DB (Java: ConnectDB.executeUpdate).
        public void MarkTailDropTimeDirty(Player player) => Calls.Add("tail-dirty");

        // Mặc định không rơi item event (Java: MapService predicates).
        public bool CanDropDeathItems(Player player) => false;

        // Ghi handoff chết bom (Java: setBom).
        public void TriggerBombDeath(Player victim, Player? attacker) => Calls.Add("bomb-death");

        // Ghi handoff chết thường (Java: setDie).
        public void TriggerPlayerDeath(Player victim, Player? attacker) => Calls.Add("death");
    }
}
