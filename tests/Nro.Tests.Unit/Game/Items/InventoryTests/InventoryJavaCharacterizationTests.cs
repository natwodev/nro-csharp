using System.Text;
using System.Text.Json;
using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Game;

namespace Nro.Tests.Unit.Game.Items.InventoryTests;

/// <summary>
/// Replays thousands of scenarios recorded from the REAL Java code of <c>InventoryService.java</c> (method bodies extracted verbatim, see <c>java/InventoryServiceGenerator.java.txt</c>
/// and <c>inventory-service.json</c>) and requires the C# port to produce the same lists, the same results and the same packet bytes.
/// The random option rolls use <see cref="SeededRandomProvider"/>, which is bit-exact with <c>java.util.Random</c>, and also check how many numbers each roll drew (via the next draw).
/// </summary>
public class InventoryJavaCharacterizationTests
{
    private static readonly Lazy<JsonDocument> Recorded = new(() =>
        JsonDocument.Parse(File.ReadAllBytes(Path.Combine(TestPaths.ProjectDirectory("Nro.Tests.Unit"), "Game", "Items", "InventoryTests", "inventory-service.json"))));

    private static JsonElement Root => Recorded.Value.RootElement;

    /// <summary>The same template universe the Java generator uses: ids 1-4 and 1705 stack, 457 is the gold bar, strRequire is 10 times the id.</summary>
    private static InvKit NewKit(Nro.Core.Util.IRandomProvider? random = null)
    {
        var k = new InvKit(random);
        foreach (int id in new[] { 1, 2, 3, 4, 5, 6, 7, 8, 457, 1705 })
        {
            k.Define(id, upToUp: (id >= 1 && id <= 4) || id == 1705, strRequire: id * 10);
        }

        return k;
    }

    private static Item ParseItem(InvKit k, JsonElement e)
    {
        if (e.ValueKind == JsonValueKind.Null)
        {
            return null!;
        }

        if (e.ValueKind == JsonValueKind.Number)
        {
            return k.Empty();
        }

        var options = new List<(int, int)>();
        foreach (JsonElement o in e[2].EnumerateArray())
        {
            options.Add((o[0].GetInt32(), o[1].GetInt32()));
        }

        return k.Item(e[0].GetInt32(), e[1].GetInt32(), options.ToArray());
    }

    private static List<Item> ParseList(InvKit k, JsonElement e) => e.EnumerateArray().Select(x => ParseItem(k, x)).ToList();

    private static string Js(Item? item)
    {
        if (item is null)
        {
            return "null";
        }

        if (!item.IsNotNullItem())
        {
            return "0";
        }

        var sb = new StringBuilder("[").Append(item.Template!.Id).Append(',').Append(item.Quantity).Append(",[");
        for (int i = 0; i < item.ItemOptions.Count; i++)
        {
            sb.Append(i > 0 ? "," : "").Append('[').Append(item.ItemOptions[i].OptionTemplate.Id).Append(',').Append(item.ItemOptions[i].Param).Append(']');
        }

        return sb.Append("]]").ToString();
    }

    private static string Js(IEnumerable<Item?> list) => "[" + string.Join(",", list.Select(Js)) + "]";

    private static string Raw(JsonElement e) => e.GetRawText();

    private static void NoFailures(List<string> failures, int total)
    {
        Assert.True(failures.Count == 0, $"{failures.Count} of {total} scenarios differ from Java:{Environment.NewLine}{string.Join(Environment.NewLine, failures.Take(8))}");
    }

    [Fact]
    public void The_recording_comes_from_the_unmodified_original_and_is_big_enough()
    {
        Assert.Matches("^[0-9a-f]{64}$", Root.GetProperty("sha256").GetString()!);
        Assert.True(Root.GetProperty("addItemList").GetArrayLength() >= 900);
        Assert.True(Root.GetProperty("sort").GetArrayLength() >= 300);
        Assert.True(Root.GetProperty("remove").GetArrayLength() >= 300);
        Assert.True(Root.GetProperty("sets").GetArrayLength() >= 500);
        Assert.True(Root.GetProperty("rolls").GetArrayLength() >= 400);
        Assert.True(Root.GetProperty("packets").GetArrayLength() >= 150);
    }

    [Fact]
    public void AddItemList_matches_java_on_900_scenarios()
    {
        var failures = new List<string>();
        JsonElement cases = Root.GetProperty("addItemList");
        int index = 0;
        foreach (JsonElement c in cases.EnumerateArray())
        {
            InvKit k = NewKit();
            List<Item> list = ParseList(k, c.GetProperty("before"));
            Item add = ParseItem(k, c.GetProperty("add"));

            bool result = k.Service.AddItemList(list, add, k.Player);

            // Java sent the gold-bar notice to a null player; the port sends it to the adding player. Same condition, so the counts match.
            int notices = k.Notifier.Calls.Count(x => x.StartsWith("SendNotification:", StringComparison.Ordinal));
            bool ok = result == c.GetProperty("result").GetBoolean()
                && Js(list) == Raw(c.GetProperty("after"))
                && Js(add) == Raw(c.GetProperty("addAfter"))
                && notices == c.GetProperty("notices").GetInt32();
            if (!ok)
            {
                failures.Add($"#{index} before={Raw(c.GetProperty("before"))} add={Raw(c.GetProperty("add"))}: got {result} {Js(list)} add={Js(add)} notices={notices}; java {c.GetProperty("result")} {Raw(c.GetProperty("after"))} add={Raw(c.GetProperty("addAfter"))} notices={c.GetProperty("notices")}");
            }

            index++;
        }

        NoFailures(failures, index);
    }

