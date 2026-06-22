using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.ProjDataBIDPairs;

public class ProjectDatabaseIDPairs : TCollectionBase<ProjectDatabaseIDPair>, IParsable
{
    const uint ProjectDatabaseIDPairsSizeInFile = sizeof(uint);
    const uint ProjectDatabaseIDPairsLenInFile = sizeof(uint);
    private readonly uint ItemSize = new ProjectDatabaseIDPair().Size;
    public uint Size => (uint)Items.Count * ItemSize + ProjectDatabaseIDPairsSizeInFile + ProjectDatabaseIDPairsLenInFile;

    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadUInt32();//Skip Size
        int capacity = reader.ReadInt32();
        Items = new List<ProjectDatabaseIDPair>(capacity);
        for (int i = 0; i < capacity; ++i)
        {
            ProjectDatabaseIDPair pair = new();
            pair.Parse(reader);
            Items.Add(pair);
        }
    }
}
