using BCnEncoder.Decoder;
using BCnEncoder.Shared.ImageFiles;
using PVRTexLib;
using System.Runtime.InteropServices;
using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Convert;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.FileTypes.Font.Texture;
using TelltaleLocalizationTool.Telltale.Graphics.DDS;
using TelltaleLocalizationTool.Telltale.Graphics.Enums;

namespace TelltaleLocalizationTool.Helper;

public static class Extensions
{
    public static XmlElement GetSingleElement(this XmlElement element, string tagName, string? name = null)
    {
        string xpath = name is null
            ? $"./{tagName}"
            : $"./{tagName}[@name='{name}']";

        XmlNodeList? nodes = element.SelectNodes(xpath);

        return nodes is null || nodes.Count == 0
            ? throw new InvalidDataException($"I can't find <{tagName}> in xml!")
            : nodes.Count > 1
            ? throw new InvalidDataException($"I found more than one <{tagName}> in xml!")
            : (XmlElement)(nodes[0] ?? throw new NullReferenceException("WTF!?"));
    }

    public static byte[] DecodeAsPvr(this byte[] data)
    {
        BcDecoder decoder = new();

        DdsFile file = DdsFile.Load(new MemoryStream(data));
        BCnEncoder.Shared.ColorRgba32[][] images = decoder.DecodeAllMipMaps(file);

        List<byte> ddsData = [];

        foreach (BCnEncoder.Shared.ColorRgba32[]? image in images)
        {

            foreach (BCnEncoder.Shared.ColorRgba32 pixel in image)
            {
                ddsData.Add(pixel.r);
                ddsData.Add(pixel.g);
                ddsData.Add(pixel.b);
                ddsData.Add(pixel.a);
            }

        }
        return [.. ddsData];
    }

