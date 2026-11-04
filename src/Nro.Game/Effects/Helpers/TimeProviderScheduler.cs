
namespace Nro.Game.Effects.Helpers;

// Lập lịch chờ cho môi trường chạy thật bằng Task.Delay trên TimeProvider.
// Nguồn Java: nro/effect/EffectSkill.java (lớp phụ trợ C# thay việc chờ của dịch vụ hiệu ứng).
public sealed class TimeProviderScheduler : IEffectScheduler
{
    private readonly TimeProvider timeProvider;

    // Tạo bộ lập lịch với nguồn thời gian được chỉ định, mặc định TimeProvider.System.
    public TimeProviderScheduler(TimeProvider? timeProvider = null)
    {
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    // Chờ bất đồng bộ trong khoảng đã cho và tôn trọng mã hủy.
    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken) => Task.Delay(delay, timeProvider, cancellationToken);
}
