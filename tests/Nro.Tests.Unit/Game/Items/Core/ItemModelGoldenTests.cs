using System.Text;
using System.Text.Json;
using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.IO;
using Nro.Core.Templates;
using Nro.Game;
using Nro.Tests.Unit.Helpers;
using Nro.Tests.Unit.Game.Items.Time;

namespace Nro.Tests.Unit.Game.Items.Core;

/// <summary>
/// Replays the cases of <c>item-model.json</c> on the port and compares the text with what the REAL Java code produced
/// (<c>ItemModelGenerator.java.txt</c> runs the original <c>Item</c>, <c>ItemOption</c>, <c>ItemService</c> and <c>RewardService</c> classes against a synthetic catalog; see its header for the exact text changes).
/// Random cases are replayed with <see cref="SeededRandomProvider"/>, a bit-exact port of <c>java.util.Random</c>, built with the same seed, so the draws line up call for call.
/// </summary>
public class ItemModelGoldenTests
{
    private static readonly Lazy<Golden> Shared = new(Golden.Load);

    public static IEnumerable<object[]> Scenarios() => Shared.Value.Cases.Select(c => c.Name).Distinct().OrderBy(n => n, StringComparer.Ordinal).Select(n => new object[] { n });

    [Fact]
    public void The_golden_file_comes_from_a_real_jdk_and_covers_every_scenario_name_the_tests_replay()
    {
        Golden golden = Shared.Value;

        Assert.StartsWith("17.", golden.Jdk);
        Assert.Equal("Asia/Ho_Chi_Minh", golden.TimeZone);
        Assert.Equal(11, golden.Sources.Length);
        Assert.True(golden.Cases.Length > 4500, "cases: " + golden.Cases.Length);
        foreach (string name in golden.Cases.Select(c => c.Name).Distinct())
        {
            Assert.True(Replay.Known(name), "no replay for scenario " + name);
        }
    }

    [Theory]
    [MemberData(nameof(Scenarios))]
    public void The_port_matches_the_java_result_for_every_case_of_the_scenario(string scenario)
    {
        Golden golden = Shared.Value;
        List<string> failures = [];
        int count = 0;
        foreach (Case c in golden.Cases.Where(c => c.Name == scenario))
        {
            count++;
            string actual;
            try
            {
                actual = Replay.Run(golden, c);
            }
            catch (Exception e)
            {
                actual = Replay.JavaName(e);
            }

            if (actual != c.Result)
            {
                failures.Add($"seed={c.Seed} args=[{c.Args}]\n   java: {c.Result}\n   port: {actual}");
            }
        }

        Assert.True(count > 0);
        Assert.True(failures.Count == 0, $"{failures.Count} of {count} cases differ in {scenario}:\n" + string.Join("\n", failures.Take(8)));
    }

    // ------------------------------------------------------------------------------------------------------------------------------------------------

    private sealed record Case(string Name, long Seed, string Args, string Result);

    private sealed class Golden
    {
        public string Jdk { get; init; } = "";

        public string TimeZone { get; init; } = "";

        public string[] Sources { get; init; } = [];

        public long BaseNow { get; init; }

        public int Templates { get; init; }

        public int Options { get; init; }

        public int[][] GearTypes { get; init; } = [];

        public int[][] Shop { get; init; } = [];

        public Case[] Cases { get; init; } = [];

        /// <summary>The Java names of the boolean fields of ItemTime, in the sorted order the harness used (read from the case names of itemTimeUpdate).</summary>
        public string[] FlagNames => Cases.Where(x => x.Name == "itemTimeUpdate").Select(x => x.Args).ToArray();

        public static Golden Load()
        {
            string path = Path.Combine(TestPaths.ProjectDirectory("Nro.Tests.Unit"), "Game", "Items", "Core", "item-model.json");
            using JsonDocument doc = JsonDocument.Parse(File.ReadAllBytes(path));
            JsonElement root = doc.RootElement;
            JsonElement meta = root.GetProperty("meta");
            return new Golden
            {
                Jdk = meta.GetProperty("jdk").GetString()!,
                TimeZone = meta.GetProperty("timezone").GetString()!,
                Sources = meta.GetProperty("sources").EnumerateArray().Select(e => e.GetString()!).ToArray(),
                BaseNow = meta.GetProperty("baseNow").GetInt64(),
                Templates = meta.GetProperty("templates").GetInt32(),
                Options = meta.GetProperty("options").GetInt32(),
                GearTypes = meta.GetProperty("gearTypes").EnumerateArray().Select(a => a.EnumerateArray().Select(x => x.GetInt32()).ToArray()).ToArray(),
                Shop = meta.GetProperty("shop").EnumerateArray().Select(a => a.EnumerateArray().Select(x => x.GetInt32()).ToArray()).ToArray(),
                Cases = root.GetProperty("cases").EnumerateArray()
                    .Select(a => new Case(a[0].GetString()!, a[1].GetInt64(), a[2].GetString()!, a[3].GetString()!))
                    .ToArray(),
            };
        }

