namespace Nro.Core;

/// <summary>
/// Counterpart of the plain <c>java.lang.Exception("Thời gian không hợp lệ")</c> that <c>TimeUtil.getTime(String, String)</c> and
/// <c>TimeUtil.isTimeNowInRangex</c> throw when a date text does not match its pattern. A dedicated type so callers can catch it precisely;
/// the message is kept identical to the Java text because the game may show it to players.
/// </summary>
public sealed class InvalidTimeException : Exception
{
    /// <summary>The exact Java message.</summary>
    public const string JavaMessage = "Thời gian không hợp lệ";

    public InvalidTimeException()
        : base(JavaMessage)
    {
    }

    public InvalidTimeException(string message)
        : base(message)
    {
    }

    public InvalidTimeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
