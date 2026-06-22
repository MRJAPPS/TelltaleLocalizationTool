using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Reg;

[Description("Processing a Registry object that includes a normal FlagIndexMap(and a reverse FlagIndexMap)")]
public class Registry : ReportBase, IParsable
{
    #region Constants

    // The size of the redundant length field that lives at the top of the struct.
    private const uint RegistrySizeInFile = sizeof(uint);

    #endregion

    #region Properties

    public FlagIndexMap mFlagIndexMap { get; set; } = new();

    public FlagIndexMap mFlagIndexMapReverse { get; set; } = new(true);

    public uint Size => mFlagIndexMap.Size + mFlagIndexMapReverse.Size + RegistrySizeInFile;

    public long Position { get; private set; }

    #endregion

    #region Implement

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // The first 4 bytes are just a size value we can calculate ourselves — skip 'em.
        _ = reader.ReadUInt32();

        mFlagIndexMap.Parse(reader);
        mFlagIndexMapReverse.Parse(reader);
        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);

        mFlagIndexMap.Write(writer);
        mFlagIndexMapReverse.Write(writer);
        Report(this);
    }

    #endregion
}