        /// <summary>The synthetic catalog the Java run used (formulas: type from the gear table or (id * 7 + 3) % 40, gender id % 4, level id % 16).</summary>
        public ItemCatalog BuildCatalog()
        {
            var options = new List<ItemOptionTemplate>();
            for (int i = 0; i < Options; i++)
            {
                options.Add(new ItemOptionTemplate(i, "Opt" + i + " #", 0));
            }

            var templates = new List<ItemTemplate>();
            for (int i = 0; i < Templates; i++)
            {
                int type = (i * 7 + 3) % 40;
                for (int t = 0; t < GearTypes.Length; t++)
                {
                    if (GearTypes[t].Contains(i))
                    {
                        type = t;
                    }
                }

                templates.Add(new ItemTemplate
                {
                    Id = (short)i,
                    Type = (sbyte)type,
                    Gender = (sbyte)(i % 4),
                    Level = (sbyte)(i % 16),
                    Name = "Item" + i,
                    StrRequire = i * 10,
                    IconID = (short)(i + 1000),
                });
            }

            var shop = new List<ShopItemEntry>();
            foreach (int[] e in Shop)
            {
                var list = new List<ItemOption>();
                for (int k = 1; k + 1 < e.Length; k += 2)
                {
                    list.Add(new ItemOption(options[e[k]], e[k + 1]));
                }

                shop.Add(new ShopItemEntry(templates[e[0]], list));
            }

            return new ItemCatalog(templates, options, shop);
        }
    }

    /// <summary>Host that records the calls in the exact text format of the Java stub (<c>Harness.log</c>).</summary>
    private sealed class JavaLogHost : IItemHost
    {
        public List<string> Log { get; } = [];

        public int EmptySlots { get; set; } = 10;

        public Dictionary<int, Item> Bag { get; } = [];

        public List<Item> Combine { get; } = [];

        public string TakeLog()
        {
            string s = string.Join(";", Log);
            Log.Clear();
            return s;
        }

        public ExternalItemTimes External { get; set; } = ExternalItemTimes.None;

        public void SendMessage(Player player, Message message)
        {
            Log.Add("msg:" + message.Command + ":" + Convert.ToHexString(message.GetData()).ToLowerInvariant());
        }

        public void SendPoint(Player player) => Log.Add("point");

        public void SendNotification(Player player, string text) => Log.Add("notify:" + text);

        public void SendNotificationFromAdmin(Player player, string text) => Log.Add("admin:" + text);

        public void SendItemBag(Player player) => Log.Add("bag");

        public int GetCountEmptyBag(Player player) => EmptySlots;

        public void AddItemBag(Player player, Item item) => Log.Add("add:" + Replay.DumpItem(item));

        public void SubQuantityItemsBag(Player player, Item? item, int quantity) => Log.Add("sub:" + (item == null ? "null" : item.Template!.Id.ToString()) + ":" + quantity);

        public Item? FindBagItem(Player player, int templateId) => Bag.TryGetValue(templateId, out Item? item) ? item : null;

        public IReadOnlyList<Item> GetCombineItems(Player player) => Combine;

        public void RemoveMonkeyTail(Player player) => Log.Add("duoikhi");

        public void IncreasePowerLimit(Player player) => Log.Add("powerlimit");

        public ExternalItemTimes GetExternalItemTimes(Player player) => External;
    }

    private static class Replay
    {
        private static readonly HashSet<string> KnownNames =
        [
            "randDoTLBoss", "randDoTL", "getDefaultOptionTL", "otpts", "otptl", "otphd", "randomSKHId", "itemSKH", "randomCS_DHD", "DoThienSu", "randTempItemKichHoat", "randTempItemKichHoat_VIP",
            "randOptionItemKichHoat", "createItemFromItemShop", "optionIdSKH", "optionIdDHD", "randOptionItemKichHoatNew", "getListOptionItemShop3", "getListOptionItemShop1",
            "getOptionParamItemShop", "getAngelItem", "trainArmor", "trainArmorNull", "isOutOfDateTime", "loadItemTimeStatus", "isOutOfDateTimeNull", "itemPredicates", "itemOptionPredicates",
            "stoneDefaultOption", "stoneOption", "getPercentOption", "getOptionInfoChuyenHoa", "itemEdits", "copyAndClone", "initBaseOptionClothes", "initBaseOptionSaoPhaLe",
            "initActivationOption", "initStarOption", "rewardTuanLoc", "rewardBeNa", "rewardPiLong", "rewardMeoDen", "rewardLancon", "rewardFirstTimeLoginPerDay", "openDTS",
            "itemTimeUpdate", "itemTimeUpdateAll", "sendAllItemTime", "clearAllItemTime", "tdlt", "sendItemTime", "sendTextTime", "itemTimeProfile", "textHelpers",
        ];

