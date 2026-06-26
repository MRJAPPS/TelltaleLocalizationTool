using System.Xml.Serialization;

namespace TelltaleLocalizationTool.BitmapFont;

public class BmpFontPage
{
    [XmlAttribute("id")]
    public int Id { get; set; }

    [XmlAttribute("file")]
    public required string File { get; set; }
}