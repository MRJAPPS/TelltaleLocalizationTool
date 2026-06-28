using System.Xml.Serialization;

namespace TelltaleLocalizationTool.BitmapFont;


[XmlRoot("font")]
public class BmpFontFile
{
    [XmlElement("info")]
    public BmpFontInfo? Info { get; set; }

    [XmlElement("common")]
    public BmpFontCommon? Common { get; set; }

    [XmlArray("pages")]
    [XmlArrayItem("page")]
    public List<BmpFontPage> Pages { get; set; }

    [XmlArray("chars")]
    [XmlArrayItem("char")]
    public List<BmpFontChar> Chars { get; set; }

    [XmlArray("kernings")]
    [XmlArrayItem("kerning")]
    public List<BmpFontKerning> Kernings { get; set; }

    public BmpFontFile()
    {
        Pages = [];
        Chars = [];
        Kernings = [];
    }

    public BmpFontFile(
        BmpFontInfo info,
        BmpFontCommon common,
        List<BmpFontPage> pages,
        List<BmpFontChar> chars,
        List<BmpFontKerning> kernings)
    {
        Info = info;
        Common = common;
        Pages = pages ?? [];
        Chars = chars ?? [];
        Kernings = kernings ?? new List<BmpFontKerning>();
    }
}