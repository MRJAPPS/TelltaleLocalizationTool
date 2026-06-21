using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class THandle : IParsable
{
    const uint SymbolSize = sizeof(ulong);
    public uint Size => SymbolSize + ISize.FOUR_ADD;
    public SymbolValue hSymbolValue { get; set; } = new();

    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadUInt32();//skip size
        hSymbolValue.Parse(reader);
    }
}
