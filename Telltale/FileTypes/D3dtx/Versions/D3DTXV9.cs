using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Structs;
using TelltaleLocalizationTool.Telltale.Meta.Structs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
public class D3DTXV9 : ID3DTX
{
    #region Public props

    /// <summary>
    /// [4 bytes] The header version of this class.
    /// </summary>
    public int mVersion { get; set; }

    /// <summary>
    /// [4 bytes] The mSamplerState state block size in bytes. Note: the parsed value is always 8.
    /// </summary>
    public int mSamplerState_BlockSize { get; set; }

    /// <summary>
    /// [4 bytes] The sampler state, bitflag value that contains values from T3SamplerStateValue.
    /// </summary>
    public T3SamplerStateBlock mSamplerState { get; set; }

    /// <summary>
    /// [4 bytes] The mPlatform block size in bytes. Note: the parsed value is always 8.
    /// </summary>
    public int mPlatform_BlockSize { get; set; }

    /// <summary>
    /// [4 bytes] The platform type enum value.
    /// </summary>
    public T3PlatformType mPlatform { get; set; }

    /// <summary>
    /// [4 bytes] The mName block size in bytes.
    /// </summary>
    public int mName_BlockSize { get; set; }

    /// <summary>
    /// [mName_StringLength bytes] The string mName.
    /// </summary>
    public string mName { get; set; } = string.Empty;

    /// <summary>
    /// [4 bytes] The mImportName block size in bytes.
    /// </summary>
    public int mImportName_BlockSize { get; set; }

    /// <summary>
    /// [mImportName_StringLength bytes] The mImportName string.
    /// </summary>
    public string mImportName { get; set; } = string.Empty;

    /// <summary>
    /// [4 bytes] The import scale of the texture file.
    /// </summary>
    public float mImportScale { get; set; }

    /// <summary>
    /// [1 byte] Whether or not the d3dtx contains a Tool Properties. [PropertySet] (Always false)
    /// </summary>
    public ToolProps mToolProps { get; set; }

    /// <summary>
    /// [4 bytes] The number of mip maps in the texture.
    /// </summary>
    public uint mNumMipLevels { get; set; }

    /// <summary>
    /// [4 bytes] The pixel width of the texture.
    /// </summary>
    public uint mWidth { get; set; }

    /// <summary>
    /// [4 bytes] The pixel height of the texture.
    /// </summary>
    public uint mHeight { get; set; }

    /// <summary>
    /// [4 bytes] The depth of a volume texture in pixels.
    /// </summary>
    public uint mDepth { get; set; }

    /// <summary>
    /// [4 bytes] mArraySize, not sure what this is for yet.
    /// </summary>
    public uint mArraySize { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines the compression used for the texture.
    /// </summary>
    public T3SurfaceFormat mSurfaceFormat { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines the dimension of the texture.
    /// </summary>
    public T3TextureLayout mTextureLayout { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines the gamma of the texture.
    /// </summary>
    public T3SurfaceGamma mSurfaceGamma { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines the multisample (anisotropic) level of the texture.
    /// </summary>
    public T3SurfaceMultisample mSurfaceMultisample { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines the resource type of the texture.
    /// </summary>
    public T3ResourceUsage mResourceUsage { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines what kind of texture it is.
    /// </summary>
    public T3TextureType mType { get; set; }

    /// <summary>
    /// [4 bytes] The size of the mSwizzle block data.
    /// </summary>
    public int mSwizzleSize { get; set; }

    /// <summary>
    /// [4 bytes] mSwizzle compression parameters. (usually used for consoles).
    /// </summary>
    public RenderSwizzleParams mSwizzle { get; set; }

    /// <summary>
    /// [4 bytes] Defines how glossy the texture is.
    /// </summary>
    public float mSpecularGlossExponent { get; set; }

    /// <summary>
    /// [4 bytes] Defines the brightness scale of the texture. (used for lightmaps)
    /// </summary>
    public float mHDRLightmapScale { get; set; }

    /// <summary>
    /// [4 bytes] Defines the toon cutoff gradient of the texture.
    /// </summary>
    public float mToonGradientCutoff { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines what kind of alpha the texture will have.
    /// </summary>
    public T3TextureAlphaMode mAlphaMode { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines the color range of the texture.
    /// </summary>
    public T3TextureColor mColorMode { get; set; }

    /// <summary>
    /// [8 bytes] A vector, defines the UV offset values when the shader on a material samples the texture.
    /// </summary>
    public Vector2 mUVOffset { get; set; }

    /// <summary>
    /// [8 bytes] A vector, defines the UV scale values when the shader on a material samples the texture.
    /// </summary>
    public Vector2 mUVScale { get; set; }

    /// <summary>
    /// [4 bytes] The size in bytes of the mArrayFrameNames block.
    /// </summary>
    public uint mArrayFrameNames_ArrayCapacity { get; set; }

    /// <summary>
    /// [4 bytes] The amount of elements in the mArrayFrameNames array.
    /// </summary>
    public int mArrayFrameNames_ArrayLength { get; set; }

    /// <summary>
    /// [8 bytes for each element] An array containing frame names. (Usually unused)
    /// </summary>
    public Symbol[] mArrayFrameNames { get; set; } = [];

    /// <summary>
    /// [4 bytes] The size in bytes of the mToonRegions block.
    /// </summary>
    public uint mToonRegions_ArrayCapacity { get; set; }

    /// <summary>
    /// [4 bytes] The amount of elements in the mToonRegions array.
    /// </summary>
    public int mToonRegions_ArrayLength { get; set; }

    /// <summary>
    /// [16 bytes for each element] An array containing a toon gradient region. (Usually unused)
    /// </summary>
    public T3ToonGradientRegion[] mToonRegions { get; set; } = [];

    /// <summary>
    /// [12 bytes] A struct for StreamHeader
    /// </summary>
    public StreamHeader mStreamHeader { get; set; }

    /// <summary>
    /// [24 bytes for each element] An array containing each pixel region in the texture.
    /// </summary>
    public RegionStreamHeader[] mRegionHeaders { get; set; } = [];

    /// <summary>
    /// A byte array of the pixel regions in a texture. Starts from smallest mip map to largest mip map.
    /// </summary>
    public List<byte[]> mPixelData { get; set; } = [];

    #endregion


    /// <summary>
    /// D3DTX V9 Header (empty constructor, only used for json deserialization)
    /// </summary>
    public D3DTXV9() { }

    #region Implement

    public uint Size => throw new NotImplementedException();

    public long Position => throw new NotImplementedException();

    public void Parse(BinaryReader reader)
    {
        throw new NotImplementedException();
    }

    public void Parse(XmlElement reader, string name)
    {
        throw new NotImplementedException();
    }

    public void ParsePixelData(BinaryReader reader, long size)
    {
        throw new NotImplementedException();
    }

    public void ParsePixelData(XmlElement element)
    {
        throw new NotImplementedException();
    }

    public void Write(BinaryWriter writer)
    {
        throw new NotImplementedException();
    }

    public void Write(XmlElement element, string name)
    {
        throw new NotImplementedException();
    }

    public void WritePixelData(BinaryWriter writer, long size)
    {
        throw new NotImplementedException();
    }

    public void WritePixelData(XmlElement element)
    {
        throw new NotImplementedException();
    }

    #endregion
}