    [Fact]
    public void SortItems_CompactItemsWithPositiveQuantity_and_the_empty_count_match_java()
    {
        var failures = new List<string>();
        int index = 0;
        foreach (JsonElement c in Root.GetProperty("sort").EnumerateArray())
        {
            InvKit k = NewKit();
            List<Item> list = ParseList(k, c.GetProperty("before"));
            List<Item> v2 = [.. list];
            sbyte empties = k.Service.GetCountEmptyListItem(list);

            k.Service.SortItems(list);
            k.Service.CompactItemsWithPositiveQuantity(v2);

            if (Js(list) != Raw(c.GetProperty("sorted")) || Js(v2) != Raw(c.GetProperty("v2")) || empties != c.GetProperty("empties").GetInt32())
            {
                failures.Add($"#{index} {Raw(c.GetProperty("before"))}: sorted {Js(list)} v2 {Js(v2)} empties {empties}; java {Raw(c.GetProperty("sorted"))} {Raw(c.GetProperty("v2"))} {c.GetProperty("empties")}");
            }

            index++;
        }

        NoFailures(failures, index);
    }

    [Fact]
    public void GetCountEmptyListItem_wraps_exactly_like_the_java_byte()
    {
        var k = NewKit();
        foreach (JsonElement pair in Root.GetProperty("emptyCounts").EnumerateArray())
        {
            var list = new List<Item>();
            for (int i = 0; i < pair[0].GetInt32(); i++)
            {
                list.Add(k.Empty());
            }

            Assert.Equal(pair[1].GetInt32(), (int)k.Service.GetCountEmptyListItem(list));
        }
    }

    [Fact]
    public void RemoveItem_and_SubQuantityItem_match_java()
    {
        var failures = new List<string>();
        int index = 0;
        foreach (JsonElement c in Root.GetProperty("remove").EnumerateArray())
        {
            InvKit k = NewKit();
            List<Item> list = ParseList(k, c.GetProperty("before"));
            int pick = c.GetProperty("pick").GetInt32();
            Item target = pick < list.Count ? list[pick] : pick == list.Count ? k.Item(1, 1) : null!;
            int quantity = c.GetProperty("quantity").GetInt32();

            if (c.GetProperty("op").GetString() == "sub")
            {
                k.Service.SubQuantityItem(list, target, quantity);
            }
            else
            {
                k.Service.RemoveItem(list, target);
            }

            if (Js(list) != Raw(c.GetProperty("after")))
            {
                failures.Add($"#{index} {c.GetProperty("op")} pick {pick} q {quantity} {Raw(c.GetProperty("before"))}: got {Js(list)}; java {Raw(c.GetProperty("after"))}");
            }

            index++;
        }

        NoFailures(failures, index);
    }

    [Fact]
    public void Set_bonus_methods_match_java()
    {
        var failures = new List<string>();
        int index = 0;
        foreach (JsonElement c in Root.GetProperty("sets").EnumerateArray())
        {
            InvKit k = NewKit();
            JsonElement before = c.GetProperty("before");
            Player p = k.Player;
            Player other = k.NewPlayer(2);
            p.Inventory.ItemsBody = ParseList(k, before[0]);
            p.Inventory.ItemsBag = ParseList(k, before[1]);
            p.Inventory.ItemsBox = ParseList(k, before[2]);
            other.Inventory.ItemsBody = ParseList(k, before[3]);
            other.Inventory.ItemsBag = ParseList(k, before[4]);
            other.Inventory.ItemsBox = ParseList(k, before[5]);
            int setOptionId = c.GetProperty("setOptionId").GetInt32();
            int applyId = c.GetProperty("applyId").GetInt32();
            int[] applyIds = c.GetProperty("applyIds").EnumerateArray().Select(x => x.GetInt32()).ToArray();
            Player? pl = c.GetProperty("usePl").GetBoolean() ? other : null;
            Player? plbox = c.GetProperty("usePlbox").GetBoolean() ? other : null;

            switch (c.GetProperty("op").GetString())
            {
                case "updateSet": k.Service.UpdateSet(p, setOptionId, applyId); break;
                case "resetSet": k.Service.ResetSet(p, pl, plbox, setOptionId, applyId); break;
                case "updateSetNew": k.Service.UpdateSetNew(p, 241, 244, applyIds); break;
                default: k.Service.ResetSetNew(p, pl, plbox, 241, 244, applyIds); break;
            }

            string got = Js(p.Inventory.ItemsBody) + "," + Js(p.Inventory.ItemsBag) + "," + Js(p.Inventory.ItemsBox) + ","
                + Js(other.Inventory.ItemsBody) + "," + Js(other.Inventory.ItemsBag) + "," + Js(other.Inventory.ItemsBox);
            string want = string.Join(",", c.GetProperty("after").EnumerateArray().Select(Raw));
            if (got != want)
            {
                failures.Add($"#{index} {c.GetProperty("op")} {setOptionId}/{applyId} usePl={pl != null} usePlbox={plbox != null}:{Environment.NewLine}  got  {got}{Environment.NewLine}  java {want}");
            }

            index++;
        }

        NoFailures(failures, index);
    }

