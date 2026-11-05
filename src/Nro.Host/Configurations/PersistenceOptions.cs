// Nguồn Java: nro/server/Manager.java (dòng 1145–1194).
namespace Nro.Host.Configurations;

// Chứa tên khóa chuỗi kết nối, không chứa bí mật. (Java: Manager.loadProperties)
public sealed class PersistenceOptions
{
    // Tên trong ConnectionStrings, mặc định ánh xạ biến môi trường ConnectionStrings__Nro. (Java: data_base.properties)
    public string ConnectionName { get; set; } = "Nro";
}
