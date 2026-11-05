using Nro.Core.IO;
using Nro.Core.Consts;
using Nro.Core.Templates;
using Nro.Game.Items.Core;
using Nro.Game.Players;
using Nro.Game.Players.Ports;
using Nro.Game.Players.Sessions;
using Nro.Game.Skills;

namespace Nro.Tests.Unit.Game.Players;

public sealed class PlayerIdentityAppearanceTests
{
    [Fact]
    public void Classification_DistinguishesPlayerPetBotAndFamily()
    {
        Player player = new() { IsPlayer = true, Id = -42 };
        Player pet = new() { IsPlayer = true, IsPet = true };
        Player bot = new() { IsBot_New = true };
        Player family = new() { IsFather = true };

        Assert.True(player.IsPl());
        Assert.True(player.IsPlAndBot());
        Assert.True(player.IsPlMan());
        Assert.False(player.GetBot());
        Assert.Equal(-42L, player.Id);

        Assert.False(pet.IsPl());
        Assert.False(pet.IsPlAndBot());
        Assert.True(pet.IsPlMan());
        Assert.False(pet.GetBot());

        Assert.False(bot.IsPl());
        Assert.True(bot.IsPlAndBot());
        Assert.False(bot.IsPlMan());
        Assert.True(bot.GetBot());

        Assert.False(family.IsPl());
        Assert.False(family.IsPlAndBot());
        Assert.True(family.IsPlMan());
    }

    [Fact]
    public void Classification_ExcludesNonInteractiveNpcAndKeepsJavaMasterKinds()
    {
        NonInteractiveNpcPlayer npc = new() { IsPlayer = true };
        Assert.False(npc.IsPl());
        Assert.False(npc.IsPlAndBot());
        Assert.False(npc.IsPlMan());

        Assert.True(new Player { IsPet = true }.IsMaster());
        Assert.True(new Player { IsFather = true }.IsMaster());
        Assert.True(new Player { IsMother = true }.IsMaster());
        Assert.True(new Player { IsPetFollow = true }.IsMaster());
        Assert.True(new Player { IsTangMonk = true }.IsMaster());
        Assert.True(new Player { IsSaibamen = true }.IsMaster());
        Assert.True(new Player { IsBodyClone = true }.IsMaster());
        Assert.True(new Player { IsLover = true }.IsMaster());
    }

    [Fact]
    public void SetIdForPet_UsesJavaOffsetsAndNegativePetId()
    {
        Assert.Equal(-9L, Player.SetIdForPet(new Player { IsPet = true }, 9));
        Assert.Equal(9L, Player.SetIdForPet(new Player { IsPet = true }, -9));
        Assert.Equal(-999_999_991L, Player.SetIdForPet(new Player { IsFather = true }, 9));
        Assert.Equal(-1_999_999_991L, Player.SetIdForPet(new Player { IsMother = true }, 9));
        Assert.Equal(-2_999_999_991L, Player.SetIdForPet(new Player { IsLover = true }, 9));
        Assert.Equal(9L, Player.SetIdForPet(new Player(), 9));
    }

    [Fact]
    public void IsDead_UsesHpAndTreatsMissingNPointAsDead()
    {
        Player player = new();
        player.NPoint.Hp = 1;
        Assert.False(player.IsDead());

        player.NPoint.Hp = 0;
        Assert.True(player.IsDead());

        player.NPoint = null!;
        Assert.True(player.IsDead());
    }

    [Fact]
    public void SessionQueries_AreFalseWithoutSessionAndUseGatewayWhenPresent()
    {
        Player player = new() { IsPlayer = true };
        Assert.False(player.IsActive());
        Assert.False(player.IsFounder());
        Assert.False(player.IsJail());
        Assert.False(player.IsModerator());

        player.SetSession(new MySession());
        Assert.False(player.IsActive());
        Assert.False(player.IsFounder());
        Assert.False(player.IsJail());
        Assert.False(player.IsModerator());

        RecordingSessionGateway gateway = new() { Active = true, Founder = true, Jail = true, Moderator = true };
        MySession session = new();
        player.SessionGateway = gateway;
        player.SetSession(session);

        Assert.Same(session, player.GetSession());
        Assert.True(player.IsActive());
        Assert.True(player.IsFounder());
        Assert.True(player.IsJail());
        Assert.True(player.IsModerator());

        player.SendMessage(new Message(7));
        Assert.Single(gateway.Messages);
    }

