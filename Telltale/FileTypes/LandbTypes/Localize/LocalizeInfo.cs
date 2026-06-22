using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;

[Description("Processing the localize info flag")]
public class LocalizeInfo : ReportBase, IParsable
{
    private long _ParsePos;

    public uint Flags { get; set; }

    public uint Size => 8;/*4 for flags and 4 for size*/

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        _ = reader.ReadUInt32();//skip size
        Flags = reader.ReadUInt32();
        Report(this);
    }
}
