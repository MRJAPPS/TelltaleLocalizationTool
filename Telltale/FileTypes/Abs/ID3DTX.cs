using System.Xml;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;
public interface ID3DTX : ITelltaleFile
{
    void ParsePixelDatas(BinaryReader reader);
    void WritePixelDatas(BinaryWriter writer);

}
