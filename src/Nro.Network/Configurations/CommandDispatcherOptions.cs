namespace Nro.Network.Configurations;

// Tập cấu hình cho bộ phân phối lệnh mạng.
// Nguồn Java: network/logic/Controller.java (C# tách cấu hình; cảnh báo lệnh chậm mặc định 5 giây, còn ngân sách lỗi ghi log tách theo phiên để một client lỗi không che lỗi phiên khác).

public sealed class CommandDispatcherOptions
{

    // Ngưỡng cảnh báo lệnh xử lý chậm; mặc định 5 giây như Java.
    public TimeSpan SlowCommandThreshold { get; init; } = TimeSpan.FromSeconds(5);

    // Số lỗi handler tối đa ghi log cho mỗi phiên để chặn một client làm ngập log.
    public int MaxLoggedErrorsPerSession { get; init; } = 5;
}
