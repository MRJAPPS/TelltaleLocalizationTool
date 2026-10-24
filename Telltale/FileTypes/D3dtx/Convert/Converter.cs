using Hexa.NET.DirectXTex;
using System.Diagnostics;
using System.Drawing;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.Graphics;
using TelltaleLocalizationTool.Telltale.Graphics.DDS;
using TelltaleLocalizationTool.Telltale.Graphics.DDS.Data;
using TelltaleLocalizationTool.Telltale.Graphics.Enums;
using TelltaleLocalizationTool.Telltale.Graphics.Image;
namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Convert;


public static class Converter
{
    public static TextureType GetTextureTypeFromExtension(string fileName)
    {
        return Path.GetExtension(fileName) switch
        {
            Formats.d3dtxExtension => TextureType.D3DTX,
            Formats.ddsExtension => TextureType.DDS,
            Formats.ktxExtension => TextureType.KTX,
            Formats.ktx2Extension => TextureType.KTX2,
            Formats.pngExtension => TextureType.PNG,
            Formats.jpegExtension => TextureType.JPEG,
            Formats.bmpExtension => TextureType.BMP,
            Formats.tifExtension => TextureType.TIFF,
            Formats.tgaExtension => TextureType.TGA,
            Formats.hdrExtension => TextureType.HDR,
            _ => throw new FormatException($"I looked but couldn't find a {nameof(TextureType)} for format {Path.GetExtension(fileName)}!"),
        };
    }

    public static string GetExtensionFromTextureType(TextureType textureType)
    {
        return textureType switch
        {
            TextureType.D3DTX => Formats.d3dtxExtension,
            TextureType.DDS => Formats.ddsExtension,
            TextureType.KTX => Formats.ktxExtension,
            TextureType.KTX2 => Formats.ktx2Extension,
            TextureType.PNG => Formats.pngExtension,
            TextureType.JPEG => Formats.jpegExtension,
            TextureType.BMP => Formats.bmpExtension,
            TextureType.TIFF => Formats.tifExtension,
            TextureType.TGA => Formats.tgaExtension,
            TextureType.HDR => Formats.hdrExtension,
            _ => throw new ArgumentOutOfRangeException(nameof(textureType), textureType, $"No extension defined for {textureType}.")
        };
    }

    public static void ConvertTextureFromD3DtxToOthersV9(string sourceFilePath, string destinationFilePath, ImageAdvancedOptions options)
    {
        CheckArgs(sourceFilePath, destinationFilePath);

        D3DTXV9 d3dtxFile = new();
        d3dtxFile.LoadD3DTX(sourceFilePath);

        DDS_Master ddsFile = new(d3dtxFile);
        byte[] array = ddsFile.GetData(d3dtxFile);

        d3dtxFile.WriteD3DTXXML(Path.ChangeExtension(
            options.WriteXmlAsideOutputTex ? destinationFilePath : sourceFilePath,
            options.Used3dtxXml ? Formats.d3dtxXmlExtension : Formats.xmlExtension));

        Texture texture = new(array, TextureType.D3DTX);
        texture.TransformTexture(options, true, false);
        texture.SaveTexture(destinationFilePath, GetTextureTypeFromExtension(destinationFilePath));
        texture.Release();
    }

