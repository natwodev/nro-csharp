using Nro.Core.IO;

namespace Nro.Core.Interfaces;

/// <summary>
/// Port of <c>network/interfaces/IMessage.java</c> (35 methods, in the same declaration order as the Java version).
/// <para>
/// REMOVED: <c>readImage()</c> and <c>writeImage(BufferedImage, String)</c>: nothing in the source code calls them
/// (the whole of <c>src/</c> was grepped), and <c>BufferedImage</c> does not exist in .NET. If images ever need to be sent, use <c>byte[]</c>
/// (<c>WriteInt(length)</c> followed by <c>Write(bytes)</c>, which matches the Java wire format).
/// </para>
/// <para>
/// Type mapping (see 01-ANALYSIS-MAPPING section 3.3): signed <c>byte</c> -> <c>sbyte</c>; <c>boolean</c> -> <c>bool</c>;
/// <c>String</c> -> <c>string</c>; <c>DataOutputStream</c>/<c>DataInputStream</c> -> <see cref="JavaDataOutput"/>/<see cref="JavaDataInput"/>.
/// </para>
/// <para>
/// Deliberately does NOT extend <see cref="IDisposable"/>: <c>Dispose()</c> here is Java's <c>dispose()</c> (it drops the stream references, after which
/// <c>Writer()</c>/<c>GetData()</c> throw), so a <c>using</c> statement would inadvertently break a packet that is still sitting in the send queue.
/// </para>
/// </summary>
public interface IMessage
{
    int Read();

    int Read(byte[] b);

    int Read(byte[] b, int off, int len);

    bool ReadBoolean();

    sbyte ReadByte();

    short ReadShort();

    int ReadInt();

    long ReadLong();

    float ReadFloat();

    double ReadDouble();

    char ReadChar();

    string ReadUTF();

    void ReadFully(byte[] b);

    void ReadFully(byte[] b, int off, int len);

    int ReadUnsignedByte();

    int ReadUnsignedShort();

    void Write(byte[] b);

    void Write(int b);

    void Write(byte[] b, int off, int len);

    void WriteBoolean(bool v);

    void WriteByte(int v);

    void WriteBytes(string s);

    void WriteChar(int v);

    void WriteChars(string s);

    void WriteDouble(double v);

    void WriteFloat(float v);

    void WriteInt(int v);

    void WriteLong(long v);

    void WriteShort(int v);

    void WriteUTF(string str);

    /// <summary>The write stream (Java: <c>DataOutputStream</c>). Throws if the message is in read mode or has been <see cref="Dispose"/>d.</summary>
    JavaDataOutput Writer();

    /// <summary>The read stream (Java: <c>DataInputStream</c>). Throws if the message is in write mode or has been <see cref="Dispose"/>d.</summary>
    JavaDataInput Reader();

    /// <summary>A COPY of the bytes written so far (a new array on every call). Throws if the message is in read mode or has been <see cref="Dispose"/>d.</summary>
    byte[] GetData();

    /// <summary>Closes the streams. For in-memory streams, closing does NOTHING: the data and further writes/reads stay intact.</summary>
    void Cleanup();

    /// <summary><see cref="Cleanup"/> followed by dropping the stream references: afterwards every read/write operation, <c>Writer()</c>, <c>Reader()</c> and <c>GetData()</c> throws.</summary>
    void Dispose();
}
