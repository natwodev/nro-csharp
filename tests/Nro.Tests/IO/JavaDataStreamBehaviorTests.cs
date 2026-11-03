using Nro.Core.IO;

namespace Nro.Tests.IO;

/// <summary>
/// Edge-case behaviour of <see cref="JavaDataOutput"/>/<see cref="JavaDataInput"/> MEASURED on a real JDK 17.0.19
/// (jshell on DataOutputStream/DataInputStream/ByteArray*Stream). Each test records a detail that a naive port very easily gets silently wrong.
/// </summary>
public class JavaDataStreamBehaviorTests
{
    private static byte[] Bytes(params int[] values) => values.Select(v => (byte)v).ToArray();

    // ------------------------------------------------------------------ JavaDataOutput

    [Fact]
    public void Multi_byte_numbers_are_written_big_endian()
    {
        var o = new JavaDataOutput();
        o.WriteShort(0x0102);
        o.WriteInt(0x03040506);
        o.WriteLong(0x0708090A0B0C0D0EL);
        o.WriteChar(0x0F10);

        Assert.Equal("0102030405060708090a0b0c0d0e0f10", GoldenFile.ToHex(o.ToArray()));
        Assert.Equal(16, o.Size);
    }

    [Fact]
    public void WriteByte_WriteShort_WriteChar_keep_only_the_low_bits_and_Write_int_keeps_the_low_8_bits()
    {
        var o = new JavaDataOutput();
        o.WriteByte(0x1FF);
        o.WriteByte(-1);
        o.WriteShort(0x12345);
        o.WriteShort(-2);
        o.WriteChar(0x1FFFF);
        o.Write(0x1234);
        o.Write(-129);

        // WriteByte(0x1FF)=ff, WriteByte(-1)=ff, WriteShort(0x12345)=2345, WriteShort(-2)=fffe, WriteChar(0x1FFFF)=ffff, Write(0x1234)=34, Write(-129)=7f
        Assert.Equal("ff" + "ff" + "2345" + "fffe" + "ffff" + "34" + "7f", GoldenFile.ToHex(o.ToArray()));
    }

    [Fact]
    public void WriteBoolean_writes_01_or_00()
    {
        var o = new JavaDataOutput();
        o.WriteBoolean(true);
        o.WriteBoolean(false);

        Assert.Equal("0100", GoldenFile.ToHex(o.ToArray()));
    }

    [Fact]
    public void WriteBytes_writes_only_the_low_byte_of_each_unit_and_WriteChars_writes_2_bytes()
    {
        var o = new JavaDataOutput();
        o.WriteBytes("éŁA");
        o.WriteChars("éA");
        o.WriteBytes(string.Empty);
        o.WriteChars(string.Empty);

        // measured on the JDK: writeBytes("éŁA") = e9 41 41; writeChars("éA") = 00e9 0041
        Assert.Equal("e94141" + "00e90041", GoldenFile.ToHex(o.ToArray()));
    }

    [Fact]
    public void WriteFloat_WriteDouble_normalise_every_NaN_but_preserve_other_values()
    {
        var o = new JavaDataOutput();
        o.WriteFloat(BitConverter.Int32BitsToSingle(0x7F800001));
        o.WriteFloat(BitConverter.Int32BitsToSingle(unchecked((int)0xFFC00000)));
        o.WriteFloat(-0.0f);
        o.WriteDouble(BitConverter.Int64BitsToDouble(0x7FF0000000000001L));
        o.WriteDouble(double.NaN);
        o.WriteDouble(-0.0);

        Assert.Equal(
            "7fc00000" + "7fc00000" + "80000000" + "7ff8000000000000" + "7ff8000000000000" + "8000000000000000",
            GoldenFile.ToHex(o.ToArray()));
    }

