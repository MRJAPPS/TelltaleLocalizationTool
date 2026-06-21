using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class THandle : IParsable, ICpacity
{
    const uint SymbolSize = sizeof(ulong);
    public uint Cpacity => SymbolSize + ICpacity.FOUR_ADD;
    public SymbolValue hSymbolValue { get; set; } = new();

    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadUInt32();//skip cap
        hSymbolValue.Parse(reader);
    }
}
