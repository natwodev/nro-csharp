using Nro.Data.Db;
using Nro.Data.Entities.StaticData;

namespace Nro.Content.StaticData;

// Adapter PostgreSQL chỉ đọc hàng static qua cơ chế INroSqlExecutor; không được gọi bởi bước nạp. Nguồn Java: nro/server/Manager.java (dòng 199–202, 439–705).
public sealed class NroSqlStaticRowSource : IStaticRowSource
{
    private const string PartsSql = "select id, type, data from part order by id";
    private const string BgItemsSql = "select id, image_id, layer, dx, dy from bg_item_template order by id";
    private const string ItemTemplatesSql = "select id, type, gender, name, description, level, icon_id, part, is_up_to_up, power_require, gold, gold_sell, gem, gem_sell, ruby, ruby_sell, head, body, leg, type_event, is_gender from item_template order by id";
    private const string ItemOptionTemplatesSql = "select id, name, type from item_option_template order by id";
    private const string HeadAvatarsSql = "select head_id, avatar_id from head_avatar order by head_id";
    private const string FlagBagsSql = "select id, icon_data, name, gold, gem, icon_id from flag_bag order by id";
    private const string HeadAnimationFramesSql = "select id, data from head_animation_frame order by id";
    private const string ImagesByNameSql = "select id, name, n_frame from img_by_name order by id";
    private const string AttributeTemplatesSql = "select id, name from attribute_template order by id";
    private const string PowerLimitsSql = "select id, power, hp, mp, damage, defense, critical from power_limit order by id";
    private const string CaptionsSql = "select id, earth, saiya, namek, power from caption order by id";
    private const string SkillTemplatesSql = "select nclass_id, id, name, max_point, mana_use_type, type, icon_id, dam_info, slot, skills from skill_template order by nclass_id, slot, id";
    private const string IntrinsicsSql = "select id, name, param_from_1, param_to_1, param_from_2, param_to_2, icon, gender from intrinsic order by id";
    private const string TaskMainsSql = "select id, name, detail from task_main_template order by id";
    private const string TaskSubsSql = "select task_main_id, name, max_count, notify, npc_id, map, idmain from task_sub_template order by task_main_id, idmain";
    private const string SideTaskTemplatesSql = "select id, name, max_count_lv1, max_count_lv2, max_count_lv3, max_count_lv4, max_count_lv5 from side_task_template order by id";
    private const string ClanTaskTemplatesSql = "select id, name, max_count_lv1, max_count_lv2, max_count_lv3, max_count_lv4, max_count_lv5 from clan_task_template order by id";
    private const string KolTaskTemplatesSql = "select id, info, max_count from task_kol_template order by id";
    private const string EventTaskTemplatesSql = "select id, name, max_count_lv1, max_count_lv2, max_count_lv3, max_count_lv4, max_count_lv5 from task_event_template order by id";
    private const string AchievementTemplatesSql = "select id, info1, info2, money, max_count from achievement_template order by id";
    private const string ShopsSql = "select id, npc_id, tag_name, type_shop from shop order by npc_id, id";
    private const string ShopTabsSql = "select id, shop_id, name from tab_shop order by shop_id, id";
    private const string ShopItemsSql = "select id, tab_id, temp_id, is_new, is_sell, type_sell, cost, costgold, icon_spec, create_time from item_shop where is_sell = true order by tab_id, create_time desc, id";
    private const string ShopItemOptionsSql = "select id, item_shop_id, option_id, param from item_shop_option order by item_shop_id, id";
    private const string BadgeTaskTemplatesSql = "select id, name, max_count, id_badges_reward from task_badges_template order by id";
    private const string BadgeTemplatesSql = "select id, id_effect, id_item, name, options from data_badges order by id";
    private const string NotificationsSql = "select id, name, text from notify order by id desc";
    private const string RadarCardsSql = "select id, icon_id, rank, max, type, mob_id, body, name, info, options, aura_id from radar order by id";
    private readonly INroSqlExecutor database;

    // Tạo nguồn hàng từ executor có tham số hóa sẵn của Nro.Data. (Java: ConnectDB)
    public NroSqlStaticRowSource(INroSqlExecutor database)
    {
        ArgumentNullException.ThrowIfNull(database);
        this.database = database;
    }

