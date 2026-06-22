using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.ProjDataBIDPairs;

public class ProjectDatabaseIDPair : IParsable
{
    private long _ParsePos;
    private uint mProjectID { get; set; }
    private uint mDBID { get; set; }

    public uint Size => sizeof(uint) * 2;

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        mProjectID = reader.ReadUInt32();
        mDBID = reader.ReadUInt32();
    }
}
