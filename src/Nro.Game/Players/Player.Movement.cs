using Nro.Core.Consts;
using Nro.Game.Players.Ports;

namespace Nro.Game.Players;

// Di chuyển, kiểm tra an toàn map và trạng thái hồi sinh của người chơi.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 2480–2729).
public partial class Player
{
    // Giữ người chơi trong biên chiến tranh Ngọc Rồng sao đen (Java: fixBlackBallWar).
    public void FixBlackBallWar(IPlayerWorldGateway world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (!world.TryGetMapState(this, out int mapId, out int mapWidth, out int mapHeight) || !world.IsBlackBallWarMap(mapId) || !IsPl())
        {
            return;
        }

        world.GetPosition(this, out int x, out int y);
        if ((x < 24 || x > mapWidth - 24 || y < 0 || y > mapHeight - 24) && !world.HasWaypointPlayerIn(this))
        {
            ResetBlackBallPosition(world, x);
            return;
        }

        if (world.GetYPhysicInTop(this, x, y) >= mapHeight - 24)
        {
            ResetBlackBallPosition(world, x);
        }
    }

    // Di chuyển một người chơi được chỉ định rồi phát vị trí mới (Java: move).
    public void Move(Player player, int toX, int toY, IPlayerWorldGateway world)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(world);

        world.GetPosition(player, out int x, out int y);
        if (toX != x)
        {
            x = toX;
        }

        if (toY != y)
        {
            y = toY;
        }

