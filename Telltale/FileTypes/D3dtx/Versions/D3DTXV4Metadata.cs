using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.T3ToonGraReg;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.TTypes;
using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
[Description("I'm reading a texture's metadata.")]
public class D3DTXV4Metadata(int version = 4) : D3DTXMetadata(version)
{
    #region Private constants

    private const string PixelData = nameof(PixelData);

    private const uint Enums = sizeof(T3SurfaceFormat) + sizeof(T3ResourceUsage) +
        sizeof(T3TextureType) + sizeof(T3TextureAlphaMode) + sizeof(T3TextureColor);
    private const int BasicTypes = (sizeof(int) * 2) + (sizeof(float) * 3);

    #endregion

    #region Private props

    private uint classTypes => mSamplerState.Size + mPlatform.Size + mName.Size + mImportName.Size +
        mToolProps.Size + mTextureInfo.Size + mUVOffset.Size + mUVScale.Size +
        mToonRegions.Size + mStreamHeader.Size;

    #endregion

    #region public props

    public int mNormalMapFormat { get; set; }

    #endregion

    #region Implement

    public override uint Size => BasicTypes + classTypes + Enums + (mAuxilaryData?.Size ?? 0);

    public override void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mVersion = reader.ReadInt32();
        if (mVersion != 4)
        {
            throw new InvalidDataException($"We need D3DTX version 4, but this texture is version {mVersion}!");
        }
        mSamplerState.Parse(reader);
        mPlatform.Parse(reader);
        mName.Parse(reader);
        mImportName.Parse(reader);
        mImportScale = reader.ReadSingle();
        mToolProps.Parse(reader);
        mTextureInfo.Parse(reader);
        mSurfaceFormat = (T3SurfaceFormat)reader.ReadUInt32();
        mResourceUsage = (T3ResourceUsage)reader.ReadUInt32();
        mType = (T3TextureType)reader.ReadUInt32();
        mNormalMapFormat = reader.ReadInt32();
        mHDRLightmapScale = reader.ReadSingle();
        mToonGradientCutoff = reader.ReadSingle();
        mAlphaMode = (T3TextureAlphaMode)reader.ReadUInt32();
        mColorMode = (T3TextureColor)reader.ReadUInt32();
        mUVOffset.Parse(reader);
        mUVScale.Parse(reader);
        mToonRegions.Parse(reader);
        mStreamHeader.Parse(reader);
        mAuxilaryData = new(mStreamHeader.mAuxDataCount);
        mAuxilaryData.Parse(reader);

