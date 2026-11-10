using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;

[Description("I've hit some texture info — reading TextureInfo now...")]
public class TextureInfo : ReportBase, ITelltaleObject
{
    public uint mNumMipLevels { get; set; }
    public uint mWidth { get; set; }
    public uint mHeight { get; set; }
    public uint mDepth { get; set; }
    public uint mArraySize { get; set; }

    #region Implement

    public uint Size => sizeof(uint) * 5;

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mNumMipLevels = reader.ReadUInt32();
        mWidth = reader.ReadUInt32();
        mHeight = reader.ReadUInt32();
        mDepth = reader.ReadUInt32();
        mArraySize = reader.ReadUInt32();

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(TextureInfo), name);
        ParseBasicTypes(myElement);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(mNumMipLevels);
        writer.Write(mWidth);
        writer.Write(mHeight);
        writer.Write(mDepth);
        writer.Write(mArraySize);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(TextureInfo));
        myElement.SetAttribute(nameof(name), name);
        AppendBasicTypes(myElement);
        _ = element.AppendChild(myElement);
    }

    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mNumMipLevels = element.OwnerDocument.CreateAttribute(nameof(this.mNumMipLevels));
        XmlAttribute mWidth = element.OwnerDocument.CreateAttribute(nameof(this.mWidth));
        XmlAttribute mHeight = element.OwnerDocument.CreateAttribute(nameof(this.mHeight));
        XmlAttribute mDepth = element.OwnerDocument.CreateAttribute(nameof(this.mDepth));
        XmlAttribute mArraySize = element.OwnerDocument.CreateAttribute(nameof(this.mArraySize));

        mNumMipLevels.Value = this.mNumMipLevels.ToString();
        mWidth.Value = this.mWidth.ToString();
        mHeight.Value = this.mHeight.ToString();
        mDepth.Value = this.mDepth.ToString();
        mArraySize.Value = this.mArraySize.ToString();

        _ = element.Attributes.Append(mNumMipLevels);
        _ = element.Attributes.Append(mWidth);
        _ = element.Attributes.Append(mHeight);
        _ = element.Attributes.Append(mDepth);
        _ = element.Attributes.Append(mArraySize);
    }

    private void ParseBasicTypes(XmlElement element)
    {
        mNumMipLevels = uint.Parse(element.GetAttribute(nameof(mNumMipLevels)));
        mWidth = uint.Parse(element.GetAttribute(nameof(mWidth)));
        mHeight = uint.Parse(element.GetAttribute(nameof(mHeight)));
        mDepth = uint.Parse(element.GetAttribute(nameof(mDepth)));
        mArraySize = uint.Parse(element.GetAttribute(nameof(mArraySize)));
    }

    #endregion
}