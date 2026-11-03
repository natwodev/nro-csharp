using System.Text.Json;
using Nro.Core.IO;
using Xunit.Abstractions;

namespace Nro.Tests.IO;

/// <summary>
/// <see cref="Message"/>: constructors, writing through Message then comparing <c>GetData()</c> with the vectors, reading through <c>Message(cmd, data)</c>,
/// <c>WriteCris/WriteShortToLong/WriteByteToLong</c>. The "expected" values come from the JDK vectors (tests/golden) or from measuring the project's own
/// <c>network.io.Message</c> class run on JDK 17 ("measured on the JDK" comment in each test).
/// </summary>
public class MessageTests
{
    private readonly ITestOutputHelper _output;

    public MessageTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ------------------------------------------------------------------ constructors

    [Theory]
    [InlineData(-218, 38)]
    [InlineData(200, -56)]
    [InlineData(255, -1)]
    [InlineData(256, 0)]
    [InlineData(-129, 127)]
    [InlineData(-27, -27)]
    [InlineData(128, -128)]
    [InlineData(127, 127)]
    [InlineData(-128, -128)]
    [InlineData(0, 0)]
    [InlineData(-1, -1)]
    [InlineData(511, -1)]
    [InlineData(-512, 0)]
    public void Int_constructor_truncates_like_Java_byte(int command, int expected)
    {
        // measured on the JDK: new Message(-218).command = 38, (200) = -56, (255) = -1, (256) = 0, (-129) = 127, (-27) = -27, (128) = -128
        Assert.Equal((sbyte)expected, new Message(command).Command);
    }

    [Fact]
    public void Int_constructor_truncates_correctly_over_the_whole_range_minus_5000_to_5000()
    {
        for (int i = -5000; i <= 5000; i++)
        {
            sbyte expected = (sbyte)(((i + 128) & 0xFF) - 128);
            if (new Message(i).Command != expected)
            {
                Assert.Fail($"Message({i}).Command = {new Message(i).Command}, expected {expected}");
            }
        }
    }

    [Fact]
    public void Sbyte_constructor_preserves_the_command_in_both_modes()
    {
        for (int v = sbyte.MinValue; v <= sbyte.MaxValue; v++)
        {
            Assert.Equal((sbyte)v, new Message((sbyte)v).Command);
            Assert.Equal((sbyte)v, new Message((sbyte)v, new byte[1]).Command);
        }
    }

    [Fact]
    public void Command_is_writable_like_a_public_Java_field()
    {
        var m = new Message(5);

        m.Command = -100;

        Assert.Equal((sbyte)-100, m.Command);
    }

