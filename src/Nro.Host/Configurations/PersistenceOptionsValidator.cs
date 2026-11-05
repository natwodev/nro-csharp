// Nguồn Java: nro/server/Manager.java (dòng 1145–1194).
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Nro.Host.Configurations;

// Kiểm khóa cấu hình database khi Host thật sự cần nạp catalog. (Java: Manager.loadProperties)
public sealed class PersistenceOptionsValidator : IValidateOptions<PersistenceOptions>
{
    private readonly IConfiguration configuration;
    private readonly IOptions<StartupOptions> startup;

    // Nhận cấu hình hợp nhất để không đưa chuỗi kết nối vào tệp nguồn. (Java: data_base.properties)
    public PersistenceOptionsValidator(IConfiguration configuration, IOptions<StartupOptions> startup)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(startup);
        this.configuration = configuration;
        this.startup = startup;
    }

    // Yêu cầu ConnectionStrings:<name> chỉ khi catalog được bật. (Java: Manager.loadDatabase)
    public ValidateOptionsResult Validate(string? name, PersistenceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.ConnectionName))
        {
            return ValidateOptionsResult.Fail("Persistence:ConnectionName không được để trống.");
        }

        if (!startup.Value.LoadStaticCatalogs)
        {
            return ValidateOptionsResult.Success;
        }

        return string.IsNullOrWhiteSpace(configuration.GetConnectionString(options.ConnectionName))
            ? ValidateOptionsResult.Fail($"Thiếu ConnectionStrings:{options.ConnectionName}. Đặt biến môi trường ConnectionStrings__{options.ConnectionName} hoặc user-secrets.")
            : ValidateOptionsResult.Success;
    }
}
