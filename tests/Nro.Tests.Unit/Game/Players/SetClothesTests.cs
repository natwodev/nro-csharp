using Nro.Core.Templates;
using Nro.Game.Items.Core;
using Nro.Game.Players.Stats;
using Nro.Game.Skills;

namespace Nro.Tests.Unit.Game.Players;

public sealed class SetClothesTests
{
    public static IEnumerable<object[]> ActivationSets()
    {
        yield return [nameof(SetClothes.Songoku), 129, 141];
        yield return [nameof(SetClothes.ThienXinHang), 127, 139];
        yield return [nameof(SetClothes.Kirin), 128, 140];
        yield return [nameof(SetClothes.Yamcha), 250, 253];
        yield return [nameof(SetClothes.OcTieu), 131, 254];
        yield return [nameof(SetClothes.PikkoroDaimao), 132, 144];
        yield return [nameof(SetClothes.Picolo), 130, 143];
        yield return [nameof(SetClothes.Slug), 251, 142];
        yield return [nameof(SetClothes.Nappa), 135, 138];
        yield return [nameof(SetClothes.Kakarot), 133, 136];
        yield return [nameof(SetClothes.Cadic), 134, 137];
        yield return [nameof(SetClothes.Broly), 252, 255];
        yield return [nameof(SetClothes.Gohan), 233, 234];
        yield return [nameof(SetClothes.Goten), 263, 264];
        yield return [nameof(SetClothes.Frieza), 265, 266];
        yield return [nameof(SetClothes.Cumber), 267, 268];
        yield return [nameof(SetClothes.ThanVuTruKaio), 245, 248];
        yield return [nameof(SetClothes.Nail), 237, 240];
        yield return [nameof(SetClothes.CadicM), 241, 244];
        yield return [nameof(SetClothes.DestructionGodChampa), 269, 272];
    }

    public static IEnumerable<object[]> Costumes()
    {
        foreach (int id in new[] { 618, 619, 620, 621, 622, 623, 624, 626, 627 }) yield return [nameof(SetClothes.CostumePirate), id];
        yield return [nameof(SetClothes.CostumeTanjiro), 1087];
        yield return [nameof(SetClothes.CostumeInosukeHashibira), 1088];
        yield return [nameof(SetClothes.CostumeInosuke), 1089];
        yield return [nameof(SetClothes.CostumeZenitsu), 1090];
        yield return [nameof(SetClothes.CostumeNezuko), 1091];
        foreach (int id in new[] { 1208, 1209, 1210 }) yield return [nameof(SetClothes.CostumeBulmaColoredHair), id];
        foreach (int id in new[] { 405, 406, 407, 629 }) yield return [nameof(SetClothes.CostumeFriezaBoss), id];
        foreach (int id in new[] { 429, 430, 431, 432, 433 }) yield return [nameof(SetClothes.CostumeFriezaMinion), id];
        foreach (int id in new[] { 1234, 1235, 1236 }) yield return [nameof(SetClothes.CostumeBeachParty), id];
    }

    [Theory]
    [MemberData(nameof(ActivationSets))]
    public void Setup_CountsEachActivationSetFromEitherJavaOption(string property, int firstOption, int alternateOption)
    {
        Player player = NewPlayer();
        for (int i = 0; i < 5; i++)
        {
            player.Inventory.ItemsBody[i] = ItemWithOption(i % 2 == 0 ? firstOption : alternateOption);
        }

        player.SetClothes.Setup();

        Assert.Equal((sbyte)5, Count(player.SetClothes, property));
    }

