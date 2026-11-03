namespace Nro.Core.IO;

/// <summary>
/// Counterpart of <c>java.io.UTFDataFormatException</c> (extends <c>IOException</c>, like the Java version):
/// the encoded string exceeds 65535 bytes on write, or the modified UTF-8 data is malformed on read.
/// The message is kept identical to JDK 17 (e.g. "malformed input around byte 2").
/// </summary>
public sealed class UtfDataFormatException : IOException
{
    public UtfDataFormatException()
    {
    }

    public UtfDataFormatException(string message)
        : base(message)
    {
    }
}
