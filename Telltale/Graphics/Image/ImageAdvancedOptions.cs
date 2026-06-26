using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.Graphics.Enums;

namespace TelltaleLocalizationTool.Telltale.Graphics.Image;


public class ImageAdvancedOptions
{
    //public TelltaleToolGame GameID { get; set; } = TelltaleToolGame.DEFAULT; //For now, we don;t use it...

    public TextureType TextureType { get; set; }

    public bool EnableMips { get; set; } = false;

    public bool AutoGenerateMips { get; set; } = false;

    public bool ManualGenerateMips { get; set; } = false;

    public uint SetMips { get; set; } = 1;

    public bool Compression { get; set; }

    public bool IsLegacyConsole { get; set; }

    public bool EnableAutomaticCompression { get; set; } = true;

    public bool IsAutomaticCompression { get; set; }

    public bool EnableTelltaleNormalMap { get; set; } = true;

    public bool IsTelltaleNormalMap { get; set; } = false;

    public bool IsTelltaleXYNormalMap { get; set; } = false;

    public bool IsSRGB { get; set; } = false;

    public T3SurfaceFormat Format { get; set; }

    public bool EnableNormalMap { get; set; }

    public bool EncodeDDSHeader { get; set; }

    public bool FilterValues { get; set; }

    public bool EnableWrapU { get; set; }

    public bool EnableWrapV { get; set; }

    public bool EnableEditing { get; set; }

    public bool EnableSwizzle { get; set; }

    public bool IsSwizzle { get; set; }

    public bool IsDeswizzle { get; set; }

    public T3PlatformType PlatformType { get; set; } = T3PlatformType.ePlatform_All;

    public bool EnableAlpha { get; set; }

    public T3TextureAlphaMode AlphaFormat { get; set; }

    public ImageEffect ImageEffect { get; set; }

    public ImageAdvancedOptions() { }

    public ImageAdvancedOptions(ImageAdvancedOptions options)
    {
        TextureType = options.TextureType;
        EnableMips = options.EnableMips;
        AutoGenerateMips = options.AutoGenerateMips;
        ManualGenerateMips = options.ManualGenerateMips;
        SetMips = options.SetMips;
        Compression = options.Compression;
        IsLegacyConsole = options.IsLegacyConsole;
        EnableAutomaticCompression = options.EnableAutomaticCompression;
        IsAutomaticCompression = options.IsAutomaticCompression;
        EnableTelltaleNormalMap = options.EnableTelltaleNormalMap;
        IsTelltaleNormalMap = options.IsTelltaleNormalMap;
        IsTelltaleXYNormalMap = options.IsTelltaleXYNormalMap;
        IsSRGB = options.IsSRGB;
        Format = options.Format;
        EnableNormalMap = options.EnableNormalMap;
        EncodeDDSHeader = options.EncodeDDSHeader;
        FilterValues = options.FilterValues;
        EnableWrapU = options.EnableWrapU;
        EnableWrapV = options.EnableWrapV;
        EnableEditing = options.EnableEditing;
        EnableSwizzle = options.EnableSwizzle;
        IsSwizzle = options.IsSwizzle;
        IsDeswizzle = options.IsDeswizzle;
        PlatformType = options.PlatformType;
        EnableAlpha = options.EnableAlpha;
        AlphaFormat = options.AlphaFormat;
        ImageEffect = options.ImageEffect;
    }
}