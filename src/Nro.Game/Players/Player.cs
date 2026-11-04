
namespace Nro.Game.Players;

// Lưu vỏ dữ liệu của người chơi; logic update, dispose, bị thương và tấn công còn cho H-01. Danh sách không khóa vì chỉ tick zone chạm vào theo D1.
// Nguồn Java: nro/player/Player.java (tên property viết hoa chữ đầu, byte Java là sbyte, Timestamp/Date là DateTime? UTC; component luôn khác null; Runnable/run/start, timer/task và lichSuGiaoDichDangXem được bỏ có chủ đích).


public class Player
{
    internal const long TIME_TRUNG_THU = 120000;

    internal static long LAST_TIME_UPDATE_LOGIN { get; set; }

    internal static readonly short[][] idOutfitFusion =
    [
        [380, 381, 382], [383, 384, 385], [391, 392, 393],
        [870, 871, 872], [873, 874, 875], [867, 868, 869],
        [1939, 1942, 1943], [1831, 1834, 1835], [1826, 1829, 1830],
        [1765, 1766, 1767], [1771, 1772, 1773], [1768, 1769, 1770],
        [1774, 1775, 1776], [1780, 1781, 1782], [1777, 1778, 1779],
    ];

    internal static readonly short[][] idOutfitCaiTrangFusion =
    [
        [566, 567, 568], [570, 571, 572], [563, 564, 565],
        [630, 631, 632], [633, 634, 635], [627, 628, 629],
    ];

    // Lưu trạng thái idOutfitHalloween (Java: idOutfitHalloween).

    public static readonly short[][][] idOutfitHalloween =
    [
        [[654, 655, 656], [654, 655, 656], [654, 655, 656]],
        [[651, 652, 653], [651, 652, 653], [651, 652, 653]],
        [[545, 548, 549], [547, 548, 549], [546, 548, 549]],
        [[760, 761, 762], [760, 761, 762], [760, 761, 762]],
    ];

    // Lưu trạng thái idOutfitMafuba (Java: idOutfitMafuba).

    public static readonly short[][] idOutfitMafuba = [[1686, 1687, 1688], [-1, -1, -1], [1218, 1219, 1220]];

    // Lưu trạng thái idOutfitGod (Java: idOutfitGod).

    public static readonly short[][] idOutfitGod = [[-1, 472, 473], [-1, 476, 477], [-1, 474, 475]];

    internal static readonly short[][] idOutFitSuperEarth =
    [
        [1436, 1437, 1438],
        [1436, 1437, 1438],
        [1442, 1437, 1438],
        [1440, 1437, 1438],
        [1439, 1437, 1438],
        [1441, 1437, 1438],
    ];

    internal static readonly short[][] idOutFitSuperNamec =
    [
        [1430, 1431, 1432],
        [1443, 1431, 1432],
        [1444, 1431, 1432],
        [1445, 1431, 1432],
        [1446, 1431, 1432],
        [1447, 1431, 1432],
    ];

    internal static readonly short[][] idOutFitSuperSaiyan =
    [
        [1433, 1434, 1435],
        [1433, 1434, 1435],
        [1448, 1434, 1435],
        [1449, 1434, 1435],
        [1450, 1434, 1435],
        [1451, 1434, 1435],
    ];

    internal static readonly sbyte[][] idAuraSuper =
    [
        [20, 21, 22, 23, 24, 25],
        [26, 27, 28, 29, 30, 31],
        [32, 33, 34, 35, 36, 37],
    ];

    // Lưu trạng thái HienThiHopThe (Java: hienThiHopThe).

    public int HienThiHopThe { get; set; } = 1;

    // Lưu trạng thái LastTimeEatPea (Java: lastTimeEatPea).

    public long LastTimeEatPea { get; set; }

    // Lưu trạng thái Session (Java: session).

    public MySession? Session { get; set; }

    // Lưu trạng thái Saving (Java: saving).

    public bool Saving { get; set; }

    // Lưu trạng thái Id (Java: id).

    public long Id { get; set; }

    // Lưu trạng thái Name (Java: name).

    public string Name { get; set; } = "";

    // Lưu trạng thái Gender (Java: gender).

    public sbyte Gender { get; set; }

    // Lưu trạng thái IsNewMember (Java: isNewMember).

    public bool IsNewMember { get; set; }

    // Lưu trạng thái AccountCreatedDays (Java: accountCreatedDays).

    public long AccountCreatedDays { get; set; }

    // Lưu trạng thái Head (Java: head).

    public short Head { get; set; }

    // Lưu trạng thái DeltaTime (Java: deltaTime).

    public int DeltaTime { get; set; }

    // Lưu trạng thái TypePk (Java: typePk).

    public sbyte TypePk { get; set; }

    // Lưu trạng thái CFlag (Java: cFlag).

    public sbyte CFlag { get; set; }

    // Lưu trạng thái HaveTennisSpaceShip (Java: haveTennisSpaceShip).

    public bool HaveTennisSpaceShip { get; set; }
    private long ResetDateOut { get; set; }

    // Lưu trạng thái Resetdame (Java: resetdame).

    public bool Resetdame { get; set; }

    // Lưu trạng thái LastTimeDame (Java: lastTimeDame).

    public long LastTimeDame { get; set; }

    // Lưu trạng thái Dametong (Java: dametong).

    public long Dametong { get; set; }

    // Lưu trạng thái LastTimeDameMayDam (Java: lastTimeDameMayDam).

    public long LastTimeDameMayDam { get; set; }

    // Lưu trạng thái TotalDameMayDam (Java: totalDameMayDam).

    public long TotalDameMayDam { get; set; }

    // Lưu trạng thái Lastimelogin (Java: lastimelogin).

    public DateTime? Lastimelogin { get; set; }

    // Lưu trạng thái FirstTimeLogin (Java: firstTimeLogin).

    public DateTime? FirstTimeLogin { get; set; }

    // Lưu trạng thái LastTimeLoginGame (Java: lastTimeLoginGame).

    public DateTime? LastTimeLoginGame { get; set; }

    // Lưu trạng thái LastTimeAttack (Java: lastTimeAttack).

    public long LastTimeAttack { get; set; }

    // Lưu trạng thái TlDameAdd (Java: tlDameAdd).

    public int TlDameAdd { get; set; }

    // Lưu trạng thái TlDameClanAdd (Java: tlDameClanAdd).

    public int TlDameClanAdd { get; set; }

    // Lưu trạng thái TlHpClanAdd (Java: tlHpClanAdd).

    public int TlHpClanAdd { get; set; }

    // Lưu trạng thái TlMpClanAdd (Java: tlMpClanAdd).

    public int TlMpClanAdd { get; set; }

    // Lưu trạng thái CanGeFirstTimeLogin (Java: canGeFirstTimeLogin).

    public bool CanGeFirstTimeLogin { get; set; }

    // Lưu trạng thái TempItemIndex (Java: tempItemIndex).

    public int TempItemIndex { get; set; } = -1;

    // Lưu trạng thái ItemThrow_Drop (Java: itemThrow_Drop).

    public Item? ItemThrow_Drop { get; set; }

    // Lưu trạng thái Item_ChangePet (Java: item_ChangePet).

    public Item? Item_ChangePet { get; set; }

    // Lưu trạng thái TrangThaiLichSuGd (Java: trangThaiLichSuGd).

    public int TrangThaiLichSuGd { get; set; }

    // Lưu trạng thái TopMayDamTraiDat (Java: topMayDamTraiDat).

    public long TopMayDamTraiDat { get; set; }

    // Lưu trạng thái TopMayDamNamec (Java: topMayDamNamec).

    public long TopMayDamNamec { get; set; }

    // Lưu trạng thái TopMayDamXayda (Java: topMayDamXayda).

    public long TopMayDamXayda { get; set; }

    // Lưu trạng thái NumUseSkill (Java: numUseSkill).

    public int NumUseSkill { get; set; }

    // Lưu trạng thái LastTimeHoiskill (Java: lastTimeHoiskill).

    public long LastTimeHoiskill { get; set; }

    // Lưu trạng thái DiemDanhBangHoi (Java: diemDanhBangHoi).

    public int DiemDanhBangHoi { get; set; }

