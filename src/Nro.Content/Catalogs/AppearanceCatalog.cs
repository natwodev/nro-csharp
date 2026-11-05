using System.Collections.ObjectModel;
using Nro.Core.Content;
using Nro.Core.Templates;

namespace Nro.Content.Catalogs;

// Danh mục ngoại hình chỉ đọc, giữ tra cứu theo id và frame theo vị trí. Nguồn Java: nro/server/Manager.java (dòng 351–380, 560–600).
public sealed class AppearanceCatalog : IAppearanceCatalog
{
    private readonly ReadOnlyCollection<Part> parts;
    private readonly ReadOnlyCollection<HeadAvatar> headAvatars;
    private readonly ReadOnlyCollection<FlagBag> flagBags;
    private readonly ReadOnlyCollection<ArrHead2Frames> headFrames;
    private readonly IReadOnlyDictionary<int, Part> partsById;
    private readonly IReadOnlyDictionary<int, HeadAvatar> headAvatarsById;
    private readonly IReadOnlyDictionary<int, FlagBag> flagBagsById;

    // Đóng danh mục từ dữ liệu đã parse và xác thực. (Java: parts, HEAD_AVATARS, FLAGS_BAGS, ARR_HEAD_2_FRAMES)
    public AppearanceCatalog(IEnumerable<Part> parts, IEnumerable<HeadAvatar> headAvatars, IEnumerable<FlagBag> flagBags, IEnumerable<ArrHead2Frames> headFrames)
    {
        ArgumentNullException.ThrowIfNull(parts);
        ArgumentNullException.ThrowIfNull(headAvatars);
        ArgumentNullException.ThrowIfNull(flagBags);
        ArgumentNullException.ThrowIfNull(headFrames);

        this.parts = Array.AsReadOnly(parts.ToArray());
        this.headAvatars = Array.AsReadOnly(headAvatars.ToArray());
        this.flagBags = Array.AsReadOnly(flagBags.ToArray());
        this.headFrames = Array.AsReadOnly(headFrames.ToArray());
        partsById = new ReadOnlyDictionary<int, Part>(this.parts.ToDictionary(part => part.Id));
        headAvatarsById = new ReadOnlyDictionary<int, HeadAvatar>(this.headAvatars.ToDictionary(avatar => avatar.HeadId));
        flagBagsById = new ReadOnlyDictionary<int, FlagBag>(this.flagBags.ToDictionary(flag => flag.Id));
    }

    // Các part đã nạp. (Java: parts)
    public IReadOnlyList<Part> Parts => parts;

    // Các avatar đầu đã nạp. (Java: HEAD_AVATARS)
    public IReadOnlyList<HeadAvatar> HeadAvatars => headAvatars;

    // Các túi cờ đã nạp. (Java: FLAGS_BAGS)
    public IReadOnlyList<FlagBag> FlagBags => flagBags;

    // Các frame đầu theo vị trí id. (Java: ARR_HEAD_2_FRAMES)
    public IReadOnlyList<ArrHead2Frames> HeadFrames => headFrames;

    // Lấy part theo id. (Java: parts)
    public Part GetPart(int id) => partsById.TryGetValue(id, out Part? value)
        ? value
        : throw new KeyNotFoundException($"Không có part id {id}.");

    // Lấy avatar đầu theo id đầu. (Java: HEAD_AVATARS)
    public HeadAvatar GetHeadAvatar(int headId) => headAvatarsById.TryGetValue(headId, out HeadAvatar? value)
        ? value
        : throw new KeyNotFoundException($"Không có avatar đầu id {headId}.");

    // Lấy túi cờ theo id. (Java: FLAGS_BAGS)
    public FlagBag GetFlagBag(int id) => flagBagsById.TryGetValue(id, out FlagBag? value)
        ? value
        : throw new KeyNotFoundException($"Không có túi cờ id {id}.");

    // Lấy hoạt ảnh đầu theo vị trí id đã được kiểm liên tục. (Java: ARR_HEAD_2_FRAMES.get)
    public ArrHead2Frames GetHeadAnimationFrame(int id) => (uint)id < (uint)headFrames.Count
        ? headFrames[id]
        : throw new ArgumentOutOfRangeException(nameof(id), id, "Không có hoạt ảnh đầu tại id đã cho.");
}
