using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

public class TSymbolValue : IParsable
{
    public ulong Value { get; set; }

    public uint Size => sizeof(ulong);

    public void Parse(BinaryReader reader)
    {
        Value = reader.ReadUInt64();
    }
}
