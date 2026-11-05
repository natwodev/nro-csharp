using Nro.Content.Catalogs;
using Nro.Game;
using Nro.Game.Content;
using Nro.Game.Effects.Ports;
using Nro.Game.Skills;
using Nro.Game.Skills.Ports;

namespace Nro.Tests.Unit.Game.Skills;

// Nguồn Java: Utils/SkillUtil.java (dòng 22–407).
public sealed class SkillUtilTests
{
    // Tạo catalog ba lớp nhỏ giữ thứ tự Java Manager.NCLASS. (Java: static NCLASS)
    private static ISkillCatalog CreateCatalog() => new SkillCatalog(
    [
        CreateClass(0, Skill.DRAGON, Skill.KAMEJOKO),
        CreateClass(1, Skill.DEMON, Skill.MASENKO),
        CreateClass(2, Skill.GALICK, Skill.ANTOMIC),
    ]);

    // Tạo lớp chứa hai template ba cấp cho kiểm thử factory. (Java: NClass)
    private static NClass CreateClass(int classId, params sbyte[] templateIds)
    {
        var result = new NClass { ClassId = classId, Name = $"Class {classId}" };
        foreach (sbyte templateId in templateIds)
        {
            var template = new SkillTemplate { Id = templateId, ClassId = classId, Name = $"Skill {templateId}" };
            for (int point = 1; point <= 3; point++)
            {
                template.Skillss.Add(new Skill { Template = template, SkillId = (short)point, Point = point, CoolDown = 100 * point });
            }

            result.SkillTemplatess.Add(template);
        }

        return result;
    }

    // Tạo SkillUtil với map ngoại tuyến có thể kiểm soát. (Java: MapService.gI)
    private static SkillUtil CreateSubject(params int[] offlineMapIds) => new(CreateCatalog(), new TestSkillMapRules(offlineMapIds));

    // Kiểm factory/template ở dòng Java 22–69, gồm bản sao và cấp 0. (Java: createSkill/findSkillTemplate/createEmptySkill/getSkill/createSkillLevel0)
    [Fact]
    public void Factory_and_template_lookup_follow_the_three_java_classes()
    {
        SkillUtil subject = CreateSubject();

        Assert.Equal(Skill.DRAGON, subject.FindSkillTemplate(Skill.DRAGON)!.Id);
        Assert.Equal(Skill.DEMON, subject.FindSkillTemplate(Skill.DEMON)!.Id);
        Assert.Equal(Skill.GALICK, subject.FindSkillTemplate(Skill.GALICK)!.Id);
        Assert.Null(subject.FindSkillTemplate(99));

        Skill copy = subject.CreateSkill(Skill.DEMON, 2)!;
        Skill source = subject.GetSkill(Skill.DEMON, 2);
        Assert.NotSame(source, copy);
        Assert.Equal(source.Template!.Id, copy.Template!.Id);
        Assert.Equal(2, copy.Point);
        Assert.Null(subject.CreateSkill(Skill.DEMON, 0));
        Assert.Null(subject.CreateSkill(Skill.DEMON, 4));

        Assert.Equal(-1, SkillUtil.CreateEmptySkill().SkillId);
        Skill levelZero = SkillUtil.CreateSkillLevel0(200);
        Assert.Equal(-1, levelZero.SkillId);
        Assert.Equal(unchecked((sbyte)200), levelZero.Template!.Id);
    }

