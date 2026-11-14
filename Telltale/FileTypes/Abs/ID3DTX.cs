using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.Graphics;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;
public interface ID3DTX : ITelltaleFile
{
    void ParsePixelDatas(BinaryReader reader);
    void WritePixelDatas(BinaryWriter writer);

    void ModifyD3DTX(ID3DTX d3dtx, ImageSection[] imageSections);

    #region Common

    D3DTXMetadata mD3dtxMetaData { get; }

    public List<byte[]> mPixelData { get; set; }

    public int Version { get; }

    #endregion

}
