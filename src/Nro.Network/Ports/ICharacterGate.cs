using Nro.Network.Sessions;

namespace Nro.Network.Ports;

// Hợp đồng cho biết phiên đã có nhân vật để bộ phân phối lệnh kiểm tra.
// Nguồn Java: network/logic/Controller.java (Java bỏ im lặng gói khi player null; tầng game hiện thực để tầng mạng không phụ thuộc Player).

public interface ICharacterGate
{

    // Cho biết phiên đã gắn nhân vật để cho phép các lệnh yêu cầu player.
    bool HasCharacter(ISession session);
}
