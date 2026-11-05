using Nro.Game.Players.Ports;

namespace Nro.Game.Players.Companions;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/PetFollow.java (dòng 10–114).
public class PetFollow : Player, IPlayerCompanion
{
    // Id kế tiếp dành cho thú đi theo; giảm sau mỗi lần tạo (Java: idb).
    public static int NextId { get; set; } = -310_720_020;

    // Thân của thú đi theo (Java: body).
    public short Body { get; }

    // Chân của thú đi theo (Java: leg).
    public short Leg { get; }

    // Cờ cho phép thú chạy idle quanh chủ (Java: idle).
    public bool IsIdle { get; set; }

    // Cổng đã nối để Dispose từ Player.Dispose() vẫn thoát map đúng Java.
    private IPlayerCompanionGateway? companionGateway;

    // Mốc di chuyển idle gần nhất theo mili giây epoch (Java: lastTimeMoveIdle).
    private long lastIdleMoveTime;

    // Khoảng chờ của lần di chuyển idle hiện tại (Java: timeMoveIdle).
    private int idleMoveInterval;

    // Tạo thú theo ngoại hình của vật phẩm và cấp id âm kế tiếp (Java: PetFollow).
    public PetFollow(Player owner, short head, short body, short leg, IPlayerCompanionGateway gateway)
    {
        ArgumentNullException.ThrowIfNull(owner);
        companionGateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        Owner = owner;
        IsPetFollow = true;
        Id = NextId;
        NextId--;
        Head = head;
        Body = body;
        Leg = leg;
    }

    // Trả đầu cố định của thú thay vì ngoại hình trang bị Player (Java: getHead).
    public override short GetHead(IPlayerRandom? random = null) => Head;

    // Trả thân cố định của thú (Java: getBody).
    public override short GetBody() => Body;

    // Trả chân cố định của thú (Java: getLeg).
    public override short GetLeg() => Leg;

    // Đưa thú vào map của chủ với lệch tọa độ nhỏ (Java: joinMapMaster).
    public void JoinMasterMap(IPlayerRandom random, IPlayerCompanionGateway gateway)
    {
        ArgumentNullException.ThrowIfNull(random);
        SetGateway(gateway);
        if (Owner is null || Owner.Zone is null)
        {
            return;
        }

        Location.X = Owner.Location.X + random.NextInt(-10, 10);
        Location.Y = Owner.Location.Y;
        if (IsPl())
        {
            DisposeCompanion();
            return;
        }

        gateway.JoinMasterZone(this, Owner);
    }

    // Cập nhật Player nền trước rồi hồi sinh, join map và idle theo nhịp Zone (Java: update).
    public void Update(PlayerRuntimeContext context, IPlayerCompanionGateway gateway)
    {
        ArgumentNullException.ThrowIfNull(context);
        SetGateway(gateway);
        base.Update(context);
        if (IsDisposed)
        {
            return;
        }

        if (IsDead())
        {
            gateway.Respawn(this, NPoint.HpMax, NPoint.MpMax);
        }

        if (Owner is not null && Owner.Zone is not null && (Zone is null || !ReferenceEquals(Zone, Owner.Zone)) && gateway.CanJoinPetFollowMasterZone(Owner))
        {
            JoinMasterMap(context.Random, gateway);
        }

        if (Owner is null || Owner.IsDead())
        {
            return;
        }

        MoveIdle(context.Clock, context.Random, gateway);
    }

    // Dịch thú về cạnh chủ trong bán kính mặc định 50 (Java: followMaster).
    public void FollowMaster(IPlayerRandom random, IPlayerCompanionGateway gateway) => FollowMaster(50, random, gateway);

    // Hủy thú, thoát map và bỏ liên kết owner theo vòng đời Java (Java: dispose).
    public void Dispose(IPlayerCompanionGateway gateway)
    {
        SetGateway(gateway);
        DisposeCompanion();
    }

    // Được Player.Dispose() gọi đa hình để không bỏ qua thoát map của companion (Java: dispose).
    public void DisposeCompanion()
    {
        if (IsDisposed)
        {
            return;
        }

        companionGateway?.ExitMap(this);
        Owner = null;
        base.Dispose();
    }

    // Cho thú đi một bước idle khi đã quá khoảng chờ nghiêm ngặt Java (Java: moveIdle).
    private void MoveIdle(IPlayerClock clock, IPlayerRandom random, IPlayerCompanionGateway gateway)
    {
        if (!IsIdle || Owner is null || unchecked(clock.NowMilliseconds - lastIdleMoveTime) <= idleMoveInterval)
        {
            return;
        }

        int direction = Location.X - Owner.Location.X <= 0 ? -1 : 1;
        int x = Owner.Location.X + random.NextInt(direction == -1 ? 30 : -50, direction == -1 ? 50 : 30);
        Location.X = x;
        Location.Y = Owner.Location.Y;
        gateway.Move(this, Location.X, Location.Y);
        lastIdleMoveTime = clock.NowMilliseconds;
        idleMoveInterval = random.NextInt(5_000, 8_000);
    }

    // Dịch thú về cạnh chủ theo hướng ngẫu nhiên và phát gói di chuyển (Java: followMaster(int)).
    private void FollowMaster(int distance, IPlayerRandom random, IPlayerCompanionGateway gateway)
    {
        ArgumentNullException.ThrowIfNull(random);
        SetGateway(gateway);
        if (Owner is null)
        {
            return;
        }

        int direction = random.NextInt(0, 1) == 0 ? -1 : 1;
        int offset = 12 + random.NextInt(0, distance);
        Location.X = Owner.Location.X + (direction > 0 ? -offset : offset);
        Location.Y = Owner.Location.Y;
        gateway.Move(this, Location.X, Location.Y);
    }

    // Ghi cổng để hủy từ Player.Dispose() vẫn đi qua world boundary.
    private void SetGateway(IPlayerCompanionGateway gateway) => companionGateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
}
