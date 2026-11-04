using System.Reflection;
using System.Text.Json;
using Nro.Core.Templates;
using Xunit.Abstractions;

namespace Nro.Tests.Unit.Core.Templates;

/// <summary>
/// Compares the 16 <c>Nro.Core.Templates</c> classes with the 16 REAL Java classes <c>nro/template/*</c> (compiled, with reflection and real calls to
/// the constructors) through <c>tests/golden/templates.json</c>. Regenerate that file with <c>tools/templates/regen.py</c>.
/// The "hand-written canary" group deliberately does NOT read the golden file: it pins down things that are easy to get silently wrong (signed bytes, names with a capital letter in the middle)
/// so that an error present in both the C# code and the test's mapping table is still caught.
/// </summary>
public class TemplatesParityTests(ITestOutputHelper output)
{
    // Hard-coded list per packet A-04, independent of the golden file: if the golden file is generated with a class missing, the test fails.
    private static readonly string[] ExpectedTopLevel =
    [
        "AchievementQuest", "AchievementTemplate", "ArrHead2Frames", "BgItem", "FlagBag", "HeadAvatar",
        "ItemOptionTemplate", "ItemTemplate", "MobTemplate", "NpcTemplate", "Part", "PartDetail",
        "Template", "TileSetInfo", "TileSetInfoData", "WaitSuperRank",
    ];

    private static readonly string[] ExpectedNested = ["BauCuaData", "TaiXiuData", "TaiXiuMD5Data", "XocDiaData"];

    // Fields that Java leaves null after construction (including via the default constructor). Each entry is a null DECISION that was put into the
    // report: C# declares them non-null and uses neutral values (empty string, empty array) because nowhere in the Java code is null checked for
    // these fields. Add or remove entries here only after the decision has been reviewed.
    private static readonly string[] ExpectedNullDefaults =
    [
        "nro.template.FlagBag.iconEffect",
        "nro.template.FlagBag.name",
        "nro.template.ItemOptionTemplate.name",
        "nro.template.ItemTemplate.description",
        "nro.template.ItemTemplate.name",
        "nro.template.MobTemplate.name",
        "nro.template.NpcTemplate.name",
    ];

    public static IEnumerable<object[]> AllClassNames()
    {
        return JavaGolden.Instance.Classes.Select(c => new object[] { c.Name });
    }

    // ------------------------------------------------------------------------------------------------ golden
    [Fact]
    public void Golden_has_all_16_top_level_classes_and_the_4_nested_classes_of_Template()
    {
        JavaGolden g = JavaGolden.Instance;

        Assert.Equal("nro-templates-golden/1", g.Schema);
        string[] top = g.Classes.Where(c => c.Enclosing is null).Select(c => c.SimpleName).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(ExpectedTopLevel.Order(StringComparer.Ordinal).ToArray(), top);

        string[] nested = g.Classes.Where(c => c.Enclosing is not null).Select(c => c.Name).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(ExpectedNested.Select(n => "nro.template.Template$" + n).Order(StringComparer.Ordinal).ToArray(), nested);
        Assert.All(g.Classes.Where(c => c.Enclosing is not null), c => Assert.Equal("nro.template.Template", c.Enclosing));
    }