        world.SetPosition(player, x, y);
        world.SendPlayerMove(player);
    }

    // Di chuyển chính người chơi rồi phát vị trí mới (Java: move).
    public void Move(int toX, int toY, IPlayerWorldGateway world) => Move(this, toX, toY, world);

    // Phạt tài khoản có thanh toán bất thường và chuyển về nhà (Java: Check_Error_Pay).
    public void CheckErrorPay(Player player, IPlayerWorldGateway world, IPlayerRandom random)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(random);

        world.ChangeMap(player, player.Gender + 21, -1, random.NextInt(200, 600), 300);
        world.SendBigMessage(player, 1139, "|1|Hệ Thống Phát Hiện\n"
            + "|0|Tài Khoản Của Bạn Có Những Hành Vi Bất Thường Ảnh Hưởng Đến Lợi Ích Của Game\n"
            + "|0|Chúng Tôi Tiến Hành Phạt Và Cảnh Cáo Bạn!\n"
            + "|7|Nếu Vi Phạm Quá 3 Lần Bạn Sẽ Bị Khoá Tài Khoản Vĩnh Viễn!\n"
            + "|7|Số Lần Vi Phạm " + ErrorPay + " / 3\n\n"
            + "|2|Bị Phạt 2 Tỷ Vàng + 500 Hồng Ngọc!");
        world.ConfiscateCurrency(player, 2_000_000_000, 500);
        world.SendMoney(player);
    }

    // Khóa tài khoản khi bộ đếm vi phạm map đạt ngưỡng Java (Java: UpdateBandAcc).
    public void UpdateBannedAccount(IPlayerWorldGateway world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (ErrorMap == 5)
        {
            world.SendNotification(this, "Tài Khoản Của Bạn Sẽ Bị Khoá Vĩnh Viễn Sau 5 Giây Nữa!");
            world.BanAccount(this);
        }
    }

    // Kiểm tra lần lượt sáu cổng vào map và phạt đúng nhánh đầu tiên khớp (Java: checkPlayerInMap).
    public void CheckPlayerInMap(IPlayerWorldGateway world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (IsPl() && IsFounder())
        {
            return;
        }

        if (Zone is null || !world.TryGetMapState(this, out int mapId, out _, out _))
        {
            return;
        }

        if (IsPl() && world.IsNguHanhSonMap(mapId) && NPoint.Power < 40_000_000_000L)
        {
            PenalizeInvalidMap(world);
        }
        else if (IsPl() && world.IsCerealMap(mapId) && NPoint.Power < 30_000_000_000L)
        {
            PenalizeInvalidMap(world);
        }
        else if (IsPl() && world.IsPrisonPlanetMap(mapId) && NPoint.Power < 60_000_000_000L)
        {
            PenalizeInvalidMap(world);
        }
        else if (IsPl() && world.IsClanTerritoryMap(mapId) && NPoint.Power < 40_000_000_000L)
        {
            PenalizeInvalidMap(world);
        }
        else if (IsPl() && world.IsFutureMap(mapId) && world.GetTaskId(this) < ConstTask.TASK_22_0)
        {
            PenalizeInvalidMap(world);
        }
        else if (IsPl() && world.IsColdMap(mapId) && world.GetTaskId(this) < ConstTask.TASK_25_0)
        {
            PenalizeInvalidMap(world);
        }
    }

    // Phạt người chơi ra ngoài biên map, trừ map khí gas hủy diệt (Java: checkPlayerLocation).
    public void CheckPlayerLocation(IPlayerWorldGateway world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (!IsPl() || Zone is null || !world.TryGetMapState(this, out int mapId, out int mapWidth, out int mapHeight))
        {
            return;
        }

        world.GetPosition(this, out int x, out int y);
        if ((x > mapWidth || x < 0 || y > mapHeight || y < 0) && !world.IsDestronGasMap(mapId))
        {
            world.ConfiscateCurrency(this, 500_000_000, 0);
            ErrorLocation++;
            world.SendInfoHpMpMoney(this);
            world.ReturnPlayerHome(this, Gender + 21, 400, 336);
            world.SendBigMessage(this, 1139, "|1|Do Phát Hiện Có Hành Vi Bất Thường Nên\n "
                + "Chúng Tôi Đã Đưa Bạn Về Nhà Và Xử Phạt 500Tr Vàng\n"
                + "|7|Nếu Bạn Còn Tái Phạm Quá 10 Lần\n"
                + "Thì Bạn Sẽ Bị Khoá Tài Khoản Account 3 Ngày\n"
                + "Số Lần Đã Tái Phạm " + ErrorLocation + " / 10");
        }
    }

    // Đồng bộ dữ liệu hiển thị của thành viên bang qua snapshot (Java: setClanMember).
    public void SetClanMember(IPlayerWorldGateway world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (ClanMember is not null)
        {
            world.UpdateClanMemberSnapshot(this, NPoint.Power, GetHead(), GetBody(), GetLeg());
        }
    }

    // Kiểm tra phiên là điều hành viên và an toàn khi chưa có session (Java: isQuanTriVien).
    public bool IsQuanTriVien() => IsModerator();

    // Đặt cờ miễn sát thương ngắn cho đúng một vai trò gia đình (Java: setJustRevivaled).
    public void SetJustRevivaled(IPlayerClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        long now = clock.NowMilliseconds;
        if (IsPet)
        {
            JustRevived = true;
            LastTimeRevived = now;
        }
        else if (IsFather)
        {
            FatherJustRevived = true;
            LastFatherRevivedTime = now;
        }
        else if (IsMother)
        {
            MotherJustRevived = true;
            LastMotherRevivedTime = now;
        }
        else if (IsLover)
        {
            LoverJustRevived = true;
            LastLoverRevivedTime = now;
        }
        else if (IsChildOne)
        {
            ChildOneJustRevived = true;
            LastChildOneRevivedTime = now;
        }
        else if (IsChildTwo)
        {
            ChildTwoJustRevived = true;
            LastChildTwoRevivedTime = now;
        }
        else if (IsChildThree)
        {
            ChildThreeJustRevived = true;
            LastChildThreeRevivedTime = now;
        }
    }

    // Xử lý chết bởi bom rồi gọi Sai Ba Men sau khoảng chờ nghiêm ngặt của Java (Java: setBom).
    public void SetBom(Player? attacker, IPlayerCombatGateway combat, IPlayerWorldGateway world, IPlayerClock clock)
    {
        ArgumentNullException.ThrowIfNull(combat);
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(clock);

        combat.TriggerPlayerDeath(this, attacker);
        if (CanDoWithTime(clock.NowMilliseconds, LastTimeBom, 5_000))
        {
            world.CallSaiBaMen(this);
            LastTimeBom = clock.NowMilliseconds;
        }
    }

    // Đặt lại vị trí, phạt HP và báo client ở cùng thứ tự Java (Java: fixBlackBallWar).
    private void ResetBlackBallPosition(IPlayerWorldGateway world, int x)
    {
        world.ResetBlackBallPosition(this, x, world.GetYPhysicInTop(this, x, 100));
        world.ApplyBlackBallBoundaryPenalty(this);
        world.SendInfoHp(this);
    }

    // Đưa về nhà, tịch thu vàng, tăng lỗi và thông báo theo sáu nhánh Java (Java: checkPlayerInMap).
    private void PenalizeInvalidMap(IPlayerWorldGateway world)
    {
        world.ReturnPlayerHome(this, Gender + 21, 400, 336);
        world.ConfiscateCurrency(this, 2_000_000_000, 0);
        ErrorMap++;
        world.SendInfoHpMpMoney(this);
        world.SendBigMessage(this, 1139, "|1|Hệ Thống Band 4.0 Phát Hiện\n "
            + "Do Bạn Có Hành Sai Trái Trong Game, Nên Chúng Tôi\n"
            + "Đưa Bạn Về Nhà Và Phạt 2 Tỷ Vàng Để Cảnh Cáo!\n"
            + "|7|Nếu Còn Tái Phạm Quá 5 Lần Thì Bạn Sẽ Bị Khoá Tài Khoản Account Vĩnh Viễn\n"
            + "Số Lần Đã Tái Phạm " + ErrorMap + " / 5");
    }
}
