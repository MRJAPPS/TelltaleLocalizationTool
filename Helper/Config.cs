using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;

namespace TelltaleLocalizationTool.Helper;

public class Config
{
    private const string appsettingsDefault = "appsettings.json";
    public const string GithubPage = "https://github.com/MRJAPPS/TelltaleLocalizationTool";
    private readonly string appsettingsJsonFileName = appsettingsDefault;
    private readonly IConfiguration config;

    #region Common constants
    public const uint Msv6Magic = 0x4d535636;
    #endregion

    public Config(string appsettingsJsonFileName, IConfiguration config)
    {
        this.appsettingsJsonFileName = appsettingsJsonFileName;
        this.config = config;
    }

    public Config(string appsettingsJsonFileName)
    {
        this.appsettingsJsonFileName = appsettingsJsonFileName;
        config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(appsettingsJsonFileName, optional: false, reloadOnChange: true)
            .Build();
    }

    public Config() : this(appsettingsDefault) { }

    #region Props

    public string CompressonatorPath
    {
        get
        {
            string ret = config["CompressonatorPath"];
            return !File.Exists(ret)
                ? throw new FileNotFoundException("Set a valid CompressonatorPath in appsettings.json! Keep in mind, this app can't do a thing without compressonatorcli version 4.5.52!")
                : ret;
        }
    }

    public string BmfontPath64
    {
        get
        {
            string ret = config["BmfontPath64"];
            return !File.Exists(ret)
                ? throw new FileNotFoundException("Set a valid BmfontPath64 in appsettings.json! We need bmfont64.exe to convert TTF fonts.")
                : ret;
        }
    }

    // D3DTX default values (Font settings) with sarcastic validation
    public T3SamplerStateValue mSamplerState => (T3SamplerStateValue)GetRequired<int>("mSamplerState", "'mSamplerState' seems to have evaporated. Maybe add it to the JSON? Just a thought.");
    public T3PlatformType mPlatform => (T3PlatformType)GetRequired<int>("mPlatform", "'mPlatform' is nowhere to be found. Did you think I'd guess?");
    public float mImportScale => GetRequired<float>("mImportScale", "No 'mImportScale'? The universe is scale-less now. Brilliant.");
    public bool mToolProps => GetRequired<bool>("mToolProps", "'mToolProps' is absent. I'll just assume everything is false... forever.");
    public uint mNumMipLevels => GetRequired<uint>("mNumMipLevels", "'mNumMipLevels' is missing. Flat textures only? How retro.");
    public uint mDepth => GetRequired<uint>("mDepth", "'mDepth' went missing. Zero-depth texture — a true minimalist approach.");
    public uint mArraySize => GetRequired<uint>("mArraySize", "'mArraySize' is gone. Array of nothing, my favorite.");
    public T3TextureLayout mTextureLayout => (T3TextureLayout)GetRequired<int>("mTextureLayout", "'mTextureLayout' is out to lunch. Maybe put it back?");

    public T3SurfaceGamma mSurfaceGamma => (T3SurfaceGamma)GetRequired<int>("mSurfaceGamma", "'mSurfaceGamma' not found. why?");

    public T3SurfaceMultisample mSurfaceMultisample => (T3SurfaceMultisample)GetRequired<int>("mSurfaceMultisample", "'mSurfaceMultisample' left the building. Smooth edges are overrated anyway.");
    public T3ResourceUsage mResourceUsage => (T3ResourceUsage)GetRequired<int>("mResourceUsage", "'mResourceUsage' is undefined. Go ahead, waste all the GPU memory.");
    public T3TextureType mType => (T3TextureType)GetRequired<int>("mType", "'mType' is AWOL. Type? What's that?");
    public uint mSwizzle => GetRequired<uint>("mSwizzle", "'mSwizzle' didn't make it. Channels will remain in order, if that even works.");
    public float mSpecularGlossExponent => GetRequired<float>("mSpecularGlossExponent", "'mSpecularGlossExponent' is absent. Zero shine for you.");
    public float mHDRLightmapScale => GetRequired<float>("mHDRLightmapScale", "'mHDRLightmapScale' is missing. Dark times ahead.");
    public float mToonGradientCutoff => GetRequired<float>("mToonGradientCutoff", "'mToonGradientCutoff' vanished. Toon style? Never heard of her.");
    public T3TextureAlphaMode mAlphaMode => (T3TextureAlphaMode)GetRequired<int>("mAlphaMode", "'mAlphaMode' is not there. Let's just pretend transparency doesn't exist.");
    public T3TextureColor mColorMode => (T3TextureColor)GetRequired<int>("mColorMode", "'mColorMode' took a vacation. Monochrome, it is.");

    public float[] mUVOffset
    {
        get
        {
            var values = GetRequiredFloatArray("mUVOffset", 2,
                "'mUVOffset' is missing or incomplete. Yes, even offsets need two numbers. Don't be stingy.");
            return values;
        }
    }

    public float[] mUVScale
    {
        get
        {
            var values = GetRequiredFloatArray("mUVScale", 2,
                "'mUVScale' needs exactly two floats. You gave me less. I'm not a mind reader.");
            return values;
        }
    }

    #endregion

    #region Validation helpers

    private T GetRequired<T>(string key, string errorMessage)
    {
        if (!config.GetSection(key).Exists())
            throw new InvalidOperationException(errorMessage);
        return config.GetValue<T>(key);
    }

    private float[] GetRequiredFloatArray(string key, int minLength, string errorMessage)
    {
        var section = config.GetSection(key);
        if (!section.Exists())
            throw new InvalidOperationException(errorMessage);

        var children = section.GetChildren().ToList();
        if (children.Count < minLength)
            throw new InvalidOperationException(
                $"Array '{key}' is feeling lonely with only {children.Count} element(s). I need at least {minLength}. Did you think I wouldn't notice?");

        return children.Select(c => float.Parse(c.Value!)).ToArray();
    }

    #endregion
}
