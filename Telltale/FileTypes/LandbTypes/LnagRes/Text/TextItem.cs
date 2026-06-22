using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Enum;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LnagRes.Text;

/// <summary>
/// A single text entry from the game – IDs, handles for animation/voice, the localised text,
/// an optional override length, and a bunch of flags. Basically a big bundle of dialogue metadata.
/// </summary>
[Description("Processing the text section")]
public class TextItem : ReportBase, IParsable
{
    #region Private Fields

    #endregion

    #region Public Properties

    /// <summary>
    /// The numeric text ID. Probably used to match this text across languages.
    /// </summary>
    public uint TextId { get; set; }

    /// <summary>
    /// The resource name symbol – an 8‑byte symbol handle for the resource.
    /// </summary>
    public TSymbolValue mResName { get; set; } = new();

    /// <summary>
    /// An extra ID (maybe the string table index?).
    /// </summary>
    public uint mId { get; set; }

    /// <summary>
    /// Another ID alias (could be a fallback or parent ID).
    /// </summary>
    public uint mIDAlias { get; set; }

    /// <summary>
    /// Handle that links to the animation data (lip‑sync, expressions, etc.).
    /// </summary>
    public THandle mhAnimation { get; set; } = new();

    /// <summary>
    /// Handle that points to the voice‑over data.
    /// </summary>
    public THandle mhVoiceData { get; set; } = new();

    /// <summary>
    /// Collection of localised strings (one per language, usually).
    /// </summary>
    public LocalDataCollection mLocalData { get; set; } = [];

    /// <summary>
    /// Override length in seconds (or whatever unit) – if zero, the game probably calculates it.
    /// </summary>
    public float mLengthOverride { get; set; }

    /// <summary>
    /// The resolved (final) localisation data after the game picks the right language.
    /// </summary>
    public ResolvedLocalData mResolvedLocalData { get; set; } = new();

    /// <summary>
    /// Recording status as an enum – shows whether the voice line was recorded yet.
    /// </summary>
    public FixedEnum<Enums.ERecordingStatus> mRecordingStatus { get; set; } = new(Enums.ERecordingStatus.eRecordingStatus_None);

    /// <summary>
    /// Arbitrary text‑item flags (scene‑related, priority, etc.).
    /// </summary>
    public Enums.ETextItemFlags mFlags { get; set; } = Enums.ETextItemFlags.Unknown;

    /// <summary>
    /// Total byte size of this text item as stored in the file.
    /// </summary>
    public uint Size =>
        (sizeof(uint) * 3) + sizeof(float) + sizeof(Enums.ETextItemFlags)
        + mResName.Size + mhAnimation.Size + mhVoiceData.Size + mLocalData.Size + mResolvedLocalData.Size + mRecordingStatus.Size;

    /// <summary>
    /// The stream offset where parsing started. Used by the report to log the exact file position.
    /// </summary>
    public long ParsePosition { get; private set; }

    #endregion

    #region IParsable Implementation

    /// <summary>
    /// Reads all fields from the binary stream in the precise order they appear:
    /// TextId, resource symbol, IDs, animation/voice handles, local data, length override,
    /// resolved local data, recording status, and finally the flags.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        ParsePosition = reader.BaseStream.Position;

        TextId = reader.ReadUInt32();
        mResName.Parse(reader);
        mId = reader.ReadUInt32();
        mIDAlias = reader.ReadUInt32();
        mhAnimation.Parse(reader);
        mhVoiceData.Parse(reader);
        mLocalData.Parse(reader);
        mLengthOverride = reader.ReadSingle();
        mResolvedLocalData.Parse(reader);
        mRecordingStatus.Parse(reader);
        mFlags = (Enums.ETextItemFlags)reader.ReadUInt32();

        Report(this);
    }

    #endregion
}