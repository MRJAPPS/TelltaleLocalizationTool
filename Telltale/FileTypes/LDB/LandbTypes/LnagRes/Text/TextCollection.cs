using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LnagRes.Text;

[Description("Processing the text collection")]
public class TextCollection : TCollectionBase<TextItem>, ITelltaleObject
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

    public long Position { get; private set; } = -1;

    #endregion

    #region Implementation

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // The first integer is the number of items that follow.
        int count = reader.ReadInt32();
        Items = new List<TextItem>(count);   // Pre‑size to avoid resizing while we add

        for (int i = 0; i < Capacity; ++i)
        {
            TextItem textItem = new();
            textItem.Parse(reader);
            Items.Add(textItem);
        }
        // Report after each item so the log shows incremental progress.
        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Items.Count);

        for (int i = 0; i < Items.Count; ++i)
        {
            Items[i].Write(writer);
        }
        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(TextCollection));
        for (int i = 0; i < Items.Count; ++i)
        {
            Items[i].Write(myElement, $"{nameof(Items)}_{i}");
        }
        _ = element.AppendChild(myElement);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(TextCollection));

        for (int i = 0; i < myElement.ChildNodes.Count; ++i)
        {
            TextItem textItem = new();
            textItem.Parse(myElement, $"{nameof(Items)}_{i}");
            Items.Add(textItem);
        }
    }

    #endregion
}