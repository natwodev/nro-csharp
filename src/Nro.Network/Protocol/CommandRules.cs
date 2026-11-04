using System.Reflection;
using Nro.Network.Configurations;

namespace Nro.Network.Protocol;

// Tập hợp các quy tắc lệnh rút ra từ đặc tả giao thức.
// Nguồn Java: nro/server/Controller.java (giữ nguyên các nhánh lệnh trước khi có nhân vật và tên hằng lệnh Java).
public static class CommandRules
{
    // Các lệnh Java Controller nhận trước khi phiên có nhân vật, gồm login/tài nguyên, -101, 126, -114, 27, -28, -29, -30; lệnh khác bị bỏ im lặng.
    public static readonly IReadOnlySet<sbyte> NoCharacterRequired = new HashSet<sbyte>
    {
        -74, -87, -67, 66, -32, 11, -41, -111, -28, -29, -30, -78, -76, 42, -101, 126, -114, 27,
    };

    // Mọi mã lệnh có tên trong Cmd_message; mã khác không thuộc giao thức.
    public static readonly IReadOnlySet<sbyte> KnownCommands = typeof(Cmd_message)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Select(f => (sbyte)f.GetRawConstantValue()!)
        .ToHashSet();
}
