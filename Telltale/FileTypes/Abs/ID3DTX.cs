using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.Graphics;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;
//TODO: Need a new type called D3dtxMetadata in the future to slim down ID3dtx and move T3Texture metadata into that class. Haven't decided yet whether it should be an abstract class or a type that covers all D3dtx versions.
public interface ID3DTX : ITelltaleFile
{
    void ParsePixelDatas(BinaryReader reader);
    void WritePixelDatas(BinaryWriter writer);

    void ModifyD3DTX(ID3DTX d3dtx, ImageSection[] imageSections);

    #region Common

    D3DTXMetadata mD3dtxMetaData { get; set; }

    public List<byte[]> mPixelData { get; set; }

    #endregion

}
