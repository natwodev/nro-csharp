namespace Nro.Game.Effects;

// Chứa bản sao chỉ đọc các trường SetClothes mà hiệu ứng cần; các trường Ct* là id template trang phục đang mặc, hoặc -1 khi không có theo mặc định Java.
// Nguồn Java: nro/player/SetClothes.java (DTO phụ trợ; lớp chính do track khác sở hữu).
public sealed record EffectSetClothesState
{
    // Id template bộ Hải Tặc đang mặc, hoặc -1 (Java: ctHaiTac).
    public int CostumePirate { get; init; } = -1;

    // Id template bộ Bunma tóc màu đang mặc, hoặc -1 (Java: ctBunmaTocMau).
    public int CostumeBulmaColoredHair { get; init; } = -1;

    // Id template bộ Tiệc bãi biển đang mặc, hoặc -1 (Java: ctTiecbaiBien).
    public int CostumeBeachParty { get; init; } = -1;

    // Id template bộ Nezuko đang mặc, hoặc -1 (Java: ctNezuko).
    public int CostumeNezuko { get; init; } = -1;

    // Id template bộ Tanjiro đang mặc, hoặc -1 (Java: ctTanjiro).
    public int CostumeTanjiro { get; init; } = -1;

    // Id template bộ Inosuke Hashibira đang mặc, hoặc -1 (Java: ctInoHashi).
    public int CostumeInosukeHashibira { get; init; } = -1;

    // Id template bộ Inosuke đang mặc, hoặc -1 (Java: ctInosuke).
    public int CostumeInosuke { get; init; } = -1;

    // Id template bộ Zenitsu đang mặc, hoặc -1 (Java: ctZenitsu).
    public int CostumeZenitsu { get; init; } = -1;

    // Id template bộ Fide đại ca đang mặc, hoặc -1 (Java: ctFideDaiCa).
    public int CostumeFriezaBoss { get; init; } = -1;

    // Id template bộ đàn em Fide đang mặc, hoặc -1 (Java: ctDanEmFide).
    public int CostumeFriezaMinion { get; init; } = -1;

    // Trạng thái bộ Cadic; 5 nghĩa là đủ bộ, Java dùng byte (Java: cadic).
    public sbyte Cadic { get; init; }
}
