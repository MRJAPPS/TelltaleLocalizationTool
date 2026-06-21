using System.Collections;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

public abstract class TCollectionBase<T> : ICollection<T>
{
    protected List<T> Items { get; set; } = [];
    public int Count => Items.Count;

    public bool IsReadOnly => false;

    public void Add(T item) => Items.Add(item);

    public int Capacity => Items.Capacity;

    public void Clear() => Items.Clear();

    public bool Contains(T item) => Items.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

    public bool Remove(T item) => Items.Remove(item);

    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
}
