using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.IO;

namespace Nro.Game.World.Mobs;

// Quản lý hiệu ứng kỹ năng có thời hạn trên mob: choáng, thôi miên, mù, trói, sô-cô-la và bình Ma Phong Ba; Mob.Update gọi trong tick zone.
// Nguồn Java: nro/mob/MobEffectSkill.java (giữ nguyên biểu thức mù isBlindDCTT && canDo(...) || mob.isDie(), nên mob chết vẫn RemoveTeleportBlind và gửi gói mỗi lượt; catch rỗng Java được thay bằng ghi log theo L07).
public sealed class MobEffectSkill
{
    private readonly Mob _mob;
    private long _lastChocolateTime;
    private int _chocolateDuration;
    private long _bottleStartTime;
    private int _bottleDuration;
    private Player? _playerUseMafuba;
    private long _lastTimeMaPhongBa;
    private int _bottleType;

    // Gắn bộ hiệu ứng với mob sở hữu.
    public MobEffectSkill(Mob mob)
    {
        _mob = mob;
    }

    // Mốc bắt đầu choáng.
    public long LastTimeStun { get; set; }

    // Thời lượng choáng.
    public int TimeStun { get; set; }

    // Cho biết mob đang choáng.
    public bool IsStun { get; set; }

    // Cho biết mob đang bị thôi miên.
    public bool IsHypnotized { get; set; }

    // Mốc bắt đầu thôi miên.
    public long LastHypnosisTime { get; set; }

    // Thời lượng thôi miên.
    public int HypnosisDuration { get; set; }

    // Cho biết mob đang mù DCTT.
    public bool IsBlindDCTT { get; set; }

    // Mốc bắt đầu mù DCTT.
    public long LastTimeBlindDCTT { get; set; }

    // Thời lượng mù DCTT.
    public int TimeBlindDCTT { get; set; }

    // Cho biết mob đang bị trói.
    public bool IsBound { get; set; }

    // Mốc bắt đầu trói.
    public long LastBindTime { get; set; }

    // Thời lượng trói.
    public int BindDuration { get; set; }

    // Cho biết mob đang hóa sô-cô-la.
    public bool IsChocolate { get; set; }

    // Cho biết mob đang bị nhốt trong bình Ma Phong Ba.
    public bool IsBottled { get; set; }

    private MapRuntime Rt => _mob.Rt;

    // Gỡ hiệu ứng hết hạn và xử lý sát thương định kỳ của Ma Phong Ba.
    public void Update()
    {
        MapRuntime rt = Rt;
        bool dead = _mob.IsDie();
        if (IsStun && (rt.CanDoWithTime(LastTimeStun, TimeStun) || dead))
        {
            RemoveStun();
        }

        if (IsHypnotized && (rt.CanDoWithTime(LastHypnosisTime, HypnosisDuration) || _mob.IsDie()))
        {
            RemoveHypnosis();
        }

        if ((IsBlindDCTT && rt.CanDoWithTime(LastTimeBlindDCTT, TimeBlindDCTT)) || _mob.IsDie())
        {
            RemoveTeleportBlind();
        }

        if (IsChocolate && (rt.CanDoWithTime(_lastChocolateTime, _chocolateDuration) || _mob.IsDie()))
        {
            RemoveChocolate();
        }

        if (IsBound && (rt.CanDoWithTime(LastBindTime, BindDuration) || _mob.IsDie()))
        {
            RemoveBind();
        }

        if (IsBottled)
        {
            if (rt.CanDoWithTime(_bottleStartTime, _bottleDuration) || _mob.IsDie())
            {
                RemoveBottle();
            }

            if (rt.CanDoWithTime(_lastTimeMaPhongBa, 500) && !_mob.IsDie())
            {
                if (_playerUseMafuba != null)
                {
                    MafubaInfo? info = rt.Players.GetMafubaInfo(_playerUseMafuba);
                    if (info != null)
                    {
                        double param = info.SkillPoint;
                        long subHp = rt.CrisGH(info.HpMax * param * (info.EffectTypeBinh == 0 ? 1 : 2) / 100);
                        if (subHp >= _mob.Point.Hp)
                        {
                            subHp = unchecked((int)(_mob.Point.Hp - 1));
                        }

                        _mob.Injured(null, subHp, false);
                    }
                }

                _lastTimeMaPhongBa = rt.Now;
            }
        }
    }

