using Nro.Core.Templates;

namespace Nro.Core.Content;

// Hợp đồng đọc dữ liệu ngoại hình tĩnh, không phụ thuộc tầng dữ liệu. Nguồn Java: nro/server/Manager.java (dòng 192–218).
public interface IAppearanceCatalog
{
    // Các bộ phận ngoại hình theo id. (Java: parts)
    IReadOnlyList<Part> Parts { get; }

    // Các avatar đầu nhân vật theo id. (Java: HEAD_AVATARS)
    IReadOnlyList<HeadAvatar> HeadAvatars { get; }

    // Các túi cờ theo id. (Java: FLAGS_BAGS)
    IReadOnlyList<FlagBag> FlagBags { get; }

    // Các frame đầu nhân vật theo id. (Java: ARR_HEAD_2_FRAMES)
    IReadOnlyList<ArrHead2Frames> HeadFrames { get; }

    // Tra bộ phận ngoại hình theo id. (Java: parts)
    Part GetPart(int id);

    // Tra avatar đầu theo id đầu. (Java: HEAD_AVATARS)
    HeadAvatar GetHeadAvatar(int headId);

    // Tra túi cờ theo id. (Java: FLAGS_BAGS)
    FlagBag GetFlagBag(int id);

    // Tra frame đầu theo vị trí id đã được kiểm liên tục. (Java: ARR_HEAD_2_FRAMES.get)
    ArrHead2Frames GetHeadAnimationFrame(int id);
}
