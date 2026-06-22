using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.ProjDataBIDPairs;

[Description("Processing a collection of ProjectDatabase that contains mProjectID and mDBID.")]
public class ProjectDatabaseIDPair : ReportBase, IParsable
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
        Report(this);
    }
}
