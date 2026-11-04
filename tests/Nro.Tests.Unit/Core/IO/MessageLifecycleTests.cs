using Nro.Core.IO;

namespace Nro.Tests.Unit.Core.IO;

/// <summary>
/// Lifecycle of <see cref="Message"/> <c>Cleanup()</c>/<c>Dispose()</c>: the tests record the behaviour of Java's <c>Message.cleanup()/dispose()</c>
/// (Message.java:77-103) that the 587 <c>cleanup()</c> calls and the <c>dispose()</c> calls in the game rely on. Every conclusion below was MEASURED by
/// running the project's own <c>network.io.Message</c> class on JDK 17.0.19 (measurement code: labels W*, D*, R*, C* in each test's comment).
/// <para>
/// Summary: <c>cleanup()</c> does nothing observable (closing ByteArray*Stream is a no-op) -> the data, <c>writer()</c>, <c>reader()</c>,
/// <c>getData()</c> and further writing/reading all stay as before. <c>dispose()</c> = <c>cleanup()</c> + setting the 4 stream references to null -> <c>writer()/reader()</c> return null
/// while <c>getData()</c> and every read/write throw NullPointerException; <c>command</c> is unchanged; <c>cleanup()/dispose()</c> a second time does not fail.
/// The C# version replaces "null then NPE" with an exception thrown immediately by <c>Writer()/Reader()/GetData()</c>: <see cref="ObjectDisposedException"/> after
/// <c>Dispose()</c>, <see cref="InvalidOperationException"/> in the wrong write/read mode; neither is an <see cref="IOException"/>.
/// </para>
/// </summary>
public class MessageLifecycleTests
{
    private static Message WriteMessage()
    {
        var m = new Message(5);
        m.Writer().WriteInt(0x01020304);
        return m;
    }