    [Fact]
    public void Read_mode_constructor_with_null_data_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Message(1, null!));
    }

    [Fact]
    public void Read_mode_constructor_does_not_copy_the_source_array()
    {
        var source = new byte[] { 10, 20, 30 };
        var m = new Message(1, source);

        source[0] = 77;

        // measured on the JDK: modifying the source array after creation makes ReadByte() see 77
        Assert.Equal((sbyte)77, m.ReadByte());
    }

    // ------------------------------------------------------------------ GetData

    [Fact]
    public void GetData_of_a_packet_with_nothing_written_is_an_empty_array_not_null()
    {
        byte[] data = new Message(3).GetData();

        Assert.NotNull(data);
        Assert.Empty(data);
    }

    [Fact]
    public void GetData_returns_a_new_copy_on_every_call_so_in_place_encryption_does_not_corrupt_the_shared_packet()
    {
        var m = new Message(9);
        m.WriteInt(0x01020304);
        m.WriteUTF("Chào");
        byte[] original = m.GetData();

        // simulate two sender threads (two players) taking the data of ONE packet and XOR-ing it in place like MessageSendCollect.doSendMessage
        byte[] forPlayerA = m.GetData();
        for (int i = 0; i < forPlayerA.Length; i++)
        {
            forPlayerA[i] ^= 0xFF;
        }

        byte[] forPlayerB = m.GetData();

        Assert.NotSame(forPlayerA, forPlayerB);
        Assert.Equal(original, forPlayerB);
        Assert.NotEqual(original, forPlayerA);
        Assert.Equal(original, m.GetData());
    }

    [Fact]
    public void GetData_from_many_threads_at_once_gives_each_thread_its_own_copy()
    {
        var m = new Message(9);
        for (int i = 0; i < 200; i++)
        {
            m.WriteInt(i * 7919);
        }

        // an INDEPENDENT copy of the original data: if GetData() returned the same array on every call, the threads would XOR that very array
        // and comparing against an "original" that is the same object would always succeed (a blind test); Clone() keeps it separate so that bug shows up.
        byte[] original = (byte[])m.GetData().Clone();
        var bad = new System.Collections.Concurrent.ConcurrentBag<int>();

        Parallel.For(0, 64, worker =>
        {
            for (int round = 0; round < 200; round++)
            {
                byte[] mine = m.GetData();
                if (!mine.AsSpan().SequenceEqual(original))
                {
                    bad.Add(worker);
                }

                for (int i = 0; i < mine.Length; i++)
                {
                    mine[i] ^= (byte)(worker + 1);
                }

                if (round % 17 == 0)
                {
                    m.Cleanup();
                }
            }
        });

        Assert.Empty(bad);
        Assert.Equal(original, m.GetData());
    }

    [Fact]
    public void Writer_and_Reader_return_the_same_object_on_every_call()
    {
        var w = new Message(1);
        var r = new Message(1, new byte[4]);

        Assert.Same(w.Writer(), w.Writer());
        Assert.Same(r.Reader(), r.Reader());
    }

    [Fact]
    public void Writing_through_Message_and_through_Writer_goes_into_the_same_buffer()
    {
        var m = new Message(1);

        m.WriteByte(1);
        m.Writer().WriteByte(2);
        m.WriteShort(0x0304);
        m.Writer().WriteShort(0x0506);

        Assert.Equal("010203040506", GoldenFile.ToHex(m.GetData()));
        Assert.Equal(6, m.Writer().Size);
    }

    [Fact]
    public void Reading_through_Message_and_through_Reader_shares_the_cursor()
    {
        var m = new Message(1, new byte[] { 1, 2, 3, 4, 5, 6 });

        Assert.Equal((sbyte)1, m.ReadByte());
        Assert.Equal((sbyte)2, m.Reader().ReadByte());
        Assert.Equal((short)0x0304, m.ReadShort());
        Assert.Equal(2, m.Reader().Available);
        Assert.Equal(0x0506, m.Reader().ReadUnsignedShort());
    }

    // ------------------------------------------------------------------ WriteCris & co (measured on the JDK via the real network.io.Message)

    [Fact]
    public void WriteCris_true_writes_an_int_truncated_to_the_low_32_bits_false_writes_a_long()
    {
        var a = new Message(1);
        a.WriteCris(0x123456789AL, true);
        var b = new Message(1);
        b.WriteCris(0x123456789AL, false);

        // measured on the JDK: writeCris(0x123456789AL,true) = 3456789a; (…,false) = 000000123456789a
        Assert.Equal("3456789a", GoldenFile.ToHex(a.GetData()));
        Assert.Equal("000000123456789a", GoldenFile.ToHex(b.GetData()));
    }

    [Fact]
    public void WriteShortToLong_and_WriteByteToLong_truncate_like_a_Java_cast()
    {
        var a = new Message(1);
        a.WriteShortToLong(-2, true);
        var b = new Message(1);
        b.WriteByteToLong(0x1FF, true);
        var c = new Message(1);
        c.WriteShortToLong(-2, false);
        var d = new Message(1);
        d.WriteByteToLong(0x1FF, false);

        // measured on the JDK: writeShortToLong(-2,true) = fffe; writeByteToLong(0x1FF,true) = ff
        Assert.Equal("fffe", GoldenFile.ToHex(a.GetData()));
        Assert.Equal("ff", GoldenFile.ToHex(b.GetData()));
        Assert.Equal("fffffffffffffffe", GoldenFile.ToHex(c.GetData()));
        Assert.Equal("00000000000001ff", GoldenFile.ToHex(d.GetData()));
    }

    [Fact]
    public void The_array_Write_methods_and_int_Write_of_Message_match_the_JDK_measurements()
    {
        var m = new Message(1);

        m.Write(0x1234);
        m.Write(new byte[] { 1, 2, 3 }, 1, 2);
        m.Write(new byte[] { 9 });
        m.WriteBytes("éŁA");
        m.WriteChars("éA");

        // measured on the JDK: write(0x1234)=34; write({1,2,3},1,2)=0203; writeBytes("éŁA")=e94141; writeChars("éA")=00e90041
        Assert.Equal("34" + "0203" + "09" + "e94141" + "00e90041", GoldenFile.ToHex(m.GetData()));
    }

    [Fact]
    public void Write_with_invalid_arguments_on_Message_throws_and_writes_nothing_and_an_over_long_UTF_preserves_the_data()
    {
        var m = new Message(1);
        m.WriteByte(1);

        Assert.Throws<ArgumentNullException>(() => m.Write((byte[])null!));
        Assert.Throws<ArgumentNullException>(() => m.WriteUTF(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => m.Write(new byte[3], 2, 5));
        Assert.Throws<UtfDataFormatException>(() => m.WriteUTF(new string('a', 65536)));

        // measured on the JDK (X10, X13): after the errors above the data is still only the byte 01
        Assert.Equal("01", GoldenFile.ToHex(m.GetData()));
    }

    [Fact]
    public void Writer_Flush_and_Close_then_writing_again_still_produces_data()
    {
        var m = new Message(1);
        m.WriteByte(3);

        m.Writer().Flush();
        m.Writer().Close();
        m.WriteByte(4);

        // measured on the JDK (X14, X15): flush -> 03; close then writeByte(4) -> 0304
        Assert.Equal("0304", GoldenFile.ToHex(m.GetData()));
    }

    // ------------------------------------------------------------------ key packet -27 (KeyHandler.sendKey/setKey) versus handshake-key.json

    [Fact]
    public void Key_packet_27_built_with_Message_matches_the_Java_payload_and_the_client_derives_the_right_key()
    {
        using JsonDocument doc = GoldenFile.Open("handshake-key.json");
        sbyte getSessionIdCmd = (sbyte)doc.RootElement.GetProperty("getSessionIdCmd").GetInt32();
        int compared = 0;

        foreach (JsonElement c in doc.RootElement.GetProperty("cases").EnumerateArray())
        {
            string id = c.GetProperty("id").GetString()!;
            byte[] keyBytes = GoldenFile.Hex(c.GetProperty("keyHex").GetString()!);
            sbyte[] keys = keyBytes.Select(b => unchecked((sbyte)b)).ToArray();
            string expectedPayload = c.GetProperty("keyPacket").GetProperty("payloadHex").GetString()!;

            // KeyHandler.sendKey: writeByte(KEYS.length); writeByte(KEYS[0]); writeByte(KEYS[i] ^ KEYS[i - 1])
            var msg = new Message(-27);
            msg.Writer().WriteByte(keys.Length);
            msg.Writer().WriteByte(keys[0]);
            for (int i = 1; i < keys.Length; i++)
            {
                msg.Writer().WriteByte(keys[i] ^ keys[i - 1]);
            }

            Assert.Equal(getSessionIdCmd, msg.Command);
            Assert.True(expectedPayload == GoldenFile.ToHex(msg.GetData()), $"{id}: payload differs from the JDK");

            // KeyHandler.setKey on the receiving side: read len, read the key, KEYS[j + 1] ^= KEYS[j]
            var received = new Message(msg.Command, msg.GetData());
            int len = received.Reader().ReadByte();
            var derived = new sbyte[len];
            for (int i = 0; i < len; i++)
            {
                derived[i] = received.Reader().ReadByte();
            }

            for (int j = 0; j < derived.Length - 1; j++)
            {
                derived[j + 1] = (sbyte)(derived[j + 1] ^ derived[j]);
            }

            Assert.True(
                c.GetProperty("clientDerivedKeyHex").GetString() == GoldenFile.ToHex(derived.Select(b => unchecked((byte)b)).ToArray()),
                $"{id}: client-derived key differs from the JDK");
            Assert.Equal(0, received.Reader().Available);
            compared++;
        }

        _output.WriteLine($"handshake-key.json: built and read back {compared} key packets -27 with Message");
        Assert.True(compared >= 3, "key cases are missing from handshake-key.json");
    }
}
