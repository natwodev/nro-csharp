// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/PlayerEvent.java (dòng 1–395).
namespace Nro.Game.Players.Gifts;

// Lưu tiến độ và trạng thái các sự kiện của người chơi. (Java: PlayerEvent)
public class PlayerEvent
{
    // Điểm Hủy Diệt của người chơi. (Java: HakaiPoint)
    public int DestructionPoints { get; set; }

    // Điểm tranh ngọc Namek của người chơi. (Java: NamekWarPoint)
    public int NamekWarPoints { get; set; }

    // Điểm sự kiện Tết của người chơi. (Java: LunaNewYearPoint)
    public int LunarNewYearEventPoints { get; set; }

    // Điểm sự kiện Giáng Sinh của người chơi. (Java: ChristMasPoint)
    public int ChristmasEventPoints { get; set; }

    // Điểm sự kiện Halloween của người chơi. (Java: HalloweenPoint)
    public int HalloweenEventPoints { get; set; }

    // Điểm sự kiện Quốc tế Phụ nữ của người chơi. (Java: InternationalWomensDayPoint)
    public int InternationalWomensDayEventPoints { get; set; }

    // Điểm sự kiện Trung Thu của người chơi. (Java: TrungThuPoint)
    public int MoonFestivalEventPoints { get; set; }

    // Điểm sự kiện Hùng Vương của người chơi. (Java: HungVuongPoint)
    public int HungKingEventPoints { get; set; }

    // Điểm sự kiện Black Friday của người chơi. (Java: BlackFridayPoint)
    public int BlackFridayEventPoints { get; set; }

    // Điểm sự kiện Phụ nữ Việt Nam của người chơi. (Java: Day20_10Point)
    public int VietnameseWomensDayEventPoints { get; set; }

    // Xác nhận đã dùng bồn tắm. (Java: isUseBonTam; nghĩa chưa chắc, cần chủ dự án duyệt)
    public bool IsBathUsed { get; set; }

    // Điểm tích lũy của người chơi. (Java: diemTichLuy)
    public int AccumulatedPoints { get; set; }

    // Mốc nạp đã nhận của người chơi. (Java: mocNapDaNhan)
    public int ReceivedTopUpMilestone { get; set; }

    // Điểm sự kiện tổng quát của người chơi. (Java: eventPoint)
    public int EventPoints { get; set; }

    // Điểm sự kiện boss của người chơi. (Java: eventPointBHM)
    public int BossEventPoints { get; set; }

    // Điểm sự kiện Ngũ Hành Sơn của người chơi. (Java: eventPointNHS)
    public int NguHanhSonEventPoints { get; set; }

    // Điểm sự kiện săn quái của người chơi. (Java: eventPointQuai)
    public int MonsterHuntEventPoints { get; set; }

    // Điểm sự kiện Quy Lão Kame của người chơi. (Java: eventPointQuyLao)
    public int KameElderEventPoints { get; set; }

    // Điểm mốc sự kiện của người chơi. (Java: eventPointMoc)
    public int EventMilestonePoints { get; set; }

    // Người chơi sở hữu tiến độ sự kiện này. (Java: player)
    public Player Player { get; set; }

    // Xác nhận đang nấu bánh chưng. (Java: cookingChungCake)
    public bool IsCookingSquareRiceCake { get; set; }

    // Thời gian nấu bánh chưng. (Java: timeCookChungCake)
    public int SquareRiceCakeCookTime { get; set; }

    // Xác nhận đang nấu bánh tét. (Java: cookingTetCake)
    public bool IsCookingCylindricalRiceCake { get; set; }

    // Thời gian nấu bánh tét. (Java: timeCookTetCake)
    public int CylindricalRiceCakeCookTime { get; set; }

    // Xác nhận đã nhận lì xì. (Java: receivedLuckyMoney)
    public bool IsLuckyMoneyReceived { get; set; }

    // Gắn trạng thái sự kiện vào người chơi. (Java: PlayerEvent)
    public PlayerEvent(Player player)
    {
        Player = player;
    }