    [Fact]
    public void GetMaster_ReturnsOwnerOnlyForJavaCompanionKinds()
    {
        Player owner = new();
        Assert.Same(owner, new Player { IsPet = true, Owner = owner }.GetMaster());
        Assert.Same(owner, new Player { IsPetFollow = true, Owner = owner }.GetMaster());
        Assert.Same(owner, new Player { IsTangMonk = true, Owner = owner }.GetMaster());
        Assert.Same(owner, new Player { IsSaibamen = true, Owner = owner }.GetMaster());
        Assert.Same(owner, new Player { IsBodyClone = true, Owner = owner }.GetMaster());
        Player father = new() { IsFather = true, Owner = owner };
        Assert.Same(father, father.GetMaster());
    }

    [Theory]
    [InlineData(8, 8)]
    [InlineData(7, 7)]
    [InlineData(6, 6)]
    [InlineData(5, 5)]
    [InlineData(4, 4)]
    [InlineData(3, -1)]
    public void GetFrontEffect_MapsEveryJavaThreshold(int level, int expected)
    {
        Player player = NewPlayerWithBodyItems();
        for (int i = 0; i < 5; i++)
        {
            player.Inventory.ItemsBody[i] = ItemWithUpgradeLevel(level);
        }

        Assert.Equal((sbyte)expected, player.GetFrontEffect());
    }

    [Fact]
    public void GetFrontEffect_RequiresTwentySlotsAndFiveFirstOptions()
    {
        Player shortList = new() { IsPlayer = true };
        shortList.Inventory.ItemsBody = Enumerable.Repeat(new Item(), 19).ToList();
        Assert.Equal((sbyte)-1, shortList.GetFrontEffect());

        Player nullItem = NewPlayerWithBodyItems();
        nullItem.Inventory.ItemsBody[2] = null!;
        Assert.Equal((sbyte)-1, nullItem.GetFrontEffect());

        Player missingOption = NewPlayerWithBodyItems();
        for (int i = 0; i < 5; i++)
        {
            missingOption.Inventory.ItemsBody[i] = ItemWithUpgradeLevel(8);
        }

        missingOption.Inventory.ItemsBody[4] = new Item();
        Assert.Equal((sbyte)-1, missingOption.GetFrontEffect());
    }

    [Fact]
    public void GetFrontEffect_UsesFirstOption72LikeJava()
    {
        Player player = NewPlayerWithBodyItems();
        for (int i = 0; i < 5; i++)
        {
            player.Inventory.ItemsBody[i] = ItemWithUpgradeLevel(8);
        }

        player.Inventory.ItemsBody[0] = new Item
        {
            ItemOptions =
            [
                new ItemOption(new ItemOptionTemplate(72, "", 0), 4),
                new ItemOption(new ItemOptionTemplate(72, "", 0), 8),
            ],
        };

        Assert.Equal((sbyte)4, player.GetFrontEffect());
    }

    [Fact]
    public void GetFrontEffect_SkipsOtherOptionsAndUsesLowestEquipmentLevel()
    {
        Player player = NewPlayerWithBodyItems();
        player.Inventory.ItemsBody[0] = new Item
        {
            ItemOptions =
            [
                new ItemOption(new ItemOptionTemplate(71, "", 0), 99),
                new ItemOption(new ItemOptionTemplate(72, "", 0), 8),
            ],
        };
        player.Inventory.ItemsBody[1] = ItemWithUpgradeLevel(8);
        player.Inventory.ItemsBody[2] = ItemWithUpgradeLevel(8);
        player.Inventory.ItemsBody[3] = ItemWithUpgradeLevel(8);
        player.Inventory.ItemsBody[4] = ItemWithUpgradeLevel(7);

        Assert.Equal((sbyte)7, player.GetFrontEffect());
    }