    [Fact]
    public void Setup_UsesTheFirstActivationSetOptionOnEachItemAndCountsMixedSets()
    {
        Player player = NewPlayer();
        player.Inventory.ItemsBody[0] = ItemWithOptions((129, 0), (127, 0));
        player.Inventory.ItemsBody[1] = ItemWithOption(129);
        player.Inventory.ItemsBody[2] = ItemWithOption(129);
        player.Inventory.ItemsBody[3] = ItemWithOption(133);
        player.Inventory.ItemsBody[4] = ItemWithOption(133);

        player.SetClothes.Setup();

        Assert.Equal((sbyte)3, player.SetClothes.Songoku);
        Assert.Equal((sbyte)0, player.SetClothes.ThienXinHang);
        Assert.Equal((sbyte)2, player.SetClothes.Kakarot);
    }

    [Fact]
    public void Setup_CountsEveryDestructionSetOptionBecauseJavaDoesNotBreakForOption21()
    {
        Player player = NewPlayer();
        for (int i = 0; i < 5; i++)
        {
            player.Inventory.ItemsBody[i] = ItemWithOptions((21, 80), (21, 79), (21, 80));
        }

        player.SetClothes.Setup();

        Assert.Equal((sbyte)10, player.SetClothes.DestructionSet);
    }

    [Theory]
    [MemberData(nameof(Costumes))]
    public void Setup_ClassifiesEveryJavaCostumeTemplate(string property, int templateId)
    {
        Player player = NewPlayer();
        player.Inventory.ItemsBody[5] = ItemWithTemplate(templateId);

        player.SetClothes.Setup();

        Assert.Equal(templateId, Costume(player.SetClothes, property));
    }

    [Fact]
    public void Setup_ResetsCountersCostumesWorldcupAndGodStateBeforeEachScan()
    {
        Player player = NewPlayer();
        for (int i = 0; i < 5; i++)
        {
            player.Inventory.ItemsBody[i] = ItemWithTemplate(555 + i, 129);
        }
        player.Inventory.ItemsBody[5] = ItemWithTemplate(1091);
        player.SetClothes.Setup();
        player.SetClothes.Worldcup = 2;

        player.Inventory.ItemsBody[2] = new Item();
        player.Inventory.ItemsBody[5] = new Item();
        player.SetClothes.Setup();

        Assert.Equal((sbyte)2, player.SetClothes.Songoku);
        Assert.Equal((sbyte)0, player.SetClothes.Worldcup);
        Assert.Equal(-1, player.SetClothes.CostumeNezuko);
        Assert.False(player.SetClothes.GodClothes);
    }

    [Fact]
    public void GodAndDestructionChecksRequireFiveNonEmptyItemsInsideTheirJavaRanges()
    {
        Player player = NewPlayer();
        for (int i = 0; i < 5; i++) player.Inventory.ItemsBody[i] = ItemWithTemplate(555 + i);
        Assert.True(player.SetClothes.CheckSetGod());
        Assert.False(player.SetClothes.CheckSetDes());

        for (int i = 0; i < 5; i++) player.Inventory.ItemsBody[i] = ItemWithTemplate(650 + i);
        Assert.True(player.SetClothes.CheckSetDes());
        Assert.False(player.SetClothes.CheckSetGod());

        player.Inventory.ItemsBody[4] = new Item();
        Assert.False(player.SetClothes.CheckSetDes());
    }

    [Fact]
    public void PlayerNPointHost_CapturesAndCommitsThePortedSetClothesState()
    {
        Player player = NewPlayer();
        for (int i = 0; i < 5; i++) player.Inventory.ItemsBody[i] = ItemWithOption(267);
        player.SetClothes.Setup();

        PlayerNPointHost host = new();
        NPointFacts facts = host.Capture(player.NPoint);
        player.PlayerSkill.SkillSelect = new Skill();
        NPointAttackFacts attackFacts = host.CaptureAttack(player.NPoint);
        facts.SetClothes.Worldcup = 2;
        host.Commit(player.NPoint, facts);

        Assert.Equal((sbyte)5, facts.SetClothes.Cumber);
        Assert.Equal((sbyte)5, attackFacts.SetClothes.Cumber);
        Assert.Equal((sbyte)2, player.SetClothes.Worldcup);
    }

