using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

public class THandle : IParsable
{
    private long _ParsePos;

    public TSymbolValue hSymbolValue { get; set; } = new();

    public uint Size => hSymbolValue.Size + ISize.FOUR_ADD;

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        _ = reader.ReadUInt32();//skip size
        hSymbolValue.Parse(reader);
    }
}