    [Fact]
    public void PetOutfit_ReturnsPartsOrMinusOneForInvalidInputs()
    {
        Player player = NewPlayerWithBodyItems();
        player.Inventory.ItemsBody[7] = new Item
        {
            Template = new ItemTemplate { Head = 10, Body = 20, Leg = 30 },
        };
        Assert.Equal((short)10, player.GetPetHead());
        Assert.Equal((short)20, player.GetPetBody());
        Assert.Equal((short)30, player.GetPetLeg());

        player.Inventory.ItemsBody[7] = null!;
        Assert.Equal((short)-1, player.GetPetHead());

        player.Inventory.ItemsBody = Enumerable.Repeat(new Item(), 7).ToList();
        Assert.Equal((short)-1, player.GetPetBody());

        player.Inventory.ItemsBody = Enumerable.Repeat(new Item(), 20).ToList();
        player.IsPlayer = false;
        Assert.Equal((short)-1, player.GetPetLeg());
    }

    [Fact]
    public void IsTargetForPet_MatchesEveryJavaBranch()
    {
        Player caller = new();
        Assert.True(caller.IsTargetForPet(new Player { IsPetFollow = true }));
        Assert.True(caller.IsTargetForPet(new Player { IsTangMonk = true }));
        Assert.True(caller.IsTargetForPet(new Player { IsBoss = true }));
        Assert.True(caller.IsTargetForPet(new Player { IsPlayer = true }));
        Assert.False(caller.IsTargetForPet(new Player { IsPet = true }));
    }

    [Theory]
    [InlineData(0, 380, 381, 382)]
    [InlineData(1, 391, 392, 393)]
    [InlineData(2, 380, 381, 382)]
    public void Appearance_FusionWinsOverCostumeAndEquipmentForEveryGender(int gender, int expectedHead, int expectedBody, int expectedLeg)
    {
        Player player = NewAppearancePlayer((sbyte)gender);
        player.Inventory.ItemsBody[0] = OutfitItem(part: 900);
        player.Inventory.ItemsBody[1] = OutfitItem(part: 901);
        player.Inventory.ItemsBody[5] = OutfitItem(id: 500, head: 100, body: 101, leg: 102);
        player.AppearanceFusionType = ConstPlayer.FUSION_DANCE;

        Assert.Equal((short)expectedHead, player.GetHead());
        Assert.Equal((short)expectedBody, player.GetBody());
        Assert.Equal((short)expectedLeg, player.GetLeg());

        player.FusionDisplayState = 0;
        Assert.Equal((short)100, player.GetHead());
        Assert.Equal((short)101, player.GetBody());
        Assert.Equal((short)102, player.GetLeg());
    }

    [Fact]
    public void Appearance_EffectsAndMabuKeepTheirJavaPriorityOverFusionAndEquipment()
    {
        Player player = NewAppearancePlayer(ConstPlayer.EARTHLING);
        player.Inventory.ItemsBody[5] = OutfitItem(id: 601, head: 99, body: 98, leg: 97);
        player.AppearanceFusionType = ConstPlayer.PORATA_FUSION;
        player.EffectSkill.IsChocolate = true;
        player.EffectSkin.IsRabbitLeader = true;

        Assert.Equal((short)412, player.GetHead());
        Assert.Equal((short)413, player.GetBody());
        Assert.Equal((short)414, player.GetLeg());

        player.EffectSkill.IsChocolate = false;
        player.IsMabuMapProtection = true;
        Assert.Equal((short)406, player.GetHead());
        Assert.Equal((short)407, player.GetBody());
        Assert.Equal((short)408, player.GetLeg());

        player.EffectSkin.IsRabbitLeader = false;
        Assert.Equal((short)566, player.GetHead());
        Assert.Equal((short)472, player.GetBody());
        Assert.Equal((short)568, player.GetLeg());
    }

    [Fact]
    public void Appearance_MonkeyTailUsesInjectedRandomAndAuraUsesCostumeThenUsedRadarThenPower()
    {
        Player player = NewAppearancePlayer(ConstPlayer.EARTHLING);
        player.IsMonkeyTailActive = true;
        Assert.Equal((short)198, player.GetHead(new FixedPlayerRandom(6)));
        Assert.Equal((short)193, player.GetBody());
        Assert.Equal((short)194, player.GetLeg());

        player.IsMonkeyTailActive = false;
        player.Inventory.ItemsBody[5] = OutfitItem(id: 2014);
        player.NPoint.Power = 180_000_000_000L;
        Assert.Equal((sbyte)14, player.GetAura([(12, (sbyte)1)], new Dictionary<int, sbyte> { [12] = 42 }));

        player.Inventory.ItemsBody[5] = new Item();
        Assert.Equal((sbyte)42, player.GetAura([(12, (sbyte)1), (13, (sbyte)1)], new Dictionary<int, sbyte> { [12] = 42, [13] = 43 }));
        Assert.Equal((sbyte)84, player.GetAura());
    }

