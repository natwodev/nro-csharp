namespace Nro.Game.Players.Stats;

// Mô hình bản chụp các dữ liệu cộng tác mà NPoint cần để tính chỉ số.
// Nguồn Java: nro/player/NPoint.java (Java đọc trực tiếp khoảng 25 lớp như ItemTime, SetClothes, Zone, MapService, InventoryService và ServerManager; C# chụp chúng một lần qua INPointHost rồi chỉ làm số học để không phụ thuộc singleton gI() và các track song song).
// Tên thuộc tính là tên Java viết hoa chữ đầu theo quy ước Player shell, nên player.itemTime.isUseHongDao của Java tương ứng facts.ItemTime.IsUseRedPeach.

// Biểu diễn một tùy chọn vật phẩm bằng mã mẫu tùy chọn và tham số của nó.
public readonly record struct NPointOption(int Id, int Param);

// Giữ phần vật phẩm NPoint nhìn thấy; ô trống Java vẫn là Item không có template nên IsNotNullItem thay cho kiểm tra đó.
public sealed class NPointItem
{
    // Cho biết vật phẩm có template, tương đương item.isNotNullItem() của Java.
    public bool IsNotNullItem { get; set; } = true;

    // Mã template vật phẩm.
    public int TemplateId { get; set; }

    // Loại template vật phẩm.
    public int TemplateType { get; set; }

    // Các tùy chọn vật phẩm; null khi danh sách Java là null.
    public List<NPointOption>? ItemOptions { get; set; } = [];

    // Phần trăm giáp tập luyện của ItemService: 10/20/30/40 cho giáp tương ứng, còn lại 0.
    public int TrainArmorPercent { get; set; }

    // Giữ đối tượng nguồn để host ghi lại giáp tập luyện NPoint chọn; NPoint không diễn giải giá trị này.
    public object? Source { get; set; }
}

// Biểu diễn tùy chọn của thẻ ra-đa OptionCard trong Java.
public sealed class NPointCardOption
{

    // Dữ liệu Id dùng trong bản chụp phục vụ công thức chỉ số.
    public int Id { get; set; }

    // Dữ liệu Param dùng trong bản chụp phục vụ công thức chỉ số.
    public int Param { get; set; }

    // Cấp thẻ kích hoạt tùy chọn; 0 nghĩa là hiệu lực ngay từ đầu.
    public sbyte Active { get; set; }
}

// Giữ phần Card mà NPoint đọc; chỉ thẻ đầu tiên có Used bằng 1 được tính.
public sealed class NPointCard
{

    // Dữ liệu Used dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Used { get; set; }

    // Dữ liệu Level dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Level { get; set; }

    // Dữ liệu Options dùng trong bản chụp phục vụ công thức chỉ số.
    public List<NPointCardOption> Options { get; set; } = [];
}

// Giữ mã và tham số thứ nhất của Intrinsic mà NPoint sử dụng.
public sealed class NPointIntrinsic
{

    // Dữ liệu Id dùng trong bản chụp phục vụ công thức chỉ số.
    public int Id { get; set; }

    // Dữ liệu Param1 dùng trong bản chụp phục vụ công thức chỉ số.
    public short Param1 { get; set; }
}

// Chứa các ngưỡng của cấp sức mạnh hiện tại từ PowerLimitManager.get(level).
public sealed record PowerLimitInfo(long Power, long Hp, long Mp, long Damage, int Defense, int Critical);

// Tập cờ ItemTime mà NPoint đọc; mọi cờ mặc định false, nghĩa là chưa có hiệu ứng vật phẩm.
public sealed class NPointItemTimeFacts
{

    // Dữ liệu IsUseStandardIpanaOintment dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseStandardIpanaOintment { get; set; }

    // Dữ liệu IsUseSpecialIpanaOintment dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseSpecialIpanaOintment { get; set; }

    // Dữ liệu IsUseSuperRegen dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseSuperRegen { get; set; }

    // Dữ liệu IsUseRabbitMooncake1 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRabbitMooncake1 { get; set; }

    // Dữ liệu IsUseRabbitMooncake2 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRabbitMooncake2 { get; set; }

    // Dữ liệu IsUseRabbitMooncake3 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRabbitMooncake3 { get; set; }

    // Dữ liệu IsUseRocket1h dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRocket1h { get; set; }

    // Dữ liệu IsBoneDragon dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsBoneDragon { get; set; }

    // Dữ liệu IsBoneDragon2 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsBoneDragon2 { get; set; }

    // Dữ liệu IsBoneDragon3 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsBoneDragon3 { get; set; }

    // Dữ liệu IsUseBloodTonic dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseBloodTonic { get; set; }

    // Dữ liệu IsUseBloodTonic2 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseBloodTonic2 { get; set; }

    // Dữ liệu IsUseEnergyTonic dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseEnergyTonic { get; set; }

