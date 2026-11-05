using Nro.Core.IO;
using Nro.Game.Players.Ports;
using Nro.Game.Players.Sessions;

namespace Nro.Game.Players;

// Đánh dấu người chơi là NPC không tương tác để giữ điều kiện instanceof (Java: NonInteractiveNPC) mà không phụ thuộc Nro.Content.
// Nguồn Java: nro/player/Player.java (dòng 816–829).
public interface INonInteractiveNpcPlayer
{
}

// Định danh, phân loại và phiên của người chơi.
// Nguồn Java: nro/player/Player.java (dòng 740–845).
public partial class Player
{
    // Cổng đọc cờ phiên và gửi gói tin; để null khi session subsystem chưa được ghép (Java: session).
    public IPlayerSessionGateway? SessionGateway { get; set; }

    // Chủ sở hữu của companion; thay cho trường master/player nằm trên các lớp con Java chưa được port hoàn chỉnh (Java: Detu.master, PetFollow.master, DuongTang.master, SaiBaMen.player, PhanThan.master).
    public Player? Owner { get; set; }

    // Tìm vị trí Whis của người chơi theo id và trả chỉ số bắt đầu từ một (Java: getPlayerWhis).
    internal static int GetPlayerWhisIndex(IReadOnlyList<Player> players, Player player)
    {
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(player);

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].Id == player.Id)
            {
                return i + 1;
            }
        }

        return -1;
    }

    // Lấy điểm hợp thể của người chơi (Java: getPointfusion).
    public PointFusion GetPointFusion() => Pointfusion;

    // Lấy số lần nhập sai mật khẩu (Java: getWrongPasswordAttempts).
    public int GetWrongPasswordAttempts() => WrongPasswordAttempts;

    // Tăng số lần nhập sai mật khẩu (Java: increaseWrongPasswordAttempts).
    public void IncreaseWrongPasswordAttempts() => WrongPasswordAttempts++;

    // Kiểm tra người chơi đã chết; NPoint thiếu được xem là đã chết như Java (Java: isDie).
    public bool IsDead() => NPoint is null || NPoint.Hp <= 0;

    // Gắn phiên đăng nhập cho người chơi (Java: setSession).
    public void SetSession(MySession? session) => Session = session;

    // Gửi gói tin khi người chơi có phiên và cổng phiên đã được ghép (Java: sendMessage).
    public void SendMessage(Message message)
    {
        if (Session is not null && SessionGateway is not null)
        {
            SessionGateway.Send(Session, message);
        }
    }

    // Lấy phiên đăng nhập hiện tại (Java: getSession).
    public MySession? GetSession() => Session;

    // Kiểm tra đây là đối tượng phụ thuộc một người chơi khác (Java: isMaster).
    public bool IsMaster() => IsPet || IsFather || IsMother || IsPetFollow || IsTangMonk || IsSaibamen || IsBodyClone || IsLover;

    // Lấy chủ sở hữu của companion; loại không có chủ trả chính đối tượng hiện tại (Java: getMaster).
    public Player? GetMaster()
    {
        return IsPet || IsPetFollow || IsTangMonk || IsSaibamen || IsBodyClone ? Owner : this;
    }

    // Sinh id dành riêng cho pet và các thành viên gia đình (Java: setIdForPet).
    public static long SetIdForPet(Player player, long id)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.IsLover)
        {
            return -3_000_000_000L + id;
        }

        if (player.IsPet)
        {
            return -id;
        }

        if (player.IsFather)
        {
            return -1_000_000_000L + id;
        }

        return player.IsMother ? -2_000_000_000L + id : id;
    }

    // Kiểm tra đây là người chơi thật, không phải companion, bot, gia đình hoặc NPC không tương tác (Java: isPl).
    public bool IsPl()
    {
        return IsPlayer && !IsPet && !IsFather && !IsMother && !IsBoss && !IsPetFollow && !IsTangMonk && !IsBodyClone && !IsBot_Valentine && !IsBot
            && !IsBot_Event && !IsBot_New && !IsSaibamen && !IsLover && !IsChildOne && !IsChildTwo && !IsChildThree && this is not INonInteractiveNpcPlayer;
    }

    // Kiểm tra đây là người chơi thật hoặc bot hợp lệ, không phải companion hay NPC không tương tác (Java: isPlandBot).
    public bool IsPlAndBot()
    {
        return (IsPlayer || IsBot || IsBot_New || IsBot_Event || IsBot_Valentine) && !IsPet && !IsFather && !IsMother && !IsBoss && !IsPetFollow && !IsTangMonk
            && !IsBodyClone && !IsSaibamen && !IsChildOne && !IsChildTwo && !IsChildThree && !IsLover && this is not INonInteractiveNpcPlayer;
    }

    // Kiểm tra đây là người chơi hoặc thành viên gia đình hợp lệ (Java: isPlMan).
    public bool IsPlMan()
    {
        return (IsPlayer || IsPet || IsFather || IsMother || IsLover || IsChildOne || IsChildTwo || IsChildThree || IsBodyClone) && !IsBoss && !IsPetFollow
            && !IsTangMonk && !IsBot && !IsBot_Event && !IsBot_New && !IsBot_Valentine && !IsSaibamen && this is not INonInteractiveNpcPlayer;
    }

    // Kiểm tra bất kỳ biến thể bot nào (Java: getBot).
    public bool GetBot() => IsBot || IsBot_New || IsBot_Event || IsBot_Valentine;

    // Kiểm tra người chơi thật có phiên đã kích hoạt (Java: isActive).
    public bool IsActive() => IsPl() && Session is not null && SessionGateway?.IsActive(Session) == true;

    // Kiểm tra phiên người sáng lập; không có phiên hoặc cổng thì trả false (Java: isFounder).
    public bool IsFounder() => Session is not null && SessionGateway?.IsFounder(Session) == true;

    // Kiểm tra phiên ở nhà giam; không có phiên hoặc cổng thì trả false (Java: isJail).
    public bool IsJail() => Session is not null && SessionGateway?.IsJail(Session) == true;

    // Kiểm tra phiên điều hành viên; không có phiên hoặc cổng thì trả false (Java: isQuanTriVien).
    public bool IsModerator() => Session is not null && SessionGateway?.IsModerator(Session) == true;
}
