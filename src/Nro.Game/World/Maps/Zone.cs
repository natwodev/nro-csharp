using System.Collections.Concurrent;
using Nro.Core.Consts;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Util;

namespace Nro.Game.World.Maps;

// Đại diện một khu của Map với người chơi, boss, pet, quái và vật phẩm trên mặt đất.
// Nguồn Java: nro/map/Zone.java (PARTIAL: getMaBuHold/setMaBuHold của Ma Bư 14 giờ chưa chuyển vì MaBuHold thuộc track P chưa hoàn thiện).
// D1: zone là actor một người ghi; tick riêng chạy lệnh Post trước Update. SnapshotList cho phép đọc/duyệt hay xóa trong lúc tick từ mọi luồng mà không ném InvalidOperationException; thao tác thêm/xóa trực tiếp cũng khóa check-then-add.
// Khác Java có chủ đích: không dùng synchronized/ArrayList chia sẻ; Update cô lập lỗi từng quái, vật phẩm, người chơi và pha để zone tiếp tục tick; danh sách người chơi trả view snapshot chỉ đọc thay danh sách thay đổi được.
public class Zone
{
    // Số người tiêu chuẩn trong map (Java: PLAYERS_TIEU_CHUAN_TRONG_MAP).
    public const sbyte PLAYERS_TIEU_CHUAN_TRONG_MAP = 7;

    private readonly object _writeGate = new();
    private readonly ConcurrentQueue<Action> _commands = new();
    private readonly SnapshotList<Player> _pets = new();
    private int _ticking;

    // Tạo zone với map chủ, chỉ số zone và sức chứa.
    public Zone(Map map, int zoneId, int maxPlayer)
    {
        Map = map;
        ZoneId = zoneId;
        MaxPlayer = maxPlayer;
    }

    // Id sẽ cấp cho vật phẩm tiếp theo xuất hiện trên đất (Java: countItemAppeaerd).
    public int CountItemAppeaerd { get; set; }

    // Lần cuối thông báo Xin Ba Tô.
    public long LastTimeNotifyXinBaTo { get; set; }

    // Map sở hữu zone.
    public Map Map { get; }

    // Chỉ số zone trong map.
    public int ZoneId { get; }

    // Số người tối đa của zone.
    public int MaxPlayer { get; set; }

    // Loại Rồng thần đang xuất hiện; -1 là không có.
    public int ShenronType { get; set; } = -1;

    // Dịch vụ và đồng hồ dùng chung lấy từ map chủ.
    public MapRuntime Runtime => Map.Runtime;

    // NPC không tương tác (Java: nonInteractiveNPCs).
    public SnapshotList<Player> NonInteractiveNPCs { get; } = new();

    // Mọi thực thể hình người: người chơi, boss và pet (Java: humanoids).
    public SnapshotList<Player> Humanoids { get; } = new();

    // Thực thể không phải boss: người chơi và pet (Java: notBosses).
    public SnapshotList<Player> NotBosses { get; } = new();

    // Người chơi thật, không gồm boss hay pet (Java: players).
    public SnapshotList<Player> Players { get; } = new();

    // Danh sách boss (Java: bosses).
    public SnapshotList<Player> Bosses { get; } = new();

    // Danh sách quái trong zone.
    public SnapshotList<Mob> Mobs { get; } = new();

    // Danh sách vật phẩm trên đất.
    public SnapshotList<ItemMap> Items { get; } = new();

    // Thời điểm rơi ngọc rồng đen gần nhất.
    public long LastTimeDropBlackBall { get; set; }

    // Đánh dấu chiến tranh ngọc rồng đen đã kết thúc.
    public bool FinishBlackBallWar { get; set; }

    // Đánh dấu map Ma Bư đã hoàn tất.
    public bool FinishMapMaBu { get; set; }

    // Trạng thái Bulon 1 còn sống.
    public bool Isbulon1Alive { get; set; } = true;

    // Trạng thái Bulon 2 còn sống.
    public bool Isbulon2Alive { get; set; } = true;

    // Trạng thái TUT còn sống.
    public bool IsTUTAlive { get; set; } = true;

    // Trạng thái Golden Frieza còn sống.
    public bool IsGoldenFriezaAlive { get; set; }

    // Đánh dấu zone đang thi đấu.
    public bool IsCompeting { get; set; }

    // Tên hạng nhất.
    public string? RankName1 { get; set; }

    // Tên hạng nhì.
    public string? RankName2 { get; set; }

    // Điểm/xếp hạng thứ nhất.
    public int Rank1 { get; set; }

    // Điểm/xếp hạng thứ hai.
    public int Rank2 { get; set; }

    // Danh sách bẫy trên map.
    public SnapshotList<TrapMap> TrapMaps { get; } = new();

    // Danh sách trạng thái giữ Ma Bư.
    public SnapshotList<MaBuHold> MaBuHolds { get; } = new();

