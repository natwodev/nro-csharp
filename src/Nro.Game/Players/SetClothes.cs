using Nro.Game.Items.Core;

namespace Nro.Game.Players;

// Trạng thái các bộ trang bị đang mặc của người chơi.
// Nguồn Java: nro/player/SetClothes.java (dòng 1–358).
public class SetClothes : IDisposable
{
    // Mã option kích hoạt bộ Hủy Diệt. (Java: io.optionTemplate.id == 21)
    private const int DestructionSetOptionId = 21;

    // Giá trị option xác nhận bộ Hủy Diệt. (Java: io.param == 80)
    private const int DestructionSetOptionParameter = 80;

    // Cận dưới template bộ Thần Linh. (Java: item.template.id >= 555)
    private const int DivineSetTemplateMinId = 555;

    // Cận trên template bộ Thần Linh. (Java: item.template.id <= 567)
    private const int DivineSetTemplateMaxId = 567;

    // Cận dưới template bộ Hủy Diệt. (Java: item.template.id >= 650)
    private const int DestructionSetTemplateMinId = 650;

    // Cận trên template bộ Hủy Diệt. (Java: item.template.id <= 662)
    private const int DestructionSetTemplateMaxId = 662;

    // Số ô trang bị thân cần quét. (Java: vòng lặp i < 5)
    private const int EquipmentSlotCount = 5;

    // Vị trí cải trang trong trang bị thân. (Java: itemsBody.get(5))
    private const int CostumeSlot = 5;

    // Người chơi sở hữu các hiệu ứng của bộ trang phục. (Java: player)
    public Player? Player { get; private set; }

    // Số món bộ Songoku đang mặc. (Java: songoku)
    public sbyte Songoku { get; set; }

    // Số món bộ Thiên Xin Hàng đang mặc. (Java: thienXinHang)
    public sbyte ThienXinHang { get; set; }

    // Số món bộ Kirin đang mặc. (Java: kirin)
    public sbyte Kirin { get; set; }

    // Số món bộ Thần Vũ Trụ Kaio đang mặc. (Java: thanVuTruKaio)
    public sbyte ThanVuTruKaio { get; set; }

    // Số món bộ Yamcha đang mặc. (Java: yamcha)
    public sbyte Yamcha { get; set; }

    // Số món bộ Ốc Tiêu đang mặc. (Java: ocTieu)
    public sbyte OcTieu { get; set; }

    // Số món bộ Pikkoro Đại Ma Vương đang mặc. (Java: pikkoroDaimao)
    public sbyte PikkoroDaimao { get; set; }

    // Số món bộ Picolo đang mặc. (Java: picolo)
    public sbyte Picolo { get; set; }

    // Số món bộ Nail đang mặc. (Java: nail)
    public sbyte Nail { get; set; }

    // Số món bộ Slug đang mặc. (Java: slug)
    public sbyte Slug { get; set; }

    // Số món bộ Kakarot đang mặc. (Java: kakarot)
    public sbyte Kakarot { get; set; }

    // Số món bộ Cadic đang mặc. (Java: cadic)
    public sbyte Cadic { get; set; }

    // Số món bộ Nappa đang mặc. (Java: nappa)
    public sbyte Nappa { get; set; }

    // Số món bộ Cadic M đang mặc. (Java: cadicM)
    public sbyte CadicM { get; set; }

    // Số món bộ Broly đang mặc. (Java: broly)
    public sbyte Broly { get; set; }

    // Số món bộ Gohan đang mặc. (Java: gohan)
    public sbyte Gohan { get; set; }

    // Số món bộ Goten đang mặc. (Java: goten)
    public sbyte Goten { get; set; }

    // Số món bộ Frieza đang mặc. (Java: frieza)
    public sbyte Frieza { get; set; }

    // Số món bộ Cumber đang mặc. (Java: cumber)
    public sbyte Cumber { get; set; }

    // Số món bộ Thần Hủy Diệt Champa đang mặc. (Java: thanhuydietchampa)
    public sbyte DestructionGodChampa { get; set; }

    // Số áo World Cup do NPoint đếm và ghi trả. (Java: worldcup)
    public sbyte Worldcup { get; set; }

    // Số option xác nhận bộ Hủy Diệt đang mặc. (Java: setDHD)
    public sbyte DestructionSet { get; set; }

    // Xác nhận năm món đầu đều là template bộ Thần Linh. (Java: godClothes)
    public bool GodClothes { get; set; }