        public static bool Known(string name) => KnownNames.Contains(name) || SetRewardDefinition.All.Any(d => d.JavaName == name);

        public static string JavaName(Exception e) => "EXC:" + e switch
        {
            ArgumentOutOfRangeException => "IndexOutOfBoundsException",
            IndexOutOfRangeException => "ArrayIndexOutOfBoundsException",
            DivideByZeroException => "ArithmeticException",
            OverflowException => "NegativeArraySizeException",
            InvalidOperationException => "NullPointerException",
            _ => e.GetType().Name,
        };

        public static string Dump(IEnumerable<ItemOption> options) => ItemTestKit.Dump(options);

        public static string DumpItem(Item it)
            => (it.Template == null ? "-" : it.Template.Id.ToString()) + "x" + it.Quantity + "|" + Dump(it.ItemOptions) + "|" + it.CreateTime + "|" + (it.Info ?? "null") + "|" + (it.Content ?? "null");

        private static string B(bool value) => value ? "true" : "false";

        private static string D(bool value) => value ? "1" : "0";

        private static int[] Pairs(string args)
        {
            if (args.Length == 0)
            {
                return [];
            }

            return args.Split(',').SelectMany(p => p.Split(':')).Select(int.Parse).ToArray();
        }

        private sealed class Env
        {
            public Env(Golden golden, Case c, long? nowOverride = null)
            {
                Catalog = golden.BuildCatalog();
                Provider = new SeededRandomProvider(c.Seed);
                Time = new ManualTimeProvider(DateTimeOffset.FromUnixTimeMilliseconds(nowOverride ?? golden.BaseNow));
                Host = new JavaLogHost();
                Random = new RandomUtil(Provider);
                Clock = new VietnamClock(Time);
                Items = new ItemService(Catalog, Random, Clock, Host);
                Rewards = new RewardService(Items, Random, Clock, Host);
            }

            public ItemCatalog Catalog { get; }

            public SeededRandomProvider Provider { get; }

            public ManualTimeProvider Time { get; }

            public JavaLogHost Host { get; }

            public RandomUtil Random { get; }

            public VietnamClock Clock { get; }

            public ItemService Items { get; }

            public RewardService Rewards { get; }

            public long Now => Time.GetUtcNow().ToUnixTimeMilliseconds();

            public Item Item(int id, params int[] opts)
            {
                var it = new Item(Catalog, Now) { Template = Catalog.GetTemplate(id), Quantity = 1 };
                for (int k = 0; k + 1 < opts.Length; k += 2)
                {
                    it.ItemOptions.Add(new ItemOption(Catalog.GetItemOptionTemplate(opts[k]), opts[k + 1]));
                }

                return it;
            }
        }

        private static System.Reflection.PropertyInfo Flag(ItemTime time, string javaName)
            => typeof(ItemTime).GetProperty(ItemTimeSpec.CSharpName(javaName)) ?? throw new InvalidOperationException("no property for " + javaName);

        private sealed class TimeRig
        {
            public TimeRig(Golden g, Case c)
            {
                Env = new Env(g, c);
                Player = new Player();
                Time = new ItemTime(Player, Env.Clock, Env.Host);
                Player.ItemTime = Time;
                Service = new ItemTimeService(Env.Host, Env.Clock);
            }

            public Env Env { get; }

            public Player Player { get; }

            public ItemTime Time { get; }

            public ItemTimeService Service { get; }

            public void SetAllLongs(long value)
            {
                foreach ((string name, string type) in ItemTimeSpec.Fields)
                {
                    if (type == "long")
                    {
                        typeof(ItemTime).GetProperty(ItemTimeSpec.CSharpName(name))!.SetValue(Time, value);
                    }
                }
            }

            public string StillOn(Golden g)
            {
                var sb = new StringBuilder();
                foreach (string name in g.FlagNames)
                {
                    if ((bool)Flag(Time, name).GetValue(Time)!)
                    {
                        sb.Append(name).Append(',');
                    }
                }

                return sb.ToString();
            }
        }