    // Điểm phe Fide trong Tranh Ngọc.
    public int PointFide { get; set; }

    // Điểm phe Cadic trong Tranh Ngọc.
    public int PointCadic { get; set; }

    // Người chơi phe Fide trong Tranh Ngọc.
    public SnapshotList<Player> PlayersFide { get; } = new();

    // Người chơi phe Cadic trong Tranh Ngọc.
    public SnapshotList<Player> PlayersCadic { get; } = new();

    // Thời điểm bắt đầu Tranh Ngọc (Java: lastTimeStartTranhNgoc).
    public long LastNamekWarStartTime { get; set; }

    // Đánh dấu Tranh Ngọc của zone đã bắt đầu (Java: startZoneTranhNgoc).
    public bool IsNamekWarZoneStarted { get; set; }

    // Thời điểm rơi ngọc Tranh Ngọc gần nhất.
    public long LastTimeDropBall { get; set; }

    // NPC kiểu Player của zone (Java: Npc).
    public Player? Npc { get; set; }

    // Hình nhân thử sát thương của map 169-171 (Java: TestDame).
    public Player? DamageTestPlayer { get; set; }

    // Trạng thái Trung úy Trắng còn sống.
    public bool IsTrungUyTrangAlive { get; set; }

    // Trạng thái Bulon 13 còn sống.
    public bool Isbulon13Alive { get; set; }

    // Trạng thái Bulon 14 còn sống.
    public bool Isbulon14Alive { get; set; }

    // ------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Phần actor: hàng đợi lệnh và tick (F-01, F-03)
    // ------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Số lệnh chờ tick kế tiếp.
    public int PendingCommandCount => _commands.Count;

    // Đưa lệnh an toàn đa luồng vào tick zone kế tiếp theo thứ tự Post.
    public void Post(Action<Zone> command)
    {
        ArgumentNullException.ThrowIfNull(command);
        _commands.Enqueue(() => command(this));
    }

    // Xếp thêm người chơi vào tick kế tiếp.
    public void RequestAddPlayer(Player player) => Post(z => z.AddPlayer(player));

    // Xếp xóa người chơi vào tick kế tiếp.
    public void RequestRemovePlayer(Player player) => Post(z => z.RemovePlayer(player));

    // Chạy các lệnh đã xếp trước lúc gọi; lệnh tự Post phải chờ tick sau để không làm đói zone, lỗi được log nhưng không chặn lệnh khác; trả số lệnh đã chạy.
    public int RunPendingCommands()
    {
        int toRun = _commands.Count;
        int ran = 0;
        while (ran < toRun && _commands.TryDequeue(out Action? command))
        {
            ran++;
            try
            {
                command();
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(Zone), e, "Lỗi lệnh của zone " + ZoneId);
            }
        }

