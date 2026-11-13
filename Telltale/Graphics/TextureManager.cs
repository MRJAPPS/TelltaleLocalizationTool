using Hexa.NET.DirectXTex;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.Graphics.Enums;
using TelltaleLocalizationTool.Telltale.Graphics.Image;
using TelltaleLocalizationTool.Telltale.Meta.Enums;

namespace TelltaleLocalizationTool.Telltale.Graphics;

public static unsafe class TextureManager
{

    public static string GetTextureDebugInfo(string filePath, TextureType textureType)
    {
        ScratchImage scratchImage = DirectXTex.CreateScratchImage();
        TexMetadata texMetadata = new();

        try
        {
            switch (textureType)
            {
                case TextureType.DDS: DirectXTex.LoadFromDDSFile(filePath, DDSFlags.None, ref texMetadata, ref scratchImage).ThrowIf(); break;
                case TextureType.PNG:
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        DirectXTex.LoadFromWICFile(filePath, WICFlags.AllFrames, ref texMetadata, ref scratchImage, default).ThrowIf(); break;
                    }
                    else if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        DirectXTex.LoadFromPNGFile(filePath, ref texMetadata, ref scratchImage).ThrowIf();
                    }
                    break;
                case TextureType.HDR: DirectXTex.LoadFromHDRFile(filePath, ref texMetadata, ref scratchImage).ThrowIf(); break;
                case TextureType.JPEG: DirectXTex.LoadFromJPEGFile(filePath, ref texMetadata, ref scratchImage).ThrowIf(); break;
                case TextureType.TGA: DirectXTex.LoadFromTGAFile2(filePath, ref texMetadata, ref scratchImage).ThrowIf(); break;
                case TextureType.TIFF: DirectXTex.LoadFromWICFile(filePath, WICFlags.AllFrames, ref texMetadata, ref scratchImage, default).ThrowIf(); break;
                case TextureType.BMP: DirectXTex.LoadFromWICFile(filePath, WICFlags.AllFrames, ref texMetadata, ref scratchImage, default).ThrowIf(); break;
                default: break;
            }
        }
        catch
        {
            scratchImage.Release();
            throw;
        }

        string debugInfo = GetTextureDebugInfo(scratchImage.GetMetadata());

        scratchImage.Release();

        return debugInfo;
    }

    public static ImageProperties GetTextureProperties(string filePath, TexMetadata metadata)
    {
        DXGIFormat dxgiFormat = (DXGIFormat)metadata.Format;

        ImageProperties properties = new()
        {
            Name = Path.GetFileNameWithoutExtension(filePath),
            Height = metadata.Height.ToString(),
            Width = metadata.Width.ToString(),
            SurfaceFormat = dxgiFormat.ToString(),
            HasAlpha = DirectXTex.HasAlpha((int)dxgiFormat) ? "True" : "False",
            SurfaceGamma = DirectXTex.IsSRGB((int)dxgiFormat) ? "sRGB" : "Linear",
            MipMapCount = metadata.MipLevels.ToString(),
            ArraySize = metadata.ArraySize.ToString(),
            TextureLayout = metadata.Dimension.ToString(),
            AlphaMode = metadata.GetAlphaMode().ToString(),
        };

        return properties;
    }

    /// <summary>
    /// Compute the pitch given the Direct3D10/DXGI format, the width and the height.
    /// </summary>
    /// <param name="dxgiFormat">The Direct3D10/DXGI format.</param>
    /// <param name="width">The width of the DDS image.</param>
    /// <param name="height">(Optional) The height of the DDS image. If not provided, it defaults to 0.</param>
    /// <returns>The pitch.</returns>
    public static uint ComputePitch(DXGIFormat dxgiFormat, ulong width, ulong height = 1)
    {
        nuint rowPitch;
        nuint slicePitch;

        DirectXTex.ComputePitch((int)dxgiFormat, (nuint)width, (nuint)height, (ulong*)(nuint*)(ulong*)&rowPitch, (ulong*)(nuint*)(ulong*)&slicePitch, CPFlags.None).ThrowIf();
        return (uint)rowPitch;
    }

    /// <summary>
    /// Returns a byte array from a DirectXTexNet DDS image.
    /// </summary>
    /// <param name="image">The DirectXTexNet DDS image.</param>
    /// <param name="flags">(Optional) The mode in which the DirectXTexNet will load the .dds file. If not provided, it defaults to NONE.</param>
    /// <returns>The byte array containing the DDS data.</returns>
    public static byte[] GetDDSByteArray(ScratchImage image, DDSFlags flags = DDSFlags.None)
    {
        Blob blob = DirectXTex.CreateBlob();
        try
        {
            TexMetadata metadata = image.GetMetadata();
            _ = DirectXTex.SaveToDDSMemory2(image.GetImages(), image.GetImageCount(), ref metadata, flags, ref blob);
            // Create a byte array to hold the data

            byte[] ddsArray = new byte[blob.GetBufferSize()];

            // Read the data from the Blob into the byte array
            Marshal.Copy((nint)blob.GetBufferPointer(), ddsArray, 0, ddsArray.Length);
            return ddsArray;
        }
        finally
        {
            blob.Release();
        }
    }

    public static ID3DTX GetTextureInformation(TexMetadata metadata, Games game, int version = 9)
    {
        return version == 9
            ? (ID3DTX)new D3DTXV9(game)
            {
                mD3dtxMetaData = new(version)
                {

                    mTextureInfo = new()
                    {
                        mWidth = (uint)metadata.Width,
                        mHeight = (uint)metadata.Height,
                        mDepth = (uint)metadata.Depth,
                        mArraySize = (uint)metadata.ArraySize,
                        mNumMipLevels = (uint)metadata.MipLevels,
                    },
                    mSurfaceFormat = DDSHelper.GetTelltaleSurfaceFormat((DXGIFormat)metadata.Format),
                    mSurfaceGamma = DirectXTex.IsSRGB(metadata.Format) ? T3SurfaceGamma.sRGB : T3SurfaceGamma.Linear,
                    mTextureLayout = DDSHelper.GetDimensionFromDDS(metadata),
                }
            }
            : throw new NotImplementedException("We need a d3dtx9!");//TODO: We also need d3dtx v4
    }

    /// <summary>
    /// Returns a byte array List containing the pixel data from an ImageSection array.
    /// </summary>
    /// <param name="sections">The sections of the image.</param>
    /// <returns></returns>
    public static List<byte[]> GetPixelDataListFromSections(ImageSection[] sections)
    {
        List<byte[]> textureData = [];

        foreach (ImageSection imageSection in sections)
        {
            textureData.Add(imageSection.Pixels);
        }

        return textureData;
    }

    /// <summary>
    /// Returns a byte array List containing the pixel data from an ImageSection array.
    /// </summary>
    /// <param name="sections">The sections of the image.</param>
    /// <returns></returns>
    public static byte[] GetPixelDataArrayFromSections(ImageSection[] sections)
    {
        byte[] textureData = [];

        foreach (ImageSection imageSection in sections)
        {
            textureData = ByteFunctions.Combine(textureData, imageSection.Pixels);
        }

        return textureData;
    }

    /// <summary>
    /// Returns the image sections of the DDS image. Each mipmap and slice is a section on its own. 
    /// </summary>
    /// <param name="ddsImage">The DirectXTexNet DDS image.</param>
    /// <param name="flags">(Optional) The mode in which the DirectXTexNet will load the .dds file. If not provided, it defaults to NONE.</param>
    /// <returns>The DDS sections</returns>
    public static ImageSection[] GetDDSImageSections(ScratchImage ddsImage, DDSFlags flags = DDSFlags.None)
    {
        List<ImageSection> sections = [];

        if (flags == DDSFlags.ForceDx9Legacy)
        {
            sections.Add(new()
            {
                Pixels = GetDDSHeaderBytes(ddsImage)
            });
        }

        Hexa.NET.DirectXTex.Image[] images = GetImages(ddsImage);

        for (int i = 0; i < images.Length; i++)
        {
            byte[] pixels = new byte[images[i].SlicePitch];

            Marshal.Copy((nint)images[i].Pixels, pixels, 0, pixels.Length);

            sections.Add(new()
            {
                Width = (nuint)images[i].Width,
                Height = (nuint)images[i].Height,
                Format = (DXGIFormat)images[i].Format,
                SlicePitch = (nuint)images[i].SlicePitch,
                RowPitch = (nuint)images[i].RowPitch,
                Pixels = pixels
            });
        }

        for (int i = 0; i < sections.Count; i++)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                Console.WriteLine($"Image {i} - Width: {sections[i].Width}, Height: {sections[i].Height}, Format: {sections[i].Format}, SlicePitch: {sections[i].SlicePitch}, RowPitch: {sections[i].RowPitch}");
                Console.WriteLine($"Image {i} - Pixels: {sections[i].Pixels.Length}");
            }