    // Đọc part theo truy vấn cố định có ORDER BY id. (Java: part)
    public Task<IReadOnlyList<PartRow>> ReadPartsAsync(CancellationToken cancellationToken = default) => ReadAsync(PartsSql, snapshot => new PartRow
    {
        PartId = snapshot.GetInt("id"),
        Type = snapshot.GetInt("type"),
        Data = TextOrEmpty(snapshot, "data"),
    }, cancellationToken);

    // Đọc nền theo truy vấn cố định có ORDER BY id. (Java: bg_item_template)
    public Task<IReadOnlyList<BgItemTemplateRow>> ReadBgItemsAsync(CancellationToken cancellationToken = default) => ReadAsync(BgItemsSql, snapshot => new BgItemTemplateRow
    {
        BgItemTemplateId = snapshot.GetInt("id"),
        ImageId = snapshot.GetInt("image_id"),
        Layer = snapshot.GetInt("layer"),
        Dx = snapshot.GetInt("dx"),
        Dy = snapshot.GetInt("dy"),
    }, cancellationToken);

    // Đọc item theo truy vấn cố định có ORDER BY id. (Java: item_template)
    public Task<IReadOnlyList<ItemTemplateRow>> ReadItemTemplatesAsync(CancellationToken cancellationToken = default) => ReadAsync(ItemTemplatesSql, snapshot => new ItemTemplateRow
    {
        ItemTemplateId = snapshot.GetInt("id"),
        Type = snapshot.GetInt("type"),
        Gender = snapshot.GetShort("gender"),
        Name = TextOrEmpty(snapshot, "name"),
        Description = TextOrEmpty(snapshot, "description"),
        Level = snapshot.GetInt("level"),
        IconId = snapshot.GetInt("icon_id"),
        Part = snapshot.GetInt("part"),
        IsUpToUp = snapshot.GetBoolean("is_up_to_up"),
        PowerRequire = snapshot.GetLong("power_require"),
        Gold = snapshot.GetInt("gold"),
        GoldSell = snapshot.GetInt("gold_sell"),
        Gem = snapshot.GetInt("gem"),
        GemSell = snapshot.GetInt("gem_sell"),
        Ruby = snapshot.GetInt("ruby"),
        RubySell = snapshot.GetInt("ruby_sell"),
        Head = snapshot.GetInt("head"),
        Body = snapshot.GetInt("body"),
        Leg = snapshot.GetInt("leg"),
        TypeEvent = snapshot.GetInt("type_event"),
        IsGender = snapshot.GetShort("is_gender"),
    }, cancellationToken);

    // Đọc option theo truy vấn cố định có ORDER BY id. (Java: item_option_template)
    public Task<IReadOnlyList<ItemOptionTemplateRow>> ReadItemOptionTemplatesAsync(CancellationToken cancellationToken = default) => ReadAsync(ItemOptionTemplatesSql, snapshot => new ItemOptionTemplateRow
    {
        ItemOptionTemplateId = snapshot.GetInt("id"),
        Name = TextOrEmpty(snapshot, "name"),
        Type = snapshot.GetShort("type"),
    }, cancellationToken);

    // Đọc avatar đầu theo truy vấn cố định có ORDER BY head_id. (Java: head_avatar)
    public Task<IReadOnlyList<HeadAvatarRow>> ReadHeadAvatarsAsync(CancellationToken cancellationToken = default) => ReadAsync(HeadAvatarsSql, snapshot => new HeadAvatarRow
    {
        HeadId = snapshot.GetInt("head_id"),
        AvatarId = snapshot.GetInt("avatar_id"),
    }, cancellationToken);

    // Đọc cờ theo truy vấn cố định có ORDER BY id. (Java: flag_bag)
    public Task<IReadOnlyList<FlagBagRow>> ReadFlagBagsAsync(CancellationToken cancellationToken = default) => ReadAsync(FlagBagsSql, snapshot => new FlagBagRow
    {
        FlagBagId = snapshot.GetInt("id"),
        IconData = TextOrEmpty(snapshot, "icon_data"),
        Name = TextOrEmpty(snapshot, "name"),
        Gold = snapshot.GetInt("gold"),
        Gem = snapshot.GetInt("gem"),
        IconId = snapshot.GetInt("icon_id"),
    }, cancellationToken);

