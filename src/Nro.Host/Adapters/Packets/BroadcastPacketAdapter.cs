using Nro.Core.IO;
using Nro.Game.Effects.Ports;
using Nro.Game.Players;
using Nro.Game.World.Maps;
using Nro.Network.Packets.Players;

namespace Nro.Host.Adapters.Packets;

// Cài đặt broadcast Service bằng Player.SendMessage; session null/đóng bị bỏ qua như Java.
// Nguồn Java: nro/services/Service.java (dòng 119–145, 417–513).
public sealed class BroadcastPacketAdapter : IEffectBroadcaster
{
    // Registry online do composition root Host cấp (Java: Client.getPlayers).
    private readonly Func<IEnumerable<Player>> _onlinePlayers;

    // Cách lấy Zone của mob mà không làm cổng Effects phụ thuộc lớp Mob thật (Java: mob.zone).
    private readonly Func<IEffectMobTarget, Zone?> _mobZone;

    // Registry do Host cung cấp; không tạo singleton Service hay tham chiếu Game từ Network.
    public BroadcastPacketAdapter(Func<IEnumerable<Player>> onlinePlayers, Func<IEffectMobTarget, Zone?> mobZone)
    {
        _onlinePlayers = onlinePlayers ?? throw new ArgumentNullException(nameof(onlinePlayers));
        _mobZone = mobZone ?? throw new ArgumentNullException(nameof(mobZone));
    }

    // Port addEffectChar: giữ idEffChar duy nhất rồi phát cmd -128/0 tới map của player.
    public void AddCharacterEffect(Player player, int effectId, int layer, int loop, int loopCount, int stand)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (!player.IdEffChar.Contains(effectId))
        {
            player.IdEffChar.Add(effectId);
        }

        Message message = CharacterEffectPacketCodec.Encode(new CharacterEffectPacket(unchecked((int)player.Id), effectId, layer, loop, loopCount, stand));
        SendToZoneOf(player, message);
        message.Cleanup();
    }

    // Gửi cho một player; Player.SendMessage tự bỏ khi session/cổng thiếu (Java: player.sendMessage).
    public void SendToPlayer(Player player, Message message)
    {
        if (player is not null && message is not null)
        {
            player.SendMessage(message);
        }
    }

    // Nhánh Service.sendMessAllPlayerInMap(Player,msg): map offline chỉ gửi self/training target, map thường gửi Players theo thứ tự tăng.
    public void SendToZoneOf(Player player, Message message)
    {
        if (player?.Zone is null || message is null)
        {
            message?.Dispose();
            return;
        }

        Player[] players = player.Zone.Players.ToArray();
        if (player.Zone.Runtime.Rules.IsMapOffline(player.Zone.Map.MapId))
        {
            bool isNonInteractiveNpc = player is INonInteractiveNpcPlayer;
            bool isTrainingBoss = IsTrainingBossOfAnyPlayer(player, players);
            if (!isNonInteractiveNpc && !isTrainingBoss)
            {
                SendToPlayer(player, message);
                message.Cleanup();
                return;
            }

            if (players.Length == 0)
            {
                message.Dispose();
                return;
            }

            foreach (Player candidate in players)
            {
                if (candidate is not null && (isNonInteractiveNpc || player.Zone.Runtime.Players.IsTrainingBossOf(player, candidate)))
                {
                    SendToPlayer(candidate, message);
                }
            }

            message.Cleanup();
            return;
        }

        if (players.Length == 0)
        {
            message.Dispose();
            return;
        }

        SendForward(players, message, candidate => candidate.IsPl());
        message.Cleanup();
    }

    // Nhánh Service.sendMessAllPlayerInMap(Zone,msg): snapshot Players và thứ tự giảm n-1..0.
    public void SendToZone(Zone zone, Message message)
    {
        if (zone is null || message is null)
        {
            message?.Dispose();
            return;
        }

        Player[] players = zone.Players.ToArray();
        if (players.Length == 0)
        {
            message.Dispose();
            return;
        }

        for (int i = players.Length - 1; i >= 0; i--)
        {
            SendToPlayer(players[i], message);
        }

        message.Cleanup();
    }

    // Phát theo zone do Host giải từ IEffectMobTarget; interface effect không kéo kiểu Mob/Zone vào Game (Java: mob.zone).
    public void SendToZoneOfMob(IEffectMobTarget mob, Message message)
    {
        if (mob is not null && message is not null)
        {
            Zone? zone = _mobZone(mob);
            if (zone is not null)
            {
                SendToZone(zone, message);
            }
        }
    }

    // Nhánh Service.sendMessAnotherNotMeInMap: map offline chỉ có training/NPC, map thường loại chính player và chỉ gửi người chơi thật.
    public void SendToZoneExcept(Player player, Message message)
    {
        if (player?.Zone is null || message is null)
        {
            message?.Cleanup();
            return;
        }

        Player[] players = player.Zone.Players.ToArray();
        if (player.Zone.Runtime.Rules.IsMapOffline(player.Zone.Map.MapId))
        {
            bool isNonInteractiveNpc = player is INonInteractiveNpcPlayer;
            bool isTrainingBoss = IsTrainingBossOfAnyPlayer(player, players);
            if (!isNonInteractiveNpc && !isTrainingBoss)
            {
                message.Cleanup();
                return;
            }

            if (players.Length == 0)
            {
                message.Dispose();
                return;
            }

            foreach (Player candidate in players)
            {
                if (!ReferenceEquals(candidate, player) && (isNonInteractiveNpc || player.Zone.Runtime.Players.IsTrainingBossOf(player, candidate)))
                {
                    SendToPlayer(candidate, message);
                }
            }

            message.Cleanup();
            return;
        }

        if (players.Length == 0)
        {
            message.Dispose();
            return;
        }

        foreach (Player candidate in players)
        {
            if (!ReferenceEquals(candidate, player) && candidate.IsPl())
            {
                SendToPlayer(candidate, message);
            }
        }

        message.Cleanup();
    }

    // Nhánh Service.sendMessAllPlayer: registry Host xác định thứ tự online, không sắp xếp lại.
    public void SendToAll(Message message)
    {
        if (message is null)
        {
            return;
        }

        foreach (Player player in _onlinePlayers())
        {
            SendToPlayer(player, message);
        }

        message.Cleanup();
    }

    private static void SendForward(Player[] players, Message message, Func<Player, bool> canSend)
    {
        foreach (Player candidate in players)
        {
            if (candidate is not null && canSend(candidate))
            {
                candidate.SendMessage(message);
            }
        }
    }

    private static bool IsTrainingBossOfAnyPlayer(Player source, Player[] players)
    {
        foreach (Player candidate in players)
        {
            if (source.Zone!.Runtime.Players.IsTrainingBossOf(source, candidate))
            {
                return true;
            }
        }

        return false;
    }
}