    [Fact]
    public void Dispose_ReleasesOwnerLikeJavaDispose()
    {
        Player player = NewPlayer();
        player.SetClothes.Dispose();

        Assert.Null(player.SetClothes.Player);
        Assert.Throws<ObjectDisposedException>(() => player.SetClothes.Setup());
    }

    private static Player NewPlayer()
    {
        Player player = new();
        player.Inventory.ItemsBody = Enumerable.Range(0, 6).Select(_ => new Item()).ToList();
        return player;
    }

    private static Item ItemWithOption(int optionId) => ItemWithOptions((optionId, 0));

    private static Item ItemWithOptions(params (int Id, int Parameter)[] options) => new()
    {
        Template = new ItemTemplate { Id = 1 },
        ItemOptions = [.. options.Select(option => new ItemOption(new ItemOptionTemplate(option.Id, string.Empty, 0), option.Parameter))],
    };

    private static Item ItemWithTemplate(int templateId, params int[] optionIds) => new()
    {
        Template = new ItemTemplate { Id = (short)templateId },
        ItemOptions = [.. optionIds.Select(id => new ItemOption(new ItemOptionTemplate(id, string.Empty, 0), 0))],
    };

    private static sbyte Count(SetClothes clothes, string property) => property switch
    {
        nameof(SetClothes.Songoku) => clothes.Songoku,
        nameof(SetClothes.ThienXinHang) => clothes.ThienXinHang,
        nameof(SetClothes.Kirin) => clothes.Kirin,
        nameof(SetClothes.Yamcha) => clothes.Yamcha,
        nameof(SetClothes.OcTieu) => clothes.OcTieu,
        nameof(SetClothes.PikkoroDaimao) => clothes.PikkoroDaimao,
        nameof(SetClothes.Picolo) => clothes.Picolo,
        nameof(SetClothes.Slug) => clothes.Slug,
        nameof(SetClothes.Nappa) => clothes.Nappa,
        nameof(SetClothes.Kakarot) => clothes.Kakarot,
        nameof(SetClothes.Cadic) => clothes.Cadic,
        nameof(SetClothes.Broly) => clothes.Broly,
        nameof(SetClothes.Gohan) => clothes.Gohan,
        nameof(SetClothes.Goten) => clothes.Goten,
        nameof(SetClothes.Frieza) => clothes.Frieza,
        nameof(SetClothes.Cumber) => clothes.Cumber,
        nameof(SetClothes.ThanVuTruKaio) => clothes.ThanVuTruKaio,
        nameof(SetClothes.Nail) => clothes.Nail,
        nameof(SetClothes.CadicM) => clothes.CadicM,
        nameof(SetClothes.DestructionGodChampa) => clothes.DestructionGodChampa,
        _ => throw new ArgumentOutOfRangeException(nameof(property)),
    };

    private static int Costume(SetClothes clothes, string property) => property switch
    {
        nameof(SetClothes.CostumePirate) => clothes.CostumePirate,
        nameof(SetClothes.CostumeTanjiro) => clothes.CostumeTanjiro,
        nameof(SetClothes.CostumeInosukeHashibira) => clothes.CostumeInosukeHashibira,
        nameof(SetClothes.CostumeInosuke) => clothes.CostumeInosuke,
        nameof(SetClothes.CostumeZenitsu) => clothes.CostumeZenitsu,
        nameof(SetClothes.CostumeNezuko) => clothes.CostumeNezuko,
        nameof(SetClothes.CostumeBulmaColoredHair) => clothes.CostumeBulmaColoredHair,
        nameof(SetClothes.CostumeFriezaBoss) => clothes.CostumeFriezaBoss,
        nameof(SetClothes.CostumeFriezaMinion) => clothes.CostumeFriezaMinion,
        nameof(SetClothes.CostumeBeachParty) => clothes.CostumeBeachParty,
        _ => throw new ArgumentOutOfRangeException(nameof(property)),
    };
}
