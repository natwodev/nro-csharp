namespace Nro.Core.IO;

/// <summary>Argument validation shared by <see cref="JavaDataOutput"/> and <see cref="JavaDataInput"/>.</summary>
internal static class JavaIoChecks
{
    /// <summary>
    /// Counterpart of <c>java.util.Objects.checkFromIndexSize(fromIndex, size, length)</c>, which the JDK calls before every
    /// <c>write/read(byte[], off, len)</c> operation: an invalid range throws BEFORE any data is touched.
    /// (Java throws <c>IndexOutOfBoundsException</c>; .NET uses <see cref="ArgumentOutOfRangeException"/>.)
    /// </summary>
    internal static void CheckFromIndexSize(int fromIndex, int size, int length)
    {
        if ((length | fromIndex | size) < 0 || size > length - fromIndex)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fromIndex),
                $"Range [{fromIndex}, {fromIndex} + {size}) out of bounds for length {length}");
        }
    }
}
