using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;

namespace TelltaleLocalizationTool.Helper;

public class Config
{
    private const string appsettingsDefault = "appsettings.json";
    private readonly string appsettingsJsonFileName = appsettingsDefault;
    private readonly IConfiguration config;

    public const string ByteAndPlay = nameof(ByteAndPlay);
    public const string GithubPage = "https://github.com/MRJAPPS/TelltaleLocalizationTool";

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

    #region Props v9 wds3nf android

    public string CompressonatorPath
    {
        get
        {
            string ret = config[nameof(CompressonatorPath)];
            return !File.Exists(ret)
                ? throw new FileNotFoundException($"Set a valid {nameof(CompressonatorPath)} in appsettings.json! Keep in mind, this app can't do a thing without compressonatorcli version 4.5.52!")
                : ret;
        }
    }

    public string BmfontPath64
    {
        get
        {
            string ret = config[nameof(BmfontPath64)];
            return !File.Exists(ret)
                ? throw new FileNotFoundException($"Set a valid {nameof(BmfontPath64)} in appsettings.json! We need bmfont64.exe to convert TTF fonts.")
                : ret;
        }
    }

    // D3DTX default values (Font settings) with sarcastic validation
    public T3SamplerStateValue mSamplerState_v9_wds3nf_android => (T3SamplerStateValue)GetRequired<int>(nameof(mSamplerState_v9_wds3nf_android), $"'{nameof(mSamplerState_v9_wds3nf_android)}' seems to have evaporated. Maybe add it to the JSON? Just a thought.");
    public T3PlatformType mPlatform_v9_wds3nf_android => (T3PlatformType)GetRequired<int>(nameof(mPlatform_v9_wds3nf_android), $"'{nameof(mPlatform_v9_wds3nf_android)}' is nowhere to be found. Did you think I'd guess?");
    public float mImportScale_v9_wds3nf_android => GetRequired<float>(nameof(mImportScale_v9_wds3nf_android), $"No '{nameof(mImportScale_v9_wds3nf_android)}'? The universe is scale-less now. Brilliant.");
    public bool mToolProps_v9_wds3nf_android => GetRequired<bool>(nameof(mToolProps_v9_wds3nf_android), $"'{nameof(mToolProps_v9_wds3nf_android)}' is absent. I'll just assume everything is false... forever.");
    public uint mNumMipLevels_v9_wds3nf_android => GetRequired<uint>(nameof(mNumMipLevels_v9_wds3nf_android), $"'{nameof(mNumMipLevels_v9_wds3nf_android)}' is missing. Flat textures only? How retro.");
    public uint mDepth_v9_wds3nf_android => GetRequired<uint>(nameof(mDepth_v9_wds3nf_android), $"'{nameof(mDepth_v9_wds3nf_android)}' went missing. Zero-depth texture — a true minimalist approach.");
    public uint mArraySize_v9_wds3nf_android => GetRequired<uint>(nameof(mArraySize_v9_wds3nf_android), $"'{nameof(mArraySize_v9_wds3nf_android)}' is gone. Array of nothing, my favorite.");
    public T3TextureLayout mTextureLayout_v9_wds3nf_android => (T3TextureLayout)GetRequired<int>(nameof(mTextureLayout_v9_wds3nf_android), $"'{nameof(mTextureLayout_v9_wds3nf_android)}' is out to lunch. Maybe put it back?");

    public T3SurfaceGamma mSurfaceGamma_v9_wds3nf_android => (T3SurfaceGamma)GetRequired<int>(nameof(mSurfaceGamma_v9_wds3nf_android), $"'{nameof(mSurfaceGamma_v9_wds3nf_android)}' not found. why?");

    public T3SurfaceMultisample mSurfaceMultisample_v9_wds3nf_android => (T3SurfaceMultisample)GetRequired<int>(nameof(mSurfaceMultisample_v9_wds3nf_android), $"'{nameof(mSurfaceMultisample_v9_wds3nf_android)}' left the building. Smooth edges are overrated anyway.");
    public T3ResourceUsage mResourceUsage_v9_wds3nf_android => (T3ResourceUsage)GetRequired<int>(nameof(mResourceUsage_v9_wds3nf_android), $"'{nameof(mResourceUsage_v9_wds3nf_android)}' is undefined. Go ahead, waste all the GPU memory.");
    public T3TextureType mType_v9_wds3nf_android => (T3TextureType)GetRequired<int>(nameof(mType_v9_wds3nf_android), $"'{nameof(mType_v9_wds3nf_android)}' is AWOL. Type? What's that?");
    public uint mSwizzle_v9_wds3nf_android => GetRequired<uint>(nameof(mSwizzle_v9_wds3nf_android), $"'{nameof(mSwizzle_v9_wds3nf_android)}' didn't make it. Channels will remain in order, if that even works.");
    public float mSpecularGlossExponent_v9_wds3nf_android => GetRequired<float>(nameof(mSpecularGlossExponent_v9_wds3nf_android), $"'{nameof(mSpecularGlossExponent_v9_wds3nf_android)}' is absent. Zero shine for you.");
    public float mHDRLightmapScale_v9_wds3nf_android => GetRequired<float>(nameof(mHDRLightmapScale_v9_wds3nf_android), $"'{nameof(mHDRLightmapScale_v9_wds3nf_android)}' is missing. Dark times ahead.");
    public float mToonGradientCutoff_v9_wds3nf_android => GetRequired<float>(nameof(mToonGradientCutoff_v9_wds3nf_android), $"'{nameof(mToonGradientCutoff_v9_wds3nf_android)}' vanished. Toon style? Never heard of her.");
    public T3TextureAlphaMode mAlphaMode_v9_wds3nf_android => (T3TextureAlphaMode)GetRequired<int>(nameof(mAlphaMode_v9_wds3nf_android), $"'{nameof(mAlphaMode_v9_wds3nf_android)}' is not there. Let's just pretend transparency doesn't exist.");
    public T3TextureColor mColorMode_v9_wds3nf_android => (T3TextureColor)GetRequired<int>(nameof(mColorMode_v9_wds3nf_android), $"'{nameof(mColorMode_v9_wds3nf_android)}' took a vacation. Monochrome, it is.");

