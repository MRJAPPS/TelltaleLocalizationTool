using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LangDB;

/// <summary>
/// Holds a unique ID for the “Owner” part of the language database.  
/// Just a uint with a header we don't care about.
/// </summary>
[Description("Processing Owner section")]
public class Owner : ReportBase, IParsable
{
    #region Fields

    #endregion

    #region Properties

    /// <summary>
    /// The unique owner ID. No idea what it owns, but it's unique.
    /// </summary>
    public uint miUniqueID { get; set; }

    /// <summary>
    /// Total byte size: 4 bytes for the header cap we skip, plus the 4‑byte ID.
    /// </summary>
    public uint Size => sizeof(uint) + ISize.FOUR_ADD;

    /// <summary>
    /// Where the reader was when parsing kicked off.
    /// </summary>
    public long ParsePosition { get; private set; }

    #endregion

    #region IParsable Implementation

    /// <summary>
    /// Reads the Owner block: skips the useless size field (it’s always 4, duh),
    /// then grabs the real unique ID.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        ParsePosition = reader.BaseStream.Position;

        // This uint is just a size marker – we already know it's 4, so we read and forget it.
        _ = reader.ReadUInt32();

        miUniqueID = reader.ReadUInt32();
        Report(this);
    }

    #endregion
}