    // Dữ liệu IsUseEnergyTonic2 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseEnergyTonic2 { get; set; }

    // Dữ liệu IsUseRedPeach0 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRedPeach0 { get; set; }

    // Dữ liệu IsUseRedPeach dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRedPeach { get; set; }

    // Dữ liệu IsUseRedPeach1 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRedPeach1 { get; set; }

    // Dữ liệu IsUseRedPeach3 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRedPeach3 { get; set; }

    // Dữ liệu IsUseRedPeach5 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRedPeach5 { get; set; }

    // Dữ liệu IsUseRedPeach10 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRedPeach10 { get; set; }

    // Dữ liệu IsUseRedPeach25 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRedPeach25 { get; set; }

    // Dữ liệu IsUseRedPeach50 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRedPeach50 { get; set; }

    // Dữ liệu IsUseRedPeach99 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRedPeach99 { get; set; }

    // Dữ liệu IsUseRedPeach999 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseRedPeach999 { get; set; }

    // Dữ liệu HasHpBoost30 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool HasHpBoost30 { get; set; }

    // Dữ liệu HasHpBoost15 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool HasHpBoost15 { get; set; }

    // Dữ liệu HasEnergyBoost30 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool HasEnergyBoost30 { get; set; }

    // Dữ liệu HasEnergyBoost15 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool HasEnergyBoost15 { get; set; }

    // Dữ liệu HasDamageBoost30 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool HasDamageBoost30 { get; set; }

    // Dữ liệu HasDamageBoost15 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool HasDamageBoost15 { get; set; }

    // Dữ liệu IsUseDarkPumpkinSoup dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseDarkPumpkinSoup { get; set; }

    // Dữ liệu IsUseBreadedShrimp dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseBreadedShrimp { get; set; }

    // Dữ liệu HasMonkeyTail dùng trong bản chụp phục vụ công thức chỉ số.
    public bool HasMonkeyTail { get; set; }

    // Dữ liệu IsUseMoonFestivalOneEgg dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseMoonFestivalOneEgg { get; set; }

    // Dữ liệu IsUseMoonFestivalTwoEgg dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseMoonFestivalTwoEgg { get; set; }

    // Dữ liệu IsUseSpecialMoonFestival dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseSpecialMoonFestival { get; set; }

    // Dữ liệu IsUseMoonFestivalCakeBox dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseMoonFestivalCakeBox { get; set; }

    // Dữ liệu IsEatMeal dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsEatMeal { get; set; }

    // Dữ liệu IsUseRage dùng trong bản chụp phục vụ công thức chỉ số (Java: isUseCuongNo).
    public bool IsUseRage { get; set; }

    // Dữ liệu IsUseRage2 dùng trong bản chụp phục vụ công thức chỉ số (Java: isUseCuongNo2).
    public bool IsUseRage2 { get; set; }

    // Dữ liệu IsUseCylindricalRiceCake dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseCylindricalRiceCake { get; set; }

    // Dữ liệu IsUseSquareRiceCake dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseSquareRiceCake { get; set; }

    // Dữ liệu IsUseCaterpillarHamburger dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseCaterpillarHamburger { get; set; }

    // Dữ liệu IsUseTamarindCrab dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseTamarindCrab { get; set; }

    // Dữ liệu IsUseSpiderCake dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseSpiderCake { get; set; }

    // Dữ liệu IsUseDodge dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseDodge { get; set; }

    // Dữ liệu IsUseDodge2 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseDodge2 { get; set; }

    // Dữ liệu IsUseReflectDamage dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseReflectDamage { get; set; }

    // Dữ liệu IsUseReflectDamage2 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseReflectDamage2 { get; set; }

    // Dữ liệu IsUseReflectDamage3 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseReflectDamage3 { get; set; }

    // Dữ liệu IsFaceMask dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsFaceMask { get; set; }

    // Dữ liệu IsUseTrueDamage dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseTrueDamage { get; set; }

    // Dữ liệu IsUseTrueDamage2 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseTrueDamage2 { get; set; }

    // Dữ liệu IsUseDurian dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseDurian { get; set; }

    // Dữ liệu IsUseStandardInvisibilityPotion dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseStandardInvisibilityPotion { get; set; }

    // Dữ liệu IsUseEnhancedInvisibilityPotion dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseEnhancedInvisibilityPotion { get; set; }

    // Dữ liệu IsUseCriticalChance2 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseCriticalChance2 { get; set; }

    // Dữ liệu IsUseCriticalChance3 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseCriticalChance3 { get; set; }

    // Dữ liệu IsPowerBurst dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsPowerBurst { get; set; }

    // Dữ liệu IsUseOneEyedCandy dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseOneEyedCandy { get; set; }

    // Dữ liệu IsUseGrilledOctopus dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseGrilledOctopus { get; set; }

