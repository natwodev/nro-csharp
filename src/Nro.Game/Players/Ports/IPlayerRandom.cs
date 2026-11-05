namespace Nro.Game.Players.Ports;

// Cổng sinh số ngẫu nhiên thuộc người chơi để không gọi Util.nextInt trực tiếp.
// Nguồn Java: nro/player/Player.java (dòng 1639–1640).
public interface IPlayerRandom
{
    // Trả số nguyên trong đoạn đóng để chọn đầu Đuôi Khỉ (Java: Util.nextInt).
    int NextInt(int minInclusive, int maxInclusive);

}