    // Template cải trang Hải Tặc, hoặc -1. (Java: ctHaiTac)
    public int CostumePirate { get; set; } = -1;

    // Template cải trang Bunma tóc màu, hoặc -1. (Java: ctBunmaTocMau)
    public int CostumeBulmaColoredHair { get; set; } = -1;

    // Template cải trang tiệc bãi biển, hoặc -1. (Java: ctTiecbaiBien)
    public int CostumeBeachParty { get; set; } = -1;

    // Template cải trang Nezuko, hoặc -1. (Java: ctNezuko)
    public int CostumeNezuko { get; set; } = -1;

    // Template cải trang Tanjiro, hoặc -1. (Java: ctTanjiro)
    public int CostumeTanjiro { get; set; } = -1;

    // Template cải trang Inosuke Hashibira, hoặc -1. (Java: ctInoHashi)
    public int CostumeInosukeHashibira { get; set; } = -1;

    // Template cải trang Inosuke, hoặc -1. (Java: ctInosuke)
    public int CostumeInosuke { get; set; } = -1;

    // Template cải trang Zenitsu, hoặc -1. (Java: ctZenitsu)
    public int CostumeZenitsu { get; set; } = -1;

    // Template cải trang Fide đại ca, hoặc -1. (Java: ctFideDaiCa)
    public int CostumeFriezaBoss { get; set; } = -1;

    // Template cải trang đàn em Fide, hoặc -1. (Java: ctDanEmFide)
    public int CostumeFriezaMinion { get; set; } = -1;

    // Gắn trạng thái bộ trang phục vào người chơi. (Java: SetClothes)
    public SetClothes(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        Player = player;
    }

    // Quét năm món trang bị thân và cải trang để thiết lập trạng thái. (Java: setup)
    public void Setup()
    {
        SetDefault();
        SetupSKT();
        SetupSKHNew();

        GodClothes = true;
        for (int i = 0; i < EquipmentSlotCount; i++)
        {
            Item item = Owner.Inventory.ItemsBody[i];
            if (!item.IsNotNullItem() || item.Template!.Id > DivineSetTemplateMaxId || item.Template.Id < DivineSetTemplateMinId)
            {
                GodClothes = false;
                break;
            }
        }

        Item costume = Owner.Inventory.ItemsBody[CostumeSlot];
        if (costume.IsNotNullItem())
        {
            SetCostume(costume.Template!.Id);
        }
    }

    // Đọc option bộ kích hoạt trên năm món đầu; mỗi món chỉ tính option bộ đầu tiên. (Java: setupSKT)
    private void SetupSKT()
    {
        for (int i = 0; i < EquipmentSlotCount; i++)
        {
            Item item = Owner.Inventory.ItemsBody[i];
            if (!item.IsNotNullItem())
            {
                break;
            }

            bool isActivationSet = false;
            foreach (ItemOption option in item.ItemOptions)
            {
                switch (option.OptionTemplate.Id)
                {
                    case 129 or 141: isActivationSet = true; Songoku++; break;
                    case 127 or 139: isActivationSet = true; ThienXinHang++; break;
                    case 128 or 140: isActivationSet = true; Kirin++; break;
                    case 250 or 253: isActivationSet = true; Yamcha++; break;
                    case 131 or 254: isActivationSet = true; OcTieu++; break;
                    case 132 or 144: isActivationSet = true; PikkoroDaimao++; break;
                    case 130 or 143: isActivationSet = true; Picolo++; break;
                    case 251 or 142: isActivationSet = true; Slug++; break;
                    case 135 or 138: isActivationSet = true; Nappa++; break;
                    case 133 or 136: isActivationSet = true; Kakarot++; break;
                    case 134 or 137: isActivationSet = true; Cadic++; break;
                    case 252 or 255: isActivationSet = true; Broly++; break;
                    case 233 or 234: isActivationSet = true; Gohan++; break;
                    case 263 or 264: isActivationSet = true; Goten++; break;
                    case 265 or 266: isActivationSet = true; Frieza++; break;
                    case 267 or 268: isActivationSet = true; Cumber++; break;
                    case DestructionSetOptionId when option.Param == DestructionSetOptionParameter: DestructionSet++; break;
                }

                if (isActivationSet)
                {
                    break;
                }
            }
        }
    }