#endif
        }

        return sections.ToArray();
    }

    public static byte[] GetDDSHeaderBytes(ScratchImage image, DDSFlags flags = DDSFlags.None)
    {
        Blob blob = DirectXTex.CreateBlob();
        TexMetadata metadata = image.GetMetadata();
        DirectXTex.SaveToDDSMemory2(image.GetImages(), image.GetImageCount(), ref metadata, flags, ref blob).ThrowIf();

        _ = blob.GetBufferPointer();

        // Extract the DDS header from the blob
        byte[] ddsHeaderCheckDX10 = new byte[148];
        Marshal.Copy((nint)blob.GetBufferPointer(), ddsHeaderCheckDX10, 0, ddsHeaderCheckDX10.Length);
        blob.Release();

        byte[] ddsHeader;
        int size = ddsHeaderCheckDX10[84] == 0x44 && ddsHeaderCheckDX10[85] == 0x58 && ddsHeaderCheckDX10[86] == 0x31 && ddsHeaderCheckDX10[87] == 0x30
            ? 148
            : 128;
        ddsHeader = new byte[size];
        Array.Copy(ddsHeaderCheckDX10, 0, ddsHeader, 0, size);

        return ddsHeader;
    }

    public static void SetDDSHeaderFourCC(ref byte[] header, char c1, char c2, char c3, char c4)
    {
        if (header == null || header.Length < 88)
        {
            throw new Exception("Invalid DDS Header length!");
        }
        header[84] = (byte)c1;
        header[85] = (byte)c2;
        header[86] = (byte)c3;
        header[87] = (byte)c4;
    }

    private static Hexa.NET.DirectXTex.Image[] GetImages(ScratchImage image)
    {
        Hexa.NET.DirectXTex.Image* pointerImages = DirectXTex.GetImages(image);

        int imageCount = (int)image.GetImageCount();

        Hexa.NET.DirectXTex.Image[] images = new Hexa.NET.DirectXTex.Image[imageCount];

        for (int i = 0; i < imageCount; i++)
        {
            images[i] = pointerImages[i];
        }

        return images;
    }

    /// <summary>
    /// Returns information about the DDS image.
    /// </summary>
    /// <param name="metadata">The metadata of the DDS image.</param>
    /// <returns>The string containing the DDS metadata information</returns>
    public static string GetTextureDebugInfo(TexMetadata metadata)
    {
        StringBuilder information = new();

        _ = information.AppendLine("||||||||||| Debug Information |||||||||||");
        _ = information.AppendLine($"Width: {metadata.Width}");
        _ = information.AppendLine($"Height: {metadata.Height}");
        _ = information.AppendLine($"Depth: {metadata.Depth}");
        _ = information.AppendLine($"Format: {Enum.GetName((DXGIFormat)metadata.Format)} " + "(" + metadata.Format + ")");
        _ = information.AppendLine($"Mips: {metadata.MipLevels}");
        _ = information.AppendLine($"Dimension: {metadata.Dimension}");
        _ = information.AppendLine($"Array Elements: {metadata.ArraySize}");
        _ = information.AppendLine($"Volumemap: {metadata.IsVolumemap()}");
        _ = information.AppendLine($"Cubemap: {metadata.IsCubemap()}");
        _ = information.AppendLine($"Alpha mode: {metadata.GetAlphaMode()}");
        _ = information.AppendLine($"Premultiplied alpha: {metadata.IsPMAlpha()}");
        _ = information.AppendLine($"Misc Flags: {metadata.MiscFlags}");
        _ = information.AppendLine($"Misc Flags2: {metadata.MiscFlags2}");

        return information.ToString();
    }

    public static unsafe void ReverseChannels(Vector4* outPixels, Vector4* inPixels, ulong width, ulong y)
    {
        for (ulong j = 0; j < width; ++j)
        {
            Vector4 value = inPixels[j];

            outPixels[j].X = value.W;
            outPixels[j].Y = value.Z;
            outPixels[j].Z = value.Y;
            outPixels[j].W = value.X;
        }
    }

    public static unsafe void RemoveZ(Vector4* outPixels, Vector4* inPixels, ulong width, ulong y)
    {
        for (ulong j = 0; j < width; ++j)
        {
            Vector2 NormalXY = new(inPixels[j].X, inPixels[j].Y);

            outPixels[j] = new Vector4(inPixels[j].X, inPixels[j].Y, 0, 0);
        }
    }

    public static unsafe void RestoreZ(Vector4* outPixels, Vector4* inPixels, ulong width, ulong y)
    {
        for (ulong j = 0; j < width; ++j)
        {
            Vector2 NormalXY = new(inPixels[j].X, inPixels[j].Y);

            NormalXY = (NormalXY * 2.0f) - Vector2.One;
            float NormalZ = (float)Math.Sqrt(Math.Clamp(1.0f - Vector2.Dot(NormalXY, NormalXY), 0, 1));

            outPixels[j] = new Vector4(inPixels[j].X, inPixels[j].Y, NormalZ, 1.0f);
        }
    }

    public static unsafe void DefaultCopy(Vector4* outPixels, Vector4* inPixels, ulong width, ulong y)
    {
        for (ulong j = 0; j < width; ++j)
        {
            outPixels[j] = inPixels[j];
        }
    }
}
