using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LangDB;

[Description("Processing Owner section")]
public class Owner : ReportBase, IParsable
{
    #region Fields

    #endregion

    #region Properties

    public uint miUniqueID { get; set; }

    public uint Size => sizeof(uint) + ISize.FOUR_ADD;

    public long Position { get; private set; }

    #endregion

    #region Implementation

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // This uint is just a size marker – we already know it's 4, so we read and forget it.
        _ = reader.ReadUInt32();

        miUniqueID = reader.ReadUInt32();
        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);
        writer.Write(miUniqueID);

        Report(this, true);
    }

    #endregion
}