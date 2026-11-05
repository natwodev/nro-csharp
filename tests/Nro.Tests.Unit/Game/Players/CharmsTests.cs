using Nro.Game.Players;
using Nro.Game.Players.Ports;

namespace Nro.Tests.Unit.Game.Players;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Charms.java (dòng 5–208).
public sealed class CharmsTests
{
    // Kiểm tra 25 mốc bùa có giá trị mặc định bằng không (Java: các trường td* và lastTimeSubMinTriTueX4).
    [Fact]
    public void Defaults_are_zero()
    {
        Charms charms = new();

        Assert.All(AllValues(charms), value => Assert.Equal(0, value));
    }

    // Kiểm tra chuỗi JSON thủ công theo đúng 21 chỉ số lưu bền và round-trip của Java (Java: toJSONArray, loadFromJSONArray).
    [Fact]
    public void Json_round_trip_preserves_the_exact_java_array_order()
    {
        const string source = "[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21]";
        Charms charms = new()
        {
            LastWisdomCharmX4MinuteSubtractionTime = 91,
            ClanToughnessCharmExpiry = 92,
            ClanStrengthCharmExpiry = 93,
            ClanWisdomCharmExpiry = 94,
        };

        charms.LoadFromJsonArray(source);

        Assert.Equal(source, charms.ToJsonArray());
        Assert.Equal(1, charms.WisdomCharmExpiry);
        Assert.Equal(8, charms.PetTrainingCharmExpiry);
        Assert.Equal(9, charms.WisdomCharmX3Expiry);
        Assert.Equal(17, charms.PetTrainingCharmX20Expiry);
        Assert.Equal(18, charms.WisdomCharmX5Expiry);
        Assert.Equal(21, charms.WisdomCharmX20Expiry);
        Assert.Equal(91, charms.LastWisdomCharmX4MinuteSubtractionTime);
        Assert.Equal(92, charms.ClanToughnessCharmExpiry);
        Assert.Equal(93, charms.ClanStrengthCharmExpiry);
        Assert.Equal(94, charms.ClanWisdomCharmExpiry);
    }

    // Kiểm tra mọi mã bùa kéo dài đúng trường đích và lấy mốc qua IPlayerClock (Java: addTimeCharms dòng 43–138).
    [Fact]
    public void Add_time_updates_every_java_item_mapping()
    {
        FixedClock clock = new(1_000_000);
        (int ItemId, Func<Charms, long> Read)[] cases =
        [
            (213, charm => charm.WisdomCharmExpiry), (214, charm => charm.StrengthCharmExpiry), (215, charm => charm.ToughnessCharmExpiry),
            (216, charm => charm.HeroismCharmExpiry), (217, charm => charm.ImmortalityCharmExpiry), (218, charm => charm.EnduranceCharmExpiry),
            (219, charm => charm.AttractionCharmExpiry), (522, charm => charm.PetTrainingCharmExpiry), (671, charm => charm.WisdomCharmX3Expiry),
            (672, charm => charm.WisdomCharmX4Expiry), (1734, charm => charm.PetTrainingCharmX2Expiry), (1735, charm => charm.PetTrainingCharmX3Expiry),
            (1736, charm => charm.PetTrainingCharmX4Expiry), (1737, charm => charm.PetTrainingCharmX5Expiry), (1738, charm => charm.PetTrainingCharmX7Expiry),
            (1739, charm => charm.PetTrainingCharmX10Expiry), (1740, charm => charm.PetTrainingCharmX20Expiry), (1741, charm => charm.WisdomCharmX5Expiry),
            (1742, charm => charm.WisdomCharmX7Expiry), (1743, charm => charm.WisdomCharmX10Expiry), (1744, charm => charm.WisdomCharmX20Expiry),
            (797, charm => charm.ClanWisdomCharmExpiry), (798, charm => charm.ClanStrengthCharmExpiry), (799, charm => charm.ClanToughnessCharmExpiry),
        ];

        foreach ((int itemId, Func<Charms, long> read) in cases)
        {
            Charms charms = new();
            charms.AddTimeCharms(itemId, 2, clock);
            Assert.Equal(1_120_000, read(charms));
        }

        Charms futureExpiry = new() { WisdomCharmExpiry = 1_300_000 };
        futureExpiry.AddTimeCharms(213, 1, clock);
        Assert.Equal(1_360_000, futureExpiry.WisdomCharmExpiry);
    }

    // Kiểm tra mã đặt lại và hàm đặt lại xóa đủ 25 mốc (Java: addTimeCharms dòng 139–141, resetAllCharms dòng 146–152).
    [Fact]
    public void Reset_clears_every_charm_expiry()
    {
        Charms charms = new();
        charms.LoadFromJsonArray("[1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1]");
        charms.LastWisdomCharmX4MinuteSubtractionTime = 1;
        charms.ClanToughnessCharmExpiry = 1;
        charms.ClanStrengthCharmExpiry = 1;
        charms.ClanWisdomCharmExpiry = 1;

        charms.AddTimeCharms(1950, 0, new FixedClock(0));

        Assert.All(AllValues(charms), value => Assert.Equal(0, value));
    }

    // Trả tập đầy đủ 25 mốc để kiểm tra reset/default (Java: các trường td* và lastTimeSubMinTriTueX4).
    private static IEnumerable<long> AllValues(Charms charms) =>
    [
        charms.WisdomCharmExpiry, charms.StrengthCharmExpiry, charms.ToughnessCharmExpiry, charms.HeroismCharmExpiry, charms.ImmortalityCharmExpiry,
        charms.EnduranceCharmExpiry, charms.AttractionCharmExpiry, charms.PetTrainingCharmExpiry, charms.PetTrainingCharmX2Expiry,
        charms.PetTrainingCharmX3Expiry, charms.PetTrainingCharmX4Expiry, charms.PetTrainingCharmX5Expiry, charms.PetTrainingCharmX7Expiry,
        charms.PetTrainingCharmX10Expiry, charms.PetTrainingCharmX20Expiry, charms.WisdomCharmX3Expiry, charms.WisdomCharmX4Expiry,
        charms.WisdomCharmX5Expiry, charms.WisdomCharmX7Expiry, charms.WisdomCharmX10Expiry, charms.WisdomCharmX20Expiry,
        charms.LastWisdomCharmX4MinuteSubtractionTime, charms.ClanToughnessCharmExpiry, charms.ClanStrengthCharmExpiry, charms.ClanWisdomCharmExpiry,
    ];

    // Cấp mốc thời gian cố định cho kiểm thử không dùng DateTime.Now (Java: System.currentTimeMillis).
    private sealed class FixedClock(long nowMilliseconds) : IPlayerClock
    {
        // Trả mốc epoch cố định của bài kiểm thử (Java: System.currentTimeMillis).
        public long NowMilliseconds { get; } = nowMilliseconds;
    }
}
