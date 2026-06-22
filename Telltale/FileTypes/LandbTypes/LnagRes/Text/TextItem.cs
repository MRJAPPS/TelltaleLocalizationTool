using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Enum;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LnagRes.Text;

[Description("Processing the text section")]
public class TextItem : ReportBase, IParsable
{
    private long _ParsePos;

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

    public uint Size => (sizeof(uint) * 3) + sizeof(float) + sizeof(Enums.ETextItemFlags)
        + mResName.Size + mhAnimation.Size + mhVoiceData.Size + mLocalData.Size + mResolvedLocalData.Size + mRecordingStatus.Size;

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
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
}
