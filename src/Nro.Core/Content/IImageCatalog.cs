namespace Nro.Core.Content;

// Hợp đồng tra số khung ảnh theo tên, không phụ thuộc tầng dữ liệu. Nguồn Java: nro/server/Manager.java (dòng 727–741).
public interface IImageCatalog
{
    // Các ảnh theo tên và số khung, khóa không phân biệt hoa thường như PostgreSQL citext. (Java: IMAGES_BY_NAME)
    IReadOnlyDictionary<string, sbyte> FramesByName { get; }

    // Thử lấy số khung của ảnh. (Java: IMAGES_BY_NAME.get)
    bool TryGetFrame(string name, out sbyte frameCount);
}
