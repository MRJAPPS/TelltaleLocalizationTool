using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.T3ToonGraReg;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Enum;
using TelltaleLocalizationTool.Telltale.Graphics;
using TelltaleLocalizationTool.Telltale.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;
public interface ID3DTX : ITelltaleFile
{
    void ParsePixelDatas(BinaryReader reader);
    void WritePixelDatas(BinaryWriter writer);

    void ModifyD3DTX(ID3DTX d3dtx, ImageSection[] imageSections);

    #region Common

    int mVersion { get; set; }

    T3SamplerStateBlock mSamplerState { get; set; }

    FixedEnum<T3PlatformType> mPlatform { get; set; }//TODO: Move this to TTypes filter

    TString mName { get; set; }

    TString mImportName { get; set; }

    float mImportScale { get; set; }

    ToolProps mToolProps { get; set; }

    TextureInfo mTextureInfo { get; set; }

    T3SurfaceFormat mSurfaceFormat { get; set; }

    T3TextureLayout mTextureLayout { get; set; }

    T3SurfaceGamma mSurfaceGamma { get; set; }

    T3SurfaceMultisample mSurfaceMultisample { get; set; }

    T3ResourceUsage mResourceUsage { get; set; }

    T3TextureType mType { get; set; }

    RenderSwizzleParams mSwizzle { get; set; }

    float mSpecularGlossExponent { get; set; }

    float mHDRLightmapScale { get; set; }

    float mToonGradientCutoff { get; set; }

    T3TextureAlphaMode mAlphaMode { get; set; }

    T3TextureColor mColorMode { get; set; }

    Vector2 mUVOffset { get; set; }

    Vector2 mUVScale { get; set; }

    TSymbolValueCollection mArrayFrameNames { get; set; }

    T3ToonGradientRegionCollection mToonRegions { get; set; }

    StreamHeaderInfo mStreamHeader { get; set; }

    AuxilaryData? mAuxilaryData { get; set; }

    List<byte[]> mPixelData { get; set; }
    #endregion

}