        Report(this);

    }

    public override void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(D3DTXV4Metadata), name);
        ParseBasicTypes(myElement);
        mSamplerState.Parse(myElement, nameof(mSamplerState));
        mPlatform.Parse(myElement, nameof(mPlatform));
        mName.Parse(myElement, nameof(mName));
        mImportName.Parse(myElement, nameof(mImportName));
        mToolProps.Parse(myElement, nameof(mToolProps));
        mTextureInfo.Parse(myElement, nameof(mTextureInfo));
        mUVOffset.Parse(myElement, nameof(mUVOffset));
        mUVScale.Parse(myElement, nameof(mUVScale));
        mToonRegions.Parse(myElement, nameof(mToonRegions));
        mStreamHeader.Parse(myElement, nameof(mStreamHeader));
        mAuxilaryData = new(mStreamHeader.mAuxDataCount);
        mAuxilaryData.Parse(myElement, nameof(mAuxilaryData));
    }

    public override void Write(BinaryWriter writer)
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
        writer.Write((uint)mResourceUsage);
        writer.Write((uint)mType);

        writer.Write(mNormalMapFormat);
        writer.Write(mHDRLightmapScale);
        writer.Write(mToonGradientCutoff);

        writer.Write((uint)mAlphaMode);
        writer.Write((uint)mColorMode);

        mUVOffset.Write(writer);
        mUVScale.Write(writer);
        mToonRegions.Write(writer);
        mStreamHeader.Write(writer);
        mAuxilaryData?.Write(writer);

        Report(this, true);

    }

    public override void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(D3DTXV4Metadata));
        myElement.SetAttribute(nameof(name), name);

        AppendBasicTypes(myElement);

        mSamplerState.Write(myElement, nameof(mSamplerState));
        mPlatform.Write(myElement, nameof(mPlatform));
        mName.Write(myElement, nameof(mName));
        mImportName.Write(myElement, nameof(mImportName));
        mToolProps.Write(myElement, nameof(mToolProps));
        mTextureInfo.Write(myElement, nameof(mTextureInfo));
        mUVOffset.Write(myElement, nameof(mUVOffset));
        mUVScale.Write(myElement, nameof(mUVScale));
        mToonRegions.Write(myElement, nameof(mToonRegions));
        mStreamHeader.Write(myElement, nameof(mStreamHeader));
        mAuxilaryData?.Write(myElement, nameof(mAuxilaryData));

        _ = element.AppendChild(myElement);
    }

    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mNormalMapFormatAttr = element.OwnerDocument.CreateAttribute(nameof(mNormalMapFormat));
        XmlAttribute mVersionAttr = element.OwnerDocument.CreateAttribute(nameof(mVersion));
        XmlAttribute mImportScaleAttr = element.OwnerDocument.CreateAttribute(nameof(mImportScale));
        XmlAttribute mHDRLightmapScaleAttr = element.OwnerDocument.CreateAttribute(nameof(mHDRLightmapScale));
        XmlAttribute mToonGradientCutoffAttr = element.OwnerDocument.CreateAttribute(nameof(mToonGradientCutoff));
        XmlAttribute mSurfaceFormatAttr = element.OwnerDocument.CreateAttribute(nameof(mSurfaceFormat));
        XmlAttribute mResourceUsageAttr = element.OwnerDocument.CreateAttribute(nameof(mResourceUsage));
        XmlAttribute mTypeAttr = element.OwnerDocument.CreateAttribute(nameof(mType));
        XmlAttribute mAlphaModeAttr = element.OwnerDocument.CreateAttribute(nameof(mAlphaMode));
        XmlAttribute mColorModeAttr = element.OwnerDocument.CreateAttribute(nameof(mColorMode));

        mNormalMapFormatAttr.Value = mNormalMapFormat.ToString();
        mVersionAttr.Value = mVersion.ToString();
        mImportScaleAttr.Value = mImportScale.ToString();
        mHDRLightmapScaleAttr.Value = mHDRLightmapScale.ToString();
        mToonGradientCutoffAttr.Value = mToonGradientCutoff.ToString();
        mSurfaceFormatAttr.Value = mSurfaceFormat.ToString();
        mResourceUsageAttr.Value = mResourceUsage.ToString();
        mTypeAttr.Value = mType.ToString();
        mAlphaModeAttr.Value = mAlphaMode.ToString();
        mColorModeAttr.Value = mColorMode.ToString();

        _ = element.Attributes.Append(mNormalMapFormatAttr);
        _ = element.Attributes.Append(mVersionAttr);
        _ = element.Attributes.Append(mImportScaleAttr);
        _ = element.Attributes.Append(mHDRLightmapScaleAttr);
        _ = element.Attributes.Append(mToonGradientCutoffAttr);
        _ = element.Attributes.Append(mSurfaceFormatAttr);
        _ = element.Attributes.Append(mResourceUsageAttr);
        _ = element.Attributes.Append(mTypeAttr);
        _ = element.Attributes.Append(mAlphaModeAttr);
        _ = element.Attributes.Append(mColorModeAttr);
    }

    private void ParseBasicTypes(XmlElement element)
    {
        mNormalMapFormat = int.Parse(element.GetAttribute(nameof(mNormalMapFormat)));
        mVersion = int.Parse(element.GetAttribute(nameof(mVersion)));
        mImportScale = float.Parse(element.GetAttribute(nameof(mImportScale)));
        mHDRLightmapScale = float.Parse(element.GetAttribute(nameof(mHDRLightmapScale)));
        mToonGradientCutoff = float.Parse(element.GetAttribute(nameof(mToonGradientCutoff)));
        mSurfaceFormat = Enum.Parse<T3SurfaceFormat>(element.GetAttribute(nameof(mSurfaceFormat)));
        mResourceUsage = Enum.Parse<T3ResourceUsage>(element.GetAttribute(nameof(mResourceUsage)));
        mType = Enum.Parse<T3TextureType>(element.GetAttribute(nameof(mType)));
        mAlphaMode = Enum.Parse<T3TextureAlphaMode>(element.GetAttribute(nameof(mAlphaMode)));
        mColorMode = Enum.Parse<T3TextureColor>(element.GetAttribute(nameof(mColorMode)));
    }

    #endregion
}
