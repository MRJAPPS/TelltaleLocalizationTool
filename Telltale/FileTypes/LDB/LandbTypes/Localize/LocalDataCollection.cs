using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Localize;

[Description("Processing the localization collection (texts and prefixes are localized, I think)")]
public class LocalDataCollection : TCollectionBase<LocalDataItem>, IParsable
{
    #region Constants

    // The header always starts with two uint fields: the redundant block size, and the item count.
    private const uint DcArrayLen = sizeof(uint);         // space for the item count
    private const uint DcArraySizeInFile = sizeof(uint);  // space for the block size (skipped)

    #endregion

    #region Private Fields

    #endregion

    #region Public Properties

    public uint Size
    {
        get
        {
            uint size = DcArrayLen + DcArraySizeInFile;
            foreach (LocalDataItem localData in Items)
            {
                size += localData.Size;
            }
            return size;
        }
    }

    public long Position { get; private set; }

    #endregion

    #region Implementation

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // The first uint is a block‑size value that we can compute from the item count and structure,
        // so there’s no point keeping it – we read and discard.
        _ = reader.ReadInt32();

        int itemCount = reader.ReadInt32();
        Items = new List<LocalDataItem>(itemCount);   // pre‑allocate to avoid resizes

        for (int i = 0; i < Capacity; ++i)
        {
            LocalDataItem localDataItem = new();
            localDataItem.Parse(reader);
            Items.Add(localDataItem);
        }

        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;
        writer.Write(Size);

        writer.Write(Items.Count);
        for (int i = 0; i < Capacity; ++i)
            Items[i].Write(writer);
        Report(this, true);
    }

    #endregion
}