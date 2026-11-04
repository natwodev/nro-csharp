using Nro.Core.IO;
using Nro.Network.Sessions;

namespace Nro.Network.Protocol;

// Hợp đồng nhận tuần tự các gói của một phiên.
// Nguồn Java: network/interfaces/IMessageHandler.java (gói sau chỉ được giao khi gói trước xong; C# ghi log lỗi rồi tiếp tục giao gói kế, khác Java có thể dừng im lặng vòng giao).

public interface IMessageHandler
{

    // Xử lý tuần tự một gói; lỗi được ghi log và không chặn gói sau (Java: onMessage).
    ValueTask OnMessageAsync(ISession session, Message message, CancellationToken cancellationToken);
}