    // Đọc frame đầu theo truy vấn cố định có ORDER BY id. (Java: array_head_2_frames)
    public Task<IReadOnlyList<HeadAnimationFrameRow>> ReadHeadAnimationFramesAsync(CancellationToken cancellationToken = default) => ReadAsync(HeadAnimationFramesSql, snapshot => new HeadAnimationFrameRow
    {
        HeadAnimationFrameId = snapshot.GetInt("id"),
        Data = TextOrEmpty(snapshot, "data"),
    }, cancellationToken);

    // Đọc ảnh theo truy vấn cố định có ORDER BY id. (Java: img_by_name)
    public Task<IReadOnlyList<ImgByNameRow>> ReadImagesByNameAsync(CancellationToken cancellationToken = default) => ReadAsync(ImagesByNameSql, snapshot => new ImgByNameRow
    {
        ImgByNameId = snapshot.GetInt("id"),
        Name = TextOrEmpty(snapshot, "name"),
        NFrame = snapshot.GetInt("n_frame"),
    }, cancellationToken);

    // Đọc mẫu thuộc tính có ORDER BY id rõ ràng. (Java: attribute_template)
    public Task<IReadOnlyList<AttributeTemplateRow>> ReadAttributeTemplatesAsync(CancellationToken cancellationToken = default) => ReadAsync(AttributeTemplatesSql, snapshot => new AttributeTemplateRow
    {
        AttributeTemplateId = snapshot.GetInt("id"),
        Name = TextOrEmpty(snapshot, "name"),
    }, cancellationToken);

    // Đọc mốc giới hạn sức mạnh có ORDER BY id rõ ràng. (Java: power_limit)
    public Task<IReadOnlyList<PowerLimitRow>> ReadPowerLimitsAsync(CancellationToken cancellationToken = default) => ReadAsync(PowerLimitsSql, snapshot => new PowerLimitRow
    {
        PowerLimitId = snapshot.GetInt("id"),
        Power = snapshot.GetLong("power"),
        Hp = snapshot.GetInt("hp"),
        Mp = snapshot.GetInt("mp"),
        Damage = snapshot.GetInt("damage"),
        Defense = snapshot.GetInt("defense"),
        Critical = snapshot.GetInt("critical"),
    }, cancellationToken);

    // Đọc danh hiệu có ORDER BY id rõ ràng. (Java: caption)
    public Task<IReadOnlyList<CaptionRow>> ReadCaptionsAsync(CancellationToken cancellationToken = default) => ReadAsync(CaptionsSql, snapshot => new CaptionRow
    {
        CaptionId = snapshot.GetInt("id"),
        Earth = TextOrEmpty(snapshot, "earth"),
        Saiya = TextOrEmpty(snapshot, "saiya"),
        Namek = TextOrEmpty(snapshot, "namek"),
        Power = snapshot.GetLong("power"),
    }, cancellationToken);

    // Đọc kỹ năng theo nclass_id, slot và id để phá tie ổn định. (Java: skill_template)
    public Task<IReadOnlyList<SkillTemplateRow>> ReadSkillTemplatesAsync(CancellationToken cancellationToken = default) => ReadAsync(SkillTemplatesSql, snapshot => new SkillTemplateRow
    {
        NclassId = snapshot.GetInt("nclass_id"),
        SkillTemplateId = snapshot.GetInt("id"),
        Name = TextOrEmpty(snapshot, "name"),
        MaxPoint = snapshot.GetShort("max_point"),
        ManaUseType = snapshot.GetShort("mana_use_type"),
        Type = snapshot.GetShort("type"),
        IconId = snapshot.GetInt("icon_id"),
        DamInfo = TextOrEmpty(snapshot, "dam_info"),
        Slot = snapshot.GetInt("slot"),
        Skills = TextOrEmpty(snapshot, "skills"),
    }, cancellationToken);

