
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class Owner : ICpacity, IParsable
{
    public uint miUniqueID { get; set; }
    public uint Cpacity => sizeof(uint) + ICpacity.FOUR_ADD;


    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadUInt32(); //Skip cap
        miUniqueID = reader.ReadUInt32();
    }

}
