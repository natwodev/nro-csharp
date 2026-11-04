using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Util;

namespace Nro.Game.World.Maps;

// Gom các phụ thuộc ngoài của map, zone, quái và vật phẩm rơi: thời gian, ngẫu nhiên, dịch vụ chủ và thư mục dữ liệu.
// Nguồn Java: nro/map/Map.java và nro/map/Zone.java (lớp phụ thay các singleton gI() và System.currentTimeMillis() bằng phụ thuộc tiêm được; một thể hiện dùng chung cho toàn máy chủ, không giữ trạng thái trò chơi).
public sealed class MapRuntime
{
    // Tạo ngữ cảnh chạy bản đồ từ các dịch vụ và nguồn thời gian/ngẫu nhiên được tiêm.
    public MapRuntime(
        IMapWorld world,
        IMapRules rules,
        IMapPlayerGateway players,
        IMobLootHost loot,
        IItemPickupHandler pickup,
        IGameFlags flags,
        IRandomProvider random,
        TimeProvider? time = null,
        bool readInt = true,
        string dataRoot = "data")
    {
        World = world;
        Rules = rules;
        Players = players;
        Loot = loot;
        Pickup = pickup;
        Flags = flags;
        Time = time ?? TimeProvider.System;
        Clock = new VietnamClock(Time);
        Random = new RandomUtil(random);
        ReadInt = readInt;
        DataRoot = dataRoot;
    }

    // Dịch vụ phát gói và điều phối thế giới bản đồ.
    public IMapWorld World { get; }

    // Quy tắc phân loại và tra cứu bản đồ.
    public IMapRules Rules { get; }

    // Cổng truy cập trạng thái người chơi.
    public IMapPlayerGateway Players { get; }

    // Dịch vụ rơi vật phẩm của quái.
    public IMobLootHost Loot { get; }

    // Trình xử lý nhặt vật phẩm.
    public IItemPickupHandler Pickup { get; }

    // Các cờ trạng thái máy chủ.
    public IGameFlags Flags { get; }

    // Nguồn thời gian của Clock và độ trễ vòng lặp tick.
    public TimeProvider Time { get; }

    // Đồng hồ Việt Nam trên Time (Java: System.currentTimeMillis và TimeUtil.is21H).
    public VietnamClock Clock { get; }

    // Tiện ích ngẫu nhiên Java Util trên nguồn đã tiêm.
    public RandomUtil Random { get; }

    // Cờ Manager.readInt: khi bật, số sát thương bị cắt về miền 32 bit.
    public bool ReadInt { get; }

    // Thư mục chứa map/tile_map_data, map/item_bg_map_data và eff_map* (Java dùng đường dẫn tương đối data).
    public string DataRoot { get; }

    // Mili giây epoch hiện tại (Java: System.currentTimeMillis).
    public long Now => Clock.CurrentTimeMillis();

    // Đúng khi đã qua nghiêm ngặt nhiều hơn miniTimeTarget mili giây từ lastTime (Java: Util.canDoWithTime).
    public bool CanDoWithTime(long lastTime, long miniTimeTarget) => Now - lastTime > miniTimeTarget;

    // Tính CrisGH theo cờ ReadInt (Java: Util.CrisGH).
    public long CrisGH(double a) => Util.CrisGH(a, ReadInt);

    // Sinh số ngẫu nhiên gồm cả hai đầu mút (Java: Util.nextInt).
    public int NextInt(int from, int to) => Random.NextInt(from, to);

    // Kiểm tra xác suất theo biến thể số nguyên (Java: Util.isTrue).
    public bool IsTrue(int ratio, int typeRatio) => Random.IsTrueInt(ratio, typeRatio);
}