    // Dữ liệu IsUseMonkeyTailPowerPotential dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUseMonkeyTailPowerPotential { get; set; }

    // Dữ liệu IsUsePetPowerPotentialCharm dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsUsePetPowerPotentialCharm { get; set; }
}

// Tập trường EffectSkill mà các công thức chỉ số cần đọc.
public sealed class NPointEffectSkillFacts
{

    // Dữ liệu IsVirus dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsVirus { get; set; }

    // Dữ liệu IsSnowflake dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsSnowflake { get; set; }

    // Dữ liệu IsMonkey dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsMonkey { get; set; }

    // Dữ liệu LevelMonkey dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte LevelMonkey { get; set; }

    // Dữ liệu HpWhistlePercent dùng trong bản chụp phục vụ công thức chỉ số.
    public int HpWhistlePercent { get; set; }

    // Dữ liệu IsChibi dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsChibi { get; set; }

    // Dữ liệu IsPowerBurst dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsPowerBurst { get; set; }

    // Dữ liệu IsCumber dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsCumber { get; set; }

    // Dữ liệu IsCumber2 dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsCumber2 { get; set; }

    // Dữ liệu IsKefla dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsKefla { get; set; }

    // Dữ liệu IsDamageBuff dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsDamageBuff { get; set; }

    // Dữ liệu DamageBuffPercent dùng trong bản chụp phục vụ công thức chỉ số.
    public int DamageBuffPercent { get; set; }
}

// Tập trường EffectSkin NPoint đọc hoặc ghi; host ghi trả IsBlastDamageMultiplierReady, IsBlastDamageMultiplierApplied và LastTimeBlastDamageMultiplier sau một lần tấn công.
public sealed class NPointEffectSkinFacts
{

    // Dữ liệu IsSlow dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsSlow { get; set; }

    // Dữ liệu IsRabbitLeader dùng trong bản chụp phục vụ công thức chỉ số (Java: isThoDaiKa).
    public bool IsRabbitLeader { get; set; }

    // Dữ liệu HpKiMultiplier dùng trong bản chụp phục vụ công thức chỉ số (Java: xHPKI).
    public int HpKiMultiplier { get; set; } = 1;

    // Dữ liệu DamageMultiplier dùng trong bản chụp phục vụ công thức chỉ số (Java: xDame).
    public int DamageMultiplier { get; set; } = 1;

    // Dữ liệu IsBlastDamageMultiplierReady dùng trong bản chụp phục vụ công thức chỉ số (Java: isXChuong).
    public bool IsBlastDamageMultiplierReady { get; set; }

    // Dữ liệu IsBlastDamageMultiplierApplied dùng trong bản chụp phục vụ công thức chỉ số (Java: isXDame).
    public bool IsBlastDamageMultiplierApplied { get; set; }

    // Dữ liệu LastTimeBlastDamageMultiplier dùng trong bản chụp phục vụ công thức chỉ số (Java: lastTimeXChuong).
    public long LastTimeBlastDamageMultiplier { get; set; }
}

// Tập trường SetClothes NPoint đọc; Worldcup còn được NPoint đếm lại số áo World Cup rồi host ghi trả.
public sealed class NPointSetClothesFacts
{

    // Dữ liệu Songoku dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Songoku { get; set; }

    // Dữ liệu Kirin dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Kirin { get; set; }

    // Dữ liệu ThanVuTruKaio dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte ThanVuTruKaio { get; set; }

    // Dữ liệu Yamcha dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Yamcha { get; set; }

    // Dữ liệu OcTieu dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte OcTieu { get; set; }

    // Dữ liệu PikkoroDaimao dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte PikkoroDaimao { get; set; }

    // Dữ liệu Picolo dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Picolo { get; set; }

    // Dữ liệu Nail dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Nail { get; set; }

    // Dữ liệu Kakarot dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Kakarot { get; set; }

    // Dữ liệu Nappa dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Nappa { get; set; }

    // Dữ liệu CadicM dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte CadicM { get; set; }

    // Dữ liệu Gohan dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Gohan { get; set; }

    // Dữ liệu Goten dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Goten { get; set; }

    // Dữ liệu Cumber dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Cumber { get; set; }

    // Dữ liệu Worldcup dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Worldcup { get; set; }

    // Dữ liệu cải trang Nezuko dùng trong bản chụp phục vụ công thức chỉ số (Java: ctNezuko).
    public int CostumeNezuko { get; set; } = -1;

    // Dữ liệu cải trang Tanjiro dùng trong bản chụp phục vụ công thức chỉ số (Java: ctTanjiro).
    public int CostumeTanjiro { get; set; } = -1;

    // Dữ liệu cải trang Inosuke Hashibira dùng trong bản chụp phục vụ công thức chỉ số (Java: ctInoHashi).
    public int CostumeInosukeHashibira { get; set; } = -1;

