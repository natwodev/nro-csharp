using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Templates;

namespace Nro.Game.World.Maps;

// Đại diện vật phẩm nằm trên mặt đất của một zone; hàm tạo thêm vật phẩm vào zone như Java.
// Nguồn Java: nro/map/ItemMap.java (khác Java có chủ đích: thời gian/dịch vụ qua MapRuntime tiêm được, tùy chọn là ItemMapOption, Dispose để danh sách tùy chọn rỗng thay null; chỉ tick zone gọi Update theo D1).
public class ItemMap
{
    private const int TimeMoveToPlayer = 10000;

    private long _lastTimeMoveToPlayer;

    // Tạo vật phẩm rơi từ id mẫu và danh sách tùy chọn.
    public ItemMap(Zone zone, int tempId, int quantity, List<ItemMapOption> itemOptions, int x, int y, long playerId)
    {
        Zone = zone;
        ItemMapId = NextId(zone);
        ItemTemplate = zone.Runtime.World.GetItemTemplate(unchecked((short)tempId));
        Init(quantity, x, y, playerId, itemOptions);
    }

    // Tạo vật phẩm rơi từ id mẫu không có tùy chọn.
    public ItemMap(Zone zone, int tempId, int quantity, int x, int y, long playerId)
    {
        Zone = zone;
        ItemMapId = NextId(zone);
        ItemTemplate = zone.Runtime.World.GetItemTemplate(unchecked((short)tempId));
        Init(quantity, x, y, playerId, []);
    }

    // Tạo vật phẩm rơi từ mẫu vật phẩm đã tra cứu.
    public ItemMap(Zone zone, ItemTemplate temp, int quantity, int x, int y, long playerId)
    {
        Zone = zone;
        ItemMapId = NextId(zone);
        ItemTemplate = temp;
        Init(quantity, x, y, playerId, []);
    }

    // Sao chép vật phẩm, đặt thời gian tạo mới và thêm lại vào zone; danh sách tùy chọn vẫn dùng chung như Java.
    public ItemMap(ItemMap itemMap)
    {
        Zone = itemMap.Zone;
        ItemMapId = itemMap.ItemMapId;
        ItemTemplate = itemMap.ItemTemplate;
        Quantity = itemMap.Quantity;
        X = itemMap.X;
        Y = itemMap.Y;
        PlayerId = itemMap.PlayerId;
        Options = itemMap.Options;
        IsBlackBall = itemMap.IsBlackBall;
        IsNamecBall = itemMap.IsNamecBall;
        _lastTimeMoveToPlayer = itemMap._lastTimeMoveToPlayer;
        CreateTime = Zone!.Runtime.Now;
        Zone.AddItem(this);
    }

    // Zone sở hữu, hoặc null sau Dispose.
    public Zone? Zone { get; private set; }

    // Id riêng của vật phẩm trong zone.
    public int ItemMapId { get; set; }

    // Mẫu vật phẩm, hoặc null sau Dispose.
    public ItemTemplate? ItemTemplate { get; private set; }

    // Số lượng vật phẩm.
    public int Quantity { get; set; }

    // Tọa độ X trên map.
    public int X { get; set; }

    // Tọa độ Y trên map.
    public int Y { get; set; }

    // Chủ nhân chiến lợi phẩm, lấy trị tuyệt đối id; -1 cho mọi người.
    public long PlayerId { get; set; }

    // Danh sách tùy chọn vật phẩm.
    public List<ItemMapOption> Options { get; set; } = [];

    // Thời điểm tạo theo mili giây epoch.
    public long CreateTime { get; set; }

    // Cho biết vật phẩm là ngọc rồng đen.
    public bool IsBlackBall { get; set; }

    // Cho biết vật phẩm là ngọc Namek.
    public bool IsNamecBall { get; set; }

    // Id bang được quyền dùng vật phẩm vệ tinh; -1 là không giới hạn bang.
    public int ClanId { get; set; } = -1;

    // Cho biết đây là ngọc doanh trại (Java: isDoanhTraiBall).
    public bool IsBarracksBall { get; set; }

    // Cho biết đây là ngọc Namek tranh đoạt (Java: isNamecBallTranhDoat).
    public bool IsContestedNamekBall { get; set; }

    // Loại người đang giữ ngọc tranh đoạt; -1 là không có.
    public sbyte TypeHaveBallTranhDoat { get; set; } = -1;

    // Đánh dấu vật phẩm đã được nhặt.
    public bool IsPickedUp { get; set; }

    // Kiểm tra mẫu vật phẩm còn tồn tại.
    public bool IsNotNullItem() => ItemTemplate != null;

    private static int NextId(Zone zone)
    {
        int id = zone.CountItemAppeaerd++;
        if (zone.CountItemAppeaerd >= 2000000000)
        {
            zone.CountItemAppeaerd = 0;
        }

        return id;
    }

    private void Init(int quantity, int x, int y, long playerId, List<ItemMapOption> options)
    {
        MapRuntime rt = Zone!.Runtime;
        if (ItemTemplate == null)
        {
            // Java sẽ ném NullPointerException ở dòng kế tiếp khi truy cập itemTemplate.id.
            throw new InvalidOperationException("Unknown item template for an item on the ground.");
        }

        Quantity = quantity;
        X = x;
        Y = y;
        PlayerId = playerId != -1 ? (playerId < 0 ? unchecked(-playerId) : playerId) : playerId;
        CreateTime = rt.Now;
        Options = options;
        IsBlackBall = rt.World.IsBlackBall(ItemTemplate.Id);
        IsNamecBall = rt.World.IsNamecBall(ItemTemplate.Id);
        _lastTimeMoveToPlayer = rt.Now;
        Zone.AddItem(this);
    }

