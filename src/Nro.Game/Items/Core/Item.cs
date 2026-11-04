using System.Globalization;
using Nro.Core.Templates;

namespace Nro.Game.Items.Core;

// Biểu diễn một vật phẩm gồm mẫu, số lượng và danh sách tùy chọn; ô trống là Item có Template null, không phải tham chiếu null.
// Nguồn Java: models/Item/Item.java (thời điểm tạo và ItemCatalog được tiêm thay System.currentTimeMillis/singleton để kiểm thử; danh sách tùy chọn luôn không null và Dispose chỉ làm rỗng; thiếu template ném InvalidOperationException thay NullPointerException; CloneItem sao chép từng ItemOption nhưng không sao chép quantityGD).
public class Item
{
    // Mã tùy chọn nhận diện bộ kích hoạt (Java: KichHoatIds).
    private static readonly HashSet<int> ActivationSetIds = [127, 128, 129, 130, 131, 132, 133, 134, 135];

    // Thực hiện template; giữ nguyên điều kiện, thứ tự và số học của Java (Java: template).
    public ItemTemplate? Template { get; set; }

    // Thực hiện info; giữ nguyên điều kiện, thứ tự và số học của Java (Java: info).
    public string? Info { get; set; }

    // Thực hiện content; giữ nguyên điều kiện, thứ tự và số học của Java (Java: content).
    public string? Content { get; set; }

    // Java quantity. (Java: quantity).
    public int Quantity { get; set; }

    // (quantity put into a trade). (Java: quantityGD).
    public int QuantityGD { get; set; }

    // Java itemOptions; never null. (Java: itemOptions).
    public List<ItemOption> ItemOptions { get; set; } = [];

    // Tạo create time; giữ nguyên điều kiện, thứ tự và số học của Java (Java: createTime).
    public long CreateTime { get; set; }

    // Thực hiện catalog; giữ nguyên điều kiện, thứ tự và số học của Java.
    public ItemCatalog? Catalog { get; set; }

    // Thực hiện thành viên; giữ nguyên điều kiện, thứ tự và số học của Java (Java: Item).
    public Item(ItemCatalog? catalog = null, long createTimeMs = 0, short? itemId = null)
    {
        Catalog = catalog;
        CreateTime = createTimeMs;
        if (itemId is { } id)
        {
            Template = (catalog ?? throw new InvalidOperationException("an item built from a template id needs a catalog")).GetTemplate(id);
        }
    }

    private ItemTemplate T => Template ?? throw new InvalidOperationException("this item has no template (Java: NullPointerException)");

    // Thực hiện required template; giữ nguyên điều kiện, thứ tự và số học của Java (Java: NullPointerException on item.template).
    internal ItemTemplate RequiredTemplate => T;

    private ItemCatalog RequiredCatalog => Catalog ?? throw new InvalidOperationException("this item has no catalog to resolve option ids");

    // Java isNotNullItem(). (Java: isNotNullItem()).
    public bool IsNotNullItem() => Template != null;

    // Java Name(). (Java: Name()).
    public string Name() => T.Name;

    // Lấy get info; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getInfo).
    public string GetInfo()
    {
        string strInfo = "";
        foreach (ItemOption itemOption in ItemOptions)
        {
            strInfo += itemOption.GetOptionString();
        }

        return strInfo;
    }

    // Java getContent(). (Java: getContent()).
    public string GetContent() => "Yêu cầu sức mạnh " + T.StrRequire.ToString(CultureInfo.InvariantCulture) + " trở lên";

    // Giải phóng dispose; giữ nguyên điều kiện, thứ tự và số học của Java (Java: dispose).
    public void Dispose()
    {
        Template = null;
        Info = null;
        Content = null;
        foreach (ItemOption io in ItemOptions)
        {
            io.Dispose();
        }

        ItemOptions.Clear();
    }

    // Kiểm tra is bug; giữ nguyên điều kiện, thứ tự và số học của Java (Java: isBUg).
    public bool IsBUg()
    {
        foreach (ItemOption itemOption in ItemOptions)
        {
            int id = itemOption.OptionTemplate.Id;
            if (id != 249 && (id == 50 || id == 77 || id == 103 || id == 5) && itemOption.Param > 35)
            {
                return true;
            }
        }

        return false;
    }

