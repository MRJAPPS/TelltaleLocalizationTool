using Hexa.NET.DirectXTex;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.Graphics.Enums;
using TelltaleLocalizationTool.Telltale.Graphics.Image;
using static TelltaleLocalizationTool.Telltale.Graphics.TextureManager;

namespace TelltaleLocalizationTool.Telltale.Graphics;

public unsafe partial class Texture
{
    public TexMetadata Metadata { get; set; }
    public ImageAdvancedOptions CurrentOptions { get; set; } = new ImageAdvancedOptions();
    public TextureType TextureType { get; set; } = TextureType.Unknown;

    private ScratchImage Image { get; set; }
    private ScratchImage OriginalImage { get; set; }
    private DXGIFormat PreviewFormat { get; set; }
    private string FilePath { get; set; } = string.Empty;
    private ulong PreviewWidth { get; set; }
    private ulong PreviewHeight { get; set; }
    private uint PreviewMip { get; set; } = 0;

    public Texture() { }
    public Texture(string filePath, DDSFlags flags = DDSFlags.None)
    {
        Initialize(filePath, flags);
        FilePath = filePath;
    }

    public Texture(byte[] data, TextureType textureType = TextureType.D3DTX, DDSFlags flags = DDSFlags.None)
    {
        Initialize(data, flags);
        TextureType = textureType;
    }

    public Texture(string filePath, TextureType textureType, DDSFlags flags = DDSFlags.None)
    {
        Initialize(filePath, textureType, flags);
        FilePath = filePath;
        TextureType = textureType;
    }

    private void Initialize(byte[] ddsData, DDSFlags flags = DDSFlags.None)
    {
        InitializeSingleScratchImage(ddsData, false, flags);
        InitializeSingleScratchImage(ddsData, true, flags);
    }

    private void InitializeSingleScratchImage(byte[] ddsData, bool isCopy, DDSFlags flags = DDSFlags.None)
    {
        ScratchImage Image = DirectXTex.CreateScratchImage();

        Span<byte> src = new(ddsData);
        Blob blob = DirectXTex.CreateBlob();
        TexMetadata meta = new();

        fixed (byte* srcPtr = src)
        {
            DirectXTex.LoadFromDDSMemory(srcPtr, (nuint)src.Length, flags, ref meta, ref Image).ThrowIf();
        }

        if (isCopy)
        {
            this.Image = Image;
            Metadata = meta;
        }
        else
        {
            OriginalImage = Image;
        }

        blob.Release();
    }

    private void Initialize(string filePath, DDSFlags flags = DDSFlags.None)
    {
        ScratchImage Image = DirectXTex.CreateScratchImage();
        Blob blob = DirectXTex.CreateBlob();
        TexMetadata meta = new();

        DirectXTex.LoadFromDDSFile(filePath, flags, ref meta, ref Image).ThrowIf();

        this.Image = Image;
        Metadata = meta;

        blob.Release();
    }

    public string GetDDSDebugInfo()
    {
        return GetTextureDebugInfo(Metadata);
    }

