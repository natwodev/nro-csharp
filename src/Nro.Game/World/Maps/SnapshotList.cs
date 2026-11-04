using System.Collections;

namespace Nro.Game.World.Maps;

// Danh sách snapshot cho quyết định D1: mỗi thay đổi tạo mảng mới rồi công bố nguyên tử để tick zone, xử lý gói và broadcast luôn đọc được ảnh nhất quán.
// Nguồn Java: nro/map/Map.java và nro/map/Zone.java (lớp phụ thay ArrayList; khác biệt có chủ đích: duyệt không ném ConcurrentModificationException hay hỏng dữ liệu; snapshot không thấy thay đổi sau lúc bắt đầu, như Java sao chép new ArrayList<>(list)).
// Người ghi lấy khóa ngắn để không mất cập nhật đồng thời; thay đổi O(n), đọc không tốn sao chép, phù hợp zone ít thay đổi nhưng nhiều lần đọc.
public sealed class SnapshotList<T> : IReadOnlyList<T>
{
    private readonly object _gate = new();
    private T[] _items = [];

    // Số phần tử trong snapshot hiện tại.
    public int Count => Volatile.Read(ref _items).Length;

    // Phần tử tại index của snapshot hiện tại; ngoài phạm vi sẽ ném lỗi như Java IndexOutOfBoundsException.
    public T this[int index] => Volatile.Read(ref _items)[index];

    // Đúng khi snapshot rỗng (Java: isEmpty).
    public bool IsEmpty => Count == 0;

    // Thêm phần tử cuối danh sách (Java: add(e)).
    public void Add(T item)
    {
        lock (_gate)
        {
            T[] old = _items;
            T[] next = new T[old.Length + 1];
            Array.Copy(old, next, old.Length);
            next[old.Length] = item;
            Volatile.Write(ref _items, next);
        }
    }

    // Chèn tại index; add(0, e) đặt phần tử mới nhất lên đầu (Java: add(index, e)).
    public void Insert(int index, T item)
    {
        lock (_gate)
        {
            T[] old = _items;
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)index, (uint)old.Length);
            T[] next = new T[old.Length + 1];
            Array.Copy(old, 0, next, 0, index);
            next[index] = item;
            Array.Copy(old, index, next, index + 1, old.Length - index);
            Volatile.Write(ref _items, next);
        }
    }

    // Thay phần tử tại index (Java: set(index, e)).
    public void Set(int index, T item)
    {
        lock (_gate)
        {
            T[] next = (T[])_items.Clone();
            next[index] = item;
            Volatile.Write(ref _items, next);
        }
    }

    // Xóa phần tử bằng item đầu tiên; trả false nếu không có (Java: remove(Object)).
    public bool Remove(T item)
    {
        lock (_gate)
        {
            int index = Array.IndexOf(_items, item);
            if (index < 0)
            {
                return false;
            }

            RemoveAtLocked(index);
            return true;
        }
    }

    // Xóa phần tử tại index (Java: remove(int)).
    public void RemoveAt(int index)
    {
        lock (_gate)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)_items.Length);
            RemoveAtLocked(index);
        }
    }

    // Xóa toàn bộ phần tử (Java: clear).
    public void Clear()
    {
        lock (_gate)
        {
            Volatile.Write(ref _items, []);
        }
    }

    // Kiểm tra có phần tử bằng item (Java: contains).
    public bool Contains(T item) => Array.IndexOf(Volatile.Read(ref _items), item) >= 0;

    // Trả vị trí phần tử bằng item đầu tiên, hoặc -1.
    public int IndexOf(T item) => Array.IndexOf(Volatile.Read(ref _items), item);

    // Sao chép độc lập snapshot hiện tại.
    public T[] ToArray() => (T[])Volatile.Read(ref _items).Clone();

    // Duyệt snapshot tại thời điểm gọi; thay đổi sau đó không hiện ra và không làm hỏng bộ duyệt.
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Volatile.Read(ref _items)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void RemoveAtLocked(int index)
    {
        T[] old = _items;
        T[] next = new T[old.Length - 1];
        Array.Copy(old, 0, next, 0, index);
        Array.Copy(old, index + 1, next, index, old.Length - index - 1);
        Volatile.Write(ref _items, next);
    }
}
