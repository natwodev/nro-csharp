using Nro.Data.Db;

namespace Nro.Game.Skills.Attributes;

// Hợp đồng các mẫu thuộc tính tăng cường.
// Nguồn Java: nro/attribute/AttributeTemplateManager.java (lớp chính thay singleton Java).
public interface IAttributeTemplateManager
{
    // Snapshot mẫu theo thứ tự nạp; Java để private, ở đây chỉ đọc phục vụ kiểm thử và công cụ.
    IReadOnlyList<AttributeTemplate> Templates { get; }

    // Nạp nối thêm attribute_template theo id; ném lỗi DB thay vì in và không nối dữ liệu khi lỗi (Java: load).
    Task LoadAsync(INroSqlExecutor database, CancellationToken cancellationToken = default);

    // Thêm mẫu (Java: add).
    void Add(AttributeTemplate template);

    // Xóa tham chiếu mẫu đầu tiên và cho biết có xóa được không (Java: remove).
    bool Remove(AttributeTemplate template);

    // Tìm mẫu đầu tiên theo id, không có trả null (Java: find).
    AttributeTemplate? Find(int id);
}
