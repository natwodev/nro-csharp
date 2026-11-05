using Nro.Core.IO;
using Nro.Core.Templates;
using Nro.Game.Items.Core;
using Nro.Game.Players;
using Nro.Game.Players.Ports;
using Nro.Game.Players.Sessions;

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

    private static Player NewPlayerWithBodyItems()
    {
        Player player = new() { IsPlayer = true };
        player.Inventory.ItemsBody = Enumerable.Repeat(new Item(), 20).ToList();
        return player;
    }

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
}
