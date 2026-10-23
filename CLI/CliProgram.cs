using Spectre.Console;
using Spectre.Console.Cli;
using System.Xml;
using TelltaleLocalizationTool.BitmapFont;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Convert;
using TelltaleLocalizationTool.Telltale.FileTypes.Font;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB;
using TelltaleLocalizationTool.Telltale.Graphics.DDS;

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
                    CheckArgs(settings);
                    PrepareConfiguration();
                    return ProcessFiles(settings, cancellationToken);
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

    private static void CheckArgs(Options settings)
    {
        if (settings.FileType is not Options.FileTypes.c_landb and not Options.FileTypes.c_font)
        {
            throw new ArgumentException($"Enter a valid file type! We need {Options.FileTypes.c_landb} or {Options.FileTypes.c_font}.");
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
            CheckExtension(settings.Input, GetExtension(settings, true));
            CheckExtension(settings.Output, GetExtension(settings, false));
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

        if (settings.FileType == Options.FileTypes.c_font)
        {
            string fontDirectory = relativeDirectory != "." &&
                (settings.Mode == Options.Modes.c_import || Path.GetFileName(relativeDirectory).Equals(relativeName, oic))
                ? Path.Combine(settings.Output, relativeDirectory)
                : Path.Combine(settings.Output, relativeDirectory, relativeName);
            return Path.Combine(fontDirectory, relativeName + GetExtension(settings, false));
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
            _ => throw new NotSupportedException($"We only support {Formats.landbExtension}({Formats.xmlExtension}) and {Formats.fontExtension}({Formats.fntExtension}) here!")
        };
    }

    private static int ProcessFiles(Options settings, CancellationToken cancellationToken)
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
                    if (settings.Mode == Options.Modes.c_import &&
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
                    throw new FileNotFoundException($"I couldn't find any {GetExtension(settings, true)} files in your input folder!");
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
                                ProcessLandb(input, output, settings.Mode == Options.Modes.c_import, ctx, current, cancellationToken);
                                break;
                            case Options.FileTypes.c_font:
                                ProcessFont(input, output, settings.Mode == Options.Modes.c_import, ctx, current, cancellationToken);
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

    private static void ProcessLandb(string input, string output, bool import, ProgressContext ctx, ProgressTask task, CancellationToken cancellationToken)
    {
        Landb landb = new(rMsv6Header: !import);
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

    private static TFont ImportFont(BmpFontFile bmpFont, string input, ProgressContext ctx, ProgressTask task, CancellationToken cancellationToken)
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
        TFont font = new()
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
            font.mTexturePages.Add(Converter.ConvertDdsToD3dtx(tempAtlas.Path, Path.GetFileNameWithoutExtension(input)));
            task.Increment(1);
            ctx.Refresh();
        }
        return font;
    }

    private static void ProcessFont(string input, string output, bool import, ProgressContext ctx, ProgressTask task, CancellationToken cancellationToken)
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
            font = ImportFont(bmpFont, input, ctx, task, cancellationToken);
            ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)}...", cancellationToken);
            font.WriteTFont(temp.Path);
        }
        else
        {
            font = new();
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
        AnsiConsole.Write(new Rule("[blue]TelltaleLocalizationTool[/]"));
        SimpleAnsiConsole.ShowInfo(Config.GithubPage);
        SimpleAnsiConsole.ShowInfo("landb: export .landb to XML, then import your translated XML back to .landb.");
        SimpleAnsiConsole.ShowInfo("font: export .font to .fnt (XML) and DDS atlases, then import .fnt back to .font.");
        SimpleAnsiConsole.ShowInfo("Font import needs Compressonator CLI 4.5.52. Set CompressonatorPath in appsettings.json!");
        SimpleAnsiConsole.ShowInfo("Use --batch for folders and --relative to include subfolders and keep their paths.");
        SimpleAnsiConsole.ShowInfo("Existing output files are kept. Choose a new output path if you've already exported or imported them.");
    }
}
