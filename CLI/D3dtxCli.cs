using Spectre.Console;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Convert;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.Graphics;
using TelltaleLocalizationTool.Telltale.Graphics.DDS;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using static TelltaleLocalizationTool.CLI.CommonCli;

namespace TelltaleLocalizationTool.CLI;

internal static class D3dtxCli
{
    internal static readonly string[] OutputTexExtensions = [Formats.ddsExtension, Formats.pngExtension, Formats.jpegExtension, Formats.bmpExtension, Formats.tifExtension, Formats.tgaExtension, Formats.hdrExtension];

    internal static T3SurfaceFormat? GetSurfaceFormat(string format)
    {
        return format.Trim().ToUpperInvariant() switch
        {
            "" or "AUTO" => null,
            "ETC1" or "ETC1_RGB" => T3SurfaceFormat.ETC1_RGB,
            "ARGB8" => T3SurfaceFormat.ARGB8,
            "ARGB4" => T3SurfaceFormat.ARGB4,
            _ => throw new ArgumentException("Use auto, ETC1, ARGB8 or ARGB4 for --format!")
        };
    }

    private static void ImportD3dtx(ID3DTX d3dtx, Texture texture, Games game, T3SurfaceFormat? format)
    {
        if (d3dtx is D3DTXV4 d3dtxv4)
        {
            Converter.ImportTextureV4(d3dtxv4, texture, game, format);
            SimpleAnsiConsole.ShowInfo($"Texture format: {d3dtxv4.mD3dtxMetaData.mSurfaceFormat}");
            return;
        }
        texture.GetDDSInformation(out ID3DTX imported, out ImageSection[] sections, game, d3dtx.mD3dtxMetaData.mVersion);
        d3dtx.ModifyD3DTX(imported, sections);
    }

    internal static void CheckOutputTexFormat(string fileNameOrFormat)
    {
        if (!OutputTexExtensions.Contains(Path.GetExtension(fileNameOrFormat).ToLowerInvariant()))
        {
            throw new NotImplementedException("Use DDS, PNG, JPEG, BMP, TIFF (.tif), TGA or HDR for your texture!");
        }
    }

    internal static void ProcessD3dtx(string input, string output, bool import, Games game, ProgressContext ctx, ProgressTask task, T3SurfaceFormat? surfaceFormat, bool force, CancellationToken cancellationToken)
    {
        string inputFileName = Path.GetFileName(input);
        ReportProgress(ctx, task, $"Reading {inputFileName}...", cancellationToken);
        using TempFile temp = new(output);
        ID3DTX d3dtx = D3dtxFactory.CreateD3dtx(game);
        if (import)
        {
            ImportFile(input, output, game, ctx, task, temp, d3dtx, surfaceFormat, force, cancellationToken);
        }
        else
        {
            ExportFile(input, output, ctx, task, temp, d3dtx, force, cancellationToken);
        }

        task.Increment(1);
    }

