using System.Xml.Serialization;

namespace TelltaleLocalizationTool.BitmapFont;

public class BmpFontCommon
{
    [XmlAttribute("lineHeight")]
    public int LineHeight { get; set; }

    [XmlAttribute("base")]
    public int Base { get; set; }

    [XmlAttribute("scaleW")]
    public int ScaleW { get; set; }

    [XmlAttribute("scaleH")]
    public int ScaleH { get; set; }

    [XmlAttribute("pages")]
    public int Pages { get; set; }

    [XmlAttribute("packed")]
    public int Packed { get; set; }

    [XmlAttribute("alphaChnl")]
    public int AlphaChnl { get; set; }

    [XmlAttribute("redChnl")]
    public int RedChnl { get; set; }

    [XmlAttribute("greenChnl")]
    public int GreenChnl { get; set; }

    [XmlAttribute("blueChnl")]
    public int BlueChnl { get; set; }
}