        private static ExternalItemTimes Profile(string text, long now)
        {
            if (text == "none")
            {
                return ExternalItemTimes.None;
            }

            sbyte fusionType = 0;
            long fusionLast = 0;
            bool cum = false, cum2 = false, pain = false, kef = false;
            long cumAt = 0, cum2At = 0, painAt = 0, kefAt = 0;
            bool hasClan = false, gone = false, hasDoanhTrai = false, picking = false;
            long dt = 0, bdkb = 0, cdrd = 0, kg = 0, boss = 0, pickAt = 0;
            int round = 0;
            static long Ago(string v, long now2) => v == "neg" ? -1 : now2 - long.Parse(v);
            foreach (string kv in text.Split(','))
            {
                string[] x = kv.Split('=');
                string v = x.Length > 1 ? x[1] : "";
                switch (x[0])
                {
                    case "fus":
                        fusionType = sbyte.Parse(v.Split('@')[0]);
                        fusionLast = now - long.Parse(v.Split('@')[1]);
                        break;
                    case "cum":
                        cum = true;
                        cumAt = Ago(v, now);
                        break;
                    case "cum2":
                        cum2 = true;
                        cum2At = Ago(v, now);
                        break;
                    case "pain":
                        pain = true;
                        painAt = Ago(v, now);
                        break;
                    case "kef":
                        kef = true;
                        kefAt = Ago(v, now);
                        break;
                    case "clan":
                        hasClan = true;
                        break;
                    case "gone":
                        gone = true;
                        break;
                    case "dt":
                        dt = now - long.Parse(v);
                        break;
                    case "bdkb":
                        bdkb = now - long.Parse(v);
                        break;
                    case "cdrd":
                        cdrd = now - long.Parse(v);
                        break;
                    case "kg":
                        kg = now - long.Parse(v);
                        break;
                    case "boss":
                        boss = now - long.Parse(v);
                        break;
                    case "round":
                        round = int.Parse(v);
                        break;
                    case "dtp":
                        hasDoanhTrai = true;
                        picking = v != "neg";
                        pickAt = v == "neg" ? 0 : now - long.Parse(v);
                        break;
                    default:
                        throw new InvalidOperationException("profile key " + x[0]);
                }
            }

            return new ExternalItemTimes
            {
                FusionType = fusionType,
                FusionLastTime = fusionLast,
                FusionDurationMs = 600000,
                IsCumber = cum,
                LastTimeCumber = cumAt,
                IsCumber2 = cum2,
                LastTimeCumber2 = cum2At,
                IsPain = pain,
                LastTimeUpPain = painAt,
                IsKefla = kef,
                LastTimeKefla = kefAt,
                Clan = hasClan
                    ? new ClanDungeonTimes
                    {
                        HaveGoneBarracks = gone,
                        LastTimeOpenBarracks = dt,
                        BarracksDurationMs = 1800000,
                        HasBarracks = hasDoanhTrai,
                        BarracksIsTimePicking = picking,
                        BarracksLastTimePick = pickAt,
                        BarracksPickDurationMs = 120000,
                        LastTimeOpenTreasureUnderSea = bdkb,
                        TreasureUnderSeaDurationMs = 1500000,
                        LastTimeOpenSnakeWay = cdrd,
                        SnakeWayDurationMs = 1200000,
                        LastTimeOpenDestronGas = kg,
                        DestronGasDurationMs = 900000,
                        LastTimeOpenBossOfTheGangs = boss,
                        BossOfTheGangsDurationMs = 1800000,
                        BossClanRound = round,
                    }
                    : null,
            };
        }

