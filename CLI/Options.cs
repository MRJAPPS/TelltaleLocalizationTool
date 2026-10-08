using Spectre.Console.Cli;
using System.ComponentModel;

namespace TelltaleLocalizationTool.CLI;

public class Options : CommandSettings
{
    public static class Modes
    {
        public const string c_export = "export";
        public const string c_import = "import";
    }
    [CommandArgument(0, "<mode>")]
    [Description($"The mode you want to use [green]{Modes.c_export}[/] OR [green]{Modes.c_import}[/] OR [blue]about[/]")]
    public string Mode { get; set; } = string.Empty;

    public static class FileTypes
    {
        public const string c_landb = "landb";
        public const string c_font = "font";
    }

    [CommandArgument(1, "<type>")]
    [Description($"The file(s) type that you want to use [green]{Modes.c_export}[/] OR [green]{Modes.c_import}[/]. You can use [green]{FileTypes.c_landb}[/] OR [green]{FileTypes.c_font}[/]")]
    public string FileType { get; set; } = string.Empty;

    [CommandOption("-i|--input <inputPath>", true)]
    [Description("The input file or directory based on --batch flag")]
    public string Input { get; set; } = string.Empty;


    [CommandOption("-o|--output <outputPath>", true)]
    [Description("The output file or directory based on --batch flag")]
    public string Output { get; set; } = string.Empty;


    [CommandOption("-b|--batch", false)]
    [Description("If you use the this flag, the input and output must be directories. [green]default: false[/]")]
    public bool Batch { get; set; } = false;

    [CommandOption("-r|--relative", false)]
    [Description("This flag must only be used with -b or --batch. When enabled, it recursively reads all files from the input folder and creates output files or folders relative to the input folder. [green]default: false[/]")]
    public bool Relative { get; set; } = false;
}
