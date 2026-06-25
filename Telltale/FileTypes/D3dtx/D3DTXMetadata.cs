using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx;

/// <summary>
/// Represents the metadata of a D3DTX file. This data is later used in creating texture files.
/// </summary>
public partial class D3DTXMetadata
{
    public string TextureName { get; set; } = string.Empty;

    public uint Width { get; set; }

    public uint Height { get; set; } = 1;

    public uint Depth { get; set; } = 1;

    public uint ArraySize { get; set; } = 1;

    public uint MipLevels { get; set; } = 1;

    public T3SurfaceFormat Format { get; set; } = T3SurfaceFormat.Unknown;

    public T3SurfaceGamma SurfaceGamma { get; set; } = T3SurfaceGamma.Unknown;

    public T3TextureLayout Dimension { get; set; } = T3TextureLayout.Unknown;

    public T3TextureAlphaMode AlphaMode { get; set; } = T3TextureAlphaMode.Unknown;

    public T3PlatformType Platform { get; set; } = T3PlatformType.ePlatform_None;

    public T3TextureType TextureType { get; set; } = T3TextureType.eTxUnknown;

    public RegionStreamHeader[] RegionHeaders { get; set; } = []; // Only used for D3DTX v3-9

    public LegacyFormat D3DFormat { get; set; } = LegacyFormat.UNKNOWN;

    public bool IsCubemap()
    {
        return Dimension is T3TextureLayout.TextureCubemap or T3TextureLayout.TextureCubemapArray;
    }

    public bool IsVolumemap()
    {
        return Dimension == T3TextureLayout.Texture3D;
    }

    public bool IsArrayTexture()
    {
        return Dimension is T3TextureLayout.Texture2DArray or T3TextureLayout.TextureCubemapArray;
    }

    public bool IsLegacyD3DTX()
    {
        return D3DFormat == LegacyFormat.UNKNOWN;
    }
}