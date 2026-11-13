using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics;
using System.Xml;
using TelltaleLocalizationTool.BitmapFont;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Convert;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.FileTypes.Font;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB;
using TelltaleLocalizationTool.Telltale.Graphics;
using TelltaleLocalizationTool.Telltale.Graphics.DDS;
using TelltaleLocalizationTool.Telltale.Meta.Enums;

namespace TelltaleLocalizationTool.CLI;

public class CliProgram : Command<Options>
{
    private const StringComparison oic = StringComparison.OrdinalIgnoreCase;

    public override int Execute(CommandContext context, Options settings, CancellationToken cancellationToken)
    {
        try
        {
            settings.Mode = settings.Mode.ToLowerInvariant();
            settings.FileType = settings.FileType.ToLowerInvariant();
            switch (settings.Mode)
            {
                case Options.Modes.c_export:
                case Options.Modes.c_import:
                    Games game = GetGame(settings.Game);
                    CheckArgs(settings);
                    PrepareConfiguration();
                    return ProcessFiles(settings, game, cancellationToken);
                case Options.Modes.c_about:
                    ShowAbout();
                    return 0;
                default:
                    throw new ArgumentException($"I can't use mode \"{settings.Mode}\"! Use export, import or about.");
            }
        }
        catch (OperationCanceledException)
        {
            SimpleAnsiConsole.ShowWarring("Cancelled! Files that were already saved are still in your output folder.");
            return 2;
        }
        catch (Exception ex)
        {
            Exception error = ex;
            while (error is TypeInitializationException && error.InnerException is not null)
            {
                error = error.InnerException;
            }
            SimpleAnsiConsole.ShowError(error.Message);
            return 1;
        }
    }

    #region Error Checking

    private static Games GetGame(string game)
    {
        game = game.Trim().ToUpper();
        if (game.Contains(',') || !Enum.TryParse(game, true, out Games value) || !Enum.IsDefined(value))
        {
            throw new ArgumentException($"I can't use game \"{game}\"! Use a name or number from Games: {string.Join(", ", Enum.GetValues<Games>().Select(item => $"{item} ({(int)item})"))}.");
        }
        return value != Games.WDS3NF && !Debugger.IsAttached
            ? throw new NotSupportedException($"{value} isn't supported yet! We only support WDS3NF for now.")
            : value;
    }

    private static void CheckArgs(Options settings)
    {
        if (settings.Mode == Options.Modes.c_import && settings.FileType == Options.FileTypes.c_font &&
            !settings.Batch && Path.GetExtension(settings.Input).Equals(Formats.ttfExtension, oic))
        {
            settings.FileType = Options.FileTypes.c_ttf;
        }
        if (settings.FileType is not Options.FileTypes.c_landb and not Options.FileTypes.c_font and not Options.FileTypes.c_d3dtx and not Options.FileTypes.c_ttf)
        {
            throw new ArgumentException($"Enter a valid file type! We need {Options.FileTypes.c_landb}, {Options.FileTypes.c_font}, {Options.FileTypes.c_d3dtx} or {Options.FileTypes.c_ttf}.");
        }
        if (settings.FileType == Options.FileTypes.c_ttf)
        {
            if (settings.Mode != Options.Modes.c_import)
            {
                throw new NotSupportedException("TTF is input only! We can't convert a bitmap font back to TTF.");
            }

            if (!float.IsFinite(settings.GlyphSize) || settings.GlyphSize <= 0 || settings.GlyphSize >= int.MaxValue || settings.GlyphSize != MathF.Truncate(settings.GlyphSize))
            {
                throw new ArgumentOutOfRangeException(nameof(settings.GlyphSize), "TTF import needs --glyphSize with a positive whole pixel size!");
            }
        }
        else if (settings.GlyphSize != -1F)
        {
            throw new ArgumentException("--glyphSize is only used for TTF import!");
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Input, nameof(settings.Input));
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Output, nameof(settings.Output));
        if (settings.Relative && !settings.Batch)
        {
            throw new ArgumentException("--relative needs --batch! A single file doesn't have subfolders to keep; Seriously, you didn't know that?");
        }

