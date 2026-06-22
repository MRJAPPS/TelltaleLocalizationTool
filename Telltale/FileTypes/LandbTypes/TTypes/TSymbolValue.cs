using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

[Description("Processing a SymbolValue(8 byte)")]
public class TSymbolValue : ReportBase, IParsable
{
    private long _ParsePos;

    public ulong Value { get; set; }

    public uint Size => sizeof(ulong);

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        Value = reader.ReadUInt64();
        Report(this);
    }
}
