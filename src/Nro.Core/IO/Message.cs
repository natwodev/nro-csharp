using Nro.Core.Interfaces;

namespace Nro.Core.IO;

/// <summary>
/// Port of <c>network/io/Message.java</c>: a packet made of a <see cref="Command"/> (<c>sbyte</c>) and either a WRITE stream
/// (constructors <c>(int)</c>/<c>(sbyte)</c>) or a READ stream (constructor <c>(sbyte, byte[])</c>). The <c>ReadX/WriteX</c> methods merely delegate to
/// <see cref="Reader"/>/<see cref="Writer"/>.
/// <para>
/// LIFECYCLE (measured on the real Java code, JDK 17; the 587 <c>cleanup()</c> calls and the <c>dispose()</c> calls in the game depend on this):
/// </para>
/// <list type="bullet">
/// <item><description><b><see cref="Cleanup"/> only "closes" in-memory streams, and closing <c>ByteArrayOutputStream/ByteArrayInputStream</c> does nothing.</b>
/// After <c>Cleanup()</c> the data is intact, <see cref="Writer"/>/<see cref="Reader"/> still return the same object, writing/reading can continue,
/// and <see cref="GetData"/> still returns everything. Calling it repeatedly, or after <see cref="Dispose"/>, never fails. This is REQUIRED: game code calls
/// <c>session.sendMessage(msg); msg.cleanup();</c> while the packet is still in the queue, and the sender thread reads <see cref="GetData"/> afterwards.</description></item>
/// <item><description><b><see cref="Dispose"/> = <see cref="Cleanup"/> followed by dropping the stream references.</b> Afterwards (Java returns <c>null</c>, and using it would throw
/// <c>NullPointerException</c>): <see cref="Writer"/>, <see cref="Reader"/>, <see cref="GetData"/> and every <c>ReadX/WriteX</c> throw
/// <see cref="ObjectDisposedException"/>. <see cref="Command"/> is still readable (<c>Controller.onMessage</c> still logs <c>_msg.command</c> after <c>dispose()</c>).
/// Calling <see cref="Dispose"/> or <see cref="Cleanup"/> again does not fail.</description></item>
/// <item><description>A write-mode packet has no <see cref="Reader"/>, and a read-mode packet has no <see cref="Writer"/>/<see cref="GetData"/> (Java: <c>null</c>/NPE);
/// here they throw <see cref="InvalidOperationException"/>. This error does NOT derive from <c>IOException</c>, so the caller's <c>catch (IOException)</c> blocks
/// do not swallow it, just as an NPE propagates through <c>catch (IOException)</c> in Java.</description></item>
/// </list>
/// <para>
/// Deliberately does NOT extend <see cref="IDisposable"/> (see <see cref="IMessage"/>). There is no <c>ReadImage/WriteImage</c> (nothing calls them).
/// </para>
/// </summary>
public class Message : IMessage
{
    private JavaDataOutput? _dos;
    private JavaDataInput? _dis;
    private bool _disposed;

    /// <summary>Java: <c>public byte command</c> (public, writable field) -> signed <c>sbyte</c> (e.g. -27, -28).</summary>
    public sbyte Command { get; set; }

    /// <summary>
    /// Java's <c>Message(int command)</c>: truncates like <c>(byte) command</c>, i.e. the signed low 8 bits
    /// (<c>new Message(-218)</c> is command 38, <c>new Message(200)</c> is -56, <c>new Message(256)</c> is 0).
    /// </summary>
    public Message(int command)
        : this(unchecked((sbyte)command))
    {
    }

    /// <summary>A packet in WRITE mode (empty write stream; <see cref="GetData"/> returns an empty array, not null).</summary>
    public Message(sbyte command)
    {
        Command = command;
        _dos = new JavaDataOutput();
    }

    /// <summary>
    /// A packet in READ mode over <paramref name="data"/>. Does NOT copy the array (like <c>ByteArrayInputStream</c>);
    /// a null <c>data</c> throws <see cref="ArgumentNullException"/> (Java: NPE in the constructor).
    /// </summary>
    public Message(sbyte command, byte[] data)
    {
        Command = command;
        _dis = new JavaDataInput(data);
    }

    public JavaDataOutput Writer() => _dos ?? throw Unavailable("writer", isWriter: true);

    public JavaDataInput Reader() => _dis ?? throw Unavailable("reader", isWriter: false);

    public byte[] GetData() => (_dos ?? throw Unavailable("writer (GetData)", isWriter: true)).ToArray();

