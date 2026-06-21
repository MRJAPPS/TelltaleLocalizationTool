using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class SymbolValue : IParsable
{
    public ulong Value { get; set; }
    public void Parse(BinaryReader reader) => Value = reader.ReadUInt64();
}
