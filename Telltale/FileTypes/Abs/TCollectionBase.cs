using System.Collections;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

/// <summary>
/// A base class for custom collections that don't want to rewrite the same List wrapper every time.
/// Basically, "List but with extra reporting superpowers."
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public abstract class TCollectionBase<T> : ReportBase, ICollection<T>
{
    #region Properties

    /// <summary>
    /// The actual list holding all the goodies. Protected so subclasses can mess with it directly.
    /// </summary>
    protected List<T> Items { get; set; } = [];

    /// <summary>
    /// Gets the number of elements in the collection. Quick head count.
    /// </summary>
    public int Count => Items.Count;

    /// <summary>
    /// Always <c>false</c> – this collection is as mutable as a teenager's mood.
    /// </summary>
    public bool IsReadOnly => false; // ICollection<T> asks, and we're like "nah, we're not read-only"

    /// <summary>
    /// Gets or sets the internal list capacity. Useful when you know the size beforehand and wanna avoid
    /// that awkward internal array reallocation dance.
    /// </summary>
    public int Capacity => Items.Capacity;

    #endregion

    #region ICollection<T> Implementation

    /// <summary>
    /// Chucks an item into the collection. Simple add, nothing fancy.
    /// </summary>
    /// <param name="item">The thing to store.</param>
    public void Add(T item) => Items.Add(item);

    /// <summary>
    /// Wipes everything out. Like a factory reset for the list.
    /// </summary>
    public void Clear() => Items.Clear();

    /// <summary>
    /// Checks if the collection contains a specific item. Does exactly what it says on the tin.
    /// </summary>
    /// <param name="item">The item to locate.</param>
    /// <returns><c>true</c> if found, <c>false</c> otherwise.</returns>
    public bool Contains(T item) => Items.Contains(item);

    /// <summary>
    /// Copies the elements to a target array, starting at the given index.
    /// Straightforward delegation to the underlying list.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">Where to start copying in the target array.</param>
    public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

    /// <summary>
    /// Removes the first occurrence of a specific item. Returns <c>true</c> if anything actually got yeeted.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><c>true</c> if the item was successfully removed; otherwise, <c>false</c>.</returns>
    public bool Remove(T item) => Items.Remove(item);

    #endregion

    #region IEnumerable Members

    /// <summary>
    /// Returns a generic enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the collection.</returns>
    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

    /// <summary>
    /// The old-school non-generic enumerator. We have to provide it because <see cref="IEnumerable"/> says so.
    /// </summary>
    /// <returns>An enumerator that can be used for good old <c>foreach</c> without generics.</returns>
    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    #endregion
}