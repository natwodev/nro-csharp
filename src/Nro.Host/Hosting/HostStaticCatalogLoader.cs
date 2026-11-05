// Nguồn Java: nro/server/ServerManager.java (dòng 132–218).
using Nro.Content.Loaders;
using Nro.Content.StaticData;
using Nro.Core.Time;
using Nro.Core.Util;
using Nro.Game.Content;
using Nro.Game.Items.Core;
using Nro.Game.Progress.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Nro.Host.Hosting;

// Ghép các graph catalog J-01 thành một bootstrap Host công bố snapshot duy nhất. (Java: Manager.loadDatabase)
public sealed class HostStaticCatalogLoader : IStaticCatalogLoader
{
    private readonly IServiceProvider services;
    private readonly TimeProvider timeProvider;
    private IStaticCatalogs current = new StaticCatalogBuilder().Build();

    // Nhận factory DI trì hoãn row source đến lúc bootstrap thật sự cần DB. (Java: Manager.loadDatabase)
    public HostStaticCatalogLoader(IServiceProvider services, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(timeProvider);
        this.services = services;
        this.timeProvider = timeProvider;
    }

    // Trả snapshot công bố gần nhất, không thay đổi nếu bất kỳ graph nào lỗi. (Java: Manager)
    public IStaticCatalogs Current => Volatile.Read(ref current);

    // Nạp J-01B trước để J-01C/D có item catalog đã kiểm FK, đồng thời bắt J-01E fail-fast khi map thiếu id 205. (Java: Manager.loadDatabase)
    public async Task<IStaticCatalogs> LoadAsync(CancellationToken cancellationToken = default)
    {
        IStaticRowSource source = services.GetRequiredService<IStaticRowSource>();
        StaticCatalogLoader appearance = AppearanceItemCatalogSteps.CreateLoader(source);
        IStaticCatalogs appearanceCatalogs = await appearance.LoadAsync(cancellationToken).ConfigureAwait(false);

        StaticCatalogLoader world = WorldCatalogSteps.CreateLoader(source);
        IStaticCatalogs worldCatalogs = await world.LoadAsync(cancellationToken).ConfigureAwait(false);

        var gameItems = new ItemCatalog(appearanceCatalogs.Items.ItemTemplates, appearanceCatalogs.Items.ItemOptionTemplates);
        var itemService = new ItemService(
            gameItems,
            new RandomUtil(new SystemRandomProvider()),
            new VietnamClock(timeProvider));
        var kolItems = new KolTaskItemFactory(itemService);

        StaticCatalogLoader skills = SkillsProgressionTaskCatalogSteps.CreateLoader(source, kolItems);
        IStaticCatalogs skillCatalogs = await skills.LoadAsync(cancellationToken).ConfigureAwait(false);

        StaticCatalogLoader commerce = CommerceBadgeNotificationRadarCatalogSteps.CreateLoader(source, appearanceCatalogs.Items);
        IStaticCatalogs commerceCatalogs = await commerce.LoadAsync(cancellationToken).ConfigureAwait(false);

        var complete = new StaticCatalogSnapshot(
            appearanceCatalogs.Items,
            worldCatalogs.World,
            appearanceCatalogs.Appearance,
            appearanceCatalogs.Images,
            skillCatalogs.Skills,
            skillCatalogs.Tasks,
            commerceCatalogs.Shops,
            skillCatalogs.Progression,
            commerceCatalogs.Badges,
            commerceCatalogs.Notifications,
            commerceCatalogs.Radar);
        Volatile.Write(ref current, complete);
        return complete;
    }
}