    public byte[] GetSectionPixelData(uint mip, uint face)
    {
        uint slice = 0;

        // Swap slice and face if it's a 3D texture, because they don't have faces.
        if (Metadata.Dimension == TexDimension.Texture3D)
        {
            slice = face;
            face = 0;
        }

        Hexa.NET.DirectXTex.Image* image = DirectXTex.GetImage(Image, (nuint)(ulong)mip, (nuint)(ulong)face, (nuint)(ulong)slice);

        ScratchImage destImage = DirectXTex.CreateScratchImage();
        byte[] pixels;

        try
        {
            _ = DirectXTex.InitializeFromImage(destImage, *image, false, CPFlags.None);

            if (DirectXTex.IsCompressed(destImage.GetMetadata().Format))
            {
                ScratchImage newDestImage = DirectXTex.CreateScratchImage();

                DirectXTex.Decompress(destImage.GetImage(0, 0, 0), (int)DXGIFormat.UNKNOWN, ref newDestImage).ThrowIf();

                destImage.Release();
                destImage = newDestImage;
            }

            if (destImage.GetMetadata().Format != (uint)DXGIFormat.R8G8B8A8_UNORM)
            {
                ScratchImage newDestImage = DirectXTex.CreateScratchImage();

                if (DirectXTex.IsSRGB(destImage.GetMetadata().Format))
                {
                    DirectXTex.Convert(destImage.GetImage(0, 0, 0), (int)DXGIFormat.R8G8B8A8_UNORM, TexFilterFlags.SrgbOut, 0.5f, ref newDestImage).ThrowIf();
                }
                else
                {
                    DirectXTex.Convert(destImage.GetImage(0, 0, 0), (int)DXGIFormat.R8G8B8A8_UNORM, TexFilterFlags.Default, 0.5f, ref newDestImage).ThrowIf();
                }

                destImage.Release();
                destImage = newDestImage;
            }

            PreviewFormat = (DXGIFormat)destImage.GetMetadata().Format;
            PreviewWidth = destImage.GetMetadata().Width;
            PreviewHeight = destImage.GetMetadata().Height;
            PreviewMip = mip;

            pixels = new byte[destImage.GetImage(0, 0, 0)->SlicePitch];

            Marshal.Copy((nint)destImage.GetImage(0, 0, 0)->Pixels, pixels, 0, pixels.Length);
        }
        finally
        {
            destImage.Release();
        }

        return pixels;
    }

    public void GetData(uint mip, uint face, out ulong width, out ulong height, out ulong pitch, out ulong length, out byte[] pixels)
    {
        pixels = GetSectionPixelData(mip, face);
        width = PreviewWidth;
        height = PreviewHeight;
        pitch = ComputePitch(PreviewFormat, PreviewWidth, PreviewHeight);
        length = width * height * (DirectXTex.BitsPerPixel((int)PreviewFormat) / 8);
    }

    public void GetBounds(out uint mip, out uint face)
    {
        mip = (uint)Image.GetMetadata().MipLevels - 1;

        if (Image.GetMetadata().IsVolumemap())
        {
            uint mipDiff = (uint)(Image.GetMetadata().MipLevels - PreviewMip);
            uint depth = (uint)Image.GetMetadata().Depth;

            while (mipDiff > 0)
            {
                depth = Math.Max(1, depth / 2);
                mipDiff--;
            }

            face = depth - 1;
        }
        else
        {
            face = (uint)Image.GetMetadata().ArraySize - 1;
        }
    }

    public void Compress(DXGIFormat format = DXGIFormat.UNKNOWN)
    {
        if (!DirectXTex.IsCompressed((int)format))
        {
            return;
        }

        ScratchImage transformedImage = DirectXTex.CreateScratchImage();

        if (!DirectXTex.IsCompressed(Image.GetMetadata().Format) && (int)format != Image.GetMetadata().Format)
        {
            TexMetadata originalMetadata = Image.GetMetadata();

            TexCompressFlags flags = TexCompressFlags.Default;

            if (DirectXTex.IsSRGB(Image.GetMetadata().Format))
            {
                flags |= TexCompressFlags.SrgbOut;
            }

            if (DirectXTex.IsSRGB((int)format))
            {
                flags |= TexCompressFlags.SrgbIn;
            }

            DirectXTex.Compress2(Image.GetImages(), Image.GetImageCount(), ref originalMetadata, (int)format, flags, 0.5f, ref transformedImage).ThrowIf();

            Image.Release();
            Image = transformedImage;
        }
        else
        {
            transformedImage.Release();
        }
    }

    public void Decompress(DXGIFormat format = DXGIFormat.UNKNOWN)
    {
        if (DirectXTex.IsCompressed((int)format))
        {
            throw new Exception("Invalid format!");
        }

        ScratchImage transformedImage = DirectXTex.CreateScratchImage();

        if (DirectXTex.IsCompressed(Image.GetMetadata().Format) && (int)format != Image.GetMetadata().Format)
        {
            TexMetadata originalMetadata = Image.GetMetadata();

            DirectXTex.Decompress2(Image.GetImages(), Image.GetImageCount(), ref originalMetadata, (int)format, ref transformedImage).ThrowIf();

            Image.Release();
            Image = transformedImage;
        }
        else
        {
            transformedImage.Release();
        }
    }