        return ran;
    }

    // Chạy một lượt zone: lệnh hàng đợi trước rồi Update; chỉ tick loop của zone được gọi và tick đồng thời sẽ ném lỗi.
    public void Tick()
    {
        if (Interlocked.CompareExchange(ref _ticking, 1, 0) != 0)
        {
            throw new InvalidOperationException("Zone " + ZoneId + " of map " + Map.MapId + " is already being ticked by another loop (a zone has exactly one writer).");
        }

        try
        {
            RunPendingCommands();
            Update();
        }
        finally
        {
            Volatile.Write(ref _ticking, 0);
        }
    }

    // Cập nhật quái, vật phẩm, NPC không tương tác, người chơi, Tranh Ngọc và hình nhân map 169-171; từng thực thể và pha tự cô lập lỗi (Java: update).
    public void Update()
    {
        RunPhase(UpdateMob, "mobs");
        RunPhase(UpdateItem, "items");
        RunPhase(UpdateNonInteractiveNPC, "non interactive npcs");
        RunPhase(UpdatePlayer, "players");
        RunPhase(UpdateNamekWarZone, "namek war");
        if (Map.MapId == 169 || Map.MapId == 170 || Map.MapId == 171)
        {
            RunPhase(UpdateDamageTest, "damage test");
        }
    }

    // Lỗi một pha, ví dụ dịch vụ Tranh Ngọc, không được ngăn các pha còn lại chạy.
    private void RunPhase(Action phase, string name)
    {
        try
        {
            phase();
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Zone), e, "Lỗi update " + name + " của zone " + ZoneId);
        }
    }

    private void UpdateMob()
    {
        for (int i = Mobs.Count - 1; i >= 0; i--)
        {
            try
            {
                if (i < Mobs.Count)
                {
                    Mobs[i].Update();
                }
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(Zone), e, "Lỗi update mobs");
            }
        }
    }

    private void UpdateNonInteractiveNPC()
    {
        if (NonInteractiveNPCs.IsEmpty)
        {
            return;
        }

        for (int i = NonInteractiveNPCs.Count - 1; i >= 0; i--)
        {
            try
            {
                if (i < NonInteractiveNPCs.Count)
                {
                    Player pl = NonInteractiveNPCs[i];
                    if (pl != null && pl.Zone != null)
                    {
                        Runtime.Players.UpdatePlayer(pl);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(Zone), e, "Lỗi update npcs");
            }
        }
    }

    private void UpdateItem()
    {
        if (Items.IsEmpty)
        {
            return;
        }

        for (int i = Items.Count - 1; i >= 0; i--)
        {
            try
            {
                if (i < Items.Count)
                {
                    ItemMap item = Items[i];
                    if (item != null && item.ItemTemplate != null)
                    {
                        item.Update();
                    }
                    else
                    {
                        Items.RemoveAt(i);
                        Logger.Warningln("Remove item " + i);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(Zone), e, "Lỗi item");
            }
        }
    }

    private void UpdatePlayer()
    {
        for (int i = NotBosses.Count - 1; i >= 0; i--)
        {
            try
            {
                if (i < NotBosses.Count)
                {
                    Player pl = NotBosses[i];
                    if (Runtime.Players.IsPl(pl))
                    {
                        Runtime.Players.UpdatePlayer(pl);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(Zone), e, "Lỗi update player");
            }
        }
    }

    private void UpdateDamageTest()
    {
        if (DamageTestPlayer != null)
        {
            try
            {
                Runtime.Players.UpdatePlayer(DamageTestPlayer);
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(Zone), e, "Lỗi update DamageTestPlayer");
            }
        }
    }

    private void UpdateNamekWarZone()
    {
        IMapWorld world = Runtime.World;
        if (!world.IsNamekWarTime() && IsNamekWarZoneStarted)
        {
            IsNamekWarZoneStarted = false;
            PlayersCadic.Clear();
            PlayersFide.Clear();
            PointCadic = 0;
            PointFide = 0;
            return;
        }

        if (!IsNamekWarZoneStarted)
        {
            return;
        }

        if (Runtime.CanDoWithTime(LastNamekWarStartTime, ConstNamekWar.TIME))
        {
            IsNamekWarZoneStarted = false;
            if (PointCadic > PointFide)
            {
                world.SendNamekWarEnd(this, ConstNamekWar.WIN, false);
                world.SendNamekWarEnd(this, ConstNamekWar.LOSE, true);
                world.GiveNamekWarPrize(PlayersCadic, ConstNamekWar.WIN, PointCadic);
                world.GiveNamekWarPrize(PlayersFide, ConstNamekWar.LOSE, PointFide);
            }
            else if (PointFide > PointCadic)
            {
                world.SendNamekWarEnd(this, ConstNamekWar.WIN, true);
                world.SendNamekWarEnd(this, ConstNamekWar.LOSE, false);
                world.GiveNamekWarPrize(PlayersFide, ConstNamekWar.WIN, PointFide);
                world.GiveNamekWarPrize(PlayersCadic, ConstNamekWar.LOSE, PointCadic);
            }
            else
            {
                world.SendNamekWarEnd(this, ConstNamekWar.DRAW, true);
                world.SendNamekWarEnd(this, ConstNamekWar.DRAW, false);
            }

            ResetNamekWar();
        }
        else if (PointCadic == 7)
        {
            IsNamekWarZoneStarted = false;
            world.SendNamekWarEnd(this, ConstNamekWar.WIN, false);
            world.SendNamekWarEnd(this, ConstNamekWar.LOSE, true);
            world.GiveNamekWarPrize(PlayersCadic, ConstNamekWar.WIN, PointCadic);
            world.GiveNamekWarPrize(PlayersFide, ConstNamekWar.LOSE, PointFide);
            ResetNamekWar();
        }
        else if (PointFide == 7)
        {
            IsNamekWarZoneStarted = false;
            world.SendNamekWarEnd(this, ConstNamekWar.WIN, true);
            world.SendNamekWarEnd(this, ConstNamekWar.LOSE, false);
            world.GiveNamekWarPrize(PlayersFide, ConstNamekWar.WIN, PointFide);
            world.GiveNamekWarPrize(PlayersCadic, ConstNamekWar.LOSE, PointCadic);
            ResetNamekWar();
        }

        // Java giữ khối này trong nhánh "chiến tranh đang chạy" đã vào ở trên nên nó cũng chạy tại tick vừa kết thúc chiến tranh.
        if (Runtime.CanDoWithTime(LastTimeDropBall, ConstNamekWar.LAST_TIME_DROP_BALL))
        {
            int id = Runtime.NextInt(353, 359);
            ItemMap? it = GetItemMapByTempId(id);
            if (it == null && !FindPlayerHaveBallTranhDoat(id))
            {
                LastTimeDropBall = Runtime.Now;
                int x = Runtime.NextInt(20, Map.MapWidth);
                int y = Map.YPhysicInTop(x, Runtime.NextInt(20, Map.MapHeight - 200));
                ItemMap itemMap = new(this, id, 1, x, y, -1) { IsContestedNamekBall = true };
                Runtime.World.DropItemMap(this, itemMap);
            }
        }
    }

    private void ResetNamekWar()
    {
        Items.Clear();
        PlayersCadic.Clear();
        PlayersFide.Clear();
        PointCadic = 0;
        PointFide = 0;
    }

    // ------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Thành viên zone
    // ------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Thêm người chơi phe Cadic nếu chưa có.
    public void AddPlayersCadic(Player player)
    {
        lock (_writeGate)
        {
            if (!PlayersCadic.Contains(player))
            {
                PlayersCadic.Add(player);
            }
        }
    }

    // Thêm người chơi phe Fide nếu chưa có.
    public void AddplayersFide(Player player)
    {
        lock (_writeGate)
        {
            if (!PlayersFide.Contains(player))
            {
                PlayersFide.Add(player);
            }
        }
    }

    // Xóa người chơi khỏi phe Cadic.
    public void RemoveplayersCadic(Player player)
    {
        lock (_writeGate)
        {
            PlayersCadic.Remove(player);
        }
    }

    // Xóa người chơi khỏi phe Fide.
    public void RemoveplayersFide(Player player)
    {
        lock (_writeGate)
        {
            PlayersFide.Remove(player);
        }
    }

    // Kiểm tra zone đã đủ người.
    public bool IsFullPlayer() => Players.Count >= MaxPlayer;

    // Trả số người chơi thật trong zone.
    public int GetNumOfPlayers() => Players.Count;

    // Trả số boss trong zone (Java: getNumOfBoss).
    public int GetNumOfBoss() => Bosses.Count;

    // Trả số boss trong zone (Java: getNumOfBosses).
    public int GetNumOfBosses() => Bosses.Count;

    // Kiểm tra boss có thể vào: false nếu đã có boss cùng id; Boss là một Player (Java: isBossCanJoin).
    public bool IsBossCanJoin(Player boss)
    {
        foreach (Player b in Bosses)
        {
            if (b.Id == boss.Id)
            {
                return false;
            }
        }

        return true;
    }

    // Đúng khi zone không có trọng tài id -1000000 (Java: IsTrongTaiDeoCoTrongKhu).
    public bool IsTrongTaiDeoCoTrongKhu()
    {
        foreach (Player b in Players)
        {
            if (b.Id == -1000000)
            {
                return false;
            }
        }

        return true;
    }

    // Thêm người chơi vào các danh sách hợp với loại của nó; bỏ qua null (Java: addPlayer).
    public void AddPlayer(Player? player)
    {
        if (player == null)
        {
            return;
        }

        IMapPlayerGateway gateway = Runtime.Players;
        lock (_writeGate)
        {
            if (!Humanoids.Contains(player))
            {
                Humanoids.Add(player);
            }

            bool isNonInteractiveNpc = gateway.IsNonInteractiveNpc(player);
            if (isNonInteractiveNpc)
            {
                NonInteractiveNPCs.Add(player);
            }

            if (!player.IsBoss && !NotBosses.Contains(player) && !player.IsPetFollow && !player.IsTangMonk && !isNonInteractiveNpc)
            {
                NotBosses.Add(player);
            }

            if (!player.IsBoss && !player.IsPetFollow && !player.IsTangMonk && !player.IsPet && !player.IsFather && !player.IsMother && !player.IsLover
                && !player.IsChildOne && !player.IsChildTwo && !player.IsChildThree && !Players.Contains(player) && !isNonInteractiveNpc)
            {
                Players.Add(player);
            }

            if (player.IsBoss)
            {
                Bosses.Add(player);
            }

            if (player.IsPet || player.IsFather || player.IsMother || player.IsPetFollow || player.IsTangMonk || player.IsLover || player.IsChildOne || player.IsChildTwo
                || player.IsChildThree)
            {
                _pets.Add(player);
            }
        }
    }

    // Xóa khỏi mọi danh sách, mỗi danh sách chỉ lần xuất hiện đầu như List.remove(Object) Java (Java: removePlayer).
    public void RemovePlayer(Player player)
    {
        lock (_writeGate)
        {
            NonInteractiveNPCs.Remove(player);
            Humanoids.Remove(player);
            NotBosses.Remove(player);
            Players.Remove(player);
            Bosses.Remove(player);
            _pets.Remove(player);
        }
    }

    // Tìm thực thể hình người theo id, hoặc null.
    public Player? GetPlayerInMap(long idPlayer)
    {
        foreach (Player pl in Humanoids)
        {
            if (pl.Id == idPlayer)
            {
                return pl;
            }
        }

        return null;
    }

    // Tìm boss luyện tập của người chơi theo id trong map ngoại tuyến, hoặc null.
    public Player? GetPlayerInMapOffline(Player player, long idPlayer)
    {
        foreach (Player pl in Bosses)
        {
            if (pl.Id == idPlayer && Runtime.Players.IsTrainingBossOf(pl, player))
            {
                return pl;
            }
        }

        return null;
    }

    // Tìm người chơi thật theo id, hoặc null.
    public Player? FindPlayerByID(long id)
    {
        foreach (Player p in Players)
        {
            if (p.Id == id)
            {
                return p;
            }
        }

        return null;
    }

    // Kiểm tra có người chơi đang giữ ngọc Namek tranh đoạt với id mẫu.
    public bool FindPlayerHaveBallTranhDoat(int id)
    {
        foreach (Player pl in Players)
        {
            if (pl != null && pl.IsHoldingContestedNamekBall && pl.ContestedNamekBallTemplateId == id)
            {
                return true;
            }
        }

        return false;
    }

    // Chọn ngẫu nhiên người không vô hình và không giữ Ma Bư, hoặc null.
    public Player? GetRandomPlayerInMap()
    {
        List<Player> visiblePlayers = [];
        foreach (Player pl in NotBosses)
        {
            if (pl == null)
            {
                continue;
            }

            if (!Runtime.Players.GetMobPlayerView(pl).IsInvisible && pl.MaBuHold == null && !pl.IsMabuHold)
            {
                visiblePlayers.Add(pl);
            }
        }

        if (visiblePlayers.Count > 0)
        {
            return visiblePlayers[Runtime.NextInt(0, visiblePlayers.Count - 1)];
        }

        return null;
    }

    // Chọn ngẫu nhiên một thực thể hình người trong zone, hoặc null khi rỗng (Java: PlayerMiNuongMap).
    public Player? PlayerMiNuongMap()
    {
        Player[] humanoids = Humanoids.ToArray();
        if (humanoids.Length > 0)
        {
            return humanoids[Runtime.NextInt(0, humanoids.Length - 1)];
        }

        return null;
    }

    // Kiểm tra zone không có trọng tài theo tên hoặc map 21-23 (Java: isKhongCoTrongTaiTrongKhu).
    public bool IsKhongCoTrongTaiTrongKhu()
    {
        bool no = true;
        foreach (Player pl in Players)
        {
            if (string.CompareOrdinal(pl.Name, "Trọng Tài") == 0)
            {
                no = false;
                break;
            }

            int mapId = pl.Zone!.Map.MapId;
            if (mapId >= 21 && mapId <= 23)
            {
                no = false;
            }
        }

        return no;
    }

    // ------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Vật phẩm trên mặt đất
    // ------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Tìm vật phẩm theo id riêng trong zone, hoặc null.
    public ItemMap? GetItemMapByItemMapId(int itemId)
    {
        foreach (ItemMap item in Items)
        {
            if (item != null && item.ItemMapId == itemId)
            {
                return item;
            }
        }

        return null;
    }

    // Tìm vật phẩm theo id mẫu, hoặc null.
    public ItemMap? GetItemMapByTempId(int tempId)
    {
        foreach (ItemMap item in Items)
        {
            if (item.ItemTemplate!.Id == tempId)
            {
                return item;
            }
        }

        return null;
    }

    // Kiểm tra người chơi có vật phẩm mẫu tempId do mình sở hữu trong zone.
    public bool FindItemMapByPlayer(Player player, int tempId)
    {
        foreach (ItemMap it in player.Zone!.Items)
        {
            if (it.PlayerId == player.Id && it.ItemTemplate!.Id == tempId)
            {
                return true;
            }
        }

        return false;
    }

    // Lấy vật phẩm người chơi được thấy; id 78 và 74 phụ thuộc nhiệm vụ (Java: getItemMapsForPlayer).
    public List<ItemMap> GetItemMapsForPlayer(Player player)
    {
        List<ItemMap> list = [];
        foreach (ItemMap item in Items)
        {
            if (item.ItemTemplate!.Id == 78 && Runtime.World.GetTaskId(player) != ConstTask.TASK_3_1)
            {
                continue;
            }

            if (item.ItemTemplate.Id == 74 && Runtime.World.GetTaskId(player) < ConstTask.TASK_3_0)
            {
                continue;
            }

            list.Add(item);
        }

        return list;
    }

    // Thêm vật phẩm mới nhất lên đầu, không thêm trùng cùng thể hiện (Java: addItem).
    public void AddItem(ItemMap? itemMap)
    {
        if (itemMap == null)
        {
            return;
        }

        lock (_writeGate)
        {
            if (!Items.Contains(itemMap))
            {
                Items.Insert(0, itemMap);
            }
        }
    }

    // Xóa vật phẩm khỏi zone.
    public void RemoveItemMap(ItemMap itemMap) => Items.Remove(itemMap);

    // Xếp thêm vật phẩm vào tick kế tiếp.
    public void RequestAddItem(ItemMap itemMap) => Post(z => z.AddItem(itemMap));

    // Xử lý quyền sở hữu và luật map khi nhặt; IItemPickupHandler tạo Item thật, xử lý túi, gói -20 và nhiệm vụ vì mô hình item/túi còn chưa hoàn thiện (Java: pickItem).
    public void PickItem(Player player, int itemMapId)
    {
        IMapWorld world = Runtime.World;
        ItemMap? itemMap = GetItemMapByItemMapId(itemMapId);
        if (itemMap != null && itemMap.ItemTemplate != null && !itemMap.IsPickedUp)
        {
            if (itemMap.ItemTemplate.Type == 22)
            {
                return;
            }

            if (itemMap.IsContestedNamekBall)
            {
                world.PickNamekWarBall(player, itemMap);
                return;
            }

            int playerId = JavaAbs(itemMap.PlayerId > 100_000_000 ? unchecked(1_000_000_000 - (int)itemMap.PlayerId) : unchecked((int)itemMap.PlayerId));
            if (playerId == player.Id || itemMap.PlayerId == player.Id || itemMap.PlayerId == -1)
            {
                int templateId = itemMap.ItemTemplate.Id;
                ItemPickupResult result = Runtime.Pickup.TryPick(player, itemMap, itemMapId);
                if (result == ItemPickupResult.Refused)
                {
                    return;
                }

                if (result == ItemPickupResult.Picked)
                {
                    bool permanentTaskItem = (Map.MapId >= 21 && Map.MapId <= 23 && itemMap.ItemTemplate != null && itemMap.ItemTemplate.Id == 74)
                        || (Map.MapId >= 42 && Map.MapId <= 44 && itemMap.ItemTemplate != null && itemMap.ItemTemplate.Id == 78);
                    if (!permanentTaskItem)
                    {
                        RemoveItemMap(itemMap);
                    }
                }
                else if (!world.IsBlackBall(templateId) && !world.IsNamecBall(templateId) && !world.IsNamecBallStone(templateId))
                {
                    world.SendNotification(player, "Hành trang không còn chỗ trống, không thể nhặt thêm");
                    return;
                }
            }
            else
            {
                world.SendNotification(player, "Không thể nhặt vật phẩm của người khác");
                return;
            }

            Runtime.Pickup.OnPicked(player, itemMap);
        }
        else
        {
            world.SendNotification(player, "Không thể thực hiện");
        }
    }

    private static int JavaAbs(int v) => v < 0 ? unchecked(-v) : v;

    // ------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Quái
    // ------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Tìm id không âm nhỏ nhất chưa có quái nào trong zone dùng (Java: getNewMobId).
    public int GetNewMobId()
    {
        Mob[] mobs = Mobs.ToArray();
        int id = 0;
        while (true)
        {
            bool exists = false;
            foreach (Mob m in mobs)
            {
                if (m.Id == id)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                return id;
            }

            id++;
        }
    }

    // Báo người chơi về các dạng Hirudegarn cấp 1 và 2 trong zone (Java: sendBigBoss).
    public void SendBigBoss(Player player)
    {
        foreach (Mob mob in Mobs)
        {
            if (!mob.IsDie() && mob.TempId == ConstMob.HIRUDEGARN)
            {
                if (mob.LvMob >= 1)
                {
                    Runtime.World.SendBigBoss2(player, 6, mob);
                }

                if (mob.LvMob >= 2)
                {
                    Runtime.World.SendBigBoss2(player, 5, mob);
                }

                break;
            }
        }
    }

    // Trả bẫy người chơi đang đứng trong đó, hoặc null (Java: isInTrap).
    public TrapMap? IsInTrap(Player player)
    {
        int x = Runtime.Players.GetX(player);
        int y = Runtime.Players.GetY(player);
        foreach (TrapMap trap in TrapMaps)
        {
            if (x >= trap.X && x <= trap.X + trap.W && y >= trap.Y && y <= trap.Y + trap.H)
            {
                return trap;
            }
        }

        return null;
    }

    // ------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Gói gửi client
    // ------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Gửi thông tin người chơi mới cho người khác có thể thấy họ (Java: load_Me_To_Another).
    public void LoadMeToAnother(Player player)
    {
        try
        {
            FixGroundOnMapZero(player);
            if (player.Zone != null)
            {
                IMapPlayerGateway gateway = Runtime.Players;
                Player[] players = Players.ToArray();
                if (Runtime.Rules.IsMapOffline(Map.MapId))
                {
                    if (gateway.IsNonInteractiveNpc(player) || IsTrainingBoss(player))
                    {
                        for (int i = players.Length - 1; i >= 0; i--)
                        {
                            Player pl = players[i];
                            if (!ReferenceEquals(player, pl) && (gateway.IsNonInteractiveNpc(player) || gateway.IsTrainingBossOf(player, pl)))
                            {
                                gateway.SendPlayerInfo(pl, player);
                            }
                        }
                    }
                }
                else if (Runtime.Rules.IsMapBangHoi(Map.MapId))
                {
                    for (int i = players.Length - 1; i >= 0; i--)
                    {
                        Player pl = players[i];
                        if (!ReferenceEquals(player, pl) && gateway.SameClan(player, pl))
                        {
                            gateway.SendPlayerInfo(pl, player);
                        }
                    }
                }
                else
                {
                    for (int i = players.Length - 1; i >= 0; i--)
                    {
                        Player pl = players[i];
                        if (!ReferenceEquals(player, pl))
                        {
                            gateway.SendPlayerInfo(pl, player);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Zone), e);
        }
    }

    // Gửi thông tin người đã có trong zone cho người chơi mới (Java: load_Another_To_Me).
    public void LoadAnotherToMe(Player player)
    {
        try
        {
            FixGroundOnMapZero(player);
            IMapPlayerGateway gateway = Runtime.Players;
            Player[] humanoids = Humanoids.ToArray();
            if (Runtime.Rules.IsMapOffline(Map.MapId))
            {
                for (int i = humanoids.Length - 1; i >= 0; i--)
                {
                    Player pl = humanoids[i];
                    if (pl != null && (gateway.IsNonInteractiveNpc(pl) || gateway.IsTrainingBossOf(pl, player) || gateway.IsDamageTestPlayer(pl)))
                    {
                        gateway.SendPlayerInfo(player, pl);
                    }
                }
            }
            else if (Runtime.Rules.IsMapBangHoi(Map.MapId))
            {
                for (int i = humanoids.Length - 1; i >= 0; i--)
                {
                    Player pl = humanoids[i];
                    if (pl != null && !ReferenceEquals(player, pl) && gateway.SameClan(player, pl))
                    {
                        gateway.SendPlayerInfo(player, pl);
                    }
                }
            }
            else
            {
                for (int i = humanoids.Length - 1; i >= 0; i--)
                {
                    Player pl = humanoids[i];
                    if (pl != null && !ReferenceEquals(player, pl))
                    {
                        gateway.SendPlayerInfo(player, pl);
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Java để catch này rỗng; tại đây log lỗi theo lint L07 mà không đổi hành vi.
            Logger.LogException(typeof(Zone), e, "load_Another_To_Me");
        }
    }

    // Trao đổi thông tin boss vừa vào với các boss khác (Java: loadBoss).
    public void LoadBoss(Player boss)
    {
        try
        {
            IMapPlayerGateway gateway = Runtime.Players;
            if (Runtime.Rules.IsMapOffline(Map.MapId))
            {
                foreach (Player pl in Bosses)
                {
                    if (!ReferenceEquals(boss, pl) && !gateway.IsPl(pl) && !pl.IsPet && !pl.IsFather && !pl.IsMother && !pl.IsPetFollow && !pl.IsTangMonk && !pl.IsBodyClone
                        && !pl.IsSaibamen && !pl.IsLover && !pl.IsChildOne && !pl.IsChildTwo && !pl.IsChildThree)
                    {
                        gateway.SendPlayerInfo(boss, pl);
                        gateway.SendPlayerInfo(pl, boss);
                    }
                }
            }
            else
            {
                foreach (Player pl in Bosses)
                {
                    if (!ReferenceEquals(boss, pl))
                    {
                        gateway.SendPlayerInfo(boss, pl);
                        gateway.SendPlayerInfo(pl, boss);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Zone), e);
        }
    }

    private bool IsTrainingBoss(Player player)
    {
        // Java: player instanceof TrainningBoss; bất cứ Player nào được gateway xác nhận là boss luyện tập của ai đó trong zone đều tính.
        foreach (Player pl in Players)
        {
            if (Runtime.Players.IsTrainingBossOf(player, pl))
            {
                return true;
            }
        }

        return false;
    }

    private void FixGroundOnMapZero(Player player)
    {
        // Java: tại map 0, ép Y xuống đất để người vừa bay vào không lơ lửng.
        if (Map.MapId == 0 && player != null)
        {
            int x = Runtime.Players.GetX(player);
            int groundY = Map.YPhysicInTop(x, 100);
            if (groundY <= 0 || groundY > Map.MapHeight)
            {
                groundY = 432;
            }

            if (Math.Abs(Runtime.Players.GetY(player) - groundY) > 5)
            {
                Runtime.World.SetPos(player, x, groundY);
            }
        }
    }

    // Gửi gói -24 lớn gồm map, cửa, quái, NPC, vật phẩm và dữ liệu nền/hiệu ứng (Java: mapInfo).
    public void MapInfo(Player pl)
    {
        try
        {
            Message msg = new(-24);
            JavaDataOutput w = msg.Writer();
            IMapPlayerGateway gateway = Runtime.Players;
            w.WriteByte(Map.MapId);
            w.WriteByte(Map.PlanetId);
            w.WriteByte(Map.TileId);
            w.WriteByte(Map.BgId);
            w.WriteByte(Map.Type);
            w.WriteUTF(Map.MapName);
            w.WriteByte(ZoneId);
            w.WriteShort(gateway.GetX(pl));
            w.WriteShort(gateway.GetY(pl));

            // Cửa chuyển map
            try
            {
                List<WayPoint> wayPoints = Map.WayPoints;
                w.WriteByte(wayPoints.Count);
                foreach (WayPoint wp in wayPoints)
                {
                    w.WriteShort(wp.MinX);
                    w.WriteShort(wp.MinY);
                    w.WriteShort(wp.MaxX);
                    w.WriteShort(wp.MaxY);
                    w.WriteBoolean(wp.IsEnter);
                    w.WriteBoolean(wp.IsOffline);
                    w.WriteUTF(wp.Name);
                }
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(Zone), e, "mapInfo waypoint");
                w.WriteByte(0);
            }

            // Quái
            try
            {
                List<Mob> mobs = [];
                foreach (Mob mob in Mobs)
                {
                    if (mob.IsBigBoss() && mob.TempId != 70 && mob.IsDie())
                    {
                        continue;
                    }

                    mobs.Add(mob);
                }

                w.WriteByte(mobs.Count);
                foreach (Mob mob in mobs)
                {
                    w.WriteBoolean(false); // Bị vô hiệu hóa
                    w.WriteBoolean(false); // Không di chuyển
                    w.WriteBoolean(false); // Bị lửa
                    w.WriteBoolean(false); // Bị băng
                    w.WriteBoolean(false); // Bị gió
                    w.WriteByte(mob.TempId);
                    w.WriteByte(0); // Hệ thống
                    msg.WriteCris(Runtime.CrisGH(mob.Point.Gethp()), Runtime.ReadInt);
                    w.WriteByte(mob.Level);
                    msg.WriteCris(Runtime.CrisGH(mob.Point.GetHpFull()), Runtime.ReadInt);
                    w.WriteShort(mob.Location.X);
                    w.WriteShort(mob.Location.Y);
                    w.WriteByte(mob.Status);
                    w.WriteByte(mob.LvMob);
                    w.WriteBoolean(mob.TempId == ConstMob.BEAR_THIEF || (mob.TempId >= ConstMob.NINE_TUSKED_ELEPHANT && mob.TempId <= ConstMob.PIANO)
                        || mob.TempId == ConstMob.KONG || mob.TempId == ConstMob.GODZILLA); // Là đại boss
                }
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(Zone), e, "mapInfo mob");
                w.WriteByte(0);
            }

            w.WriteByte(0);

            // NPC
            try
            {
                IReadOnlyList<MapNpcInfo> npcs = Runtime.World.GetNpcsForPlayer(pl);
                w.WriteByte(npcs.Count);
                foreach (MapNpcInfo npc in npcs)
                {
                    w.WriteByte(npc.Status);
                    w.WriteShort(npc.Cx);
                    w.WriteShort(npc.Cy);
                    w.WriteByte(npc.TempId);
                    w.WriteShort(npc.Avartar);
                }
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(Zone), e, "mapInfo npc");
                w.WriteByte(0);
            }

            // Vật phẩm
            try
            {
                List<ItemMap> itemsMap = GetItemMapsForPlayer(pl);
                w.WriteByte(itemsMap.Count);
                foreach (ItemMap it in itemsMap)
                {
                    w.WriteShort(it.ItemMapId);
                    w.WriteShort(it.ItemTemplate!.Id);
                    w.WriteShort(it.X);
                    w.WriteShort(it.Y);
                    w.WriteInt(unchecked((int)it.PlayerId));
                }
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(Zone), e, "mapInfo item");
                w.WriteByte(0);
            }

            // Vật phẩm nền
            byte[]? bgItem = FileIO.ReadFile(MapFile("item_bg_map_data"));
            if (bgItem != null)
            {
                w.Write(bgItem);
            }
            else
            {
                w.WriteShort(0);
            }

            // Hiệu ứng map
            IGameFlags flags = Runtime.Flags;
            string effFolder = flags.LunarNewYear ? "eff_map_event_luna_new_year" : flags.Christmas ? "eff_map_christmas" : flags.Halloween ? "eff_map_halloween" : "eff_map";
            byte[]? effItem = FileIO.ReadFile(MapFile(effFolder));
            if (effItem != null)
            {
                w.Write(effItem);
            }
            else
            {
                w.WriteShort(0);
            }

            w.WriteByte(Map.BgType);
            w.WriteByte(gateway.GetIdSpaceShip(pl));
            w.WriteByte(Map.MapId == 148 ? 1 : 0);
            gateway.SendMessage(pl, msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Zone), e);
        }
    }

    private string MapFile(string folder) => Path.Combine(Runtime.DataRoot, "map", folder, Map.MapId.ToString(System.Globalization.CultureInfo.InvariantCulture));
}