    // Cho biết hiệu ứng khống chế đang chặn hành động của mob.
    public bool IsHaveEffectSkill() => IsBound || IsBlindDCTT || IsStun || IsHypnotized;

    // Bật choáng với mốc thời gian và thời lượng đã cho.
    public void StartStun(long lastTimeStartBlind, int timeBlind)
    {
        LastTimeStun = lastTimeStartBlind;
        TimeStun = timeBlind;
        IsStun = true;
    }

    private void RemoveStun()
    {
        IsStun = false;
        SendEffectOff(40);
    }

    // Đặt trạng thái thôi miên với mốc thời gian và thời lượng.
    public void SetHypnosis(long lastHypnosisTime, int hypnosisDuration)
    {
        IsHypnotized = true;
        LastHypnosisTime = lastHypnosisTime;
        HypnosisDuration = hypnosisDuration;
    }

    // Tắt thôi miên và gửi hiệu ứng tắt.
    public void RemoveHypnosis()
    {
        IsHypnotized = false;
        SendEffectOff(41);
    }

    // Bật mù DCTT với mốc thời gian và thời lượng.
    public void SetStartBlindDCTT(long lastTimeBlindDCTT, int timeBlindDCTT)
    {
        IsBlindDCTT = true;
        LastTimeBlindDCTT = lastTimeBlindDCTT;
        TimeBlindDCTT = timeBlindDCTT;
    }

    // Tắt mù DCTT và gửi hiệu ứng tắt.
    public void RemoveTeleportBlind()
    {
        IsBlindDCTT = false;
        SendEffectOff(40);
    }

    // Bật trói với mốc thời gian và thời lượng.
    public void SetBind(long lastBindTime, int bindDuration)
    {
        LastBindTime = lastBindTime;
        BindDuration = bindDuration;
        IsBound = true;
    }

    // Tắt trói và gửi hiệu ứng tắt.
    public void RemoveBind()
    {
        IsBound = false;
        SendEffectOff(32);
    }

    // Hủy trạng thái sô-cô-la và thông báo toàn zone.
    public void RemoveChocolate()
    {
        IsChocolate = false;
        try
        {
            Message msg = new(-112);
            msg.Writer().WriteByte(0);
            msg.Writer().WriteByte(_mob.Id);
            Rt.World.SendMessAllPlayerInMap(_mob.Zone!, msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(MobEffectSkill), e);
        }
    }

    // Bật trạng thái sô-cô-la với mốc thời gian và thời lượng.
    public void SetChocolate(long lastChocolateTime, int chocolateDuration)
    {
        _lastChocolateTime = lastChocolateTime;
        _chocolateDuration = chocolateDuration;
        IsChocolate = true;
    }

    // Hủy trạng thái bình Ma Phong Ba và trả thân mob bình thường.
    public void RemoveBottle()
    {
        IsBottled = false;
        Rt.World.SendBodyMob(_mob, 0, -1);
    }

    // Nhốt mob vào bình Ma Phong Ba của attacker từ bottleStartTime trong bottleDuration mili-giây.
    public void SetBottle(Player attacker, long bottleStartTime, int bottleDuration)
    {
        int bottleType = Rt.Players.GetMafubaInfo(attacker)?.NewSkillTypeItem ?? 0;
        _bottleStartTime = bottleStartTime;
        _bottleDuration = bottleDuration;
        IsBottled = true;
        _bottleType = bottleType;
        _playerUseMafuba = attacker;
        Rt.World.SendBodyMob(_mob, 1, bottleType == 0 ? 11175 : 11166);
    }

    // Loại bình đã chọn khi SetBottle. (Java: trường private typeBinh)
    public int BottleType => _bottleType;

    private void SendEffectOff(int effectId)
    {
        try
        {
            Message msg = new(-124);
            msg.Writer().WriteByte(0);
            msg.Writer().WriteByte(1);
            msg.Writer().WriteByte(effectId);
            msg.Writer().WriteByte(_mob.Id);
            Rt.World.SendMessAllPlayerInMap(_mob.Zone!, msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(MobEffectSkill), e);
        }
    }
}
