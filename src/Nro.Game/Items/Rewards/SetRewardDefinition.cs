namespace Nro.Game.Items.Rewards;

// Mô tả bằng dữ liệu mười tám phần thưởng bộ Thần Linh và Hủy Diệt thay cho các phương thức Java gần giống nhau.
// Nguồn Java: models/Item/ItemService.java, settlkaio đến sethdnappa (giữ id gói, lựa chọn ngẫu nhiên, tùy chọn, điều kiện số ô trống và nguyên văn thông báo, kể cả lỗi chính tả Java).
public sealed record SetRewardDefinition(
    string JavaName,
    SetRewardKind Kind,
    int PackItemId,
    int[] CandidateIds,
    int SetOptionId,
    int BonusOptionId,
    int EmptySlotsExclusiveMin,
    string Message)
{
    // Thực hiện all; giữ nguyên điều kiện, thứ tự và số học của Java.
    public static IReadOnlyList<SetRewardDefinition> All { get; } =
    [
        new("settlkaio", SetRewardKind.ThanLinh, 1703, [555, 556, 562, 563, 561], 127, 139, 1, "Bạn đã nhận được 1 món Thần Linh "),
        new("settlgenki", SetRewardKind.ThanLinh, 1703, [555, 556, 562, 563, 561], 128, 140, 1, "Bạn đã nhận được món Thần Linh "),
        new("settlson", SetRewardKind.ThanLinh, 1703, [555, 556, 562, 563, 561], 129, 141, 1, "Bạn đã nhận được món Thần Linh "),
        new("settlpico", SetRewardKind.ThanLinh, 1703, [557, 558, 564, 565, 561], 130, 142, 1, "Bạn đã nhận được món Thần Linh "),
        new("settloctieu", SetRewardKind.ThanLinh, 1703, [557, 558, 564, 565, 561], 131, 143, 4, "Bạn đã nhận được món Thần Linh "),
        new("settlpiko", SetRewardKind.ThanLinh, 1703, [557, 558, 564, 565, 561], 132, 144, 1, "Bạn đã nhận được món Thần Linh "),
        new("settlgalick", SetRewardKind.ThanLinh, 1703, [559, 560, 566, 567, 561], 133, 136, 1, "Bạn đã nhận được món thần linh "),
        new("settlcadick", SetRewardKind.ThanLinh, 1703, [559, 560, 566, 567, 561], 134, 137, 1, "Bạn đã nhận được món Thàn linh "),
        new("settlnappa", SetRewardKind.ThanLinh, 1703, [559, 560, 566, 567, 561], 135, 138, 1, "Bạn đã nhận được món thần linh "),
        new("sethdkaio", SetRewardKind.HuyDiet, 1704, [650, 651, 657, 658, 656], 127, 139, 1, "Bạn đã nhận được món Hủy Diệt "),
        new("sethdgenki", SetRewardKind.HuyDiet, 1704, [650, 651, 657, 658, 656], 128, 140, 1, "Bạn đã nhận được món Hủy Diệt "),
        new("sethdson", SetRewardKind.HuyDiet, 1704, [650, 651, 657, 658, 656], 129, 141, 1, "Bạn đã nhận được món Hủy Diệt "),
        new("sethdpico", SetRewardKind.HuyDiet, 1704, [652, 653, 659, 660, 656], 130, 142, 1, "Bạn đã nhận được món Hủy Diệt "),
        new("sethdoctieu", SetRewardKind.HuyDiet, 1704, [652, 653, 659, 660, 656], 131, 143, 1, "Bạn đã nhận được món Hủy Diệt "),
        new("sethdpiko", SetRewardKind.HuyDiet, 1704, [652, 653, 659, 660, 656], 132, 144, 4, "Bạn đã nhận được món Hủy DIệt "),
        new("sethdcadick", SetRewardKind.HuyDiet, 1704, [654, 655, 661, 662, 656], 133, 136, 1, "Bạn đã nhận được món Hủy Diệt "),
        new("sethdcadic", SetRewardKind.HuyDiet, 1704, [654, 655, 661, 662, 656], 134, 137, 1, "Bạn đã nhận được món Hủy Diệt "),
        new("sethdnappa", SetRewardKind.HuyDiet, 1704, [654, 655, 661, 662, 656], 135, 138, 1, "Bạn đã nhận được món Hủy Diệt "),
    ];
}
