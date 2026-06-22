using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;

[Description("Processing the localization collection (texts and prefixes are localized, I think)")]
public class LocalDataCollection : TCollectionBase<LocalDataItem>, IParsable
{
    private long _ParsePos;
    private const uint DcArrayLen = sizeof(uint);
    private const uint DcArraySizeInFile = sizeof(uint);
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

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        _ = reader.ReadInt32();//skip size
        Items = new(reader.ReadInt32());
        for (int i = 0; i < Capacity; ++i)
        {
            LocalDataItem localDataItem = new();
            localDataItem.Parse(reader);
            Items.Add(localDataItem);
        }
        Report(this);
    }
}
