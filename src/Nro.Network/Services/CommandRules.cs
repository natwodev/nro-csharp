using System.Reflection;
using Nro.Network.Consts;

namespace Nro.Network.Services;

/// <summary>Facts about the commands that come from the protocol specification (<c>docs/PROTOCOL.md</c> section 3).</summary>
public static class CommandRules
{
    /// <summary>
    /// Commands the Java <c>Controller</c> accepts BEFORE the session has a character (section 3.4 of the specification): the login and resource
    /// commands, the empty cases <c>-101</c>, <c>126</c>, <c>-114</c>, <c>27</c>, and <c>-28</c>, <c>-29</c>, <c>-30</c> (which do their own checks inside).
    /// Every other command requires a character and is silently dropped without one.
    /// </summary>
    public static readonly IReadOnlySet<sbyte> NoCharacterRequired = new HashSet<sbyte>
    {
        -74, -87, -67, 66, -32, 11, -41, -111, -28, -29, -30, -78, -76, 42, -101, 126, -114, 27,
    };

    /// <summary>Every command number that has a name in <see cref="Cmd_message"/>. A packet with any other number is not part of the protocol.</summary>
    public static readonly IReadOnlySet<sbyte> KnownCommands = typeof(Cmd_message)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Select(f => (sbyte)f.GetRawConstantValue()!)
        .ToHashSet();
}