    // Lưu trạng thái VeTangNgoc_SoLuongNgoc (Java: veTangNgoc_SoLuongNgoc).

    public int VeTangNgoc_SoLuongNgoc { get; set; }

    // Lưu trạng thái Player_NhanNgoc (Java: player_NhanNgoc).

    public Player? Player_NhanNgoc { get; set; }

    // Lưu trạng thái VeTangHongNgoc_SoLuongHongNgoc (Java: veTangHongNgoc_SoLuongHongNgoc).

    public int VeTangHongNgoc_SoLuongHongNgoc { get; set; }

    // Lưu trạng thái Player_NhanHongNgoc (Java: player_NhanHongNgoc).

    public Player? Player_NhanHongNgoc { get; set; }

    // Lưu trạng thái Cuoc (Java: cuoc).

    public int Cuoc { get; set; }

    // Lưu trạng thái RubyWin (Java: rubyWin).

    public int RubyWin { get; set; }

    // Lưu trạng thái NameClan (Java: nameClan).

    public string? NameClan { get; set; }

    // Lưu trạng thái LastTimeDropTail (Java: lastTimeDropTail).

    public long LastTimeDropTail { get; set; }

    // Lưu trạng thái LastTimeChangeBadges (Java: lastTimeChangeBadges).

    public long LastTimeChangeBadges { get; set; }

    // Lưu trạng thái MapTransitionTime (Java: mapTransitionTime).

    public long MapTransitionTime { get; set; }
    private int WrongPasswordAttempts { get; set; }

    // Lưu trạng thái RestartNauBanh (Java: restartNauBanh).

    public sbyte RestartNauBanh { get; set; }

    // Lưu trạng thái Khisukien (Java: khisukien).

    public bool Khisukien { get; set; }

    // Lưu trạng thái LastTimeCallKhi (Java: lastTimeCallKhi).

    public long LastTimeCallKhi { get; set; }

    // Lưu trạng thái Lasttimekhisukien (Java: lasttimekhisukien).

    public long Lasttimekhisukien { get; set; }
    private long LastTimeBom { get; set; }

    // Lưu trạng thái PartDanhHieu (Java: partDanhHieu).

    public int PartDanhHieu { get; set; }

    // Lưu trạng thái Titleitem (Java: titleitem).

    public bool Titleitem { get; set; }

    // Lưu trạng thái PointRank (Java: pointRank).

    public sbyte PointRank { get; set; }

    // Lưu trạng thái TypeTabPet (Java: typeTabPet).

    public sbyte TypeTabPet { get; set; }

    // Lưu trạng thái TypeBox (Java: typeBox).

    public sbyte TypeBox { get; set; }

    // Lưu trạng thái TypeVip (Java: typeVip).

    public int TypeVip { get; set; } = -1;

    // Lưu trạng thái IsPlayer (Java: isPlayer).

    public bool IsPlayer { get; set; }

    // Lưu trạng thái IsDeTu (Java: isDeTu).

    public bool IsDeTu { get; set; }

    // Lưu trạng thái IsBo (Java: isBo).

    public bool IsBo { get; set; }

    // Lưu trạng thái IsMe (Java: isMe).

    public bool IsMe { get; set; }

    // Lưu trạng thái IsPetFollow (Java: isPetFollow).

    public bool IsPetFollow { get; set; }

    // Lưu trạng thái IsDuongTang (Java: isDuongTang).

    public bool IsDuongTang { get; set; }

    // Lưu trạng thái IsBoss (Java: isBoss).

    public bool IsBoss { get; set; }

    // Lưu trạng thái IsNguoiYeu (Java: isNguoiYeu).

    public bool IsNguoiYeu { get; set; }

    // Lưu trạng thái IsConOne (Java: isConOne).

    public bool IsConOne { get; set; }

    // Lưu trạng thái IsConTwo (Java: isConTwo).

    public bool IsConTwo { get; set; }

    // Lưu trạng thái IsConThree (Java: isConThree).

    public bool IsConThree { get; set; }

    // Lưu trạng thái IsPhanThan (Java: isPhanThan).

    public bool IsPhanThan { get; set; }

    // Lưu trạng thái NhapNgocRongTimes (Java: nhapNgocRongTimes).

    public int NhapNgocRongTimes { get; set; } = 1;

    // Lưu trạng thái NhanKeoHayBiGheoNpc_1 (Java: nhanKeoHayBiGheoNpc_1).

    public int NhanKeoHayBiGheoNpc_1 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_2 (Java: nhanKeoHayBiGheoNpc_2).

    public int NhanKeoHayBiGheoNpc_2 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_3 (Java: nhanKeoHayBiGheoNpc_3).

    public int NhanKeoHayBiGheoNpc_3 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_4 (Java: nhanKeoHayBiGheoNpc_4).

    public int NhanKeoHayBiGheoNpc_4 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_5 (Java: nhanKeoHayBiGheoNpc_5).

    public int NhanKeoHayBiGheoNpc_5 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_6 (Java: nhanKeoHayBiGheoNpc_6).

    public int NhanKeoHayBiGheoNpc_6 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_7 (Java: nhanKeoHayBiGheoNpc_7).

    public int NhanKeoHayBiGheoNpc_7 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_8 (Java: nhanKeoHayBiGheoNpc_8).

    public int NhanKeoHayBiGheoNpc_8 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_9 (Java: nhanKeoHayBiGheoNpc_9).

    public int NhanKeoHayBiGheoNpc_9 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_10 (Java: nhanKeoHayBiGheoNpc_10).

    public int NhanKeoHayBiGheoNpc_10 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_11 (Java: nhanKeoHayBiGheoNpc_11).

    public int NhanKeoHayBiGheoNpc_11 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_12 (Java: nhanKeoHayBiGheoNpc_12).

    public int NhanKeoHayBiGheoNpc_12 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_13 (Java: nhanKeoHayBiGheoNpc_13).

    public int NhanKeoHayBiGheoNpc_13 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_14 (Java: nhanKeoHayBiGheoNpc_14).

    public int NhanKeoHayBiGheoNpc_14 { get; set; }

    // Lưu trạng thái NhanKeoHayBiGheoNpc_15 (Java: nhanKeoHayBiGheoNpc_15).

    public int NhanKeoHayBiGheoNpc_15 { get; set; }

    // Lưu trạng thái CheckTrongNgay (Java: checkTrongNgay).

    public long CheckTrongNgay { get; set; }

    // Lưu trạng thái DuaTopTangLiXi (Java: duaTopTangLiXi).

    public int DuaTopTangLiXi { get; set; }

    // Lưu trạng thái DuaTopMoLiXi (Java: duaTopMoLiXi).

    public int DuaTopMoLiXi { get; set; }

    // Lưu trạng thái DuaTopBanPhaoHoa (Java: duaTopBanPhaoHoa).

    public int DuaTopBanPhaoHoa { get; set; }

    // Lưu trạng thái DuaTopBanPhaoHoaVIP (Java: duaTopBanPhaoHoaVIP).

    public int DuaTopBanPhaoHoaVIP { get; set; }

    // Lưu trạng thái DuaTopTrangTriCayNoel (Java: duaTopTrangTriCayNoel).

    public int DuaTopTrangTriCayNoel { get; set; }

    // Lưu trạng thái DuaTopCheTaoNguoiTuyet (Java: duaTopCheTaoNguoiTuyet).

    public int DuaTopCheTaoNguoiTuyet { get; set; }

    // Lưu trạng thái DuaTopCheTaoNguoiTuyetBangGia (Java: duaTopCheTaoNguoiTuyetBangGia).

    public int DuaTopCheTaoNguoiTuyetBangGia { get; set; }

    // Lưu trạng thái DuaTopDotDiem (Java: duaTopDotDiem).

    public int DuaTopDotDiem { get; set; }

    // Lưu trạng thái DuaTopPhaoHoaVuLan (Java: duaTopPhaoHoaVuLan).

    public int DuaTopPhaoHoaVuLan { get; set; }

    // Lưu trạng thái DuaTopHoaDang (Java: duaTopHoaDang).

    public int DuaTopHoaDang { get; set; }

    // Lưu trạng thái DuaTopHoaDangCoLoiChuc (Java: duaTopHoaDangCoLoiChuc).

    public int DuaTopHoaDangCoLoiChuc { get; set; }

