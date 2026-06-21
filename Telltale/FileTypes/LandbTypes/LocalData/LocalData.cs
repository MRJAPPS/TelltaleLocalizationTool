using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LocalData;

public class LocalDataCollection : TCollectionBase<LocalDataItem>, IParsable, ICpacity
{
    const uint DcArrayLenCap = 4;
    public new uint Cpacity => (uint)Items.Count + ICpacity.FOUR_ADD + DcArrayLenCap;

    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadInt32();//skip cpa
        Items = new(reader.ReadInt32());
        for (int i = 0; i < base.Cpacity; ++i)
        {
            LocalDataItem localDataItem = new();
            localDataItem.Parse(reader);
            Items.Add(localDataItem);
        }
    }
}