    public static void ConvertTextureFromOthersToD3DtxV9(string sourceFilePath, string destinationFilePath, ImageAdvancedOptions options, string? xmlFile = null)
    {
        CheckArgs(sourceFilePath, destinationFilePath);

        if (!Path.GetExtension(destinationFilePath).Equals(Formats.d3dtxExtension, StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException($"The output file format must be {Formats.d3dtxExtension}!");
        }

        D3DTXV9 d3dtxMaster = new();
        if (File.Exists(xmlFile))
        {
            d3dtxMaster.LoadD3DTXXML(xmlFile);
        }
        else
        {
            throw new FileNotFoundException("Couldn't find the D3dtx XML file to build the D3dtx.");
        }

        DDSFlags flags = d3dtxMaster.IsLegacyD3DTX() ? DDSFlags.ForceDx9Legacy : DDSFlags.None;

        Texture texture = new(sourceFilePath, GetTextureTypeFromExtension(sourceFilePath), flags);

        if (d3dtxMaster.mD3dtxMetaData.mType is T3TextureType.eTxBumpmap or T3TextureType.eTxNormalMap)
        {
            options.IsTelltaleNormalMap = true;
        }
        else if (d3dtxMaster.mD3dtxMetaData.mType is T3TextureType.eTxNormalXYMap)
        {
            options.IsTelltaleNormalMap = true;
        }

        if (d3dtxMaster.mD3dtxMetaData.mSurfaceGamma is T3SurfaceGamma.sRGB)
        {
            options.IsSRGB = true;
        }

        texture.TransformTexture(options, true, true);

        texture.GetDDSInformationV9(out D3DTXV9 d3dtx, out ImageSection[] sections, flags);

        if (options.EnableSwizzle)
        {
            d3dtx.mD3dtxMetaData.mPlatform.EnumItem = options.PlatformType;
        }

        d3dtxMaster.ModifyD3DTX(d3dtx, sections);

        texture.Release();

        d3dtxMaster.WriteD3DTX(destinationFilePath);
    }


    #region NEW

    public static D3DTXV9 ConvertDdsToD3dtx(string ddsFile, string? fontName = default)
    {
        ArgumentNullException.ThrowIfNull(ddsFile, nameof(ddsFile));
        CheckArgsExtension(ddsFile);

        DdsD3dtx ddsData;
        D3DTXV9 result = new();
        using TempFile etc2rgbaDds = new(ddsFile);

        _ = CompressWithCompressonatorCli(ddsFile, etc2rgbaDds.Path);
        ddsData = GetD3dtxDataFormDds(etc2rgbaDds.Path);
        result.MapDdsToD3dtxForFont(ddsData, fontName);
        return result;
        //local methods:
        static void CheckArgsExtension(string ddsFile)
        {
            string ddsExtension = Path.GetExtension(ddsFile);
            if (!ddsExtension.Equals(Formats.ddsExtension))
            {
                throw new NotSupportedException($"We don't support {ddsExtension} extension for input file(we need {Formats.ddsExtension})!");
            }
        }
    }

    public static void ConvertDdsToD3dtx(string ddsFile, string d3dtxFile, string? fontName = default, bool writeXml = true)
    {
        CheckArgs(ddsFile, d3dtxFile);
        CheckArgsExtension(ddsFile, d3dtxFile);

        DdsD3dtx ddsData;
        D3DTXV9 d3dtx = new();
        using TempFile etc2rgbaDds = new(ddsFile);

        _ = CompressWithCompressonatorCli(ddsFile, etc2rgbaDds.Path);
        ddsData = GetD3dtxDataFormDds(etc2rgbaDds.Path);
        d3dtx.MapDdsToD3dtxForFont(ddsData, fontName);
        d3dtx.WriteD3DTX(d3dtxFile);
        if (writeXml)
            d3dtx.WriteD3DTXXML(Path.ChangeExtension(d3dtxFile, Formats.xmlExtension));
        //local methods:
        static void CheckArgsExtension(string ddsFile, string d3dtxFile)
        {
            string ddsExtension = Path.GetExtension(ddsFile);
            string d3dtxExtension = Path.GetExtension(d3dtxFile);
            if (!ddsExtension.Equals(Formats.ddsExtension))
            {
                throw new NotSupportedException($"We don't support {ddsExtension} extension for input file(we need {Formats.ddsExtension})!");
            }
            if (!d3dtxExtension.Equals(Formats.d3dtxExtension))
            {
                throw new NotSupportedException($"We don't support {d3dtxExtension} extension for output file!(we need {Formats.d3dtxExtension})");
            }
        }
    }

    public static Size ConvertPngToBmp(string inputPngPath, string outputBmpPath)
    {
        Texture png = new(inputPngPath, TextureType.PNG);

        string outputWitoutEx = Path.Combine(
            Path.GetDirectoryName(outputBmpPath) ?? throw new DirectoryNotFoundException($"GetDirectoryName returns null for {outputBmpPath}"),
            Path.GetFileNameWithoutExtension(outputBmpPath));

        png.SaveTexture(outputWitoutEx, TextureType.BMP);

        Texture result = new(Path.ChangeExtension(outputWitoutEx, "bmp"), TextureType.BMP);

        return new Size((int)result.Metadata.Width, (int)result.Metadata.Height);
    }

    public static bool CompressWithCompressonatorCli(string input, string output, T3SurfaceFormat format = T3SurfaceFormat.ETC2_RGBA, bool @throw = true)
    {
        string compressAlgo = format.ToString();
        string excutable = Program.Config.CompressonatorPath;
        string args = $"-fd {compressAlgo} \"{input}\" \"{output}\"";

        if (!CheckArgs(input, output, @throw))
        {
            return false;
        }

        ProcessStartInfo psi = new()
        {
            FileName = excutable,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = new() { StartInfo = psi };
        _ = process.Start();

        string stdo = process.StandardOutput.ReadToEnd();
        string stde = process.StandardError.ReadToEnd();

        process.WaitForExit();

#if DEBUG
        if (Debugger.IsAttached)
        {
            Console.WriteLine("Compressonator error:");
            Console.WriteLine(stde);
            Console.WriteLine("Compressonator output:");
            Console.WriteLine(stdo);
        }
#endif

        bool result = process.ExitCode == 0;
        return @throw && !result ? throw new Exception($"CCLIE: [{stde}] CCLIO: [{stdo}]") : result;
    }

    private static bool CheckArgs(string input, string output, bool @throw)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return @throw ? throw new ArgumentException("Input path cannot be null or empty!", nameof(input)) : false;
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            return @throw ? throw new ArgumentException("Output path cannot be null or empty!", nameof(output)) : false;
        }

        return true;
    }

    private static void CheckArgs(string sourceFilePath, string destinationFilePath)
    {
        if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationFilePath) || !Directory.Exists(Path.GetDirectoryName(destinationFilePath)))
        {
            throw new ArgumentException("Most likely the output folder couldn't be found or one of the arguments is null.");
        }
    }

    private static DdsD3dtx GetD3dtxDataFormDds(string ddsFile)
    {
        DDS_HEADER ddsHeader = new();
        ddsHeader.Load_DDS_HEADER(ddsFile);
        return new(ddsHeader.dwWidth, ddsHeader.dwHeight, ddsHeader.Get_DDS_DATA(ddsFile));
    }

    #endregion
}