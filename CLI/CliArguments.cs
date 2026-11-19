using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using static TelltaleLocalizationTool.CLI.CommonCli;

namespace TelltaleLocalizationTool.CLI;

internal static class CliArguments
{
    internal static Games Prepare(Options settings)
    {
        Games game = GetGame(settings.Game);
        if (settings.Mode == Options.Modes.c_generateTestFonts)
        {
            settings.FileType = Options.FileTypes.c_font;
            settings.Relative = true;
            settings.Batch = true;
        }
        CheckArgs(settings, game);
        return game;
    }

    private static Games GetGame(string game)
    {
        game = game.Trim().ToUpper();
        return game.Contains(',') || !Enum.TryParse(game, true, out Games value) || !Enum.IsDefined(value)
            ? throw new ArgumentException($"I can't use game \"{game}\"! Use a name or number from Games: {string.Join(", ", Enum.GetValues<Games>().Select(item => $"{item} ({(int)item})"))}.")
            : value;
    }

    private static void CheckArgs(Options settings, Games game)
    {
        if (!string.IsNullOrWhiteSpace(settings.Format))
        {
            if (game != Games.WDS1 || settings.FileType != Options.FileTypes.c_d3dtx || settings.Mode != Options.Modes.c_import)
            {
                throw new ArgumentException($"--format is only used for WDS1 d3dtx import({nameof(T3SurfaceFormat.ARGB8)},{nameof(T3SurfaceFormat.ARGB4)},{nameof(T3SurfaceFormat.ETC1_RGB)})!");
            }
            _ = D3dtxCli.GetSurfaceFormat(settings.Format);
        }
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
        if (settings.FileType != Options.FileTypes.c_ttf && settings.ReferenceFnt != null)
        {
            throw new ArgumentException("--reference-fnt is only used for TTF import!");
        }
        if (settings.FallbackTtf != null && settings.FileType != Options.FileTypes.c_ttf)
        {
            throw new ArgumentException("--fallback-ttf is only used for TTF import!");
        }
        if (settings.ReferenceFont != null && (settings.FileType != Options.FileTypes.c_ttf || game != Games.WDS1 || settings.ReferenceFnt != null))
        {
            throw new ArgumentException("--reference-font requires WDS1 TTF import and replaces --reference-fnt!");
        }
        if ((settings.ReferenceFnt != null || settings.ReferenceFont != null) && settings.Batch)
        {
            throw new ArgumentException("A reference font requires a single TTF input!");
        }

        CheckPaths(settings);
        if (settings.FileType is not Options.FileTypes.c_landb && string.Join(" ", Environment.GetCommandLineArgs()).Contains("--text_mode"))
        {
            throw new ArgumentException("You can only use text-mode when you're importing or exporting one or more landb files!");
        }
    }

    private static void CheckPaths(Options settings)
    {
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
                D3dtxCli.CheckOutputTexFormat(import ? settings.Input : settings.Output);
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
}
