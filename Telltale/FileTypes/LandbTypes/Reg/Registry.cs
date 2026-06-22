using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Reg;

public class Registry : IParsable
{
    private const uint RegistrySizeInFile = sizeof(uint);

    public FlagIndexMap mFlagIndexMap { get; set; } = new();

    public FlagIndexMap mFlagIndexMapReverse { get; set; } = new(true);

    public uint Size => mFlagIndexMap.Size + mFlagIndexMapReverse.Size + RegistrySizeInFile;

    public long ParsePosition { get; private set; }

    public void Parse(BinaryReader reader)
    {
        ParsePosition = reader.BaseStream.Position;
        _ = reader.ReadUInt32();//skip size
        mFlagIndexMap.Parse(reader);
        mFlagIndexMapReverse.Parse(reader);
    }
}