    public static bool HasDDSHeader(this ID3DTX d3dtx)
    {
        const uint DDSMagicLittleEndian = 0x20534444;

        foreach (byte[] region in d3dtx.mPixelData)
        {
            if (region.Length >= 4)
            {
                uint magic = BitConverter.ToUInt32(region, 0);

                if (magic == DDSMagicLittleEndian)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsFormatIncompatibleWithDDS(this T3SurfaceFormat format)
    {
        return format switch
        {
            T3SurfaceFormat.ATC_RGB => true,
            T3SurfaceFormat.ATC_RGBA => true,
            T3SurfaceFormat.ATC_RGB1A => true,
            T3SurfaceFormat.ETC1_RGB => true,
            T3SurfaceFormat.ETC2_RGB => true,
            T3SurfaceFormat.ETC2_RGBA => true,
            T3SurfaceFormat.ETC2_RGB1A => true,
            T3SurfaceFormat.ETC2_R => true,
            T3SurfaceFormat.ETC2_RG => true,
            T3SurfaceFormat.ASTC_RGBA_4x4 => true,
            T3SurfaceFormat.PVRTC2 => true,
            T3SurfaceFormat.PVRTC4 => true,
            T3SurfaceFormat.PVRTC2a => true,
            T3SurfaceFormat.PVRTC4a => true,
            _ => false,
        };
    }

    public static bool IsPlatformIncompatibleWithDDS(this T3PlatformType type)
    {
        return type switch
        {
            T3PlatformType.ePlatform_PS3 => true,
            T3PlatformType.ePlatform_PS4 => true,
            T3PlatformType.ePlatform_WiiU => true,
            T3PlatformType.ePlatform_Wii => true,
            T3PlatformType.ePlatform_Xbox => true,
            T3PlatformType.ePlatform_XBOne => true,
            T3PlatformType.ePlatform_NX => true,
            T3PlatformType.ePlatform_Vita => true,
            _ => false
        };
    }

    public static PVRTexLibPixelFormat GetPVRFormat(this T3SurfaceFormat format)
    {
        return format switch
        {
            T3SurfaceFormat.ETC1_RGB => PVRTexLibPixelFormat.ETC1,
            T3SurfaceFormat.ETC2_RGB => PVRTexLibPixelFormat.ETC2_RGB,
            T3SurfaceFormat.ETC2_RGBA => PVRTexLibPixelFormat.ETC2_RGBA,
            T3SurfaceFormat.ETC2_RGB1A => PVRTexLibPixelFormat.ETC2_RGB_A1,
            T3SurfaceFormat.ETC2_R => PVRTexLibPixelFormat.EAC_R11,
            T3SurfaceFormat.ETC2_RG => PVRTexLibPixelFormat.EAC_RG11,
            T3SurfaceFormat.ASTC_RGBA_4x4 => PVRTexLibPixelFormat.ASTC_4x4,
            T3SurfaceFormat.PVRTC2 => PVRTexLibPixelFormat.PVRTCI_2bpp_RGB,
            T3SurfaceFormat.PVRTC4 => PVRTexLibPixelFormat.PVRTCI_4bpp_RGB,
            T3SurfaceFormat.PVRTC2a => PVRTexLibPixelFormat.PVRTCI_2bpp_RGBA,
            T3SurfaceFormat.PVRTC4a => PVRTexLibPixelFormat.PVRTCI_4bpp_RGBA,
            _ => 0
        };
    }

    public static bool IsCubemap(this ID3DTX d3dtx)
    {
        return d3dtx.mD3dtxMetaData.mTextureLayout is T3TextureLayout.TextureCubemap or T3TextureLayout.TextureCubemapArray;
    }

    public static bool IsVolumemap(this ID3DTX d3dtx)
    {
        return d3dtx.mD3dtxMetaData.mTextureLayout == T3TextureLayout.Texture3D;
    }

    public static bool IsLegacyD3DTX(this ID3DTX _)
    {
        return false; //We focus on newer games...
    }

    public static byte[] GetReversedMipPixelData(this ID3DTX d3dtx)
    {
        D3DTXMetadata metadata = d3dtx.mD3dtxMetaData;
        List<byte[]> finalArray = [];

        RegionStreamHeader[] regionHeaders = [.. metadata.mStreamHeader];

        for (int i = 0; i < metadata.mTextureInfo.mNumMipLevels; i++)
        {
            for (int j = 0; j < regionHeaders.Length; j++)
            {
                if (regionHeaders[j].mMipIndex == i)
                {
                    finalArray.Add(d3dtx.mPixelData[j]);
                }
            }
        }

        return [.. finalArray.SelectMany(b => b)];
    }

    public static byte[] GetPixelDataByFirstMipmapIndex(this ID3DTX d3dtx, T3SurfaceFormat surfaceFormat, int width, int height, T3PlatformType platformType)
    {
        int index = 0;
        D3DTXMetadata metadata = d3dtx.mD3dtxMetaData;
        if (metadata.mStreamHeader.Count == 1)
        {
            index = metadata.mStreamHeader[0].mMipIndex;
        }

        return d3dtx.GetPixelDataByMipmapIndex(index, surfaceFormat, width, height, platformType);
    }

    public static byte[] GetPixelDataByMipmapIndex(this ID3DTX d3dtx, int mipmapIndex, T3SurfaceFormat surfaceFormat, int width, int height, T3PlatformType platformType)
    {
        D3DTXMetadata metadata = d3dtx.mD3dtxMetaData;
        List<byte[]> newPixelData = [];

        RegionStreamHeader[] regionHeaders = [.. metadata.mStreamHeader];

        for (int i = 0; i < regionHeaders.Length; i++)
        {
            if (regionHeaders[i].mMipIndex == mipmapIndex)
            {
                if (regionHeaders[i].mMipCount > 1)
                {
                    newPixelData.Add(d3dtx.mPixelData[i]); continue;
                }

                // GetPixelData()[i] = DecodePixelDataByPlatform(GetPixelData()[i], surfaceFormat, width, height, platformType);

                newPixelData.Add(d3dtx.mPixelData[i]);
            }
        }

        return [.. newPixelData.SelectMany(b => b)];
    }

    public static byte[] GetPixelDataByFaceIndex(this ID3DTX d3dtx, int faceIndex)
    {
        D3DTXMetadata metadata = d3dtx.mD3dtxMetaData;
        List<byte[]> newPixelData = [];

        RegionStreamHeader[] regionHeaders = [.. metadata.mStreamHeader];

        int divideBy = 1;

        for (int i = 0; i < regionHeaders.Length; i++)
        {
            if (regionHeaders[i].mFaceIndex == faceIndex)
            {
                if (regionHeaders[i].mMipCount > 1)
                {
                    newPixelData.Add(d3dtx.mPixelData[i]); continue;
                }

                //  GetPixelData()[i] = DecodePixelDataByPlatform(GetPixelData()[i], surfaceFormat, width / divideBy, height / divideBy, platformType);

                divideBy *= 2;

                newPixelData.Add(d3dtx.mPixelData[i]);
            }
        }

        // Reverse the elements in the list to get the correct order.
        newPixelData.Reverse();

        return [.. newPixelData.SelectMany(b => b)];
    }

    public static void WriteD3DTXXML(this ID3DTX d3dtx, string fileName)
    {
        XmlDocument doc = new();
        XmlElement element = doc.CreateElement("Root");
        _ = doc.AppendChild(element);
        d3dtx.Write(element, nameof(d3dtx));
        doc.Save(fileName);
    }

    public static void LoadD3DTXXML(this ID3DTX d3dtx, string fileName)
    {
        XmlDocument doc = new();
        doc.Load(fileName);
        d3dtx.Parse(doc.DocumentElement ?? throw new InvalidDataException("Where is your Xml element?(Root)"), nameof(d3dtx));
    }

    public static void LoadD3DTX(this ID3DTX d3dtx, string fileName)
    {
        using FileStream fileStream = new(fileName, FileMode.Open, FileAccess.Read);
        using BinaryReader reader = new(fileStream);
        d3dtx.Parse(reader);
    }

    public static void WriteD3DTX(this ID3DTX d3dtx, string fileName)
    {
        using FileStream fileStream = new(fileName, FileMode.Create, FileAccess.Write);
        using BinaryWriter writer = new(fileStream);
        d3dtx.Write(writer);
    }

    public static byte[] DecodeTexturePvr(this ID3DTX d3dtx, byte[] data)
    {
        D3DTXMetadata metadata = d3dtx.mD3dtxMetaData;
        PVRTexLibPixelFormat format = metadata.mSurfaceFormat.GetPVRFormat();
        ulong RGBA8888 = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 8, 8, 8, 8);
        uint width = metadata.mTextureInfo.mWidth;
        uint height = metadata.mTextureInfo.mHeight;
        uint depth = metadata.mTextureInfo.mDepth;
        uint numMipMaps = metadata.mTextureInfo.mNumMipLevels;
        uint numArrayMembers = metadata.mTextureInfo.mArraySize;
        uint numFaces = d3dtx.IsCubemap() ? 6U : 1U;
        using PVRTextureHeader textureHeader = new((ulong)format, width, height, depth, numMipMaps, numArrayMembers, numFaces);
        ulong textureSize = textureHeader.GetTextureDataSize();

        if (textureSize == 0)
        {
            throw new Exception("Could not create PVR header!");
        }

        unsafe
        {
            fixed (byte* ptr = &data[0])
            {
                using PVRTexture texture = new(textureHeader, ptr);

                PVRTexLibColourSpace colorSpace = metadata.mSurfaceGamma == T3SurfaceGamma.sRGB ? PVRTexLibColourSpace.sRGB : PVRTexLibColourSpace.Linear;
                _ = texture.Transcode(RGBA8888, PVRTexLibVariableType.UnsignedByteNorm, colorSpace);

                void* ddsPtr = null;
                try
                {
                    ddsPtr = texture.SaveTextureToMemory(PVRTexLibFileContainerType.DDS, out ulong size);

                    byte[] ddsData = new byte[size];

                    Marshal.Copy(new nint(ddsPtr), ddsData, 0, (int)size);

                    return ddsData;
                }
                finally
                {
                    texture.Dispose();
                }

            }
        }
    }


    public static void ResetForWD3NF_Font(this D3DTXV9 d3dtx, string fontName)
    {
        D3DTXMetadata metadata = d3dtx.mD3dtxMetaData;
        Config config = Program.Config;
        //========= Metadata =========
        metadata.mVersion = 9;
        metadata.mSamplerState.mData = config.mSamplerState;
        metadata.mPlatform.EnumItem = config.mPlatform;
        metadata.mName = fontName;
        metadata.mImportName = Config.GithubPage;
        metadata.mImportScale = config.mImportScale;
        metadata.mToolProps.mbHasProps = config.mToolProps;
        metadata.mSurfaceFormat = T3SurfaceFormat.ETC2_RGBA;
        metadata.mSurfaceFormat = config.mSurfaceFormat;
        metadata.mTextureLayout = config.mTextureLayout;
        metadata.mSurfaceGamma = config.mSurfaceGamma;
        metadata.mSurfaceMultisample = config.mSurfaceMultisample;
        metadata.mType = config.mType;
        metadata.mSwizzle.mSwizzle = config.mSwizzle;
        metadata.mSpecularGlossExponent = config.mSpecularGlossExponent;
        metadata.mHDRLightmapScale = config.mHDRLightmapScale;
        metadata.mToonGradientCutoff = config.mToonGradientCutoff;
        metadata.mAlphaMode = config.mAlphaMode;
        metadata.mColorMode = config.mColorMode;
        metadata.mUVOffset = new() { mX = config.mUVOffset[0], mY = config.mUVOffset[1] };
        metadata.mUVScale = new() { mX = config.mUVScale[0], mY = config.mUVScale[1] };
        //========= D3dtx =========
        d3dtx.ReadMsv6Header = false;
    }

    public static void ExportTextures(this FontD3dtxV9Collection d3dtxList, string fontName, string outputDirectory, TextureType toTextureType = TextureType.DDS)
    {
        int counter = 0;
        _ = Directory.CreateDirectory(outputDirectory);
        foreach (D3DTXV9 d3dtxv9 in d3dtxList)
        {
            bool autoPixelData = d3dtxv9.AutoPixelData;
            bool writeHeader = d3dtxv9.WriteMsv6Header;
            //I know I could add an in-memory method to the Converter class for converting D3dtx to any format, but I prefer using it this way...
            string outputName = $"{fontName}{FontTexName.Separator}{d3dtxv9.mD3dtxMetaData.mName}{FontTexName.Separator}{counter}{Converter.GetExtensionFromTextureType(toTextureType)}";
            string outputD3dtxName = $"{d3dtxv9.mD3dtxMetaData.mName}{Converter.GetExtensionFromTextureType(TextureType.D3DTX)}";

            string finalOutput = Path.Combine(outputDirectory, outputName);
            string tempD3dtxPath = Path.Combine(outputDirectory, outputD3dtxName);

            using TempFile tempD3dtx = new(outputD3dtxName);
            using FileStream tempFs = new(tempD3dtx.Path, FileMode.Create, FileAccess.Write);
            using BinaryWriter d3dtxTempW = new(tempFs);

            d3dtxv9.AutoPixelData = true;
            d3dtxv9.WriteMsv6Header = true;
            d3dtxv9.Write(d3dtxTempW);
            d3dtxv9.WriteMsv6Header = false;
            d3dtxv9.AutoPixelData = autoPixelData;

            d3dtxTempW.Close();
            Converter.ConvertTextureFromD3DtxToOthersV9(tempD3dtx.Path, finalOutput, new());
            counter++;
        }

    }


    public static void ImportTextures(this FontD3dtxV9Collection d3dtxList, string fontName, string inputDirectory, TextureType fromTextureType = TextureType.DDS)
    {
        IEnumerable<FontTexName> fontTexNames = FontTexName.GetSortedTexFiles(inputDirectory, $"{fontName}*{FontTexName.Separator}*{Converter.GetExtensionFromTextureType(fromTextureType)}");
        foreach (FontTexName fontTexName in fontTexNames)
        {
            //Convert input to d3dtx
            //We need to convert pixel data to rgba... for this version, we need just rgbamap
            throw new NotImplementedException();
        }
    }

    public static void Load_DDS_HEADER(this DDS_HEADER ddsHeader, string inputFile)
    {
        if (!File.Exists(inputFile))
        {
            throw new FileNotFoundException("Please provide a file with a DDS magic that actually exists!", inputFile);
        }
        using FileStream inputFs = new(inputFile, FileMode.Open, FileAccess.Read);
        using BinaryReader reader = new(inputFs);
        ddsHeader.Parse(reader);
    }

    public static byte[] Get_DDS_DATA(this DDS_HEADER ddsHeader, string inputFile)
    {
        if (!File.Exists(inputFile))
        {
            throw new FileNotFoundException("Please provide a file with a DDS magic that actually exists!", inputFile);
        }
        using FileStream inputFs = new(inputFile, FileMode.Open, FileAccess.Read);
        using BinaryReader reader = new(inputFs);
        ddsHeader.Parse(reader);
        return reader.ReadBytes((int)(inputFs.Length - inputFs.Position));
    }
}
