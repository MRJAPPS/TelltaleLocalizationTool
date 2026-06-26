using System.Collections;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

public abstract class TCollectionBase<T> : ReportBase, ICollection<T>
{
    #region Properties

    protected List<T> Items { get; set; } = [];

    public int Count => Items.Count;

    public bool IsReadOnly => false; // ICollection<T> asks, and we're like "nah, we're not read-only"

    public int Capacity => Items.Capacity;

    #endregion

    #region ICollection<T> Implementation

    public void Add(T item) => Items.Add(item);

    public void Clear() => Items.Clear();

    public bool Contains(T item) => Items.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

    public bool Remove(T item) => Items.Remove(item);

    #endregion

    #region IEnumerable Members

    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    #endregion

    #region INDEX
    
    public T this[int index]
    {
        get => Items[index];
        set => Items[index] = value;
    }

    #endregion
}