    // Kiểm tra is skh; giữ nguyên điều kiện, thứ tự và số học của Java (Java: isSKH).
    public bool IsSKH()
    {
        foreach (ItemOption itemOption in ItemOptions)
        {
            if (itemOption.OptionTemplate.Id >= 127 && itemOption.OptionTemplate.Id <= 135)
            {
                return true;
            }
        }

        return false;
    }

    // template level 15. (Java: isDTS()).
    public bool IsDTS() => T.Level == 15;

    // template level 13. (Java: isDTL()).
    public bool IsDTL() => T.Level == 13;

    // template level 14. (Java: isDHD()).
    public bool IsDHD() => T.Level == 14;

    // Kiểm tra is dhd1; giữ nguyên điều kiện, thứ tự và số học của Java (Java: isDHD1).
    public bool IsDHD1() => T.Id >= 650 && T.Id <= 662;

    // Kiểm tra is dtlnew; giữ nguyên điều kiện, thứ tự và số học của Java (Java: isDTLNew).
    public bool IsDTLNew() => T.Id >= 555 && T.Id <= 567;

    // Java isManhThienSu(). (Java: isManhThienSu()).
    public bool IsManhThienSu() => T.Id >= 1066 && T.Id <= 1070;

    // Java isDaMayMan(). (Java: isDaMayMan()).
    public bool IsDaMayMan() => T.Id >= 1079 && T.Id <= 1083;

    // Java isDaNangCapTS(). (Java: isDaNangCapTS()).
    public bool IsDaNangCapTS() => T.Id >= 1074 && T.Id <= 1078;

    // Java isCongThuc(). (Java: isCongThuc()).
    public bool IsCongThuc() => T.Id >= 1071 && T.Id <= 1073;

    // Java isCongThucVip(). (Java: isCongThucVip()).
    public bool IsCongThucVip() => T.Id >= 1084 && T.Id <= 1086;

    // template type 14. (Java: isDaNangCap()).
    public bool IsDaNangCap() => T.Type == 14;

    // Thực hiện type name; giữ nguyên điều kiện, thứ tự và số học của Java (Java: typeName).
    public string TypeName() => T.Type switch
    {
        0 => "Áo",
        1 => "Quần",
        2 => "Găng",
        3 => "Giày",
        4 => "Rada",
        _ => "",
    };

    // Lấy get gender name; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getGenderName).
    public string GetGenderName() => T.Gender == 0 ? "Trái Đất" : T.Gender == 1 ? "Namếc" : "Xay da";

    // Thực hiện type manh; giữ nguyên điều kiện, thứ tự và số học của Java (Java: typeManh).
    public sbyte TypeManh() => T.Id switch
    {
        1066 => 0,
        1067 => 1,
        1070 => 2,
        1068 => 3,
        1069 => 4,
        _ => -1,
    };

    // Kiểm tra sách tuyệt kỹ (Java: isSachTuyetKy).
    public bool IsUltimateSkillBook() => T.Id == 1044 || T.Id == 1211 || T.Id == 1212;

    // Kiểm tra biến thể 2 của sách tuyệt kỹ (Java: isSachTuyetKy2).
    public bool IsUltimateSkillBookVariant2() => T.Id >= 1278 && T.Id <= 1280;

    // Kiểm tra can nang cap with ndc; giữ nguyên điều kiện, thứ tự và số học của Java (Java: canNangCapWithNDC).
    public bool CanNangCapWithNDC(Item daNangCap)
    {
        ArgumentNullException.ThrowIfNull(daNangCap);
        ItemTemplate t = T;
        ItemTemplate stone = daNangCap.T;
        if (t.Type == 0 && stone.Id == 223)
        {
            return true;
        }

        if (t.Type == 1 && stone.Id == 222)
        {
            return true;
        }

        if (t.Type == 2 && stone.Id == 224)
        {
            return true;
        }

        if (t.Type == 3 && stone.Id == 221)
        {
            return true;
        }

        return t.Type == 4 && stone.Id == 220;
    }

