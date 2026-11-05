// Nguồn Java: nro/server/ServerManager.java (dòng 607–620).
namespace Nro.Host.Configurations;

// Chứa lịch tác vụ nền thuộc Host. (Java: ServerManager.autoSave)
public sealed class SchedulingOptions
{
    // Chu kỳ tự lưu mặc định năm phút; hosted service tự lưu sẽ được nối ở gói sau. (Java: ServerManager.autoSave)
    public TimeSpan AutoSaveInterval { get; set; } = TimeSpan.FromMinutes(5);
}
