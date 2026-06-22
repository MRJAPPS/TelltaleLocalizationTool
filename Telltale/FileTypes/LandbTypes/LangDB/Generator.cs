using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LangDB;

/// <summary>
/// Keeps track of the next unique ID to be generated – basically a counter.  
/// Same layout as Owner: a header we skip, then the actual value.
/// </summary>
[Description("Processing Generator section")]
public class Generator : ReportBase, IParsable
{
    #region Fields

    /// <summary>
    /// Stream position when parsing started. Exposed because some other code might want it, I guess.
    /// </summary>
    public long _ParsePos;

    #endregion

    #region Properties

    /// <summary>
    /// The next unique ID that will be handed out. Incremented somewhere else.
    /// </summary>
    public uint miNextUniqueID { get; set; }

    /// <summary>
    /// Total byte size: 4 bytes for the skipped cap, plus 4 bytes for the ID itself.
    /// </summary>
    public uint Size => sizeof(uint) + ISize.FOUR_ADD;

    /// <summary>
    /// The stream offset where we started reading.
    /// </summary>
    public long ParsePosition => _ParsePos;

    #endregion

    #region IParsable Implementation

    /// <summary>
    /// Reads the Generator block: the first uint is a redundant size (always 4), so we skip it.
    /// Then we grab the next unique ID and call it a day.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;

        // Skip the size cap – it's just telling us what we already know.
        _ = reader.ReadUInt32();

        miNextUniqueID = reader.ReadUInt32();
        Report(this);
    }

    #endregion
}