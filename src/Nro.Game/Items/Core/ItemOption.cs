using System.Globalization;
using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Templates;

namespace Nro.Game.Items.Core;

// Biểu diễn một tùy chọn vật phẩm gồm mẫu văn bản và tham số thay thế dấu #.
// Nguồn Java: models/Item/ItemOption.java (việc tra mẫu theo id chuyển sang ItemService.CreateOption vì C# không dùng singleton; constructor không tham số dành cho deserialize và OptionTemplate phải được gán trước khi đọc; bản sao chỉ chia sẻ hàng mẫu bất biến).
public class ItemOption
{
    // Thực hiện param; giữ nguyên điều kiện, thứ tự và số học của Java (Java: param).
    public int Param { get; set; }

    // Java optionTemplate. Never null on an option made by a template constructor. (Java: optionTemplate).
    public ItemOptionTemplate OptionTemplate { get; set; } = null!;

    // Thực hiện thành viên; giữ nguyên điều kiện, thứ tự và số học của Java (Java: ItemOption).
    public ItemOption()
    {
    }

    // Thực hiện thành viên; giữ nguyên điều kiện, thứ tự và số học của Java (Java: ItemOption).
    public ItemOption(ItemOption io)
    {
        ArgumentNullException.ThrowIfNull(io);
        Param = io.Param;
        OptionTemplate = io.OptionTemplate;
    }

    // Java ItemOption(ItemOptionTemplate temp, int param). (Java: ItemOption(ItemOptionTemplate temp, int param)).
    public ItemOption(ItemOptionTemplate temp, int param)
    {
        OptionTemplate = temp;
        Param = param;
    }

    // Lấy get option string; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getOptionString).
    public string GetOptionString() => Util.Replace(OptionTemplate.Name, "#", Param.ToString(CultureInfo.InvariantCulture));

    // Kiểm tra is option can upgrade; giữ nguyên điều kiện, thứ tự và số học của Java (Java: isOptionCanUpgrade).
    public bool IsOptionCanUpgrade()
    {
        int opId = OptionTemplate.Id;
        return opId == 0 || opId == 6 || opId == 7 || opId == 14 || opId == 22 || opId == 23 || opId == 27 || opId == 28 || opId == 47;
    }

    // Kiểm tra have expiry date; giữ nguyên điều kiện, thứ tự và số học của Java (Java: haveExpiryDate).
    public bool HaveExpiryDate() => OptionTemplate.Id == 93 || OptionTemplate.Id == 260 || OptionTemplate.Id == 261;

    // Giải phóng dispose; giữ nguyên điều kiện, thứ tự và số học của Java (Java: dispose).
    public void Dispose()
    {
    }

    // Thực hiện to string; giữ nguyên điều kiện, thứ tự và số học của Java (Java: toString).
    public override string ToString()
    {
        const string q = "\"";
        return "{" + q + "id" + q + ":" + q + OptionTemplate.Id.ToString(CultureInfo.InvariantCulture) + q + ","
            + q + "param" + q + ":" + q + Param.ToString(CultureInfo.InvariantCulture) + q + "}";
    }
}
