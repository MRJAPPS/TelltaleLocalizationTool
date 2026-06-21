using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class THandle : IParsable
{
    public SymbolValue hSymbolValue { get; set; } = new();

    public uint Size => hSymbolValue.Size + ISize.FOUR_ADD;

    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadUInt32();//skip size
        hSymbolValue.Parse(reader);
    }
}
