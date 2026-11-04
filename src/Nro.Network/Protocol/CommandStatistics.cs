namespace Nro.Network.Protocol;

// Lưu các bộ đếm an toàn luồng phục vụ chẩn đoán và kiểm thử của bộ phân phối lệnh.
// Nguồn Java: nro/server/Controller.java (bộ đếm là phần quan sát C# bổ sung, không thay đổi xử lý giao thức Java).
public sealed class CommandStatistics
{
    private long _handled;
    private long _failed;
    private long _notImplemented;
    private long _unknown;
    private long _droppedNoCharacter;

    // Số lệnh có handler hoàn tất.
    public long Handled => Interlocked.Read(ref _handled);

    // Số lệnh có handler ném lỗi; gói bị mất nhưng kết nối vẫn mở.
    public long Failed => Interlocked.Read(ref _failed);

    // Số lệnh có trong giao thức nhưng chưa đăng ký handler.
    public long NotImplemented => Interlocked.Read(ref _notImplemented);

    // Số lệnh có mã không thuộc giao thức.
    public long Unknown => Interlocked.Read(ref _unknown);

    // Số lệnh bị bỏ vì phiên chưa có nhân vật.
    public long DroppedNoCharacter => Interlocked.Read(ref _droppedNoCharacter);

    internal void CountHandled() => Interlocked.Increment(ref _handled);

    internal void CountFailed() => Interlocked.Increment(ref _failed);

    internal void CountNotImplemented() => Interlocked.Increment(ref _notImplemented);

    internal void CountUnknown() => Interlocked.Increment(ref _unknown);

    internal void CountDroppedNoCharacter() => Interlocked.Increment(ref _droppedNoCharacter);
}