    // Lưu trạng thái DuaTopMoHopMaQuy (Java: duaTopMoHopMaQuy).

    public int DuaTopMoHopMaQuy { get; set; }

    // Lưu trạng thái DuaTopThiepHalloween (Java: duaTopThiepHalloween).

    public int DuaTopThiepHalloween { get; set; }

    // Lưu trạng thái DuaTopMoThiep83 (Java: duaTopMoThiep83).

    public int DuaTopMoThiep83 { get; set; }

    // Lưu trạng thái DuaTopTangBongHoaHong (Java: duaTopTangBongHoaHong).

    public int DuaTopTangBongHoaHong { get; set; }

    // Lưu trạng thái DuaTopLamBanhTrungThu (Java: duaTopLamBanhTrungThu).

    public int DuaTopLamBanhTrungThu { get; set; }

    // Lưu trạng thái DuaTopMoHopTrungThuDacBiet (Java: duaTopMoHopTrungThuDacBiet).

    public int DuaTopMoHopTrungThuDacBiet { get; set; }

    // Lưu trạng thái UocMienPhi (Java: uocMienPhi).

    public int UocMienPhi { get; set; }

    // Lưu trạng thái NhanQuaHungVuongFree (Java: nhanQuaHungVuongFree).

    public int NhanQuaHungVuongFree { get; set; }

    // Lưu trạng thái LastTimeNauBanhHungVuong (Java: lastTimeNauBanhHungVuong).

    public long LastTimeNauBanhHungVuong { get; set; }

    // Lưu trạng thái TypeBanhDangNau (Java: typeBanhDangNau).

    public int TypeBanhDangNau { get; set; }

    // Lưu trạng thái DuaTopMoHopQuaGioTo (Java: duaTopMoHopQuaGioTo).

    public int DuaTopMoHopQuaGioTo { get; set; }

    // Lưu trạng thái DuaTopDangBanhHungVuong (Java: duaTopDangBanhHungVuong).

    public int DuaTopDangBanhHungVuong { get; set; }

    // Lưu trạng thái DuaTopMoTrungRongVang (Java: duaTopMoTrungRongVang).

    public int DuaTopMoTrungRongVang { get; set; }

    // Lưu trạng thái DuaTopDoiDuaHau (Java: duaTopDoiDuaHau).

    public int DuaTopDoiDuaHau { get; set; }

    // Lưu trạng thái DuaTopMoHopBlackFriday (Java: duaTopMoHopBlackFriday).

    public int DuaTopMoHopBlackFriday { get; set; }

    // Lưu trạng thái DuaTopMuaSamBlackFriday (Java: duaTopMuaSamBlackFriday).

    public int DuaTopMuaSamBlackFriday { get; set; }

    // Lưu trạng thái DuaTopMoHop20_10 (Java: duaTopMoHop20_10).

    public int DuaTopMoHop20_10 { get; set; }

    // Lưu trạng thái DuaTopMoThiepChuc (Java: duaTopMoThiepChuc).

    public int DuaTopMoThiepChuc { get; set; }

    // Lưu trạng thái DuaTopTangThiepChuc (Java: duaTopTangThiepChuc).

    public int DuaTopTangThiepChuc { get; set; }

    // Lưu trạng thái HoTongDuongTang (Java: hoTongDuongTang).

    public bool HoTongDuongTang { get; set; }

    // Lưu trạng thái MapHoTong (Java: mapHoTong).

    public int MapHoTong { get; set; }

    // Lưu trạng thái LastTimeDuongTang (Java: lastTimeDuongTang).

