using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class LocalizeInfo : IParsable, ICpacity
{
    public uint Flags { get; set; }

    public uint Cpacity => 8;/*4 for flags and 4 for cpacity*/

    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadUInt32();//skip cpa
        Flags = reader.ReadUInt32();
    }
}