    // Kiểm tra đá pha lê ép sao (Java: isDaPhaLeEpSao).
    public bool IsCrystalStarUpgradeStone() => Template != null && (Template.Type == 30 || (Template.Id >= 14 && Template.Id <= 20));

    // Kiểm tra đá pha lê cấp 1 (Java: isDaPhaLeC1).
    public bool IsCrystalTier1() => Template != null && Template.Id >= 411 && Template.Id <= 447;

    // Kiểm tra đá pha lê cấp 2; giữ nguyên ưu tiên toán tử Java nên vật phẩm thiếu Template vẫn truy cập T ở vế sau và ném lỗi (Java: isDaPhaLeC2).
    public bool IsCrystalTier2() => (Template != null && Template.Id >= 1416 && Template.Id <= 1422) || T.Id == 964 || T.Id == 965;

    // Kiểm tra đá pha lê mới; giữ nguyên ưu tiên toán tử Java nên vật phẩm thiếu Template vẫn truy cập T ở các vế sau và ném lỗi (Java: isDaPhaLeMoi).
    public bool IsCrystalTierNew() => (Template != null && Template.Id >= 1416 && Template.Id <= 1422) || T.Id == 964 || T.Id == 965 || (T.Id >= 1426 && T.Id <= 1434);

    // Kiểm tra đá pha lê cũ (Java: isDaPhaLeCu).
    public bool IsCrystalTierOld() => Template != null && Template.Id >= 441 && Template.Id <= 447;

    // Kiểm tra loại trang bị trên người; giữ nguyên ưu tiên Java nên vật phẩm thiếu Template vẫn có thể truy cập T ở vế sau và ném lỗi (Java: isTypeBody).
    public bool IsTypeBody() => (Template != null && 0 <= Template.Type && Template.Type < 6) || T.Type == 32 || T.Type == 35 || T.Type == 11 || T.Type == 23;

    // Java isHaveOption(id). (Java: isHaveOption(id)).
    public bool IsHaveOption(int id)
    {
        for (int i = 0; i < ItemOptions.Count; i++)
        {
            ItemOption itemOption = ItemOptions[i];
            if (itemOption != null && itemOption.OptionTemplate.Id == id)
            {
                return true;
            }
        }

        return false;
    }

    // Tính phần trăm tùy chọn trang bị so với giá trị shop sau khi đảo hệ số sao; chia nguyên, mẫu 0 ném DivideByZeroException, thiếu shop cho phần trăm âm và param * 100 tràn int như Java (Java: getPercentOption).
    public int GetPercentOption()
    {
        int partOptionId;
        switch (T.Type)
        {
            case 0:
                partOptionId = 47;
                break;
            case 1:
                partOptionId = 6;
                break;
            case 2:
                partOptionId = 0;
                break;
            case 3:
                partOptionId = 7;
                break;
            case 4:
                partOptionId = 14;
                break;
            default:
                return 0;
        }

        int paramZin = RequiredCatalog.GetOptionParamItemShop(T.Id, partOptionId);
        int param = ItemUpgradeMath.ReversePoint(GetOptionParam(partOptionId), GetOptionParam(72));
        return param * 100 / paramZin;
    }

    // Lấy get option param; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getOptionParam).
    public int GetOptionParam(int id)
    {
        for (int i = 0; i < ItemOptions.Count; i++)
        {
            ItemOption itemOption = ItemOptions[i];
            if (itemOption != null && itemOption.OptionTemplate.Id == id)
            {
                return itemOption.Param;
            }
        }

        return 0;
    }

    // Thêm add option param; giữ nguyên điều kiện, thứ tự và số học của Java (Java: addOptionParam).
    public void AddOptionParam(int id, int param)
    {
        for (int i = 0; i < ItemOptions.Count; i++)
        {
            ItemOption itemOption = ItemOptions[i];
            if (itemOption != null && itemOption.OptionTemplate.Id == id)
            {
                itemOption.Param += param;
                return;
            }
        }

        ItemOptions.Add(new ItemOption(RequiredCatalog.GetItemOptionTemplate(id), param));
    }

