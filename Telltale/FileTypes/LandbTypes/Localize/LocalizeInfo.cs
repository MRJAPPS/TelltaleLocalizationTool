using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;

public class LocalizeInfo : IParsable
{
    public uint Flags { get; set; }

    public uint Size => 8;/*4 for flags and 4 for size*/

    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadUInt32();//skip size
        Flags = reader.ReadUInt32();
    }
}
