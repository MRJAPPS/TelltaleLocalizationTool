using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LocalData;

public class LocalDataCollection : TCollectionBase<LocalDataItem>, IParsable
{
    const uint DcArrayLen = sizeof(uint);
    const uint DcArraySizeInFile = sizeof(uint);
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

    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadInt32();//skip size
        Items = new(reader.ReadInt32());
        for (int i = 0; i < Capacity; ++i)
        {
            LocalDataItem localDataItem = new();
            localDataItem.Parse(reader);
            Items.Add(localDataItem);
        }
    }
}
