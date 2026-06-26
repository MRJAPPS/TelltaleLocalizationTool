using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.T3ToonGraReg;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Enum;
using TelltaleLocalizationTool.Telltale.Graphics;
using TelltaleLocalizationTool.Telltale.Meta;
using TelltaleLocalizationTool.Telltale.Meta.Versions;
using TelltaleLocalizationTool.Telltale.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
[Description("Reading a D3DTX version 9")]
public class D3DTXV9(bool AutoPixelData = true) : ReportBase, ID3DTX
{
    #region Private constants

    private const int MyVersion = 9;
    private const string PixelData = nameof(PixelData);
    private const uint Msv6Magic = 0x4d535636;

    private const uint Enums = sizeof(T3SurfaceFormat) + sizeof(T3TextureLayout) + sizeof(T3SurfaceGamma) +
                 sizeof(T3SurfaceMultisample) + sizeof(T3ResourceUsage) + sizeof(T3TextureType) +
                sizeof(T3TextureAlphaMode) + sizeof(T3TextureColor);
    private const int BasicTypes = sizeof(int) + (sizeof(float) * 4);

    #endregion

    #region private fields

    private byte[] msv6Header = [];//Use this for xml export/import

    #endregion

    #region Private props

    private uint DefaultSectionChunkSize => BasicTypes + ClassTypes + Enums + (mAuxilaryData?.Size ?? 0);
    private uint ClassTypes => mSamplerState.Size + mPlatform.Size + mName.Size + mImportName.Size +
                 mToolProps.Size + mTextureInfo.Size + mSwizzle.Size +
                 mUVOffset.Size + mUVScale.Size + mArrayFrameNames.Size +
                 mToonRegions.Size + mStreamHeader.Size;
    private uint AsyncSectionChunkSize => (uint)mPixelData.Sum(item => item.Length);

    #endregion

    #region Implement

    public int mVersion { get; set; }

    public T3SamplerStateBlock mSamplerState { get; set; } = new();

    public FixedEnum<T3PlatformType> mPlatform { get; set; } = new();//TODO: Move this to TTypes filter

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

    public StreamHeaderInfo mStreamHeader { get; set; } = new(MyVersion);

    public AuxilaryData? mAuxilaryData { get; set; }

    public List<byte[]> mPixelData { get; set; } = [];

    public uint Size => DefaultSectionChunkSize + AsyncSectionChunkSize;

    public long Position { get; private set; }

    public IMetaHeader? Header { get; private set; }

    public void Parse(BinaryReader reader)
    {
        ParseMetaHeader(reader);
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
        if (AutoPixelData)
        {
            ParsePixelDatas(reader);
        }

        Report(this, extra: $"(APD: {AutoPixelData}),(nameof(mSurfaceGamma) => {mSurfaceGamma}, nameof(mSurfaceFormat) => {mSurfaceFormat}, nameof(mTextureLayout) => {mTextureLayout}");
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(D3DTXV9), name);

        ParseBasicTypes(myElement);

