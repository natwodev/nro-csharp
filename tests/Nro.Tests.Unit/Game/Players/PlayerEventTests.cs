// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/PlayerEvent.java (dòng 1–395).
using Nro.Game.Players.Gifts;

namespace Nro.Tests.Unit.Game.Players;

// Kiểm thử trạng thái sự kiện người chơi. (Java: nro/player/PlayerEvent.java)
public sealed class PlayerEventTests
{
    // Kiểm tra toàn bộ giá trị mặc định của trạng thái sự kiện. (Java: các trường dòng 311–336)
    [Fact]
    public void Defaults_are_zero_or_false_and_owner_is_preserved()
    {
        PlayerEvent eventState = new(null!);

        Assert.Null(eventState.Player);
        Assert.All(AllPointValues(eventState), value => Assert.Equal(0, value));
        Assert.False(eventState.IsBathUsed);
        Assert.False(eventState.IsCookingSquareRiceCake);
        Assert.False(eventState.IsCookingCylindricalRiceCake);
        Assert.False(eventState.IsLuckyMoneyReceived);
    }

    // Kiểm tra từng cặp cộng/trừ giữ cả số âm và tràn số theo phép toán int Java. (Java: add*/sub*)
    [Theory]
    [MemberData(nameof(PointOperations))]
    public void Add_and_subtract_preserve_java_unbounded_int_behavior(
        string _,
        Action<PlayerEvent, int> set,
        Func<PlayerEvent, int> read,
        Action<PlayerEvent, int> add,
        Action<PlayerEvent, int> subtract)
    {
        PlayerEvent eventState = new(null!);

        set(eventState, 0);
        add(eventState, -3);
        Assert.Equal(-3, read(eventState));
        subtract(eventState, -3);
        Assert.Equal(0, read(eventState));

        set(eventState, int.MaxValue);
        add(eventState, 1);
        Assert.Equal(int.MinValue, read(eventState));
        subtract(eventState, 1);
        Assert.Equal(int.MaxValue, read(eventState));
    }

    // Kiểm tra các cờ và thời gian nấu bánh được gán đúng. (Java: cookingChungCake, timeCookChungCake, cookingTetCake, timeCookTetCake, receivedLuckyMoney)
    [Fact]
    public void Cooking_flags_and_times_are_mutable()
    {
        PlayerEvent eventState = new(null!)
        {
            IsCookingSquareRiceCake = true,
            SquareRiceCakeCookTime = 120,
            IsCookingCylindricalRiceCake = true,
            CylindricalRiceCakeCookTime = 240,
            IsLuckyMoneyReceived = true,
        };

        Assert.True(eventState.IsCookingSquareRiceCake);
        Assert.Equal(120, eventState.SquareRiceCakeCookTime);
        Assert.True(eventState.IsCookingCylindricalRiceCake);
        Assert.Equal(240, eventState.CylindricalRiceCakeCookTime);
        Assert.True(eventState.IsLuckyMoneyReceived);
    }

    // Kiểm tra update rỗng không thay đổi trạng thái. (Java: update)
    [Fact]
    public void Update_does_not_change_state()
    {
        PlayerEvent eventState = new(null!)
        {
            DestructionPoints = 1,
            NamekWarPoints = 2,
            LunarNewYearEventPoints = 3,
            ChristmasEventPoints = 4,
            HalloweenEventPoints = 5,
            InternationalWomensDayEventPoints = 6,
            MoonFestivalEventPoints = 7,
            HungKingEventPoints = 8,
            BlackFridayEventPoints = 9,
            VietnameseWomensDayEventPoints = 10,
            AccumulatedPoints = 11,
            ReceivedTopUpMilestone = 12,
            EventPoints = 13,
            BossEventPoints = 14,
            NguHanhSonEventPoints = 15,
            MonsterHuntEventPoints = 16,
            KameElderEventPoints = 17,
            EventMilestonePoints = 18,
            IsBathUsed = true,
            IsCookingSquareRiceCake = true,
            SquareRiceCakeCookTime = 19,
            IsCookingCylindricalRiceCake = true,
            CylindricalRiceCakeCookTime = 20,
            IsLuckyMoneyReceived = true,
        };
        int[] expectedPoints = AllPointValues(eventState).ToArray();

        eventState.Update();

        Assert.Equal(expectedPoints, AllPointValues(eventState));
        Assert.True(eventState.IsBathUsed);
        Assert.True(eventState.IsCookingSquareRiceCake);
        Assert.True(eventState.IsCookingCylindricalRiceCake);
        Assert.True(eventState.IsLuckyMoneyReceived);
    }