        settings.Input = Path.GetFullPath(settings.Input);
        settings.Output = Path.GetFullPath(settings.Output);
        if (settings.Batch)
        {
            if (!Directory.Exists(settings.Input))
            {
                throw new DirectoryNotFoundException($"I couldn't find your input folder! \"{settings.Input}\"");
            }
            if (File.Exists(settings.Output))
            {
                throw new ArgumentException("The output must be a directory when you're using --batch!");
            }
            if (Path.TrimEndingDirectorySeparator(settings.Input).Equals(Path.TrimEndingDirectorySeparator(settings.Output), oic))
            {
                throw new ArgumentException("Input and output folders must be different! Keep your source files somewhere safe.");
            }
        }
        else
        {
            if (!File.Exists(settings.Input))
            {
                throw new FileNotFoundException($"Are you sure \"{settings.Input}\" exists?");
            }
            if (settings.FileType == Options.FileTypes.c_d3dtx)
            {
                bool import = settings.Mode == Options.Modes.c_import;
                CheckExtension(import ? settings.Output : settings.Input, Formats.d3dtxExtension);
                CheckOutputTexFormat(import ? settings.Input : settings.Output);
            }
            else
            {
                CheckExtension(settings.Input, GetExtension(settings, true));
                CheckExtension(settings.Output, GetExtension(settings, false));
            }
            if (Directory.Exists(settings.Output))
            {
                throw new ArgumentException("The output must be a file path! Use --batch if you want to process folders.");
            }
        }
    }

    private static void PrepareConfiguration()
    {
        string currentDirectoryConfig = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        string applicationDirectoryConfig = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(currentDirectoryConfig) && File.Exists(applicationDirectoryConfig))
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        }
    }

    private static void CheckExtension(string fileName, string extension)
    {
        if (!Path.GetExtension(fileName).Equals(extension, oic))
        {
            throw new FormatException($"We need a {extension} file here! \"{fileName}\"");
        }
    }

    private static void CheckOutput(string output)
    {
        if (File.Exists(output) || Directory.Exists(output))
        {
            throw new IOException($"\"{output}\" already exists! Choose another output path.");
        }
    }

    #endregion

    #region File Processing

    private static IEnumerable<string> EnumerateFontImportFiles(
        string inputDirectory,
        bool recursive,
        CancellationToken cancellationToken)
    {
        EnumerationOptions options = new()
        {
            RecurseSubdirectories = true,
            MaxRecursionDepth = recursive ? int.MaxValue : 1,
            IgnoreInaccessible = false,
            AttributesToSkip = FileAttributes.ReparsePoint
        };

        foreach (string file in Directory.EnumerateFiles(inputDirectory, $"*{Formats.fntExtension}", options))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return file;
        }
    }

    private static string GetOutputPath(Options settings, string input)
    {
        if (!settings.Batch)
        {
            return settings.Output;
        }

        string relativeDirectory = Path.GetRelativePath(
            settings.Input,
            Path.GetDirectoryName(input) ?? settings.Input);
        string relativeName = Path.GetFileNameWithoutExtension(input);

        if (settings.FileType is Options.FileTypes.c_font or Options.FileTypes.c_d3dtx or Options.FileTypes.c_ttf)
        {
            string fileDir = relativeDirectory != "." &&
                (settings.Mode == Options.Modes.c_import || Path.GetFileName(relativeDirectory).Equals(relativeName, oic))
                ? Path.Combine(settings.Output, relativeDirectory)
                : Path.Combine(settings.Output, relativeDirectory, relativeName);
            return Path.Combine(fileDir, relativeName + GetExtension(settings, false));
        }

        string relativeFile = Path.GetRelativePath(settings.Input, input);
        return Path.Combine(settings.Output, Path.ChangeExtension(relativeFile, GetExtension(settings, false)));
    }

    private static string GetExtension(Options settings, bool input)
    {
        bool binary = settings.Mode == Options.Modes.c_export ? input : !input;
        return settings.FileType switch
        {
            Options.FileTypes.c_landb => binary ? Formats.landbExtension : Formats.xmlExtension,
            Options.FileTypes.c_font => binary ? Formats.fontExtension : Formats.fntExtension,
            Options.FileTypes.c_ttf => input ? Formats.ttfExtension : Formats.fontExtension,
            Options.FileTypes.c_d3dtx => binary ? Formats.d3dtxExtension : settings.Batch ? Formats.ddsExtension :
                Path.GetExtension(settings.Mode == Options.Modes.c_import ? settings.Input : settings.Output).ToLowerInvariant(),
            _ => throw new NotSupportedException($"We only support {Formats.landbExtension}({Formats.xmlExtension}), {Formats.fontExtension}({Formats.fntExtension}) and {Formats.d3dtxExtension}({Formats.ddsExtension},{Formats.pngExtension},{Formats.jpegExtension}) here!")
        };
    }

    private static int ProcessFiles(Options settings, Games game, CancellationToken cancellationToken)
    {
        int success = 0, failed = 0;
        AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new ElapsedTimeColumn(), new SpinnerColumn())
            .Start(ctx =>
            {
                ProgressTask total = ctx.AddTask("Finding input files...");
                total.IsIndeterminate = true;
                List<string> files = [];
                if (settings.Batch)
                {
                    if (settings.Mode == Options.Modes.c_import && settings.FileType == Options.FileTypes.c_d3dtx)
                    {
                        EnumerationOptions options = new()
                        {
                            RecurseSubdirectories = true,
                            MaxRecursionDepth = settings.Relative ? int.MaxValue : 1,
                            IgnoreInaccessible = false,
                            AttributesToSkip = FileAttributes.ReparsePoint
                        };

                        foreach (string file in Directory.EnumerateFiles(settings.Input, "*", options))
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            string extension = Path.GetExtension(file).ToLowerInvariant();
                            if (extension is not Formats.ddsExtension and not Formats.pngExtension and not Formats.jpegExtension)
                            {
                                continue;
                            }
                            files.Add(file);
                            total.Description = $"Found {files.Count} input file(s)";
                            ctx.Refresh();
                        }
                    }
                    else if (settings.Mode == Options.Modes.c_import &&
                        settings.FileType == Options.FileTypes.c_font)
                    {
                        foreach (string file in EnumerateFontImportFiles(settings.Input, settings.Relative, cancellationToken))
                        {
                            files.Add(file);
                            total.Description = $"Found {files.Count} input file(s)";
                            ctx.Refresh();
                        }
                    }
                    else
                    {
                        EnumerationOptions options = new()
                        {
                            RecurseSubdirectories = settings.Relative,
                            IgnoreInaccessible = false,
                            AttributesToSkip = FileAttributes.ReparsePoint
                        };

                        foreach (string file in Directory.EnumerateFiles(
                            settings.Input,
                            $"*{GetExtension(settings, true)}",
                            options))
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            files.Add(file);
                            total.Description = $"Found {files.Count} input file(s)";
                            ctx.Refresh();
                        }
                    }

                    files.Sort(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    files.Add(settings.Input);
                }
                cancellationToken.ThrowIfCancellationRequested();
                if (files.Count == 0)
                {
                    string extension = settings.FileType == Options.FileTypes.c_d3dtx && settings.Mode == Options.Modes.c_import
                        ? $"{Formats.ddsExtension}, {Formats.pngExtension} or {Formats.jpegExtension}"
                        : GetExtension(settings, true);
                    throw new FileNotFoundException($"I couldn't find any {extension} files in your input folder!");
                }

                total.IsIndeterminate = false;
                total.MaxValue = files.Count;
                total.Description = $"{settings.Mode}: 0/{files.Count} file(s)";
                ProgressTask? current = null;
                SimpleAnsiConsole.ShowInfo($"Found {files.Count} file(s). Let's {settings.Mode} them!");
                foreach (string input in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string output = GetOutputPath(settings, input);
                    if (current != null)
                    {
                        _ = ctx.RemoveTask(current);
                    }

                    current = ctx.AddTask(Markup.Escape(Path.GetFileName(input)), maxValue: 3);
                    try
                    {
                        CheckOutput(output);
                        SimpleAnsiConsole.ShowInfo($"{settings.Mode}: \"{input}\"");
                        switch (settings.FileType)
                        {
                            case Options.FileTypes.c_landb:
                                ProcessLandb(input, output, settings.Mode == Options.Modes.c_import, game, ctx, current, cancellationToken);
                                break;
                            case Options.FileTypes.c_font:
                                ProcessFont(input, output, settings.Mode == Options.Modes.c_import, game, ctx, current, cancellationToken);
                                break;
                            case Options.FileTypes.c_ttf:
                                ProcessTtf(input, output, settings.GlyphSize, game, ctx, current, cancellationToken);
                                break;
                            case Options.FileTypes.c_d3dtx:
                                ProcessD3dtx(input, output, settings.Mode == Options.Modes.c_import, game, ctx, current, cancellationToken);
                                break;
                        }
                        success++;
                        current.Description = $"[green]Saved:[/] {Markup.Escape(Path.GetFileName(output))}";
                        SimpleAnsiConsole.ShowInfo($"Saved to \"{output}\"");
                    }
                    catch (OperationCanceledException)
                    {
                        current.Description = "[yellow]Cancelled![/]";
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Exception error = ex;
                        while (error is TypeInitializationException && error.InnerException is not null)
                        {
                            error = error.InnerException;
                        }
                        failed++;
                        current.Description = $"[red]Failed:[/] {Markup.Escape(Path.GetFileName(input))}";
                        SimpleAnsiConsole.ShowError($"\"{input}\": {error.Message}");
                    }
                    finally
                    {
                        current.StopTask();
                        ctx.Refresh();
                    }
                    total.Increment(1);
                    total.Description = $"{settings.Mode}: {success + failed}/{files.Count} file(s)";
                    ctx.Refresh();
                }
                total.Description = failed == 0 ? "[green]All files processed![/]" : "[yellow]Finished with errors![/]";
            });
        SimpleAnsiConsole.ShowInfo($"Done! {success} succeeded, {failed} failed.");
        return failed == 0 ? 0 : 1;
    }

    private static void ReportProgress(ProgressContext ctx, ProgressTask task, string message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        task.Description = Markup.Escape(message);
        SimpleAnsiConsole.ShowInfo(message);
        ctx.Refresh();
    }

    private static void SaveResult(TempFile temp, string output, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        File.Copy(temp.Path, output);
    }

    #endregion

    #region Landb

    private static void ProcessLandb(string input, string output, bool import, Games game, ProgressContext ctx, ProgressTask task, CancellationToken cancellationToken)
    {
        Landb landb = new(rMsv6Header: !import, game: game);
        XmlDocument doc = new();
        ReportProgress(ctx, task, $"Reading {Path.GetFileName(input)}...", cancellationToken);
        if (import)
        {
            doc.Load(input);
        }
        else
        {
            using FileStream inputStream = new(input, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new(inputStream);
            landb.Parse(reader);
        }
        task.Increment(1);

        ReportProgress(ctx, task, import ? "Importing landb XML..." : "Exporting landb XML...", cancellationToken);
        if (import)
        {
            XmlElement root = doc.DocumentElement ?? throw new InvalidDataException("The input XML file has no elements!");
            XmlElement element = root.GetSingleElement(nameof(Landb));
            landb.Parse(root, element.GetAttribute("name"));
        }
        else
        {
            XmlElement root = doc.CreateElement("Root");
            _ = doc.AppendChild(root);
            landb.Write(root, nameof(landb));
        }
        task.Increment(1);

        ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)}...", cancellationToken);
        using TempFile temp = new(output);
        if (import)
        {
            using FileStream outputStream = new(temp.Path, FileMode.Create, FileAccess.Write);
            using BinaryWriter writer = new(outputStream);
            landb.Write(writer);
        }
        else
        {
            doc.Save(temp.Path);
        }
        SaveResult(temp, output, cancellationToken);
        task.Increment(1);
    }

    #endregion

    #region Font

    private static void ProcessTtf(string input, string output, float glyphSize, Games game, ProgressContext ctx, ProgressTask task, CancellationToken cancellationToken)
    {
        task.IsIndeterminate = true;
        TFont font;
        try
        {
            font = Converter.ConvertTtfToFont(input, glyphSize, game,
                message => ReportProgress(ctx, task, message, cancellationToken), cancellationToken);
        }
        finally
        {
            task.IsIndeterminate = false;
        }
        task.Value = 2;
        ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)}...", cancellationToken);
        using TempFile temp = new(output);
        font.WriteTFont(temp.Path);
        SaveResult(temp, output, cancellationToken);
        task.Increment(1);
    }

    private static TFont ImportFont(BmpFontFile bmpFont, string input, Games game, ProgressContext ctx, ProgressTask task, CancellationToken cancellationToken)
    {
        if (bmpFont.Info == null || bmpFont.Common == null)
        {
            throw new InvalidDataException($"Your fnt file needs {nameof(BmpFontFile.Info)} and {nameof(BmpFontFile.Common)} elements! I can't build a font out of nothing.");
        }
        if (bmpFont.Common.ScaleW <= 0 || bmpFont.Common.ScaleH <= 0 || bmpFont.Pages.Count == 0 || bmpFont.Common.Pages != bmpFont.Pages.Count)
        {
            throw new InvalidDataException("Check your fnt dimensions and page count! We need valid atlas sizes and at least one page.");
        }
        List<BmpFontPage> pages = [.. bmpFont.Pages.OrderBy(page => page.Id)];
        string inputDirectory = Path.GetDirectoryName(input)!;
        for (int i = 0; i < pages.Count; ++i)
        {
            BmpFontPage page = pages[i];
            if (page.Id != i)
            {
                throw new InvalidDataException("Font page IDs must start at 0 with no gaps or duplicates!");
            }
            string atlas = Path.GetFullPath(Path.Combine(inputDirectory, page.File));
            if (!File.Exists(atlas))
            {
                throw new FileNotFoundException($"I couldn't find font atlas \"{atlas}\"!");
            }
            CheckExtension(atlas, Formats.ddsExtension);
        }
        TFont font = new(game: game)
        {
            mBase = bmpFont.Common.Base,
            mbUnicode = bmpFont.Info.Unicode == 1,
            mFontScale = 1,//Default
            mGlyphInfos = [],
            mHeight = bmpFont.Info.Size,
            mIsDistanceField = false,//Default
            mName = bmpFont.Info.Face,
            mWiiScale = 0.5F,//Default
            ReadMsv6Header = false,
            WriteMsv6Header = true,
            mTexturePages = []
        };
        foreach (BmpFontChar bmpChar in bmpFont.Chars)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (bmpChar.Page < 0 || bmpChar.Page >= pages.Count)
            {
                throw new InvalidDataException($"Glyph {bmpChar.Id} refers to a font page that doesn't exist!");
            }
            font.mGlyphInfos.Add(new()
            {
                mChar = new() { CharCode = (uint)bmpChar.Id },
                mTexturePage = bmpChar.Page,
                mChannel = bmpChar.Channel,
                mRect = new()
                {
                    Left = bmpChar.X / (float)bmpFont.Common.ScaleW,
                    Top = bmpChar.Y / (float)bmpFont.Common.ScaleH,
                    Right = (bmpChar.X + bmpChar.Width) / (float)bmpFont.Common.ScaleW,
                    Bottom = (bmpChar.Y + bmpChar.Height) / (float)bmpFont.Common.ScaleH
                },
                mGlyphMetrics = new()
                {
                    mXOffset = bmpChar.XOffset,
                    mYOffset = bmpChar.YOffset,
                    mXAdvance = bmpChar.XAdvance
                },
                mGlyphSize = new()
                {
                    mWidth = bmpChar.Width,
                    mHeight = bmpChar.Height
                }
            });
        }
        task.Increment(1);
        foreach (BmpFontPage page in pages)
        {
            ReportProgress(ctx, task, $"Importing atlas {page.Id}: {page.File}", cancellationToken);
            using TempFile tempAtlas = new(Path.ChangeExtension(page.File, Formats.ddsExtension));
            File.Copy(Path.Combine(inputDirectory, page.File), tempAtlas.Path, true);
            font.mTexturePages.Add(Converter.ConvertDdsToD3dtx(tempAtlas.Path, Path.GetFileNameWithoutExtension(input), game));
            task.Increment(1);
            ctx.Refresh();
        }
        return font;
    }

    #endregion

    #region D3dtx

    private static void ImportD3dtx(D3DTXV9 d3dtx, Texture texture, Games game)
    {
        texture.GetDDSInformationV9(out D3DTXV9 imported, out ImageSection[] sections, game);
        if (imported.mD3dtxMetaData.mSurfaceFormat == T3SurfaceFormat.Unknown)
        {
            throw new NotSupportedException("I can't map this image's pixel format to a Telltale surface format! Save it as a supported DDS format first.");
        }
        int mipLevels = (int)imported.mD3dtxMetaData.mTextureInfo.mNumMipLevels;
        int arraySize = (int)imported.mD3dtxMetaData.mTextureInfo.mArraySize;
        int depth = (int)imported.mD3dtxMetaData.mTextureInfo.mDepth;
        List<(ImageSection section, int mip, int face)> images = [];
        int index = 0;
        if (imported.IsVolumemap())
        {
            for (int mip = 0; mip < mipLevels; ++mip)
            {
                for (int face = 0; face < Math.Max(1, depth >> mip); ++face)
                {
                    images.Add((sections[index++], mip, face));
                }
            }
        }
        else
        {
            for (int face = 0; face < arraySize; ++face)
            {
                for (int mip = 0; mip < mipLevels; ++mip)
                {
                    images.Add((sections[index++], mip, face));
                }
            }
        }

        d3dtx.mD3dtxMetaData.mTextureInfo = imported.mD3dtxMetaData.mTextureInfo;
        d3dtx.mD3dtxMetaData.mSurfaceFormat = imported.mD3dtxMetaData.mSurfaceFormat;
        d3dtx.mD3dtxMetaData.mSurfaceGamma = imported.mD3dtxMetaData.mSurfaceGamma;
        d3dtx.mD3dtxMetaData.mTextureLayout = imported.mD3dtxMetaData.mTextureLayout;
        if (imported.IsCubemap())
        {
            d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize /= 6;
            d3dtx.mD3dtxMetaData.mTextureLayout = d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize > 1
                ? T3TextureLayout.TextureCubemapArray : T3TextureLayout.TextureCubemap;
        }
        d3dtx.mD3dtxMetaData.mStreamHeader = new(9)
        {
            mRegionCount = sections.Length,
            mAuxDataCount = d3dtx.mD3dtxMetaData.mStreamHeader.mAuxDataCount,
            mTotalDataSize = sections.Sum(section => section.Pixels.Length)
        };
        d3dtx.mPixelData.Clear();
        //Telltale stores the smallest mip first. DDS starts with the biggest one.
        foreach ((ImageSection section, int mip, int face) in images.OrderByDescending(image => image.mip).ThenBy(image => image.face))
        {
            d3dtx.mPixelData.Add(section.Pixels);
            d3dtx.mD3dtxMetaData.mStreamHeader.Add(new(9)
            {
                mMipIndex = mip,
                mFaceIndex = face,
                mMipCount = 1,
                mDataSize = (uint)section.Pixels.Length,
                mPitch = (int)section.RowPitch,
                mSlicePitch = (int)section.SlicePitch
            });
        }
    }

    private static void CheckOutputTexFormat(string fileNameOrFormat)
    {
        string format = Path.GetExtension(fileNameOrFormat).ToLowerInvariant();
        switch (format)
        {
            case Formats.jpegExtension:
            case Formats.pngExtension:
            case Formats.ddsExtension:
                return;
        }
        throw new NotImplementedException($"The necessary conversions for your requested format haven't been implemented! You can only use {Formats.jpegExtension}, {Formats.pngExtension}, and {Formats.ddsExtension}!");
    }

    private static void ProcessD3dtx(string input, string output, bool import, Games game, ProgressContext ctx, ProgressTask task, CancellationToken cancellationToken)
    {
        string inputFileName = Path.GetFileName(input);
        ReportProgress(ctx, task, $"Reading {inputFileName}...", cancellationToken);
        using TempFile temp = new(output);
        D3DTXV9 d3dtx = new(game: game);
        if (import)
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
            d3dtx.LoadD3DTXXML(xmlFile);
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
                    if (!d3dtx.mD3dtxMetaData.mSurfaceFormat.IsFormatIncompatibleWithDDS())
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
                ImportD3dtx(d3dtx, texture, game);
            }
            finally
            {
                texture.Release();
            }
            task.Increment(1);
            ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)}...", cancellationToken);
            d3dtx.WriteD3DTX(temp.Path);
            SaveResult(temp, output, cancellationToken);
        }
        else
        {
            CheckOutputTexFormat(output);
            string xmlFile = Path.ChangeExtension(output, Formats.d3dtxXmlExtension);
            CheckOutput(xmlFile);
            using TempFile tempXml = new(xmlFile);
            d3dtx.LoadD3DTX(input);
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
                    if (Path.GetExtension(output).Equals(Formats.pngExtension, oic))
                    {
                        texture.SaveAsPNG(temp.Path);
                    }
                    else
                    {
                        texture.SaveAsJPEG(temp.Path);
                    }
                }
                finally
                {
                    texture.Release();
                }
            }
            d3dtx.WriteD3DTXXML(tempXml.Path);
            task.Increment(1);
            ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)} and metadata...", cancellationToken);
            bool savedXml = false;
            try
            {
                SaveResult(tempXml, xmlFile, cancellationToken);
                savedXml = true;
                SaveResult(temp, output, cancellationToken);
            }
            catch
            {
                if (savedXml)
                {
                    try
                    {
                        File.Delete(xmlFile);
                    }
                    catch (Exception ex)
                    {
                        SimpleAnsiConsole.ShowWarring($"I couldn't remove the incomplete metadata \"{xmlFile}\": {ex.Message}");
                    }
                }
                throw;
            }
        }
        task.Increment(1);
    }

    #endregion

    #region Font

    private static void ProcessFont(string input, string output, bool import, Games game, ProgressContext ctx, ProgressTask task, CancellationToken cancellationToken)
    {
        string inputFileName = Path.GetFileName(input);
        ReportProgress(ctx, task, $"Reading {inputFileName}...", cancellationToken);
        TFont font;
        using TempFile temp = new(output);
        if (import)
        {
            BmpFontFile bmpFont = BmpFontLoader.LoadFromFile(input);
            task.Increment(1);
            task.MaxValue = bmpFont.Pages.Count + 3;
            ReportProgress(ctx, task, "Importing font glyphs and DDS atlases...", cancellationToken);
            font = ImportFont(bmpFont, input, game, ctx, task, cancellationToken);
            ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)}...", cancellationToken);
            font.WriteTFont(temp.Path);
        }
        else
        {
            font = new(game: game);
            font.ReadTFont(input);
            task.Increment(1);
            task.MaxValue = font.mTexturePages.Count + 3;
            ReportProgress(ctx, task, "Exporting font glyphs...", cancellationToken);
            BmpFontFile bmpFont = BmpFontFile.FromTFont(font);
            string fontName = Path.GetFileNameWithoutExtension(output);
            DirectoryInfo outputFntsDir = Directory.CreateDirectory(
                Path.GetDirectoryName(output) ?? throw new DirectoryNotFoundException("The output folder isn't valid? How's that even possible? I couldn't even confirm whether the output folder for your font got created or not!"));

            for (int i = 0; i < bmpFont.Pages.Count; ++i)
            {
                bmpFont.Pages[i].File = $"{fontName}_{i}{Formats.ddsExtension}";
                CheckOutput(Path.Combine(outputFntsDir.FullName, bmpFont.Pages[i].File));
            }
            BmpFontFile.SerializeToXml(bmpFont, temp.Path);
            task.Increment(1);
            List<TempFile> tempAtlases = [];
            List<string> savedAtlases = [];
            try
            {
                for (int i = 0; i < font.mTexturePages.Count; ++i)
                {
                    ReportProgress(ctx, task, $"Exporting atlas {i}: {bmpFont.Pages[i].File}", cancellationToken);
                    DDS_Master dds = new(font.mTexturePages[i]);
                    TempFile tempAtlas = new(bmpFont.Pages[i].File);
                    tempAtlases.Add(tempAtlas);
                    File.WriteAllBytes(tempAtlas.Path, dds.GetData(font.mTexturePages[i]));
                    task.Increment(1);
                    ctx.Refresh();
                }
                for (int i = 0; i < tempAtlases.Count; ++i)
                {
                    string atlas = Path.Combine(outputFntsDir.FullName, bmpFont.Pages[i].File);
                    ReportProgress(ctx, task, $"Saving atlas {i}: {bmpFont.Pages[i].File}", cancellationToken);
                    SaveResult(tempAtlases[i], atlas, cancellationToken);
                    savedAtlases.Add(atlas);
                }
                ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)}...", cancellationToken);
                SaveResult(temp, output, cancellationToken);
            }
            catch
            {
                //These are only the atlases we just created. Existing files never get into this list.
                foreach (string atlas in savedAtlases)
                {
                    try
                    {
                        File.Delete(atlas);
                    }
                    catch (Exception ex)
                    {
                        SimpleAnsiConsole.ShowWarring($"I couldn't remove the incomplete atlas \"{atlas}\": {ex.Message}");
                    }
                }
                throw;
            }
            finally
            {
                foreach (TempFile tempAtlas in tempAtlases)
                {
                    tempAtlas.Dispose();
                }
            }
            task.Increment(1);
            return;
        }
        SaveResult(temp, output, cancellationToken);
        task.Increment(1);
    }

    #endregion

    private static void ShowAbout()
    {
        Table commandsTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.SlateBlue1)
            .Title("[bold blue] Commands & Info [/]")
            .AddColumn(new TableColumn("[bold]Item[/]").NoWrap())
            .AddColumn(new TableColumn("[bold]Description[/]"));

        (string Item, string Desc)[] commandsRows = new (string Item, string Desc)[]
        {
        ("[cyan]github[/]",  $"[bold green underline]{Config.GithubPage}[/]"),
        ("[cyan]landb[/]",   "export .landb to XML, then import your translated XML back to .landb."),
        ("[cyan]font[/]",    "export .font to .fnt (XML) and DDS atlases, then import .fnt back to .font."),
        ("[cyan]ttf[/]",     "import a .ttf with a chosen font size; bmfont64.exe generates .fnt files into a folder, then the tool converts them to .font."),
        ("[cyan]d3dtx[/]",   "export .d3dtx to DDS, PNG or JPEG with .d3dtx.xml metadata, then import the image and its metadata back to .d3dtx."),
        ("[grey]info[/]",    "[grey]D3dtx batch export uses DDS. Batch import reads DDS, PNG and JPEG from the input folder and each texture's subfolder.[/]"),
        ("[orange1]warn[/]", "[orange1]Font import needs Compressonator CLI 4.5.52.[/] Set [bold]CompressonatorPath[/] in appsettings.json!"),
        ("[orange1]warn[/]", "[orange1]TTF import needs bmfont64.exe and --glyphSize <size>.[/] Set [bold]BmfontPath64[/] in appsettings.json!"),
        ("[grey]info[/]",    "[grey]Use --batch for folders and --relative to include subfolders and keep their paths.[/]"),
        ("[grey]game[/]",    "[grey]Use --game or -g with WDS3NF or 0. Default: WDS3NF. Other games aren't supported yet.[/]"),
        ("[grey]info[/]",    "[grey]Existing output files are kept. Choose a new output path if you've already exported or imported them.[/]"),
        };

        AnsiConsole.Live(commandsTable)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Start(ctx =>
            {
                foreach ((string item, string desc) in commandsRows)
                {
                    _ = commandsTable.AddRow(item, desc);
                    ctx.Refresh();
                    Thread.Sleep(70);
                }
            });

        AnsiConsole.WriteLine();

        Table creditsTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.MediumPurple)
            .Title("[bold magenta] Credits & Thanks [/]")
            .ShowRowSeparators()
            .AddColumn(new TableColumn("[bold]Project[/]").NoWrap())
            .AddColumn(new TableColumn("[bold]Note[/]"));

        (string Project, string Note)[] creditsRows = new (string Project, string Note)[]
        {
        ("[green]Telltale Texture Tool[/]",
         "Built on top of [link=https://github.com/Telltale-Modding-Group/Telltale-Texture-Tool][green]Telltale Texture Tool[/][/] [grey]—[/] but I've [magenta]reshaped[/] a few bits along the way to fit my needs."),
        ("[green]TelltaleInspector[/]",
         "All file analysis was done by [cyan]poking around[/] with [link=https://github.com/LucasSaragosa/TelltaleInspector][green]TelltaleInspector[/][/] [grey]—[/] [magenta]huge thanks[/] to that project!"),
        ("[green]Compressonator[/]",
         "Without [link=https://github.com/GPUOpen-Tools/compressonator][green]Compressonator[/][/] this tool is [red]pretty much useless![/] The whole point is [orange1]targeting Android[/], and that's where [magenta]the magic happens[/]."),
        ("[green]bmfont64[/]",
         "To save myself [grey]the headache[/] of implementing [orange1]TTF parsing[/] from scratch, I leaned on [link=https://www.angelcode.com/products/bmfont/][green]bmfont64[/][/]. It's [grey]not mandatory[/] here [grey]—[/] but the input format is always [cyan]FNT (XML flavor)[/], which bmfont64 generates for you."),
        ("[green]@ByteAndPlay[/]",
         "All of these tools are also downloadable from my Telegram channel: [link=https://t.me/ByteAndPlay][green]@ByteAndPlay[/][/]."),
        };

        AnsiConsole.Live(creditsTable)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Start(ctx =>
            {
                foreach ((string project, string note) in creditsRows)
                {
                    _ = creditsTable.AddRow(project, note);
                    ctx.Refresh();
                    Thread.Sleep(120);
                }
            });
    }
}
