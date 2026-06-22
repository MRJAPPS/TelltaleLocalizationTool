using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.ProjDataBIDPairs;

public class ProjectDatabaseIDPair : IParsable
{
    uint mProjectID { get; set; }
    uint mDBID { get; set; }

    public uint Size => sizeof(uint) * 2;

    public void Parse(BinaryReader reader)
    {
        mProjectID = reader.ReadUInt32();
        mDBID = reader.ReadUInt32();
    }
}
