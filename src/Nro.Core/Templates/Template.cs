namespace Nro.Core.Templates;

// Là lớp vỏ chứa các lớp dữ liệu cược Tài Xỉu MD5, Tài Xỉu, Xóc Đĩa và Bầu Cua.
// Nguồn Java: nro/template/Template.java (giữ các lớp lồng nhau độc lập tương ứng public static class của Java; lớp ngoài vẫn có hàm tạo mặc định công khai như Java).
public class Template
{
    // Dữ liệu cược Tài Xỉu MD5.
    public class TaiXiuMD5Data
    {
        // Mã người chơi (Java: playerId)
        public long PlayerId { get; set; }

        // Giá trị cược Tài (Java: tai)
        public long Tai { get; set; }

        // Giá trị cược Xỉu (Java: xiu)
        public long Xiu { get; set; }

        public TaiXiuMD5Data(long playerId, long tai, long xiu)
        {
            PlayerId = playerId;
            Tai = tai;
            Xiu = xiu;
        }
    }

    // Dữ liệu cược Tài Xỉu.
    public class TaiXiuData
    {
        // Mã người chơi (Java: playerId)
        public long PlayerId { get; set; }

        // Giá trị cược Tài (Java: tai)
        public long Tai { get; set; }

        // Giá trị cược Xỉu (Java: xiu)
        public long Xiu { get; set; }

        public TaiXiuData(long playerId, long tai, long xiu)
        {
            PlayerId = playerId;
            Tai = tai;
            Xiu = xiu;
        }
    }

    // Dữ liệu cược Xóc Đĩa.
    public class XocDiaData
    {
        // Mã người chơi (Java: playerId)
        public long PlayerId { get; set; }

        // Giá trị cược chẵn nhân 1 (Java: chanX1)
        public long ChanX1 { get; set; }

        // Giá trị cược chẵn xỉu (Java: chanXiu)
        public long ChanXiu { get; set; }

        // Giá trị cược chẵn nhân 3 (Java: chanX3)
        public long ChanX3 { get; set; }

        // Giá trị cược chẵn nhân 15 (Java: chanX15)
        public long ChanX15 { get; set; }

        // Giá trị cược lẻ nhân 1 (Java: leX1)
        public long LeX1 { get; set; }

        // Giá trị cược lẻ tài (Java: leTai)
        public long LeTai { get; set; }

        // Giá trị cược lẻ nhân 3 (Java: leX3)
        public long LeX3 { get; set; }

        // Giá trị cược lẻ nhân 15 (Java: leX15)
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

    // Dữ liệu cược Bầu Cua (Java: BauCuaData).
    public class GourdCrabData
    {
        // Mã người chơi (Java: playerId)
        public long PlayerId { get; set; }

        // Giá trị cược bầu (Java: bau)
        public long Bau { get; set; }

        // Giá trị cược cua (Java: cua)
        public long Cua { get; set; }

        // Giá trị cược tôm (Java: tom)
        public long Tom { get; set; }

        // Giá trị cược cá (Java: ca)
        public long Ca { get; set; }

        // Giá trị cược nai (Java: nai)
        public long Nai { get; set; }

        // Giá trị cược gà (Java: ga)
        public long Ga { get; set; }

        public GourdCrabData(long playerId, long bau, long cua, long tom, long ca, long nai, long ga)
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