    // Cộng điểm Hủy Diệt mà không giới hạn biên như Java. (Java: addHakaiPoint)
    public void AddDestructionPoints(int points) => DestructionPoints = unchecked(DestructionPoints + points);

    // Trừ điểm Hủy Diệt mà không giới hạn biên như Java. (Java: subHakaiPoint)
    public void SubtractDestructionPoints(int points) => DestructionPoints = unchecked(DestructionPoints - points);

    // Cộng điểm tranh ngọc Namek mà không giới hạn biên như Java. (Java: addNamekWarPoint)
    public void AddNamekWarPoints(int points) => NamekWarPoints = unchecked(NamekWarPoints + points);

    // Trừ điểm tranh ngọc Namek mà không giới hạn biên như Java. (Java: subNamekWarPoint)
    public void SubtractNamekWarPoints(int points) => NamekWarPoints = unchecked(NamekWarPoints - points);

    // Cộng điểm sự kiện Tết mà không giới hạn biên như Java. (Java: addLunaNewYearPoint)
    public void AddLunarNewYearEventPoints(int points) => LunarNewYearEventPoints = unchecked(LunarNewYearEventPoints + points);

    // Trừ điểm sự kiện Tết mà không giới hạn biên như Java. (Java: subLunaNewYearPoint)
    public void SubtractLunarNewYearEventPoints(int points) => LunarNewYearEventPoints = unchecked(LunarNewYearEventPoints - points);

    // Cộng điểm sự kiện Giáng Sinh mà không giới hạn biên như Java. (Java: addChristMasPoint)
    public void AddChristmasEventPoints(int points) => ChristmasEventPoints = unchecked(ChristmasEventPoints + points);

    // Trừ điểm sự kiện Giáng Sinh mà không giới hạn biên như Java. (Java: subChristMasPoint)
    public void SubtractChristmasEventPoints(int points) => ChristmasEventPoints = unchecked(ChristmasEventPoints - points);

    // Cộng điểm sự kiện Halloween mà không giới hạn biên như Java. (Java: addHalloweenPoint)
    public void AddHalloweenEventPoints(int points) => HalloweenEventPoints = unchecked(HalloweenEventPoints + points);

    // Trừ điểm sự kiện Halloween mà không giới hạn biên như Java. (Java: subHalloweenPoint)
    public void SubtractHalloweenEventPoints(int points) => HalloweenEventPoints = unchecked(HalloweenEventPoints - points);

    // Cộng điểm sự kiện Quốc tế Phụ nữ mà không giới hạn biên như Java. (Java: addInternationalWomensDayPoint)
    public void AddInternationalWomensDayEventPoints(int points) => InternationalWomensDayEventPoints = unchecked(InternationalWomensDayEventPoints + points);

    // Trừ điểm sự kiện Quốc tế Phụ nữ mà không giới hạn biên như Java. (Java: subInternationalWomensDayPoint)
    public void SubtractInternationalWomensDayEventPoints(int points) => InternationalWomensDayEventPoints = unchecked(InternationalWomensDayEventPoints - points);

    // Cộng điểm sự kiện Trung Thu mà không giới hạn biên như Java. (Java: addTrungThuPoint)
    public void AddMoonFestivalEventPoints(int points) => MoonFestivalEventPoints = unchecked(MoonFestivalEventPoints + points);

    // Trừ điểm sự kiện Trung Thu mà không giới hạn biên như Java. (Java: subTrungThuPoint)
    public void SubtractMoonFestivalEventPoints(int points) => MoonFestivalEventPoints = unchecked(MoonFestivalEventPoints - points);

    // Cộng điểm sự kiện Hùng Vương mà không giới hạn biên như Java. (Java: addHungVuongPoint)
    public void AddHungKingEventPoints(int points) => HungKingEventPoints = unchecked(HungKingEventPoints + points);

    // Trừ điểm sự kiện Hùng Vương mà không giới hạn biên như Java. (Java: subHungVuongPoint)
    public void SubtractHungKingEventPoints(int points) => HungKingEventPoints = unchecked(HungKingEventPoints - points);

