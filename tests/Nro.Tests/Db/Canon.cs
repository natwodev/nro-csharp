using System.Globalization;
using System.Text.Json;

namespace Nro.Tests.Db;

/// <summary>
/// A "canonical" string for a result: brings the Java value recorded in the golden file and the actual C# value to the same
/// string form so they can be compared with <c>==</c> and give readable mismatch messages. The type is kept in the string (<c>int:5</c> differs from <c>long:5</c>).
/// </summary>
internal static class Canon
{
    /// <summary>Result of an operation with no return value (Java <c>void</c>).</summary>
    public static readonly object Void = new();

    public static string Value(object? value, bool isVoid = false)
    {
        if (isVoid || ReferenceEquals(value, Void))
        {
            return "void";
        }

        return value switch
        {
            null => "null",
            bool flag => "bool:" + (flag ? "true" : "false"),
            sbyte number => "byte:" + number.ToString(CultureInfo.InvariantCulture),
            short number => "short:" + number.ToString(CultureInfo.InvariantCulture),
            int number => "int:" + number.ToString(CultureInfo.InvariantCulture),
            long number => "long:" + number.ToString(CultureInfo.InvariantCulture),
            float number => float.IsNaN(number)
                ? "float:NaN"
                : $"float:{BitConverter.SingleToInt32Bits(number):X8}({number.ToString("R", CultureInfo.InvariantCulture)})",
            double number => double.IsNaN(number)
                ? "double:NaN"
                : $"double:{BitConverter.DoubleToInt64Bits(number):X16}({number.ToString("R", CultureInfo.InvariantCulture)})",
            string text => "string:" + JsonSerializer.Serialize(text),
            decimal number => "decimal:" + number.ToString(CultureInfo.InvariantCulture),
            DateTime time => "timestamp:" + time.Ticks.ToString(CultureInfo.InvariantCulture),
            DateOnly date => "date:" + date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            byte[] bytes => "bytes:" + Convert.ToHexString(bytes).ToLowerInvariant(),
            _ => "unknown:" + value.GetType().FullName,
        };
    }

    /// <summary>
    /// Recorded Java exception to canonical kind. NPE/CCE/AIOOBE are JVM exceptions, so only the KIND is compared (the JVM text has no
    /// C# equivalent); <c>java.lang.Exception</c> is a custom error of <c>ConnectResultSet</c>, so the MESSAGE is compared too.
    /// </summary>
    public static string JavaError(string javaClass, string? message) => javaClass switch
    {
        "java.lang.NullPointerException" => "err:NPE",
        "java.lang.ClassCastException" => "err:CCE",
        "java.lang.ArrayIndexOutOfBoundsException" => "err:AIOOBE",
        "java.lang.Exception" => "err:EXC:" + message,
        _ => throw new InvalidOperationException($"Java exception not mapped yet: {javaClass}"),
    };

    /// <summary>Actual C# exception to canonical kind (same mapping table as <see cref="JavaError"/>).</summary>
    public static string Error(Exception exception) => exception switch
    {
        NullReferenceException => "err:NPE",
        InvalidCastException => "err:CCE",
        IndexOutOfRangeException => "err:AIOOBE",
        InvalidOperationException => "err:EXC:" + exception.Message,
        _ => "err:UNEXPECTED:" + exception.GetType().FullName + ":" + exception.Message,
    };
}
