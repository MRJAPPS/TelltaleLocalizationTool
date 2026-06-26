using BCnEncoder.Decoder;
using BCnEncoder.Shared.ImageFiles;
using PVRTexLib;
using System.Runtime.InteropServices;
using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;

namespace TelltaleLocalizationTool.Helper;

public static class Extensions
{
    public static XmlElement GetSingleElement(this XmlElement element, string tagName, string? name = null)
    {
        string xpath = name is null
            ? $"./{tagName}"
            : $"./{tagName}[@name='{name}']";

        XmlNodeList? nodes = element.SelectNodes(xpath);

        if (nodes is null || nodes.Count == 0)
            throw new InvalidDataException($"I can't find <{tagName}> in xml!");

        if (nodes.Count > 1)
            throw new InvalidDataException($"I found more than one <{tagName}> in xml!");

        return (XmlElement)(nodes[0] ?? throw new NullReferenceException("WTF!?"));
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
                    return true;
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

    public static bool IsCubemap(this ID3DTX d3dtx) => d3dtx.mTextureLayout is T3TextureLayout.TextureCubemap or T3TextureLayout.TextureCubemapArray;

    public static bool IsVolumemap(this ID3DTX d3dtx) => d3dtx.mTextureLayout == T3TextureLayout.Texture3D;

    public static bool IsLegacyD3DTX(this ID3DTX d3dtx) => false; //We focus on newer games...

    public static byte[] GetReversedMipPixelData(this ID3DTX d3dtx)
    {
        List<byte[]> finalArray = [];

        RegionStreamHeader[] regionHeaders = [.. d3dtx.mStreamHeader];

        for (int i = 0; i < d3dtx.mTextureInfo.mNumMipLevels; i++)
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

        if (d3dtx.mStreamHeader.Count == 1)
        {
            index = d3dtx.mStreamHeader[0].mMipIndex;
        }

        return d3dtx.GetPixelDataByMipmapIndex(index, surfaceFormat, width, height, platformType);
    }

    public static byte[] GetPixelDataByMipmapIndex(this ID3DTX d3dtx, int mipmapIndex, T3SurfaceFormat surfaceFormat, int width, int height, T3PlatformType platformType)
    {
        List<byte[]> newPixelData = [];

        RegionStreamHeader[] regionHeaders = [.. d3dtx.mStreamHeader];

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

    public static byte[] GetPixelDataByFaceIndex(this ID3DTX d3dtx, int faceIndex, T3SurfaceFormat surfaceFormat, int width, int height, T3PlatformType platformType)
    {
        List<byte[]> newPixelData = [];

        RegionStreamHeader[] regionHeaders = [.. d3dtx.mStreamHeader];

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

        return newPixelData.SelectMany(b => b).ToArray();
    }

    public static void WriteD3DTXXML(this ID3DTX d3dtx, string fileName)
    {
        XmlDocument doc = new();
        XmlElement element = doc.CreateElement("Root");
        doc.AppendChild(element);
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
        PVRTexLibPixelFormat format = d3dtx.mSurfaceFormat.GetPVRFormat();
        ulong RGBA8888 = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 8, 8, 8, 8);
        uint width = d3dtx.mTextureInfo.mWidth;
        uint height = d3dtx.mTextureInfo.mHeight;
        uint depth = d3dtx.mTextureInfo.mDepth;
        uint numMipMaps = d3dtx.mTextureInfo.mNumMipLevels;
        uint numArrayMembers = d3dtx.mTextureInfo.mArraySize;
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

                PVRTexLibColourSpace colorSpace = d3dtx.mSurfaceGamma == T3SurfaceGamma.sRGB ? PVRTexLibColourSpace.sRGB : PVRTexLibColourSpace.Linear;
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

    public static void ResetForWD3NF_Font(this D3DTXV9 d3dtx)
    {
        throw new NotImplementedException($"{nameof(Extensions)}.{nameof(ResetForWD3NF_Font)}");
    }

}