    [Fact]
    public void Write_array_with_an_invalid_range_throws_and_writes_nothing()
    {
        var o = new JavaDataOutput();
        o.WriteByte(9);

        Assert.Throws<ArgumentOutOfRangeException>(() => o.Write(new byte[3], 2, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.Write(new byte[3], -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.Write(new byte[3], 1, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => o.Write(new byte[3], 4, 0));
        Assert.Equal(1, o.Size);
        Assert.Equal("09", GoldenFile.ToHex(o.ToArray()));
    }

    [Fact]
    public void Write_array_with_a_valid_range_including_length_0_and_ranges_at_the_boundary()
    {
        var o = new JavaDataOutput();
        o.Write(Bytes(1, 2, 3), 1, 2);
        o.Write(Bytes(7, 8, 9), 3, 0);
        o.Write(Bytes(4, 5));
        o.Write(Array.Empty<byte>());

        Assert.Equal("0203" + "0405", GoldenFile.ToHex(o.ToArray()));
    }

    [Fact]
    public void Null_argument_throws_ArgumentNullException()
    {
        var o = new JavaDataOutput();

        Assert.Throws<ArgumentNullException>(() => o.Write((byte[])null!));
        Assert.Throws<ArgumentNullException>(() => o.Write(null!, 0, 0));
        Assert.Throws<ArgumentNullException>(() => o.WriteUTF(null!));
        Assert.Throws<ArgumentNullException>(() => o.WriteBytes(null!));
        Assert.Throws<ArgumentNullException>(() => o.WriteChars(null!));
        Assert.Equal(0, o.Size);
    }

    [Fact]
    public void WriteUTF_too_long_writes_no_bytes_and_preserves_earlier_data()
    {
        var o = new JavaDataOutput();
        o.WriteByte(1);

        var ex = Assert.Throws<UtfDataFormatException>(() => o.WriteUTF(new string('a', 65536)));

        Assert.Equal("encoded string (aaaaaaaa...aaaaaaaa) too long: 65536 bytes", ex.Message);
        Assert.IsAssignableFrom<IOException>(ex);
        Assert.Equal("01", GoldenFile.ToHex(o.ToArray()));
        Assert.Equal(1, o.Size);
    }

    [Fact]
    public void ToArray_returns_a_new_copy_on_every_call()
    {
        var o = new JavaDataOutput();
        o.WriteInt(0x01020304);

        byte[] a = o.ToArray();
        a[0] = 0x99;
        byte[] b = o.ToArray();
        byte[] c = o.ToArray();

        Assert.Equal("01020304", GoldenFile.ToHex(b));
        Assert.NotSame(b, c);
        Assert.Equal(b, c);
    }

    [Fact]
    public void ToArray_of_an_empty_stream_is_an_empty_array_not_null()
    {
        var o = new JavaDataOutput();

        Assert.NotNull(o.ToArray());
        Assert.Empty(o.ToArray());
        Assert.Equal(0, o.Size);
    }

    [Fact]
    public void Close_and_Flush_do_nothing_and_writing_after_closing_still_works()
    {
        var o = new JavaDataOutput();
        o.WriteInt(0x01020304);

        o.Flush();
        o.Close();
        o.Close();
        o.WriteShort(0x0506);
        o.Flush();

        // measured on the JDK: dos.close(); dos.close(); os.close(); dos.writeShort(...) -> size 6, bytes 010203040506
        Assert.Equal("010203040506", GoldenFile.ToHex(o.ToArray()));
        Assert.Equal(6, o.Size);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(32)]
    [InlineData(1000)]
    public void Buffer_grows_automatically_with_correct_content_from_any_initial_capacity(int capacity)
    {
        var o = new JavaDataOutput(capacity);
        var expected = new List<byte>();
        for (int i = 0; i < 100_000; i++)
        {
            byte v = unchecked((byte)((i * 31) + (i >> 8)));
            o.WriteByte(v);
            expected.Add(v);
        }

        o.Write(Bytes(1, 2, 3), 0, 3);
        expected.AddRange(new byte[] { 1, 2, 3 });
        o.WriteUTF(new string('x', 5000));
        expected.Add(0x13);
        expected.Add(0x88);
        expected.AddRange(Enumerable.Repeat((byte)'x', 5000));

        Assert.Equal(expected.Count, o.Size);
        Assert.Equal(expected.ToArray(), o.ToArray());
    }

    [Fact]
    public void Negative_initial_capacity_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new JavaDataOutput(-1));
    }

    // ------------------------------------------------------------------ JavaDataInput

    [Fact]
    public void Reads_signed_and_unsigned_numbers()
    {
        var i = new JavaDataInput(Bytes(0xFF, 0x80, 0x7F, 0xFF, 0xFE, 0x80, 0x00));

        Assert.Equal((sbyte)-1, i.ReadByte());
        Assert.Equal(128, i.ReadUnsignedByte());
        Assert.Equal(127, i.ReadUnsignedByte());
        Assert.Equal((short)-2, i.ReadShort());
        Assert.Equal(32768, new JavaDataInput(Bytes(0x80, 0x00)).ReadUnsignedShort());
        Assert.Equal(2, i.Available);
    }

    [Fact]
    public void ReadBoolean_is_true_when_the_byte_is_non_zero()
    {
        var i = new JavaDataInput(Bytes(0x00, 0x01, 0x02, 0x80, 0xFF));

        Assert.False(i.ReadBoolean());
        Assert.True(i.ReadBoolean());
        Assert.True(i.ReadBoolean());
        Assert.True(i.ReadBoolean());
        Assert.True(i.ReadBoolean());
    }

    [Fact]
    public void ReadFloat_ReadDouble_preserve_every_bit_including_the_NaN_payload()
    {
        var i = new JavaDataInput(Bytes(0x7F, 0x80, 0x00, 0x01, 0x7F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01));

        Assert.Equal(0x7F800001, BitConverter.SingleToInt32Bits(i.ReadFloat()));
        Assert.Equal(0x7FF0000000000001L, BitConverter.DoubleToInt64Bits(i.ReadDouble()));
    }

    [Fact]
    public void Read_returns_minus_1_at_the_end_of_the_data_even_when_asked_for_0_bytes()
    {
        var i = new JavaDataInput(Array.Empty<byte>());

        // measured on the JDK: read(b,0,0) at the end = -1 (differs from the general InputStream convention), read(b) = -1, read() = -1
        Assert.Equal(-1, i.Read());
        Assert.Equal(-1, i.Read(new byte[0], 0, 0));
        Assert.Equal(-1, i.Read(new byte[3]));
        Assert.Equal(-1, i.Read(new byte[3], 1, 2));
    }

    [Fact]
    public void Read_array_returns_the_number_of_bytes_read_which_may_be_fewer_than_requested()
    {
        var i = new JavaDataInput(Bytes(1, 2, 3, 4, 5));
        var b = new byte[8];

        Assert.Equal(0, i.Read(b, 0, 0));
        Assert.Equal(3, i.Read(b, 2, 3));
        Assert.Equal(2, i.Read(b, 0, 7));
        Assert.Equal(-1, i.Read(b));

        // Read(b,2,3) writes 01 02 03 into b[2..5); Read(b,0,7) has only 2 bytes left (04 05) so it writes into b[0..2)
        Assert.Equal("0405010203000000", GoldenFile.ToHex(b));
    }

    [Fact]
    public void Reading_fixed_size_numbers_with_too_little_data_throws_EOF_after_consuming_the_remainder()
    {
        // measured on the JDK: readShort with 1 byte -> EOF, available()=0; readLong with 3 bytes -> EOF, available()=0
        var shortCase = new JavaDataInput(Bytes(9));
        Assert.Throws<EndOfStreamException>(() => shortCase.ReadShort());
        Assert.Equal(0, shortCase.Available);

        var longCase = new JavaDataInput(Bytes(1, 2, 3));
        Assert.Throws<EndOfStreamException>(() => longCase.ReadLong());
        Assert.Equal(0, longCase.Available);

        var intCase = new JavaDataInput(Bytes(1, 2, 3));
        Assert.Throws<EndOfStreamException>(() => intCase.ReadInt());
        Assert.Equal(0, intCase.Available);

        var charCase = new JavaDataInput(Bytes(1));
        Assert.Throws<EndOfStreamException>(() => charCase.ReadChar());
        Assert.Equal(0, charCase.Available);

        var unsignedCase = new JavaDataInput(Bytes(1));
        Assert.Throws<EndOfStreamException>(() => unsignedCase.ReadUnsignedShort());
        Assert.Equal(0, unsignedCase.Available);

        var floatCase = new JavaDataInput(Bytes(1, 2));
        Assert.Throws<EndOfStreamException>(() => floatCase.ReadFloat());
        Assert.Equal(0, floatCase.Available);

        var doubleCase = new JavaDataInput(Bytes(1, 2, 3, 4, 5, 6, 7));
        Assert.Throws<EndOfStreamException>(() => doubleCase.ReadDouble());
        Assert.Equal(0, doubleCase.Available);

        var empty = new JavaDataInput(Array.Empty<byte>());
        Assert.Throws<EndOfStreamException>(() => empty.ReadByte());
        Assert.Throws<EndOfStreamException>(() => empty.ReadUnsignedByte());
        Assert.Throws<EndOfStreamException>(() => empty.ReadBoolean());
    }

    [Fact]
    public void ReadFully_with_too_little_data_leaves_the_bytes_read_in_the_array_and_throws_EOF()
    {
        var i = new JavaDataInput(Bytes(1, 2, 3));
        var buf = new byte[5];

        Assert.Throws<EndOfStreamException>(() => i.ReadFully(buf));

        // measured on the JDK: buf = 0102030000, available() = 0
        Assert.Equal("0102030000", GoldenFile.ToHex(buf));
        Assert.Equal(0, i.Available);
    }

    [Fact]
    public void ReadFully_with_length_0_never_throws_even_at_the_end_of_the_data()
    {
        var i = new JavaDataInput(Array.Empty<byte>());

        i.ReadFully(Array.Empty<byte>());
        i.ReadFully(new byte[4], 1, 0);
        i.ReadFully(new byte[4], 4, 0);

        Assert.Equal(0, i.Available);
    }

    [Fact]
    public void ReadFully_reads_exactly_the_specified_range()
    {
        var i = new JavaDataInput(Bytes(9, 8, 7, 6));
        var buf = new byte[6];

        i.ReadFully(buf, 1, 3);

        Assert.Equal("000908070000", GoldenFile.ToHex(buf));
        Assert.Equal(1, i.Available);
    }

    [Fact]
    public void Reading_into_an_array_with_an_invalid_range_throws_and_a_null_argument_throws_ArgumentNullException()
    {
        var i = new JavaDataInput(Bytes(1, 2, 3));

        Assert.Throws<ArgumentOutOfRangeException>(() => i.Read(new byte[3], 2, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => i.Read(new byte[3], -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => i.ReadFully(new byte[3], 2, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => i.ReadFully(new byte[3], 1, -1));
        Assert.Throws<ArgumentNullException>(() => i.Read(null!));
        Assert.Throws<ArgumentNullException>(() => i.Read(null!, 0, 0));
        Assert.Throws<ArgumentNullException>(() => i.ReadFully(null!));
        Assert.Throws<ArgumentNullException>(() => new JavaDataInput(null!));
        Assert.Equal(3, i.Available);
    }

    [Fact]
    public void SkipBytes_skips_at_most_n_bytes_and_does_not_throw_at_the_end_of_the_data()
    {
        var i = new JavaDataInput(Bytes(1, 2, 3, 4, 5));

        // measured on the JDK: skipBytes(-3)=0 skipBytes(0)=0 skipBytes(2)=2 skipBytes(10)=3 skipBytes(1)=0
        Assert.Equal(0, i.SkipBytes(-3));
        Assert.Equal(0, i.SkipBytes(0));
        Assert.Equal(2, i.SkipBytes(2));
        Assert.Equal(3, i.SkipBytes(10));
        Assert.Equal(0, i.SkipBytes(1));
        Assert.Equal(0, i.Available);
    }

    [Fact]
    public void Close_does_nothing_and_reading_after_closing_still_works()
    {
        var i = new JavaDataInput(Bytes(1, 2, 3, 4, 5));
        Assert.Equal((sbyte)1, i.ReadByte());

        i.Close();
        i.Close();

        // measured on the JDK: after dis.close() + is.close(): readByte()=2, available()=3
        Assert.Equal((sbyte)2, i.ReadByte());
        Assert.Equal(3, i.Available);
    }

    [Fact]
    public void Constructor_does_not_copy_the_source_array_like_ByteArrayInputStream()
    {
        var source = Bytes(10, 20, 30);
        var i = new JavaDataInput(source);

        source[0] = 77;

        // measured on the real JDK via Message: modifying the source array after creation makes the reader see the new value (77)
        Assert.Equal((sbyte)77, i.ReadByte());
    }

    // ------------------------------------------------------------------ ReadUTF: position of the cursor after an error

    [Fact]
    public void ReadUTF_malformed_input_consumes_the_declared_length_but_no_more()
    {
        // length 2: C2 41 (the next byte is not 10xxxxxx) followed by 2 extra bytes
        var i = new JavaDataInput(GoldenFile.Hex("0002c241aabb"));

        var ex = Assert.Throws<UtfDataFormatException>(() => i.ReadUTF());

        Assert.Equal("malformed input around byte 2", ex.Message);
        Assert.Equal(2, i.Available);
    }

    [Fact]
    public void ReadUTF_declared_length_longer_than_the_data_consumes_everything_then_throws_EOF_not_a_format_error()
    {
        var i = new JavaDataInput(GoldenFile.Hex("0005414243"));

        Assert.Throws<EndOfStreamException>(() => i.ReadUTF());
        Assert.Equal(0, i.Available);
    }

    [Fact]
    public void ReadUTF_missing_the_whole_length_prefix_throws_EOF()
    {
        var i = new JavaDataInput(GoldenFile.Hex("00"));

        Assert.Throws<EndOfStreamException>(() => i.ReadUTF());
        Assert.Equal(0, i.Available);
    }

    /// <summary>The expected result written as hex of each UTF-16 unit (4 digits per unit) so the test name does not contain lone surrogates or NUL.</summary>
    [Theory]
    [InlineData("0002c080", "0000")]
    [InlineData("000100", "0000")]
    [InlineData("0002c181", "0041")]
    [InlineData("0003e08181", "0041")]
    [InlineData("0006eda0bdedb880", "d83dde00")]
    [InlineData("0003eda080", "d800")]
    [InlineData("0003edb080", "dc00")]
    [InlineData("0006edb080eda080", "dc00d800")]
    public void ReadUTF_accepts_NUL_overlong_forms_and_surrogates_like_the_JDK(string hex, string expectedUnitsHex)
    {
        string actual = new JavaDataInput(GoldenFile.Hex(hex)).ReadUTF();

        Assert.Equal(expectedUnitsHex, string.Concat(actual.Select(ch => ((int)ch).ToString("x4"))));
    }

    [Theory]
    [InlineData("0004f09f9880")]
    [InlineData("0001f0")]
    [InlineData("0001ff")]
    [InlineData("000180")]
    [InlineData("0001bf")]
    public void ReadUTF_rejects_standard_4_byte_UTF8_and_invalid_lead_bytes(string hex)
    {
        Assert.Throws<UtfDataFormatException>(() => new JavaDataInput(GoldenFile.Hex(hex)).ReadUTF());
    }

    [Fact]
    public void WriteUTF_writes_NUL_as_C0_80_and_non_BMP_as_two_3_byte_sequences_unlike_Encoding_UTF8()
    {
        var o = new JavaDataOutput();
        o.WriteUTF("\u0000");
        o.WriteUTF("😀");
        o.WriteUTF("\ud800");

        Assert.Equal("0002c080" + "0006eda0bdedb880" + "0003eda080", GoldenFile.ToHex(o.ToArray()));
        Assert.NotEqual(
            GoldenFile.ToHex(System.Text.Encoding.UTF8.GetBytes("😀")),
            GoldenFile.ToHex(o.ToArray().AsSpan(6, 6)));
    }
}