    // Kiểm lựa chọn đấm/chưởng và mặc định phân thân ở dòng Java 72–90. (Java: isUseSkillDam/isUseSkillChuong/getPercentPhanThan)
    [Theory]
    [InlineData(Skill.DRAGON, true, false)]
    [InlineData(Skill.LIEN_HOAN, true, false)]
    [InlineData(Skill.KAMEJOKO, false, true)]
    [InlineData(Skill.MASENKO, false, true)]
    [InlineData(Skill.TU_SAT, false, false)]
    public void Selected_skill_conditions_match_java(sbyte templateId, bool punch, bool blast)
    {
        var player = new Player();
        player.PlayerSkill.SkillSelect = new Skill { Template = new SkillTemplate { Id = templateId }, Point = 4 };

        Assert.Equal(punch, SkillUtil.IsUsingPunchSkill(player));
        Assert.Equal(blast, SkillUtil.IsUsingBlastSkill(player));
        Assert.Equal(40, SkillUtil.GetClonePercent(player));
        Assert.Equal(10, SkillUtil.GetClonePercent(null));
        player.PlayerSkill.SkillSelect = null;
        Assert.Equal(10, SkillUtil.GetClonePercent(player));
    }

    // Bảng đầu vào/kết quả các công thức một tham số ở dòng Java 92–170. (Java: các hàm getTime/getPercent/getRange)
    [Theory]
    [MemberData(nameof(LevelRules))]
    public void Level_rules_match_java_table(Func<int, int> rule, int level, int expected) => Assert.Equal(expected, rule(level));

    // Cung cấp bảng công thức một tham số để mỗi dòng Java có kết quả kiểm chứng. (Java: dòng 92–170)
    public static IEnumerable<object[]> LevelRules()
    {
        yield return [(Func<int, int>)SkillUtil.GetTimeMonkey, 2, 70_000];
        yield return [(Func<int, int>)SkillUtil.GetPercentHpMonkey, 2, 50];
        yield return [(Func<int, int>)SkillUtil.GetMonkeyDamagePercent, 2, 5];
        yield return [(Func<int, int>)SkillUtil.GetStunDuration, 2, 4_000];
        yield return [(Func<int, int>)SkillUtil.GetBottleDuration, 7, 10_000];
        yield return [(Func<int, int>)SkillUtil.GetTimeShield, 2, 20_000];
        yield return [(Func<int, int>)SkillUtil.GetBindDuration, 2, 10_000];
        yield return [(Func<int, int>)SkillUtil.GetTeleportBlindDuration, 2, 1_500];
        yield return [(Func<int, int>)SkillUtil.GetHypnosisDuration, 2, 6_000];
        yield return [(Func<int, int>)SkillUtil.GetStunRange, 2, 180];
        yield return [(Func<int, int>)SkillUtil.GetSelfDestructRange, 2, 660];
        yield return [(Func<int, int>)SkillUtil.GetEnergyBallRange, 2, 410];
        yield return [(Func<int, int>)SkillUtil.GetWhistleHpPercent, 2, 50];
        yield return [(Func<int, int>)SkillUtil.GetPowerBurstDamagePercent, 2, 15];
        yield return [(Func<int, int>)SkillUtil.GetPowerBurstDuration, 2, 35_000];
        yield return [(Func<int, int>)SkillUtil.GetHealHpPercent, 2, 55];
        yield return [(Func<int, int>)SkillUtil.GetPercentCharge, 2, 5];
    }

    // Kiểm ba thời lượng cố định ở dòng Java 108–125. (Java: getTimeSocola/getTimeTranformation/getTimett)
    [Fact]
    public void Fixed_duration_rules_match_java()
    {
        Assert.Equal(30_000, SkillUtil.GetChocolateDuration());
        Assert.Equal(300_000, SkillUtil.GetTimeTranformation());
        Assert.Equal(300_000, SkillUtil.GetTimett());
    }

