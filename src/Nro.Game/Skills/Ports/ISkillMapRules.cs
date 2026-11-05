namespace Nro.Game.Skills.Ports;

// Nguồn Java: nro/services/MapService.java (dòng 438–445).
// Cổng tra cứu phân loại map mà quy tắc kỹ năng cần, không phụ thuộc MapService singleton Java.
public interface ISkillMapRules
{
    // Kiểm tra map ngoại tuyến. (Java: isMapOffline)
    bool IsMapOffline(int mapId);
}
