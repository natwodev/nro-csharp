using Nro.Game.Players.Ports;

namespace Nro.Game.Players.Companions;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/SaiBaMen.java (dòng 12–120).
public class SaiBaMen : Player, IPlayerCompanion
{
    // Thời lượng hiệu ứng chuẩn bị tự hủy gửi client (Java: writeShort(2000)).
    private const short SelfDestructEffectDurationMilliseconds = 2_000;

    // Thời gian chờ thực tế trước khi nổ, giữ so sánh nghiêm ngặt Java (Java: Util.canDoWithTime(..., 2500)).
    private const int SelfDestructDelayMilliseconds = 2_500;

    // Cổng đã nối để Dispose từ Player.Dispose() vẫn thoát map đúng Java.
    private IPlayerCompanionGateway? companionGateway;

    // Tạo Saibamen với chỉ số chủ đã clone và HP/KI đầy (Java: SaiBaMen).
    public SaiBaMen(Player owner, IPlayerRandom random, IPlayerCompanionGateway gateway)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(random);
        companionGateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        Owner = owner;
        IsSaibamen = true;
        Id = random.NextInt(1_700_000_000, 1_800_000_000);
        Gender = 2;
        Name = "Saibamen";
        CopyStats(owner);
        NPoint.CalPoint();
        NPoint.SetFullHpMp();
    }

    // Cập nhật Player nền trước, ưu tiên nổ đã arm để tránh vòng lặp bận Java (Java: update, setBom).
    public void Update(PlayerRuntimeContext context, IPlayerCompanionGateway gateway)
    {
        ArgumentNullException.ThrowIfNull(context);
        SetGateway(gateway);
        base.Update(context);
        if (IsDisposed)
        {
            return;
        }

        if (PlayerSkill.PrepareTuSat)
        {
            SetBomb(context.Clock, gateway);
            return;
        }

        if (IsDead())
        {
            DisposeCompanion();
            return;
        }

        if (Owner is not null && (Zone is null || !ReferenceEquals(Zone, Owner.Zone)))
        {
            JoinMap(context.Clock, gateway);
        }
    }

    // Đưa Saibamen vào map chủ và bắt đầu tự hủy (Java: joinMap).
    public void JoinMap(IPlayerClock clock, IPlayerCompanionGateway gateway)
    {
        ArgumentNullException.ThrowIfNull(clock);
        SetGateway(gateway);
        if (Owner is null || Owner.Zone is null)
        {
            return;
        }

        Location.X = Owner.Location.X;
        Location.Y = Owner.Location.Y;
        gateway.JoinMasterZone(this, Owner);
        SetBomb(clock, gateway);
    }

    // Trả đầu Saibamen cố định (Java: getHead).
    public override short GetHead(IPlayerRandom? random = null) => 642;

    // Trả thân Saibamen cố định (Java: getBody).
    public override short GetBody() => 643;

    // Trả chân Saibamen cố định (Java: getLeg).
    public override short GetLeg() => 644;

    // Hủy Saibamen, gỡ owner.Saibamen và thoát map (Java: dispose).
    public void Dispose(IPlayerCompanionGateway gateway)
    {
        SetGateway(gateway);
        DisposeCompanion();
    }

    // Được Player.Dispose() gọi đa hình để gỡ liên kết chủ và thoát map (Java: dispose).
    public void DisposeCompanion()
    {
        if (IsDisposed)
        {
            return;
        }

        companionGateway?.ExitMap(this);
        if (Owner is not null && ReferenceEquals(Owner.Saibamen, this))
        {
            Owner.Saibamen = null;
        }

        Owner = null;
        base.Dispose();
    }

    // Bắt đầu hiệu ứng, rồi sau hơn 2,5 giây handoff chết và AOE qua cổng Zone/Combat (Java: setBom).
    private void SetBomb(IPlayerClock clock, IPlayerCompanionGateway gateway)
    {
        if (!PlayerSkill.PrepareTuSat)
        {
            PlayerSkill.PrepareTuSat = true;
            PlayerSkill.LastTimePrepareTuSat = clock.NowMilliseconds;
            gateway.BroadcastSaiBaMenSelfDestruct(this, 104, SelfDestructEffectDurationMilliseconds);
            NPoint.Hp = 0;
            return;
        }

        if (unchecked(clock.NowMilliseconds - PlayerSkill.LastTimePrepareTuSat) > SelfDestructDelayMilliseconds)
        {
            PlayerSkill.PrepareTuSat = false;
            gateway.ExplodeSaiBaMen(this, NPoint.HpMax);
        }
    }

    // Sao chép chính xác nhóm chỉ số Java trước calPoint và SetFullHpMp (Java: SaiBaMen).
    private void CopyStats(Player owner)
    {
        NPoint.BaseHp = owner.NPoint.BaseHp;
        NPoint.BaseMp = owner.NPoint.BaseMp;
        NPoint.BaseDamage = owner.NPoint.BaseDamage;
        NPoint.Hp = owner.NPoint.Hp;
        NPoint.Mp = owner.NPoint.Mp;
        NPoint.Damage = owner.NPoint.Damage;
        NPoint.HpMax = owner.NPoint.HpMax;
        NPoint.MpMax = owner.NPoint.MpMax;
        NPoint.Power = owner.NPoint.Power;
        NPoint.Potential = owner.NPoint.Potential;
        NPoint.Stamina = owner.NPoint.Stamina;
        NPoint.MaxStamina = owner.NPoint.MaxStamina;
    }

    // Ghi cổng để hủy từ Player.Dispose() vẫn đi qua world boundary.
    private void SetGateway(IPlayerCompanionGateway gateway) => companionGateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
}
