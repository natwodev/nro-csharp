namespace Nro.Game.Items.Core;

// Cung cấp hai phép toán nâng cấp thuần mà Item dùng để tăng và đảo gần đúng chỉ số.
// Nguồn Java: nro/combine/CombineUtil.java (PARTIAL: bản sao nội bộ của pointUp/reversePoint để mô hình vật phẩm độc lập; track Combine sẽ dùng lớp riêng cho cùng kết quả).
internal static class ItemUpgradeMath
{
    // raises by 10 percent (at least 1) times, in 32-bit arithmetic. (Java: CombineUtil.pointUp(n, iters)).
    public static int PointUp(int n, int iters)
    {
        for (int i = 0; i < iters; i++)
        {
            n += Math.Max(n / 10, 1);
        }

        return n;
    }

    // undoes approximately, dividing out one eleventh per step. A negative throws (Java: NegativeArraySizeException or ArrayIndexOutOfBoundsException). (Java: CombineUtil.reversePoint(n, iters)).
    public static int ReversePoint(int n, int iters)
    {
        int[] interValues = new int[iters + 1];
        interValues[iters] = n;
        for (int i = iters; i > 0; i--)
        {
            int prevValue = interValues[i];
            int subValue = prevValue / 11;
            interValues[i - 1] = prevValue - subValue;
        }

        return interValues[0];
    }
}
