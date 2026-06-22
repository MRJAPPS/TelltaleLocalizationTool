using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LangDB;

public class Owner : IParsable
{

    private long _ParsePos;
    public uint miUniqueID { get; set; }
    public uint Size => sizeof(uint) + ISize.FOUR_ADD;

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        _ = reader.ReadUInt32(); //Skip cap
        miUniqueID = reader.ReadUInt32();
    }

}
