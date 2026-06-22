using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;

/// <summary>
/// Contains the resolved localisation data chunk – a prefix string, the actual text,
/// and extra localisation info. This is where the real words end up.
/// </summary>
[Description("Processing the resolved localization data section")]
public class ResolvedLocalData : ReportBase, IParsable
{
    #region Constants

    // Size of the header field that stores the total length of this block.
    // We don't really need it to parse, but it's always present.
    private const uint ResolvedLocalDataSizeInFile = sizeof(uint);

    #endregion

    #region Fields

    #endregion

    #region Properties

    /// <summary>
    /// The prefix string (like a tag or ID prefix). Read from the stream.
    /// </summary>
    public TString mPrefix { get; set; } = [];

    /// <summary>
    /// The actual localised text. This is what you're probably looking for.
    /// </summary>
    public TString mText { get; set; } = [];

    /// <summary>
    /// Extra localisation metadata (stuff like IDs and flags).
    /// </summary>
    public LocalizeInfo mLocalInfo { get; set; } = new();

    /// <summary>
    /// Total byte size of this resolved data block.
    /// </summary>
    public uint Size =>
        mPrefix.Size + mText.Size + mLocalInfo.Size + ResolvedLocalDataSizeInFile;

    /// <summary>
    /// Where we were in the stream when parsing kicked off.
    /// </summary>
    public long ParsePosition { get; private set; }

    #endregion

    #region IParsable Implementation

    /// <summary>
    /// Reads the resolved data from the binary stream: skips the redundant capacity field,
    /// then parses prefix, text, and localisation info in that exact order.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        ParsePosition = reader.BaseStream.Position;

        // The first int is just a capacity/size value we can compute ourselves,
        // so we read it and throw it away – no point storing it.
        _ = reader.ReadInt32();

        mPrefix.Parse(reader);
        mText.Parse(reader);
        mLocalInfo.Parse(reader);

        Report(this);
    }

    #endregion
}