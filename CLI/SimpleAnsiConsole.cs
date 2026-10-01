using Spectre.Console;

namespace TelltaleLocalizationTool.CLI;
public static class SimpleAnsiConsole
{
    public static void ShowError(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold red]✗  Error:[/] {message}");
    }
    
    public static void ShowInfo(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold blue]ℹ️ Info:[/] {message}");
    }
    
    public static void ShowWarring(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[#FFA500 bold]⚠️ Warring:[/] {message}");
    }

}
