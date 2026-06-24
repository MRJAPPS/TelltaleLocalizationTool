using System.Xml;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;
public interface ID3DTX : ITelltaleObject
{
    void ParsePixelData(BinaryReader reader, long size);
    void WritePixelData(BinaryWriter writer, long size);
    void ParsePixelData(XmlElement element);
    void WritePixelData(XmlElement element);
}
