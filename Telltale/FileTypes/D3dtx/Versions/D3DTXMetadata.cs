using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.T3ToonGraReg;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
[Description("I'm reading a texture's metadata.")]
public class D3DTXMetadata(int version) : ReportBase, ITelltaleObject
{
    #region Private constants

    private const string PixelData = nameof(PixelData);

    private const uint Enums = sizeof(T3SurfaceFormat) + sizeof(T3TextureLayout) + sizeof(T3SurfaceGamma) +
                 sizeof(T3SurfaceMultisample) + sizeof(T3ResourceUsage) + sizeof(T3TextureType) +
                sizeof(T3TextureAlphaMode) + sizeof(T3TextureColor);
    private const int BasicTypes = sizeof(int) + (sizeof(float) * 4);

    #endregion

    #region Private props

    private uint classTypes => mSamplerState.Size + mPlatform.Size + mName.Size + mImportName.Size +
             mToolProps.Size + mTextureInfo.Size + mSwizzle.Size +
             mUVOffset.Size + mUVScale.Size + mArrayFrameNames.Size +
             mToonRegions.Size + mStreamHeader.Size;

    #endregion

    #region public props

    public int mVersion { get; set; }

    public T3SamplerStateBlock mSamplerState { get; set; } = new();

    public FixedEnum<T3PlatformType> mPlatform { get; set; } = new();

    public TString mName { get; set; } = string.Empty;

    public TString mImportName { get; set; } = string.Empty;

    public float mImportScale { get; set; }

    public ToolProps mToolProps { get; set; } = new();

    public TextureInfo mTextureInfo { get; set; } = new();

    public T3SurfaceFormat mSurfaceFormat { get; set; }

    public T3TextureLayout mTextureLayout { get; set; }

    public T3SurfaceGamma mSurfaceGamma { get; set; }

    public T3SurfaceMultisample mSurfaceMultisample { get; set; }

    public T3ResourceUsage mResourceUsage { get; set; }

    public T3TextureType mType { get; set; }

    public RenderSwizzleParams mSwizzle { get; set; } = new();

    public float mSpecularGlossExponent { get; set; }

    public float mHDRLightmapScale { get; set; }

    public float mToonGradientCutoff { get; set; }

    public T3TextureAlphaMode mAlphaMode { get; set; }

    public T3TextureColor mColorMode { get; set; }

    public Vector2 mUVOffset { get; set; } = new();

    public Vector2 mUVScale { get; set; } = new();

    public TSymbolValueCollection mArrayFrameNames { get; set; } = [];

    public T3ToonGradientRegionCollection mToonRegions { get; set; } = [];

    public StreamHeaderInfo mStreamHeader { get; set; } = new(version);

    public AuxilaryData? mAuxilaryData { get; set; }

    #endregion

    #region Implement 

    public uint Size => BasicTypes + classTypes + Enums + (mAuxilaryData?.Size ?? 0);

    public long Position { get; private set; }

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mVersion = reader.ReadInt32();
        mSamplerState.Parse(reader);
        mPlatform.Parse(reader);
        mName.Parse(reader);
        mImportName.Parse(reader);
        mImportScale = reader.ReadSingle();
        mToolProps.Parse(reader);
        mTextureInfo.Parse(reader);
        mSurfaceFormat = (T3SurfaceFormat)reader.ReadUInt32();
        mTextureLayout = (T3TextureLayout)reader.ReadUInt32();
        mSurfaceGamma = (T3SurfaceGamma)reader.ReadUInt32();
        mSurfaceMultisample = (T3SurfaceMultisample)reader.ReadUInt32();
        mResourceUsage = (T3ResourceUsage)reader.ReadUInt32();
        mType = (T3TextureType)reader.ReadUInt32();
        mSwizzle.Parse(reader);
        mSpecularGlossExponent = reader.ReadSingle();
        mHDRLightmapScale = reader.ReadSingle();
        mToonGradientCutoff = reader.ReadSingle();
        mAlphaMode = (T3TextureAlphaMode)reader.ReadUInt32();
        mColorMode = (T3TextureColor)reader.ReadUInt32();
        mUVOffset.Parse(reader);
        mUVScale.Parse(reader);
        mArrayFrameNames.Parse(reader);
        mToonRegions.Parse(reader);
        mStreamHeader.Parse(reader);
        mAuxilaryData = new(mStreamHeader.mAuxDataCount);
        mAuxilaryData.Parse(reader);

