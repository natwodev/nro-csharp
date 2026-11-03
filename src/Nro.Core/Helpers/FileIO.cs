namespace Nro.Core;

/// <summary>
/// Port of <c>Utils/FileIO.java</c>: whole-file reads and writes. As in Java, an I/O failure (missing file, no permission, bad path) is NOT thrown: the read functions
/// return <c>null</c> and the write function does nothing. The only change is that <see cref="WriteFile"/> reports success with a <c>bool</c>
/// (Java returned <c>void</c>, so callers can simply ignore it). A <c>null</c> argument still throws, as the NullPointerException did.
/// </summary>
public static class FileIO
{
    /// <summary>Java <c>readFile(String url)</c>: all bytes of the file, or <c>null</c> when it cannot be read.</summary>
    public static byte[]? ReadFile(string url)
    {
        ArgumentNullException.ThrowIfNull(url);
        try
        {
            return File.ReadAllBytes(url);
        }
        catch (Exception ex) when (IsFileFailure(ex))
        {
            return null;
        }
    }

    /// <summary>
    /// Java <c>loadFile(String url)</c>: the content as a <see cref="MemoryStream"/> (Java returned a <c>ByteArrayOutputStream</c>; use <see cref="MemoryStream.ToArray"/> for
    /// <c>toByteArray()</c>), or <c>null</c> when the file cannot be read. The caller owns (disposes) the stream. The Java stack-trace print on failure is dropped.
    /// </summary>
    public static MemoryStream? LoadFile(string url)
    {
        byte[]? data = ReadFile(url);
        if (data is null)
        {
            return null;
        }

        var stream = new MemoryStream(data.Length);
        stream.Write(data, 0, data.Length);
        return stream;
    }

    /// <summary>
    /// Java <c>writeFile(String url, byte[] ab)</c>: replaces the file with <paramref name="ab"/>. The parent folder must exist (Java did not create it either).
    /// Returns <c>false</c> when the file could not be written. DELIBERATE DIFFERENCE: a <c>null</c> array is rejected before the file is touched
    /// (Java created an empty file first, then threw).
    /// </summary>
    public static bool WriteFile(string url, byte[] ab)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(ab);
        try
        {
            File.WriteAllBytes(url, ab);
            return true;
        }
        catch (Exception ex) when (IsFileFailure(ex))
        {
            return false;
        }
    }

    // Everything FileInputStream/FileOutputStream report as IOException in Java: I/O errors, access denied, a path that is empty or has illegal characters.
    private static bool IsFileFailure(Exception ex) => ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException;
}