    // Cộng điểm sự kiện Black Friday mà không giới hạn biên như Java. (Java: addBlackFridayPoint)
    public void AddBlackFridayEventPoints(int points) => BlackFridayEventPoints = unchecked(BlackFridayEventPoints + points);

    // Trừ điểm sự kiện Black Friday mà không giới hạn biên như Java. (Java: subBlackFridayPoint)
    public void SubtractBlackFridayEventPoints(int points) => BlackFridayEventPoints = unchecked(BlackFridayEventPoints - points);

    // Cộng điểm sự kiện Phụ nữ Việt Nam mà không giới hạn biên như Java. (Java: add20_10Point)
    public void AddVietnameseWomensDayEventPoints(int points) => VietnameseWomensDayEventPoints = unchecked(VietnameseWomensDayEventPoints + points);

    // Trừ điểm sự kiện Phụ nữ Việt Nam mà không giới hạn biên như Java. (Java: sub20_10Point)
    public void SubtractVietnameseWomensDayEventPoints(int points) => VietnameseWomensDayEventPoints = unchecked(VietnameseWomensDayEventPoints - points);

    // Cộng điểm sự kiện tổng quát mà không giới hạn biên như Java. (Java: addEventPoint)
    public void AddEventPoints(int points) => EventPoints = unchecked(EventPoints + points);

    // Trừ điểm sự kiện tổng quát mà không giới hạn biên như Java. (Java: subEventPoint)
    public void SubtractEventPoints(int points) => EventPoints = unchecked(EventPoints - points);

    // Cộng điểm sự kiện boss mà không giới hạn biên như Java. (Java: addEventPointBHM)
    public void AddBossEventPoints(int points) => BossEventPoints = unchecked(BossEventPoints + points);

    // Trừ điểm sự kiện boss mà không giới hạn biên như Java. (Java: subEventPointBHM)
    public void SubtractBossEventPoints(int points) => BossEventPoints = unchecked(BossEventPoints - points);

    // Cộng điểm sự kiện Ngũ Hành Sơn mà không giới hạn biên như Java. (Java: addEventPointNHS)
    public void AddNguHanhSonEventPoints(int points) => NguHanhSonEventPoints = unchecked(NguHanhSonEventPoints + points);

    // Trừ điểm sự kiện Ngũ Hành Sơn mà không giới hạn biên như Java. (Java: subEventPointNHS)
    public void SubtractNguHanhSonEventPoints(int points) => NguHanhSonEventPoints = unchecked(NguHanhSonEventPoints - points);

    // Cộng điểm sự kiện săn quái mà không giới hạn biên như Java. (Java: addEventPointQuai)
    public void AddMonsterHuntEventPoints(int points) => MonsterHuntEventPoints = unchecked(MonsterHuntEventPoints + points);

    // Trừ điểm sự kiện săn quái mà không giới hạn biên như Java. (Java: subEventPointQuai)
    public void SubtractMonsterHuntEventPoints(int points) => MonsterHuntEventPoints = unchecked(MonsterHuntEventPoints - points);

    // Cộng điểm sự kiện Quy Lão Kame mà không giới hạn biên như Java. (Java: addEventPointQuyLao)
    public void AddKameElderEventPoints(int points) => KameElderEventPoints = unchecked(KameElderEventPoints + points);

    // Trừ điểm sự kiện Quy Lão Kame mà không giới hạn biên như Java. (Java: subEventPointQuyLao)
    public void SubtractKameElderEventPoints(int points) => KameElderEventPoints = unchecked(KameElderEventPoints - points);

    // Cộng điểm mốc sự kiện mà không giới hạn biên như Java. (Java: addEventPointMoc)
    public void AddEventMilestonePoints(int points) => EventMilestonePoints = unchecked(EventMilestonePoints + points);

    // Trừ điểm mốc sự kiện mà không giới hạn biên như Java. (Java: subEventPointMoc)
    public void SubtractEventMilestonePoints(int points) => EventMilestonePoints = unchecked(EventMilestonePoints - points);

    // Duy trì hợp đồng cập nhật rỗng của Java. (Java: update)
    public void Update()
    {
    }
}