    // Cập nhật: ngọc đen bám người chơi đầu tiên, vệ tinh tăng hiệu ứng cho chủ gần đó, vật phẩm hết bảo hộ sau 45 giây và bị xóa sau 50 hoặc 180 giây (Java: update).
    public void Update()
    {
        if (!IsNotNullItem() || Zone == null)
        {
            return;
        }

        MapRuntime rt = Zone.Runtime;
        if (IsBlackBall)
        {
            if (rt.CanDoWithTime(_lastTimeMoveToPlayer, TimeMoveToPlayer))
            {
                if (Zone != null && !Zone.Players.IsEmpty)
                {
                    Player player = Zone.Players[0];
                    if (player.Zone != null && ReferenceEquals(player.Zone, Zone))
                    {
                        X = rt.Players.GetX(player);
                        Y = Zone.Map.YPhysicInTop(X, rt.Players.GetY(player) - 24);
                        ReAppearItem();
                        _lastTimeMoveToPlayer = rt.Now;
                    }
                }
            }

            return;
        }

        // Vệ tinh
        if (ItemTemplate!.Type == 22)
        {
            SatelliteUpdate();
        }

        if (rt.CanDoWithTime(CreateTime, 45000))
        {
            if (ItemTemplate!.Type != 22 && ItemTemplate.Id != 726 && ItemTemplate.Id != 992)
            {
                PlayerId = -1;
            }
        }

        if (((rt.CanDoWithTime(CreateTime, 50000) && IsNotNullItem() && ItemTemplate!.Type != 22) || rt.CanDoWithTime(CreateTime, 180000)) && !IsNamecBall)
        {
            if (Zone != null && Zone.Map.MapId != 21 && Zone.Map.MapId != 22 && Zone.Map.MapId != 23 && ItemTemplate!.Id != 78 && ItemTemplate.Id != 726
                && !(rt.Rules.IsBarracksMap(Zone.Map.MapId) && ItemTemplate.Id >= 14 && ItemTemplate.Id <= 20))
            {
                rt.World.RemoveItemMapAndSendClient(this);
            }
        }

        // Vật phẩm DHVT 726
        if (Zone != null && Zone.Map.MapId == 52 && IsNotNullItem() && ItemTemplate!.Id == 726)
        {
            if (!FindPlayerById(PlayerId))
            {
                rt.World.RemoveItemMapAndSendClient(this);
            }
        }

        if (Zone != null && IsNotNullItem() && ItemTemplate!.Id == 460 && PlayerId == 123456789 && rt.CanDoWithTime(CreateTime, 5000))
        {
            rt.World.RemoveItemMapAndSendClient(this);
        }
    }

    // Cộng param vào tùy chọn id, hoặc thêm tùy chọn mới khi chưa có.
    public void AddOptionParam(int id, int param)
    {
        for (int i = 0; i < Options.Count; i++)
        {
            ItemMapOption itemOption = Options[i];
            if (itemOption != null && itemOption.Id == id)
            {
                itemOption.Param += param;
                return;
            }
        }

        Options.Add(new ItemMapOption(id, param));
    }

    // Lấy param của tùy chọn id, hoặc 0 khi không có.
    public int GetOptionParam(int id)
    {
        for (int i = 0; i < Options.Count; i++)
        {
            ItemMapOption itemOption = Options[i];
            if (itemOption != null && itemOption.Id == id)
            {
                return itemOption.Param;
            }
        }

        return 0;
    }

    // Đặt param tùy chọn id nếu có, dù tên Java là getOptionParam (Java: getOptionParam(int, int)).
    public void SetOptionParam(int id, int param)
    {
        for (int i = 0; i < Options.Count; i++)
        {
            ItemMapOption itemOption = Options[i];
            if (itemOption != null && itemOption.Id == id)
            {
                itemOption.Param = param;
                return;
            }
        }
    }

    // Trừ param khỏi tùy chọn id nếu có.
    public void SubOptionParam(int id, int param)
    {
        for (int i = 0; i < Options.Count; i++)
        {
            ItemMapOption itemOption = Options[i];
            if (itemOption != null && itemOption.Id == id)
            {
                itemOption.Param -= param;
                return;
            }
        }
    }

    private bool FindPlayerById(long id)
    {
        foreach (Player pl in Zone!.Players)
        {
            if (pl.Id == id)
            {
                return true;
            }
        }

        return false;
    }

    private void SatelliteUpdate()
    {
        MapRuntime rt = Zone!.Runtime;
        IMapPlayerGateway gateway = rt.Players;
        foreach (Player pl in Zone.Players)
        {
            if (!gateway.IsDie(pl) && Util.GetDistance(gateway.GetX(pl), gateway.GetY(pl), X, Y) < 200 && gateway.HasSatellite(pl)
                && (pl.Id == PlayerId || (ClanId != -1 && gateway.GetClanId(pl) == ClanId)))
            {
                int id = ItemTemplate!.Id;
                if (id >= 342 && id <= 345)
                {
                    gateway.ApplySatelliteEffect(pl, id, rt.Now);
                }
            }
        }
    }

    private void ReAppearItem()
    {
        Zone!.Runtime.World.SendItemMapDisappear(this);
        Zone.Runtime.World.DropItemMap(Zone, this);
    }

    // Tách khỏi zone và mẫu; danh sách tùy chọn được làm rỗng thay vì null (Java: dispose).
    public void Dispose()
    {
        Zone = null;
        ItemTemplate = null;
        Options = [];
    }
}
