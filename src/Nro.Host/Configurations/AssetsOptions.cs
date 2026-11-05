// Nguồn Java: nro/server/Manager.java (dòng 1145–1194).
namespace Nro.Host.Configurations;

// Chứa gốc tài nguyên sẽ được dùng bởi handler asset ở gói J-06. (Java: Manager.loadProperties)
public sealed class AssetsOptions
{
    // Gốc tuyệt đối của tài nguyên; để trống khi Host chưa bật handler asset. (Java: DataGame)
    public string Root { get; set; } = string.Empty;
}
