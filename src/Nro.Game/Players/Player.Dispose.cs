using Nro.Game.Players.Ports;
using Nro.Game.Players.Companions;

namespace Nro.Game.Players;

// Hủy Player an toàn nhiều lần, giữ các component non-nullable để test/caller không gặp NullReferenceException.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 2863–3076).
public partial class Player : IDisposable
{
    // Cờ đã hủy; thay chuỗi gán null Java để Dispose lần hai không làm gì (Java: dispose).
    public bool IsDisposed { get; private set; }

    // Cổng đã ghép bởi composition root cho IDisposable.Dispose() không đối số (Java: dispose).
    public IPlayerDeathGateway? DeathGateway { get; set; }

    // Hủy Player theo IDisposable; khi còn đồ giao dịch mà chưa ghép cổng, giữ nguyên để không làm mất đồ và cho phép gọi lại với gateway (Java: dispose).
    public void Dispose()
    {
        if (IsDisposed || (ItemsTradeWVP.Count != 0 && DeathGateway is null))
        {
            return;
        }

        DisposeCore(DeathGateway);
    }

    // Hủy Player với cổng trả đồ giao dịch; gọi lần hai không làm gì và không ném (Java: dispose).
    public void Dispose(IPlayerDeathGateway gateway)
    {
        if (IsDisposed)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(gateway);
        DeathGateway = gateway;
        DisposeCore(gateway);
    }

    // Thực hiện giải phóng đúng thứ tự Java, ngoại trừ property non-nullable không bị gán null (Java: dispose).
    private void DisposeCore(IPlayerDeathGateway? gateway)
    {
        if (ItemsTradeWVP.Count != 0)
        {
            if (gateway is null)
            {
                return;
            }

            foreach (Item item in ItemsTradeWVP)
            {
                gateway.ReturnTradeItem(this, item);
            }

            ItemsTradeWVP.Clear();
        }

        DisposeDependentPlayer(Pet, gateway);
        Pet = null;
        DisposeDependentPlayer(PetFollow, gateway);
        PetFollow = null;
        DisposeDependentPlayer(TangMonk, gateway);
        TangMonk = null;
        DisposeDependentPlayer(BodyClone, gateway);
        BodyClone = null;
        DisposeDependentPlayer(Saibamen, gateway);
        Saibamen = null;
        MapBlackBall?.Clear();
        MapMaBu?.Clear();
        MapCapsule?.Clear();
        if (EggMob is not null)
        {
            EggMob.Dispose();
            EggMob = null;
        }

        DisposeIfSupported(SetClothes);
        EffectSkill.Dispose();
        DisposeIfSupported(MabuEgg);
        MabuEgg = null;
        DisposeIfSupported(WatermelonPlant);
        WatermelonPlant = null;
        DisposeIfSupported(BillEgg);
        BillEgg = null;
        PlayerTask.Dispose();
        ItemTime.Dispose();
        DisposeIfSupported(Fusion);
        DisposeIfSupported(MagicTree);
        MagicTree = null;
        PlayerIntrinsic.Dispose();
        Inventory.Dispose();
        PlayerSkill.Dispose();
        DisposeIfSupported(Combine);
        DisposeIfSupported(IDMark);
        DisposeIfSupported(Charms);
        EffectSkin.Dispose();
        DisposeIfSupported(Gift);
        NPoint.Dispose();
        EffectFlagBag.Dispose();
        ItemsWoodChest.Clear();
        if (Pvp is not null)
        {
            Pvp.Dispose();
            gateway?.DisposePvp(this);
            Pvp = null;
        }

        Achievement.Dispose();
        TemporaryEnemies.Clear();
        FatherTemporaryEnemies.Clear();
        MotherTemporaryEnemies.Clear();
        LoverTemporaryEnemies.Clear();
        ChildOneTemporaryEnemies.Clear();
        ChildTwoTemporaryEnemies.Clear();
        ChildThreeTemporaryEnemies.Clear();
        RewardBlackBall.Dispose();
        DisposeIfSupported(SuperRank);
        Friends.Clear();
        Enemies.Clear();
        Cards.Clear();
        DisposeIfSupported(DropItem);
        DisposeIfSupported(Satellite);
        DisposeIfSupported(Training);
        Zone = null;
        MapBeforeCapsule = null;
        Clan = null;
        ClanMember = null;
        Session = null;
        AutoTrainingNotification = null;
        Name = string.Empty;
        IsDisposed = true;
    }

    // Gọi Dispose khi component C# tương ứng đã port IDisposable; placeholder chưa có Dispose vẫn được giữ non-null (Java: dispose).
    private static void DisposeIfSupported(object? component)
    {
        if (component is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    // Hủy companion bằng cùng cổng nếu có; nhánh IDisposable không đối số chỉ dùng khi không có đồ giao dịch (Java: dispose).
    private static void DisposeDependentPlayer(Player? dependent, IPlayerDeathGateway? gateway)
    {
        if (dependent is null)
        {
            return;
        }

        if (dependent is IPlayerCompanion companion)
        {
            companion.DisposeCompanion();
            return;
        }

        if (gateway is not null)
        {
            dependent.Dispose(gateway);
            return;
        }

        dependent.Dispose();
    }
}
