using Nro.Game.Players.Ports;

namespace Nro.Game.Players;

// Chết, rơi vàng và phần trăm đóng góp giải Chọn Ai Đây.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 2731–2861, 3078–3091).
public partial class Player
{
    // Xử lý chết không có người tấn công; giữ overload Java setDie() (Java: setDie).
    public void SetDie(IPlayerDeathGateway gateway, IPlayerRandom random) => SetDieCore(null, gateway, random);

    // Xử lý chết có người tấn công; giữ overload Java setDie(Player) (Java: setDie).
    public void SetDie(Player? attacker, IPlayerDeathGateway gateway, IPlayerRandom random) => SetDieCore(attacker, gateway, random);

    // Xử lý chết cấp độ; Java 2735–2797 và 2799–2861 giống hệt nhau trong nguồn hiện tại nên wrapper này gọi cùng lõi (Java: setDieLV).
    public void SetDieByLevel(Player? attacker, IPlayerDeathGateway gateway, IPlayerRandom random) => SetDieCore(attacker, gateway, random);

    // Thực hiện chuỗi side effect chết chung; khác biệt SetDie/SetDieByLevel hiện không có trong Java nguồn, chỉ khác tên entry point (Java: setDie/setDieLV).
    private void SetDieCore(Player? attacker, IPlayerDeathGateway gateway, IPlayerRandom random)
    {
        ArgumentNullException.ThrowIfNull(gateway);
        ArgumentNullException.ThrowIfNull(random);
        if (IsDisposed)
        {
            return;
        }

        DropDeathGold(gateway, random);

        if (EffectSkin.HpKiMultiplier > 1)
        {
            EffectSkin.HpKiMultiplier = 1;
            gateway.SendPoint(this);
        }

        if (EffectSkin.DamageMultiplier > 1)
        {
            EffectSkin.DamageMultiplier = 1;
            gateway.SendPoint(this);
        }

        PlayerSkill.PrepareQCKK = false;
        PlayerSkill.PrepareLaze = false;
        PlayerSkill.PrepareTuSat = false;
        gateway.RemoveSkillEffectsWhenDie(this);
        NPoint.SetHp(0);
        NPoint.SetMp(0);
        if (EggMob is not null)
        {
            EggMob.SummonedMobDie();
            EggMob.Dispose();
            EggMob = null;
        }

        gateway.SendCharacterDeath(this);
        if (ShouldAddEnemy(attacker))
        {
            gateway.AddEnemy(this, attacker!);
        }

        TypePk = 0;
        if (Pvp is not null && gateway.CanResolvePvpDeath(this))
        {
            gateway.ResolvePvpDeath(this);
        }

        if (BodyClone is not null)
        {
            BodyClone.SetDie(attacker, gateway, random);
        }

        gateway.DropBlackBall(this);
        gateway.DropNamekBall(this);
        if (IsHoldingContestedNamekBall)
        {
            gateway.DropContestedNamekBall(this);
            gateway.SendContestedNamekBallLiftUpdate(this);
        }
    }

    // Trừ và rơi vàng theo sức mạnh, random và ba ngưỡng template của Java (Java: setDie/setDieLV).
    private void DropDeathGold(IPlayerDeathGateway gateway, IPlayerRandom random)
    {
        if (!IsPl() || gateway.IsWarMap(this))
        {
            return;
        }

        long goldLoss = NPoint.Power / 1_000_000;
        if (goldLoss > 32_000)
        {
            goldLoss = 32_000;
        }

        int gold = unchecked((int)goldLoss) - random.NextInt(10, 100);
        if (Inventory.Gold < gold || gold < 1)
        {
            return;
        }

        Inventory.Gold -= gold;
        gateway.SendMoney(this);
        gold = gold * 95 / 100;
        int itemTemplateId = gold < 10_000 ? 189 : gold < 20_000 ? 188 : 190;
        gateway.DropGold(this, itemTemplateId, gold);
    }

    // Kiểm tra đúng hai vế loại companion/boss của Java trước khi add enemy (Java: setDie/setDieLV).
    private bool ShouldAddEnemy(Player? attacker)
    {
        return attacker is not null && !IsPet && !IsFather && !IsMother && !IsPetFollow && !IsTangMonk && !IsBodyClone && !IsLover && !IsChildOne && !IsChildTwo
            && !IsChildThree && !IsBoss && !attacker.IsPet && !attacker.IsFather && !attacker.IsMother && !attacker.IsPetFollow && !attacker.IsTangMonk
            && !attacker.IsBoss && !attacker.IsBodyClone && !attacker.IsLover && !attacker.IsChildOne && !attacker.IsChildTwo && !attacker.IsChildThree
            && !attacker.ItemTime.IsUseIncognito;
    }

    // Tính phần trăm đóng góp Chọn Ai Đây; khác Java: Java trả NaN/Infinity khi mẫu số 0, còn C# tạm trả "0" chờ chủ dự án duyệt (Java: percentGold).
    public string PercentGold(int type, int normalGoldPool, int vipGoldPool)
    {
        int numerator = type switch
        {
            0 => GoldNormar,
            1 => GoldVIP,
            _ => 0,
        };
        int denominator = type == 0 ? normalGoldPool : type == 1 ? vipGoldPool : 0;
        if (denominator <= 0)
        {
            return "0";
        }

        return Math.Ceiling((double)numerator / denominator * 100).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
    }
}
