using Hexa.NET.DirectXTex;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Font.Glyph;

[Description("I want to read a glyph's info!")]
public class GlyphInfo : ReportBase, ITelltaleObject
{

    #region Public props

    public FChar mChar { get; set; } = new();

    public int mTexturePage { get; set; }

    public int mChannel { get; set; }

    public TRect mRect { get; set; } = new();

    public GlyphSize mGlyphSize { get; set; } = new();

    public GlyphMetrics mGlyphMetrics { get; set; } = new();

    #endregion

    #region Implement

    public uint Size => sizeof(int) + sizeof(float) * 6 + mRect.Size + mChar.Size;

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mChar.Parse(reader);
        mTexturePage = reader.ReadInt32();
        mChannel = reader.ReadInt32();
        mRect.Parse(reader);
        mGlyphSize.Parse(reader);
        mGlyphMetrics.Parse(reader);

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(GlyphInfo), name);

        ParseBasicTypes(myElement);

        mChar.Parse(myElement, nameof(mChar));
        mRect.Parse(myElement, nameof(mRect));
        mGlyphSize.Parse(myElement, nameof(mGlyphSize));
        mGlyphMetrics.Parse(myElement, nameof(mGlyphMetrics));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        mChar.Write(writer);
        writer.Write(mTexturePage);
        writer.Write(mChannel);
        mRect.Write(writer);
        mGlyphSize.Write(writer);
        mGlyphMetrics.Write(writer);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(GlyphInfo));
        myElement.SetAttribute(nameof(name), name);

        AppendBasicTypes(myElement);
        mChar.Write(myElement, nameof(mChar));
        mRect.Write(myElement, nameof(mRect));
        mGlyphSize.Write(myElement, nameof(mGlyphSize));
        mGlyphMetrics.Write(myElement, nameof(mGlyphMetrics));

        _ = element.AppendChild(myElement);
    }

    #endregion

    #region XML

    private void ParseBasicTypes(XmlElement element)
    {
        mChannel = int.Parse(element.GetAttribute(nameof(mChannel)));
        mTexturePage = int.Parse(element.GetAttribute(nameof(mTexturePage)));
    }

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mChannel, mTexturePage;

        mChannel = element.OwnerDocument.CreateAttribute(nameof(this.mChannel));
        mTexturePage = element.OwnerDocument.CreateAttribute(nameof(this.mTexturePage));

        mChannel.Value = this.mChannel.ToString();
        mTexturePage.Value = this.mTexturePage.ToString();

        _ = element.Attributes.Append(mChannel);
        _ = element.Attributes.Append(mTexturePage);
    }

    #endregion
}
