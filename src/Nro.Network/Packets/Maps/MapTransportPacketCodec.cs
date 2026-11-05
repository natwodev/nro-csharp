using Nro.Core.IO;

namespace Nro.Network.Packets.Maps;

// Mã hóa MAP_TRASPORT theo DataOutputStream Java, không chứa luật chọn zone.
// Nguồn Java: source-game/Teamobi2026/SRC/src/nro/models/map/service/ChangeMapService.java (dòng 64–100); docs/PROTOCOL.md, dòng -91.
public static class MapTransportPacketCodec
{
    // Lệnh MAP_TRASPORT có dấu trên wire (Java: new Message(-91)).
    public const sbyte Command = -91;

    // Tạo payload b count, rồi từng u mapName/u planetName theo Java.
    public static Message Encode(MapTransportPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        Message message = new(Command);
        foreach (IReadOnlyList<MapTransportDestination> section in packet.Sections)
        {
            if (section.Count > byte.MaxValue)
            {
                throw new InvalidDataException("MAP_TRASPORT không thể chứa quá 255 đích đến trong một section.");
            }

            message.Writer().WriteByte(section.Count);
            foreach (MapTransportDestination destination in section)
            {
                message.Writer().WriteUTF(destination.MapName);
                message.Writer().WriteUTF(destination.PlanetName);
            }
        }

        return message;
    }

    // Đọc payload -91 theo thứ tự Java để golden test xác nhận byte-exact.
    public static MapTransportPacket Decode(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.Command != Command)
        {
            throw new InvalidDataException("MAP_TRASPORT không đúng command.");
        }

        var sections = new List<IReadOnlyList<MapTransportDestination>>();
        while (message.Reader().Available > 0)
        {
            int count = message.ReadUnsignedByte();
            var destinations = new List<MapTransportDestination>(count);
            for (int i = 0; i < count; i++)
            {
                destinations.Add(new MapTransportDestination(message.ReadUTF(), message.ReadUTF()));
            }

            sections.Add(destinations);
        }

        return new MapTransportPacket(sections);
    }
}