        ParseHeader(myElement);

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
        ParsePixelData(myElement);
    }

    public void ParsePixelDatas(BinaryReader reader)
    {
        mPixelData = [];
        foreach (RegionStreamHeader item in mStreamHeader)
        {
            int dataSize = (int)item.mDataSize;
            byte[] imageData = reader.ReadBytes(dataSize);
            mPixelData.Add(imageData);
        }
    }

    public void Write(BinaryWriter writer)
    {
        WriteMetaHeader(writer);

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

        if (AutoPixelData)
        {
            WritePixelDatas(writer);
        }

        Report(this, true, $"(APD: {AutoPixelData}),(nameof(mSurfaceGamma) => {mSurfaceGamma}, nameof(mSurfaceFormat) => {mSurfaceFormat}, nameof(mTextureLayout) => {mTextureLayout}");
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(D3DTXV9));
        myElement.SetAttribute(nameof(name), name);

        AppendBasicTypes(myElement);

        AppendHeader(myElement);

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

        AppendPixelData(myElement);

        _ = element.AppendChild(myElement);
    }

    public void WritePixelDatas(BinaryWriter writer)
    {
        for (int i = 0; i < mPixelData.Count; i++)
        {
            writer.Write(mPixelData[i]);
        }
    }

    public void ModifyD3DTX(ID3DTX d3dtx, ImageSection[] imageSections)
    {
        mTextureInfo = d3dtx.mTextureInfo;
        mSurfaceGamma = d3dtx.mSurfaceGamma;

        mPixelData.Clear();
        mPixelData = TextureManager.GetPixelDataListFromSections(imageSections);

        mStreamHeader = new(MyVersion)
        {
            mRegionCount = imageSections.Length,
            mAuxDataCount = mStreamHeader.mAuxDataCount,
            mTotalDataSize = (int)ByteFunctions.GetByteArrayListElementsCount(mPixelData)
        };

        for (int i = 0; i < mStreamHeader.mRegionCount; i++)
        {
            RegionStreamHeader item = new(MyVersion)
            {
                mDataSize = (uint)mPixelData[i].Length,
                mMipCount = 1, // mMipCount is a strange variable, it is always 1 for every single texture
                mPitch = (int)imageSections[i].RowPitch,
                mSlicePitch = (int)imageSections[i].SlicePitch,
            };
            mStreamHeader.Add(item);
        }

        if (d3dtx.IsCubemap())
        {
            mTextureInfo.mArraySize /= 6;
            mTextureLayout = mTextureInfo.mArraySize > 1 ? T3TextureLayout.TextureCubemapArray : T3TextureLayout.TextureCubemap;

            int interval = mStreamHeader.mRegionCount / (int)mTextureInfo.mNumMipLevels;
            // Example a cube array textures with 5 mips will have 30 regions (6 faces * 5 mips)
            // If the array is 2 element there will be 60 regions (6 faces * 5 mips * 2 elements)
            // The mip index will be the region index % interval
            for (int i = 0; i < mStreamHeader.Count; i++)
            {
                mStreamHeader[i].mFaceIndex = i % (6 * (int)mTextureInfo.mArraySize); // Unknown guess, it could be 6 or 6 * mArraySize
                mStreamHeader[i].mMipIndex = (mStreamHeader.mRegionCount - i - 1) / interval;
            }
        }

        else if (d3dtx.IsVolumemap())
        {
            mTextureLayout = T3TextureLayout.Texture3D;

            int currDepth = (int)d3dtx.mTextureInfo.mDepth;
            int currentMipIndex = (int)(d3dtx.mTextureInfo.mNumMipLevels - 1);
            int copyOfDepth = currDepth;

            int depthIndex = 0;
            int faceIndex = 0;

            for (int i = 0; i < mStreamHeader.Count; i++)
            {
                mStreamHeader[mStreamHeader.mRegionCount - currDepth + depthIndex++].mFaceIndex = faceIndex++; // NOTE: This could be different for 3D textures. I don't have information at the moment.
                mStreamHeader[i].mMipIndex = currentMipIndex;

                if (depthIndex == copyOfDepth)
                {
                    currentMipIndex--;

                    copyOfDepth = Math.Max(copyOfDepth / 2, 1);
                    currDepth += copyOfDepth;

                    depthIndex = 0;
                    faceIndex = 0;
                }
            }
        }
        else
        {
            mTextureLayout = mTextureInfo.mArraySize > 1 ? T3TextureLayout.Texture2DArray : T3TextureLayout.Texture2D;

            int interval = mStreamHeader.mRegionCount / (int)mTextureInfo.mNumMipLevels;

            for (int i = 0; i < mStreamHeader.mRegionCount; i++)
            {
                mStreamHeader[i].mFaceIndex = i % (int)mTextureInfo.mArraySize;
                mStreamHeader[i].mMipIndex = (mStreamHeader.mRegionCount - i - 1) / interval;
            }
        }
    }

    #endregion

    #region Header

    public void WriteMetaHeader(BinaryWriter writer)
    {
        if (Header == null)
        {
            throw new NullReferenceException($"{nameof(Header)} is not properly initialized!");
        }

        uint debugSectionChunkSize = ((MSV6)Header).mDebugSectionChunkSize;
        Header.SetMetaSectionChunkSizes(DefaultSectionChunkSize, debugSectionChunkSize, AsyncSectionChunkSize);
        Header.WriteToBinary(writer);
    }

    public void ParseMetaHeader(BinaryReader reader)
    {
        long Position = reader.BaseStream.Position;
        uint magic = reader.ReadUInt32();
        if (magic != Msv6Magic)
        {
            throw new NotImplementedException("Right now I can only handle MSV6!! I can't deal with the Meta Stream version of the input file!");
        }

        reader.BaseStream.Position = Position;
        Header = MetaHeaderFactory.CreateMetaHeader(Meta.Enums.MetaVersion.MSV6);
        Header.ReadFromBinary(reader);
        //===========================
        reader.BaseStream.Position -= Header.GetHeaderByteSize();
        msv6Header = new byte[Header.GetHeaderByteSize()];
        _ = reader.Read(msv6Header);
    }

    #endregion

    #region XML

    public void AppendBasicTypes(XmlElement element)
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

    public void ParseBasicTypes(XmlElement element)
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

    public void AppendPixelData(XmlElement element)
    {
        foreach (byte[] pixelBytes in mPixelData)
        {
            XmlElement pixelElement = element.OwnerDocument.CreateElement(nameof(PixelData));
            pixelElement.InnerText = System.Convert.ToBase64String(pixelBytes);
            _ = element.AppendChild(pixelElement);
        }
    }

    public void ParsePixelData(XmlElement element)
    {
        mPixelData = [];
        foreach (XmlNode node in element.ChildNodes)
        {
            if (node is XmlElement pixelElement && pixelElement.Name == nameof(PixelData))
            {
                byte[] data = System.Convert.FromBase64String(pixelElement.InnerText);
                mPixelData.Add(data);
            }
        }
    }

    private void ParseHeader(XmlElement element)
    {
        XmlElement msv6H = element.GetSingleElement(nameof(msv6Header));
        msv6Header = System.Convert.FromBase64String(msv6H.InnerText);
        using MemoryStream headerTempStream = new(msv6Header);
        using BinaryReader reader = new(headerTempStream);
        Header ??= MetaHeaderFactory.CreateMetaHeader(Meta.Enums.MetaVersion.MSV6);
        Header.ReadFromBinary(reader);
    }

    private void AppendHeader(XmlElement element)
    {
        string headerB64 = System.Convert.ToBase64String(msv6Header);
        XmlElement b64 = element.OwnerDocument.CreateElement(nameof(msv6Header));
        b64.InnerText = headerB64;
        _ = element.AppendChild(b64);
    }

    #endregion
}
