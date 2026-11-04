using Nro.Game;

namespace Nro.Tests.Unit.Game.World.Maps;

public class SnapshotListTests
{
    [Fact]
    public void Add_Insert_Remove_Set_FollowListSemantics()
    {
        SnapshotList<int> list = new();
        list.Add(1);
        list.Add(3);
        list.Insert(1, 2);
        list.Insert(0, 0);
        list.Insert(4, 4);
        Assert.Equal([0, 1, 2, 3, 4], list.ToArray());
        Assert.True(list.Remove(2));
        Assert.False(list.Remove(99));
        list.RemoveAt(0);
        list.Set(0, 10);
        Assert.Equal([10, 3, 4], list.ToArray());
        Assert.Equal(3, list.Count);
        Assert.True(list.Contains(3));
        Assert.Equal(2, list.IndexOf(4));
        list.Clear();
        Assert.True(list.IsEmpty);
    }

    [Fact]
    public void Remove_RemovesOnlyTheFirstEqualElement()
    {
        SnapshotList<string> list = new();
        list.Add("a");
        list.Add("b");
        list.Add("a");
        list.Remove("a");
        Assert.Equal(["b", "a"], list.ToArray());
    }

    [Fact]
    public void BadIndexes_ThrowLikeJava()
    {
        SnapshotList<int> list = new();
        list.Add(1);
        Assert.Throws<IndexOutOfRangeException>(() => list[1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(2, 5));
        Assert.Throws<IndexOutOfRangeException>(() => list.Set(1, 5));
    }

    [Fact]
    public void ChangingWhileEnumerating_NeverThrows_AndTheEnumerationKeepsItsSnapshot()
    {
        SnapshotList<int> list = new();
        for (int i = 0; i < 5; i++)
        {
            list.Add(i);
        }

        List<int> seen = [];
        foreach (int item in list)
        {
            seen.Add(item);
            list.Remove(item);
            list.Add(item + 100);
            list.Insert(0, -item);
        }

        Assert.Equal([0, 1, 2, 3, 4], seen);
        Assert.Equal(10, list.Count);
    }

    [Fact]
    public async Task ParallelWritersAndReaders_LoseNothingAndNeverThrow()
    {
        SnapshotList<int> list = new();
        int writers = 8;
        int perWriter = 500;
        using CancellationTokenSource stop = new();
        Task reader = Task.Run(() =>
        {
            long sum = 0;
            while (!stop.IsCancellationRequested)
            {
                foreach (int v in list)
                {
                    sum += v;
                }

                _ = list.Count;
            }

            return sum;
        });
        Task[] tasks = Enumerable.Range(0, writers).Select(w => Task.Run(() =>
        {
            for (int i = 0; i < perWriter; i++)
            {
                list.Add((w * perWriter) + i);
            }
        })).ToArray();
        await Task.WhenAll(tasks);
        await stop.CancelAsync();
        await reader;
        Assert.Equal(writers * perWriter, list.Count);
        Assert.Equal(writers * perWriter, list.Distinct().Count());
    }
}
