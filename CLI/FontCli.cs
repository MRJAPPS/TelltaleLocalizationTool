using Spectre.Console;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.BitmapFont;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Convert;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.Font;
using TelltaleLocalizationTool.Telltale.FileTypes.Font.Glyph;
using TelltaleLocalizationTool.Telltale.Graphics.DDS;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using static TelltaleLocalizationTool.CLI.CommonCli;

namespace TelltaleLocalizationTool.CLI;

internal static class FontCli
{
    internal static void ProcessFont(string input, string output, bool import, Games game, ProgressContext ctx, ProgressTask task, bool force, CancellationToken cancellationToken)
    {
        string inputFileName = Path.GetFileName(input);
        ReportProgress(ctx, task, $"Reading {inputFileName}...", cancellationToken);
        using TempFile temp = new(output);
        if (!import)
        {
            ExportFont(input, output, game, ctx, task, temp, force, cancellationToken);
            return;
        }

        BmpFontFile bmpFont = BmpFontLoader.LoadFromFile(input);
        task.Increment(1);
        task.MaxValue = bmpFont.Pages.Count + 3;
        ReportProgress(ctx, task, "Importing font glyphs and DDS atlases...", cancellationToken);
        TFont font = ImportFont(bmpFont, input, game, ctx, task, cancellationToken);
        ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)}...", cancellationToken);
        font.WriteToFile(temp.Path);
        SaveResult(temp, output, force, cancellationToken);
        task.Increment(1);
    }

    private static void ExportFont(string input, string output, Games game, ProgressContext ctx, ProgressTask task, TempFile temp, bool force, CancellationToken cancellationToken)
    {
        TFont font = new(game: game);
        font.LoadFromFile(input);
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
            CheckOutput(Path.Combine(outputFntsDir.FullName, bmpFont.Pages[i].File), force);
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
                bool atlasAlreadyExisted = File.Exists(atlas);
                SaveResult(tempAtlases[i], atlas, force, cancellationToken);
                if (!atlasAlreadyExisted)
                {
                    savedAtlases.Add(atlas);
                }
            }
            ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)}...", cancellationToken);
            SaveResult(temp, output, force, cancellationToken);
        }
        catch
        {
            //These are only the atlases we just created. Existing files never get into this list.
            foreach (string atlas in savedAtlases)
            {
                RemoveIncompleteOutput(atlas, "atlas");
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
    }

    internal static void ProcessTtf(string input, string output, Options settings, Games game, ProgressContext ctx, ProgressTask task, CancellationToken cancellationToken)
    {
        task.IsIndeterminate = true;
        TFont font;
        try
        {
            font = Converter.ConvertTtfToFont(input, settings.GlyphSize, game,
                message => ReportProgress(ctx, task, message, cancellationToken), settings.FallbackTtf, cancellationToken);
        }
        finally
        {
            task.IsIndeterminate = false;
        }
        if (settings.ReferenceFont != null)
        {
            TFont reference = new(game: game);
            reference.LoadFromFile(settings.ReferenceFont);
            font.ApplyWds1Reference(reference);
        }
        else if (settings.ReferenceFnt != null)
        {
            BmpFontFile reference = BmpFontLoader.LoadFromFile(settings.ReferenceFnt);
            if (reference.Info is not { Size: > 0 } info || reference.Common is not { } metrics)
            {
                throw new InvalidDataException("The reference FNT needs a positive font height and common metrics!");
            }
            font.mName = info.Face;
            font.mHeight = info.Size;
            font.mBase = metrics.Base;
        }
        if (game == Games.WDS1)
        {
            font.EnsureWds1FallbackGlyphs();
            for (int page = 0; page < font.mTexturePages.Count; page++)
            {
                font.mTexturePages[page].mD3dtxMetaData.mName = $"{Path.GetFileNameWithoutExtension(output)}_{page}";
            }
        }
        task.Value = 2;
        ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)}...", cancellationToken);
        using TempFile temp = new(output);
        font.WriteToFile(temp.Path);
        SaveResult(temp, output, settings.Force, cancellationToken);
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

            font.mGlyphInfos.Add(CreateTGlyphInfo(bmpChar, bmpFont.Common.ScaleW, bmpFont.Common.ScaleH));

        }
        task.Increment(1);
        ImportFontTextures(Path.GetFileNameWithoutExtension(input), game, ctx, task, pages, inputDirectory, font, cancellationToken);
        return font;
    }

    private static GlyphInfo CreateTGlyphInfo(BmpFontChar bmpChar, int scaleW, int scaleH)
    {
        return new GlyphInfo
        {
            mChar = new() { CharCode = (uint)bmpChar.Id },
            mTexturePage = bmpChar.Page,
            mChannel = bmpChar.Channel,
            mRect = Utils.GlyTCalcRect(bmpChar.X, bmpChar.Y, bmpChar.Width, bmpChar.Height, scaleW, scaleH),
            mGlyphMetrics = Utils.GlyTCalcMetrics(bmpChar.XOffset, bmpChar.YOffset, bmpChar.XAdvance),
            mGlyphSize = Utils.GlyTCalcSize(bmpChar.Width, bmpChar.Height)
        };
    }

    private static void ImportFontTextures(string fontName, Games game, ProgressContext ctx, ProgressTask task, List<BmpFontPage> pages, string inputDirectory, TFont font, CancellationToken cancellationToken)
    {
        foreach (BmpFontPage page in pages)
        {
            ReportProgress(ctx, task, $"Importing atlas {page.Id}: {page.File}", cancellationToken);
            using TempFile tempAtlas = new(Path.ChangeExtension(page.File, Formats.ddsExtension));
            File.Copy(Path.Combine(inputDirectory, page.File), tempAtlas.Path, true);
            T3SurfaceFormat fontTextureFormat = Utils.GetDefaultFormat(game);
            ID3DTX d3dtx = Converter.ConvertDdsToD3dtx(tempAtlas.Path, fontName, game, fontTextureFormat);
            font.mTexturePages.Add(d3dtx);
            task.Increment(1);
            ctx.Refresh();
        }
    }

    internal static void ProcessTestFont(int number, string output, Games game, ProgressContext ctx, ProgressTask task, bool force, CancellationToken cancellationToken)
    {
        string fontFileName = Path.GetFileName(output);
        int testChar = number + Program.Config.TestFntBaseCharCode;
        string testFntFilePath = Program.Config.TestFntFilePath;
        string testFntDir = Path.GetDirectoryName(testFntFilePath) ?? throw new NullReferenceException($"How do I get the directory for this path???\n{testFntFilePath}");

        ReportProgress(ctx, task, $"Genrating test for {fontFileName}... ({testChar}, \"{(char)testChar}\")", cancellationToken);
        using TempFile temp = new(output);
        BmpFontFile bmpFont = BmpFontLoader.LoadFromFile(testFntFilePath);
        if (bmpFont.Common is null)
        {
            throw new NullReferenceException($"Your {nameof(Config.TestFntFilePath)} file doesn't include {nameof(bmpFont.Common.ScaleW)} and {nameof(bmpFont.Common.ScaleH)} info!");
        }

        task.Increment(1);
        task.MaxValue = bmpFont.Pages.Count + 3;
        ReportProgress(ctx, task, "Importing font glyphs and DDS atlases...", cancellationToken);
        TFont font = ImportFont(bmpFont, testFntFilePath, game, ctx, task, cancellationToken);
        ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)}...", cancellationToken);

        GlyphInfo ginfo = font.mGlyphInfos.Where(glyph => glyph.mChar.CharCode == number + '0').Single();

        for (int i = 0; i < font.mGlyphInfos.Count; ++i)
        {
            font.mGlyphInfos[i].mTexturePage = ginfo.mTexturePage;
            font.mGlyphInfos[i].mRect = ginfo.mRect;
        }
        font.WriteToFile(temp.Path);

        SaveResult(temp, output, force, cancellationToken);
        task.Increment(1);
    }
}
