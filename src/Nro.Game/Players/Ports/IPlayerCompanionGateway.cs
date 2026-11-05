using Nro.Game.Players.Companions;

namespace Nro.Game.Players.Ports;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/PetFollow.java (dòng 42–114), nro/player/SaiBaMen.java (dòng 40–120).
// Cổng cho side effect map, gói tin và combat của companion; Zone/MapService/Service Java không được gọi trực tiếp.
public interface IPlayerCompanionGateway
{
    // Cổng chính sách cũ giữ lại tương thích; PetFollow không dùng để từ chối vì Java dùng chuỗi OR các loại map (Java: PetFollow.update 76–83).
    bool CanJoinPetFollowMasterZone(Player master);

    // Sáu phân loại MapService được PetFollow dùng nguyên văn trong biểu thức OR Java.
    bool IsOfflineMap(int mapId);
    bool IsClanMap(int mapId);
    bool IsPotaufeuMap(int mapId);
    bool IsWarMap(int mapId);
    bool IsTestDamageMap(int mapId);
    bool IsContestedNamekMap(int mapId);

    // Đưa companion vào Zone hiện tại của chủ và phát dữ liệu join map (Java: ChangeMapService.goToMap, Zone.load_Me_To_Another).
    void JoinMasterZone(Player companion, Player master);

    // Gỡ companion ra khỏi map hiện tại (Java: ChangeMapService.exitMap).
    void ExitMap(Player companion);

    // Di chuyển companion và phát gói di chuyển trong map (Java: PlayerService.playerMove).
    void Move(Player companion, int x, int y);

    // Hồi sinh companion với HP và KI đầy (Java: Service.hsChar).
    void Respawn(Player companion, long hp, long mp);

    // Phát hiệu ứng chuẩn bị tự hủy Saibamen cho mọi người chơi trong map (Java: Message -45, Service.sendMessAllPlayerInMap).
    void BroadcastSaiBaMenSelfDestruct(SaiBaMen saibamen, short effectId, short durationMilliseconds);

    // Cho Saibamen chết và gây sát thương vùng, gồm mọi gói HP/mission cần thiết (Java: setDie, injured, sendInfoHpMpMoney, Send_Info_NV).
    void ExplodeSaiBaMen(SaiBaMen saibamen, double damage);
}
