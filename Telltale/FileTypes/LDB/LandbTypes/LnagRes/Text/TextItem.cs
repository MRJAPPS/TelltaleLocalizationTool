using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Enum;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Localize;
using TelltaleLocalizationTool.Telltale.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LnagRes.Text;

[Description("Processing the text section")]
public class TextItem : ReportBase, ILandb
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
        (sizeof(uint) * 3) + sizeof(float) + sizeof(Enums.ETextItemFlags)
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

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(TextItem), name);

        ParseBasicTypes(myElement);

        mResName.Parse(myElement, nameof(mResName));
        mhAnimation.Parse(myElement, nameof(mhAnimation));
        mhVoiceData.Parse(myElement, nameof(mhVoiceData));
        mLocalData.Parse(myElement, nameof(mLocalData));
        mResolvedLocalData.Parse(myElement, nameof(mResolvedLocalData));
        mRecordingStatus.Parse(myElement, nameof(mRecordingStatus));
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

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(TextItem));
        myElement.SetAttribute(nameof(name), name);

        AppendBasicTypes(myElement);
        mResName.Write(myElement, nameof(mResName));
        mhAnimation.Write(myElement, nameof(mhAnimation));
        mhVoiceData.Write(myElement, nameof(mhVoiceData));
        mLocalData.Write(myElement, nameof(mLocalData));
        mResolvedLocalData.Write(myElement, nameof(mResolvedLocalData));
        mRecordingStatus.Write(myElement, nameof(mRecordingStatus));

        _ = element.AppendChild(myElement);
    }

    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute TextId, mId, mIDAlias, mLengthOverride, mFlags;

        TextId = element.OwnerDocument.CreateAttribute(nameof(this.TextId));
        mId = element.OwnerDocument.CreateAttribute(nameof(this.mId));
        mIDAlias = element.OwnerDocument.CreateAttribute(nameof(this.mIDAlias));
        mLengthOverride = element.OwnerDocument.CreateAttribute(nameof(this.mLengthOverride));
        mFlags = element.OwnerDocument.CreateAttribute(nameof(this.mFlags));

        TextId.Value = this.TextId.ToString();
        mId.Value = this.mId.ToString();
        mIDAlias.Value = this.mIDAlias.ToString();
        mLengthOverride.Value = this.mLengthOverride.ToString();
        mFlags.Value = this.mFlags.ToString();

        _ = element.Attributes.Append(TextId);
        _ = element.Attributes.Append(mId);
        _ = element.Attributes.Append(mIDAlias);
        _ = element.Attributes.Append(mLengthOverride);
        _ = element.Attributes.Append(mFlags);
    }

    private void ParseBasicTypes(XmlElement element)
    {
        TextId = XmlConvert.ToUInt32(element.GetAttribute(nameof(TextId)));

        mId = XmlConvert.ToUInt32(element.GetAttribute(nameof(mId)));

        mIDAlias = XmlConvert.ToUInt32(element.GetAttribute(nameof(mIDAlias)));

        mLengthOverride = XmlConvert.ToSingle(element.GetAttribute(nameof(mLengthOverride)));

        mFlags = System.Enum.Parse<Enums.ETextItemFlags>(element.GetAttribute(nameof(mFlags)));
    }

    #endregion
}