    // Đọc option bộ kích hoạt mới trên năm món đầu; mỗi món chỉ tính option bộ đầu tiên. (Java: setupSKHNew)
    private void SetupSKHNew()
    {
        for (int i = 0; i < EquipmentSlotCount; i++)
        {
            Item item = Owner.Inventory.ItemsBody[i];
            if (!item.IsNotNullItem())
            {
                break;
            }

            bool isActivationSet = false;
            foreach (ItemOption option in item.ItemOptions)
            {
                switch (option.OptionTemplate.Id)
                {
                    case 245 or 246 or 247 or 248: isActivationSet = true; ThanVuTruKaio++; break;
                    case 237 or 238 or 239 or 240: isActivationSet = true; Nail++; break;
                    case 241 or 242 or 243 or 244: isActivationSet = true; CadicM++; break;
                    case 269 or 270 or 271 or 272: isActivationSet = true; DestructionGodChampa++; break;
                }

                if (isActivationSet)
                {
                    break;
                }
            }
        }
    }

    // Khôi phục toàn bộ bộ đếm và trạng thái mặc định trước lần quét mới. (Java: setDefault)
    private void SetDefault()
    {
        Songoku = 0;
        ThienXinHang = 0;
        Kirin = 0;
        Yamcha = 0;
        ThanVuTruKaio = 0;
        OcTieu = 0;
        PikkoroDaimao = 0;
        Picolo = 0;
        Slug = 0;
        Nail = 0;
        Kakarot = 0;
        Cadic = 0;
        Nappa = 0;
        Broly = 0;
        CadicM = 0;
        Gohan = 0;
        Goten = 0;
        Frieza = 0;
        Cumber = 0;
        DestructionGodChampa = 0;
        DestructionSet = 0;
        Worldcup = 0;
        GodClothes = false;
        CostumePirate = -1;
        CostumeInosukeHashibira = -1;
        CostumeInosuke = -1;
        CostumeNezuko = -1;
        CostumeTanjiro = -1;
        CostumeZenitsu = -1;
        CostumeBulmaColoredHair = -1;
        CostumeFriezaBoss = -1;
        CostumeFriezaMinion = -1;
        CostumeBeachParty = -1;
    }

    // Kiểm tra đủ năm món đầu là template thuộc bộ Thần Linh. (Java: checkSetGod)
    public bool CheckSetGod()
    {
        for (int i = 0; i < EquipmentSlotCount; i++)
        {
            Item item = Owner.Inventory.ItemsBody[i];
            if (!item.IsNotNullItem() || item.Template!.Id < DivineSetTemplateMinId || item.Template.Id > DivineSetTemplateMaxId)
            {
                return false;
            }
        }

        return true;
    }

    // Kiểm tra đủ năm món đầu là template thuộc bộ Hủy Diệt. (Java: checkSetDes)
    public bool CheckSetDes()
    {
        for (int i = 0; i < EquipmentSlotCount; i++)
        {
            Item item = Owner.Inventory.ItemsBody[i];
            if (!item.IsNotNullItem() || item.Template!.Id < DestructionSetTemplateMinId || item.Template.Id > DestructionSetTemplateMaxId)
            {
                return false;
            }
        }

        return true;
    }

    // Giải phóng liên kết đến người chơi sở hữu. (Java: dispose)
    public void Dispose()
    {
        Player = null;
    }

    // Trả người chơi còn sống; Java ném NullPointerException sau dispose. (Java: player)
    private Player Owner => Player ?? throw new ObjectDisposedException(nameof(SetClothes));

    // Ánh xạ template cải trang vào trạng thái tương ứng. (Java: switch ct.template.id)
    private void SetCostume(short templateId)
    {
        switch (templateId)
        {
            case 618 or 619 or 620 or 621 or 622 or 623 or 624 or 626 or 627: CostumePirate = templateId; break;
            case 1087: CostumeTanjiro = templateId; break;
            case 1088: CostumeInosukeHashibira = templateId; break;
            case 1089: CostumeInosuke = templateId; break;
            case 1090: CostumeZenitsu = templateId; break;
            case 1091: CostumeNezuko = templateId; break;
            case 1208 or 1209 or 1210: CostumeBulmaColoredHair = templateId; break;
            case 405 or 406 or 407 or 629: CostumeFriezaBoss = templateId; break;
            case 429 or 430 or 431 or 432 or 433: CostumeFriezaMinion = templateId; break;
            case 1234 or 1235 or 1236: CostumeBeachParty = templateId; break;
        }
    }
}