    // Đọc nội tại có ORDER BY id rõ ràng. (Java: intrinsic)
    public Task<IReadOnlyList<IntrinsicRow>> ReadIntrinsicsAsync(CancellationToken cancellationToken = default) => ReadAsync(IntrinsicsSql, snapshot => new IntrinsicRow
    {
        IntrinsicId = snapshot.GetInt("id"),
        Name = TextOrEmpty(snapshot, "name"),
        ParamFrom1 = snapshot.GetInt("param_from_1"),
        ParamTo1 = snapshot.GetInt("param_to_1"),
        ParamFrom2 = snapshot.GetInt("param_from_2"),
        ParamTo2 = snapshot.GetInt("param_to_2"),
        Icon = snapshot.GetInt("icon"),
        Gender = snapshot.GetShort("gender"),
    }, cancellationToken);

    // Đọc nhiệm vụ chính theo id; bước được đọc riêng để kiểm FK. (Java: task_main_template)
    public Task<IReadOnlyList<TaskMainTemplateRow>> ReadTaskMainsAsync(CancellationToken cancellationToken = default) => ReadAsync(TaskMainsSql, snapshot => new TaskMainTemplateRow
    {
        TaskMainTemplateId = snapshot.GetInt("id"),
        Name = TextOrEmpty(snapshot, "name"),
        Detail = TextOrEmpty(snapshot, "detail"),
    }, cancellationToken);

    // Đọc bước theo parent/idmain để grouping không phụ thuộc thứ tự máy chủ. (Java: task_sub_template)
    public Task<IReadOnlyList<TaskSubTemplateRow>> ReadTaskSubsAsync(CancellationToken cancellationToken = default) => ReadAsync(TaskSubsSql, snapshot => new TaskSubTemplateRow
    {
        TaskMainId = snapshot.GetInt("task_main_id"),
        Name = TextOrEmpty(snapshot, "name"),
        MaxCount = snapshot.GetInt("max_count"),
        Notify = TextOrEmpty(snapshot, "notify"),
        NpcId = snapshot.GetInt("npc_id"),
        Map = snapshot.GetInt("map"),
        Idmain = snapshot.GetInt("idmain"),
    }, cancellationToken);

    // Đọc nhiệm vụ phụ theo id. (Java: side_task_template)
    public Task<IReadOnlyList<SideTaskTemplateRow>> ReadSideTaskTemplatesAsync(CancellationToken cancellationToken = default) => ReadAsync(SideTaskTemplatesSql, snapshot => new SideTaskTemplateRow
    {
        SideTaskTemplateId = snapshot.GetInt("id"), Name = TextOrEmpty(snapshot, "name"),
        MaxCountLv1 = TextOrEmpty(snapshot, "max_count_lv1"), MaxCountLv2 = TextOrEmpty(snapshot, "max_count_lv2"), MaxCountLv3 = TextOrEmpty(snapshot, "max_count_lv3"), MaxCountLv4 = TextOrEmpty(snapshot, "max_count_lv4"), MaxCountLv5 = TextOrEmpty(snapshot, "max_count_lv5"),
    }, cancellationToken);

    // Đọc nhiệm vụ bang hội theo id. (Java: clan_task_template)
    public Task<IReadOnlyList<ClanTaskTemplateRow>> ReadClanTaskTemplatesAsync(CancellationToken cancellationToken = default) => ReadAsync(ClanTaskTemplatesSql, snapshot => new ClanTaskTemplateRow
    {
        ClanTaskTemplateId = snapshot.GetInt("id"), Name = TextOrEmpty(snapshot, "name"),
        MaxCountLv1 = TextOrEmpty(snapshot, "max_count_lv1"), MaxCountLv2 = TextOrEmpty(snapshot, "max_count_lv2"), MaxCountLv3 = TextOrEmpty(snapshot, "max_count_lv3"), MaxCountLv4 = TextOrEmpty(snapshot, "max_count_lv4"), MaxCountLv5 = TextOrEmpty(snapshot, "max_count_lv5"),
    }, cancellationToken);

