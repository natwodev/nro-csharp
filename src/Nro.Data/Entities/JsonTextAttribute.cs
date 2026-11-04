namespace Nro.Data.Entities;

// Thuộc tính đánh dấu cột text chứa JSON hoặc mảng lồng; giá trị được lưu và đọc đúng nguyên chuỗi, giữ mọi ký tự theo quy tắc D8 của kế hoạch port; tầng entity không phân tích hay định dạng lại; chỉ là metadata, không có hành vi.
// Nguồn Java: không có tương ứng trong Java, là thuộc tính đánh dấu của bản C#.
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class JsonTextAttribute : Attribute
{
}
