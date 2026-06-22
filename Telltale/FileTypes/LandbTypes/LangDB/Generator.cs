using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LangDB;

[Description("Processing Generator section")]
public class Generator : ReportBase, IParsable
{
    public long _ParsePos;

    public uint miNextUniqueID { get; set; }

    public uint Size => sizeof(uint) + ISize.FOUR_ADD;

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        _ = reader.ReadUInt32(); //skip cap
        miNextUniqueID = reader.ReadUInt32();
        Report(this);
    }
}
