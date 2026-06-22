using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.ProjDataBIDPairs;

[Description("Processing a collection of ProjectDatabaseIDPair objects that have mProjectID and mDBID. No idea what this is, honestly.")]
public class ProjectDatabaseIDPairs : TCollectionBase<ProjectDatabaseIDPair>, IParsable
{
    private long _ParsePos;
    private const uint ProjectDatabaseIDPairsSizeInFile = sizeof(uint);
    private const uint ProjectDatabaseIDPairsLenInFile = sizeof(uint);
    private readonly uint ItemSize = new ProjectDatabaseIDPair().Size;
    public uint Size => ((uint)Items.Count * ItemSize) + ProjectDatabaseIDPairsSizeInFile + ProjectDatabaseIDPairsLenInFile;

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        _ = reader.ReadUInt32();//Skip Size
        int capacity = reader.ReadInt32();
        Items = new List<ProjectDatabaseIDPair>(capacity);
        for (int i = 0; i < capacity; ++i)
        {
            ProjectDatabaseIDPair pair = new();
            pair.Parse(reader);
            Items.Add(pair);
        }
        Report(this);
    }
}