    // Lấy get option param; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getOptionParam).
    public void GetOptionParam(int id, int param)
    {
        for (int i = 0; i < ItemOptions.Count; i++)
        {
            ItemOption itemOption = ItemOptions[i];
            if (itemOption != null && itemOption.OptionTemplate.Id == id)
            {
                itemOption.Param = param;
                return;
            }
        }
    }

    // Java subOptionParam(id, param). (Java: subOptionParam(id, param)).
    public void SubOptionParam(int id, int param)
    {
        for (int i = 0; i < ItemOptions.Count; i++)
        {
            ItemOption itemOption = ItemOptions[i];
            if (itemOption != null && itemOption.OptionTemplate.Id == id)
            {
                itemOption.Param -= param;
                return;
            }
        }
    }

    // Trừ tham số tùy chọn và chỉ xóa khi chính đối số param không dương, không phải khi giá trị mới không dương; giữ đúng điều kiện Java (Java: subOptionParamAndRemoveIfZero).
    public void SubOptionParamAndRemoveIfZero(int id, int param)
    {
        for (int i = 0; i < ItemOptions.Count; i++)
        {
            ItemOption itemOption = ItemOptions[i];
            if (itemOption != null && itemOption.OptionTemplate.Id == id)
            {
                itemOption.Param -= param;
                if (param <= 0)
                {
                    ItemOptions.RemoveAt(i);
                }

                break;
            }
        }
    }

    // Xóa remove option; giữ nguyên điều kiện, thứ tự và số học của Java (Java: removeOption).
    public void RemoveOption(int id)
    {
        for (int i = 0; i < ItemOptions.Count; i++)
        {
            ItemOption itemOption = ItemOptions[i];
            if (itemOption != null && itemOption.OptionTemplate.Id == id)
            {
                ItemOptions.RemoveAt(i);
                break;
            }
        }
    }

    // Lấy tùy chọn do đá pha lê tạo; đá không rõ trả chính đối tượng tùy chọn đầu của vật phẩm, không phải bản sao, và danh sách rỗng ném ArgumentOutOfRangeException như IndexOutOfBoundsException Java (Java: getOptionDaPhaLe).
    public ItemOption GetCrystalOption()
    {
        (int optionId, int param)? pair = T.Id switch
        {
            20 => (77, 5),
            19 => (103, 5),
            18 => (80, 5),
            17 => (81, 5),
            16 => (50, 3),
            15 => (94, 2),
            14 => (108, 2),
            441 => (95, 5),
            442 => (96, 5),
            443 => (97, 5),
            444 => (98, 5),
            445 => (99, 5),
            446 => (100, 5),
            447 => (101, 5),
            1416 => (95, 5),
            1417 => (96, 5),
            1418 => (97, 5),
            1419 => (98, 5),
            1420 => (99, 5),
            1421 => (100, 5),
            1422 => (101, 5),
            1426 => (95, 5),
            1427 => (96, 5),
            1428 => (97, 5),
            1429 => (98, 5),
            1430 => (99, 5),
            1431 => (100, 5),
            1432 => (101, 5),
            1433 => (153, 5),
            1434 => (160, 5),
            _ => null,
        };
        if (pair is { } p)
        {
            return new ItemOption(RequiredCatalog.GetItemOptionTemplate(p.optionId), p.param);
        }

        return ItemOptions[0];
    }

    // Lấy get option info; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getOptionInfo).
    public string GetOptionInfo(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);
        bool haveOption = false;
        var optionInfo = new List<string>();
        Item itC = CloneItem();
        ItemOption iodpl = item.GetCrystalOption();
        foreach (ItemOption io in itC.ItemOptions)
        {
            if (!haveOption && io.OptionTemplate.Id == iodpl.OptionTemplate.Id)
            {
                io.Param += iodpl.Param;
                haveOption = true;
            }

            if (io.OptionTemplate.Id != 72 && io.OptionTemplate.Id != 73 && io.OptionTemplate.Id != 102 && io.OptionTemplate.Id != 107)
            {
                optionInfo.Add(io.GetOptionString());
            }
        }