    [Fact]
    public void Golden_records_the_hashes_of_exactly_16_Java_files()
    {
        JavaGolden g = JavaGolden.Instance;

        Assert.Equal(
            ExpectedTopLevel.Select(n => $"nro/template/{n}.java").Order(StringComparer.Ordinal).ToArray(),
            g.SourcePaths.Order(StringComparer.Ordinal).ToArray());
        Assert.All(g.SourceHashes, h => Assert.Matches("^[0-9a-f]{64}$", h));
        Assert.Equal(g.SourceHashes.Count, g.SourceHashes.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Null_decisions_match_exactly_the_set_of_fields_Java_leaves_null()
    {
        var nullFields = new SortedSet<string>(StringComparer.Ordinal);
        foreach (JavaClass jc in JavaGolden.Instance.Classes)
        {
            foreach (JavaInstance inst in jc.Instances)
            {
                foreach (JsonProperty p in inst.State.EnumerateObject())
                {
                    if (p.Value.ValueKind == JsonValueKind.Null)
                    {
                        nullFields.Add($"{jc.Name}.{p.Name}");
                    }
                }
            }
        }

        Assert.Equal(ExpectedNullDefaults, nullFields.ToArray());
    }

    // ------------------------------------------------------------------------------------------------ per class
    [Theory]
    [MemberData(nameof(AllClassNames))]
    public void Class_exists_and_has_the_right_shape(string javaClass)
    {
        (JavaClass jc, VerifyResult r) = Start(javaClass);

        TemplatesVerifier.CheckShape(jc, r);

        AssertNoErrors(r);
        Assert.Equal(1, r.Classes);
        Assert.Equal(jc.Nested.Length, r.NestedTypes);
    }

    [Theory]
    [MemberData(nameof(AllClassNames))]
    public void Every_Java_field_has_a_CSharp_property_with_the_right_name_type_and_access(string javaClass)
    {
        (JavaClass jc, VerifyResult r) = Start(javaClass);

        TemplatesVerifier.CheckFields(jc, r);

        AssertNoErrors(r);
        Assert.Equal(jc.Fields.Length, r.Fields);
    }

    [Theory]
    [MemberData(nameof(AllClassNames))]
    public void CSharp_constructors_match_Java_constructors_in_both_types_and_parameter_names(string javaClass)
    {
        (JavaClass jc, VerifyResult r) = Start(javaClass);

        TemplatesVerifier.CheckConstructors(jc, r);

        AssertNoErrors(r);
        Assert.Equal(jc.Ctors.Length, r.Constructors);
    }

    [Theory]
    [MemberData(nameof(AllClassNames))]
    public void CSharp_methods_match_Java_and_there_are_no_extra_methods(string javaClass)
    {
        (JavaClass jc, VerifyResult r) = Start(javaClass);

        TemplatesVerifier.CheckMethods(jc, r);

        AssertNoErrors(r);
        Assert.Equal(jc.Methods.Length, r.Methods);
    }

    [Theory]
    [MemberData(nameof(AllClassNames))]
    public void Sample_instantiation_gives_exactly_the_state_of_the_real_Java_constructor_call(string javaClass)
    {
        (JavaClass jc, VerifyResult r) = Start(javaClass);

        TemplatesVerifier.CheckInstances(jc, r);

        AssertNoErrors(r);
        Assert.Equal(jc.Instances.Length, r.Instances);
    }

    [Theory]
    [MemberData(nameof(AllClassNames))]
    public void Assigning_through_a_setter_and_reading_back_gives_the_right_value_without_changing_other_fields(string javaClass)
    {
        (JavaClass jc, VerifyResult r) = Start(javaClass);

        TemplatesVerifier.CheckSetters(jc, r);

        AssertNoErrors(r);
        Assert.Equal(jc.InstanceFields.Count(), r.Setters + r.SettersSkipped);
    }

    [Theory]
    [MemberData(nameof(AllClassNames))]
    public void Reference_type_properties_are_declared_non_null(string javaClass)
    {
        (JavaClass jc, VerifyResult r) = Start(javaClass);

        TemplatesVerifier.CheckNullability(jc, r);

        AssertNoErrors(r);
    }

    [Fact]
    public void Total_number_of_items_compared_equals_the_total_in_golden()
    {
        JavaGolden g = JavaGolden.Instance;
        var r = new VerifyResult();

        foreach (JavaClass jc in g.Classes)
        {
            TemplatesVerifier.CheckClass(jc, r);
            TemplatesVerifier.CheckNullability(jc, r);
        }

        output.WriteLine(
            $"Compared: {r.Classes} classes ({r.NestedTypes} nested), {r.Fields} fields, {r.Constructors} constructors, "
            + $"{r.Methods} methods, {r.Instances} sample instantiations, {r.Setters} setter assign-and-read-back checks ({r.SettersSkipped} array/list fields checked elsewhere), "
            + $"{r.Values} values compared ({r.NullDefaults} null->neutral defaults)");
        AssertNoErrors(r);
        Assert.Equal(g.Classes.Count, r.Classes);
        Assert.Equal(g.Classes.Sum(c => c.Nested.Length), r.NestedTypes);
        Assert.Equal(g.Classes.Sum(c => c.Fields.Length), r.Fields);
        Assert.Equal(g.Classes.Sum(c => c.Ctors.Length), r.Constructors);
        Assert.Equal(g.Classes.Sum(c => c.Methods.Length), r.Methods);
        Assert.Equal(g.Classes.Sum(c => c.Instances.Length), r.Instances);
        Assert.Equal(g.Classes.Sum(c => c.InstanceFields.Count()), r.Setters + r.SettersSkipped);
        Assert.Equal(ExpectedNullDefaults.Length, r.NullDefaults);
        Assert.True(r.Fields > 0 && r.Values > 0 && r.Setters > 0, "there must be at least one field, one compared value and one setter assignment");
    }

    // ------------------------------------------------------------------------------------------------ hand-written canaries (do not read golden)
    [Theory]
    [InlineData("byte", typeof(sbyte))]
    [InlineData("short", typeof(short))]
    [InlineData("int", typeof(int))]
    [InlineData("long", typeof(long))]
    [InlineData("boolean", typeof(bool))]
    [InlineData("java.lang.String", typeof(string))]
    [InlineData("short[]", typeof(short[]))]
    [InlineData("int[]", typeof(int[]))]
    [InlineData("java.util.List<java.lang.Integer>", typeof(List<int>))]
    [InlineData("java.util.List<nro.template.PartDetail>", typeof(List<PartDetail>))]
    public void Type_mapping_table_follows_section_3_3_of_the_analysis_document(string javaType, Type expected)
    {
        Assert.Equal(expected, JavaTypeMap.ToClr(javaType));
    }

    [Theory]
    [InlineData("id", "Id")]
    [InlineData("iconID", "IconID")]
    [InlineData("iconId", "IconId")]
    [InlineData("isUpToUp", "IsUpToUp")]
    [InlineData("isRecieve", "IsRecieve")]
    [InlineData("ruby_sell", "Ruby_sell")]
    [InlineData("percent_gold", "Percent_gold")]
    [InlineData("TypeEvent", "TypeEvent")]
    [InlineData("LUNNAR_NEW_YEAR", "LUNNAR_NEW_YEAR")]
    [InlineData("chanX15", "ChanX15")]
    public void Naming_rule_capitalizes_only_the_first_letter_section_3_5(string javaName, string expected)
    {
        Assert.Equal(expected, JavaTypeMap.ToClrName(javaName));
    }

    [Theory]
    [InlineData(typeof(PartDetail), "Dx", typeof(sbyte))]
    [InlineData(typeof(PartDetail), "Dy", typeof(sbyte))]
    [InlineData(typeof(PartDetail), "IconId", typeof(short))]
    [InlineData(typeof(BgItem), "Layer", typeof(sbyte))]
    [InlineData(typeof(BgItem), "Dx", typeof(short))]
    [InlineData(typeof(BgItem), "Dy", typeof(short))]
    [InlineData(typeof(BgItem), "IdImage", typeof(short))]
    [InlineData(typeof(BgItem), "Id", typeof(int))]
    [InlineData(typeof(ItemTemplate), "Id", typeof(short))]
    [InlineData(typeof(ItemTemplate), "Type", typeof(sbyte))]
    [InlineData(typeof(ItemTemplate), "Gender", typeof(sbyte))]
    [InlineData(typeof(ItemTemplate), "Level", typeof(sbyte))]
    [InlineData(typeof(ItemTemplate), "IsGender", typeof(sbyte))]
    [InlineData(typeof(ItemTemplate), "IconID", typeof(short))]
    [InlineData(typeof(ItemTemplate), "Part", typeof(short))]
    [InlineData(typeof(ItemTemplate), "IsUpToUp", typeof(bool))]
    [InlineData(typeof(ItemTemplate), "Ruby_sell", typeof(int))]
    [InlineData(typeof(ItemTemplate), "TypeEvent", typeof(int))]
    [InlineData(typeof(MobTemplate), "Type", typeof(sbyte))]
    [InlineData(typeof(MobTemplate), "RangeMove", typeof(sbyte))]
    [InlineData(typeof(MobTemplate), "Speed", typeof(sbyte))]
    [InlineData(typeof(MobTemplate), "DartType", typeof(sbyte))]
    [InlineData(typeof(MobTemplate), "PercentDame", typeof(sbyte))]
    [InlineData(typeof(MobTemplate), "PercentTiemNang", typeof(sbyte))]
    [InlineData(typeof(MobTemplate), "Percent_gold", typeof(sbyte))]
    [InlineData(typeof(MobTemplate), "Hp", typeof(int))]
    [InlineData(typeof(FlagBag), "IconId", typeof(short))]
    [InlineData(typeof(FlagBag), "IconEffect", typeof(short[]))]
    [InlineData(typeof(AchievementQuest), "Completed", typeof(long))]
    [InlineData(typeof(AchievementQuest), "IsRecieve", typeof(bool))]
    [InlineData(typeof(AchievementTemplate), "Money", typeof(int))]
    [InlineData(typeof(AchievementTemplate), "MaxCount", typeof(long))]
    [InlineData(typeof(Part), "PartDetails", typeof(List<PartDetail>))]
    [InlineData(typeof(ArrHead2Frames), "Frames", typeof(List<int>))]
    [InlineData(typeof(TileSetInfo), "TileSetInfoData", typeof(List<TileSetInfoData>))]
    [InlineData(typeof(TileSetInfoData), "Data", typeof(int[]))]
    [InlineData(typeof(WaitSuperRank), "RivalId", typeof(long))]
    [InlineData(typeof(Template.XocDiaData), "LeX15", typeof(long))]
    [InlineData(typeof(Template.BauCuaData), "Ga", typeof(long))]
    public void Canary_hand_written_property_types_without_reading_golden(Type owner, string property, Type expected)
    {
        PropertyInfo? p = owner.GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.NotNull(p);
        Assert.Equal(expected, p.PropertyType);
        Assert.NotNull(p.SetMethod);
        Assert.True(p.SetMethod.IsPublic, "a public Java field must be assignable from outside");
    }

    [Fact]
    public void Canary_signed_Java_byte_keeps_negative_values_intact()
    {
        var detail = new PartDetail(short.MinValue, sbyte.MinValue, sbyte.MaxValue);
        var mob = new MobTemplate { Type = -1, Percent_gold = -128, Speed = 127 };
        var item = new ItemTemplate(-1, -128, 127, "n", "d", -2, -3, true, -4, -5, -6);

        Assert.Equal(short.MinValue, detail.IconId);
        Assert.Equal((sbyte)-128, detail.Dx);
        Assert.Equal((sbyte)127, detail.Dy);
        Assert.Equal((sbyte)-1, mob.Type);
        Assert.Equal((sbyte)-128, mob.Percent_gold);
        Assert.Equal((sbyte)127, mob.Speed);
        Assert.Equal(new[] { (short)-1, (short)-2, (short)-3 }, new[] { item.Id, item.IconID, item.Part });
        Assert.Equal(new[] { (sbyte)-128, (sbyte)127, (sbyte)-6 }, new[] { item.Type, item.Gender, item.IsGender });
        Assert.Equal(new[] { -4, -5 }, new[] { item.StrRequire, item.TypeEvent });
    }

    [Fact]
    public void Canary_ItemTemplate_11_event_flags_are_protected_not_constants_and_have_values_1_to_11()
    {
        // A subclass being able to read them proves the members are protected; the values 1..11 are copied from the Java declarations (protected byte X = n).
        var probe = new ItemTemplateProbe();

        Assert.Equal(new sbyte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, probe.EventFlags());
        Assert.Equal((sbyte)0, probe.Level);
    }

    [Fact]
    public void Canary_pre_initialized_lists_are_empty_non_null_and_independent_between_objects()
    {
        var part1 = new Part();
        var part2 = new Part();
        var frames1 = new ArrHead2Frames();
        var frames2 = new ArrHead2Frames();

        part1.PartDetails.Add(new PartDetail(1, 2, 3));
        frames1.Frames.Add(7);

        Assert.Single(part1.PartDetails);
        Assert.Empty(part2.PartDetails);
        Assert.Single(frames1.Frames);
        Assert.Empty(frames2.Frames);
    }

    // ------------------------------------------------------------------------------------------------ helpers
    private static void AssertNoErrors(VerifyResult r)
    {
        // Print every error line in full (xUnit's Assert.Empty truncates long elements).
        if (r.Errors.Count > 0)
        {
            Assert.Fail(string.Join(Environment.NewLine, r.Errors));
        }
    }

    private static (JavaClass Class, VerifyResult Result) Start(string javaClass)
    {
        return (JavaGolden.Instance.ByName[javaClass], new VerifyResult());
    }

    /// <summary>A subclass used only to read the protected members of <see cref="ItemTemplate"/>.</summary>
    private sealed class ItemTemplateProbe : ItemTemplate
    {
        public sbyte[] EventFlags()
        {
            return
            [
                LUNNAR_NEW_YEAR, CHRISTMAS, VU_LAN_FESTIVAL, HALLOWEEN, INTERNATIONAL_WOMANS_DAY, TRUNG_THU,
                HUNG_VUONG, BLACK_FRIDAY, WORLD_CUP, VALENTINE, DAY_20_10,
            ];
        }
    }
}
