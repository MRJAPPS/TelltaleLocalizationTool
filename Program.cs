using Spectre.Console.Cli;
using TelltaleLocalizationTool.CLI;
using TelltaleLocalizationTool.Helper;

namespace TelltaleLocalizationTool;

public static class Program
{
    public static Config Config { get; } = new();

    private static void InitEncoding()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;
    }

    public static int Main(string[] args)
    {
        InitEncoding();
        CommandApp<CliProgram> app = new();
        return app.Run(args);
    }
}