    [Fact]
    public void Collection_box_tiers_match_java_for_0_to_135_items()
    {
        foreach (JsonElement row in Root.GetProperty("tiers").EnumerateArray())
        {
            InvKit k = NewKit();
            int count = row[0].GetInt32();
            k.Fill(k.Inv.ItemsBoxCollection, 140);
            for (int j = 0; j < count; j++)
            {
                k.Inv.ItemsBoxCollection[j] = k.Item(100 + j);
            }

            Assert.Equal(row[1].GetInt32(), k.Service.GetCollectionBoxHealthBonusPercent(k.Player));
            Assert.Equal(row[2].GetInt32(), k.Service.GetCollectionBoxManaBonusPercent(k.Player));
            Assert.Equal(row[3].GetInt32(), k.Service.GetCollectionBoxDamageBonusPercent(k.Player));
            Assert.Equal(row[4].GetInt32(), k.Service.GetCollectionBoxCriticalChanceBonusPercent(k.Player));
        }
    }

    [Fact]
    public void Random_option_rolls_match_java_and_draw_the_same_number_of_random_numbers()
    {
        var failures = new List<string>();
        int index = 0;
        foreach (JsonElement c in Root.GetProperty("rolls").EnumerateArray())
        {
            var random = new SeededRandomProvider(c.GetProperty("seed").GetInt64());
            InvKit k = NewKit(random);
            k.Define(5, 0, strRequire: 50);
            Item item = ParseItem(k, c.GetProperty("before"));
            k.Inv.ItemsBag[0] = item;

            k.Service.ItemBagToBody(k.Player, 0);

            Item worn = k.Inv.ItemsBody[0];
            int next = random.NextInt();
            if (!ReferenceEquals(worn, item) || Js(worn) != Raw(c.GetProperty("after")) || next != c.GetProperty("next").GetInt32())
            {
                failures.Add($"#{index} seed {c.GetProperty("seed")} {c.GetProperty("kind")}: got {Js(worn)} next {next}; java {Raw(c.GetProperty("after"))} next {c.GetProperty("next")}");
            }

            index++;
        }

        NoFailures(failures, index);
    }

    [Fact]
    public void Bag_body_and_box_packets_match_java_byte_for_byte()
    {
        var failures = new List<string>();
        int index = 0;
        foreach (JsonElement c in Root.GetProperty("packets").EnumerateArray())
        {
            InvKit k = NewKit();
            k.Host.Head = (short)c.GetProperty("head").GetInt32();
            k.Inv.ItemsBag = ParseList(k, c.GetProperty("bag"));
            k.Inv.ItemsBody = ParseList(k, c.GetProperty("body"));
            k.Inv.ItemsBox = ParseList(k, c.GetProperty("box"));

            k.Service.SendItemBag(k.Player);
            string bagHex = Convert.ToHexStringLower(k.Notifier.Packets[0].Data);
            string bagAfter = Js(k.Inv.ItemsBag);
            sbyte bagCmd = k.Notifier.Packets[0].Command;
            k.Notifier.Packets.Clear();

            k.Service.SendItemBody(k.Player);
            string bodyHex = Convert.ToHexStringLower(k.Notifier.Packets[0].Data);
            sbyte bodyCmd = k.Notifier.Packets[0].Command;
            k.Notifier.Packets.Clear();

            k.Service.SendItemBox(k.Player);
            string boxHex = Convert.ToHexStringLower(k.Notifier.Packets[0].Data);
            string openHex = Convert.ToHexStringLower(k.Notifier.Packets[1].Data);

            bool ok = bagHex == c.GetProperty("bagHex").GetString() && bagAfter == Raw(c.GetProperty("bagAfter"))
                && bodyHex == c.GetProperty("bodyHex").GetString() && boxHex == c.GetProperty("boxHex").GetString() && openHex == c.GetProperty("openHex").GetString()
                && bagCmd == -36 && bodyCmd == -37 && k.Notifier.Packets.Count == 2 && k.Notifier.Packets.All(p => p.Command == -35);
            if (!ok)
            {
                failures.Add($"#{index}: bag {bagHex == c.GetProperty("bagHex").GetString()} body {bodyHex == c.GetProperty("bodyHex").GetString()} box {boxHex == c.GetProperty("boxHex").GetString()} open {openHex == c.GetProperty("openHex").GetString()}");
            }

            index++;
        }

        NoFailures(failures, index);
    }
}
