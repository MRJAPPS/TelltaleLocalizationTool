using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class Generator : IParsable, ICpacity
{

    public uint miNextUniqueID { get; set; }

    public uint Cpacity => sizeof(uint) + ICpacity.FOUR_ADD;

    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadUInt32(); //skip cap
        miNextUniqueID = reader.ReadUInt32();
    }
}
