using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;

/// <summary>
/// A single localised data entry – prefix, text, extra info, and a symbol handle that
/// isn’t actually read by Parse (probably set from somewhere else).
/// </summary>
[Description("Processing the localization data section")]
public class LocalDataItem : ReportBase, IParsable
{
    #region Private Fields

    /// <summary>
    /// Stream position when Parse was called – used for the report.
    /// </summary>

    #endregion

    #region Public Fields

    /// <summary>
    /// Extra localisation metadata (flags and stuff). A field because it’s assigned directly in other code.
    /// </summary>
    public LocalizeInfo mLocalInfo = new();

    #endregion

    #region Public Properties

    /// <summary>
    /// The prefix string – often an ID or tag that comes before the real text.
    /// </summary>
    public TString mPrefix { get; set; } = [];

    /// <summary>
    /// The actual localised text.
    /// </summary>
    public TString mText { get; set; } = [];

    /// <summary>
    /// A handle to a symbol value (not populated by this Parse, just stored for later reference).
    /// </summary>
    public TSymbolValue hSymbolValue { get; set; } = new();

    /// <summary>
    /// Total byte size of the prefix + text + local info chunk.
    /// (The handle is deliberately not included – it lives outside this block.)
    /// </summary>
    public uint Size => mPrefix.Size + mText.Size + mLocalInfo.Size;

    /// <summary>
    /// Where the reader was when parsing began.
    /// </summary>
    public long Position { get; private set; }

    #endregion

    #region Implementation

    /// <summary>
    /// Reads the prefix, text, and local info from the stream in that exact order.
    /// The symbol handle is handled separately, so it’s not touched here.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        mPrefix.Parse(reader);
        mText.Parse(reader);
        mLocalInfo.Parse(reader);
        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        mPrefix.Write(writer);
        mText.Write(writer);
        mLocalInfo.Write(writer);

        Report(this,true);
    }

    #endregion
}