    [Theory]
    [InlineData(0, 545, 548, 549)]
    [InlineData(1, 547, 548, 549)]
    [InlineData(2, 546, 548, 549)]
    public void Appearance_HalloweenEffectWinsOverCostumeFusionAndEquipmentForEveryGender(int gender, int head, int body, int leg)
    {
        Player player = NewAppearancePlayer((sbyte)gender);
        player.Inventory.ItemsBody[5] = OutfitItem(id: 601, head: 100, body: 101, leg: 102);
        player.AppearanceFusionType = ConstPlayer.PORATA_FUSION;
        player.EffectSkill.IsHalloween = true;
        player.EffectSkill.HalloweenOutfitId = 2;

        Assert.Equal((short)head, player.GetHead());
        Assert.Equal((short)body, player.GetBody());
        Assert.Equal((short)leg, player.GetLeg());
    }

    [Theory]
    [InlineData(79_999_999_999L, -1)]
    [InlineData(80_000_000_000L, 6)]
    [InlineData(100_000_000_000L, 6)]
    [InlineData(110_000_000_000L, 82)]
    [InlineData(120_000_000_000L, 83)]
    [InlineData(180_000_000_000L, 84)]
    public void AuraPower_UsesEveryJavaThreshold(long power, int aura)
    {
        Player player = NewAppearancePlayer(ConstPlayer.EARTHLING);
        player.NPoint.Power = power;
        Assert.Equal((sbyte)aura, player.GetAuraPower());
    }

    [Theory]
    [InlineData(0, 27, 1, 1436, 1437, 1438)]
    [InlineData(1, 28, 6, 1447, 1431, 1432)]
    [InlineData(2, 29, 3, 1448, 1434, 1435)]
    public void SuperOutfit_UsesJavaSkillIndexAndReturnsMinusOneOutsideTheSixEntryRange(int gender, int skillId, int point, int head, int body, int leg)
    {
        Player player = NewAppearancePlayer((sbyte)gender);
        player.PlayerSkill.Skills!.Add(new Skill { Template = new SkillTemplate { Id = (sbyte)skillId }, Point = point });
        player.NumUseSkill = 0;

        Assert.Equal((short)head, player.GetHeadSuper());
        Assert.Equal((short)body, player.GetBodySuper());
        Assert.Equal((short)leg, player.GetLegSuper());

        player.NumUseSkill = point;
        Assert.Equal((short)-1, player.GetHeadSuper());
        Assert.Equal((short)-1, player.GetBodySuper());
        Assert.Equal((short)-1, player.GetLegSuper());
    }

    [Fact]
    public void FlagBagAndMount_KeepJavaBranchOrderAndInjectedMapLookup()
    {
        Player player = NewAppearancePlayer(ConstPlayer.EARTHLING);
        player.IdNRNM = 353;
        player.Inventory.ItemsBody[8] = OutfitItem(part: 77);
        Assert.Equal((short)31, player.GetFlagBag(true, ConstTask.TASK_3_2, 66));
        Assert.Equal((short)30, player.GetFlagBag(false, ConstTask.TASK_3_2, 66));

        player.IdNRNM = -1;
        Assert.Equal((short)77, player.GetFlagBag(false, ConstTask.TASK_3_2, 66));
        player.Inventory.ItemsBody[8] = new Item();
        Assert.Equal((short)28, player.GetFlagBag(false, ConstTask.TASK_3_2, 66));
        Assert.Equal((short)66, player.GetFlagBag(false, 0, 66));

        player.Inventory.ItemsBody[9] = OutfitItem(id: 499, type: 0);
        Assert.Equal((short)499, player.GetMount());
        player.Inventory.ItemsBody[9] = OutfitItem(id: 700, type: 0);
        Assert.Equal((short)17, player.GetMount(new Dictionary<int, short> { [700] = 17 }));
        player.Inventory.ItemsBody[9] = OutfitItem(id: 701, type: 24, gender: 1);
        Assert.Equal((short)-1, player.GetMount());
    }

