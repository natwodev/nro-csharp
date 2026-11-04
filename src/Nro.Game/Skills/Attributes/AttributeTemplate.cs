namespace Nro.Game.Skills.Attributes;

// Mô tả bất biến của một tăng cường toàn máy chủ; #value đại diện cho giá trị.
// Nguồn Java: nro/attribute/AttributeTemplate.java (chỉ có getter nên giữ bất biến).
public sealed class AttributeTemplate
{
    public int Id { get; }

    public string Name { get; }

    public AttributeTemplate(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
    // Mã mẫu thuộc tính.
    // Tên/mẫu văn bản có thể chứa #value.
    // Tạo mẫu thuộc tính bất biến.
