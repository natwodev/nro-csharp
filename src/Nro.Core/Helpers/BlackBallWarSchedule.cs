namespace Nro.Core;

/// <summary>
/// The three times of day that Java's <c>TimeUtil</c> read from the static fields of <c>BlackBallWar</c> (<c>HOUR_OPEN/MIN_OPEN/SECOND_OPEN</c>,
/// <c>HOUR_CLOSE/...</c>, <c>HOUR_CAN_PICK_DB/...</c>). Passing them in keeps <see cref="VietnamClock"/> free of game state; the BlackBallWar port builds
/// this value from its own constants.
/// </summary>
/// <param name="Open">When the war opens (<c>LocalTime.of(HOUR_OPEN, MIN_OPEN, SECOND_OPEN)</c>).</param>
/// <param name="Close">When the war closes.</param>
/// <param name="CanPick">From when the dragon balls can be picked up.</param>
public readonly record struct BlackBallWarSchedule(TimeOnly Open, TimeOnly Close, TimeOnly CanPick);