    // Kiểm bảng đệ trứng và Vòng Kim Cô ở dòng Java 172–204, gồm cấp biên. (Java: getTempMobMe đến getDAMEVongKimCo)
    [Fact]
    public void Summon_rules_match_java_tables_and_reject_out_of_range_levels()
    {
        Assert.Equal(8, SkillUtil.GetEggMobTemplateId(1));
        Assert.Equal(50, SkillUtil.GetEggMobTemplateId(7));
        Assert.Equal(160_000, SkillUtil.GetEggMobSurvivalDuration(3));
        Assert.Equal(900, SkillUtil.GetEggMobHp(1_000, 7));
        Assert.Equal(69, SkillUtil.GetGoldenHeadbandTemplateId(1));
        Assert.Equal(92, SkillUtil.GetGoldenHeadbandTemplateId(5));
        Assert.Equal(180_000, SkillUtil.GetGoldenHeadbandSurvivalDuration(5));
        Assert.Equal(1_400, SkillUtil.GetGoldenHeadbandHp(1_000, 5));
        Assert.Equal(1_400, SkillUtil.GetGoldenHeadbandDamage(1_000, 5));
        Assert.Throws<IndexOutOfRangeException>(() => SkillUtil.GetEggMobTemplateId(0));
        Assert.Throws<IndexOutOfRangeException>(() => SkillUtil.GetGoldenHeadbandTemplateId(6));
    }

    // Kiểm lookup/nâng cấp pet ở dòng Java 206–238, gồm mức tối đa và hồi chiêu ô thứ hai. (Java: getSkillbyId/upSkillPet)
    [Fact]
    public void Lookup_and_pet_upgrade_follow_java()
    {
        SkillUtil subject = CreateSubject();
        var player = new Player();
        Skill first = subject.CreateSkill(Skill.DRAGON, 1)!;
        Skill second = subject.CreateSkill(Skill.DRAGON, 2)!;
        player.PlayerSkill.Skills!.Add(first);
        player.PlayerSkill.Skills.Add(second);

        Assert.Same(first, SkillUtil.GetSkillById(player, Skill.DRAGON));
        Assert.Null(SkillUtil.GetSkillById(player, Skill.ANTOMIC));
        Assert.True(subject.UpSkillPet(player.PlayerSkill.Skills, 1));
        Assert.Equal(3, player.PlayerSkill.Skills[1].Point);
        Assert.Equal(1000, player.PlayerSkill.Skills[1].CoolDown);

        player.PlayerSkill.Skills[0] = new Skill { Template = new SkillTemplate { Id = Skill.DRAGON }, Point = 7 };
        Assert.False(subject.UpSkillPet(player.PlayerSkill.Skills, 0));
    }

    // Kiểm toàn bộ dải vật phẩm sách kỹ năng ở dòng Java 240–387. (Java: getTempSkillSkillByItemID/getSkillByItemID/getSkillByLevel)
    [Theory]
    [MemberData(nameof(ItemSkillRules))]
    public void Item_skill_ranges_match_java(int itemTemplateId, sbyte expectedTemplateId)
    {
        SkillUtil subject = CreateSubject();
        var player = new Player();
        player.PlayerSkill.Skills!.Add(subject.CreateSkill(expectedTemplateId, 1) ?? new Skill { Template = new SkillTemplate { Id = expectedTemplateId } });

        Assert.Equal(expectedTemplateId, SkillUtil.GetTemplateIdByItemId(itemTemplateId));
        Assert.Equal(expectedTemplateId, SkillUtil.GetSkillByItemId(player, itemTemplateId)!.Template!.Id);
    }

    // Cung cấp điểm đầu mỗi dải vật phẩm học kỹ năng. (Java: dòng 240–284)
    public static IEnumerable<object[]> ItemSkillRules()
    {
        yield return [66, Skill.DRAGON];
        yield return [79, Skill.DEMON];
        yield return [87, Skill.GALICK];
        yield return [94, Skill.KAMEJOKO];
        yield return [101, Skill.MASENKO];
        yield return [108, Skill.ANTOMIC];
        yield return [115, Skill.THAI_DUONG_HA_SAN];
        yield return [122, Skill.TRI_THUONG];
        yield return [129, Skill.TAI_TAO_NANG_LUONG];
        yield return [300, Skill.KAIOKEN];
        yield return [307, Skill.QUA_CAU_KENH_KHI];
        yield return [314, Skill.BIEN_KHI];
        yield return [321, Skill.TU_SAT];
        yield return [328, Skill.MAKANKOSAPPO];
        yield return [335, Skill.DE_TRUNG];
        yield return [434, Skill.KHIEN_NANG_LUONG];
        yield return [474, Skill.SOCOLA];
        yield return [481, Skill.LIEN_HOAN];
        yield return [488, Skill.DICH_CHUYEN_TUC_THOI];
        yield return [495, Skill.THOI_MIEN];
        yield return [502, Skill.TROI];
        yield return [509, Skill.HUYT_SAO];
    }

