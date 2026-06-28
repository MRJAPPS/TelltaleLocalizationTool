using Hexa.NET.DirectXTex;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;

namespace TelltaleLocalizationTool.Telltale.Graphics.DDS;

public unsafe class DDS_Master
{
    public byte[] header = [];
    public byte[] pixelData = [];

    /// <summary>
    /// Create a DDS file from a D3DTX.
    /// </summary>
    /// <param name="d3dtx">The D3DTX data that will be used.</param>
    public DDS_Master(ID3DTX d3dtx)
    {
        if (d3dtx.HasDDSHeader())
        {
            return;
        }

        if (d3dtx.mD3dtxMetaData.mSurfaceFormat.IsFormatIncompatibleWithDDS())
        {
            if (d3dtx.mD3dtxMetaData.mSurfaceFormat is T3SurfaceFormat.ATC_RGB or
                T3SurfaceFormat.ATC_RGBA or
                T3SurfaceFormat.ATC_RGB1A)
            {
                InitializeUnsupportedDDSHeader(d3dtx);
                InitializeDDSPixelData(d3dtx);

                pixelData = ByteFunctions.Combine(header, pixelData).DecodeAsPvr();
                InitializeDDSHeader(d3dtx);
            }
            else
            {
                pixelData = d3dtx.mD3dtxMetaData.mSurfaceFormat == T3SurfaceFormat.CTX1
                    ? throw new Exception("This format is not supported. Please, contact the maintainer as soon as possible!")
                    : d3dtx.DecodeTexturePvr(d3dtx.GetReversedMipPixelData());
            }

            return;
        }

        InitializeDDSHeader(d3dtx);
        InitializeDDSPixelData(d3dtx);
    }

    private void InitializeDDSHeader(ID3DTX d3dtx)
    {
        ScratchImage image = DirectXTex.CreateScratchImage();

        T3SurfaceFormat surfaceFormat = d3dtx.mD3dtxMetaData.mSurfaceFormat;
        T3SurfaceGamma surfaceGamma = d3dtx.mD3dtxMetaData.mSurfaceGamma;
        T3PlatformType platformType = d3dtx.mD3dtxMetaData.mPlatform.EnumItem;
        TextureInfo textureInfo = d3dtx.mD3dtxMetaData.mTextureInfo;
        TexMetadata metadata = new()
        {
            Width = textureInfo.mWidth,
            Height = textureInfo.mHeight,
            ArraySize = d3dtx.IsCubemap() ? d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize * 6 : d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize,
            Depth = textureInfo.mDepth,
            MipLevels = textureInfo.mNumMipLevels,
            // Format = d3dtx.IsLegacyD3DTX() ? (int)DDSHelper.GetDXGIFormat(d3dtx.D3DFormat) : (int)DDSHelper.GetDXGIFormat(surfaceFormat, surfaceGamma, platformType), // We don't use legacy...
            Format = (int)DDSHelper.GetDXGIFormat(surfaceFormat, surfaceGamma, platformType),
            Dimension = d3dtx.IsVolumemap() ? TexDimension.Texture3D : TexDimension.Texture2D,
        };

        if (platformType.IsPlatformIncompatibleWithDDS())
        {
            metadata.MipLevels = 1;
        }

        _ = image.Initialize(ref metadata, CPFlags.None);

        //DDSFlags flags = D3DTX_Master.IsFormatIncompatibleWithDDS(surfaceFormat) ? DDSFlags.ForceDx9Legacy : DDSFlags.ForceDx9Legacy;
        header = TextureManager.GetDDSHeaderBytes(image);

        Console.WriteLine("Header length: " + header.Length);

        image.Release();
    }

