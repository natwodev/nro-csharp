namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/HeadAvatar.java</c>. Java has only one two-parameter constructor (no default constructor).
/// </summary>
public class HeadAvatar
{
    public int HeadId { get; set; }

    public int AvatarId { get; set; }

    public HeadAvatar(int headId, int avatarId)
    {
        HeadId = headId;
        AvatarId = avatarId;
    }
}
