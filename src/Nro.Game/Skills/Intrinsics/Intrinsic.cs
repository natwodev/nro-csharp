using System.Globalization;
using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Skills.Intrinsics;

// Mô tả một nội tại: catalog giữ khoảng tham số, còn người chơi giữ bản sao có giá trị đã quay.
// Nguồn Java: nro/intrinsic/Intrinsic.java (copy không chép Param1/Param2 nên khởi đầu 0; p0..p3 thay theo chuỗi thường, tương đương replaceAll mẫu literal Java).
public class Intrinsic
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public short ParamFrom1 { get; set; }

    public short ParamTo1 { get; set; }

    public short ParamFrom2 { get; set; }

    public short ParamTo2 { get; set; }

    public short Icon { get; set; }

    // Hệ áp dụng: 0 Trái Đất, 1 Namek, 2 Saiyan, giá trị khác áp dụng tất cả (Java: gender).
    public sbyte Gender { get; set; }

    public short Param1 { get; set; }

    public short Param2 { get; set; }

    public Intrinsic()
    {
    }

    // Sao chép mẫu nhưng cố ý không chép Param1/Param2 để chúng bắt đầu 0 (Java: Intrinsic).
    public Intrinsic(Intrinsic intrinsic)
    {
        ArgumentNullException.ThrowIfNull(intrinsic);
        Id = intrinsic.Id;
        Name = intrinsic.Name;
        ParamFrom1 = intrinsic.ParamFrom1;
        ParamTo1 = intrinsic.ParamTo1;
        ParamFrom2 = intrinsic.ParamFrom2;
        ParamTo2 = intrinsic.ParamTo2;
        Icon = intrinsic.Icon;
        Gender = intrinsic.Gender;
    }

    // Thay lần lượt p0..p3 trong tên bằng bốn cận khoảng (Java: getDescription).
    public string GetDescription()
    {
        string text = Util.Replace(Name, "p0", ParamFrom1.ToString(CultureInfo.InvariantCulture));
        text = Util.Replace(text, "p1", ParamTo1.ToString(CultureInfo.InvariantCulture));
        text = Util.Replace(text, "p2", ParamFrom2.ToString(CultureInfo.InvariantCulture));
        return Util.Replace(text, "p3", ParamTo2.ToString(CultureInfo.InvariantCulture));
    }

    // Rút hai khoảng phần trăm thành p0/p1, thay bằng giá trị quay; nội tại khác id 0 thêm khoảng thứ nhất (Java: getName).
    public string GetName()
    {
        string text = Util.Replace(Name, "p0% đến p1", "p0");
        text = Util.Replace(text, "p2% đến p3", "p1");
        text = Util.Replace(text, "p0", Param1.ToString(CultureInfo.InvariantCulture));
        text = Util.Replace(text, "p1", Param2.ToString(CultureInfo.InvariantCulture));
        return text + (Id != 0 ? " [" + ParamFrom1.ToString(CultureInfo.InvariantCulture) + " đến " + ParamTo1.ToString(CultureInfo.InvariantCulture) + "]" : "");
    }
}
    // Mã, tên, hai khoảng tham số, biểu tượng và hệ áp dụng của nội tại.
    // Hai tham số thực tế đã quay cho người chơi.
    // Tạo nội tại rỗng để trình nạp điền dữ liệu.
