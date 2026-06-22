using System.ComponentModel;
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
    public long ParsePosition { get; private set; }

    #endregion

    #region IParsable

    /// <summary>
    /// Reads the two uint fields directly from the stream.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        ParsePosition = reader.BaseStream.Position;
        mProjectID = reader.ReadUInt32();
        mDBID = reader.ReadUInt32();
        Report(this);
    }

    #endregion
}