    private static void ImportFile(string input, string output, Games game, ProgressContext ctx, ProgressTask task, TempFile temp, ID3DTX d3dtx, T3SurfaceFormat? surfaceFormat, bool force, CancellationToken cancellationToken)
    {
        CheckOutputTexFormat(input);
        string xmlFile = Path.ChangeExtension(input, Formats.d3dtxXmlExtension);
        if (!File.Exists(xmlFile))
        {
            xmlFile = Path.ChangeExtension(input, Formats.xmlExtension);
        }
        if (!File.Exists(xmlFile))
        {
            throw new FileNotFoundException($"Your texture needs its {Formats.d3dtxXmlExtension} file next to it! I can't guess the original metadata.");
        }
        d3dtx.LoadFromXML(xmlFile);
        task.Increment(1);
        ReportProgress(ctx, task, "Importing d3dtx texture and metadata...", cancellationToken);
        Texture texture = new(input, Converter.GetTextureTypeFromExtension(input.ToLowerInvariant()));
        try
        {
            if (!Path.GetExtension(input).Equals(Formats.ddsExtension, oic))
            {
                if (d3dtx.IsCubemap() || d3dtx.IsVolumemap() || d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize > 1)
                {
                    throw new NotSupportedException("Use DDS for cubemaps, texture arrays and 3D textures! One picture can't hold all their faces or slices.");
                }
                uint mipLevels = Math.Min(d3dtx.mD3dtxMetaData.mTextureInfo.mNumMipLevels, texture.GetMaxMipLevels());
                if (mipLevels > 1)
                {
                    texture.GenerateMipMaps(mipLevels);
                }
                if (d3dtx is not D3DTXV4 && !d3dtx.mD3dtxMetaData.mSurfaceFormat.IsFormatIncompatibleWithDDS())
                {
                    DXGIFormat format = DDSHelper.GetDXGIFormat(d3dtx.mD3dtxMetaData.mSurfaceFormat, d3dtx.mD3dtxMetaData.mSurfaceGamma);
                    if (Hexa.NET.DirectXTex.DirectXTex.IsCompressed((int)format))
                    {
                        texture.Compress(format);
                    }
                    else
                    {
                        texture.Convert(format);
                    }
                }
            }
            ImportD3dtx(d3dtx, texture, game, surfaceFormat);
        }
        finally
        {
            texture.Release();
        }
        task.Increment(1);
        ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)}...", cancellationToken);
        d3dtx.WriteToFile(temp.Path);
        SaveResult(temp, output, force, cancellationToken);
    }

    private static void ExportFile(string input, string output, ProgressContext ctx, ProgressTask task, TempFile temp, ID3DTX d3dtx, bool force, CancellationToken cancellationToken)
    {
        CheckOutputTexFormat(output);
        string xmlFile = Path.ChangeExtension(output, Formats.d3dtxXmlExtension);
        CheckOutput(xmlFile, force);
        bool xmlAlreadyExisted = File.Exists(xmlFile);
        using TempFile tempXml = new(xmlFile);
        d3dtx.LoadFromFile(input);
        task.Increment(1);
        ReportProgress(ctx, task, "Exporting d3dtx texture...", cancellationToken);
        DDS_Master dds = new(d3dtx);
        byte[] data = dds.GetData(d3dtx);
        if (d3dtx.IsCubemap() && !d3dtx.HasDDSHeader() && data.Length >= 148 && BitConverter.ToUInt32(data, 84) == 0x30315844)
        {
            //The generated DX10 header counts faces as array items. DDS needs the cube flag and the cube count.
            BitConverter.GetBytes(BitConverter.ToUInt32(data, 112) | 0xFE00U).CopyTo(data, 112);
            BitConverter.GetBytes(BitConverter.ToUInt32(data, 136) | 4U).CopyTo(data, 136);
            BitConverter.GetBytes(d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize).CopyTo(data, 140);
        }
        if (Path.GetExtension(output).Equals(Formats.ddsExtension, oic))
        {
            File.WriteAllBytes(temp.Path, data);
        }
        else
        {
            Texture texture = new(data);
            try
            {
                if (texture.Metadata.ArraySize > 1 || texture.Metadata.Depth > 1)
                {
                    throw new NotSupportedException("Use DDS for cubemaps, texture arrays and 3D textures! One picture can't hold all their faces or slices.");
                }
                texture.SaveTexture(temp.Path, Converter.GetTextureTypeFromExtension(output.ToLowerInvariant()));
            }
            finally
            {
                texture.Release();
            }
        }
        d3dtx.WriteToXML(tempXml.Path);
        task.Increment(1);
        ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)} and metadata...", cancellationToken);
        bool savedXml = false;
        try
        {
            SaveResult(tempXml, xmlFile, force, cancellationToken);
            savedXml = true;
            SaveResult(temp, output, force, cancellationToken);
        }
        catch
        {
            if (savedXml && !xmlAlreadyExisted)
            {
                RemoveIncompleteOutput(xmlFile, "metadata");
            }
            throw;
        }
    }
}
