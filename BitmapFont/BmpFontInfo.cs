using System.Xml.Serialization;

namespace TelltaleLocalizationTool.BitmapFont;

public class BmpFontInfo
{
    [XmlAttribute("face")]
    public required string Face { get; set; }

    [XmlAttribute("size")]
    public int Size { get; set; }

    [XmlAttribute("bold")]
    public int Bold { get; set; }

    [XmlAttribute("italic")]
    public int Italic { get; set; }

    [XmlAttribute("charset")]
    public required string Charset { get; set; }

    [XmlAttribute("unicode")]
    public int Unicode { get; set; }

    [XmlAttribute("stretchH")]
    public int StretchH { get; set; }

    [XmlAttribute("smooth")]
    public int Smooth { get; set; }

    [XmlAttribute("aa")]
    public int Aa { get; set; }

    [XmlAttribute("padding")]
    public required string Padding { get; set; }

    [XmlAttribute("spacing")]
    public required string Spacing { get; set; }

    [XmlAttribute("outline")]
    public int Outline { get; set; }
}