        public static string Run(Golden g, Case c)
        {
            var e = new Env(g, c);
            string[] a = c.Args.Split(',');
            switch (c.Name)
            {
                case "randDoTLBoss":
                {
                    RolledDrop d = e.Items.RandDoTLBoss();
                    return d.TemplateId + "|" + Dump(d.Options);
                }

                case "randDoTL":
                {
                    RolledDrop d = e.Items.RandDoTL();
                    return d.TemplateId + "|" + Dump(d.Options);
                }

                case "getDefaultOptionTL":
                    return Dump(e.Items.GetDefaultOptionTL(int.Parse(c.Args)));
                case "otpts":
                    return DumpItem(e.Items.Otpts(short.Parse(c.Args), 3));
                case "otptl":
                    return DumpItem(e.Items.Otptl(short.Parse(c.Args), 3));
                case "otphd":
                    return DumpItem(e.Items.Otphd(short.Parse(c.Args), 3));
                case "randomSKHId":
                    return e.Items.RandomSKHId((sbyte)int.Parse(c.Args)).ToString();
                case "itemSKH":
                    return DumpItem(e.Items.ItemSKH(int.Parse(a[0]), int.Parse(a[1])));
                case "randomCS_DHD":
                    return DumpItem(e.Items.RandomCS_DHD(int.Parse(a[0]), int.Parse(a[1])));
                case "DoThienSu":
                    return DumpItem(e.Items.DoThienSu(int.Parse(a[0]), int.Parse(a[1])));
                case "randTempItemKichHoat":
                    return e.Items.RandTempItemKichHoat(int.Parse(c.Args)).ToString();
                case "randTempItemKichHoat_VIP":
                    return e.Items.RandTempItemKichHoat_VIP(int.Parse(c.Args)).ToString();
                case "randOptionItemKichHoat":
                    return "[" + string.Join(", ", e.Items.RandOptionItemKichHoat(int.Parse(c.Args))) + "]";
                case "randOptionItemKichHoatNew":
                    return "[" + string.Join(", ", e.Items.RandOptionItemKichHoatNew(int.Parse(c.Args))) + "]";
                case "createItemFromItemShop":
                {
                    string[] parts = c.Args.Split(';');
                    int[] set = Pairs(parts[1]);
                    var options = new List<ItemOption>();
                    for (int k = 0; k + 1 < set.Length; k += 2)
                    {
                        options.Add(new ItemOption(e.Catalog.GetItemOptionTemplate(set[k]), set[k + 1]));
                    }

                    return DumpItem(e.Items.CreateItemFromItemShop(e.Catalog.GetTemplate(int.Parse(parts[0])), options));
                }

                case "optionIdSKH":
                case "optionIdDHD":
                {
                    var sb = new StringBuilder();
                    for (int id = -2; id <= 301; id++)
                    {
                        sb.Append(c.Name == "optionIdSKH" ? e.Items.OptionIdSKH(id) : e.Items.OptionIdDHD(id)).Append(',');
                    }

                    return sb.ToString();
                }

                case "getListOptionItemShop3":
                    return Dump(e.Items.GetListOptionItemShop(int.Parse(a[0]), int.Parse(a[1]), int.Parse(a[2])));
                case "getListOptionItemShop1":
                    return Dump(e.Items.GetListOptionItemShop(short.Parse(c.Args)));
                case "getOptionParamItemShop":
                    return e.Items.GetOptionParamItemShop(short.Parse(a[0]), int.Parse(a[1])).ToString();
                case "getAngelItem":
                    return DumpItem(e.Items.GetAngelItem(int.Parse(a[0]), int.Parse(a[1])));
                case "trainArmor":
                {
                    Item it = e.Item(int.Parse(c.Args));
                    return e.Items.GetPercentTrainArmor(it) + "," + B(e.Items.IsTrainArmor(it));
                }

                case "trainArmorNull":
                    return e.Items.GetPercentTrainArmor(null) + "," + B(e.Items.IsTrainArmor(null));
                case "isOutOfDateTimeNull":
                    return B(e.Items.IsOutOfDateTime(null));
                case "isOutOfDateTime":
                case "loadItemTimeStatus":
                {
                    string[] parts = c.Args.Split(";age=");
                    long age = long.Parse(parts[1]);
                    Item it = e.Item(5, Pairs(parts[0]));
                    it.CreateTime = e.Now - (age * 60000L);
                    if (c.Name == "isOutOfDateTime")
                    {
                        bool r = e.Items.IsOutOfDateTime(it);
                        return B(r) + "|" + Dump(it.ItemOptions) + "|" + (it.CreateTime - e.Now);
                    }

                    e.Items.LoadItemTimeStatus(new Player(), it);
                    return Dump(it.ItemOptions) + "|" + (it.CreateTime - e.Now) + "|" + e.Host.TakeLog();
                }

                case "itemPredicates":
                    return ItemPredicates(e, int.Parse(c.Args));
                case "itemOptionPredicates":
                    return ItemOptionPredicates(e, c.Args);
                case "stoneDefaultOption":
                {
                    ItemOption io = e.Item(300, Pairs(c.Args)).GetOptionDaPhaLe();
                    return io.OptionTemplate.Id + ":" + io.Param;
                }

                case "stoneOption":
                {
                    ItemOption io = e.Item(int.Parse(c.Args), 1, 1).GetOptionDaPhaLe();
                    return io.OptionTemplate.Id + ":" + io.Param;
                }

                case "getPercentOption":
                {
                    string[] parts = c.Args.Split(';');
                    return e.Item(int.Parse(parts[0]), Pairs(parts[1])).GetPercentOption().ToString();
                }

                case "getOptionInfoChuyenHoa":
                {
                    string[] parts = c.Args.Split(';');
                    Item target = e.Item(5, 47, 200, 21, 5, 0, 1000, 6, 77, 7, 9);
                    string info = target.GetOptionInfoChuyenHoa(e.Item(int.Parse(parts[0]), Pairs(parts[1])), 3);
                    return info + "|" + Dump(target.ItemOptions);
                }

                case "itemEdits":
                    return ItemEdits(e, c.Args);
                case "copyAndClone":
                {
                    Item it = e.Item(5, 47, 10, 21, 5);
                    it.Info = "i";
                    it.Content = "c";
                    it.Quantity = 7;
                    it.QuantityGD = 3;
                    Item copy = e.Items.CopyItem(it);
                    Item clone = it.CloneItem();
                    copy.ItemOptions[0].Param = 1;
                    clone.ItemOptions[1].Param = 2;
                    return DumpItem(it) + "#" + DumpItem(copy) + "#" + DumpItem(clone) + "#" + copy.QuantityGD + clone.QuantityGD;
                }

                case "initBaseOptionClothes":
                {
                    var list = new List<ItemOption>();
                    e.Rewards.InitBaseOptionClothes(int.Parse(a[0]), int.Parse(a[1]), list);
                    return Dump(list);
                }

                case "initBaseOptionSaoPhaLe":
                {
                    Item it = e.Item(int.Parse(c.Args), 47, 1);
                    e.Rewards.InitBaseOptionSaoPhaLe(it);
                    return Dump(it.ItemOptions);
                }

                case "initActivationOption":
                {
                    var list = new List<ItemOption>();
                    e.Rewards.InitActivationOption(int.Parse(a[0]), int.Parse(a[1]), list);
                    return Dump(list);
                }

                case "initStarOption":
                {
                    int[][][] stars =
                    [
                        [[1, 1, 2]],
                        [[3, 0, 100], [7, 100, 100]],
                        [[2, 50, 100], [4, 25, 100], [6, 1, 1000]],
                        [[1, -1, 5]],
                        [[1, 5, 0]],
                    ];
                    int[][] set = stars[int.Parse(c.Args["set".Length..])];
                    RewardService.RatioStar[] arr = set.Select(s => new RewardService.RatioStar((sbyte)s[0], s[1], s[2])).ToArray();
                    Item it = e.Item(5);
                    e.Rewards.InitStarOption(it, arr);
                    return Dump(it.ItemOptions);
                }

                case "rewardTuanLoc":
                case "rewardBeNa":
                case "rewardPiLong":
                case "rewardMeoDen":
                case "rewardLancon":
                {
                    var p = new Player();
                    e.Host.EmptySlots = int.Parse(c.Args["slots=".Length..]);
                    switch (c.Name)
                    {
                        case "rewardTuanLoc":
                            e.Rewards.RewardTuanLoc(p);
                            break;
                        case "rewardBeNa":
                            e.Rewards.RewardBeNa(p);
                            break;
                        case "rewardPiLong":
                            e.Rewards.RewardPiLong(p);
                            break;
                        case "rewardMeoDen":
                            e.Rewards.RewardMeoDen(p);
                            break;
                        default:
                            e.Rewards.RewardLancon(p);
                            break;
                    }

                    return e.Host.TakeLog() + "|" + B(p.CanReward_TuanLoc) + B(p.HaveReward_TuanLoc) + B(p.CanReward_PeNa) + B(p.HaveReward_PeNa) + B(p.CanReward_PiLong) + B(p.HaveReward_PiLong)
                        + B(p.CanReward_MeoDen) + B(p.HaveReward_MeoDen) + B(p.CanReward) + B(p.HaveReward);
                }

                case "rewardFirstTimeLoginPerDay":
                {
                    long offsetMin = long.Parse(c.Args["offsetMin=".Length..]);
                    var e2 = new Env(g, c, g.BaseNow + (8 * 3600000L));
                    var p = new Player { FirstTimeLogin = DateTimeOffset.FromUnixTimeMilliseconds(e2.Now + (offsetMin * 60000L)).UtcDateTime };
                    e2.Rewards.RewardFirstTimeLoginPerDay(p);
                    long after = new DateTimeOffset(DateTime.SpecifyKind(p.FirstTimeLogin!.Value, DateTimeKind.Utc)).ToUnixTimeMilliseconds();
                    return e2.Host.TakeLog() + "|" + (after - e2.Now);
                }

                case "itemTimeUpdate":
                {
                    var sb = new StringBuilder();
                    foreach (long d in new long[] { 0, 1, 3000, 3001, 5000, 5001, 10000, 10001, 30000, 30001, 60000, 60001, 120000, 120001, 180000, 180001, 600000, 600001, 900000, 900001, 1200001, 1800000,
                        1800001, 3000001, 3600000, 3600001, 5400001, 7200001, 9000001, 86400000, 86400001 })
                    {
                        var t = new TimeRig(g, c);
                        t.SetAllLongs(t.Env.Now - d);
                        t.Time.AutoPlayDurationMs = 60000;
                        Flag(t.Time, c.Args).SetValue(t.Time, true);
                        t.Time.Update();
                        sb.Append(d).Append((bool)Flag(t.Time, c.Args).GetValue(t.Time)! ? ":1" : ":0");
                        string lg = t.Env.Host.TakeLog();
                        if (lg.Length > 0)
                        {
                            sb.Append('/').Append(lg);
                        }

                        sb.Append(' ');
                    }

                    return sb.ToString().Trim();
                }

                case "itemTimeUpdateAll":
                {
                    var t = new TimeRig(g, c);
                    t.SetAllLongs(t.Env.Now - 100000000L);
                    foreach (string name in g.FlagNames)
                    {
                        Flag(t.Time, name).SetValue(t.Time, true);
                    }

                    t.Time.AutoPlayDurationMs = 1000;
                    t.Time.Update();
                    return t.Env.Host.TakeLog() + "|" + t.StillOn(g);
                }

                case "sendAllItemTime":
                {
                    var t = new TimeRig(g, c);
                    t.SetAllLongs(t.Env.Now - 100000);
                    t.Time.AutoPlayDurationMs = 180000;
                    t.Time.IconMeal = 4242;
                    if (c.Args == "flag=ALL")
                    {
                        foreach (string name in g.FlagNames)
                        {
                            Flag(t.Time, name).SetValue(t.Time, true);
                        }
                    }
                    else
                    {
                        Flag(t.Time, c.Args["flag=".Length..]).SetValue(t.Time, true);
                    }

                    t.Service.SendAllItemTime(t.Player);
                    return t.Env.Host.TakeLog();
                }

                case "clearAllItemTime":
                {
                    var t = new TimeRig(g, c);
                    if (c.Args == "flags=ALL")
                    {
                        foreach (string name in g.FlagNames)
                        {
                            Flag(t.Time, name).SetValue(t.Time, true);
                        }
                    }

                    t.Service.ClearAllItemTime(t.Player);
                    return c.Args == "flags=ALL" ? t.Env.Host.TakeLog() + "|" + t.StillOn(g) : t.Env.Host.TakeLog();
                }

                case "tdlt":
                {
                    string[] parts = c.Args.Split(';');
                    var t = new TimeRig(g, c);
                    Item it = t.Env.Item(5, 1, int.Parse(parts[0]["minutes=".Length..]));
                    t.Service.TurnOnAutoPlay(t.Player, it);
                    string on = t.Env.Host.TakeLog() + "|" + B(t.Time.IsAutoPlayEnabled) + "," + t.Time.AutoPlayDurationMs + "," + Dump(it.ItemOptions);
                    t.Env.Time.Advance(TimeSpan.FromMilliseconds(long.Parse(parts[1]["elapsedMs=".Length..])));
                    t.Service.TurnOffAutoPlay(t.Player, it);
                    return on + "|" + t.Env.Host.TakeLog() + "|" + B(t.Time.IsAutoPlayEnabled) + "," + Dump(it.ItemOptions);
                }

                case "sendItemTime":
                {
                    var t = new TimeRig(g, c);
                    t.Service.SendItemTime(t.Player, int.Parse(a[0]), int.Parse(a[1]));
                    return t.Env.Host.TakeLog();
                }

                case "sendTextTime":
                {
                    var t = new TimeRig(g, c);
                    t.Service.SendTextTime(t.Player, unchecked((sbyte)int.Parse(a[0])), a[1], int.Parse(a[2]));
                    return t.Env.Host.TakeLog();
                }

                case "itemTimeProfile":
                {
                    string[] parts = c.Args.Split(';');
                    var t = new TimeRig(g, c);
                    t.Env.Host.External = Profile(parts[1], t.Env.Now);
                    switch (parts[0])
                    {
                        case "sendAll":
                            t.Service.SendAllItemTime(t.Player);
                            break;
                        case "sendTextDoanhTrai":
                            t.Service.SendTextBarracks(t.Player);
                            break;
                        case "sendTextTimePickDoanhTrai":
                            t.Service.SendTextTimePickBarracks(t.Player);
                            break;
                        case "sendTextBanDoKhoBau":
                            t.Service.SendTextTreasureUnderSea(t.Player);
                            break;
                        case "sendTextConDuongRanDoc":
                            t.Service.SendTextSnakeWay(t.Player);
                            break;
                        case "sendTextKhiGasHuyDiet":
                            t.Service.SendTextDestronGas(t.Player);
                            break;
                        default:
                            t.Service.SendTextClanBossMap(t.Player);
                            break;
                    }

                    return t.Env.Host.TakeLog();
                }

                case "textHelpers":
                {
                    var t = new TimeRig(g, c);
                    t.Service.SendTextXinbato(t.Player);
                    t.Service.RemoveTextBarracks(t.Player);
                    t.Service.RemoveTextTreasureUnderSea(t.Player);
                    t.Service.RemoveTextSnakeWay(t.Player);
                    t.Service.RemoveTextDestronGas(t.Player);
                    t.Service.RemoveTextClanBossMap(t.Player);
                    t.Service.RemoveItemTime(t.Player, 2755);
                    t.Service.SendCanAutoPlay(t.Player);
                    t.Time.IsAutoPlayEnabled = true;
                    t.Service.SendCanAutoPlay(t.Player);
                    return t.Env.Host.TakeLog();
                }

                case "openDTS":
                {
                    string[] parts = c.Args.Split(';');
                    var p = new Player { Gender = (sbyte)int.Parse(parts[1]["g=".Length..]) };
                    e.Host.EmptySlots = int.Parse(parts[2]["slots=".Length..]);
                    foreach (int id in parts[0].Split('+').Select(int.Parse))
                    {
                        e.Host.Combine.Add(e.Item(id));
                    }

                    e.Items.OpenDTS(p);
                    return e.Host.TakeLog();
                }

                default:
                {
                    SetRewardDefinition? def = SetRewardDefinition.All.FirstOrDefault(d => d.JavaName == c.Name);
                    if (def == null)
                    {
                        throw new InvalidOperationException("unknown scenario " + c.Name);
                    }

                    string[] parts = c.Args.Split(',');
                    e.Host.EmptySlots = int.Parse(parts[0]["slots=".Length..]);
                    if (parts[1] == "pack=True" || parts[1] == "pack=true")
                    {
                        int packId = c.Name.StartsWith("settl", StringComparison.Ordinal) ? 1703 : 1704;
                        e.Host.Bag[packId] = e.Item(packId);
                    }

                    e.Items.ClaimSetReward(new Player(), c.Name);
                    return e.Host.TakeLog();
                }
            }
        }

