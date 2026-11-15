using Hexa.NET.DirectXTex;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using TelltaleLocalizationTool.BitmapFont;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.FileTypes.Font;
using TelltaleLocalizationTool.Telltale.Graphics;
using TelltaleLocalizationTool.Telltale.Graphics.DDS;
using TelltaleLocalizationTool.Telltale.Graphics.DDS.Data;
using TelltaleLocalizationTool.Telltale.Graphics.Enums;
using TelltaleLocalizationTool.Telltale.Graphics.Image;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
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

    public static void ConvertTextureFromD3DtxToOthers(string sourceFilePath, string destinationFilePath, ImageAdvancedOptions options, Games game)
    {
        CheckArgs(sourceFilePath, destinationFilePath);

        ID3DTX d3dtxFile = D3dtxFactory.CreateD3dtx(game);
        d3dtxFile.LoadD3DTX(sourceFilePath);

        DDS_Master ddsFile = new(d3dtxFile);
        byte[] array = ddsFile.GetData(d3dtxFile);

        d3dtxFile.WriteD3DTXXML(Path.ChangeExtension(
            options.WriteXmlAsideOutputTex ? destinationFilePath : sourceFilePath,
            options.Used3dtxXml ? Formats.d3dtxXmlExtension : Formats.xmlExtension));

        //The msCtx value isn't always null in D3dtx, TFont, and Landb!
        if (d3dtxFile.msCtx!.Game == Games.WDS1 && d3dtxFile.mD3dtxMetaData.mSurfaceFormat == T3SurfaceFormat.ETC1_RGB)
        {
            //The ETC1_RGB decoder in TelltaleTextureTool doesn't work properly! Implementing a new decoder in C# is also beyond my knowledge right now (I'll implement it later if I remember), and it's better to do the decoding via the compressonatorcli tool too, from dds...
            using TempFile ddsEtc1Input = new($"{T3SurfaceFormat.ETC1_RGB}_{d3dtxFile.msCtx!.Game}.dds");

            DDS_HEADER etc1Header = new();
            etc1Header.SetMeToEtc1DefaultHeader(d3dtxFile.mD3dtxMetaData.mTextureInfo.mWidth, d3dtxFile.mD3dtxMetaData.mTextureInfo.mHeight,
                (uint)d3dtxFile.mD3dtxMetaData.mStreamHeader.Sum(streamH => streamH.mDataSize));
            // D3DTX stores mips data in reverse order
            etc1Header.WriteMeToDdsFile(ddsEtc1Input.Path, d3dtxFile.mPixelData.AsEnumerable().Reverse());

            _ = MagicWithCompressonatorCli(ddsEtc1Input.Path, destinationFilePath, T3SurfaceFormat.ARGB8);
        }
        else
        {
            Texture texture = new(array, TextureType.D3DTX);
            try
            {
                texture.TransformTexture(options, true, false);
                texture.SaveTexture(destinationFilePath, GetTextureTypeFromExtension(destinationFilePath));
            }
            catch
            {
                throw;
            }
            finally
            {
                texture.Release();
            }
        }
    }

    public static void ConvertTextureFromOthersToD3DtxV9(string sourceFilePath, string destinationFilePath, ImageAdvancedOptions options, Games game, string? xmlFile = null)
    {
        ConvertTextureFromOthersToD3Dtx(sourceFilePath, destinationFilePath, options, game, xmlFile);
    }

    public static void ConvertTextureFromOthersToD3DtxV4(string sourceFilePath, string destinationFilePath, ImageAdvancedOptions options, Games game, string? xmlFile = null, T3SurfaceFormat? format = null)
    {
        ConvertTextureFromOthersToD3Dtx(sourceFilePath, destinationFilePath, options, game, xmlFile, format);
    }

    public static void ConvertTextureFromOthersToD3Dtx(string sourceFilePath, string destinationFilePath, ImageAdvancedOptions options, Games game, string? xmlFile = null, T3SurfaceFormat? format = null)
    {
        CheckArgs(sourceFilePath, destinationFilePath);

        if (!Path.GetExtension(destinationFilePath).Equals(Formats.d3dtxExtension, StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException($"The output file format must be {Formats.d3dtxExtension}!");
        }

        ID3DTX d3dtxMaster = D3dtxFactory.CreateD3dtx(game);
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

        try
        {
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

            if (d3dtxMaster is D3DTXV4)
            {
                options = new(options) { EnableAutomaticCompression = false, EnableTelltaleNormalMap = false };
            }
            texture.TransformTexture(options, true, true);
            if (d3dtxMaster is D3DTXV4 d3dtxv4)
            {
                ImportTextureV4(d3dtxv4, texture, game, format);
                d3dtxMaster.WriteD3DTX(destinationFilePath);
                return;
            }

            texture.GetDDSInformation(out ID3DTX d3dtx, out ImageSection[] sections, game, d3dtxMaster.mD3dtxMetaData.mVersion, flags);

            if (options.EnableSwizzle)
            {
                d3dtx.mD3dtxMetaData.mPlatform.EnumItem = options.PlatformType;
            }

            d3dtxMaster.ModifyD3DTX(d3dtx, sections);
        }
        catch
        {
            throw;
        }
        finally
        {
            texture.Release();
        }

        d3dtxMaster.WriteD3DTX(destinationFilePath);
    }

    public static void ImportTextureV4(D3DTXV4 d3dtx, Texture texture, Games game, T3SurfaceFormat? format = null)
    {
        if (format is not null and not T3SurfaceFormat.ETC1_RGB and not T3SurfaceFormat.ARGB8 and not T3SurfaceFormat.ARGB4)
        {
            throw new NotSupportedException("D3DTX version 4 import supports ETC1, ARGB8 and ARGB4!");
        }
        texture.GetDDSInformation(out ID3DTX source, out _, game, 4);
        if (source.IsCubemap() || source.IsVolumemap() || source.mD3dtxMetaData.mTextureInfo.mArraySize != 1)
        {
            throw new NotSupportedException("D3DTX version 4 only supports 2D textures!");
        }

        using TempFile inputDds = new("texture.dds");
        using TempFile convertedDds = new("texture.dds");
        //Compressonator can't read every packed DDS format (including ARGB4).
        texture.Decompress(DXGIFormat.R8G8B8A8_UNORM);
        texture.Convert(DXGIFormat.R8G8B8A8_UNORM);
        texture.GetDDSInformation(out source, out ImageSection[] sourceSections, game, 4);
        format ??= GetSurfaceFormatV4(sourceSections);
        texture.SaveAsDDS(inputDds.Path);
        _ = MagicWithCompressonatorCli(inputDds.Path, convertedDds.Path,
            format == T3SurfaceFormat.ETC1_RGB ? format.Value : T3SurfaceFormat.ARGB8);
        if (format == T3SurfaceFormat.ETC1_RGB)
        {
            ImageSection[] sections = GetETC1ImageSections(convertedDds.Path, source);
            source.mD3dtxMetaData.mSurfaceFormat = format.Value;
            d3dtx.ModifyD3DTX(source, sections);
            return;
        }
        Texture converted = new(convertedDds.Path, TextureType.DDS);
        try
        {
            if (format == T3SurfaceFormat.ARGB4)
            {
                converted.Convert(DXGIFormat.B4G4R4A4_UNORM);
            }
            converted.GetDDSInformation(out ID3DTX imported, out ImageSection[] sections, game, 4);
            if (imported.mD3dtxMetaData.mSurfaceFormat != format)
            {
                throw new InvalidDataException($"The converted texture isn't {format}!");
            }
            d3dtx.ModifyD3DTX(imported, sections);
        }
        finally
        {
            converted.Release();
        }
    }

    private static T3SurfaceFormat GetSurfaceFormatV4(ImageSection[] sections)
    {
        bool opaque = true, argb4 = true;
        foreach (ImageSection section in sections)
        {
            for (nuint row = 0; row < section.Height; ++row)
            {
                for (nuint column = 0; column < section.Width; ++column)
                {
                    int offset = checked((int)(row * section.RowPitch + column * 4));
                    opaque &= section.Pixels[offset + 3] == byte.MaxValue;
                    for (int channel = 0; channel < 4; ++channel)
                    {
                        argb4 &= section.Pixels[offset + channel] % 17 == 0;
                    }
                    if (!opaque && !argb4)
                    {
                        return T3SurfaceFormat.ARGB8;
                    }
                }
            }
        }
        return opaque ? T3SurfaceFormat.ETC1_RGB : argb4 ? T3SurfaceFormat.ARGB4 : T3SurfaceFormat.ARGB8;
    }

    private static ImageSection[] GetETC1ImageSections(string filePath, ID3DTX source)
    {
        using BinaryReader reader = new(File.OpenRead(filePath));
        byte[] header = reader.ReadBytes(128);
        uint width = source.mD3dtxMetaData.mTextureInfo.mWidth;
        uint height = source.mD3dtxMetaData.mTextureInfo.mHeight;
        uint mipLevels = source.mD3dtxMetaData.mTextureInfo.mNumMipLevels;
        if (header.Length != 128 || BitConverter.ToUInt32(header, 0) != 0x20534444 ||
            BitConverter.ToUInt32(header, 4) != 124 || BitConverter.ToUInt32(header, 76) != 32 ||
            BitConverter.ToUInt32(header, 80) != 4 || BitConverter.ToUInt32(header, 84) != 0x20435445 ||
            BitConverter.ToUInt32(header, 12) != height || BitConverter.ToUInt32(header, 16) != width ||
            Math.Max(1U, BitConverter.ToUInt32(header, 28)) != mipLevels)
        {
            throw new InvalidDataException("Compressonator didn't produce the expected ETC1 DDS texture!");
        }
        ImageSection[] sections = new ImageSection[mipLevels];
        for (int i = 0; i < sections.Length; ++i)
        {
            uint rowPitch = ((width + 3) / 4) * 8;
            uint slicePitch = rowPitch * ((height + 3) / 4);
            byte[] pixels = reader.ReadBytes(checked((int)slicePitch));
            if (pixels.Length != slicePitch)
            {
                throw new EndOfStreamException("The ETC1 mip data is incomplete!");
            }
            sections[i] = new() { Width = width, Height = height, RowPitch = rowPitch, SlicePitch = slicePitch, Pixels = pixels };
            width = Math.Max(1U, width / 2);
            height = Math.Max(1U, height / 2);
        }
        if (reader.BaseStream.Position != reader.BaseStream.Length)
        {
            throw new InvalidDataException("The ETC1 DDS has unexpected data after its mip levels!");
        }
        return sections;
    }


    #region NEW

    public static TFont ConvertTtfToFont(string input, float glyphSize, Games game, Action<string>? report = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        if (!Path.GetExtension(input).Equals(Formats.ttfExtension, StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"We need a {Formats.ttfExtension} file here!");
        }

        if (!float.IsFinite(glyphSize) || glyphSize <= 0 || glyphSize >= int.MaxValue || glyphSize != MathF.Truncate(glyphSize))
        {
            throw new ArgumentOutOfRangeException(nameof(glyphSize), "Glyph size must be a positive whole number of pixels!");
        }

        cancellationToken.ThrowIfCancellationRequested();
        string executable = Path.GetFullPath(Program.Config.BmfontPath64);
        (string fontName, bool bold, bool italic) = ReadTtfInfo(input);
        DirectoryInfo tempDirectory = Directory.CreateTempSubdirectory("TelltaleTtf_");
        try
        {
            string source = Path.Combine(tempDirectory.FullName, "source.ttf");
            string config = Path.Combine(tempDirectory.FullName, "font.bmfc");
            string fnt = Path.Combine(tempDirectory.FullName, "font.fnt");
            File.Copy(input, source);
            File.WriteAllText(config, $"""
                fileVersion=1
                fontName={fontName}
                fontFile=source.ttf
                charSet=1
                fontSize=-{(int)glyphSize}
                aa=1
                scaleH=100
                useSmoothing=1
                isBold={(bold ? 1 : 0)}
                isItalic={(italic ? 1 : 0)}
                useUnicode=1
                disableBoxChars=1
                outputInvalidCharGlyph=0
                dontIncludeKerningPairs=1
                useHinting=1
                renderFromOutline=0
                useClearType=0
                autoFitNumPages=0
                paddingDown=1
                paddingUp=1
                paddingRight=1
                paddingLeft=1
                spacingHoriz=1
                spacingVert=1
                useFixedHeight=0
                forceZero=0
                outWidth=1024
                outHeight=1024
                outBitDepth=32
                fontDescFormat=1
                fourChnlPacked=0
                textureFormat=dds
                textureCompression=0
                alphaChnl=0
                redChnl=4
                greenChnl=4
                blueChnl=4
                invA=0
                invR=0
                invG=0
                invB=0
                outlineThickness=0
                chars=32-55295,57344-1114111
                """, new UTF8Encoding(false));//TODO: THIS IS FOR WDS3NF, Move this to map table

            ProcessStartInfo psi = new()
            {
                FileName = executable,
                WorkingDirectory = tempDirectory.FullName,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add(config);
            psi.ArgumentList.Add("-o");
            psi.ArgumentList.Add(fnt);

            ConcurrentQueue<string> messages = new();
            StringBuilder log = new();
            using Process process = new() { StartInfo = psi };
            process.OutputDataReceived += (_, message) => { if (message.Data != null) { messages.Enqueue(message.Data); } };
            process.ErrorDataReceived += (_, message) => { if (message.Data != null) { messages.Enqueue(message.Data); } };
            report?.Invoke($"Generating {fontName} at {glyphSize} pixels with BMFont...");
            _ = process.Start();
            try
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                while (!process.WaitForExit(100))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ReportMessages();
                }
                process.WaitForExit();
                ReportMessages();
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }

                process.WaitForExit();
                throw;
            }
            if (process.ExitCode != 0 || !File.Exists(fnt))
            {
                throw new InvalidOperationException($"BMFont couldn't generate the font (exit code {process.ExitCode})!{Environment.NewLine}{log}");
            }

            report?.Invoke("Reading generated font glyphs and DDS atlases...");
            BmpFontFile bmpFont = BmpFontLoader.LoadFromFile(fnt);
            if (bmpFont.Info == null || bmpFont.Chars.Count == 0)
            {
                throw new InvalidDataException("BMFont didn't generate any usable glyphs!");
            }

            bmpFont.Info.Size = (int)glyphSize;
            TFont font = BmpFontFile.ToTFont(bmpFont, tempDirectory.FullName, Path.GetFileNameWithoutExtension(input), game,
                (page, done) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    report?.Invoke($"{(done ? "Imported" : "Importing")} atlas {page.Id + 1}/{bmpFont.Pages.Count}: {page.File}");
                });
            cancellationToken.ThrowIfCancellationRequested();
            return font;

            void ReportMessages()
            {
                while (messages.TryDequeue(out string? message))
                {
                    _ = log.AppendLine(message);
                    report?.Invoke($"BMFont: {message}");
                }
            }
        }
        finally
        {
            tempDirectory.Delete(true);
        }
    }

    private static (string Name, bool Bold, bool Italic) ReadTtfInfo(string input)
    {
        using BinaryReader reader = new(File.OpenRead(input));
        uint signature = ReadUInt32();
        if (signature is not 0x00010000 and not 0x74727565)
        {
            throw new InvalidDataException("This isn't a TrueType font!");
        }

        ushort tables = ReadUInt16();
        reader.BaseStream.Position = 12;
        uint nameOffset = 0, nameLength = 0, headOffset = 0;
        for (int index = 0; index < tables; ++index)
        {
            string tag = Encoding.ASCII.GetString(reader.ReadBytes(4));
            _ = ReadUInt32();
            uint offset = ReadUInt32(), length = ReadUInt32();
            if ((long)offset + length > reader.BaseStream.Length)
            {
                throw new InvalidDataException($"Invalid TTF table: {tag}");
            }

            if (tag == "name") { nameOffset = offset; nameLength = length; }
            if (tag == "head" && length >= 46)
            {
                headOffset = offset;
            }
        }
        if (nameOffset == 0 || nameLength < 6)
        {
            throw new InvalidDataException("I couldn't find the TTF name table!");
        }

        reader.BaseStream.Position = nameOffset;
        _ = ReadUInt16();
        ushort names = ReadUInt16(), strings = ReadUInt16();
        if (6L + (names * 12) > nameLength || strings > nameLength)
        {
            throw new InvalidDataException("The TTF name table is incomplete!");
        }

        string? family = null;
        for (int index = 0; index < names; ++index)
        {
            ushort platform = ReadUInt16(), encoding = ReadUInt16(), language = ReadUInt16();
            ushort name = ReadUInt16(), length = ReadUInt16(), offset = ReadUInt16();
            if (name != 1 || (platform != 0 && !(platform == 3 && (encoding == 1 || encoding == 10))))
            {
                continue;
            }

            if ((long)strings + offset + length > nameLength || length % 2 != 0)
            {
                throw new InvalidDataException("Invalid TTF family name!");
            }

            long position = reader.BaseStream.Position;
            reader.BaseStream.Position = (long)nameOffset + strings + offset;
            string candidate = Encoding.BigEndianUnicode.GetString(reader.ReadBytes(length));
            reader.BaseStream.Position = position;
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            family ??= candidate;
            if (platform == 3 && language == 0x0409) { family = candidate; break; }
        }
        if (family == null || family.IndexOfAny(['\r', '\n', '\0']) >= 0)
        {
            throw new InvalidDataException("I couldn't read a usable Unicode family name from the TTF!");
        }

        ushort style = 0;
        if (headOffset != 0)
        {
            reader.BaseStream.Position = headOffset + 44;
            style = ReadUInt16();
        }
        return (family, (style & 1) != 0, (style & 2) != 0);

        ushort ReadUInt16()
        {
            return BinaryPrimitives.ReverseEndianness(reader.ReadUInt16());
        }

        uint ReadUInt32()
        {
            return BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
        }
    }

    public static ID3DTX ConvertDdsToD3dtx(string ddsFile, string? name, Games game, T3SurfaceFormat format)
    {
        ArgumentNullException.ThrowIfNull(ddsFile, nameof(ddsFile));
        CheckArgsExtension(ddsFile);

        DdsD3dtx ddsData;
        ID3DTX result = D3dtxFactory.CreateD3dtx(game);
        using TempFile ddsResult = new(ddsFile);

        _ = MagicWithCompressonatorCli(ddsFile, ddsResult.Path, format);
        ddsData = GetD3dtxDataFormDds(ddsResult.Path, format);
        CallMapDdsToD3dtx();
        return result;
        void CallMapDdsToD3dtx()
        {
            switch (result.Version)
            {
                case 9:
                    ((D3DTXV9)result).MapDdsToD3dtx(ddsData, name);
                    break;
                case 4:
                    ((D3DTXV4)result).MapDdsToD3dtx(ddsData, name, format);
                    break;
                default: throw new NotImplementedException($"This version({result.Version}) of D3dtx isn't implemented yet — come back later.");
            }
        }
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

    public static void ConvertDdsToD3dtx(string ddsFile, string d3dtxFile, Games game, string? fontName = default, bool writeXml = true)
    {
        CheckArgs(ddsFile, d3dtxFile);
        CheckArgsExtension(ddsFile, d3dtxFile);

        DdsD3dtx ddsData;
        D3DTXV9 d3dtx = new(game: game);
        using TempFile etc2rgbaDds = new(ddsFile);

        _ = MagicWithCompressonatorCli(ddsFile, etc2rgbaDds.Path);
        ddsData = GetD3dtxDataFormDds(etc2rgbaDds.Path, T3SurfaceFormat.ETC2_RGBA);
        d3dtx.MapDdsToD3dtx(ddsData, fontName);
        d3dtx.WriteD3DTX(d3dtxFile);
        if (writeXml)
        {
            d3dtx.WriteD3DTXXML(Path.ChangeExtension(d3dtxFile, Formats.xmlExtension));
        }
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

    public static bool MagicWithCompressonatorCli(string input, string output, T3SurfaceFormat format = T3SurfaceFormat.ETC2_RGBA, bool @throw = true)
    {
        string compressAlgo = format switch
        {
            T3SurfaceFormat.ARGB8 => "ARGB_8888",
            T3SurfaceFormat.ETC1_RGB => "ETC_RGB",
            _ => format.ToString()
        };
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

        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();
        string stdo = outputTask.GetAwaiter().GetResult();
        string stde = errorTask.GetAwaiter().GetResult();

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

        return !string.IsNullOrWhiteSpace(output) || (@throw ? throw new ArgumentException("Output path cannot be null or empty!", nameof(output)) : false);
    }

    private static void CheckArgs(string sourceFilePath, string destinationFilePath)
    {
        if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationFilePath) || !Directory.Exists(Path.GetDirectoryName(destinationFilePath)))
        {
            throw new ArgumentException("Most likely the output folder couldn't be found or one of the arguments is null.");
        }
    }

    private static DdsD3dtx GetD3dtxDataFormDds(string ddsFile, T3SurfaceFormat format)
    {
        DDS_HEADER ddsHeader = new();
        ddsHeader.Load_DDS_HEADER(ddsFile);
        byte[] pixelData = ddsHeader.Get_DDS_DATA(ddsFile);

        int len = Utils.GetBiggestDdsMipBytesLength((int)ddsHeader.dwWidth, (int)ddsHeader.dwHeight, format, out _);
        //The point of adding this section is to make sure the first mip is always present! We don't care about the rest!
        pixelData = pixelData.Slice(0, len);
        ddsHeader.dwMipMapCount = 1;

        return new(ddsHeader.dwWidth, ddsHeader.dwHeight, pixelData);
    }

    #endregion
}
