using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.ProjDataBIDPairs;

[Description("Processing a collection of ProjectDatabase that contains mProjectID and mDBID.")]
public class ProjectDatabaseIDPair : ReportBase, ITelltaleFile
{
    #region Fields


    #endregion

    #region Properties

    private uint mProjectID { get; set; }

    private uint mDBID { get; set; }

    public uint Size => sizeof(uint) * 2;

    public long Position { get; private set; }

    #endregion

    #region Implement

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mProjectID = reader.ReadUInt32();
        mDBID = reader.ReadUInt32();

        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(mProjectID);
        writer.Write(mDBID);

        Report(this,true);
    }

    #endregion
}