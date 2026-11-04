using Nro.Core.IO;
using Nro.Network.Sessions;

namespace Nro.Network.Protocol;

// Hợp đồng xử lý một mã lệnh của giao thức.
// Nguồn Java: network/logic/Controller.java (payload đã đứng tại đầu dữ liệu; handler đọc đúng cấu trúc lệnh từ Message.Reader).

public interface ICommandHandler
{

    // Xử lý một lệnh với payload ở đầu dữ liệu và mã hủy (Java: Controller.onMessage).
    ValueTask HandleAsync(ISession session, Message message, CancellationToken cancellationToken);
}