        Report(this, extra: $"({nameof(mSurfaceGamma)} => {mSurfaceGamma}, {nameof(mSurfaceFormat)} => {mSurfaceFormat},{nameof(mTextureLayout)} => {mTextureLayout}");
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(D3DTXMetadata), name);
        ParseBasicTypes(myElement);
        mSamplerState.Parse(myElement, nameof(mSamplerState));
        mPlatform.Parse(myElement, nameof(mPlatform));
        mName.Parse(myElement, nameof(mName));
        mImportName.Parse(myElement, nameof(mImportName));
        mToolProps.Parse(myElement, nameof(mToolProps));
        mTextureInfo.Parse(myElement, nameof(mTextureInfo));
        mSwizzle.Parse(myElement, nameof(mSwizzle));
        mUVOffset.Parse(myElement, nameof(mUVOffset));
        mUVScale.Parse(myElement, nameof(mUVScale));
        mArrayFrameNames.Parse(myElement, nameof(mArrayFrameNames));
        mToonRegions.Parse(myElement, nameof(mToonRegions));
        mStreamHeader.Parse(myElement, nameof(mStreamHeader));
        mAuxilaryData = new(mStreamHeader.mAuxDataCount);
        mAuxilaryData.Parse(reader, nameof(mAuxilaryData));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(mVersion);
        mSamplerState.Write(writer);
        mPlatform.Write(writer);
        mName.Write(writer);
        mImportName.Write(writer);
        writer.Write(mImportScale);
        mToolProps.Write(writer);
        mTextureInfo.Write(writer);

        writer.Write((uint)mSurfaceFormat);
        writer.Write((uint)mTextureLayout);
        writer.Write((uint)mSurfaceGamma);
        writer.Write((uint)mSurfaceMultisample);
        writer.Write((uint)mResourceUsage);
        writer.Write((uint)mType);

        mSwizzle.Write(writer);

        writer.Write(mSpecularGlossExponent);
        writer.Write(mHDRLightmapScale);
        writer.Write(mToonGradientCutoff);

        writer.Write((uint)mAlphaMode);
        writer.Write((uint)mColorMode);

        mUVOffset.Write(writer);
        mUVScale.Write(writer);
        mArrayFrameNames.Write(writer);
        mToonRegions.Write(writer);
        mStreamHeader.Write(writer);
        mAuxilaryData = new(mStreamHeader.mAuxDataCount);
        mAuxilaryData.Write(writer);

        Report(this, true, $"({nameof(mSurfaceGamma)} => {mSurfaceGamma}, {nameof(mSurfaceFormat)} => {mSurfaceFormat},{nameof(mTextureLayout)} => {mTextureLayout}");
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(D3DTXMetadata));
        myElement.SetAttribute(nameof(name), name);

        AppendBasicTypes(myElement);

        mSamplerState.Write(myElement, nameof(mSamplerState));
        mPlatform.Write(myElement, nameof(mPlatform));
        mName.Write(myElement, nameof(mName));
        mImportName.Write(myElement, nameof(mImportName));
        mToolProps.Write(myElement, nameof(mToolProps));
        mTextureInfo.Write(myElement, nameof(mTextureInfo));
        mSwizzle.Write(myElement, nameof(mSwizzle));
        mUVOffset.Write(myElement, nameof(mUVOffset));
        mUVScale.Write(myElement, nameof(mUVScale));
        mArrayFrameNames.Write(myElement, nameof(mArrayFrameNames));
        mToonRegions.Write(myElement, nameof(mToonRegions));
        mStreamHeader.Write(myElement, nameof(mStreamHeader));
        mAuxilaryData = new(mStreamHeader.mAuxDataCount);
        mAuxilaryData.Write(element, nameof(mAuxilaryData));

        _ = element.AppendChild(myElement);
    }

    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mVersionAttr = element.OwnerDocument.CreateAttribute(nameof(mVersion));
        XmlAttribute mImportScaleAttr = element.OwnerDocument.CreateAttribute(nameof(mImportScale));
        XmlAttribute mSpecularGlossExponentAttr = element.OwnerDocument.CreateAttribute(nameof(mSpecularGlossExponent));
        XmlAttribute mHDRLightmapScaleAttr = element.OwnerDocument.CreateAttribute(nameof(mHDRLightmapScale));
        XmlAttribute mToonGradientCutoffAttr = element.OwnerDocument.CreateAttribute(nameof(mToonGradientCutoff));
        XmlAttribute mSurfaceFormatAttr = element.OwnerDocument.CreateAttribute(nameof(mSurfaceFormat));
        XmlAttribute mTextureLayoutAttr = element.OwnerDocument.CreateAttribute(nameof(mTextureLayout));
        XmlAttribute mSurfaceGammaAttr = element.OwnerDocument.CreateAttribute(nameof(mSurfaceGamma));
        XmlAttribute mSurfaceMultisampleAttr = element.OwnerDocument.CreateAttribute(nameof(mSurfaceMultisample));
        XmlAttribute mResourceUsageAttr = element.OwnerDocument.CreateAttribute(nameof(mResourceUsage));
        XmlAttribute mTypeAttr = element.OwnerDocument.CreateAttribute(nameof(mType));
        XmlAttribute mAlphaModeAttr = element.OwnerDocument.CreateAttribute(nameof(mAlphaMode));
        XmlAttribute mColorModeAttr = element.OwnerDocument.CreateAttribute(nameof(mColorMode));

        mVersionAttr.Value = mVersion.ToString();
        mImportScaleAttr.Value = mImportScale.ToString();
        mSpecularGlossExponentAttr.Value = mSpecularGlossExponent.ToString();
        mHDRLightmapScaleAttr.Value = mHDRLightmapScale.ToString();
        mToonGradientCutoffAttr.Value = mToonGradientCutoff.ToString();
        mSurfaceFormatAttr.Value = mSurfaceFormat.ToString();
        mTextureLayoutAttr.Value = mTextureLayout.ToString();
        mSurfaceGammaAttr.Value = mSurfaceGamma.ToString();
        mSurfaceMultisampleAttr.Value = mSurfaceMultisample.ToString();
        mResourceUsageAttr.Value = mResourceUsage.ToString();
        mTypeAttr.Value = mType.ToString();
        mAlphaModeAttr.Value = mAlphaMode.ToString();
        mColorModeAttr.Value = mColorMode.ToString();

        _ = element.Attributes.Append(mVersionAttr);
        _ = element.Attributes.Append(mImportScaleAttr);
        _ = element.Attributes.Append(mSpecularGlossExponentAttr);
        _ = element.Attributes.Append(mHDRLightmapScaleAttr);
        _ = element.Attributes.Append(mToonGradientCutoffAttr);
        _ = element.Attributes.Append(mSurfaceFormatAttr);
        _ = element.Attributes.Append(mTextureLayoutAttr);
        _ = element.Attributes.Append(mSurfaceGammaAttr);
        _ = element.Attributes.Append(mSurfaceMultisampleAttr);
        _ = element.Attributes.Append(mResourceUsageAttr);
        _ = element.Attributes.Append(mTypeAttr);
        _ = element.Attributes.Append(mAlphaModeAttr);
        _ = element.Attributes.Append(mColorModeAttr);
    }

    private void ParseBasicTypes(XmlElement element)
    {
        mVersion = int.Parse(element.GetAttribute(nameof(mVersion)));
        mImportScale = float.Parse(element.GetAttribute(nameof(mImportScale)));
        mSpecularGlossExponent = float.Parse(element.GetAttribute(nameof(mSpecularGlossExponent)));
        mHDRLightmapScale = float.Parse(element.GetAttribute(nameof(mHDRLightmapScale)));
        mToonGradientCutoff = float.Parse(element.GetAttribute(nameof(mToonGradientCutoff)));
        mSurfaceFormat = Enum.Parse<T3SurfaceFormat>(element.GetAttribute(nameof(mSurfaceFormat)));
        mTextureLayout = Enum.Parse<T3TextureLayout>(element.GetAttribute(nameof(mTextureLayout)));
        mSurfaceGamma = Enum.Parse<T3SurfaceGamma>(element.GetAttribute(nameof(mSurfaceGamma)));
        mSurfaceMultisample = Enum.Parse<T3SurfaceMultisample>(element.GetAttribute(nameof(mSurfaceMultisample)));
        mResourceUsage = Enum.Parse<T3ResourceUsage>(element.GetAttribute(nameof(mResourceUsage)));
        mType = Enum.Parse<T3TextureType>(element.GetAttribute(nameof(mType)));
        mAlphaMode = Enum.Parse<T3TextureAlphaMode>(element.GetAttribute(nameof(mAlphaMode)));
        mColorMode = Enum.Parse<T3TextureColor>(element.GetAttribute(nameof(mColorMode)));
    }

    #endregion
}