    // Đọc nhiệm vụ KOL theo id. (Java: task_kol_template)
    public Task<IReadOnlyList<TaskKolTemplateRow>> ReadKolTaskTemplatesAsync(CancellationToken cancellationToken = default) => ReadAsync(KolTaskTemplatesSql, snapshot => new TaskKolTemplateRow
    {
        TaskKolTemplateId = snapshot.GetInt("id"), Info = TextOrEmpty(snapshot, "info"), MaxCount = TextOrEmpty(snapshot, "max_count"),
    }, cancellationToken);

    // Đọc nhiệm vụ sự kiện theo id. (Java: task_event_template)
    public Task<IReadOnlyList<TaskEventTemplateRow>> ReadEventTaskTemplatesAsync(CancellationToken cancellationToken = default) => ReadAsync(EventTaskTemplatesSql, snapshot => new TaskEventTemplateRow
    {
        TaskEventTemplateId = snapshot.GetInt("id"), Name = TextOrEmpty(snapshot, "name"),
        MaxCountLv1 = TextOrEmpty(snapshot, "max_count_lv1"), MaxCountLv2 = TextOrEmpty(snapshot, "max_count_lv2"), MaxCountLv3 = TextOrEmpty(snapshot, "max_count_lv3"), MaxCountLv4 = TextOrEmpty(snapshot, "max_count_lv4"), MaxCountLv5 = TextOrEmpty(snapshot, "max_count_lv5"),
    }, cancellationToken);

    // Đọc thành tựu theo id. (Java: achievement_template)
    public Task<IReadOnlyList<AchievementTemplateRow>> ReadAchievementTemplatesAsync(CancellationToken cancellationToken = default) => ReadAsync(AchievementTemplatesSql, snapshot => new AchievementTemplateRow
    {
        AchievementTemplateId = snapshot.GetInt("id"), Info1 = TextOrEmpty(snapshot, "info1"), Info2 = TextOrEmpty(snapshot, "info2"), Money = snapshot.GetInt("money"), MaxCount = snapshot.GetLong("max_count"),
    }, cancellationToken);

    // Đọc cửa hàng theo NPC rồi id để phá tie ổn định. (Java: ShopDAO.getShops)
    public Task<IReadOnlyList<ShopRow>> ReadShopsAsync(CancellationToken cancellationToken = default) => ReadAsync(ShopsSql, snapshot => new ShopRow
    {
        ShopId = snapshot.GetInt("id"), NpcId = snapshot.GetInt("npc_id"), TagName = TextOrEmpty(snapshot, "tag_name"), TypeShop = NullableInt(snapshot, "type_shop"),
    }, cancellationToken);

    // Đọc tab cửa hàng theo shop rồi id. (Java: ShopDAO.loadShopTab)
    public Task<IReadOnlyList<TabShopRow>> ReadShopTabsAsync(CancellationToken cancellationToken = default) => ReadAsync(ShopTabsSql, snapshot => new TabShopRow
    {
        TabShopId = snapshot.GetInt("id"), ShopId = snapshot.GetInt("shop_id"), Name = TextOrEmpty(snapshot, "name"),
    }, cancellationToken);

    // Đọc item đang bán theo tab, thời điểm tạo giảm dần rồi id. (Java: ShopDAO.loadItemShop)
    public Task<IReadOnlyList<ItemShopRow>> ReadShopItemsAsync(CancellationToken cancellationToken = default) => ReadAsync(ShopItemsSql, snapshot => new ItemShopRow
    {
        ItemShopId = snapshot.GetInt("id"), TabId = snapshot.GetInt("tab_id"), TempId = snapshot.GetInt("temp_id"), IsNew = checked((short)snapshot.GetInt("is_new")), IsSell = snapshot.GetBoolean("is_sell"), TypeSell = NullableInt(snapshot, "type_sell"), Cost = NullableInt(snapshot, "cost"), Costgold = NullableInt(snapshot, "costgold"), IconSpec = NullableInt(snapshot, "icon_spec"), CreateTime = snapshot.GetObject("create_time") as DateTime?,
    }, cancellationToken);

    // Đọc option item shop theo item rồi id. (Java: ShopDAO.loadItemShopOption)
    public Task<IReadOnlyList<ItemShopOptionRow>> ReadShopItemOptionsAsync(CancellationToken cancellationToken = default) => ReadAsync(ShopItemOptionsSql, snapshot => new ItemShopOptionRow
    {
        ItemShopOptionId = snapshot.GetInt("id"), ItemShopId = snapshot.GetInt("item_shop_id"), OptionId = snapshot.GetInt("option_id"), Param = snapshot.GetInt("param"),
    }, cancellationToken);