    public float[] mUVOffset_v9_wds3nf_android
    {
        get
        {
            var values = GetRequiredFloatArray(nameof(mUVOffset_v9_wds3nf_android), 2,
                $"'{nameof(mUVOffset_v9_wds3nf_android)}' is missing or incomplete. Yes, even offsets need two numbers. Don't be stingy.");
            return values;
        }
    }

    public float[] mUVScale_v9_wds3nf_android
    {
        get
        {
            var values = GetRequiredFloatArray(nameof(mUVScale_v9_wds3nf_android), 2,
                $"'{nameof(mUVScale_v9_wds3nf_android)}' needs exactly two floats. You gave me less. I'm not a mind reader.");
            return values;
        }
    }

    #endregion

    #region Props v4 wds1 android

    public float mImportScale_v4_wds1_android => GetRequired<float>(nameof(mImportScale_v4_wds1_android), $"'{nameof(mImportScale_v4_wds1_android)}' vanished.");
    public uint mNumMipLevels_v4_wds1_android => GetRequired<uint>(nameof(mNumMipLevels_v4_wds1_android), $"'{nameof(mNumMipLevels_v4_wds1_android)}' is missing. Flat textures only? How retro.");
    public uint mDepth_v4_wds1_android => GetRequired<uint>(nameof(mDepth_v4_wds1_android), $"'{nameof(mDepth_v4_wds1_android)}' went missing. Zero-depth texture — a true minimalist approach.");
    public uint mArraySize_v4_wds1_android => GetRequired<uint>(nameof(mArraySize_v4_wds1_android), $"'{nameof(mArraySize_v4_wds1_android)}' is gone. Array of nothing, my favorite.");
    public T3ResourceUsage mResourceUsage_v4_wds1_android => (T3ResourceUsage)GetRequired<int>(nameof(mResourceUsage_v4_wds1_android), $"'{nameof(mResourceUsage_v4_wds1_android)}' is undefined.");
    public T3TextureType mType_v4_wds1_android => (T3TextureType)GetRequired<int>(nameof(mType_v4_wds1_android), $"'{nameof(mType_v4_wds1_android)}' is not here. Type? What's that?");
    public int mNormalMapFormat_v4_wds1_android => GetRequired<int>(nameof(mNormalMapFormat_v4_wds1_android), $"'{nameof(mNormalMapFormat_v4_wds1_android)}' didn't make it. Just don't look at it sideways.");
    public float mHDRLightmapScale_v4_wds1_android => GetRequired<float>(nameof(mHDRLightmapScale_v4_wds1_android), $"'{nameof(mHDRLightmapScale_v4_wds1_android)}' is missing. Dark times ahead.");
    public float mToonGradientCutoff_v4_wds1_android => GetRequired<float>(nameof(mToonGradientCutoff_v4_wds1_android), $"'{nameof(mToonGradientCutoff_v4_wds1_android)}' vanished. Toon style? Never heard of her.");
    public T3TextureColor mColorMode_v4_wds1_android => (T3TextureColor)GetRequired<int>(nameof(mColorMode_v4_wds1_android), $"'{nameof(mColorMode_v4_wds1_android)}' took a vacation..");

    public float[] mUVOffset_v4_wds1_android
    {
        get
        {
            var values = GetRequiredFloatArray(nameof(mUVOffset_v4_wds1_android), 2,
                $"'{nameof(mUVOffset_v4_wds1_android)}' is missing or incomplete. Yes, even offsets need two numbers. Don't be stingy.");
            return values;
        }
    }

    public float[] mUVScale_v4_wds1_android
    {
        get
        {
            var values = GetRequiredFloatArray(nameof(mUVScale_v4_wds1_android), 2,
                $"'{nameof(mUVScale_v4_wds1_android)}' needs exactly two floats. You gave me less. I'm not a mind reader.");
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

        return [.. children.Select(c => float.Parse(c.Value!))];
    }

    #endregion
}
