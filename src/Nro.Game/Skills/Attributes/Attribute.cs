using System.Globalization;
using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Skills.Attributes;

// Biểu diễn một thuộc tính tăng cường toàn máy chủ với giá trị và thời hạn.
// Nguồn Java: nro/attribute/Attribute.java (nhận Template đã tra để lỗi ngay lúc nạp; trường volatile an toàn với tick; tên Attribute giữ Java và che System.Attribute trong namespace này).
public class Attribute
{
    private volatile int value;
    private volatile int time;
    private volatile bool changed;
    private volatile AttributeTemplate template;

    // Tạo thuộc tính với mẫu đã tra cứu; mẫu null bị từ chối ngay (Java: Attribute).
    public Attribute(int id, AttributeTemplate template, int value, int time)
    {
        ArgumentNullException.ThrowIfNull(template);
        Id = id;
        this.value = value;
        this.time = time;
        this.template = template;
    }

    public int Id { get; }

    public int Value => value;

    // Số giây còn lại; -1 không hết hạn, 0 đã hết hạn.
    public int Time => time;

    public AttributeTemplate Template => template;

    // Cờ đã thay đổi từ khi nạp để ghi lại; cố ý không xóa như Java (Java: isChanged).
    public bool IsChanged => changed;

    // Trừ một giây khi thời hạn dương, đánh dấu thay đổi và trả true; các trường hợp khác trả false (Java: update).
    public bool Update()
    {
        if (time > 0)
        {
            time--;
            SetChanged();
            return true;
        }

        return false;
    }

    public void SetChanged()
    {
        changed = true;
    }

    // Kiểm tra hết hạn chính xác ở 0; -1 không hết hạn (Java: isExpired).
    public bool IsExpired() => time == 0;

    // Đổi giá trị và đánh dấu cần lưu (Java: setValue).
    public void SetValue(int newValue)
    {
        value = newValue;
        SetChanged();
    }

    // Đổi thời hạn và đánh dấu cần lưu (Java: setTime).
    public void SetTime(int newTime)
    {
        time = newTime;
        SetChanged();
    }

    // Đổi mẫu không-null và đánh dấu cần lưu (Java: setTemplate).
    public void SetTemplate(AttributeTemplate newTemplate)
    {
        ArgumentNullException.ThrowIfNull(newTemplate);
        template = newTemplate;
        SetChanged();
    }

    // Ghép tên mẫu đã thay #value và thời hạn dạng TimeUtil, trừ khi thời hạn là -1 (Java: toString).
    public override string ToString()
    {
        string text = Util.Replace(template.Name, "#value", value.ToString(CultureInfo.InvariantCulture));
        int currentTime = time;
        if (currentTime != -1)
        {
            string strTimeAgo = TimeUtil.GetTimeAgo(currentTime);
            text += " trong vòng " + strTimeAgo;
        }

        return text;
    }
}
    // Mã bản ghi thuộc tính.
    // Giá trị tăng cường hiện tại.
    // Mẫu mô tả loại thuộc tính.
    // Đánh dấu bản ghi cần lưu lại.
