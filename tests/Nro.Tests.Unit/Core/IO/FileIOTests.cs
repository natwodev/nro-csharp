using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Tests.Unit.Core.IO;

/// <summary><see cref="FileIO"/>: every test works inside its own temporary folder, which is deleted afterwards. Nothing is written elsewhere.</summary>
public sealed class FileIOTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("nro-fileio-").FullName;

    public void Dispose()
    {
        Directory.Delete(_dir, recursive: true);
    }

    private string P(string name) => Path.Combine(_dir, name);

    [Fact]
    public void ReadFile_returns_all_bytes()
    {
        byte[] all = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        File.WriteAllBytes(P("a.bin"), all);
        Assert.Equal(all, FileIO.ReadFile(P("a.bin")));
    }

    [Fact]
    public void ReadFile_of_an_empty_file_is_an_empty_array_not_null()
    {
        File.WriteAllBytes(P("empty"), []);
        byte[]? data = FileIO.ReadFile(P("empty"));
        Assert.NotNull(data);
        Assert.Empty(data);
    }

    [Fact]
    public void ReadFile_reads_a_large_file()
    {
        byte[] big = new byte[3 * 1024 * 1024 + 17];
        new Random(1).NextBytes(big);
        File.WriteAllBytes(P("big"), big);
        Assert.Equal(big, FileIO.ReadFile(P("big")));
    }

    [Fact]
    public void ReadFile_returns_null_when_the_file_cannot_be_read_like_Java()
    {
        Assert.Null(FileIO.ReadFile(P("missing.bin")));
        Assert.Null(FileIO.ReadFile(P("no-such-folder/missing.bin")));
        Assert.Null(FileIO.ReadFile(_dir));          // a folder
        Assert.Null(FileIO.ReadFile(string.Empty));  // Java: FileNotFoundException
        Assert.Null(FileIO.ReadFile("bad\0name"));   // illegal character in the path
    }

    [Fact]
    public void ReadFile_returns_null_for_a_file_that_is_locked_for_writing_by_someone_else()
    {
        string path = P("locked.bin");
        File.WriteAllBytes(path, [1, 2, 3]);
        using var lockHolder = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.Null(FileIO.ReadFile(path));
    }

    [Fact]
    public void A_null_path_throws_like_the_Java_NullPointerException()
    {
        Assert.Throws<ArgumentNullException>(() => FileIO.ReadFile(null!));
        Assert.Throws<ArgumentNullException>(() => FileIO.LoadFile(null!));
        Assert.Throws<ArgumentNullException>(() => FileIO.WriteFile(null!, [1]));
    }

    [Fact]
    public void LoadFile_returns_the_content_in_a_memory_stream_or_null()
    {
        byte[] data = [10, 20, 30, 0, 255];
        File.WriteAllBytes(P("l.bin"), data);
        using (MemoryStream? stream = FileIO.LoadFile(P("l.bin")))
        {
            Assert.NotNull(stream);
            Assert.Equal(data, stream.ToArray());
        }

        Assert.Null(FileIO.LoadFile(P("missing")));
        File.WriteAllBytes(P("empty"), []);
        using MemoryStream? empty = FileIO.LoadFile(P("empty"));
        Assert.NotNull(empty);
        Assert.Equal(0, empty.Length);
    }

    [Fact]
    public void WriteFile_creates_and_returns_true()
    {
        byte[] data = Enumerable.Range(0, 1000).Select(i => (byte)(i * 7)).ToArray();
        Assert.True(FileIO.WriteFile(P("w.bin"), data));
        Assert.Equal(data, File.ReadAllBytes(P("w.bin")));
    }

    [Fact]
    public void WriteFile_replaces_a_longer_existing_file_without_leaving_old_bytes()
    {
        File.WriteAllBytes(P("r.bin"), new byte[500]);
        Assert.True(FileIO.WriteFile(P("r.bin"), [1, 2, 3]));
        Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(P("r.bin")));
    }

    [Fact]
    public void WriteFile_of_an_empty_array_leaves_an_empty_file()
    {
        File.WriteAllBytes(P("e.bin"), [9, 9]);
        Assert.True(FileIO.WriteFile(P("e.bin"), []));
        Assert.Empty(File.ReadAllBytes(P("e.bin")));
    }

    [Fact]
    public void WriteFile_does_not_create_missing_folders_and_reports_false()
    {
        string path = P("missing-folder/x.bin");
        Assert.False(FileIO.WriteFile(path, [1]));
        Assert.False(Directory.Exists(P("missing-folder")));
        Assert.False(FileIO.WriteFile(string.Empty, [1]));
        Assert.False(FileIO.WriteFile(_dir, [1])); // a folder
    }

    [Fact]
    public void WriteFile_with_a_null_array_throws_before_touching_the_file()
    {
        File.WriteAllBytes(P("keep.bin"), [5, 6, 7]);
        Assert.Throws<ArgumentNullException>(() => FileIO.WriteFile(P("keep.bin"), null!));
        Assert.Equal(new byte[] { 5, 6, 7 }, File.ReadAllBytes(P("keep.bin")));
        Assert.Throws<ArgumentNullException>(() => FileIO.WriteFile(P("new.bin"), null!));
        Assert.False(File.Exists(P("new.bin")));
    }

    [Fact]
    public void WriteFile_to_a_file_locked_by_someone_else_reports_false()
    {
        string path = P("locked2.bin");
        File.WriteAllBytes(path, [1]);
        using var holder = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.False(FileIO.WriteFile(path, [2]));
    }

    [Fact]
    public void Write_then_read_round_trips_a_binary_asset_like_the_data_files_of_the_game()
    {
        byte[] data = new byte[70_000];
        new Random(42).NextBytes(data);
        Assert.True(FileIO.WriteFile(P("data.bin"), data));
        Assert.Equal(data, FileIO.ReadFile(P("data.bin")));
    }
}
