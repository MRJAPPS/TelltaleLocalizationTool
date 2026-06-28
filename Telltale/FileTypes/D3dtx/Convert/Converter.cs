using Hexa.NET.DirectXTex;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.Graphics;
using TelltaleLocalizationTool.Telltale.Graphics.DDS;
using TelltaleLocalizationTool.Telltale.Graphics.Enums;
using TelltaleLocalizationTool.Telltale.Graphics.Image;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Convert;


public static class Converter
{
    private static TextureType GetTextureTypeFromExtension(string fileName) => Path.GetExtension(fileName) switch
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

    public static void ConvertTextureFromD3DtxToOthersV9(string sourceFilePath, string destinationFilePath, ImageAdvancedOptions options)
    {
        CheckArgs(sourceFilePath, destinationFilePath);

        D3DTXV9 d3dtxFile = new();
        d3dtxFile.LoadD3DTX(sourceFilePath);

        DDS_Master ddsFile = new(d3dtxFile);
        byte[] array = ddsFile.GetData(d3dtxFile);

        d3dtxFile.WriteD3DTXXML(Path.ChangeExtension(sourceFilePath, Formats.xmlExtension));

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
            d3dtxMaster.ResetForWD3NF_Font();
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

    private static void CheckArgs(string sourceFilePath, string destinationFilePath)
    {
        if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationFilePath) || !Directory.Exists(Path.GetDirectoryName(destinationFilePath)))
        {
            throw new ArgumentException("Most likely the output folder couldn't be found or one of the arguments is null.");
        }
    }

    #region NEW

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
            return false;

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
        if (@throw && !result)
            throw new Exception($"CCLIE: [{stde}] CCLIO: [{stdo}]");
        return result;
    }

    private static bool CheckArgs(string input, string output, bool @throw)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            if (@throw)
                throw new ArgumentException("Input path cannot be null or empty!", nameof(input));
            return false;
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            if (@throw)
                throw new ArgumentException("Output path cannot be null or empty!", nameof(output));
            return false;
        }

        return true;
    }

    #endregion
}