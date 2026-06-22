using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LnagRes.Text;

/// <summary>
/// A collection of <see cref="TextItem"/> objects – all the dialogue entries for a language.
/// The file block consists of a count field and then the items themselves.
/// </summary>
[Description("Processing the text collection")]
public class TextCollection : TCollectionBase<TextItem>, IParsable
{
    #region Constants

    // These represent the space taken by the size and count fields in the header,
    // even though the current Parse only reads the count.
    private const uint TextCollectionSizeInFile = sizeof(uint); // Reserved size field (unused in reading)
    private const uint TextCollectionLenSize = sizeof(uint);     // The item count field

    #endregion

    #region Private Fields

    #endregion

    #region Public Properties

    /// <summary>
    /// Total byte size of this collection block, including the header (size+count) and all items.
    /// </summary>
    public uint Size
    {
        get
        {
            uint size = TextCollectionSizeInFile + TextCollectionLenSize;
            foreach (TextItem textItem in Items)
            {
                size += textItem.Size;
            }
            return size;
        }
    }

    /// <summary>
    /// The stream offset where parsing began.
    /// </summary>
    public long ParsePosition { get; private set; }

    #endregion

    #region IParsable Implementation

    /// <summary>
    /// Reads the collection from the binary stream: grabs the item count first,
    /// then creates and parses each <see cref="TextItem"/>.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        ParsePosition = reader.BaseStream.Position;

        // The first integer is the number of items that follow.
        int count = reader.ReadInt32();
        Items = new List<TextItem>(count);   // Pre‑size to avoid resizing while we add

        for (int i = 0; i < Capacity; ++i)
        {
            TextItem textItem = new();
            textItem.Parse(reader);
            Items.Add(textItem);

            // Report after each item so the log shows incremental progress.
            Report(this);
        }
    }

    #endregion
}