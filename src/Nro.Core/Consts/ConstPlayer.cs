namespace Nro.Core.Consts;

/// <summary>
/// Port of <c>consts/ConstPlayer.java</c>: class and constant names are preserved; values were machine-translated from Java
/// (port-csharp/tools/consts/gen_consts_cs.py), not copied by hand.
/// Automatically verified against the real values read from the compiled Java class: <c>ConstsParityTests</c> (tests/golden/consts.json).
/// </summary>
public static class ConstPlayer
{
    public static readonly int[] HEADMONKEY = new int[] { 192, 195, 196, 199, 197, 200, 198 };
    public static readonly int[] DUOIHEADMONKEY = new int[] { 192, 195, 196, 199, 197, 200, 198 };

    public const sbyte TRAI_DAT = 0;
    public const sbyte NAMEC = 1;
    public const sbyte XAYDA = 2;

    //type pk
    public const sbyte NON_PK = 0;
    public const sbyte PK_PVP = 3;
    public const sbyte PK_PVP_2 = 4;
    public const sbyte PK_ALL = 5;

    //type fushion
    public const sbyte NON_FUSION = 0;
    public const sbyte LUONG_LONG_NHAT_THE = 4;
    public const sbyte HOP_THE_PORATA = 6;
    public const sbyte HOP_THE_PORATA2 = 8;
    public const sbyte HOP_THE_PORATA3 = 10;
    public const sbyte HOP_THE_PORATA4 = 12;
    public const sbyte HOP_THE_PORATA5 = 14;

    //
    public const int QTY_MAX_ITEM_BODY_PLAYER = 12;
    public const int QTY_MAX_ITEM_BODY_PET = 7;
    public const int QTY_MAX_SKILL_PET = 3;

    //
    //const skill player
    public static readonly int[] SKILL_TD = new int[] { 0, 1, 6, 20, 10, 24, 19 };
    public static readonly int[] SKILL_NAMEC = new int[] { 17, 3, 12, 11, 18, 26, 19 };
    public static readonly int[] SKILL_XAYDA = new int[] { 4, 5, 8, 13, 22, 25, 19 };
}
