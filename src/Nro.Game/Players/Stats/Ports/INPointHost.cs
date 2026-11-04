namespace Nro.Game.Players.Stats.Ports;

// Định nghĩa cầu nối giữa NPoint và phần còn lại của trò chơi: đọc bản chụp cộng tác viên và ghi gói tin, nhiệm vụ, chat hay trường Java thay đổi ở đối tượng khác.
// Nguồn Java: nro/player/NPoint.java (thay khoảng mười singleton Service.gI(), PlayerService.gI(), MapService.gI() và trường công khai của nhiều track; nhờ đó NPoint chỉ còn số học để kiểm thử độc lập. Theo D1, mọi lời gọi chạy trong tick của zone chứa người chơi, không có thread riêng).

public interface INPointHost
{
    // ---- reads ----

    // Chụp mọi dữ liệu cần cho CalPoint, SetBasePoint và CalculatePowerPotential, đúng một lần mỗi phép tính.
    NPointFacts Capture(NPoint npoint);

    // Ghi trả SetClothes.Worldcup và Inventory.TrainingArmor mà phép tính đã thay đổi.
    void Commit(NPoint npoint, NPointFacts facts);

    // Chụp dữ liệu GetDamageAttack cần; với Quả cầu kênh khí còn nạp HP quái và người chơi xung quanh.
    NPointAttackFacts CaptureAttack(NPoint npoint);

    // Ghi trả các cờ EffectSkin GetDamageAttack thay đổi sau đòn Chưởng nạp: IsDamageBoosted, IsChargedBlast và LastTimeChargedBlast.
    void CommitAttack(NPoint npoint, NPointAttackFacts facts);

    // Chụp trạng thái Update cần cho mỗi tick.
    NPointUpdateFacts CaptureUpdate(NPoint npoint);

    // Lấy ngưỡng PowerLimit của cấp; trả null khi cấp không có bản ghi.
    PowerLimitInfo? GetPowerLimit(int limitPower);

    // ---- writes ----

    // Đặt người chơi chết như player.setDie() Java.
    void SetDie(Player player);

    // Hồi HP và MP qua PlayerService Java.
    void RestoreHpMp(Player player, long hp, long mp);

    // Gửi thể lực hiện tại qua PlayerService Java.
    void SendCurrentStamina(Player player);

    // Gửi chat cho người chơi qua Service Java.
    void Chat(Player player, string text);

    // Dừng nạp năng lượng qua EffectSkillService Java.
    void StopCharge(Player player);

    // Tăng CountCharging và trả giá trị mới; StopCharge có thể đã đặt lại bộ đếm giữa các lần gọi.
    int IncrementCountCharging(Player player);

    // Xóa lượt nạp cũ không gửi gói: IsCharging=false, CountCharging=0; chỉ dùng khi đang nạp nhưng chưa chọn kỹ năng.
    void ClearCharging(Player player);

    // Gửi thông báo thường qua Service Java.
    void SendNotification(Player player, string text);

    // Gửi hộp thông báo xác nhận qua Service Java.
    void SendNotificationOk(Player player, string text);

    // Gửi thông tin tiền qua Service Java.
    void SendMoney(Player player);

    // Gửi bảng chỉ số qua Service Java.
    void SendPoint(Player player);

    // Gửi thông tin đệ tử gốc qua Service Java.
    void SendBasePetInfo(Player player);

    // Hiển thị thông tin đệ tử qua Service Java.
    void ShowInfoPet(Player player);

    // Kiểm tra hoàn thành nhiệm vụ sức mạnh qua TaskService Java.
    void CheckDoneTaskPower(Player player, long power);

    // Kiểm tra hoàn thành nhiệm vụ dùng tiềm năng qua TaskService Java.
    void CheckPotentialUseTaskCompletion(Player player);
}