    // Dữ liệu cải trang Inosuke dùng trong bản chụp phục vụ công thức chỉ số (Java: ctInosuke).
    public int CostumeInosuke { get; set; } = -1;
}

// Chứa thời điểm hết hạn bùa theo mili giây epoch; 0 nghĩa là chưa từng mua.
public sealed class NPointCharmsFacts
{

    // Dữ liệu TdTriTue dùng trong bản chụp phục vụ công thức chỉ số.
    public long TdTriTue { get; set; }

    // Dữ liệu TdTriTue3 dùng trong bản chụp phục vụ công thức chỉ số.
    public long TdTriTue3 { get; set; }

    // Dữ liệu TdTriTue4 dùng trong bản chụp phục vụ công thức chỉ số.
    public long TdTriTue4 { get; set; }

    // Dữ liệu TdTriTue5 dùng trong bản chụp phục vụ công thức chỉ số.
    public long TdTriTue5 { get; set; }

    // Dữ liệu TdTriTue7 dùng trong bản chụp phục vụ công thức chỉ số.
    public long TdTriTue7 { get; set; }

    // Dữ liệu TdTriTue10 dùng trong bản chụp phục vụ công thức chỉ số.
    public long TdTriTue10 { get; set; }

    // Dữ liệu TdTriTue20 dùng trong bản chụp phục vụ công thức chỉ số.
    public long TdTriTue20 { get; set; }

    // Dữ liệu PetTraining dùng trong bản chụp phục vụ công thức chỉ số (Java: tdDeTu; nghĩa chưa chắc, cần chủ dự án duyệt).
    public long PetTraining { get; set; }

    // Dữ liệu PetTraining2 dùng trong bản chụp phục vụ công thức chỉ số (Java: tdDeTu2; nghĩa chưa chắc, cần chủ dự án duyệt).
    public long PetTraining2 { get; set; }

    // Dữ liệu PetTraining3 dùng trong bản chụp phục vụ công thức chỉ số (Java: tdDeTu3; nghĩa chưa chắc, cần chủ dự án duyệt).
    public long PetTraining3 { get; set; }

    // Dữ liệu PetTraining4 dùng trong bản chụp phục vụ công thức chỉ số (Java: tdDeTu4; nghĩa chưa chắc, cần chủ dự án duyệt).
    public long PetTraining4 { get; set; }

    // Dữ liệu PetTraining5 dùng trong bản chụp phục vụ công thức chỉ số (Java: tdDeTu5; nghĩa chưa chắc, cần chủ dự án duyệt).
    public long PetTraining5 { get; set; }

    // Dữ liệu PetTraining7 dùng trong bản chụp phục vụ công thức chỉ số (Java: tdDeTu7; nghĩa chưa chắc, cần chủ dự án duyệt).
    public long PetTraining7 { get; set; }

    // Dữ liệu PetTraining10 dùng trong bản chụp phục vụ công thức chỉ số (Java: tdDeTu10; nghĩa chưa chắc, cần chủ dự án duyệt).
    public long PetTraining10 { get; set; }

    // Dữ liệu PetTraining20 dùng trong bản chụp phục vụ công thức chỉ số (Java: tdDeTu20; nghĩa chưa chắc, cần chủ dự án duyệt).
    public long PetTraining20 { get; set; }
}

// Chứa dữ liệu Player mà NPoint đọc; host vận hành nạp từ Player shell.
public sealed class NPointPlayerFacts
{

    // Dữ liệu Id dùng trong bản chụp phục vụ công thức chỉ số.
    public long Id { get; set; }

    // Dữ liệu Name dùng trong bản chụp phục vụ công thức chỉ số.
    public string? Name { get; set; }

    // Dữ liệu Gender dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Gender { get; set; }

    // Cho biết đây là người chơi thực theo player.isPl() của Java.
    public bool IsPl { get; set; }

    // Cho biết người chơi thuộc nhóm isPlMan() của Java.
    public bool IsPlMan { get; set; }

    // Gộp bốn cờ bot isBot, isBot_New, isBot_Event và isBot_Valentine như getBot() của Java.
    public bool IsBotAny { get; set; }

    // Dữ liệu IsBoss dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsBoss { get; set; }

    // Dữ liệu IsPet dùng trong bản chụp phục vụ công thức chỉ số (Java: isDeTu).
    public bool IsPet { get; set; }

    // Dữ liệu IsFather dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsFather { get; set; }

    // Dữ liệu IsMother dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsMother { get; set; }

    // Dữ liệu IsBodyClone dùng trong bản chụp phục vụ công thức chỉ số (Java: isPhanThan).
    public bool IsBodyClone { get; set; }

    // Dữ liệu IsMabuMapProtected dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsMabuMapProtected { get; set; }

    // Trạng thái Pet khi người chơi là đệ tử (Java: detuStatus); PetFusionStatus là giá trị đang hợp thể.
    public sbyte PetStatus { get; set; }

