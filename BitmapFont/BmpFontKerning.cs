using System.Xml.Serialization;

namespace TelltaleLocalizationTool.BitmapFont;

public class BmpFontKerning
{
    [XmlAttribute("first")]
    public int First { get; set; }

    [XmlAttribute("second")]
    public int Second { get; set; }

    [XmlAttribute("amount")]
    public int Amount { get; set; }
}