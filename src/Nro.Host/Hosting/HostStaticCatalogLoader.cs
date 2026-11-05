// Nguồn Java: nro/server/ServerManager.java (dòng 132–218).
using Nro.Content.Loaders;
using Nro.Content.StaticData;
using Nro.Core.Time;
using Nro.Core.Util;
using Nro.Game.Content;
using Nro.Game.Items.Core;
using Nro.Game.Progress.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Nro.Host.Hosting;

// Ghép các graph catalog J-01 thành một bootstrap Host công bố snapshot duy nhất. (Java: Manager.loadDatabase)
public sealed class HostStaticCatalogLoader : IStaticCatalogLoader
{
    private readonly IServiceProvider services;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<HostStaticCatalogLoader> logger;
    private IStaticCatalogs current = new StaticCatalogBuilder().Build();

    // Nhận factory DI trì hoãn row source đến lúc bootstrap thật sự cần DB. (Java: Manager.loadDatabase)
    public HostStaticCatalogLoader(IServiceProvider services, TimeProvider timeProvider, ILogger<HostStaticCatalogLoader> logger)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);
        this.services = services;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    // Trả snapshot công bố gần nhất, không thay đổi nếu bất kỳ graph nào lỗi. (Java: Manager)
    public IStaticCatalogs Current => Volatile.Read(ref current);

    // Nạp J-01B trước để J-01C/D có item catalog đã kiểm FK, đồng thời cảnh báo mapId thưa theo bằng chứng tra cứu Java. (Java: Manager.loadDatabase, MapService.getMapById)
    public async Task<IStaticCatalogs> LoadAsync(CancellationToken cancellationToken = default)
    {
        IStaticRowSource source = services.GetRequiredService<IStaticRowSource>();
        StaticCatalogLoader appearance = AppearanceItemCatalogSteps.CreateLoader(source);
        IStaticCatalogs appearanceCatalogs = await appearance.LoadAsync(cancellationToken).ConfigureAwait(false);

        StaticCatalogLoader world = WorldCatalogSteps.CreateLoader(source);
        IStaticCatalogs worldCatalogs = await world.LoadAsync(cancellationToken).ConfigureAwait(false);
        foreach (string warning in world.LastReport.Warnings)
        {
            logger.LogWarning("Cảnh báo static catalog: {CatalogWarning}", warning);
        }

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