        private static string ItemPredicates(Env e, int id)
        {
            Item it = e.Item(id);
            var sb = new StringBuilder();
            sb.Append(D(it.IsDTS())).Append(D(it.IsDTL())).Append(D(it.IsDHD())).Append(D(it.IsDHD1())).Append(D(it.IsDTLNew()))
                .Append(D(it.IsManhThienSu())).Append(D(it.IsDaMayMan())).Append(D(it.IsDaNangCapTS())).Append(D(it.IsCongThuc()))
                .Append(D(it.IsCongThucVip())).Append(D(it.IsDaNangCap())).Append(D(it.IsSachTuyetKy())).Append(D(it.IsSachTuyetKy2()))
                .Append(D(it.IsDaPhaLeEpSao())).Append(D(it.IsDaPhaLeC1())).Append(D(it.IsDaPhaLeC2())).Append(D(it.IsDaPhaLeMoi()))
                .Append(D(it.IsDaPhaLeCu())).Append(D(it.IsTypeBody())).Append(D(it.CanPhaLeHoa())).Append(D(it.IsDoKyGui()))
                .Append(D(it.IsItemCollection())).Append(D(it.IsItemTemplateToPet())).Append(D(it.IsItemTemplateToPlayer()))
                .Append(D(it.IsTrangBiPSH())).Append(D(it.IsTrangBiKhoaGd())).Append(D(it.IsTrangBiHacHoa()));
            sb.Append('|').Append(it.TypeName()).Append('|').Append(it.GetGenderName()).Append('|').Append(it.TypeManh());
            sb.Append('|').Append(it.GetInfo()).Append('|').Append(it.GetContent()).Append('|').Append(it.Name());
            foreach (int stoneId in new[] { 223, 222, 224, 221, 220, 5 })
            {
                sb.Append(D(it.CanNangCapWithNDC(e.Item(stoneId))));
            }

            return sb.ToString();
        }