    /// <summary>Java <c>writeCris</c>: <paramref name="is"/> = true writes a 4-byte <c>int</c> (truncated to the low 32 bits), otherwise an 8-byte <c>long</c>.</summary>
    public void WriteCris(long v, bool @is)
    {
        if (@is)
        {
            WriteInt(unchecked((int)v));
            return;
        }

        WriteLong(v);
    }

    /// <summary>Java <c>writeShortToLong</c>: <paramref name="is"/> = true writes a 2-byte <c>short</c> (truncated to the low 16 bits), otherwise an 8-byte <c>long</c>.</summary>
    public void WriteShortToLong(long v, bool @is)
    {
        if (@is)
        {
            WriteShort(unchecked((short)v));
            return;
        }

        WriteLong(v);
    }

    /// <summary>Java <c>writeByteToLong</c>: <paramref name="is"/> = true writes a 1-byte <c>byte</c> (truncated to the low 8 bits), otherwise an 8-byte <c>long</c>.</summary>
    public void WriteByteToLong(long v, bool @is)
    {
        if (@is)
        {
            WriteByte(unchecked((sbyte)v));
            return;
        }

        WriteLong(v);
    }

    /// <summary>Java <c>cleanup()</c>: closes the streams that are still referenced. Closing in-memory streams does nothing, so the data and further writes/reads are NOT affected.</summary>
    public void Cleanup()
    {
        _dis?.Close();
        _dos?.Close();
    }

    /// <summary>Java <c>dispose()</c>: <see cref="Cleanup"/> followed by dropping the stream references. <see cref="Command"/> is unchanged.</summary>
    public void Dispose()
    {
        Cleanup();
        _disposed = true;
        _dis = null;
        _dos = null;
    }

    public int Read() => Reader().Read();

    public int Read(byte[] b) => Reader().Read(b);

    public int Read(byte[] b, int off, int len) => Reader().Read(b, off, len);

    public bool ReadBoolean() => Reader().ReadBoolean();

    public sbyte ReadByte() => Reader().ReadByte();

    public short ReadShort() => Reader().ReadShort();

    public int ReadInt() => Reader().ReadInt();

    public long ReadLong() => Reader().ReadLong();

    public float ReadFloat() => Reader().ReadFloat();

    public double ReadDouble() => Reader().ReadDouble();

    public char ReadChar() => Reader().ReadChar();

    public string ReadUTF() => Reader().ReadUTF();

    public void ReadFully(byte[] b) => Reader().ReadFully(b);

    public void ReadFully(byte[] b, int off, int len) => Reader().ReadFully(b, off, len);

    public int ReadUnsignedByte() => Reader().ReadUnsignedByte();

    public int ReadUnsignedShort() => Reader().ReadUnsignedShort();

    public void Write(byte[] b) => Writer().Write(b);

    public void Write(int b) => Writer().Write(b);

    public void Write(byte[] b, int off, int len) => Writer().Write(b, off, len);

    public void WriteBoolean(bool v) => Writer().WriteBoolean(v);

    public void WriteByte(int v) => Writer().WriteByte(v);

    public void WriteBytes(string s) => Writer().WriteBytes(s);

    public void WriteChar(int v) => Writer().WriteChar(v);

    public void WriteChars(string s) => Writer().WriteChars(s);

    public void WriteDouble(double v) => Writer().WriteDouble(v);

    public void WriteFloat(float v) => Writer().WriteFloat(v);

    public void WriteInt(int v) => Writer().WriteInt(v);

    public void WriteLong(long v) => Writer().WriteLong(v);

    public void WriteShort(int v) => Writer().WriteShort(v);

    public void WriteUTF(string str) => Writer().WriteUTF(str);

    /// <summary>The error replacing Java's "<c>null</c> then NullPointerException": <see cref="ObjectDisposedException"/> if already disposed, otherwise a wrong-mode error.</summary>
    private Exception Unavailable(string what, bool isWriter)
    {
        if (_disposed)
        {
            return new ObjectDisposedException(
                nameof(Message),
                $"Message has been disposed with Dispose(): {what} is no longer available (Java: returns null, using it would throw NullPointerException)");
        }

        return new InvalidOperationException(
            isWriter
                ? $"Message is in READ mode (created with Message(command, data)): there is no {what} (Java: null/NPE)"
                : $"Message is in WRITE mode (created with Message(command)): there is no {what} (Java: null/NPE)");
    }
}
