using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LocalData;

public class LocalDataCollection : TCollectionBase<LocalDataItem>, IParsable
{
    const uint DcArrayLenCap = 4;
    public uint Size => throw new NotImplementedException();

    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadInt32();//skip size
        int capacity = reader.ReadInt32();
        Items = new(capacity);
        for (int i = 0; i < capacity; ++i)
        {
            LocalDataItem localDataItem = new();
            localDataItem.Parse(reader);
            Items.Add(localDataItem);
        }
    }
}
