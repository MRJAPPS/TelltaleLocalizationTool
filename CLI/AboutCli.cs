using Spectre.Console;
using TelltaleLocalizationTool.Helper;

namespace TelltaleLocalizationTool.CLI;

internal static class AboutCli
{
    internal static void ShowAbout()
    {
        Table commandsTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.SlateBlue1)
            .Title("[bold blue] Commands & Info [/]")
            .AddColumn(new TableColumn("[bold]Item[/]").NoWrap())
            .AddColumn(new TableColumn("[bold]Description[/]"));

        (string Item, string Desc)[] commandsRows =
        [
        ("[cyan]github[/]",  $"[bold green underline]{Config.GithubPage}[/]"),
        ("[cyan]landb[/]",   "export .landb to XML, then import your translated XML back to .landb."),
        ("[cyan]font[/]",    "export .font to .fnt (XML) and DDS atlases, then import .fnt back to .font."),
        ("[cyan]test_font[/]", "Recursively creates same-named test .font files for input .font files, using TestFntFilePath; each file maps one sequential character to every glyph."),
        ("[cyan]ttf[/]",     "import a .ttf with a chosen font size; bmfont64.exe generates .fnt files into a folder, then the tool converts them to .font."),
        ("[cyan]d3dtx[/]",   "export .d3dtx to DDS, PNG, JPEG, BMP, TIFF (.tif), TGA or HDR with .d3dtx.xml metadata, then import the image and its metadata back to .d3dtx."),
        ("[grey]info[/]",    "[grey]D3dtx batch export uses DDS. Batch import reads all supported image formats from the input folder and each texture's subfolder.[/]"),
        ("[grey]format[/]",  "[grey]WDS1 import automatically selects ETC1 for opaque images, ARGB4 for exact 4-bit RGBA, or ARGB8. Override with --format ETC1, ARGB8 or ARGB4. ETC1 discards alpha.[/]"),
        ("[orange1]warn[/]", "[orange1]Font import needs Compressonator CLI 4.5.52 (tested version) or later.[/] Set [bold]CompressonatorPath[/] in appsettings.json!"),
        ("[orange1]warn[/]", "[orange1]TTF import needs bmfont64.exe and --glyphSize <size>.[/] Set [bold]BmfontPath64[/] in appsettings.json!"),
        ("[grey]info[/]",    "[grey]Use --batch for folders and --relative to include subfolders and keep their paths.[/]"),
        ("[grey]game[/]",    "[grey]Use --game or -g with WDS3NF (0) or WDS1 (1). WDS1 supports d3dtx version 4.[/]"),
        ("[grey]info[/]",    "[grey]Existing output files are kept. Choose a new output path if you've already exported or imported them or use --force flag![/]"),
        ];

        ShowRows(commandsTable, commandsRows, 70);

        AnsiConsole.WriteLine();

        Table creditsTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.MediumPurple)
            .Title("[bold magenta] Credits & Thanks [/]")
            .ShowRowSeparators()
            .AddColumn(new TableColumn("[bold]Project[/]").NoWrap())
            .AddColumn(new TableColumn("[bold]Note[/]"));

        (string Project, string Note)[] creditsRows =
        [
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
        ];

        ShowRows(creditsTable, creditsRows, 120);
    }

    private static void ShowRows(Table table, (string Item, string Desc)[] rows, int delay)
    {
        AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Start(ctx =>
            {
                foreach ((string item, string desc) in rows)
                {
                    _ = table.AddRow(item, desc);
                    ctx.Refresh();
                    Thread.Sleep(delay);
                }
            });
    }
}