    // Loại đệ tử: 0 thường, 1 Ma Bu, 2-4 Sơn Tinh/Thủy Tinh, 5 Bu Nhí/Xên Nhí/Fide Đại Ca.
    public sbyte PetType { get; set; }

    // Dữ liệu WeeklyCard dùng trong bản chụp phục vụ công thức chỉ số.
    public int WeeklyCard { get; set; }

    // Dữ liệu LastWeeklyCardTime dùng trong bản chụp phục vụ công thức chỉ số.
    public long LastWeeklyCardTime { get; set; }

    // Dữ liệu MonthlyCard dùng trong bản chụp phục vụ công thức chỉ số.
    public int MonthlyCard { get; set; }

    // Dữ liệu LastMonthlyCardTime dùng trong bản chụp phục vụ công thức chỉ số.
    public long LastMonthlyCardTime { get; set; }

    // Dữ liệu YearlyCard dùng trong bản chụp phục vụ công thức chỉ số.
    public int YearlyCard { get; set; }

    // Dữ liệu LastYearlyCardTime dùng trong bản chụp phục vụ công thức chỉ số.
    public long LastYearlyCardTime { get; set; }

    // Dữ liệu SupremeCard dùng trong bản chụp phục vụ công thức chỉ số.
    public int SupremeCard { get; set; }

    // Dữ liệu LastSupremeCardTime dùng trong bản chụp phục vụ công thức chỉ số.
    public long LastSupremeCardTime { get; set; }

    // Dữ liệu ClanHpBonusPercent dùng trong bản chụp phục vụ công thức chỉ số (Java: tlHpClanAdd).
    public int ClanHpBonusPercent { get; set; }

    // Dữ liệu ClanMpBonusPercent dùng trong bản chụp phục vụ công thức chỉ số (Java: tlMpClanAdd).
    public int ClanMpBonusPercent { get; set; }

    // Dữ liệu ClanDamageBonusPercent dùng trong bản chụp phục vụ công thức chỉ số (Java: tlDameClanAdd).
    public int ClanDamageBonusPercent { get; set; }

    // Dữ liệu UsesEmperorTitle dùng trong bản chụp phục vụ công thức chỉ số.
    public bool UsesEmperorTitle { get; set; }

    // Dữ liệu LastEmperorTitleTime dùng trong bản chụp phục vụ công thức chỉ số.
    public long LastEmperorTitleTime { get; set; }

    // Dữ liệu TransformationState dùng trong bản chụp phục vụ công thức chỉ số.
    public int TransformationState { get; set; }

    // Dữ liệu Trbne dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte Trbne { get; set; }

    // Dữ liệu CFlag dùng trong bản chụp phục vụ công thức chỉ số.
    public sbyte CFlag { get; set; }

    // Dữ liệu TypeChibi dùng trong bản chụp phục vụ công thức chỉ số.
    public int TypeChibi { get; set; }

    // Dữ liệu Buff dùng trong bản chụp phục vụ công thức chỉ số.
    public Buff Buff { get; set; } = Buff.NONE;

    // Dữ liệu IsAccountProtected dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsAccountProtected { get; set; }

    // Cho biết phiên tồn tại và có Vip_Point dương.
    public bool HasVipPoint { get; set; }

    // Cho biết vệ tinh tồn tại và đang thông minh.
    public bool SatelliteIsIntelligent { get; set; }

    // Cờ dùng xiên cá từ EffectFlagBag (Java: useXienCa).
    public bool UseFishSkewer { get; set; }
}

// Chứa dữ liệu Zone và MapService mà NPoint đọc; Zone null nghĩa là player.zone của Java null.
public sealed class NPointZoneFacts
{

    // Dữ liệu MapId dùng trong bản chụp phục vụ công thức chỉ số.
    public int MapId { get; set; }

    // Loại bản đồ; giá trị 3 không tăng sức mạnh hoặc tiềm năng.
    public int MapType { get; set; }

    // Dữ liệu IsMapBlackBallWar dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsMapBlackBallWar { get; set; }

    // Dữ liệu IsMapCold dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsMapCold { get; set; }

    // Dữ liệu IsMapChristMasEvent dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsMapChristMasEvent { get; set; }

    // Dữ liệu IsMap5000NamTruoc dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsMap5000NamTruoc { get; set; }

    // Dữ liệu IsMapCereal dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsMapCereal { get; set; }

    // Dữ liệu IsInvisibleMap dùng trong bản chụp phục vụ công thức chỉ số (Java: isMapVohinh).
    public bool IsInvisibleMap { get; set; }

    // Dữ liệu IsMapKVTH dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsMapKVTH { get; set; }

    // Dữ liệu IsTreasureUnderSeaMap dùng trong bản chụp phục vụ công thức chỉ số (Java: isMapBanDoKhoBau).
    public bool IsTreasureUnderSeaMap { get; set; }

