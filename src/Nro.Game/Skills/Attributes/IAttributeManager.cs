using Nro.Data.Db;

namespace Nro.Game.Skills.Attributes;

// Hợp đồng các tăng cường toàn máy chủ đang hiệu lực.
// Nguồn Java: nro/attribute/AttributeManager.java (lớp chính thay ServerManager singleton).
public interface IAttributeManager
{
    // Snapshot danh sách; đối tượng còn sống nhưng danh sách là bản sao, an toàn khi thêm/xóa đồng thời (Java: getAttributes).
    IReadOnlyList<Attribute> Attributes { get; }

    // Nạp nối thêm attribute_server theo id; mẫu lạ làm thất bại và không nối gì (Java: loadAttributeServer).
    Task LoadAsync(INroSqlExecutor database, IAttributeTemplateManager templates, CancellationToken cancellationToken = default);

    // Ghi mọi thuộc tính changed theo id, không xóa cờ như Java, trả số dòng đã ghi (Java: updateAttributeServer).
    Task<int> SaveChangedAsync(INroSqlExecutor database, CancellationToken cancellationToken = default);

    // Thêm thuộc tính (Java: add).
    void Add(Attribute attribute);

    // Xóa phần tử trùng tham chiếu đầu tiên và cho biết có xóa được không (Java: remove).
    bool Remove(Attribute attribute);

    // Tìm thuộc tính đầu tiên của mẫu, không có trả null (Java: find).
    Attribute? Find(int templateId);

    // Mỗi giây nhiều nhất một lần, trừ một giây ở thuộc tính chưa hết hạn (Java: update).
    void Update();

    // Đổi thời hạn thuộc tính đầu tiên và luôn trả false, kể cả khi tìm thấy, để giữ quirk Java (Java: setTime).
    bool SetTime(int templateId, int time);
}
