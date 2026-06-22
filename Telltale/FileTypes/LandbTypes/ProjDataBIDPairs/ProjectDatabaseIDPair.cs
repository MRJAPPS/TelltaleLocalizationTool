using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.ProjDataBIDPairs;

/// <summary>
/// A single ID pair: a project ID and a database ID. Always 8 bytes in the stream.
/// </summary>
[Description("Processing a collection of ProjectDatabase that contains mProjectID and mDBID.")]
public class ProjectDatabaseIDPair : ReportBase, IParsable
{
    #region Fields


    #endregion

    #region Properties

    /// <summary>
    /// Project identifier read from the file.
    /// </summary>
    private uint mProjectID { get; set; }

    /// <summary>
    /// Database identifier read from the file.
    /// </summary>
    private uint mDBID { get; set; }

    /// <summary>
    /// Always 8 bytes – two uint values back to back.
    /// </summary>
    public uint Size => sizeof(uint) * 2;

    /// <summary>
    /// The stream position where parsing started.
    /// </summary>
    public long Position { get; private set; }

    #endregion

    #region Implement

    /// <summary>
    /// Reads the two uint fields directly from the stream.
    /// </summary>
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