using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Items.Rewards;

// Tính và cấp phần thưởng, nhất là chỉ số trang bị theo bảng phần thưởng.
// Nguồn Java: models/Reward/RewardService.java (lớp chính được chuyển đổi; giữ bảng số liệu, phép chia và quy tắc quay thưởng Java.)

// Port of models/Reward/RewardService.java: the option rules for gear, star stones and activation sets, and the daily/event reward packs.
// No singleton and no new Random(): the service is built with the ItemService (item factory and option lookup), a RandomUtil, a VietnamClock and the
// IItemHost that touches the player's bag. The base stat table of initBaseOptionClothes is data (BaseClothesOptions, generated from the Java switch) and the random
// +-10 percent variation is rolled here in the Java order.
// Not ported: getRewardItems(player, mob, x, yEnd), which in Java ignores all its arguments and returns a new empty list; it only matters once ItemMap (track F) exists. The two
// initStarOption overloads for ItemMap and Item became overloads over the option list they change (an ItemMap keeps its options in a list too).
public class RewardService
{
    private static readonly int[][][] ActivationSet =
    [
        [[129, 141, 1, 1000], [127, 140, 1, 1000], [128, 139, 1, 1000]],
        [[131, 143, 1, 1000], [132, 144, 1, 1000], [130, 142, 1, 1000]],
        [[135, 138, 1, 1000], [133, 136, 1, 1000], [134, 137, 1, 1000]],
    ];

    private readonly ItemService items;
    private readonly RandomUtil random;
    private readonly VietnamClock clock;
    private readonly IItemHost host;

    // Item factory (also the option-template lookup).
    // Random source.
    // Time source.
    // Bag and notification access (NullItemHost when omitted).
    public RewardService(ItemService items, RandomUtil random, VietnamClock clock, IItemHost? host = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(clock);
        this.items = items;
        this.random = random;
        this.clock = clock;
        this.host = host ?? NullItemHost.Instance;
    }

    // Java RewardService.RatioStar: the chance of a socket star, ratio out of typeRatio.
    public sealed class RatioStar
    {
        // Number of stars given (the parameter of option 107).
        // Numerator of the chance.
        // Denominator of the chance.
        public RatioStar(sbyte numStar, int ratio, int typeRatio)
        {
            NumStar = numStar;
            Ratio = ratio;
            TypeRatio = typeRatio;
        }

        // Java numStar.
        public sbyte NumStar { get; set; }

        // Java ratio.
        public int Ratio { get; set; }

        // Java typeRatio.
        public int TypeRatio { get; set; }
    }

