using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;

public class LocalizeInfo : IParsable
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
    }
}
