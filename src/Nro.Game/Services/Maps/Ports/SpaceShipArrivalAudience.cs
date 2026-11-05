namespace Nro.Game.Services.Maps.Ports;

// Phạm vi nhận gói -65 giữ nguyên typeSendMSG Java.
// Nguồn Java: source-game/Teamobi2026/SRC/src/nro/models/map/service/ChangeMapService.java (dòng 747–756).
public enum SpaceShipArrivalAudience : sbyte
{
    // Gửi tất cả người chơi trong map (Java: case 0).
    AllInMap = 0,

    // Chỉ gửi chính người chơi (Java: case 1).
    Self = 1,

    // Gửi người khác trong map (Java: case 2).
    OthersInMap = 2,
}