    // Dữ liệu IsBarracksMap dùng trong bản chụp phục vụ công thức chỉ số (Java: isMapDoanhTrai).
    public bool IsBarracksMap { get; set; }

    // Gộp các vị từ bản đồ Bình hút năng lượng, Địa ngục, Hirudegarn, Potara, Thánh địa và Hành tinh thực vật (Java: isTnsmDividedMap).
    public bool IsPowerPotentialDividedMap { get; set; }
}

// Chứa trường Clan mà công thức tiềm năng cần đọc.
public sealed class NPointClanFacts
{

    // Dữ liệu LasttimeBuffExp dùng trong bản chụp phục vụ công thức chỉ số.
    public long LasttimeBuffExp { get; set; }

    // Dữ liệu TimeStarBuffExp dùng trong bản chụp phục vụ công thức chỉ số.
    public long TimeStarBuffExp { get; set; }

    // Dữ liệu ClanPowerPotentialBonus dùng trong bản chụp phục vụ công thức chỉ số (Java: congTiemNangSucManhToanBangHoi).
    public int ClanPowerPotentialBonus { get; set; }

    // Dữ liệu WisdomCharmExpiry dùng trong bản chụp phục vụ công thức chỉ số (Java: buaTriTue).
    public long WisdomCharmExpiry { get; set; }

    // Dữ liệu Level dùng trong bản chụp phục vụ công thức chỉ số.
    public int Level { get; set; }
}

// Chứa đệ tử hợp thể hoặc đi theo; null khi không có. NPoint chỉ đọc chỉ số sau khi đệ tử đã tính lại (Java: NPointDetuFacts).
public sealed class NPointPetFacts
{

    // Dữ liệu PetType dùng trong bản chụp phục vụ công thức chỉ số (Java: typeDeTu).
    public sbyte PetType { get; set; }

    // Dữ liệu HpMax dùng trong bản chụp phục vụ công thức chỉ số.
    public long HpMax { get; set; }

    // Dữ liệu MpMax dùng trong bản chụp phục vụ công thức chỉ số.
    public long MpMax { get; set; }

    // Dữ liệu Damage dùng trong bản chụp phục vụ công thức chỉ số.
    public long Damage { get; set; }

    // Số ô vật phẩm đang mặc của đệ tử.
    public int BodyItemCount { get; set; }

    // Vật phẩm mặc ở ô 5 của đệ tử; null khi danh sách ngắn hơn.
    public NPointItem? BodySkin { get; set; }
}

// Chứa dữ liệu chủ và bản ghi đệ tử khi người chơi là đệ tử hoặc phân thân.
public sealed class NPointMasterFacts
{

    // Dữ liệu HpMax dùng trong bản chụp phục vụ công thức chỉ số.
    public long HpMax { get; set; }

    // Dữ liệu MpMax dùng trong bản chụp phục vụ công thức chỉ số.
    public long MpMax { get; set; }

    // Dữ liệu Damage dùng trong bản chụp phục vụ công thức chỉ số.
    public long Damage { get; set; }

    // Dữ liệu PetPowerPotentialPercent dùng trong bản chụp phục vụ công thức chỉ số.
    public int PetPowerPotentialPercent { get; set; }

    // Dữ liệu ItemTimeIsEatMeal dùng trong bản chụp phục vụ công thức chỉ số.
    public bool ItemTimeIsEatMeal { get; set; }

    // Dữ liệu ItemTimeIsUsePetPowerPotentialCharm dùng trong bản chụp phục vụ công thức chỉ số.
    public bool ItemTimeIsUsePetPowerPotentialCharm { get; set; }

    // Dữ liệu ItemTimeIsBoneDragon dùng trong bản chụp phục vụ công thức chỉ số.
    public bool ItemTimeIsBoneDragon { get; set; }

    // Dữ liệu WeeklyCard dùng trong bản chụp phục vụ công thức chỉ số.
    public int WeeklyCard { get; set; }

    // Dữ liệu LastWeeklyCardTime dùng trong bản chụp phục vụ công thức chỉ số.
    public long LastWeeklyCardTime { get; set; }

    // Dữ liệu MonthlyCard dùng trong bản chụp phục vụ công thức chỉ số.
    public int MonthlyCard { get; set; }

    // Dữ liệu LastMonthlyCardTime dùng trong bản chụp phục vụ công thức chỉ số.
    public long LastMonthlyCardTime { get; set; }

    // Dữ liệu YearlyCard dùng trong bản chụp phục vụ công thức chỉ số.
    public int YearlyCard { get; set; }

    // Dữ liệu LastYearlyCardTime dùng trong bản chụp phục vụ công thức chỉ số.
    public long LastYearlyCardTime { get; set; }

    // Dữ liệu SupremeCard dùng trong bản chụp phục vụ công thức chỉ số.
    public int SupremeCard { get; set; }