        if (!haveOption)
        {
            optionInfo.Add(iodpl.GetOptionString());
        }

        itC.Dispose();
        return string.Join("\n", optionInfo);
    }

    // Lấy get option info cuong hoa; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getOptionInfoCuongHoa).
    public string GetOptionInfoCuongHoa(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var optionInfo = new List<string>();
        Item itC = CloneItem();
        ItemOption iodpl = item.GetCrystalOption();
        foreach (ItemOption io in itC.ItemOptions)
        {
            if (io.OptionTemplate.Id != 72 && io.OptionTemplate.Id != 73 && io.OptionTemplate.Id != 102 && io.OptionTemplate.Id != 107 && io.OptionTemplate.Id != 218)
            {
                optionInfo.Add(io.GetOptionString());
            }
        }

        optionInfo.Add(iodpl.GetOptionString());
        itC.Dispose();
        return string.Join("\n", optionInfo);
    }

    // Xem trước tùy chọn khi chuyển hóa mà không sửa hai vật phẩm; phép nhân param * percent tràn int theo Java (Java: getOptionInfoChuyenHoa).
    public string GetOptionInfoChuyenHoa(Item item, int level)
    {
        ArgumentNullException.ThrowIfNull(item);
        var optionInfo = new List<string>();
        Item itC = CloneItem();
        int percent = item.GetPercentOption();
        foreach (ItemOption io in itC.ItemOptions)
        {
            if (io.IsOptionCanUpgrade())
            {
                io.Param = ItemUpgradeMath.PointUp(io.Param * percent / 100, level);
            }

            if (io.OptionTemplate.Id != 72 && io.OptionTemplate.Id != 73 && io.OptionTemplate.Id != 102 && io.OptionTemplate.Id != 107 && io.OptionTemplate.Id != 218)
            {
                optionInfo.Add(io.GetOptionString());
            }
        }

        foreach (ItemOption io in item.ItemOptions)
        {
            if (!io.IsOptionCanUpgrade() && io.OptionTemplate.Id != 72 && io.OptionTemplate.Id != 73 && io.OptionTemplate.Id != 102 && io.OptionTemplate.Id != 107 && io.OptionTemplate.Id != 218)
            {
                optionInfo.Add(io.GetOptionString());
            }
        }

        itC.Dispose();
        return string.Join("\n", optionInfo);
    }

    // Lấy get option info; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getOptionInfo).
    public string GetOptionInfo()
    {
        var optionInfo = new List<string>();
        foreach (ItemOption io in ItemOptions)
        {
            if (io.OptionTemplate.Id != 72 && io.OptionTemplate.Id != 73 && io.OptionTemplate.Id != 102 && io.OptionTemplate.Id != 107 && io.OptionTemplate.Id != 218)
            {
                optionInfo.Add(io.GetOptionString());
            }
        }

        return string.Join("\n", optionInfo);
    }

    // Lấy get option info upgrade; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getOptionInfoUpgrade).
    public string GetOptionInfoUpgrade()
    {
        var optionInfo = new List<string>();
        foreach (ItemOption io in ItemOptions)
        {
            if (io.IsOptionCanUpgrade() || io.OptionTemplate.Id == 21 || (io.Param == 30 && io.OptionTemplate.Id != 218))
            {
                optionInfo.Add(io.GetOptionString());
            }
        }

        return string.Join("\n", optionInfo);
    }

    // Kiểm tra have option; giữ nguyên điều kiện, thứ tự và số học của Java (Java: haveOption).
    public bool HaveOption(int idOption)
    {
        if (IsNotNullItem())
        {
            return ItemOptions.Any(op => op != null && op.OptionTemplate.Id == idOption);
        }

        return false;
    }

    // Kiểm tra have set kich hoat; giữ nguyên điều kiện, thứ tự và số học của Java (Java: haveSetKichHoat).
    public bool HasActivationSet()
    {
        if (IsNotNullItem())
        {
            return ItemOptions.Any(op => op != null && ActivationSetIds.Contains(op.OptionTemplate.Id));
        }

        return false;
    }

    // type 21, 11 or 23. (Java: isTrangBiPSH()).
    public bool IsTrangBiPSH() => T.Type == 21 || T.Type == 11 || T.Type == 23;

    // Kiểm tra is trang bi hsd; giữ nguyên điều kiện, thứ tự và số học của Java (Java: isTrangBiHSD).
    public bool IsTrangBiHSD()
    {
        foreach (ItemOption itemOption in ItemOptions)
        {
            if (itemOption.OptionTemplate.Id == 93 && itemOption.Param >= 0)
            {
                return true;
            }
        }

        return false;
    }

    // type 27. (Java: isTrangBiKhoaGd()).
    public bool IsTrangBiKhoaGd() => T.Type == 27;

    // Kiểm tra is trang bi hac hoa; giữ nguyên điều kiện, thứ tự và số học của Java (Java: isTrangBiHacHoa).
    public bool IsTrangBiHacHoa() => T.Type <= 5 || T.Type == 32 || T.Type == 21 || T.Type == 23 || T.Type == 11 || T.Type == 72;

    // Lấy get option info upgrade final; giữ nguyên điều kiện, thứ tự và số học của Java (Java: getOptionInfoUpgradeFinal).
    public string GetOptionInfoUpgradeFinal()
    {
        var optionInfo = new List<string>();
        Item clone = CloneItem();
        foreach (ItemOption io in clone.ItemOptions)
        {
            if (io.IsOptionCanUpgrade())
            {
                io.Param = ItemUpgradeMath.PointUp(io.Param, 1);
            }

            if (io.IsOptionCanUpgrade() || io.Param == 30)
            {
                optionInfo.Add(io.GetOptionString());
            }
        }

        return string.Join("\n", optionInfo);
    }

    // type below 5, or 32. (Java: canPhaLeHoa()).
    public bool CanPhaLeHoa() => Template != null && (Template.Type < 5 || Template.Type == 32);

    // Kiểm tra is do ky gui; giữ nguyên điều kiện, thứ tự và số học của Java (Java: isDoKyGui).
    public bool IsDoKyGui() => Template != null
        && (ItemOptions.Any(op => op.OptionTemplate.Id == 86) || ItemOptions.Any(op => op.OptionTemplate.Id == 87)
            || Template.Type == 14 || Template.Type == 15 || Template.Type == 6 || (Template.Id >= 14 && Template.Id <= 20));

    // type 5, 11, 21 or 23. (Java: isItemCollection()).
    public bool IsItemCollection() => Template != null && (Template.Type == 5 || Template.Type == 11 || Template.Type == 21 || Template.Type == 23);

    // Java isItemTemplateToPet(). (Java: isItemTemplateToPet()).
    public bool IsItemTemplateToPet() => Template != null
        && (Template.Id == 547 || Template.Id == 548 || Template.Id == 932 || Template.Id == 1302 || Template.Id == 1371 || Template.Id == 1380 || Template.Id == 1381);

    // Java isItemTemplateToPlayer(). (Java: isItemTemplateToPlayer()).
    public bool IsItemTemplateToPlayer() => Template != null && Template.Id >= 544 && Template.Id <= 546;

    // Sao chép clone item; giữ nguyên điều kiện, thứ tự và số học của Java (Java: cloneItem).
    public Item CloneItem()
    {
        var item = new Item(Catalog, 0)
        {
            ItemOptions = [],
            Template = Template,
            Info = Info,
            Content = Content,
            Quantity = Quantity,
            CreateTime = CreateTime,
        };
        foreach (ItemOption io in ItemOptions)
        {
            item.ItemOptions.Add(new ItemOption(io));
        }

        return item;
    }

    // Thiết lập set option param; giữ nguyên điều kiện, thứ tự và số học của Java (Java: setOptionParam).
    public void SetOptionParam(int id, int param)
    {
        for (int i = 0; i < ItemOptions.Count; i++)
        {
            ItemOption itemOption = ItemOptions[i];
            if (itemOption != null && itemOption.OptionTemplate.Id == id)
            {
                itemOption.Param = param;
                return;
            }
        }

        ItemOptions.Add(new ItemOption(RequiredCatalog.GetItemOptionTemplate(id), param));
    }
}
