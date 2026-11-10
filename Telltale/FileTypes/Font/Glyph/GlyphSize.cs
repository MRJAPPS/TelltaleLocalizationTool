using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Font.Glyph;

[Description("Reading glyph size (width and height)")]
public class GlyphSize : ReportBase, ITelltaleObject
{
    #region public props

    public float mWidth { get; set; }

    public float mHeight { get; set; }

    #endregion

    #region Implement

    public uint Size => sizeof(float) * 2;

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mWidth = reader.ReadSingle();
        mHeight = reader.ReadSingle();

        Report(this);
    }

    public void Parse(XmlElement reader, string _)
    {
        mWidth = float.Parse(reader.GetAttribute(nameof(mWidth)));
        mHeight = float.Parse(reader.GetAttribute(nameof(mHeight)));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(mWidth);
        writer.Write(mHeight);

        Report(this, true);
    }

    public void Write(XmlElement element, string __)
    {

        XmlAttribute mWidth, mHeight;
        mWidth = element.OwnerDocument.CreateAttribute(nameof(this.mWidth));
        mHeight = element.OwnerDocument.CreateAttribute(nameof(this.mHeight));

        mWidth.Value = this.mWidth.ToString();
        mHeight.Value = this.mHeight.ToString();

        _ = element.Attributes.Append(mWidth);
        _ = element.Attributes.Append(mHeight);
    }

    #endregion
}
