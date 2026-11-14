using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.T3ToonGraReg;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.TTypes;
using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
[Description("I'm reading a texture's metadata(D3DTXMetadata).")]
public abstract class D3DTXMetadata(int version) : ReportBase, ITelltaleObject
{
    #region Public props

    public int mVersion { get; set; } = version;

    public T3SamplerStateBlock mSamplerState { get; set; } = new();

    public FixedEnum<T3PlatformType> mPlatform { get; set; } = new();

    public TString mName { get; set; } = string.Empty;

    public TString mImportName { get; set; } = string.Empty;

    public float mImportScale { get; set; }

    public ToolProps mToolProps { get; set; } = new();

    public TextureInfo mTextureInfo { get; set; } = new(version);

    public T3SurfaceFormat mSurfaceFormat { get; set; }

    //Normalized values for conversion. V4 is always a linear 2D texture.
    public T3TextureLayout mTextureLayout { get; set; } = T3TextureLayout.Texture2D;

    public T3SurfaceGamma mSurfaceGamma { get; set; } = T3SurfaceGamma.Linear;

    public T3ResourceUsage mResourceUsage { get; set; }

    public T3TextureType mType { get; set; }

    public float mHDRLightmapScale { get; set; }

    public float mToonGradientCutoff { get; set; }

    public T3TextureAlphaMode mAlphaMode { get; set; }

    public T3TextureColor mColorMode { get; set; }

    public Vector2 mUVOffset { get; set; } = new();

    public Vector2 mUVScale { get; set; } = new();

    public T3ToonGradientRegionCollection mToonRegions { get; set; } = [];

    public StreamHeaderInfo mStreamHeader { get; set; } = new(version);

    public AuxilaryData? mAuxilaryData { get; set; }

    #endregion

    #region Implement

    public MetaStreamContext? msCtx { get; protected set; }

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    public abstract uint Size { get; }

    public long Position { get; protected set; } = -1;

    //It's my children's job to implement you, since I only have the core shared parts left now!
    public abstract void Parse(BinaryReader reader);
    public abstract void Parse(XmlElement reader, string name);
    public abstract void Write(BinaryWriter writer);
    public abstract void Write(XmlElement element, string name);

    #endregion
}