    // Cung cấp đủ 16 cặp cộng/trừ điểm của Java. (Java: add*/sub*)
    public static IEnumerable<object[]> PointOperations()
    {
        yield return CreatePointOperation(nameof(PlayerEvent.DestructionPoints), (state, value) => state.DestructionPoints = value, state => state.DestructionPoints, (state, value) => state.AddDestructionPoints(value), (state, value) => state.SubtractDestructionPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.NamekWarPoints), (state, value) => state.NamekWarPoints = value, state => state.NamekWarPoints, (state, value) => state.AddNamekWarPoints(value), (state, value) => state.SubtractNamekWarPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.LunarNewYearEventPoints), (state, value) => state.LunarNewYearEventPoints = value, state => state.LunarNewYearEventPoints, (state, value) => state.AddLunarNewYearEventPoints(value), (state, value) => state.SubtractLunarNewYearEventPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.ChristmasEventPoints), (state, value) => state.ChristmasEventPoints = value, state => state.ChristmasEventPoints, (state, value) => state.AddChristmasEventPoints(value), (state, value) => state.SubtractChristmasEventPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.HalloweenEventPoints), (state, value) => state.HalloweenEventPoints = value, state => state.HalloweenEventPoints, (state, value) => state.AddHalloweenEventPoints(value), (state, value) => state.SubtractHalloweenEventPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.InternationalWomensDayEventPoints), (state, value) => state.InternationalWomensDayEventPoints = value, state => state.InternationalWomensDayEventPoints, (state, value) => state.AddInternationalWomensDayEventPoints(value), (state, value) => state.SubtractInternationalWomensDayEventPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.MoonFestivalEventPoints), (state, value) => state.MoonFestivalEventPoints = value, state => state.MoonFestivalEventPoints, (state, value) => state.AddMoonFestivalEventPoints(value), (state, value) => state.SubtractMoonFestivalEventPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.HungKingEventPoints), (state, value) => state.HungKingEventPoints = value, state => state.HungKingEventPoints, (state, value) => state.AddHungKingEventPoints(value), (state, value) => state.SubtractHungKingEventPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.BlackFridayEventPoints), (state, value) => state.BlackFridayEventPoints = value, state => state.BlackFridayEventPoints, (state, value) => state.AddBlackFridayEventPoints(value), (state, value) => state.SubtractBlackFridayEventPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.VietnameseWomensDayEventPoints), (state, value) => state.VietnameseWomensDayEventPoints = value, state => state.VietnameseWomensDayEventPoints, (state, value) => state.AddVietnameseWomensDayEventPoints(value), (state, value) => state.SubtractVietnameseWomensDayEventPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.EventPoints), (state, value) => state.EventPoints = value, state => state.EventPoints, (state, value) => state.AddEventPoints(value), (state, value) => state.SubtractEventPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.BossEventPoints), (state, value) => state.BossEventPoints = value, state => state.BossEventPoints, (state, value) => state.AddBossEventPoints(value), (state, value) => state.SubtractBossEventPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.NguHanhSonEventPoints), (state, value) => state.NguHanhSonEventPoints = value, state => state.NguHanhSonEventPoints, (state, value) => state.AddNguHanhSonEventPoints(value), (state, value) => state.SubtractNguHanhSonEventPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.MonsterHuntEventPoints), (state, value) => state.MonsterHuntEventPoints = value, state => state.MonsterHuntEventPoints, (state, value) => state.AddMonsterHuntEventPoints(value), (state, value) => state.SubtractMonsterHuntEventPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.KameElderEventPoints), (state, value) => state.KameElderEventPoints = value, state => state.KameElderEventPoints, (state, value) => state.AddKameElderEventPoints(value), (state, value) => state.SubtractKameElderEventPoints(value));
        yield return CreatePointOperation(nameof(PlayerEvent.EventMilestonePoints), (state, value) => state.EventMilestonePoints = value, state => state.EventMilestonePoints, (state, value) => state.AddEventMilestonePoints(value), (state, value) => state.SubtractEventMilestonePoints(value));
    }

    // Đóng gói một cặp mutator để MemberData có đúng kiểu delegate. (Java: add*/sub*)
    private static object[] CreatePointOperation(string name, Action<PlayerEvent, int> set, Func<PlayerEvent, int> read, Action<PlayerEvent, int> add, Action<PlayerEvent, int> subtract) => [name, set, read, add, subtract];

    // Trả mọi giá trị điểm để xác nhận giá trị mặc định. (Java: các trường int của PlayerEvent)
    private static IEnumerable<int> AllPointValues(PlayerEvent eventState) =>
    [
        eventState.DestructionPoints, eventState.NamekWarPoints, eventState.LunarNewYearEventPoints, eventState.ChristmasEventPoints,
        eventState.HalloweenEventPoints, eventState.InternationalWomensDayEventPoints, eventState.MoonFestivalEventPoints,
        eventState.HungKingEventPoints, eventState.BlackFridayEventPoints, eventState.VietnameseWomensDayEventPoints,
        eventState.AccumulatedPoints, eventState.ReceivedTopUpMilestone, eventState.EventPoints, eventState.BossEventPoints,
        eventState.NguHanhSonEventPoints, eventState.MonsterHuntEventPoints, eventState.KameElderEventPoints, eventState.EventMilestonePoints,
        eventState.SquareRiceCakeCookTime, eventState.CylindricalRiceCakeCookTime,
    ];
}
