using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.IO;

namespace Nro.Game.Effects.Services;

// Gửi các hiệu ứng bản đồ và lớp phủ đóng băng; singleton gI() Java trở thành lớp thường do EffectEnvironment.Create tạo và gói đi qua IEffectBroadcaster.
// Nguồn Java: nro/effect/EffectMapService.java (KHÁC BIỆT CÓ CHỦ ĐÍCH: Java nuốt lỗi trong catch (Exception), C# vẫn không làm lỗi gói dừng tick nhưng ghi log theo D2/L07; giữ id ghi byte thứ nhất lẫn thứ ba, bỏ qua loop, và khi gỡ gửi cho người chơi rồi mọi người trực tuyến).
public sealed class EffectMapService : IEffectMapService
{
    private readonly IEffectBroadcaster broadcaster;
    private readonly GameLogger logger;

    public EffectMapService(IEffectBroadcaster broadcaster, GameLogger logger)
    {
        ArgumentNullException.ThrowIfNull(broadcaster);
        ArgumentNullException.ThrowIfNull(logger);
        this.broadcaster = broadcaster;
        this.logger = logger;
    }

    // Cài đặt SendEffectMapToPlayer theo hợp đồng dịch vụ hiệu ứng.
    public void SendEffectMapToPlayer(Player player, int id, int layer, int loop, int x, int y, int delay)
    {
        try
        {
            var msg = new Message(113);
            msg.Writer().WriteByte(id);
            msg.Writer().WriteByte(layer);
            msg.Writer().WriteByte(id);
            msg.Writer().WriteShort(x);
            msg.Writer().WriteShort(y);
            msg.Writer().WriteShort(delay);
            broadcaster.SendToPlayer(player, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            logger.LogException(typeof(EffectMapService), e);
        }
    }

    // Cài đặt SendFrozenCharacterEffect theo hợp đồng dịch vụ hiệu ứng (Java: sendCharEffect_HoaBang).
    public void SendFrozenCharacterEffect(Player player)
    {
        try
        {
            var msg = new Message(-128);
            msg.Writer().WriteByte(0);
            msg.Writer().WriteInt(unchecked((int)player.Id));
            msg.Writer().WriteShort(202);
            msg.Writer().WriteByte(1);
            msg.Writer().WriteByte(-1);
            msg.Writer().WriteShort(10);
            msg.Writer().WriteByte(0);
            broadcaster.SendToZoneOf(player, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            logger.LogException(typeof(EffectMapService), e);
        }
    }

    // Cài đặt RemoveFrozenCharacterEffect theo hợp đồng dịch vụ hiệu ứng (Java: Remove_sendCharEffect_HoaBang).
    public void RemoveFrozenCharacterEffect(Player player)
    {
        try
        {
            var msg = new Message(-128);
            msg.Writer().WriteByte(1);
            msg.Writer().WriteInt(unchecked((int)player.Id));
            msg.Writer().WriteShort(202);
            msg.Writer().WriteByte(1);
            msg.Writer().WriteByte(-1);
            msg.Writer().WriteShort(10);
            msg.Writer().WriteByte(1);
            broadcaster.SendToPlayer(player, msg);
            broadcaster.SendToAll(msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            logger.LogException(typeof(EffectMapService), e);
        }
    }

    // Cài đặt SendEffectMapToAllInMap theo hợp đồng dịch vụ hiệu ứng.
    public void SendEffectMapToAllInMap(Zone zone, int id, int layer, int loop, int x, int y, int delay)
    {
        try
        {
            var msg = new Message(113);
            msg.Writer().WriteByte(loop);
            msg.Writer().WriteByte(layer);
            msg.Writer().WriteByte(id);
            msg.Writer().WriteShort(x);
            msg.Writer().WriteShort(y);
            msg.Writer().WriteShort(delay);
            broadcaster.SendToZone(zone, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            logger.LogException(typeof(EffectMapService), e);
        }
    }

    // Cài đặt SendEffectMapToAllInMap theo hợp đồng dịch vụ hiệu ứng.
    public void SendEffectMapToAllInMap(Player player, int id, int layer, int loop, int x, int y, int delay)
    {
        Message? msg = null;
        try
        {
            msg = new Message(113);
            msg.Writer().WriteByte(loop);
            msg.Writer().WriteByte(layer);
            msg.Writer().WriteByte(id);
            msg.Writer().WriteShort(x);
            msg.Writer().WriteShort(y);
            msg.Writer().WriteShort(delay);
            broadcaster.SendToZoneOf(player, msg);
        }
        catch (Exception e)
        {
            logger.LogException(typeof(EffectMapService), e);
        }
        finally
        {
            msg?.Cleanup();
        }
    }
}
