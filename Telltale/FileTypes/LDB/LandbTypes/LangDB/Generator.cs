using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LangDB;

[Description("Processing Generator section")]
public class Generator : ReportBase, ITelltaleFile
{
    #region Fields

    #endregion

    #region Properties

    public uint miNextUniqueID { get; set; }

    public uint Size => sizeof(uint) + ISize.FOUR_ADD;

    public long Position { get; private set; }

    #endregion

    #region Implementation

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // Skip the size cap – it's just telling us what we already know.
        _ = reader.ReadUInt32();

        miNextUniqueID = reader.ReadUInt32();

        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);
        writer.Write(miNextUniqueID);

        Report(this, true);
    }

    #endregion
}