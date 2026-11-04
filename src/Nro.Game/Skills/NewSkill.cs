
namespace Nro.Game.Skills;

// Lưu trạng thái thi triển các chiêu đặc biệt tích lực: Super Kamehameha, Liên Hoàn Chưởng và Ma Phong Ba.
// Nguồn Java: nro/skill/NewSkill.java (Timer 250 ms được thay bằng Tick của zone, giữ nhịp fixed-delay; dịch vụ qua ISkillSpecialHost; giữ quirk gán isStartSkillSpecial=true ở mọi bước).
public class NewSkill
{
    public const int TIME_GONG = 2000;
    public const int TIME_END_24_25 = 3000;
    public const int TIME_END_26 = 11000;

    // Chu kỳ bước 250 ms mà Java khởi động trong setSkillSpecial.
    public const int StepPeriodMs = 250;

    private Player? player;

    // Chủ sở hữu, null sau Dispose (Java: player).
    public Player? Player => player;

    public Skill? SkillSelect { get; set; }

    // Hướng quay -1 hoặc 1 của người chơi (Java: dir).
    public sbyte Dir { get; set; }

    // Tọa độ X của người chơi lúc bắt đầu (Java: _xPlayer).
    public short XPlayer { get; set; }

    // Tọa độ Y của người chơi lúc bắt đầu (Java: _yPlayer).
    public short YPlayer { get; set; }

    // Tầm ngang của mục tiêu, luôn lưu trị tuyệt đối (Java: _xObjTaget, lỗi chính tả gốc).
    public short XObjTaget { get; set; }

    // Tung độ tuyệt đối của mục tiêu (Java: _yObjTaget, lỗi chính tả gốc).
    public short YObjTaget { get; set; }

    // Danh sách người chơi trúng mục tiêu (Java: playersTaget, lỗi chính tả gốc).
    public List<Player> PlayersTaget { get; }

    // Danh sách quái trúng mục tiêu (Java: mobsTaget, lỗi chính tả gốc).
    public List<Mob> MobsTaget { get; }

    public bool IsStartSkillSpecial { get; set; }

    public sbyte StepSkillSpecial { get; set; }

    public long LastTimeSkillSpecial { get; set; }

    public sbyte TypePaint { get; set; }

    public sbyte TypeItem { get; set; }

    // True khi bộ định thời bước đang chạy (Java: isActive).
    public bool IsActive { get; private set; }

    // Khoảng mili giây giữa hai bước khi đang hoạt động.
    public int PeriodMs { get; private set; }

    // Thời điểm epoch mili giây đến hạn chạy bước kế tiếp.
    public long NextRunAtMs { get; private set; }

    public NewSkill(Player player)
    {
        this.player = player;
        PlayersTaget = [];
        MobsTaget = [];
    }

    // Bắt đầu niệm chiêu: chọn kỹ năng, tăng CurrLevel dưới 1000 cho người chơi thật, tính tầm theo Java rồi đặt lại trạng thái và hẹn bước 250 ms; giữ ép short nên abs(-32768) vẫn -32768 (Java: setSkillSpecial).
    public void SetSkillSpecial(ISkillSpecialHost host, long nowMs, sbyte dir, short xPlayer, short yPlayer, short xObjTaget, short yObjTaget)
    {
        ArgumentNullException.ThrowIfNull(host);
        Player owner = player ?? throw new InvalidOperationException("This NewSkill was disposed");
        TypeItem = host.IsUsingNcd(owner) ? (sbyte)2 : (sbyte)0;
        Skill selected = owner.PlayerSkill.SkillSelect ?? throw new InvalidOperationException("The player has no selected skill");
        SkillSelect = selected;
        if (host.IsPl(owner) && selected.CurrLevel < 1000)
        {
            selected.CurrLevel++;
            host.SendCurrLevelSpecial(owner, selected);
        }

        Dir = dir;
        XPlayer = xPlayer;
        YPlayer = yPlayer;

        int length = xObjTaget - xPlayer;
        int dx = dir * (selected.Point * 100 + 100);
        if (selected.Template is null)
        {
            throw new InvalidOperationException("The selected skill has no template");
        }

        if (selected.Template.Id != Skill.MA_PHONG_BA)
        {
            if (Math.Abs(dx) < Math.Abs(length) || Math.Abs(length) < 100)
            {
                XObjTaget = unchecked((short)dx);
            }
            else
            {
                XObjTaget = unchecked((short)length);
            }
        }
        else
        {
            XObjTaget = 75;
        }

        // Java mở rộng short sang int trước khi abs; ép lại short khiến abs(-32768) thành -32768.
        XObjTaget = unchecked((short)Math.Abs((int)XObjTaget));
        YObjTaget = unchecked((short)Math.Abs((int)yObjTaget));
        IsStartSkillSpecial = true;
        StepSkillSpecial = 0;
        LastTimeSkillSpecial = nowMs;
        Start(StepPeriodMs, nowMs);
    }

    // Kết thúc niệm chiêu, xóa mục tiêu và dừng bộ định thời bước (Java: closeSkillSpecial).
    public void CloseSkillSpecial()
    {
        IsStartSkillSpecial = false;
        StepSkillSpecial = 0;
        PlayersTaget.Clear();
        MobsTaget.Clear();
        Close();
    }

    // Khởi động bộ định thời bước nếu chưa chạy; đang chạy thì không đổi gì (Java: start).
    public void Start(int periodMs, long nowMs)
    {
        if (!IsActive)
        {
            IsActive = true;
            PeriodMs = periodMs;
            NextRunAtMs = nowMs + periodMs;
        }
    }

    // Được tick zone gọi thay Timer Java: chỉ chạy khi đến hạn, dừng nếu chủ đã Dispose, còn lại hẹn từ nowMs thêm một chu kỳ và cập nhật kỹ năng; trả true khi đã chạy bước.
    public bool Tick(ISkillSpecialHost host, long nowMs)
    {
        ArgumentNullException.ThrowIfNull(host);
        if (!IsActive || nowMs < NextRunAtMs)
        {
            return false;
        }

        if (player is null)
        {
            Close();
            return false;
        }

        NextRunAtMs = nowMs + PeriodMs;
        Update(host, player);
        return true;
    }

    // Bỏ chủ và kỹ năng chọn; Tick kế tiếp mới dừng timer như Java (Java: dispose).
    public void Dispose()
    {
        player = null;
        SkillSelect = null;
    }

    private void Update(ISkillSpecialHost host, Player owner)
    {
        // Java dùng phép gán trong if nên luôn true; bảo toàn hành vi này.
        IsStartSkillSpecial = true;
        host.UpdateSkillSpecial(owner);
    }

    private void Close()
    {
        IsActive = false;
    }
}
    // Kỹ năng đặc biệt đang chọn.
    // Cờ bắt đầu, bước hiện tại, thời điểm dùng và kiểu vẽ/vật phẩm của chiêu đặc biệt.
    // Tạo trạng thái chiêu đặc biệt cho người chơi và hai danh sách mục tiêu rỗng.