    // Java initChiSoItem(item): adds the base options of a gear item.
    public void InitChiSoItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);
        InitBaseOptionClothes(item.RequiredTemplate.Id, item.RequiredTemplate.Type, item.ItemOptions);
    }

    // Java initBaseOptionClothes(tempId, type, list): appends the base options of a gear piece. Each option with a known id and value gets that value plus a random variation of +-10 percent
    // (nextInt(-(v * 10 / 100), v * 10 / 100), integer division, so a value below 10 has no variation but still draws a number). One option is added per filled slot, slot 0 to 4, so the random
    // numbers are used in slot order. The option id of a slot is resolved by position in the option table (an unknown id throws).
    public void InitBaseOptionClothes(int tempId, int type, List<ItemOption> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        int[][] optionParam = BaseClothesOptions.GetOptionParams(tempId, type);
        for (int i = 0; i < optionParam.Length; i++)
        {
            if (optionParam[i][0] != -1 && optionParam[i][1] != -1)
            {
                list.Add(items.CreateOption(
                    optionParam[i][0],
                    optionParam[i][1] + random.NextInt(-(optionParam[i][1] * 10 / 100), optionParam[i][1] * 10 / 100)));
            }
        }
    }

    // Java initStarOption(ItemMap item, ratioStars) / initStarOption(Item item, ratioStars): picks one of the star chances at random and, with that chance, adds option 107 with the star count.
    // options is the option list of the item or of the dropped item. The pick is drawn first (nextInt(0, length - 1)), the chance second, and the chance is drawn only when
    // the ratio is neither zero nor total (RandomUtil.IsTrueInt).
    public void InitStarOption(List<ItemOption> options, RatioStar[] ratioStars)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(ratioStars);
        RatioStar ratioStar = ratioStars[random.NextInt(0, ratioStars.Length - 1)];
        if (random.IsTrueInt(ratioStar.Ratio, ratioStar.TypeRatio))
        {
            options.Add(items.CreateOption(107, ratioStar.NumStar));
        }
    }

    // Overload of InitStarOption(List{ItemOption}, RatioStar[]) for an item.
    public void InitStarOption(Item item, RatioStar[] ratioStars)
    {
        ArgumentNullException.ThrowIfNull(item);
        InitStarOption(item.ItemOptions, ratioStars);
    }

    // Khởi tạo tùy chọn cơ bản của Sao Pha Lê (Java: initBaseOptionSaoPhaLe).
    // Java initBaseOptionSaoPhaLe(itemReward): adds the option of a socket stone (441 to 447: options 95 to 101 with 5, 5, 5, 3, 3, 5, 5; 220 to 225: options 71, 70, 69, 68, 67, 73 with 0).
    // Java bug kept: for any other template id the code adds new ItemOption(-1, 5), which asks the option table for position -1 and throws IndexOutOfBoundsException. The C# version throws
    // ArgumentOutOfRangeException in the same case, after the other options were left untouched.
    public void InitializeCrystalStarBaseOption(Item itemReward)
    {
        ArgumentNullException.ThrowIfNull(itemReward);
        int optionId = -1;
        int param = 5;
        switch (itemReward.RequiredTemplate.Id)
        {
            case 441:
                optionId = 95;
                param = 5;
                break;
            case 442:
                optionId = 96;
                param = 5;
                break;
            case 443:
                optionId = 97;
                param = 5;
                break;
            case 444:
                optionId = 98;
                param = 3;
                break;
            case 445:
                optionId = 99;
                param = 3;
                break;
            case 446:
                optionId = 100;
                param = 5;
                break;
            case 447:
                optionId = 101;
                param = 5;
                break;
            case 220:
                optionId = 71;
                param = 0;
                break;
            case 221:
                optionId = 70;
                param = 0;
                break;
            case 222:
                optionId = 69;
                param = 0;
                break;
            case 223:
                optionId = 68;
                param = 0;
                break;
            case 224:
                optionId = 67;
                param = 0;
                break;
            case 225:
                optionId = 73;
                param = 0;
                break;
        }

        if (optionId != -1)
        {
            itemReward.ItemOptions.Add(items.CreateOption(optionId, param));
        }
        else
        {
            itemReward.ItemOptions.Add(items.CreateOption(optionId, 5));
        }
    }

    // Java initActivationOption(gender, type, list): for body-part types 0 to 4 (type &lt;= 4) picks one of the three set options of the race at random (nextInt(0, 2)) and adds the set
    // option, its bonus option and the untradeable option 30 with parameter 7.
    public void InitActivationOption(int gender, int type, List<ItemOption> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        if (type <= 4)
        {
            int[] idOption = ActivationSet[gender][random.NextInt(0, 2)];
            list.Add(items.CreateOption(idOption[0], 1));
            list.Add(items.CreateOption(idOption[1], 1));
            list.Add(items.CreateOption(30, 7));
        }
    }

    private void NoRoom(Player player) => host.SendNotification(player, "Cần 1 ô hành trang trống");

    // Phát phần thưởng Tuần Lộc, giữ nguyên hậu tố tên riêng (nghĩa chưa chắc, cần chủ dự án duyệt). (Java: rewardTuanLoc)
    // Java rewardTuanLoc(player): gives one of 13 reward items (equal chance) with its own option rolls and, for the first nine, a 99 percent chance of a 7, 15 or 30 day expiry option 93. Needs one free
    // bag slot; sets canReward_TuanLoc = false and haveReward_TuanLoc = true only when there is room.
    public void RewardTuanLoc(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (host.GetCountEmptyBag(player) > 0)
        {
            player.CanReward_TuanLoc = false;
            player.HasReward_TuanLoc = true;
            short[] expiryDays = [7, 15, 30];
            short[] listItem = [1654, 1748, 1829, 1841, 1467, 822, 823, 388, 391, 394, 925, 926, 927];
            Item itemReceived = items.CreateNewItem(listItem[random.NextInt(0, listItem.Length - 1)]);
            int itemTemplate = itemReceived.RequiredTemplate.Id;
            if (itemTemplate == 1654)
            {
                itemReceived.AddOptionParam(50, random.NextInt(15, 17));
                itemReceived.AddOptionParam(77, random.NextInt(15, 17));
                itemReceived.AddOptionParam(103, random.NextInt(15, 17));
                itemReceived.AddOptionParam(106, 0);
                if (random.IsTrueInt(99, 100))
                {
                    itemReceived.ItemOptions.Add(items.CreateOption(93, expiryDays[random.NextInt(0, expiryDays.Length - 1)]));
                }
            }

            if (itemTemplate == 1748)
            {
                itemReceived.AddOptionParam(103, random.NextInt(15, 25));
                itemReceived.AddOptionParam(81, random.NextInt(5, 15));
                itemReceived.AddOptionParam(236, random.NextInt(20, 25));
                if (random.IsTrueInt(99, 100))
                {
                    itemReceived.ItemOptions.Add(items.CreateOption(93, expiryDays[random.NextInt(0, expiryDays.Length - 1)]));
                }
            }

            if (itemTemplate == 1829)
            {
                itemReceived.AddOptionParam(77, random.NextInt(15, 20));
                itemReceived.AddOptionParam(80, random.NextInt(10, 25));
                itemReceived.AddOptionParam(236, random.NextInt(20, 25));
                if (random.IsTrueInt(99, 100))
                {
                    itemReceived.ItemOptions.Add(items.CreateOption(93, expiryDays[random.NextInt(0, expiryDays.Length - 1)]));
                }
            }

            if (itemTemplate == 1841)
            {
                itemReceived.AddOptionParam(77, 23);
                itemReceived.AddOptionParam(80, random.NextInt(10, 20));
                itemReceived.AddOptionParam(94, random.NextInt(10, 20));
                if (random.IsTrueInt(99, 100))
                {
                    itemReceived.ItemOptions.Add(items.CreateOption(93, expiryDays[random.NextInt(0, expiryDays.Length - 1)]));
                }
            }

            if (itemTemplate == 1467)
            {
                itemReceived.AddOptionParam(77, random.NextInt(10, 20));
                itemReceived.AddOptionParam(50, random.NextInt(10, 20));
                itemReceived.AddOptionParam(103, random.NextInt(10, 20));
                itemReceived.AddOptionParam(101, random.NextInt(20, 50));
                if (random.IsTrueInt(99, 100))
                {
                    itemReceived.ItemOptions.Add(items.CreateOption(93, expiryDays[random.NextInt(0, expiryDays.Length - 1)]));
                }
            }

            if (itemTemplate == 822 || itemTemplate == 823)
            {
                itemReceived.AddOptionParam(50, random.NextInt(15, 20));
                itemReceived.AddOptionParam(77, random.NextInt(15, 20));
                itemReceived.AddOptionParam(103, random.NextInt(15, 20));
                itemReceived.AddOptionParam(106, 0);
                if (random.IsTrueInt(99, 100))
                {
                    itemReceived.ItemOptions.Add(items.CreateOption(93, expiryDays[random.NextInt(0, expiryDays.Length - 1)]));
                }
            }

            if (itemTemplate == 388 || itemTemplate == 391 || itemTemplate == 394)
            {
                itemReceived.AddOptionParam(77, 30);
                itemReceived.AddOptionParam(106, 0);
                itemReceived.AddOptionParam(80, 50);
                itemReceived.AddOptionParam(50, 20);
            }

            if (itemTemplate == 925 || itemTemplate == 926 || itemTemplate == 927)
            {
                itemReceived.AddOptionParam(87, 0);
                itemReceived.AddOptionParam(30, 0);
                itemReceived.AddOptionParam(93, 35);
            }

            host.AddItemBag(player, itemReceived);
            host.SendItemBag(player);
            host.SendNotification(player, "Bạn vừa nhận được " + itemReceived.RequiredTemplate.Name);
        }
        else
        {
            NoRoom(player);
        }
    }

    private void RewardPack(Player player, Action<Player> markReceived, short templateId, int minQuantity, int maxQuantity)
    {
        if (host.GetCountEmptyBag(player) > 0)
        {
            markReceived(player);
            int quantity = random.NextInt(minQuantity, maxQuantity);
            Item item = items.CreateNewItem(templateId);
            item.ItemOptions.Add(items.CreateOption(86, 0));
            item.ItemOptions.Add(items.CreateOption(93, 30));
            item.Quantity = quantity;
            host.AddItemBag(player, item);
            host.SendItemBag(player);
            host.SendNotification(player, "Bạn vừa nhận được " + quantity + " " + item.RequiredTemplate.Name);
        }
        else
        {
            NoRoom(player);
        }
    }

    // Java rewardBeNa(player): 1 to 5 of item 758 with options 86 and 93 = 30; sets canReward_PeNa = false, haveReward_PeNa = true. Needs one free slot.
    public void RewardBeNa(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        RewardPack(
            player,
            p =>
            {
                p.CanReceiveEventReward_PeNa = false;
                p.HasEventReward_PeNa = true;
            },
            758,
            1,
            5);
    }

    // Java rewardPiLong(player): 1 to 5 of item 1187 with options 86 and 93 = 30; sets canReward_PiLong = false, haveReward_PiLong = true. Needs one free slot.
    public void RewardPiLong(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        RewardPack(
            player,
            p =>
            {
                p.CanReceiveEventReward_PiLong = false;
                p.HasEventReward_PiLong = true;
            },
            1187,
            1,
            5);
    }

    // Java rewardMeoDen(player): 1 to 3 of item 1187 with options 86 and 93 = 30; sets canReward_MeoDen = false, haveReward_MeoDen = true. Needs one free slot.
    public void RewardMeoDen(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        RewardPack(
            player,
            p =>
            {
                p.CanReceiveEventReward_MeoDen = false;
                p.HasEventReward_MeoDen = true;
            },
            1187,
            1,
            3);
    }

    // Java rewardLancon(player): 1 to 3 of item 758 with options 86 and 93 = 30; sets canReward = false, haveReward = true. Needs one free slot.
    public void RewardLancon(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        RewardPack(
            player,
            p =>
            {
                p.CanReceiveEventReward = false;
                p.HasEventReward = true;
            },
            758,
            1,
            3);
    }

    // Java rewardFirstTimeLoginPerDay(player): on the first login of a new Vietnam calendar day gives item 649 (options 74 and 30 = 0) and an admin-style notice, then stamps
    // firstTimeLogin with now. Note the Java method does NOT check for a free slot before adding the item (the bag service decides what happens when it is full).
    // FirstTimeLogin is null (Java: NullPointerException).
    public void RewardFirstTimeLoginPerDay(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        DateTime first = player.FirstTimeLogin ?? throw new InvalidOperationException("the player has no first-login time (Java: NullPointerException)");
        if (first.Kind == DateTimeKind.Unspecified)
        {
            first = DateTime.SpecifyKind(first, DateTimeKind.Utc);
        }

        long nowMs = clock.CurrentTimeMillis();
        if (Util.IsAfterDay(nowMs, new DateTimeOffset(first.ToUniversalTime()).ToUnixTimeMilliseconds()))
        {
            Item item = items.CreateNewItem(649);
            item.Quantity = 1;
            item.ItemOptions.Add(items.CreateOption(74, 0));
            item.ItemOptions.Add(items.CreateOption(30, 0));
            host.AddItemBag(player, item);
            host.SendItemBag(player);
            host.SendNotificationFromAdmin(player, "Quà đăng nhập hàng ngày: \nBạn nhận được " + item.RequiredTemplate.Name + " số lượng : " + item.Quantity);
            player.FirstTimeLogin = clock.Now().UtcDateTime;
        }
    }
}