    // Dữ liệu LastSupremeCardTime dùng trong bản chụp phục vụ công thức chỉ số.
    public long LastSupremeCardTime { get; set; }

    // Dữ liệu Charms dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointCharmsFacts Charms { get; } = new();
}

// Chứa kỹ năng người chơi đã chọn từ player.playerSkill.skillSelect.
public sealed class NPointSkillFacts
{
    // Mã template của kỹ năng đã chọn.
    public sbyte TemplateId { get; set; }

    // Phần trăm sát thương so với đòn đánh cơ sở.
    public short Damage { get; set; }

    // Cấp của kỹ năng đã chọn.
    public int Point { get; set; }
}

// Chứa giá trị toàn máy chủ từ static và manager Java; thuộc tính thiếu hoặc hết hạn có giá trị 0 và không hoạt động.
public sealed class NPointServerFacts
{
    // Cờ Manager.readInt giới hạn Util.CrisGH trong miền int.
    public bool ReadInt { get; set; } = true;

    // Hệ số kinh nghiệm máy chủ.
    public int RateExpServer { get; set; } = 10;

    // Cờ sự kiện Quốc tế Phụ nữ.
    public bool InternationalWomansDay { get; set; }

    // Dữ liệu WomansDayHp dùng trong bản chụp phục vụ công thức chỉ số.
    public int WomansDayHp { get; set; }

    // Dữ liệu WomansDayMp dùng trong bản chụp phục vụ công thức chỉ số.
    public int WomansDayMp { get; set; }

    // Dữ liệu WomansDayDame dùng trong bản chụp phục vụ công thức chỉ số.
    public int WomansDayDame { get; set; }

    // Kết quả tra thuộc tính máy chủ HP, KI, DAMAGE và POWER_POTENTIAL; chỉ hiệu lực khi tồn tại và chưa hết hạn.
    public bool AttributeHpActive { get; set; }

    // Dữ liệu AttributeHpValue dùng trong bản chụp phục vụ công thức chỉ số.
    public int AttributeHpValue { get; set; }

    // Dữ liệu AttributeKiActive dùng trong bản chụp phục vụ công thức chỉ số.
    public bool AttributeKiActive { get; set; }

    // Dữ liệu AttributeKiValue dùng trong bản chụp phục vụ công thức chỉ số.
    public int AttributeKiValue { get; set; }

    // Dữ liệu AttributeSucDanhActive dùng trong bản chụp phục vụ công thức chỉ số.
    public bool AttributeSucDanhActive { get; set; }

    // Dữ liệu AttributeSucDanhValue dùng trong bản chụp phục vụ công thức chỉ số.
    public int AttributeSucDanhValue { get; set; }

    // Dữ liệu AttributeTnsmActive dùng trong bản chụp phục vụ công thức chỉ số.
    public bool AttributeTnsmActive { get; set; }

    // Dữ liệu AttributeTnsmValue dùng trong bản chụp phục vụ công thức chỉ số.
    public int AttributeTnsmValue { get; set; }

    // Danh sách mã người chơi TopKillWhis, xếp tốt nhất trước.
    public List<long> WhisPlayerIds { get; set; } = [];
}

// Chứa kết quả tra InventoryService cộng vào chỉ số cơ sở; chỉ có ý nghĩa với người chơi thực.
public sealed class NPointInventoryFacts
{
    // Cho biết người chơi có vật phẩm Rồng Nhí.
    public bool HasRongNhi { get; set; }

    // Dữ liệu HpItemsInBox dùng trong bản chụp phục vụ công thức chỉ số.
    public int HpItemsInBox { get; set; }

    // Dữ liệu MpItemsInBox dùng trong bản chụp phục vụ công thức chỉ số.
    public int MpItemsInBox { get; set; }

    // Dữ liệu DamageItemsInBox dùng trong bản chụp phục vụ công thức chỉ số.
    public int DamageItemsInBox { get; set; }

    // Dữ liệu CritItemsInBox dùng trong bản chụp phục vụ công thức chỉ số.
    public int CritItemsInBox { get; set; }

    // Danh sách vật phẩm đang mặc; độ dài quan trọng vì công thức truy cập ô 5 hoặc 6.
    public List<NPointItem> ItemsBody { get; set; } = [];

    // Danh sách vật phẩm trong túi.
    public List<NPointItem> ItemsBag { get; set; } = [];

    // Giáp tập luyện được NPoint chọn và host ghi trả vào inventory.
    public NPointItem? TrainingArmor { get; set; }
}

// Gom toàn bộ dữ liệu NPoint đọc trong một lần tính; INPointHost.Capture xây dựng bản chụp này.
public sealed class NPointFacts
{
    // Hằng trạng thái hợp thể Pet (Java: Detu.FUSION).
    public const sbyte PetFusionStatus = 4;

    // Dữ liệu Player dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointPlayerFacts Player { get; } = new();

