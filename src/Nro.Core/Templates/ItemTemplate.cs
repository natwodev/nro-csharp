namespace Nro.Core.Templates;

// Mẫu vật phẩm, được nạp từ bảng item_template; byte có dấu của Java được giữ bằng sbyte, tên và mô tả mặc định chuỗi rỗng thay cho null.
// Nguồn Java: nro/template/ItemTemplate.java (giữ 11 cờ sự kiện là thuộc tính protected theo đối tượng, không static/final, với giá trị 1..11).
public class ItemTemplate
{
    // Mã mẫu vật phẩm (Java: id)
    public short Id { get; set; }

    // Kiểu vật phẩm (Java: type)
    public sbyte Type { get; set; }

    // Giới tính áp dụng cho vật phẩm (Java: gender)
    public sbyte Gender { get; set; }

    // Tên vật phẩm, mặc định chuỗi rỗng thay cho null của Java (Java: name)
    public string Name { get; set; } = "";

    // Mô tả vật phẩm, mặc định chuỗi rỗng thay cho null của Java (Java: description)
    public string Description { get; set; } = "";

    // Cấp yêu cầu của vật phẩm (Java: level)
    public sbyte Level { get; set; } = 0;

    // Mã biểu tượng vật phẩm (Java: iconID)
    public short IconID { get; set; }

    // Mã bộ phận hiển thị (Java: part)
    public short Part { get; set; }

    // Cho biết vật phẩm có thể nâng cấp tiếp hay không (Java: isUpToUp)
    public bool IsUpToUp { get; set; }

    // Sức mạnh yêu cầu (Java: strRequire)
    public int StrRequire { get; set; }

    // Giá mua bằng vàng (Java: gold)
    public int Gold { get; set; }

    // Giá bán bằng vàng (Java: goldSell)
    public int GoldSell { get; set; }

    // Giá mua bằng ngọc (Java: gem)
    public int Gem { get; set; }

    // Giá bán bằng ngọc (Java: gemSell)
    public int GemSell { get; set; }

    // Giá mua bằng hồng ngọc (Java: ruby)
    public int Ruby { get; set; }

    // Giá bán bằng hồng ngọc (Java: ruby_sell)
    public int Ruby_sell { get; set; }

    // Mã đầu của vật phẩm (Java: head)
    public int Head { get; set; }

    // Mã thân của vật phẩm (Java: body)
    public int Body { get; set; }

    // Mã chân của vật phẩm (Java: leg)
    public int Leg { get; set; }

    // Kiểu sự kiện của vật phẩm
    public int TypeEvent { get; set; }

    // Cờ sự kiện Tết Âm lịch, giữ là thuộc tính protected theo đối tượng.
    protected sbyte LUNNAR_NEW_YEAR { get; set; } = 1;

    // Cờ sự kiện Giáng sinh, giữ là thuộc tính protected theo đối tượng.
    protected sbyte CHRISTMAS { get; set; } = 2;

    // Cờ sự kiện Vu Lan, giữ là thuộc tính protected theo đối tượng.
    protected sbyte VU_LAN_FESTIVAL { get; set; } = 3;

    // Cờ sự kiện Halloween, giữ là thuộc tính protected theo đối tượng.
    protected sbyte HALLOWEEN { get; set; } = 4;

    // Cờ sự kiện Quốc tế Phụ nữ, giữ là thuộc tính protected theo đối tượng.
    protected sbyte INTERNATIONAL_WOMANS_DAY { get; set; } = 5;

    // Cờ sự kiện Trung Thu, giữ là thuộc tính protected theo đối tượng.
    protected sbyte TRUNG_THU { get; set; } = 6;

    // Cờ sự kiện Hùng Vương, giữ là thuộc tính protected theo đối tượng.
    protected sbyte HUNG_VUONG { get; set; } = 7;

    // Cờ sự kiện Black Friday, giữ là thuộc tính protected theo đối tượng.
    protected sbyte BLACK_FRIDAY { get; set; } = 8;

    // Cờ sự kiện World Cup, giữ là thuộc tính protected theo đối tượng.
    protected sbyte WORLD_CUP { get; set; } = 9;

    // Cờ sự kiện Valentine, giữ là thuộc tính protected theo đối tượng.
    protected sbyte VALENTINE { get; set; } = 10;

    // Cờ sự kiện 20/10, giữ là thuộc tính protected theo đối tượng.
    protected sbyte DAY_20_10 { get; set; } = 11;

    // Giới tính bổ sung áp dụng cho vật phẩm (Java: isGender)
    public sbyte IsGender { get; set; }

    // Tạo mẫu trống để bộ nạp gán từng trường.
    public ItemTemplate()
    {
    }

    // Tạo mẫu từ các trường cơ bản mà hàm tạo Java tiếp nhận.
    public ItemTemplate(short id, sbyte type, sbyte gender, string name, string description, short iconID, short part, bool isUpToUp, int strRequire, int TypeEvent, sbyte isGender)
    {
        Id = id;
        Type = type;
        Gender = gender;
        Name = name;
        Description = description;
        IconID = iconID;
        Part = part;
        IsUpToUp = isUpToUp;
        StrRequire = strRequire;
        this.TypeEvent = TypeEvent;
        IsGender = isGender;
    }
}