    private void InitializeUnsupportedDDSHeader(ID3DTX d3dtx)
    {
        ScratchImage image = DirectXTex.CreateScratchImage();

        T3SurfaceFormat surfaceFormat = d3dtx.mD3dtxMetaData.mSurfaceFormat;
        T3SurfaceGamma surfaceGamma = d3dtx.mD3dtxMetaData.mSurfaceGamma;
        TextureInfo textureInfo = d3dtx.mD3dtxMetaData.mTextureInfo;
        TexMetadata metadata = new()
        {
            Width = textureInfo.mWidth,
            Height = textureInfo.mHeight,
            ArraySize = textureInfo.mArraySize,
            Depth = textureInfo.mDepth,
            MipLevels = textureInfo.mNumMipLevels,
            Format = (int)DDSHelper.GetEquivalentDXGIFormat(surfaceFormat, surfaceGamma),
            Dimension = d3dtx.IsVolumemap() ? TexDimension.Texture3D : TexDimension.Texture2D,
        };

        _ = image.Initialize(ref metadata, CPFlags.None);

        header = TextureManager.GetDDSHeaderBytes(image, DDSFlags.ForceDx9Legacy);

        Console.WriteLine("Header length: " + header.Length);

        if (d3dtx.mD3dtxMetaData.mSurfaceFormat == T3SurfaceFormat.ATC_RGB)
        {
            TextureManager.SetDDSHeaderFourCC(ref header, 'A', 'T', 'C', ' ');
        }
        else if (d3dtx.mD3dtxMetaData.mSurfaceFormat == T3SurfaceFormat.ATC_RGBA)
        {
            TextureManager.SetDDSHeaderFourCC(ref header, 'A', 'T', 'C', 'A');
        }
        else if (d3dtx.mD3dtxMetaData.mSurfaceFormat == T3SurfaceFormat.ATC_RGB1A)
        {
            TextureManager.SetDDSHeaderFourCC(ref header, 'A', 'T', 'C', 'I');
        }

        image.Release();
    }

    private void InitializeDDSPixelData(ID3DTX d3dtx)
    {
        // NOTES: Telltale mip levels are reversed in Poker Night 2 and above. The first level are the smallest mip levels and the last level is the largest mip level.
        // The faces are NOT reverse.
        // This is likely corelating with the way that KTX and KTX2 files are written.
        // Some normal maps specifically with type 4 (eTxNormalMap) channels are all reversed (ABGR instead of RGBA) (Only applies for newer games)
        // Some surface formats are dependant on platforms. For example, iOS textures have their R and B channels swapped.
        // Some surface formats are not supported by DDS. In this case, the texture will be written as a raw texture.

        if (d3dtx.IsLegacyD3DTX())
        {
            pixelData = d3dtx.mPixelData[^1];
            return;
        }

        T3SurfaceFormat surfaceFormat = d3dtx.mD3dtxMetaData.mSurfaceFormat;

        List<byte[]> textureData = [];

        if (d3dtx.mD3dtxMetaData.mPlatform.EnumItem.IsPlatformIncompatibleWithDDS())
        {
            textureData.Add(d3dtx.GetPixelDataByFirstMipmapIndex(d3dtx.mD3dtxMetaData.mSurfaceFormat, (int)d3dtx.mD3dtxMetaData.mTextureInfo.mWidth, (int)d3dtx.mD3dtxMetaData.mTextureInfo.mHeight, d3dtx.mD3dtxMetaData.mPlatform.EnumItem));
        }
        else
        {
            TextureInfo textureInfo = d3dtx.mD3dtxMetaData.mTextureInfo;
            if (d3dtx.IsVolumemap())
            {
                int divideBy = 1;
                for (int i = 0; i < textureInfo.mNumMipLevels; i++)
                {
                    textureData.Add(d3dtx.GetPixelDataByMipmapIndex(i, d3dtx.mD3dtxMetaData.mSurfaceFormat, (int)textureInfo.mWidth / divideBy, (int)textureInfo.mHeight / divideBy, d3dtx.mD3dtxMetaData.mPlatform.EnumItem));
                    divideBy *= 2;
                }
            }
            else
            {
                int totalFaces = (int)(d3dtx.IsCubemap() ? d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize * 6 : d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize);

                // Get each face of the 2D texture
                for (int i = 0; i < totalFaces; i++)
                {
                    textureData.Add(d3dtx.GetPixelDataByFaceIndex(i));
                }
            }
        }

        pixelData = textureData.SelectMany(b => b).ToArray();
    }

    /// <summary>
    /// Writes a D3DTX into a DDS file on the disk.
    /// </summary>
    /// <param name="d3dtx"></param>
    /// <param name="destinationPath"></param>
    public void WriteD3DTXAsDDS(ID3DTX d3dtx, string fileName, string destinationDirectory)
    {
        byte[] pixelData = GetData(d3dtx);
        string newDDSPath = Path.Combine(destinationDirectory, $"{fileName}.dds");
        File.WriteAllBytes(newDDSPath, pixelData);
    }

    /// <summary>
    /// Get the correct DDS data from a Telltale texture.
    /// </summary>
    /// <param name="d3dtx"></param>
    /// <returns></returns>
    public byte[] GetData(ID3DTX d3dtx)
    {
        // If the D3DTX has a DDS header, return the whole pixel data.
        if (d3dtx.HasDDSHeader())
        {
            return d3dtx.mPixelData[^1];
        }

        // Return the created DDS file.
        return ByteFunctions.Combine(header, pixelData);
    }
}