    // Dữ liệu ItemTime dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointItemTimeFacts ItemTime { get; } = new();

    // Dữ liệu EffectSkill dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointEffectSkillFacts EffectSkill { get; } = new();

    // Dữ liệu EffectSkin dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointEffectSkinFacts EffectSkin { get; } = new();

    // Dữ liệu SetClothes dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointSetClothesFacts SetClothes { get; } = new();

    // Dữ liệu Charms dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointCharmsFacts Charms { get; } = new();

    // Dữ liệu Inventory dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointInventoryFacts Inventory { get; } = new();

    // Dữ liệu Server dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointServerFacts Server { get; } = new();

    // Zone của người chơi; null khi người chơi chưa ở zone nào.
    public NPointZoneFacts? Zone { get; set; }

    // Bang hội của người chơi; null khi chưa có.
    public NPointClanFacts? Clan { get; set; }

    // Pet của người chơi; null khi chưa có (Java: detu).
    public NPointPetFacts? Pet { get; set; }

    // Chủ của người chơi khi là đệ tử hoặc phân thân; null trong trường hợp khác.
    public NPointMasterFacts? Master { get; set; }

    // Kiểu hợp thể của người chơi.
    public sbyte TypeFusion { get; set; }

    // Sát thương cộng từ PointFusion (Java: dameFusion).
    public int FusionDamage { get; set; }

    // Thời hạn quà Ngọc Rồng; có bảy phần tử tương ứng sao 1 đến 7.
    public long[] BlackBallTimeOut { get; set; } = new long[7];

    // Danh sách thẻ ra-đa của người chơi.
    public List<NPointCard> Cards { get; set; } = [];

    // Tùy chọn của huy hiệu đang dùng; rỗng khi không có huy hiệu.
    public List<NPointOption> BadgeOptions { get; set; } = [];

    // Nội tại hiện thời của người chơi.
    public NPointIntrinsic Intrinsic { get; set; } = new();

    // Kỹ năng đang chọn; null khi chưa chọn kỹ năng.
    public NPointSkillFacts? SkillSelect { get; set; }
}

// Chứa dữ liệu getDameAttack đọc; nhỏ hơn NPointFacts vì chỉ dùng cho từng đòn đánh.
public sealed class NPointAttackFacts
{

    // Dữ liệu SkillSelect dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointSkillFacts SkillSelect { get; set; } = new();

    // Dữ liệu Intrinsic dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointIntrinsic Intrinsic { get; set; } = new();

    // Dữ liệu SetClothes dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointSetClothesFacts SetClothes { get; } = new();

    // Dữ liệu EffectSkill dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointEffectSkillFacts EffectSkill { get; } = new();

    // Dữ liệu EffectSkin dùng trong bản chụp phục vụ công thức chỉ số.
    public NPointEffectSkinFacts EffectSkin { get; } = new();

    // Cho biết người chơi là Pet (Java: isDeTu).
    public bool IsPet { get; set; }

    // Thời hạn bùa đệ tử của chủ; bằng 0 khi người chơi không phải đệ tử (Java: master.charms.tdDeTu; nghĩa chưa chắc, cần chủ dự án duyệt).
    public long MasterPetCharmTraining { get; set; }

    // Cờ Manager.readInt dùng cùng ý nghĩa với NPointServerFacts.ReadInt.
    public bool ReadInt { get; set; } = true;

    // Tổng HP quái sống quanh người tung Quả cầu kênh khí trong bán kính 350 + 30*cấp; host tính từ zone.mobs và zone.getHumanoids, kỹ năng khác giữ 0.
    public long AroundMobHp { get; set; }

    // Dữ liệu AroundPlayerHp dùng trong bản chụp phục vụ công thức chỉ số.
    public long AroundPlayerHp { get; set; }
}

// Chứa dữ liệu update đọc mỗi tick: trạng thái nạp EffectSkill và các cờ đệ tử/boss.
public sealed class NPointUpdateFacts
{
    // Cho biết EffectSkill của người chơi tồn tại.
    public bool HasEffectSkill { get; set; } = true;

    // Dữ liệu IsCharging dùng trong bản chụp phục vụ công thức chỉ số.
    public bool IsCharging { get; set; }

    // Dữ liệu CountCharging dùng trong bản chụp phục vụ công thức chỉ số.
    public int CountCharging { get; set; }

    // Kết quả isHaveEffectSkill của EffectSkill.
    public bool IsHaveEffectSkill { get; set; }

    // Cho biết người chơi đã chọn kỹ năng.
    public bool HasSkillSelect { get; set; } = true;

    // Cấp của kỹ năng đang chọn.
    public int SkillSelectPoint { get; set; }

    // Cờ Manager.readInt dùng cùng ý nghĩa với NPointServerFacts.ReadInt.
    public bool ReadInt { get; set; } = true;
}
