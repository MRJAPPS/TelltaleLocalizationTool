using Spectre.Console.Cli;
using TelltaleLocalizationTool.Telltale.Meta.Enums;

namespace TelltaleLocalizationTool.CLI;

public class CliProgram : Command<Options>
{
    public override int Execute(CommandContext context, Options settings, CancellationToken cancellationToken)
    {
        try
        {
            settings.Mode = settings.Mode.ToLowerInvariant();
            settings.FileType = settings.FileType.ToLowerInvariant();
            CommonCli.PrepareConfiguration();
            switch (settings.Mode)
            {
                case Options.Modes.c_generateTestFonts:
                case Options.Modes.c_export:
                case Options.Modes.c_import:
                    Games game = CliArguments.Prepare(settings);
                    return BatchCli.ProcessFiles(settings, game, cancellationToken);

                case Options.Modes.c_about:
                    AboutCli.ShowAbout();
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
            SimpleAnsiConsole.ShowError(CommonCli.GetErrorMessage(ex));
            return 1;
        }
    }
}
