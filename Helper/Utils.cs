using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.Meta;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using TelltaleLocalizationTool.Telltale.Meta.Versions;

namespace TelltaleLocalizationTool.Helper;
public static class Utils
{
    #region Map

    public static int GetBiggestDdsMipBytesLength(int width, int height, T3SurfaceFormat format, out int pitch, string? errorMessage = null)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException($"{nameof(width)}({width}) and {nameof(height)}({height}) must be greater than zero! thank you for your attention to this matter...");
        }

        const int blockDimension = 4;
        const int etc1BytesPerBlock = 8;
        const int etc2BytesPerBlock = 16;
        const int argb8BytesPerPixel = 4;
        const int argb4BytesPerPixel = 2;

        switch (format)
        {
            case T3SurfaceFormat.ETC1_RGB:
                {//https://registry.khronos.org/OpenGL/extensions/OES/OES_compressed_ETC1_RGB8_texture.txt
                    int blocksWide = (width + blockDimension - 1) / blockDimension;
                    int blocksHigh = (height + blockDimension - 1) / blockDimension;
                    pitch = blocksWide * etc1BytesPerBlock;
                    return pitch * blocksHigh;
                }

            case T3SurfaceFormat.ETC2_RGBA:
                {//https://registry.khronos.org/OpenGL-Refpages/es3.0/html/glCompressedTexImage2D.xhtml (for 8bitAlpha)
                    int blocksWide = (width + blockDimension - 1) / blockDimension;
                    int blocksHigh = (height + blockDimension - 1) / blockDimension;
                    pitch = blocksWide * etc2BytesPerBlock;
                    return pitch * blocksHigh;
                }
            //https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds-pguide
            //https://learn.microsoft.com/en-us/windows/win32/direct3d9/d3dformat#unsigned-formats
            case T3SurfaceFormat.ARGB8:
                {
                    pitch = width * argb8BytesPerPixel;
                    return pitch * height;
                }

            case T3SurfaceFormat.ARGB4:
                {
                    pitch = width * argb4BytesPerPixel;
                    return pitch * height;
                }

            default:
                throw new FormatException(errorMessage ?? $"GetBiggestDdsMipBytesLength error: Unsupported format '{format}'.");
        }
    }

    public static T3SurfaceFormat GetDefaultFormat(Games game)
    {
        return game switch
        {
            Games.WDS3NF => T3SurfaceFormat.ETC2_RGBA,
            Games.WDS1 => T3SurfaceFormat.ARGB8,
            _ => throw new NotImplementedException("WTF? what is this game? should I use RGBA8?? no... I don't..."),
        };
    }

    #endregion

    #region Checking

    public static void CheckSectionsV56(string errorMessage, IMetaHeader? header, long fileLen, long filePos)

    {
        if (header is MSV6 hdr6)
        {
            long sectionSize = (long)hdr6.mDefaultSectionChunkSize + hdr6.mDebugSectionChunkSize + hdr6.mAsyncSectionChunkSize;
            if (sectionSize > fileLen - filePos)
            {
                throw new EndOfStreamException(errorMessage);
            }
        }
        else if (header is MSV5 hdr5)
        {
            long sectionSize = (long)hdr5.mDefaultSectionChunkSize + hdr5.mDebugSectionChunkSize + hdr5.mAsyncSectionChunkSize;
            if (sectionSize > fileLen - filePos)
            {
                throw new EndOfStreamException(errorMessage);
            }
        }
    }


    public static void CheckHeader(Games game, IMetaHeader? header, bool required)
    {
        if (!required) return;
        switch (game)
        {
            case Games.WDS3NF:
                if (header is not MSV6)
                {
                    throw new InvalidDataException($"Bro, are you living in a parallel universe? Because in my world {nameof(Games.WDS3NF)} uses {nameof(MSV6)}! Your file isn't MSV6!");
                }

                break;
            case Games.WDS1:
                if (header is not MSV5)
                {
                    throw new InvalidDataException($"Bro, are you living in a parallel universe? Because in my world {nameof(Games.WDS1)} uses {nameof(MSV5)}! Your file isn't MSV6!");
                }

                break;
            default:
                throw new NotImplementedException($"Your input is not valid! USE {nameof(Games.WDS3NF)} or  {nameof(Games.WDS1)}");
        }
    }


    #endregion
}
