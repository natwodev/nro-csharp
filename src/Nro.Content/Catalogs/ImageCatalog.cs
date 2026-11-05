using System.Collections.ObjectModel;
using Nro.Core.Content;

namespace Nro.Content.Catalogs;

// Danh mục ảnh theo tên, dùng khóa không phân biệt hoa thường như cột citext. Nguồn Java: nro/server/Manager.java (dòng 727–741).
public sealed class ImageCatalog : IImageCatalog
{
    private readonly IReadOnlyDictionary<string, sbyte> framesByName;

    // Đóng danh mục từ ánh xạ đã kiểm tên ảnh không trùng. (Java: IMAGES_BY_NAME)
    public ImageCatalog(IReadOnlyDictionary<string, sbyte> framesByName)
    {
        ArgumentNullException.ThrowIfNull(framesByName);
        var copy = new Dictionary<string, sbyte>(StringComparer.OrdinalIgnoreCase);
        foreach ((string name, sbyte frameCount) in framesByName)
        {
            if (!copy.TryAdd(name, frameCount))
            {
                throw new ArgumentException($"Tên ảnh bị trùng: '{name}'.", nameof(framesByName));
            }
        }

        this.framesByName = new ReadOnlyDictionary<string, sbyte>(copy);
    }

    // Ảnh theo tên và số khung. (Java: IMAGES_BY_NAME)
    public IReadOnlyDictionary<string, sbyte> FramesByName => framesByName;

    // Thử lấy số khung của ảnh. (Java: IMAGES_BY_NAME.get)
    public bool TryGetFrame(string name, out sbyte frameCount)
    {
        ArgumentNullException.ThrowIfNull(name);
        return framesByName.TryGetValue(name, out frameCount);
    }
}
