namespace Nro.Core.Templates;

// Ánh xạ mã đầu nhân vật tới mã avatar.
// Nguồn Java: nro/template/HeadAvatar.java (giữ constructor hai tham số duy nhất của Java, không có constructor mặc định).
public class HeadAvatar
{
    // Mã đầu nhân vật (Java: headId)
    public int HeadId { get; set; }

    // Mã avatar tương ứng (Java: avatarId)
    public int AvatarId { get; set; }

    public HeadAvatar(int headId, int avatarId)
    {
        HeadId = headId;
        AvatarId = avatarId;
    }
}
