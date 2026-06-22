using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LangDB;

public class Generator : IParsable
{
    public long _ParsePos;

    public uint miNextUniqueID { get; set; }

    public uint Size => sizeof(uint) + ISize.FOUR_ADD;

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        _ = reader.ReadUInt32(); //skip cap
        miNextUniqueID = reader.ReadUInt32();
    }
}
