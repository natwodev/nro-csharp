namespace Nro.Core.Time;

// Lịch giờ trong ngày của sự kiện Ngọc Rồng Sao Đen: giờ mở, giờ đóng và giờ bắt đầu được nhặt ngọc
// Nguồn Java: các trường tĩnh HOUR_OPEN/MIN_OPEN/SECOND_OPEN, HOUR_CLOSE/..., HOUR_CAN_PICK_DB/... của BlackBallWar mà TimeUtil đọc;
// ở C# truyền vào dưới dạng giá trị để VietnamClock không phụ thuộc trạng thái game, phần port BlackBallWar tự tạo giá trị này từ hằng số của nó
public readonly record struct BlackBallWarSchedule(
    // Giờ mở sự kiện (Java: LocalTime.of(HOUR_OPEN, MIN_OPEN, SECOND_OPEN))
    TimeOnly Open,

    // Giờ đóng sự kiện
    TimeOnly Close,

    // Giờ bắt đầu được nhặt ngọc rồng sao đen
    TimeOnly CanPick);