    private static Message ReadMessage() => new((sbyte)-27, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

    // ------------------------------------------------------------------ Cleanup in write mode (W1..W8)

    [Fact]
    public void Cleanup_does_not_lose_data_GetData_still_returns_everything_W1()
    {
        Message m = WriteMessage();

        m.Cleanup();

        Assert.Equal("01020304", GoldenFile.ToHex(m.GetData()));
    }

    [Fact]
    public void Cleanup_does_not_change_Writer_it_is_still_the_same_object_W2()
    {
        Message m = WriteMessage();
        JavaDataOutput before = m.Writer();

        m.Cleanup();

        Assert.Same(before, m.Writer());
    }

    [Fact]
    public void After_Cleanup_writing_still_works_through_Message_and_through_Writer_W3_W4()
    {
        Message m = WriteMessage();
        m.Cleanup();

        m.WriteShort(0x0506);
        m.Writer().WriteByte(7);

        Assert.Equal("01020304" + "0506" + "07", GoldenFile.ToHex(m.GetData()));
    }

    [Fact]
    public void Repeated_Cleanup_does_not_fail_and_Command_is_unchanged_W5_W6()
    {
        Message m = WriteMessage();

        m.Cleanup();
        m.Cleanup();
        m.Cleanup();

        Assert.Equal("01020304", GoldenFile.ToHex(m.GetData()));
        Assert.Equal((sbyte)5, m.Command);
    }

    [Fact]
    public void Send_queue_scenario_producer_calls_Cleanup_right_after_enqueueing_and_the_consumer_still_gets_all_the_data()
    {
        // session.sendMessage(msg); msg.cleanup();   <- 587 places in the game; the sender thread only reads getData() AFTER that
        var queue = new Queue<Message>();
        Message m = WriteMessage();
        m.WriteUTF("Xin chào");
        byte[] written = m.GetData();

        queue.Enqueue(m); // sendMessage
        m.Cleanup(); // the caller's cleanup
        Message queued = queue.Dequeue(); // the sender thread takes it out
        byte[] sent = queued.GetData();
        queued.Cleanup(); // Sender.run calls cleanup() again after sending

        Assert.Equal(written, sent);
        Assert.Equal(written, m.GetData());
    }

    // ------------------------------------------------------------------ Dispose in write mode (D1..D12)

    [Fact]
    public void Dispose_then_Writer_Reader_GetData_all_throw_ObjectDisposedException_D1_D2_D3()
    {
        Message m = WriteMessage();

        m.Dispose();

        Assert.Throws<ObjectDisposedException>(() => m.Writer());
        Assert.Throws<ObjectDisposedException>(() => m.Reader());
        Assert.Throws<ObjectDisposedException>(() => m.GetData());
    }

    [Fact]
    public void Dispose_then_Cleanup_and_Dispose_again_do_not_fail_and_Command_is_intact_D7_D8_D9()
    {
        Message m = WriteMessage();
        m.Dispose();

        m.Cleanup();
        m.Dispose();
        m.Cleanup();

        // Controller.onMessage still reads _msg.command in a finally block AFTER _msg.dispose()
        Assert.Equal((sbyte)5, m.Command);
    }

    [Fact]
    public void Dispose_preserves_a_Command_that_was_changed_earlier()
    {
        Message m = WriteMessage();
        m.Command = -77;

        m.Dispose();

        Assert.Equal((sbyte)-77, m.Command);
    }

    // ------------------------------------------------------------------ every ReadX/WriteX must throw the right type of exception

    private static readonly (string Name, Action<Message> Run)[] WriteActions =
    [
        ("Write(int)", m => m.Write(1)),
        ("Write(byte[])", m => m.Write(new byte[1])),
        ("Write(byte[],int,int)", m => m.Write(new byte[1], 0, 1)),
        ("WriteBoolean", m => m.WriteBoolean(true)),
        ("WriteByte", m => m.WriteByte(1)),
        ("WriteBytes", m => m.WriteBytes("a")),
        ("WriteChar", m => m.WriteChar(1)),
        ("WriteChars", m => m.WriteChars("a")),
        ("WriteDouble", m => m.WriteDouble(1)),
        ("WriteFloat", m => m.WriteFloat(1)),
        ("WriteInt", m => m.WriteInt(1)),
        ("WriteLong", m => m.WriteLong(1)),
        ("WriteShort", m => m.WriteShort(1)),
        ("WriteUTF", m => m.WriteUTF("a")),
        ("WriteCris(true)", m => m.WriteCris(1, true)),
        ("WriteCris(false)", m => m.WriteCris(1, false)),
        ("WriteShortToLong(true)", m => m.WriteShortToLong(1, true)),
        ("WriteShortToLong(false)", m => m.WriteShortToLong(1, false)),
        ("WriteByteToLong(true)", m => m.WriteByteToLong(1, true)),
        ("WriteByteToLong(false)", m => m.WriteByteToLong(1, false)),
        ("Writer()", m => m.Writer()),
        ("GetData()", m => m.GetData()),
    ];

    private static readonly (string Name, Action<Message> Run)[] ReadActions =
    [
        ("Read()", m => m.Read()),
        ("Read(byte[])", m => m.Read(new byte[1])),
        ("Read(byte[],int,int)", m => m.Read(new byte[1], 0, 1)),
        ("ReadBoolean", m => m.ReadBoolean()),
        ("ReadByte", m => m.ReadByte()),
        ("ReadShort", m => m.ReadShort()),
        ("ReadInt", m => m.ReadInt()),
        ("ReadLong", m => m.ReadLong()),
        ("ReadFloat", m => m.ReadFloat()),
        ("ReadDouble", m => m.ReadDouble()),
        ("ReadChar", m => m.ReadChar()),
        ("ReadUTF", m => m.ReadUTF()),
        ("ReadFully(byte[])", m => m.ReadFully(new byte[1])),
        ("ReadFully(byte[],int,int)", m => m.ReadFully(new byte[1], 0, 1)),
        ("ReadUnsignedByte", m => m.ReadUnsignedByte()),
        ("ReadUnsignedShort", m => m.ReadUnsignedShort()),
        ("Reader()", m => m.Reader()),
    ];

    private static List<string> WrongExceptions(Message m, (string Name, Action<Message> Run)[] actions, Type expected)
    {
        var problems = new List<string>();
        foreach ((string name, Action<Message> run) in actions)
        {
            Exception? ex = Record.Exception(() => run(m));
            if (ex is null)
            {
                problems.Add($"{name}: no exception thrown");
            }
            else if (ex.GetType() != expected)
            {
                problems.Add($"{name}: threw {ex.GetType().Name}, expected {expected.Name}");
            }
            else if (ex is IOException)
            {
                problems.Add($"{name}: the exception must not be an IOException (Java: NPE propagating through catch IOException)");
            }
        }

        return problems;
    }

    [Fact]
    public void After_Dispose_every_write_and_read_method_of_a_write_mode_packet_throws_ObjectDisposedException_D4_D6_D10_D12()
    {
        Message m = WriteMessage();
        m.Dispose();

        List<string> problems = WrongExceptions(m, WriteActions, typeof(ObjectDisposedException));
        problems.AddRange(WrongExceptions(m, ReadActions, typeof(ObjectDisposedException)));

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void After_Dispose_every_write_and_read_method_of_a_read_mode_packet_throws_ObjectDisposedException_R8_R9()
    {
        Message m = ReadMessage();
        m.Dispose();

        List<string> problems = WrongExceptions(m, WriteActions, typeof(ObjectDisposedException));
        problems.AddRange(WrongExceptions(m, ReadActions, typeof(ObjectDisposedException)));

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void A_write_mode_packet_that_is_not_disposed_throws_InvalidOperationException_from_every_read_method_W0()
    {
        Message m = WriteMessage();

        List<string> problems = WrongExceptions(m, ReadActions, typeof(InvalidOperationException));

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
        Assert.Equal("01020304", GoldenFile.ToHex(m.GetData()));
    }

    [Fact]
    public void A_read_mode_packet_that_is_not_disposed_throws_InvalidOperationException_from_every_write_method_and_GetData_R0_R1_R2()
    {
        Message m = ReadMessage();

        List<string> problems = WrongExceptions(m, WriteActions, typeof(InvalidOperationException));

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
        Assert.Equal((sbyte)1, m.ReadByte());
    }

    // ------------------------------------------------------------------ Cleanup/Dispose in read mode (R3..R11)

    [Fact]
    public void Cleanup_in_read_mode_does_not_reset_the_cursor_reading_continues_where_it_left_off_R3_R4_R5_R6()
    {
        Message m = ReadMessage();
        JavaDataInput reader = m.Reader();

        Assert.Equal((sbyte)1, m.ReadByte());
        m.Cleanup();
        Assert.Equal((sbyte)2, m.ReadByte());
        Assert.Same(reader, m.Reader());
        Assert.Equal(0x03040506, m.Reader().ReadInt());
        m.Cleanup();

        // measured on the JDK: readByte=1; cleanup; readByte=2; reader().readInt()=50595078 (=0x03040506); cleanup; readShort()=1800 (=0x0708)
        Assert.Equal((short)0x0708, m.ReadShort());
        Assert.Equal(2, m.Reader().Available);
    }

    [Fact]
    public void ReadUTF_with_a_declared_length_longer_than_the_remaining_data_throws_EOF_after_Cleanup_R7()
    {
        Message m = ReadMessage();
        m.ReadFully(new byte[8]);
        m.Cleanup();

        Assert.Throws<EndOfStreamException>(() => m.ReadUTF());
        Assert.Equal(0, m.Reader().Available);
    }

    [Fact]
    public void Dispose_in_read_mode_Command_remains_and_Cleanup_afterwards_does_not_fail_R10_R11()
    {
        Message m = ReadMessage();
        m.ReadByte();

        m.Dispose();
        m.Cleanup();

        Assert.Equal((sbyte)-27, m.Command);
    }

    // ------------------------------------------------------------------ Dispose is not IDisposable

    [Fact]
    public void Dispose_is_just_a_regular_method_not_IDisposable_so_there_is_no_accidental_using()
    {
        Message m = WriteMessage();

        Assert.False(m is IDisposable);
    }
}