    // Kiểm biên dải vật phẩm, cấp dùng chung, thay skill và tỉ lệ tấn công ở dòng Java 290–409. (Java: getSkillByItemID/getSkillByLevel/setSkill/getTyleSkillAttack)
    [Fact]
    public void Item_lookup_replace_and_attack_type_follow_java()
    {
        SkillUtil subject = CreateSubject();
        var player = new Player();
        Skill previous = subject.CreateSkill(Skill.KAMEJOKO, 1)!;
        player.PlayerSkill.Skills!.Add(previous);
        Skill replacement = subject.CreateSkill(Skill.KAMEJOKO, 2)!;

        Assert.Equal(Skill.DEMON, SkillUtil.GetTemplateIdByItemId(86));
        Assert.Equal(-1, SkillUtil.GetTemplateIdByItemId(85));
        Assert.Null(SkillUtil.GetSkillByItemId(player, 85));
        Assert.Equal(2, subject.GetSkillByLevel(94, 2)!.Point);
        Assert.Null(subject.GetSkillByLevel(85, 2));
        SkillUtil.SetSkill(player, replacement);
        Assert.Same(replacement, player.PlayerSkill.Skills[0]);
        Assert.Equal(2, SkillUtil.GetSkillAttackType(new Skill { Template = new SkillTemplate { Id = Skill.TRI_THUONG } }));
        Assert.Equal(1, SkillUtil.GetSkillAttackType(replacement));
        Assert.Equal(0, SkillUtil.GetSkillAttackType(new Skill { Template = new SkillTemplate { Id = Skill.DRAGON } }));
    }

    // Kiểm SkillUtil cài IEffectRules và chuyển map offline qua cổng. (Java: SkillUtil timing/MapService map predicates)
    [Fact]
    public void Effect_rules_are_provided_by_skill_util()
    {
        SkillUtil subject = CreateSubject(44);
        IEffectRules effects = subject;

        Assert.True(effects.IsMapOffline(44));
        Assert.False(effects.IsMapOffline(45));
        Assert.True(effects.IsMapBlackBallWar(85));
        Assert.False(effects.IsMapBlackBallWar(92));
        Assert.True(effects.IsMapMaBu12H(120));
        Assert.False(effects.IsMapMaBu12H(121));
        Assert.Equal(60_000, effects.GetTimeMonkey(1));
        Assert.Equal(15_000, effects.GetTimeShield(1));
        Assert.Equal(300_000, effects.GetTimeTranformation());
        Assert.Equal(300_000, effects.GetTimett());
        Assert.Equal(30_000, effects.GetPowerBurstDuration(1));
    }

    // Cổng map tối thiểu cho kiểm thử SkillUtil. (Java: MapService.isMapOffline)
    private sealed class TestSkillMapRules : ISkillMapRules
    {
        // Tập id map ngoại tuyến của fixture. (Java: Manager.MAPS)
        private readonly HashSet<int> offline;

        // Khởi tạo tập map offline từ dữ liệu kiểm thử. (Java: Manager.MAPS)
        public TestSkillMapRules(params int[] offlineMapIds) => offline = [.. offlineMapIds];

        // Kiểm tra map có nằm trong tập offline dựng sẵn. (Java: isMapOffline)
        public bool IsMapOffline(int mapId) => offline.Contains(mapId);
    }
}