    // Đọc điều kiện nhiệm vụ huy hiệu theo id. (Java: TASKS_BADGES_TEMPLATE)
    public Task<IReadOnlyList<TaskBadgesTemplateRow>> ReadBadgeTaskTemplatesAsync(CancellationToken cancellationToken = default) => ReadAsync(BadgeTaskTemplatesSql, snapshot => new TaskBadgesTemplateRow
    {
        TaskBadgesTemplateId = snapshot.GetInt("id"), Name = TextOrEmpty(snapshot, "name"), MaxCount = snapshot.GetInt("max_count"), IdBadgesReward = snapshot.GetInt("id_badges_reward"),
    }, cancellationToken);

    // Đọc mẫu huy hiệu theo id. (Java: BAGES_TEMPLATES)
    public Task<IReadOnlyList<DataBadgesRow>> ReadBadgeTemplatesAsync(CancellationToken cancellationToken = default) => ReadAsync(BadgeTemplatesSql, snapshot => new DataBadgesRow
    {
        DataBadgesId = snapshot.GetInt("id"), IdEffect = snapshot.GetInt("id_effect"), IdItem = snapshot.GetInt("id_item"), Name = TextOrEmpty(snapshot, "name"), Options = TextOrEmpty(snapshot, "options"),
    }, cancellationToken);

    // Đọc thông báo theo id giảm dần. (Java: NOTIFY)
    public Task<IReadOnlyList<NotifyRow>> ReadNotificationsAsync(CancellationToken cancellationToken = default) => ReadAsync(NotificationsSql, snapshot => new NotifyRow
    {
        NotifyId = snapshot.GetInt("id"), Name = TextOrEmpty(snapshot, "name"), Text = TextOrEmpty(snapshot, "text"),
    }, cancellationToken);

    // Đọc thẻ radar theo id. (Java: RadarService.RADAR_TEMPLATE)
    public Task<IReadOnlyList<RadarRow>> ReadRadarCardsAsync(CancellationToken cancellationToken = default) => ReadAsync(RadarCardsSql, snapshot => new RadarRow
    {
        RadarId = snapshot.GetInt("id"), IconId = NullableInt(snapshot, "icon_id"), Rank = NullableShort(snapshot, "rank"), Max = NullableInt(snapshot, "max"), Type = NullableInt(snapshot, "type"), MobId = NullableInt(snapshot, "mob_id"), Body = NullableText(snapshot, "body"), Name = NullableText(snapshot, "name"), Info = NullableText(snapshot, "info"), Options = NullableText(snapshot, "options"), AuraId = NullableShort(snapshot, "aura_id"),
    }, cancellationToken);

    private async Task<IReadOnlyList<T>> ReadAsync<T>(string sql, Func<IResultSnapshot, T> map, CancellationToken cancellationToken)
    {
        using IResultSnapshot snapshot = await database.ExecuteQueryAsync(sql, cancellationToken).ConfigureAwait(false);
        var rows = new List<T>();
        while (snapshot.Next())
        {
            cancellationToken.ThrowIfCancellationRequested();
            rows.Add(map(snapshot));
        }

        return rows;
    }

    private static string TextOrEmpty(IResultSnapshot snapshot, string column) => snapshot.GetObject(column) as string ?? String.Empty;
    private static string? NullableText(IResultSnapshot snapshot, string column) => snapshot.GetObject(column) as string;
    private static int? NullableInt(IResultSnapshot snapshot, string column) => snapshot.GetObject(column) switch
    {
        null => null,
        int value => value,
        short value => value,
        long value when value is >= int.MinValue and <= int.MaxValue => (int)value,
        object value when Int32.TryParse(value.ToString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int parsed) => parsed,
        object value => throw new InvalidDataException($"Cột {column} có giá trị '{value}' không phải Int32."),
    };
    private static short? NullableShort(IResultSnapshot snapshot, string column) => NullableInt(snapshot, column) is int value ? checked((short)value) : null;
}
