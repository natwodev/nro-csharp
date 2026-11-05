using Nro.Core.Util;

namespace Nro.Game.Players;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/Utils/Functions.java (dòng 28–30).
public static class PlayerFunctions
{
    // Lọc spam trừ tài khoản Anwin, giữ nguyên điều kiện thứ tự Java (Java: isSpam).
    public static bool IsSpam(Player? player, string? text) => Functions.IsSpam(text) && player is not null && player.Name != "Anwin";
}
