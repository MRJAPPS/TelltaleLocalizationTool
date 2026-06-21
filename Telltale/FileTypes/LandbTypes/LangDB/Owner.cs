using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LangDB;

public class Owner : IParsable
{
    public uint miUniqueID { get; set; }
    public uint Size => sizeof(uint) + ISize.FOUR_ADD;


    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadUInt32(); //Skip cap
        miUniqueID = reader.ReadUInt32();
    }

}
