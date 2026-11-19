using Spectre.Console.Cli;
using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Font.Enums;
using TelltaleLocalizationTool.Telltale.FileTypes.Ldb.LandbTypes.Enum;
using TelltaleLocalizationTool.Telltale.Meta.Enums;

namespace TelltaleLocalizationTool.CLI;

public class Options : CommandSettings
{
    public static class Modes
    {
        public const string c_export = "export";
        public const string c_import = "import";
        public const string c_about = "about";
    }
    [CommandArgument(0, "<mode>")]
    [Description($"The mode you want to use [green]{Modes.c_export}[/] OR [green]{Modes.c_import}[/] OR [blue]about[/]")]
    public string Mode { get; set; } = string.Empty;

    public static class FileTypes
    {
        public const string c_landb = "landb";
        public const string c_font = "font";
        public const string c_d3dtx = "d3dtx";
        public const string c_ttf = "ttf";//Input only!
    }

    [CommandArgument(1, "[type]")]
    [Description($"The file(s) type: [green]{FileTypes.c_landb}[/], [green]{FileTypes.c_font}[/], [green]{FileTypes.c_d3dtx}[/] or [green]{FileTypes.c_ttf}[/] (import only). Not needed for about.")]
    public string FileType { get; set; } = string.Empty;

    [CommandOption("-i|--input <inputPath>", false)]
    [Description("The input file or directory based on --batch flag")]
    public string Input { get; set; } = string.Empty;


    [CommandOption("-o|--output <outputPath>", false)]
    [Description("The output file or directory based on --batch flag")]
    public string Output { get; set; } = string.Empty;


    [CommandOption("-b|--batch", false)]
    [Description("If you use the this flag, the input and output must be directories.")]
    public bool Batch { get; set; } = false;

    [CommandOption("-r|--relative", false)]
    [Description("This flag must only be used with -b or --batch. When enabled, it recursively reads all files from the input folder and creates output files or folders relative to the input folder.")]
    public bool Relative { get; set; } = false;

    [CommandOption("-f|--force", false)]
    [Description("[red]Overwrite existing output files.[/]")]
    public bool Force { get; set; } = false;

    [CommandOption("-g|--game <game>", false)]
    [Description("The target game name or enum value.")]
    public string Game { get; set; } = string.Empty;

    [CommandOption("--format <format>", false)]
    [Description("WDS1 d3dtx import format: auto (default), ETC1, ARGB8 or ARGB4. Auto checks alpha and 4-bit channel precision in every mip.")]
    public string Format { get; set; } = string.Empty;

    [CommandOption("--glyphSize <size>", false)]
    [Description("Required for TTF import. Sets the glyph size in whole pixels; bmfont64.exe must be available.")]
    public float GlyphSize { get; set; } = -1F;//-1 IS NOT VALID, USER MUST GIVE US A SIZE!

    [CommandOption("--fallback-ttf <path>")]
    [Description("Use this TTF for glyphs missing from the primary TTF.")]
    public string? FallbackTtf { get; set; }

    [CommandOption("--reference-fnt <path>")]
    [Description("Preserve the face name, font height and base from an exported FNT when replacing a font.")]
    public string? ReferenceFnt { get; set; }

    [CommandOption("--reference-font <path>")]
    [Description("Preserve WDS1 rendering settings from the original game font when importing a TTF.")]
    public string? ReferenceFont { get; set; }

    [CommandOption("--text_transform")]
    [Description($"Using this flag, you can control how texts are read or converted from Landb XMLs. Options: {nameof(TextTransformMode.shaped)}, {nameof(TextTransformMode.logical)}, {nameof(TextTransformMode.none)} (default: {nameof(TextTransformMode.Def)}). For more information, check the docs and code on [link=https://github.com/MRJAPPS/TelltaleLocalizationTool]Github[/].")]
    public TextTransformMode TextTransform { get; set; } = TextTransformMode.Def;

}
