using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Enum;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Localize;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LnagRes.Text;

[Description("Processing the text section")]
public class TextItem : ReportBase, IParsable
{
    #region Private Fields

    #endregion

    #region Public Properties

    public uint TextId { get; set; }

    public TSymbolValue mResName { get; set; } = new();

    public uint mId { get; set; }

    public uint mIDAlias { get; set; }

    public THandle mhAnimation { get; set; } = new();

    public THandle mhVoiceData { get; set; } = new();

    public LocalDataCollection mLocalData { get; set; } = [];

    public float mLengthOverride { get; set; }

    public ResolvedLocalData mResolvedLocalData { get; set; } = new();

    public FixedEnum<Enums.ERecordingStatus> mRecordingStatus { get; set; } = new(Enums.ERecordingStatus.eRecordingStatus_None);

    public Enums.ETextItemFlags mFlags { get; set; } = Enums.ETextItemFlags.Unknown;

    public uint Size =>
        sizeof(uint) * 3 + sizeof(float) + sizeof(Enums.ETextItemFlags)
        + mResName.Size + mhAnimation.Size + mhVoiceData.Size + mLocalData.Size + mResolvedLocalData.Size + mRecordingStatus.Size;

    public long Position { get; private set; }

    #endregion

    #region Implementation

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

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

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(TextId);
        mResName.Write(writer);
        writer.Write(mId);
        writer.Write(mIDAlias);
        mhAnimation.Write(writer);
        mhVoiceData.Write(writer);
        mLocalData.Write(writer);
        writer.Write(mLengthOverride);
        mResolvedLocalData.Write(writer);
        mRecordingStatus.Write(writer);
        writer.Write((uint)mFlags);

        Report(this, true);
    }

    #endregion
}