    [Fact]
    public void DailyGiftAndHpMp_UseFakeNotifierAtJavaIntervals()
    {
        Player player = NewAppearancePlayer(ConstPlayer.NAMEKIAN);
        var notifier = new RecordingClientNotifier { FreeCharmAvailable = true, DailyCheckInAvailable = true };

        player.SendDailyGiftReminders(300_000, notifier);
        Assert.Equal((sbyte)6, Assert.Single(notifier.TimedTexts).TextId);
        Assert.Contains("Ông Moori", notifier.TimedTexts[0].Text);
        Assert.Equal(0L, player.LastTimeSendTextTime);
        Assert.Equal(300_000L, player.LastTimeSendTextTime_2);

        player.SendDailyGiftReminders(300_001, notifier);
        Assert.Equal(2, notifier.TimedTexts.Count);
        Assert.Equal((sbyte)5, notifier.TimedTexts[1].TextId);
        Assert.Equal(300_001L, player.LastTimeSendTextTime);
        Assert.Equal(300_000L, player.LastTimeSendTextTime_2);

        player.NPoint.Hp = 123;
        player.NPoint.Mp = 456;
        player.SendInfoHpMp(notifier);
        Assert.Equal(new[] { (123L, 456L) }, notifier.HpMp);

        player.IsPlayer = false;
        player.SendInfoHpMp(notifier);
        Assert.Single(notifier.HpMp);
    }

    private static Player NewPlayerWithBodyItems()
    {
        Player player = new() { IsPlayer = true };
        player.Inventory.ItemsBody = Enumerable.Repeat(new Item(), 20).ToList();
        return player;
    }

    private static Player NewAppearancePlayer(sbyte gender)
    {
        Player player = new() { IsPlayer = true, Gender = gender };
        player.Inventory.ItemsBody = Enumerable.Range(0, 12).Select(_ => new Item()).ToList();
        return player;
    }

    private static Item OutfitItem(short id = 0, sbyte type = 0, sbyte gender = 3, short part = -1, int head = -1, int body = -1, int leg = -1) => new()
    {
        Template = new ItemTemplate { Id = id, Type = type, Gender = gender, Part = part, Head = head, Body = body, Leg = leg },
    };

    private static Item ItemWithUpgradeLevel(int level) => new()
    {
        ItemOptions = [new ItemOption(new ItemOptionTemplate(72, "", 0), level)],
    };

    private sealed class NonInteractiveNpcPlayer : Player, INonInteractiveNpcPlayer
    {
    }

    private sealed class RecordingSessionGateway : IPlayerSessionGateway
    {
        public bool Active { get; init; }

        public bool Founder { get; init; }

        public bool Jail { get; init; }

        public bool Moderator { get; init; }

        public List<Message> Messages { get; } = [];

        public void Send(MySession session, Message message) => Messages.Add(message);

        public bool IsActive(MySession session) => Active;

        public bool IsFounder(MySession session) => Founder;

        public bool IsJail(MySession session) => Jail;

        public bool IsModerator(MySession session) => Moderator;
    }

    private sealed class FixedPlayerRandom(int result) : IPlayerRandom
    {
        public int NextInt(int minInclusive, int maxInclusive)
        {
            Assert.Equal(0, minInclusive);
            Assert.Equal(6, maxInclusive);
            return result;
        }
    }

    private sealed class RecordingClientNotifier : IPlayerClientNotifier
    {
        public bool FreeCharmAvailable { get; init; }

        public bool DailyCheckInAvailable { get; init; }

        public List<(sbyte TextId, string Text, sbyte Seconds)> TimedTexts { get; } = [];

        public List<(long Hp, long Mp)> HpMp { get; } = [];

        public bool CanReceiveDailyGift(Player player, sbyte dailyGiftId) => dailyGiftId == ConstDailyGift.FREE_CHARM ? FreeCharmAvailable : DailyCheckInAvailable;

        public void SendTimedText(Player player, sbyte textId, string text, sbyte seconds) => TimedTexts.Add((textId, text, seconds));

        public void SendCurrentHp(Player player, long hp) => HpMp.Add((hp, -1));

        public void SendCurrentMp(Player player, long mp)
        {
            (long hp, _) = HpMp[^1];
            HpMp[^1] = (hp, mp);
        }

        public void ReportNotificationFailure(Player player, Exception exception)
        {
        }
    }
}
