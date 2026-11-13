using Spectre.Console.Cli;
using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Font.Enums;
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

    [CommandOption("-g|--game <game>", true)]
    [Description("The target game name or enum value.")]
    public string Game { get; set; } = string.Empty;

    [CommandOption("--glyphSize <size>", false)]
    [Description("Required for TTF import. Sets the glyph size in whole pixels; bmfont64.exe must be available.")]
    public float GlyphSize { get; set; } = -1F;//-1 IS NOT VALID, USER MUST GIVE US A SIZE!
}