        private static string ItemOptionPredicates(Env e, string args)
        {
            string[] parts = args.Split(';');
            Item it = e.Item(int.Parse(parts[0]), Pairs(parts[1]));
            var sb = new StringBuilder();
            sb.Append(D(it.IsBUg())).Append(D(it.IsSKH())).Append(D(it.HaveSetKichHoat())).Append(D(it.IsTrangBiHSD()))
                .Append(D(it.IsHaveOption(47))).Append(D(it.HaveOption(30))).Append(D(it.IsDoKyGui()));
            sb.Append('|').Append(it.GetOptionParam(47)).Append('|').Append(it.GetInfo()).Append('|').Append(it.GetOptionInfo()).Append('|').Append(it.GetOptionInfoUpgrade())
                .Append('|').Append(it.GetOptionInfoUpgradeFinal()).Append('|').Append(Dump(it.CloneItem().ItemOptions));
            foreach (int stone in new[] { 14, 16, 20, 441, 447, 1416, 1426, 1433, 1434, 300 })
            {
                string r;
                try
                {
                    r = it.GetOptionInfo(e.Item(stone, 1, 1)) + "#" + it.GetOptionInfoCuongHoa(e.Item(stone, 1, 1));
                }
                catch (Exception ex)
                {
                    r = JavaName(ex);
                }

                sb.Append('|').Append(r);
            }

            return sb.ToString();
        }

        private static string ItemEdits(Env e, string sequence)
        {
            Item it = e.Item(5, 47, 10, 21, 5, 47, 3);
            foreach (string op in sequence.Split(';'))
            {
                string[] p = op.Split(':');
                int a = int.Parse(p[0] == "rm" || p.Length > 1 ? p[1] : "0");
                int b = p.Length > 2 ? int.Parse(p[2]) : 0;
                switch (p[0])
                {
                    case "add":
                        it.AddOptionParam(a, b);
                        break;
                    case "set":
                        it.SetOptionParam(a, b);
                        break;
                    case "sub":
                        it.SubOptionParam(a, b);
                        break;
                    case "subrm":
                        it.SubOptionParamAndRemoveIfZero(a, b);
                        break;
                    case "rm":
                        it.RemoveOption(a);
                        break;
                    case "get":
                        it.GetOptionParam(a, b);
                        break;
                }
            }

            return Dump(it.ItemOptions);
        }
    }
}
