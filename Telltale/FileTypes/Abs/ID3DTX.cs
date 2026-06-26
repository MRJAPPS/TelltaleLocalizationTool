using System;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.T3ToonGraReg;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Enum;
using TelltaleLocalizationTool.Telltale.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;
public interface ID3DTX : ITelltaleFile
{
    void ParsePixelDatas(BinaryReader reader);
    void WritePixelDatas(BinaryWriter writer);
    #region Common

    public int mVersion { get; set; }

    public T3SamplerStateBlock mSamplerState { get; set; }

    public FixedEnum<T3PlatformType> mPlatform { get; set; }//TODO: Move this to TTypes filter

    public TString mName { get; set; }

    public TString mImportName { get; set; }

    public float mImportScale { get; set; }

    public ToolProps mToolProps { get; set; }

    public TextureInfo mTextureInfo { get; set; }

    public T3SurfaceFormat mSurfaceFormat { get; set; }

    public T3TextureLayout mTextureLayout { get; set; }

    public T3SurfaceGamma mSurfaceGamma { get; set; }

    public T3SurfaceMultisample mSurfaceMultisample { get; set; }

    public T3ResourceUsage mResourceUsage { get; set; }

    public T3TextureType mType { get; set; }

    public RenderSwizzleParams mSwizzle { get; set; } 

    public float mSpecularGlossExponent { get; set; }

    public float mHDRLightmapScale { get; set; }

    public float mToonGradientCutoff { get; set; }

    public T3TextureAlphaMode mAlphaMode { get; set; }

    public T3TextureColor mColorMode { get; set; }

    public Vector2 mUVOffset { get; set; }

    public Vector2 mUVScale { get; set; }

    public TSymbolValueCollection mArrayFrameNames { get; set; }

    public T3ToonGradientRegionCollection mToonRegions { get; set; }

    public StreamHeaderInfo mStreamHeader { get; set; }

    public AuxilaryData? mAuxilaryData { get; set; }

    public List<byte[]> mPixelData { get; set; }
    #endregion

}
