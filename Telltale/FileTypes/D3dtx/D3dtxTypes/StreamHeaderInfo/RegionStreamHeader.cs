using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;

[Description("Parsing a RegionStreamHeader...")]
public class RegionStreamHeader(int version) : ReportBase, ITelltaleObject
{
    #region Public props
    // mVersion >= 5
    public int mFaceIndex { get; set; }
    // mVersion >= 3
    public int mMipIndex { get; set; }
    // mVersion >= 4
    public int mMipCount { get; set; }
    // mVersion >= 3
    public uint mDataSize { get; set; }
    // mVersion >= 3
    public int mPitch { get; set; }
    // mVersion >= 9
    public int mSlicePitch { get; set; }
    #endregion

    #region Implement

    public uint Size => sizeof(int) * 5 + sizeof(uint);//I want to write it like this, don't nitpick.

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        if (version >= 5)
        {
            mFaceIndex = reader.ReadInt32();
        }
        if (version >= 3)
        {
            mMipIndex = reader.ReadInt32();
        }
        if (version >= 4)
        {
            mMipCount = reader.ReadInt32();
        }
        if (version >= 3)
        {
            mDataSize = reader.ReadUInt32();
        }
        if (version >= 3)
        {
            mPitch = reader.ReadInt32();
        }
        if (version >= 9)
        {
            mSlicePitch = reader.ReadInt32();
        }

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(RegionStreamHeader), name);
        ParseBasicTypes(myElement);
    }

    public void Write(BinaryWriter writer)
    {

        if (version >= 5)
        {
            writer.Write(mFaceIndex);
        }
        if (version >= 3)
        {
            writer.Write(mMipIndex);
        }
        if (version >= 4)
        {
            writer.Write(mMipCount);
        }
        if (version >= 3)
        {
            writer.Write(mDataSize);
        }
        if (version >= 3)
        {
            writer.Write(mPitch);
        }
        if (version >= 9)
        {
            writer.Write(mSlicePitch);
        }
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(RegionStreamHeader));
        myElement.SetAttribute(nameof(name), name);
        AppendBasicTypes(myElement);
        _ = element.AppendChild(myElement);
    }

    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mFaceIndex = element.OwnerDocument.CreateAttribute(nameof(this.mFaceIndex));
        XmlAttribute mMipIndex = element.OwnerDocument.CreateAttribute(nameof(this.mMipIndex));
        XmlAttribute mMipCount = element.OwnerDocument.CreateAttribute(nameof(this.mMipCount));
        XmlAttribute mDataSize = element.OwnerDocument.CreateAttribute(nameof(this.mDataSize));
        XmlAttribute mPitch = element.OwnerDocument.CreateAttribute(nameof(this.mPitch));
        XmlAttribute mSlicePitch = element.OwnerDocument.CreateAttribute(nameof(this.mSlicePitch));
        XmlAttribute versionAttr = element.OwnerDocument.CreateAttribute(nameof(version));

        mFaceIndex.Value = this.mFaceIndex.ToString();
        mMipIndex.Value = this.mMipIndex.ToString();
        mMipCount.Value = this.mMipCount.ToString();
        mDataSize.Value = this.mDataSize.ToString();
        mPitch.Value = this.mPitch.ToString();
        mSlicePitch.Value = this.mSlicePitch.ToString();
        versionAttr.Value = version.ToString();

        element.Attributes.Append(mFaceIndex);
        element.Attributes.Append(mMipIndex);
        element.Attributes.Append(mMipCount);
        element.Attributes.Append(mDataSize);
        element.Attributes.Append(mPitch);
        element.Attributes.Append(mSlicePitch);
        element.Attributes.Append(versionAttr);
    }

    private void ParseBasicTypes(XmlElement element)
    {
        mFaceIndex = int.Parse(element.GetAttribute(nameof(mFaceIndex)));
        mMipIndex = int.Parse(element.GetAttribute(nameof(mMipIndex)));
        mMipCount = int.Parse(element.GetAttribute(nameof(mMipCount)));
        mDataSize = uint.Parse(element.GetAttribute(nameof(mDataSize)));
        mPitch = int.Parse(element.GetAttribute(nameof(mPitch)));
        mSlicePitch = int.Parse(element.GetAttribute(nameof(mSlicePitch)));
        version = int.Parse(element.GetAttribute(nameof(version)));
    }

    #endregion
}