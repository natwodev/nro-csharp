using Nro.Core.IO;
using Nro.Network.Configurations;

namespace Nro.Game.Skills;

// Quản lý kỹ năng đã học, kỹ năng chọn, phím tắt và trạng thái chuẩn bị chiêu của một người chơi.
// Nguồn Java: nro/skill/PlayerSkill.java (gửi phím tắt tách thành tạo/gửi Message, không nuốt lỗi; tra Skill kiểm tra Template null thay vì ném NullPointerException).
public class PlayerSkill
{
    // Mã sub-command của gói phím tắt Java messageSubCommand((byte)61).
    private const sbyte ShortCutSubCommand = 61;

    // Chủ sở hữu, null ở đối tượng trần và sau Dispose (Java: player).
    public Player? Player { get; private set; }

    // Danh sách kỹ năng đã học, null ở đối tượng trần và sau Dispose (Java: skills).
    public List<Skill>? Skills { get; set; }

    // Kỹ năng đang chọn.
    public Skill? SkillSelect { get; set; }

    // Mốc thời gian của trạng thái học kỹ năng (Java: Time).
    public long Time { get; set; }

    // Mã mẫu vật phẩm kỹ năng (Java: ItemTemplateSkillId).
    public short ItemTemplateSkillId { get; set; }

    // Tiềm năng tích lũy (Java: Potential).
    public long Potential { get; set; }

    // Mã kỹ năng của 10 ô phím tắt; kiểu có dấu và -1 là ô trống trong dữ liệu lưu (Java: skillShortCut).
    public sbyte[] SkillShortCut { get; set; } = new sbyte[10];

    public bool PrepareQCKK { get; set; }

    public bool PrepareTuSat { get; set; }

    public bool PrepareLaze { get; set; }

    public long LastTimePrepareQCKK { get; set; }

    public long LastTimePrepareTuSat { get; set; }

    public long LastTimePrepareLaze { get; set; }

    // Tạo ô học kỹ năng trần: Time và ItemTemplateSkillId bằng -1, không có chủ hay danh sách (Java: PlayerSkill()).
    public PlayerSkill()
    {
        Time = -1;
        ItemTemplateSkillId = -1;
        Potential = 0;
    }

    // Tạo bộ kỹ năng thật của người chơi với danh sách rỗng; Time và ItemTemplateSkillId giữ 0 như Java.
    public PlayerSkill(Player player)
    {
        Player = player;
        Skills = [];
    }

    // Tìm kỹ năng học đầu tiên có mã mẫu yêu cầu, không có trả null; ném InvalidOperationException nếu danh sách null (Java: getSkillbyId).
    public Skill? GetSkillbyId(int id)
    {
        foreach (Skill skill in RequireSkills())
        {
            if (skill.Template is not null && skill.Template.Id == id)
            {
                return skill;
            }
        }

        return null;
    }

    // Tạo hai Message KSkill/OSkill, mỗi gói có sub-command 61, độ dài int và byte thô; bên gọi phải Cleanup hoặc dùng SendSkillShortCut.
    public IReadOnlyList<Message> BuildSkillShortCutMessages()
    {
        byte[] raw = new byte[SkillShortCut.Length];
        for (int i = 0; i < raw.Length; i++)
        {
            raw[i] = unchecked((byte)SkillShortCut[i]);
        }

        var result = new List<Message>(2);
        foreach (string kind in new[] { "KSkill", "OSkill" })
        {
            var msg = new Message(Cmd_message.SUB_COMMAND);
            msg.Writer().WriteByte(ShortCutSubCommand);
            msg.Writer().WriteUTF(kind);
            msg.Writer().WriteInt(raw.Length);
            msg.Writer().Write(raw);
            result.Add(msg);
        }

        return result;
    }

    // Gửi hai gói phím tắt và luôn Cleanup; ngoại lệ từ sender không bị nuốt khác với Java (Java: sendSkillShortCut).
    public void SendSkillShortCut(Action<Message> send)
    {
        ArgumentNullException.ThrowIfNull(send);
        IReadOnlyList<Message> messages = BuildSkillShortCutMessages();
        try
        {
            foreach (Message msg in messages)
            {
                send(msg);
            }
        }
        finally
        {
            foreach (Message msg in messages)
            {
                msg.Cleanup();
            }
        }
    }

    // Chọn chỉ số hoạt ảnh/nút: 1 đấm, 2 chưởng, 4 ba chưởng đặc biệt, còn lại 3; ném InvalidOperationException khi thiếu kỹ năng hay mẫu (Java: getIndexSkillSelect).
    public sbyte GetIndexSkillSelect()
    {
        SkillTemplate template = (SkillSelect ?? throw new InvalidOperationException("No skill is selected")).Template
            ?? throw new InvalidOperationException("The selected skill has no template");
        switch (template.Id)
        {
            case Skill.DRAGON:
            case Skill.DEMON:
            case Skill.GALICK:
            case Skill.KAIOKEN:
            case Skill.LIEN_HOAN:
                return 1;
            case Skill.KAMEJOKO:
            case Skill.ANTOMIC:
            case Skill.MASENKO:
                return 2;
            case Skill.LIEN_HOAN_CHUONG:
            case Skill.SUPER_KAME:
            case Skill.MA_PHONG_BA:
                return 4;
            default:
                return 3;
        }
    }

    // Đếm kỹ năng có SkillId khác -1; sbyte bảo toàn tràn Java từ 127 sang -128, và ném InvalidOperationException nếu danh sách null (Java: getSizeSkill).
    public sbyte GetSizeSkill()
    {
        sbyte size = 0;
        foreach (Skill skill in RequireSkills())
        {
            if (skill.SkillId != -1)
            {
                size = unchecked((sbyte)(size + 1));
            }
        }

        return size;
    }

    // Dispose kỹ năng chọn và toàn bộ danh sách, xóa danh sách rồi bỏ chủ sở hữu, lựa chọn và danh sách (Java: dispose).
    public void Dispose()
    {
        SkillSelect?.Dispose();
        if (Skills is not null)
        {
            foreach (Skill skill in Skills)
            {
                skill.Dispose();
            }

            Skills.Clear();
        }

        Player = null;
        SkillSelect = null;
        Skills = null;
    }

    private List<Skill> RequireSkills() => Skills ?? throw new InvalidOperationException("This PlayerSkill has no skill list (it was created without a player or already disposed)");
}
    // Cờ và mốc thời gian chuẩn bị Quả Cầu Kênh Khí, Tự Sát và Laze.