    public long LastTimeDuongTang { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_1 (Java: nhanLiXiForNPC_1).

    public int NhanLiXiForNPC_1 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_2 (Java: nhanLiXiForNPC_2).

    public int NhanLiXiForNPC_2 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_3 (Java: nhanLiXiForNPC_3).

    public int NhanLiXiForNPC_3 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_4 (Java: nhanLiXiForNPC_4).

    public int NhanLiXiForNPC_4 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_5 (Java: nhanLiXiForNPC_5).

    public int NhanLiXiForNPC_5 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_6 (Java: nhanLiXiForNPC_6).

    public int NhanLiXiForNPC_6 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_7 (Java: nhanLiXiForNPC_7).

    public int NhanLiXiForNPC_7 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_8 (Java: nhanLiXiForNPC_8).

    public int NhanLiXiForNPC_8 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_9 (Java: nhanLiXiForNPC_9).

    public int NhanLiXiForNPC_9 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_10 (Java: nhanLiXiForNPC_10).

    public int NhanLiXiForNPC_10 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_11 (Java: nhanLiXiForNPC_11).

    public int NhanLiXiForNPC_11 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_12 (Java: nhanLiXiForNPC_12).

    public int NhanLiXiForNPC_12 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_13 (Java: nhanLiXiForNPC_13).

    public int NhanLiXiForNPC_13 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_14 (Java: nhanLiXiForNPC_14).

    public int NhanLiXiForNPC_14 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_15 (Java: nhanLiXiForNPC_15).

    public int NhanLiXiForNPC_15 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_16 (Java: nhanLiXiForNPC_16).

    public int NhanLiXiForNPC_16 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_17 (Java: nhanLiXiForNPC_17).

    public int NhanLiXiForNPC_17 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_18 (Java: nhanLiXiForNPC_18).

    public int NhanLiXiForNPC_18 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_19 (Java: nhanLiXiForNPC_19).

    public int NhanLiXiForNPC_19 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_20 (Java: nhanLiXiForNPC_20).

    public int NhanLiXiForNPC_20 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_21 (Java: nhanLiXiForNPC_21).

    public int NhanLiXiForNPC_21 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_22 (Java: nhanLiXiForNPC_22).

    public int NhanLiXiForNPC_22 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_23 (Java: nhanLiXiForNPC_23).

    public int NhanLiXiForNPC_23 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_24 (Java: nhanLiXiForNPC_24).

    public int NhanLiXiForNPC_24 { get; set; }

    // Lưu trạng thái NhanLiXiForNPC_25 (Java: nhanLiXiForNPC_25).

    public int NhanLiXiForNPC_25 { get; set; }

    // Lưu trạng thái IsHoldNamecBallTranhDoat (Java: isHoldNamecBallTranhDoat).

    public bool IsHoldNamecBallTranhDoat { get; set; }

    // Lưu trạng thái TempIdNamecBallHoldTranhDoat (Java: tempIdNamecBallHoldTranhDoat).

    public int TempIdNamecBallHoldTranhDoat { get; set; } = -1;

    // Lưu trạng thái LastTimePickItem (Java: lastTimePickItem).

    public long LastTimePickItem { get; set; }

    // Lưu trạng thái LastTimeUpdateBallWar (Java: lastTimeUpdateBallWar).

    public long LastTimeUpdateBallWar { get; set; }

    // Lưu trạng thái TotalDamageTaken (Java: totalDamageTaken).

    public long TotalDamageTaken { get; set; }

    // Lưu trạng thái ThongBaoChangeMap (Java: thongBaoChangeMap).

    public bool ThongBaoChangeMap { get; set; }

    // Lưu trạng thái TextThongBaoChangeMap (Java: textThongBaoChangeMap).

    public string? TextThongBaoChangeMap { get; set; }

    // Lưu trạng thái ThongBaoThua (Java: thongBaoThua).

    public bool ThongBaoThua { get; set; }

    // Lưu trạng thái TextThongBaoThua (Java: textThongBaoThua).

    public string? TextThongBaoThua { get; set; }

    // Lưu trạng thái LevelWoodChest (Java: levelWoodChest).

    public int LevelWoodChest { get; set; }

    // Lưu trạng thái GoldChallenge (Java: goldChallenge).

    public long GoldChallenge { get; set; }

    // Lưu trạng thái RubyChallenge (Java: rubyChallenge).

    public long RubyChallenge { get; set; }

    // Lưu trạng thái LastTimeRewardWoodChest (Java: lastTimeRewardWoodChest).

    public long LastTimeRewardWoodChest { get; set; }

    // Lưu trạng thái ItemsWoodChest (Java: itemsWoodChest).

    public List<Item> ItemsWoodChest { get; set; } = [];

    // Lưu trạng thái IndexWoodChest (Java: indexWoodChest).

    public int IndexWoodChest { get; set; }

    // Lưu trạng thái LastTimePKDHVT23 (Java: lastTimePKDHVT23).

    public long LastTimePKDHVT23 { get; set; }

    // Lưu trạng thái LostByDeath (Java: lostByDeath).

    public bool LostByDeath { get; set; }

    // Lưu trạng thái IsPKDHVT (Java: isPKDHVT).

    public bool IsPKDHVT { get; set; }

    // Lưu trạng thái LastTimePKVoDaiSinhTu (Java: lastTimePKVoDaiSinhTu).

    public long LastTimePKVoDaiSinhTu { get; set; }

    // Lưu trạng thái HaveRewardVDST (Java: haveRewardVDST).

    public bool HaveRewardVDST { get; set; }

    // Lưu trạng thái ThoiVangVoDaiSinhTu (Java: thoiVangVoDaiSinhTu).

    public int ThoiVangVoDaiSinhTu { get; set; }

    // Lưu trạng thái TimePKVDST (Java: timePKVDST).

    public long TimePKVDST { get; set; }

    // Lưu trạng thái BinhChonHatMit (Java: binhChonHatMit).

    public int BinhChonHatMit { get; set; }

    // Lưu trạng thái BinhChonPlayer (Java: binhChonPlayer).

    public int BinhChonPlayer { get; set; }

    // Lưu trạng thái ZoneBinhChon (Java: zoneBinhChon).

    public Zone? ZoneBinhChon { get; set; }

    // Lưu trạng thái Mbv (Java: mbv).

    public int Mbv { get; set; }

    // Lưu trạng thái Baovetaikhoan (Java: baovetaikhoan).

    public bool Baovetaikhoan { get; set; }

    // Lưu trạng thái Mbvtime (Java: mbvtime).

    public long Mbvtime { get; set; }

    // Lưu trạng thái Captcha (Java: captcha).

    public string Captcha { get; set; } = "";

    // Lưu trạng thái TimeGohome (Java: timeGohome).

    public int TimeGohome { get; set; }

    // Lưu trạng thái LastUpdateGohomeTime (Java: lastUpdateGohomeTime).

    public long LastUpdateGohomeTime { get; set; }

    // Lưu trạng thái GoHome (Java: goHome).

    public bool GoHome { get; set; }

    // Lưu trạng thái MaBu2H (Java: maBu2H).

    public MajinBuu14H? MaBu2H { get; set; }

    // Lưu trạng thái IsMabuHold (Java: isMabuHold).

    public bool IsMabuHold { get; set; }

    // Lưu trạng thái MaBuHold (Java: maBuHold).

    public MaBuHold? MaBuHold { get; set; }

    // Lưu trạng thái PrecentMabuHold (Java: precentMabuHold).

    public int PrecentMabuHold { get; set; }

    // Lưu trạng thái IsPhuHoMapMabu (Java: isPhuHoMapMabu).

    public bool IsPhuHoMapMabu { get; set; }

    // Lưu trạng thái IsCopy (Java: isCopy).

    public bool IsCopy { get; set; }

    // Lưu trạng thái LastPkCommesonTime (Java: lastPkCommesonTime).

    public long LastPkCommesonTime { get; set; }

    // Lưu trạng thái TimesPerDayCuuSat (Java: timesPerDayCuuSat).

    public int TimesPerDayCuuSat { get; set; }

    // Lưu trạng thái LastTimeCuuSat (Java: lastTimeCuuSat).

    public long LastTimeCuuSat { get; set; }

    // Lưu trạng thái NhanVangNangVIP (Java: nhanVangNangVIP).

    public bool NhanVangNangVIP { get; set; }

    // Lưu trạng thái NhanDeTuNangVIP (Java: nhanDeTuNangVIP).

    public bool NhanDeTuNangVIP { get; set; }

    // Lưu trạng thái NhanSKHVIP (Java: nhanSKHVIP).

    public bool NhanSKHVIP { get; set; }

    // Lưu trạng thái CanReward_TuanLoc (Java: canReward_TuanLoc).

    public bool CanReward_TuanLoc { get; set; }

    // Lưu trạng thái ChangeMapVIP_TuanLoc (Java: changeMapVIP_TuanLoc).

    public bool ChangeMapVIP_TuanLoc { get; set; }

    // Lưu trạng thái HaveReward_TuanLoc (Java: haveReward_TuanLoc).

    public bool HaveReward_TuanLoc { get; set; }

    // Lưu trạng thái CanReward (Java: canReward).

    public bool CanReward { get; set; }

    // Lưu trạng thái ChangeMapVIP (Java: changeMapVIP).

    public bool ChangeMapVIP { get; set; }

    // Lưu trạng thái HaveReward (Java: haveReward).

    public bool HaveReward { get; set; }

    // Lưu trạng thái CanReward_MeoDen (Java: canReward_MeoDen).

    public bool CanReward_MeoDen { get; set; }

    // Lưu trạng thái ChangeMapVIP_MeoDen (Java: changeMapVIP_MeoDen).

    public bool ChangeMapVIP_MeoDen { get; set; }

    // Lưu trạng thái HaveReward_MeoDen (Java: haveReward_MeoDen).

    public bool HaveReward_MeoDen { get; set; }

    // Lưu trạng thái CanReward_PiLong (Java: canReward_PiLong).

    public bool CanReward_PiLong { get; set; }

    // Lưu trạng thái ChangeMapVIP_PiLong (Java: changeMapVIP_PiLong).

    public bool ChangeMapVIP_PiLong { get; set; }

    // Lưu trạng thái HaveReward_PiLong (Java: haveReward_PiLong).

    public bool HaveReward_PiLong { get; set; }

    // Lưu trạng thái CanReward_PeNa (Java: canReward_PeNa).

    public bool CanReward_PeNa { get; set; }

    // Lưu trạng thái ChangeMapVIP_PeNa (Java: changeMapVIP_PeNa).

    public bool ChangeMapVIP_PeNa { get; set; }

    // Lưu trạng thái HaveReward_PeNa (Java: haveReward_PeNa).

    public bool HaveReward_PeNa { get; set; }

    // Lưu trạng thái LastTimeJoinDT (Java: lastTimeJoinDT).

    public long LastTimeJoinDT { get; set; }

    // Lưu trạng thái JoinCDRD (Java: joinCDRD).

    public bool JoinCDRD { get; set; }

    // Lưu trạng thái LastTimeJoinCDRD (Java: lastTimeJoinCDRD).

    public long LastTimeJoinCDRD { get; set; }

    // Lưu trạng thái TalkToThuongDe (Java: talkToThuongDe).

    public bool TalkToThuongDe { get; set; }

    // Lưu trạng thái TalkToThanMeo (Java: talkToThanMeo).

    public bool TalkToThanMeo { get; set; }

    // Lưu trạng thái TimeChangeMap144 (Java: timeChangeMap144).

    public long TimeChangeMap144 { get; set; }

    // Lưu trạng thái LevelCDRDDone (Java: levelCDRDDone).

    public int LevelCDRDDone { get; set; }

    // Lưu trạng thái TimeCDRDDone (Java: timeCDRDDone).

    public long TimeCDRDDone { get; set; }

    // Lưu trạng thái LastTimeUpdateTopCDRD (Java: lastTimeUpdateTopCDRD).

    public long LastTimeUpdateTopCDRD { get; set; }

    // Lưu trạng thái LevelKhiGasDone (Java: levelKhiGasDone).

    public int LevelKhiGasDone { get; set; }

    // Lưu trạng thái TimeKhiGasDone (Java: timeKhiGasDone).

    public long TimeKhiGasDone { get; set; }

    // Lưu trạng thái LastTimeUpdateTopKhiGas (Java: lastTimeUpdateTopKhiGas).

    public long LastTimeUpdateTopKhiGas { get; set; }

    // Lưu trạng thái TimesPerDayBDKB (Java: timesPerDayBDKB).

    public int TimesPerDayBDKB { get; set; }

    // Lưu trạng thái LastTimeJoinBDKB (Java: lastTimeJoinBDKB).

    public long LastTimeJoinBDKB { get; set; }

    // Lưu trạng thái Bdkb_countPerDay (Java: bdkb_countPerDay).

    public int Bdkb_countPerDay { get; set; }

    // Lưu trạng thái Bdkb_lastTimeJoin (Java: bdkb_lastTimeJoin).

    public long Bdkb_lastTimeJoin { get; set; }

    // Lưu trạng thái Bdkb_isJoinBdkb (Java: bdkb_isJoinBdkb).

    public bool Bdkb_isJoinBdkb { get; set; }

    // Lưu trạng thái LevelBDKBDone (Java: levelBDKBDone).

    public int LevelBDKBDone { get; set; }

    // Lưu trạng thái TimeBDKBDone (Java: timeBDKBDone).

    public long TimeBDKBDone { get; set; }

    // Lưu trạng thái LastTimeUpdateTopBDKB (Java: lastTimeUpdateTopBDKB).

    public long LastTimeUpdateTopBDKB { get; set; }

    // Lưu trạng thái CallBossPocolo (Java: callBossPocolo).

    public bool CallBossPocolo { get; set; }

    // Lưu trạng thái ZoneSieuThanhThuy (Java: zoneSieuThanhThuy).

    public Zone? ZoneSieuThanhThuy { get; set; }

    // Lưu trạng thái WinSTT (Java: winSTT).

    public bool WinSTT { get; set; }

    // Lưu trạng thái LastTimeWinSTT (Java: lastTimeWinSTT).

    public long LastTimeWinSTT { get; set; }

    // Lưu trạng thái LastTimeUpdateSTT (Java: lastTimeUpdateSTT).

    public long LastTimeUpdateSTT { get; set; }

    // Lưu trạng thái Notify (Java: notify).

    public string? Notify { get; set; }

    // Lưu trạng thái IsOffline (Java: isOffline).

    public bool IsOffline { get; set; }

    // Lưu trạng thái LevelLuyenTap (Java: levelLuyenTap).

    public int LevelLuyenTap { get; set; }

    // Lưu trạng thái IsThachDau (Java: isThachDau).

    public bool IsThachDau { get; set; }

    // Lưu trạng thái TnsmLuyenTap (Java: tnsmLuyenTap).

    public int TnsmLuyenTap { get; set; }

    // Lưu trạng thái DangKyTapTuDong (Java: dangKyTapTuDong).

    public bool DangKyTapTuDong { get; set; }

    // Lưu trạng thái LastTimeOffline (Java: lastTimeOffline).

    public long LastTimeOffline { get; set; }

    // Lưu trạng thái MapIdDangTapTuDong (Java: mapIdDangTapTuDong).

    public int MapIdDangTapTuDong { get; set; }

    // Lưu trạng thái LastMapOffline (Java: lastMapOffline).

    public int LastMapOffline { get; set; }

    // Lưu trạng thái LastZoneOffline (Java: lastZoneOffline).

    public int LastZoneOffline { get; set; }

    // Lưu trạng thái LastXOffline (Java: lastXOffline).

    public int LastXOffline { get; set; }

    // Lưu trạng thái ThongBaoTapTuDong (Java: thongBaoTapTuDong).

    public string? ThongBaoTapTuDong { get; set; }

    // Lưu trạng thái TeleTapTuDong (Java: teleTapTuDong).

    public bool TeleTapTuDong { get; set; }

    // Lưu trạng thái Traning (Java: traning).

    public Traning Traning { get; set; } = null!;

    // Lưu trạng thái DoesNotAttack (Java: doesNotAttack).

    public bool DoesNotAttack { get; set; }

    // Lưu trạng thái LastTimePlayerNotAttack (Java: lastTimePlayerNotAttack).

    public long LastTimePlayerNotAttack { get; set; }

    // Lưu trạng thái TimeNotAttack (Java: timeNotAttack).

    public int TimeNotAttack { get; set; } = 1800000;

    // Lưu trạng thái TemporaryEnemies (Java: temporaryEnemies).

    public List<Player> TemporaryEnemies { get; set; } = [];

    // Lưu trạng thái JustRevived (Java: justRevived).

    public bool JustRevived { get; set; }

    // Lưu trạng thái LastTimeRevived (Java: lastTimeRevived).

    public long LastTimeRevived { get; set; }


    // Lưu trạng thái DoesNotAttackBo (Java: doesNotAttackBo).

    public bool DoesNotAttackBo { get; set; }

    // Lưu trạng thái LastTimePlayerNotAttackBo (Java: lastTimePlayerNotAttackBo).

    public long LastTimePlayerNotAttackBo { get; set; }

    // Lưu trạng thái TimeNotAttackBo (Java: timeNotAttackBo).

    public int TimeNotAttackBo { get; set; } = 1800000;

    // Lưu trạng thái TemporaryEnemiesBo (Java: temporaryEnemiesBo).

    public List<Player> TemporaryEnemiesBo { get; set; } = [];

    // Lưu trạng thái JustRevivedBo (Java: justRevivedBo).

    public bool JustRevivedBo { get; set; }

    // Lưu trạng thái LastTimeRevivedBo (Java: lastTimeRevivedBo).

    public long LastTimeRevivedBo { get; set; }

    // Lưu trạng thái NameFather (Java: nameFather).

    public string? NameFather { get; set; }

    // Lưu trạng thái DoesNotAttackMe (Java: doesNotAttackMe).

    public bool DoesNotAttackMe { get; set; }

    // Lưu trạng thái LastTimePlayerNotAttackMe (Java: lastTimePlayerNotAttackMe).

    public long LastTimePlayerNotAttackMe { get; set; }

    // Lưu trạng thái TimeNotAttackMe (Java: timeNotAttackMe).

    public int TimeNotAttackMe { get; set; } = 1800000;

    // Lưu trạng thái TemporaryEnemiesMe (Java: temporaryEnemiesMe).

    public List<Player> TemporaryEnemiesMe { get; set; } = [];

    // Lưu trạng thái JustRevivedMe (Java: justRevivedMe).

    public bool JustRevivedMe { get; set; }

    // Lưu trạng thái LastTimeRevivedMe (Java: lastTimeRevivedMe).

    public long LastTimeRevivedMe { get; set; }

    // Lưu trạng thái NameMother (Java: nameMother).

    public string? NameMother { get; set; }

    // Lưu trạng thái DoesNotAttackNguoiYeu (Java: doesNotAttackNguoiYeu).

    public bool DoesNotAttackNguoiYeu { get; set; }

    // Lưu trạng thái LastTimePlayerNotAttackNguoiYeu (Java: lastTimePlayerNotAttackNguoiYeu).

    public long LastTimePlayerNotAttackNguoiYeu { get; set; }

    // Lưu trạng thái TimeNotAttackNguoiYeu (Java: timeNotAttackNguoiYeu).

    public int TimeNotAttackNguoiYeu { get; set; } = 1800000;

    // Lưu trạng thái TemporaryEnemiesNguoiYeu (Java: temporaryEnemiesNguoiYeu).

    public List<Player> TemporaryEnemiesNguoiYeu { get; set; } = [];

    // Lưu trạng thái JustRevivedNguoiYeu (Java: justRevivedNguoiYeu).

    public bool JustRevivedNguoiYeu { get; set; }

    // Lưu trạng thái LastTimeRevivedNguoiYeu (Java: lastTimeRevivedNguoiYeu).

    public long LastTimeRevivedNguoiYeu { get; set; }

    // Lưu trạng thái DoesNotAttackConone (Java: doesNotAttackConone).

    public bool DoesNotAttackConone { get; set; }

    // Lưu trạng thái LastTimePlayerNotAttackConone (Java: lastTimePlayerNotAttackConone).

    public long LastTimePlayerNotAttackConone { get; set; }

    // Lưu trạng thái TimeNotAttackConone (Java: timeNotAttackConone).

    public int TimeNotAttackConone { get; set; } = 1800000;

    // Lưu trạng thái TemporaryEnemiesConone (Java: temporaryEnemiesConone).

    public List<Player> TemporaryEnemiesConone { get; set; } = [];

    // Lưu trạng thái JustRevivedConone (Java: justRevivedConone).

    public bool JustRevivedConone { get; set; }

    // Lưu trạng thái LastTimeRevivedConone (Java: lastTimeRevivedConone).

    public long LastTimeRevivedConone { get; set; }

    // Lưu trạng thái DoesNotAttackContwo (Java: doesNotAttackContwo).

    public bool DoesNotAttackContwo { get; set; }

    // Lưu trạng thái LastTimePlayerNotAttackContwo (Java: lastTimePlayerNotAttackContwo).

    public long LastTimePlayerNotAttackContwo { get; set; }

    // Lưu trạng thái TimeNotAttackContwo (Java: timeNotAttackContwo).

    public int TimeNotAttackContwo { get; set; } = 1800000;

    // Lưu trạng thái TemporaryEnemiesContwo (Java: temporaryEnemiesContwo).

    public List<Player> TemporaryEnemiesContwo { get; set; } = [];

    // Lưu trạng thái JustRevivedContwo (Java: justRevivedContwo).

    public bool JustRevivedContwo { get; set; }

    // Lưu trạng thái LastTimeRevivedContwo (Java: lastTimeRevivedContwo).

    public long LastTimeRevivedContwo { get; set; }

    // Lưu trạng thái DoesNotAttackConthree (Java: doesNotAttackConthree).

    public bool DoesNotAttackConthree { get; set; }

    // Lưu trạng thái LastTimePlayerNotAttackConthree (Java: lastTimePlayerNotAttackConthree).

    public long LastTimePlayerNotAttackConthree { get; set; }

    // Lưu trạng thái TimeNotAttackConthree (Java: timeNotAttackConthree).

    public int TimeNotAttackConthree { get; set; } = 1800000;

    // Lưu trạng thái TemporaryEnemiesConthree (Java: temporaryEnemiesConthree).

    public List<Player> TemporaryEnemiesConthree { get; set; } = [];

    // Lưu trạng thái JustRevivedConthree (Java: justRevivedConthree).

    public bool JustRevivedConthree { get; set; }

    // Lưu trạng thái LastTimeRevivedConthree (Java: lastTimeRevivedConthree).

    public long LastTimeRevivedConthree { get; set; }

    // Lưu trạng thái XSend (Java: xSend).

    public int XSend { get; set; }

    // Lưu trạng thái YSend (Java: ySend).

    public int YSend { get; set; }

    // Lưu trạng thái IsFly (Java: isFly).

    public bool IsFly { get; set; }

    // Lưu trạng thái LastTimeShenronAppeared_Halloween (Java: lastTimeShenronAppeared_Halloween).

    public long LastTimeShenronAppeared_Halloween { get; set; }

    // Lưu trạng thái IsShenronAppear_Halloween (Java: isShenronAppear_Halloween).

    public bool IsShenronAppear_Halloween { get; set; }

    // Lưu trạng thái ShenronEvent_Halloween (Java: shenronEvent_Halloween).

    public ShenronHalloweenEvent? ShenronEvent_Halloween { get; set; }

    // Lưu trạng thái LastTimeShenronAppeared_Christmas (Java: lastTimeShenronAppeared_Christmas).

    public long LastTimeShenronAppeared_Christmas { get; set; }

    // Lưu trạng thái IsShenronAppear_Christmas (Java: isShenronAppear_Christmas).

    public bool IsShenronAppear_Christmas { get; set; }

    // Lưu trạng thái ShenronEvent_Christmas (Java: shenronEvent_Christmas).

    public ShenronChristMasEvent? ShenronEvent_Christmas { get; set; }

    // Lưu trạng thái TypeChibi (Java: typeChibi).

    public int TypeChibi { get; set; }

    // Lưu trạng thái LastTimeChibi (Java: lastTimeChibi).

    public long LastTimeChibi { get; set; }

    // Lưu trạng thái LastTimeUpdateChibi (Java: lastTimeUpdateChibi).

    public long LastTimeUpdateChibi { get; set; }

    // Lưu trạng thái IdNRNM (Java: idNRNM).

    public short IdNRNM { get; set; } = -1;

    // Lưu trạng thái IdGo (Java: idGo).

    public short IdGo { get; set; } = -1;

    // Lưu trạng thái LastTimePickNRNM (Java: lastTimePickNRNM).

    public long LastTimePickNRNM { get; set; }

    // Lưu trạng thái IsBot (Java: isBot).

    public bool IsBot { get; set; }
    private long TimeSpawnBot { get; set; }

    // Lưu trạng thái IsBot_New (Java: isBot_New).

    public bool IsBot_New { get; set; }

    // Lưu trạng thái IsBotLogin (Java: isBotLogin).

    public bool IsBotLogin { get; set; }

    // Lưu trạng thái IsBot_Event (Java: isBot_Event).

    public bool IsBot_Event { get; set; }

    // Lưu trạng thái IsBot_Valentine (Java: isBot_Valentine).

    public bool IsBot_Valentine { get; set; }

    // Lưu trạng thái GoldNormar (Java: goldNormar).

    public int GoldNormar { get; set; }

    // Lưu trạng thái GoldVIP (Java: goldVIP).

    public int GoldVIP { get; set; }

    // Lưu trạng thái AutoHP (Java: autoHP).

    public bool AutoHP { get; set; }

    // Lưu trạng thái AutoKI (Java: autoKI).

    public bool AutoKI { get; set; }

    // Lưu trạng thái AutoSD (Java: autoSD).

    public bool AutoSD { get; set; }

    // Lưu trạng thái AutoGiap (Java: autoGiap).

    public bool AutoGiap { get; set; }

    // Lưu trạng thái THE_TUAN (Java: THE_TUAN).

    public int THE_TUAN { get; set; }

    // Lưu trạng thái LASTTIME_THE_TUAN (Java: LASTTIME_THE_TUAN).

    public long LASTTIME_THE_TUAN { get; set; }

    // Lưu trạng thái THE_THANG (Java: THE_THANG).

    public int THE_THANG { get; set; }

    // Lưu trạng thái LASTTIME_THE_THANG (Java: LASTTIME_THE_THANG).

    public long LASTTIME_THE_THANG { get; set; }

    // Lưu trạng thái THE_NAM (Java: THE_NAM).

    public int THE_NAM { get; set; }

    // Lưu trạng thái LASTTIME_THE_NAM (Java: LASTTIME_THE_NAM).

    public long LASTTIME_THE_NAM { get; set; }

    // Lưu trạng thái THE_CHI_TON (Java: THE_CHI_TON).

    public int THE_CHI_TON { get; set; }

    // Lưu trạng thái LASTTIME_THE_CHI_TON (Java: LASTTIME_THE_CHI_TON).

    public long LASTTIME_THE_CHI_TON { get; set; }

    // Lưu trạng thái DIEM_DANH (Java: DIEM_DANH).

    public int DIEM_DANH { get; set; }

    // Lưu trạng thái CHECK_DAY_ON_ONE (Java: CHECK_DAY_ON_ONE).

    public int CHECK_DAY_ON_ONE { get; set; }

    // Lưu trạng thái DIEM_DANH_TUAN (Java: DIEM_DANH_TUAN).

    public int DIEM_DANH_TUAN { get; set; }

    // Lưu trạng thái CHECK_DAY_ON_TUAN (Java: CHECK_DAY_ON_TUAN).

    public int CHECK_DAY_ON_TUAN { get; set; }

    // Lưu trạng thái DIEM_DANH_THANG (Java: DIEM_DANH_THANG).

    public int DIEM_DANH_THANG { get; set; }

    // Lưu trạng thái CHECK_DAY_ON_THANG (Java: CHECK_DAY_ON_THANG).

    public int CHECK_DAY_ON_THANG { get; set; }

    // Lưu trạng thái DIEM_DANH_NAM (Java: DIEM_DANH_NAM).

    public int DIEM_DANH_NAM { get; set; }

    // Lưu trạng thái CHECK_DAY_ON_NAM (Java: CHECK_DAY_ON_NAM).

    public int CHECK_DAY_ON_NAM { get; set; }

    // Lưu trạng thái DIEM_DANH_CHI_TON (Java: DIEM_DANH_CHI_TON).

    public int DIEM_DANH_CHI_TON { get; set; }

    // Lưu trạng thái CHECK_DAY_ON_CHI_TON (Java: CHECK_DAY_ON_CHI_TON).

    public int CHECK_DAY_ON_CHI_TON { get; set; }

    // Lưu trạng thái IsUseDanhHieu_ThienTu (Java: isUseDanhHieu_ThienTu).

    public bool IsUseDanhHieu_ThienTu { get; set; }

    // Lưu trạng thái LastTimeDanhHieu_ThienTu (Java: lastTimeDanhHieu_ThienTu).

    public long LastTimeDanhHieu_ThienTu { get; set; }

    // Lưu trạng thái IsUseDanhHieu_2 (Java: isUseDanhHieu_2).

    public bool IsUseDanhHieu_2 { get; set; }

    // Lưu trạng thái LastTimeDanhHieu_2 (Java: lastTimeDanhHieu_2).

    public long LastTimeDanhHieu_2 { get; set; }

    // Lưu trạng thái IsUseDanhHieu_3 (Java: isUseDanhHieu_3).

    public bool IsUseDanhHieu_3 { get; set; }

    // Lưu trạng thái LastTimeDanhHieu_3 (Java: lastTimeDanhHieu_3).

    public long LastTimeDanhHieu_3 { get; set; }

    // Lưu trạng thái IsUseDanhHieu_4 (Java: isUseDanhHieu_4).

    public bool IsUseDanhHieu_4 { get; set; }

    // Lưu trạng thái LastTimeDanhHieu_4 (Java: lastTimeDanhHieu_4).

    public long LastTimeDanhHieu_4 { get; set; }

    // Lưu trạng thái SlBanhChung (Java: slBanhChung).

    public int SlBanhChung { get; set; }

    // Lưu trạng thái SlBanhTet (Java: slBanhTet).

    public int SlBanhTet { get; set; }

    // Lưu trạng thái ItemsTradeWVP (Java: itemsTradeWVP).

    public List<Item> ItemsTradeWVP { get; set; } = [];

    // Lưu trạng thái GoldTradeWVP (Java: goldTradeWVP).

    public long GoldTradeWVP { get; set; }

    // Lưu trạng thái TradeWVP (Java: tradeWVP).

    public bool TradeWVP { get; set; }

    // Lưu trạng thái PlIdWVP (Java: plIdWVP).

    public long PlIdWVP { get; set; }

    // Lưu trạng thái LastTimeTranformation (Java: lastTimeTranformation).

    public long LastTimeTranformation { get; set; }

    // Lưu trạng thái Isbienhinh (Java: isbienhinh).

    public int Isbienhinh { get; set; }

    // Lưu trạng thái CountBDKB (Java: countBDKB).

    public sbyte CountBDKB { get; set; }

    // Lưu trạng thái FirstJoinBDKB (Java: firstJoinBDKB).

    public bool FirstJoinBDKB { get; set; }

    // Lưu trạng thái LastimeJoinBDKB (Java: lastimeJoinBDKB).

    public long LastimeJoinBDKB { get; set; }

    // Lưu trạng thái GoldTai (Java: goldTai).

    public int GoldTai { get; set; }

    // Lưu trạng thái Last_time_dd (Java: last_time_dd).

    public long Last_time_dd { get; set; }

    // Lưu trạng thái GoldXiu (Java: goldXiu).

    public int GoldXiu { get; set; }

    // Lưu trạng thái BeforeDispose (Java: beforeDispose).

    public bool BeforeDispose { get; set; }

    // Lưu trạng thái ErrorMap (Java: errorMap).

    public sbyte ErrorMap { get; set; }

    // Lưu trạng thái ErrorLocation (Java: errorLocation).

    public sbyte ErrorLocation { get; set; }

    // Lưu trạng thái ErrorPay (Java: errorPay).

    public sbyte ErrorPay { get; set; }

    // Lưu trạng thái Thoigianduhanh (Java: thoigianduhanh).

    public long Thoigianduhanh { get; set; }

    // Lưu trạng thái Isthoigianduhanh (Java: isthoigianduhanh).

    public bool Isthoigianduhanh { get; set; }

    // Lưu trạng thái ThueOil (Java: thueOil).

    public bool ThueOil { get; set; }

    // Lưu trạng thái ThueMacki (Java: thueMacki).

    public bool ThueMacki { get; set; }

    // Lưu trạng thái ThueGas (Java: thueGas).

    public bool ThueGas { get; set; }

    // Lưu trạng thái ThueElec (Java: thueElec).

    public bool ThueElec { get; set; }

    // Lưu trạng thái GiaiPhongAnNgoKhong (Java: giaiPhongAnNgoKhong).

    public bool GiaiPhongAnNgoKhong { get; set; }

    // Lưu trạng thái IsBossFlag (Java: isBossFlag).

    public bool IsBossFlag { get; set; }

    // Lưu trạng thái Giaiphongan (Java: giaiphongan).

    public sbyte Giaiphongan { get; set; }

    // Lưu trạng thái Solanhotong (Java: solanhotong).

    public short Solanhotong { get; set; }

    // Lưu trạng thái NhanThuocTrongNgay (Java: nhanThuocTrongNgay).

    public sbyte NhanThuocTrongNgay { get; set; }

    // Lưu trạng thái TimeTrongNgay (Java: timeTrongNgay).

    public long TimeTrongNgay { get; set; }

    // Lưu trạng thái Bosshoimau (Java: bosshoimau).

    public bool Bosshoimau { get; set; }

    // Lưu trạng thái Trbne (Java: trbne).

    public sbyte Trbne { get; set; }

    // Lưu trạng thái Trbgiap (Java: trbgiap).

    public sbyte Trbgiap { get; set; }

    // Lưu trạng thái Actived (Java: actived).

    public int Actived { get; set; }

    // Lưu trạng thái Account_id (Java: account_id).

    public int Account_id { get; set; }

    // Lưu trạng thái Demthoigian (Java: demthoigian).

    public long Demthoigian { get; set; }

    // Lưu trạng thái Timeres (Java: timeres).

    public long Timeres { get; set; }

    // Lưu trạng thái Timedanhboss (Java: timedanhboss).

    public int Timedanhboss { get; set; }

    // Lưu trạng thái Bossxuathien (Java: bossxuathien).

    public int Bossxuathien { get; set; }

    // Lưu trạng thái Pvp (Java: pvp).

    public IPVP? Pvp { get; set; }

    // Lưu trạng thái MaxTime (Java: maxTime).

    public sbyte MaxTime { get; set; } = 30;

    // Lưu trạng thái Type (Java: type).

    public sbyte Type { get; set; }

    // Lưu trạng thái Timeoff (Java: timeoff).

    public long Timeoff { get; set; }

    // Lưu trạng thái Buff (Java: buff).

    public Buff Buff { get; set; } = Buff.NONE;

    // Lưu trạng thái Pointfusion (Java: pointfusion).

    public PointFusion Pointfusion { get; set; } = null!;

    // Lưu trạng thái DailyGiftData (Java: dailyGiftData).

    public List<DailyGiftData> DailyGiftData { get; set; } = [];

    // Lưu trạng thái Event (Java: event).

    public PlayerEvent Event { get; set; } = null!;

    // Lưu trạng thái Saibamen (Java: saibamen).

    public SaiBaMen? Saibamen { get; set; }

    // Lưu trạng thái IsSaibamen (Java: isSaibamen).

    public bool IsSaibamen { get; set; }

    // Lưu trạng thái Badges (Java: badges).

    public Badges Badges { get; set; } = null!;

    // Lưu trạng thái MapIdBeforeLogout (Java: mapIdBeforeLogout).

    public int MapIdBeforeLogout { get; set; }

    // Lưu trạng thái PhanThan (Java: phanThan).

    public PhanThan? PhanThan { get; set; }

    // Lưu trạng thái MapBlackBall (Java: mapBlackBall).

    public List<Zone>? MapBlackBall { get; set; }

    // Lưu trạng thái MapMaBu (Java: mapMaBu).

    public List<Zone>? MapMaBu { get; set; }

    // Lưu trạng thái DataBadges (Java: dataBadges).

    public List<BadgesData> DataBadges { get; set; } = [];

    // Lưu trạng thái DataTaskBadges (Java: dataTaskBadges).

    public List<BadgesTask> DataTaskBadges { get; set; } = [];

    // Lưu trạng thái ItemEvent (Java: itemEvent).

    public ItemEvent ItemEvent { get; set; } = null!;

    // Lưu trạng thái Zone (Java: zone).

    public Zone? Zone { get; set; }

    // Lưu trạng thái MapBeforeCapsule (Java: mapBeforeCapsule).

    public Zone? MapBeforeCapsule { get; set; }

    // Lưu trạng thái MapCapsule (Java: mapCapsule).

    public List<Zone>? MapCapsule { get; set; }

    // Lưu trạng thái Detu (Java: detu).

    public Detu? Detu { get; set; }

    // Lưu trạng thái PetFollow (Java: petFollow).

    public PetFollow? PetFollow { get; set; }

    // Lưu trạng thái Duongtang (Java: duongtang).

    public DuongTang? Duongtang { get; set; }

    // Lưu trạng thái SuperRank (Java: superRank).

    public SuperRank SuperRank { get; set; } = null!;

    // Lưu trạng thái EggMob (Java: deTrung).

    public EggMob? EggMob { get; set; }

    // Lưu trạng thái Location (Java: location).

    public Location Location { get; set; } = null!;

    // Thực hiện SetClothes (Java: setClothes).

    public SetClothes SetClothes { get; set; } = null!;

    // Lưu trạng thái EffectSkill (Java: effectSkill).

    public EffectSkill EffectSkill { get; set; } = null!;

    // Lưu trạng thái MabuEgg (Java: mabuEgg).

    public MabuEgg? MabuEgg { get; set; }

    // Lưu trạng thái BillEgg (Java: billEgg).

    public BillEgg? BillEgg { get; set; }

    // Lưu trạng thái Duahau (Java: duahau).

    public MelonPlant? Duahau { get; set; }

    // Lưu trạng thái PlayerTask (Java: playerTask).

    public TaskPlayer PlayerTask { get; set; } = null!;

    // Lưu trạng thái ItemTime (Java: itemTime).

    public ItemTime ItemTime { get; set; } = null!;

    // Lưu trạng thái Fusion (Java: fusion).

    public Fusion Fusion { get; set; } = null!;

    // Lưu trạng thái MagicTree (Java: magicTree).

    public MagicTree? MagicTree { get; set; }

    // Lưu trạng thái PlayerIntrinsic (Java: playerIntrinsic).

    public IntrinsicPlayer PlayerIntrinsic { get; set; } = null!;

    // Lưu trạng thái Inventory (Java: inventory).

    public Inventory Inventory { get; set; } = null!;

    // Lưu trạng thái PlayerSkill (Java: playerSkill).

    public PlayerSkill PlayerSkill { get; set; } = null!;

    // Lưu trạng thái Combine (Java: combine).

    public Combine Combine { get; set; } = null!;

    // Lưu trạng thái IDMark (Java: iDMark).

    public IDMark IDMark { get; set; } = null!;

    // Lưu trạng thái Charms (Java: charms).

    public Charms Charms { get; set; } = null!;

    // Lưu trạng thái EffectSkin (Java: effectSkin).

    public EffectSkin EffectSkin { get; set; } = null!;

    // Lưu trạng thái Gift (Java: gift).

    public Gift Gift { get; set; } = null!;

    // Lưu trạng thái NPoint (Java: nPoint).

    public NPoint NPoint { get; set; } = null!;

    // Lưu trạng thái RewardBlackBall (Java: rewardBlackBall).

    public RewardBlackBall RewardBlackBall { get; set; } = null!;

    // Lưu trạng thái EffectFlagBag (Java: effectFlagBag).

    public EffectFlagBag EffectFlagBag { get; set; } = null!;

    // Lưu trạng thái FightMabu (Java: fightMabu).

    public FightMabu FightMabu { get; set; } = null!;

    // Lưu trạng thái NewSkill (Java: newSkill).

    public NewSkill NewSkill { get; set; } = null!;

    // Lưu trạng thái Achievement (Java: achievement).

    public Achievement Achievement { get; set; } = null!;

    // Lưu trạng thái Clan (Java: clan).

    public Clan? Clan { get; set; }

    // Lưu trạng thái ClanMember (Java: clanMember).

    public ClanMember? ClanMember { get; set; }

    // Lưu trạng thái Satellite (Java: satellite).

    public Satellite Satellite { get; set; } = null!;

    // Lưu trạng thái Friends (Java: friends).

    public List<Friend> Friends { get; set; } = [];

    // Lưu trạng thái Enemies (Java: enemies).

    public List<Enemy> Enemies { get; set; } = [];
    private DropItem DropItem { get; set; } = null!;

    // Lưu trạng thái Cards (Java: cards).

    public List<Card> Cards { get; set; } = [];

    // Lưu trạng thái IsAutoHoiSinh (Java: isAutoHoiSinh).

    public bool IsAutoHoiSinh { get; set; }

    // Lưu trạng thái SoNgayTaoAcc (Java: soNgayTaoAcc).

    public int SoNgayTaoAcc { get; set; }

    // Lưu trạng thái QuocTich (Java: quocTich).

    public sbyte QuocTich { get; set; }

    // Lưu trạng thái TimeChangeZone (Java: timeChangeZone).

    public long TimeChangeZone { get; set; }

    // Lưu trạng thái LastUseOptionTime (Java: lastUseOptionTime).

    public long LastUseOptionTime { get; set; }

    // Lưu trạng thái LastTimeHoiPhuc (Java: lastTimeHoiPhuc).

    public long LastTimeHoiPhuc { get; set; }

    // Lưu trạng thái DucNTdamethanmeo (Java: ducNTdamethanmeo).

    public float DucNTdamethanmeo { get; set; }

    // Lưu trạng thái TimeChatKhukhu (Java: timeChatKhukhu).

    public long TimeChatKhukhu { get; set; }

    // Lưu trạng thái IdEffChar (Java: idEffChar).

    public List<int> IdEffChar { get; set; } = [];

    // Lưu trạng thái BoughtSkill (Java: boughtSkill).

    public List<int> BoughtSkill { get; set; } = [];

    // Lưu trạng thái LearnSkill (Java: learnSkill).

    public PlayerSkill LearnSkill { get; set; } = null!;

    // Lưu trạng thái GoldChan (Java: goldChan).

    public int GoldChan { get; set; }

    // Lưu trạng thái GoldLe (Java: goldLe).

    public int GoldLe { get; set; }

    // Lưu trạng thái ReceivedGift50k (Java: receivedGift50k).

    public bool ReceivedGift50k { get; set; }

    // Lưu trạng thái ReceivedGift100k (Java: receivedGift100k).

    public bool ReceivedGift100k { get; set; }

    // Lưu trạng thái Mocnap (Java: mocnap).

    public int[] Mocnap { get; set; } = [0, 0, 0, 0, 0];

    // Lưu trạng thái LastTimeSendTextTime (Java: lastTimeSendTextTime).

    public long LastTimeSendTextTime { get; set; }

    // Lưu trạng thái LastTimeSendTextTime_2 (Java: lastTimeSendTextTime_2).

    public long LastTimeSendTextTime_2 { get; set; }

    // Khởi tạo shell; nowMs thay thời gian tường Java cho LastUseOptionTime, truyền 0 khi chưa biết.

    public Player(long nowMs = 0)
    {
        LearnSkill = new PlayerSkill();
        LastUseOptionTime = nowMs;
        Location = new Location();
        NPoint = new NPoint(this);
        Inventory = new Inventory();
        PlayerSkill = new PlayerSkill(this);
        SetClothes = new SetClothes(this);
        EffectSkill = new EffectSkill(this);
        Fusion = new Fusion(this);
        PlayerIntrinsic = new IntrinsicPlayer();
        RewardBlackBall = new RewardBlackBall(this);
        EffectFlagBag = new EffectFlagBag();
        FightMabu = new FightMabu(this);
        Satellite = new Satellite();
        IDMark = new IDMark();
        Combine = new Combine();
        PlayerTask = new TaskPlayer();
        ItemTime = new ItemTime(this);
        Charms = new Charms();
        Gift = new Gift(this);
        EffectSkin = new EffectSkin(this);
        NewSkill = new NewSkill(this);
        Achievement = new Achievement(this);
        DropItem = new DropItem(this);
        SuperRank = new SuperRank(this);
        ItemEvent = new ItemEvent(this);
        Badges = new Badges();
        Event = new PlayerEvent(this);
        Traning = new Traning();
        Pointfusion = new PointFusion(this);
        Buff = Buff.NONE;
    }
}
