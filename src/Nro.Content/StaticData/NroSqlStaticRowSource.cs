using Nro.Data.Db;
using Nro.Data.Entities.StaticData;

namespace Nro.Content.StaticData;

// Adapter PostgreSQL chỉ đọc hàng static qua cơ chế INroSqlExecutor; không được gọi bởi bước nạp. Nguồn Java: nro/server/Manager.java (dòng 351–436, 560–600, 727–741).
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
}