    public void TransformImage(ImageEffect conversionMode = ImageEffect.DEFAULT)
    {
        Decompress();

        TransformImageFunc transformFunction = DefaultCopy;

        if (conversionMode == ImageEffect.RESTORE_Z)
        {
            transformFunction = RestoreZ;
        }
        else if (conversionMode == ImageEffect.REMOVE_Z)
        {
            transformFunction = RemoveZ;
        }
        else if (conversionMode == ImageEffect.SWIZZLE_ABGR)
        {
            transformFunction = ReverseChannels;
        }

        ScratchImage transformedImage = DirectXTex.CreateScratchImage();
        TexMetadata texMetadata = Image.GetMetadata();

        DirectXTex.TransformImage2(Image.GetImages(), Image.GetImageCount(), ref texMetadata, transformFunction, ref transformedImage).ThrowIf();

        Image.Release();

        Image = transformedImage;
    }

    public void Initialize(string filePath, TextureType textureType, DDSFlags flags = DDSFlags.None)
    {
        TextureType = textureType;
        Initialize(filePath, textureType, false, flags);
        Initialize(filePath, textureType, true, flags);
    }

    private void Initialize(string filePath, TextureType textureType, bool isCopy = false, DDSFlags flags = DDSFlags.None)
    {
        ScratchImage scratchImage = DirectXTex.CreateScratchImage();
        TexMetadata texMetadata = new();

        try
        {
            switch (textureType)
            {
                case TextureType.DDS: DirectXTex.LoadFromDDSFile(filePath, flags, ref texMetadata, ref scratchImage).ThrowIf(); break;
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

            if (isCopy)
            {
                if (!Image.IsNull)
                {
                    Image.Release();
                }
                Image = scratchImage;
                Metadata = texMetadata;
            }
            else
            {
                if (!OriginalImage.IsNull)
                {
                    OriginalImage.Release();
                }
                OriginalImage = scratchImage;
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            scratchImage.Release();
            throw new Exception("Failed to load image!");
        }
    }

    private void ResetImageToOriginal()
    {
        if (TextureType != TextureType.D3DTX)
        {
            Initialize(FilePath, TextureType, true, DDSFlags.None);
        }
        else
        {
            byte[] memory = GetDDSByteArray(OriginalImage);
            InitializeSingleScratchImage(memory, true);
        }
    }

    /// <summary>
    /// Changes the image itself based on the options provided.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="keepOriginal"></param>
    /// <param name="convertingOnly"></param>
    public void TransformTexture(ImageAdvancedOptions options, bool keepOriginal = false, bool convertingOnly = false)
    {
        ResetImageToOriginal();

        if (options.EnableSwizzle && options.IsDeswizzle)
        {
            Deswizzle(options.PlatformType);
        }

        Decompress();

        if (options.EnableNormalMap)
        {
            GenerateNormalMap();
        }

        if (options.EnableEditing)
        {
            if (options.ImageEffect != ImageEffect.DEFAULT)
            {
                TransformImage(options.ImageEffect);
            }
        }

        if (options.EnableTelltaleNormalMap && options.IsTelltaleNormalMap)
        {
            TransformImage(ImageEffect.SWIZZLE_ABGR);
        }

        if (CurrentOptions.EnableNormalMap != options.EnableNormalMap)
        {
            if (!CurrentOptions.EnableNormalMap)
            { GenerateNormalMap(); }
        }

        if (options.EnableMips)
        {
            if (options.AutoGenerateMips)
            {
                GenerateMipMaps(0);
            }
            else if (options.ManualGenerateMips && options.SetMips > 1)
            {
                GenerateMipMaps(Math.Min(options.SetMips, GetMaxMipLevels()));
            }
        }

        if (convertingOnly)
        {
            if (options.EnableAutomaticCompression)
            {
                if (options.EnableNormalMap && options.IsTelltaleXYNormalMap)
                {
                    Compress(DXGIFormat.BC5_UNORM);
                }
                else if (OriginalImage.IsAlphaAllOpaque())
                {
                    if (options.IsSRGB)
                    {
                        Compress(DXGIFormat.BC1_UNORM_SRGB);
                    }
                    else
                    {
                        Compress(DXGIFormat.BC1_UNORM);
                    }
                }
                else
                {
                    if (options.IsSRGB)
                    {
                        Compress(DXGIFormat.BC3_UNORM_SRGB);
                    }
                    else
                    {
                        Compress(DXGIFormat.BC3_UNORM);
                    }
                }
            }
        }
        else if (keepOriginal)
        {
            Compress((DXGIFormat)OriginalImage.GetMetadata().Format);
        }

        if (options.EnableSwizzle && options.IsSwizzle)
        {
            Swizzle(options.PlatformType);
        }

        CurrentOptions = new(options);
    }

    public uint GetMaxMipLevels()
    {
        uint width = (uint)Metadata.Width;
        uint height = (uint)Metadata.Height;
        uint depth = (uint)Metadata.Depth;

        uint mipLevels = 1;

        while (width > 1 || height > 1 || depth > 1)
        {
            width = Math.Max(1, width / 2);
            height = Math.Max(1, height / 2);
            depth = Math.Max(1, depth / 2);

            mipLevels++;
        }

        return mipLevels;
    }

    public void GenerateNormalMap()
    {
        if (DirectXTex.IsCompressed(Image.GetMetadata().Format))
        {
            Decompress();
        }

        ScratchImage destImage = DirectXTex.CreateScratchImage();
        TexMetadata metadata = Image.GetMetadata();

        DirectXTex.ComputeNormalMap2(Image.GetImages(), Image.GetImageCount(), ref metadata, CNMAPFlags.Default, 7, metadata.Format, ref destImage).ThrowIf();

        Image.Release();
        Image = destImage;
    }

    public void Swizzle(T3PlatformType platform = T3PlatformType.ePlatform_None)
    {
        TexMetadata metadata = Image.GetMetadata();

        ImageSection[] images = GetDDSImageSections(Image);

        foreach (ImageSection image in images)
        {
            image.Pixels = platform switch
            {
                T3PlatformType.ePlatform_NX => DrSwizzler.Swizzler.SwitchSwizzle(image.Pixels, (int)image.Width, (int)image.Height, (DrSwizzler.DDS.DXEnums.DXGIFormat)image.Format),
                T3PlatformType.ePlatform_PS3 => DrSwizzler.Swizzler.PS3Swizzle(image.Pixels, (int)image.Width, (int)image.Height, (DrSwizzler.DDS.DXEnums.DXGIFormat)image.Format),
                T3PlatformType.ePlatform_PS4 => DrSwizzler.Swizzler.PS4Swizzle(image.Pixels, (int)image.Width, (int)image.Height, (DrSwizzler.DDS.DXEnums.DXGIFormat)image.Format),
                T3PlatformType.ePlatform_Xbox => DrSwizzler.Swizzler.Xbox360Swizzle(image.Pixels, (int)image.Width, (int)image.Height, (DrSwizzler.DDS.DXEnums.DXGIFormat)image.Format),
                T3PlatformType.ePlatform_Vita => DrSwizzler.Swizzler.VitaSwizzle(image.Pixels, (int)image.Width, (int)image.Height, (DrSwizzler.DDS.DXEnums.DXGIFormat)image.Format),
                _ => image.Pixels
            };
        }

        byte[] header = GetDDSHeaderBytes(Image);
        byte[] pixels = images.SelectMany(x => x.Pixels).ToArray();

        byte[] newDDS = ByteFunctions.Combine(header, pixels);

        Image.Release();

        InitializeSingleScratchImage(newDDS, true);
    }

    public void Deswizzle(T3PlatformType platform = T3PlatformType.ePlatform_None)
    {
        TexMetadata metadata = Image.GetMetadata();

        ImageSection[] images = GetDDSImageSections(Image);

        foreach (ImageSection image in images)
        {
            image.Pixels = platform switch
            {
                T3PlatformType.ePlatform_NX => DrSwizzler.Deswizzler.SwitchDeswizzle(image.Pixels, (int)image.Width, (int)image.Height, (DrSwizzler.DDS.DXEnums.DXGIFormat)image.Format),
                T3PlatformType.ePlatform_PS3 => DrSwizzler.Deswizzler.PS3Deswizzle(image.Pixels, (int)image.Width, (int)image.Height, (DrSwizzler.DDS.DXEnums.DXGIFormat)image.Format),
                T3PlatformType.ePlatform_PS4 => DrSwizzler.Deswizzler.PS4Deswizzle(image.Pixels, (int)image.Width, (int)image.Height, (DrSwizzler.DDS.DXEnums.DXGIFormat)image.Format),
                T3PlatformType.ePlatform_Xbox => DrSwizzler.Deswizzler.Xbox360Deswizzle(image.Pixels, (int)image.Width, (int)image.Height, (DrSwizzler.DDS.DXEnums.DXGIFormat)image.Format),
                T3PlatformType.ePlatform_Vita => DrSwizzler.Deswizzler.VitaDeswizzle(image.Pixels, (int)image.Width, (int)image.Height, (DrSwizzler.DDS.DXEnums.DXGIFormat)image.Format),
                _ => image.Pixels
            };
        }

        byte[] header = GetDDSHeaderBytes(Image);
        byte[] pixels = images.SelectMany(x => x.Pixels).ToArray();

        byte[] newDDS = ByteFunctions.Combine(header, pixels);

        Image.Release();

        InitializeSingleScratchImage(newDDS, true);
    }

    public void GenerateMipMaps(uint mipLevels = 0)
    {
        Decompress();

        ScratchImage destImage = DirectXTex.CreateScratchImage();
        TexMetadata metadata = Image.GetMetadata();

        if (metadata.IsVolumemap())
        {
            DirectXTex.GenerateMipMaps3D2(Image.GetImages(), Image.GetImageCount(), TexFilterFlags.Default, (nuint)(ulong)mipLevels, ref destImage).ThrowIf();
        }
        else
        {
            DirectXTex.GenerateMipMaps2(Image.GetImages(), Image.GetImageCount(), ref metadata, TexFilterFlags.Default, (nuint)(ulong)mipLevels, ref destImage).ThrowIf();
        }
#if DEBUG
        if (Debugger.IsAttached)
            Console.WriteLine("Mipmaps generated" + destImage.GetImageCount());
#endif
        Image.Release();
        Image = destImage;
    }

    public void SaveAsPNG(string filePath)
    {
        Decompress();

        if (Image.GetMetadata().Format is not ((int)DXGIFormat.R8_UNORM) and
            not ((int)DXGIFormat.B8G8R8A8_UNORM) and
            not ((int)DXGIFormat.B8G8R8X8_UNORM) and
            not ((int)DXGIFormat.R8G8B8A8_UNORM))
        {
            Convert(DXGIFormat.R8G8B8A8_UNORM);
        }

        if (Metadata.ArraySize == 1)
        {
            if (Metadata.Depth == 1)
            {
                DirectXTex.SaveToPNGFile(Image.GetImage(0, 0, 0), filePath).ThrowIf();
            }
            else
            {
                for (uint i = 0; i < Metadata.Depth; i++)
                {
                    StringBuilder sb = new();
                    _ = sb.Append(filePath).Append('_').Append(i);
                    DirectXTex.SaveToPNGFile(Image.GetImage(0, 0, i), sb.ToString()).ThrowIf();
                }
            }
        }
        else
        {
            for (uint i = 0; i < Metadata.ArraySize; i++)
            {
                StringBuilder sb = new();
                _ = sb.Append(filePath).Append('_').Append(i);
                DirectXTex.SaveToPNGFile(Image.GetImage(0, i, 0), sb.ToString()).ThrowIf();
            }
        }
    }

    public void SaveAsJPEG(string filePath)
    {
        Decompress();

        if (Image.GetMetadata().Format is not ((int)DXGIFormat.R8_UNORM) and
            not ((int)DXGIFormat.R8G8B8A8_UNORM))
        {
            Convert(DXGIFormat.R8G8B8A8_UNORM);
        }

        if (Metadata.ArraySize == 1)
        {
            if (Metadata.Depth == 1)
            {
                DirectXTex.SaveToJPEGFile(Image.GetImage(0, 0, 0), filePath).ThrowIf();
            }
            else
            {
                for (uint i = 0; i < Metadata.Depth; i++)
                {
                    StringBuilder sb = new();
                    _ = sb.Append(filePath).Append('_').Append(i);
                    DirectXTex.SaveToJPEGFile(Image.GetImage(0, 0, i), sb.ToString()).ThrowIf();
                }
            }
        }
        else
        {
            for (uint i = 0; i < Metadata.ArraySize; i++)
            {
                StringBuilder sb = new();
                _ = sb.Append(filePath).Append('_').Append(i);
                DirectXTex.SaveToJPEGFile(Image.GetImage(0, i, 0), sb.ToString()).ThrowIf();
            }
        }
    }

    public void SaveAsTGA(string filePath)
    {
        Decompress();

        if (Image.GetMetadata().Format is not ((int)DXGIFormat.R8_UNORM) and
            not ((int)DXGIFormat.A8_UNORM) and
            not ((int)DXGIFormat.R8G8B8A8_UNORM) and
            not ((int)DXGIFormat.R8G8B8A8_UNORM_SRGB) and
            not ((int)DXGIFormat.B8G8R8A8_UNORM) and
            not ((int)DXGIFormat.B8G8R8A8_UNORM_SRGB) and
            not ((int)DXGIFormat.B8G8R8X8_UNORM) and
            not ((int)DXGIFormat.B8G8R8X8_UNORM_SRGB) and
            not ((int)DXGIFormat.B8G8R8X8_UNORM) and
            not ((int)DXGIFormat.B5G5R5A1_UNORM))
        {
            Convert(DXGIFormat.R8G8B8A8_UNORM);
        }

        TexMetadata tgaMetadata = Image.GetMetadata();

        if (Metadata.ArraySize == 1)
        {
            if (Metadata.Depth == 1)
            {
                DirectXTex.SaveToTGAFile2(Image.GetImage(0, 0, 0), filePath, ref tgaMetadata).ThrowIf();
            }
            else
            {
                for (uint i = 0; i < Metadata.Depth; i++)
                {
                    StringBuilder sb = new();
                    _ = sb.Append(filePath).Append('_').Append(i);
                    DirectXTex.SaveToTGAFile2(Image.GetImage(0, 0, i), filePath, ref tgaMetadata).ThrowIf();
                }
            }
        }
        else
        {
            for (uint i = 0; i < Metadata.ArraySize; i++)
            {
                StringBuilder sb = new();
                _ = sb.Append(filePath).Append('_').Append(i);
                DirectXTex.SaveToTGAFile2(Image.GetImage(0, i, 0), filePath, ref tgaMetadata).ThrowIf();
            }
        }
    }

    public void SaveAsHDR(string filePath)
    {
        Decompress();

        if (Image.GetMetadata().Format != (int)DXGIFormat.R32G32B32A32_FLOAT)
        {
            Convert(DXGIFormat.R32G32B32A32_FLOAT);
        }

        if (Metadata.ArraySize == 1)
        {
            if (Metadata.Depth == 1)
            {
                _ = DirectXTex.SaveToHDRFile(Image.GetImage(0, 0, 0), filePath);
            }
            else
            {
                for (uint i = 0; i < Metadata.Depth; i++)
                {
                    StringBuilder sb = new();
                    _ = sb.Append(filePath).Append('_').Append(i);
                    DirectXTex.SaveToHDRFile(Image.GetImage(0, 0, i), sb.ToString()).ThrowIf();
                }
            }
        }
        else
        {
            for (uint i = 0; i < Metadata.ArraySize; i++)
            {
                StringBuilder sb = new();
                _ = sb.Append(filePath).Append('_').Append(i);
                DirectXTex.SaveToHDRFile(Image.GetImage(0, i, 0), sb.ToString()).ThrowIf();
            }
        }
    }

    public void SaveAsWIC(string filePath)
    {
        Decompress();

        if (Image.GetMetadata().Format is ((int)DXGIFormat.R8G8_UNORM) or ((int)DXGIFormat.R8G8_SNORM))
        {
            Convert(DXGIFormat.R8G8B8A8_UNORM);
        }

        if (Metadata.ArraySize == 1)
        {
            if (Metadata.Depth == 1)
            {
                DirectXTex.SaveToWICFile(Image.GetImage(0, 0, 0), WICFlags.None, DirectXTex.GetWICCodec(WICCodecs.CodecBmp), filePath, null, default).ThrowIf();
            }
            else
            {
                for (uint i = 0; i < Metadata.Depth; i++)
                {
                    StringBuilder sb = new();
                    _ = sb.Append(filePath).Append('_').Append(i);
                    DirectXTex.SaveToWICFile(Image.GetImage(0, 0, i), WICFlags.None, DirectXTex.GetWICCodec(WICCodecs.CodecBmp), filePath, null, default).ThrowIf();
                }
            }
        }
        else
        {
            for (uint i = 0; i < Metadata.ArraySize; i++)
            {
                StringBuilder sb = new();
                _ = sb.Append(filePath).Append('_').Append(i);
                DirectXTex.SaveToWICFile(Image.GetImage(0, i, 0), WICFlags.None, DirectXTex.GetWICCodec(WICCodecs.CodecBmp), filePath, null, default).ThrowIf();
            }
        }
    }

    public void SaveAsDDS(string filePath)
    {
        TexMetadata metadata = Image.GetMetadata();
        _ = DirectXTex.SaveToDDSFile2(Image.GetImages(), Image.GetImageCount(), ref metadata, DDSFlags.None, filePath);
    }

    public void SaveTexture(string filePath, TextureType textureType = TextureType.Unknown)
    {
        switch (textureType)
        {
            case TextureType.DDS:
                filePath += ".dds";
                SaveAsDDS(filePath); break;
            case TextureType.BMP:
                filePath += ".bmp";
                SaveAsWIC(filePath); break;
            case TextureType.PNG:
                filePath += ".png";
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    SaveAsWIC(filePath);
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    SaveAsPNG(filePath);
                }
                break;
            case TextureType.JPEG:
                filePath += ".jpeg";
                SaveAsJPEG(filePath); break;
            case TextureType.HDR:
                filePath += ".hdr";
                SaveAsHDR(filePath); break;
            case TextureType.TGA:
                filePath += ".tga";
                SaveAsTGA(filePath); break;
            case TextureType.TIFF:
                filePath += ".tiff";
                SaveAsWIC(filePath); break;
            default:
                filePath += ".dds";
                SaveAsDDS(filePath); break;
        }
    }

    public void Convert(DXGIFormat format)
    {
        TexMetadata texMetadata = Image.GetMetadata();

        if ((int)format != texMetadata.Format)
        {
            ScratchImage transformedImage = DirectXTex.CreateScratchImage();

            DirectXTex.Convert2(Image.GetImages(), Image.GetImageCount(), ref texMetadata, (int)format, TexFilterFlags.Default, 0.5f, ref transformedImage).ThrowIf();

            Image.Release();
            Image = transformedImage;
        }
    }

    public void GetDDSInformationV9(out D3DTXV9 d3dtxv9, out ImageSection[] sections, DDSFlags flags = DDSFlags.None)
    {
        d3dtxv9 = (D3DTXV9)GetTextureInformation(Image.GetMetadata());
        sections = GetDDSImageSections(Image, flags);
    }

    public void Release()
    {
        if (!Image.IsNull)
        {
            Image.Release();
        }
        if (!OriginalImage.IsNull)
        {
            OriginalImage.Release();
        }
    }
}