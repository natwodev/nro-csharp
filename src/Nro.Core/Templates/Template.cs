namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/Template.java</c>: a class that only serves as a "shell" for four nested data classes (Tai Xiu MD5 bets, Tai Xiu,
/// Xoc Dia, Bau Cua). Java's nested <c>public static class</c> corresponds to a public nested class in C#
/// (C# has no "nested class that needs an enclosing instance"; every nested class stands on its own). The outer class stays a regular class because Java has a
/// public default constructor.
/// </summary>
public class Template
{
    public class TaiXiuMD5Data
    {
        public long PlayerId { get; set; }

        public long Tai { get; set; }

        public long Xiu { get; set; }

        public TaiXiuMD5Data(long playerId, long tai, long xiu)
        {
            PlayerId = playerId;
            Tai = tai;
            Xiu = xiu;
        }
    }

    public class TaiXiuData
    {
        public long PlayerId { get; set; }

        public long Tai { get; set; }

        public long Xiu { get; set; }

        public TaiXiuData(long playerId, long tai, long xiu)
        {
            PlayerId = playerId;
            Tai = tai;
            Xiu = xiu;
        }
    }

    public class XocDiaData
    {
        public long PlayerId { get; set; }

        public long ChanX1 { get; set; }

        public long ChanXiu { get; set; }

        public long ChanX3 { get; set; }

        public long ChanX15 { get; set; }

        public long LeX1 { get; set; }

        public long LeTai { get; set; }

        public long LeX3 { get; set; }

        public long LeX15 { get; set; }

        public XocDiaData(long playerId, long chanX1, long chanXiu, long chanX3, long chanX15, long leX1, long leTai, long leX3, long leX15)
        {
            PlayerId = playerId;
            ChanX1 = chanX1;
            ChanXiu = chanXiu;
            ChanX3 = chanX3;
            ChanX15 = chanX15;
            LeX1 = leX1;
            LeTai = leTai;
            LeX3 = leX3;
            LeX15 = leX15;
        }
    }

    public class BauCuaData
    {
        public long PlayerId { get; set; }

        public long Bau { get; set; }

        public long Cua { get; set; }

        public long Tom { get; set; }

        public long Ca { get; set; }

        public long Nai { get; set; }

        public long Ga { get; set; }

        public BauCuaData(long playerId, long bau, long cua, long tom, long ca, long nai, long ga)
        {
            PlayerId = playerId;
            Bau = bau;
            Cua = cua;
            Tom = tom;
            Ca = ca;
            Nai = nai;
            Ga = ga;
        }
    }
}
