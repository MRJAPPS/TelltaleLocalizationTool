using Spectre.Console;

namespace TelltaleLocalizationTool.CLI;
public static class SimpleAnsiConsole
{
    public static void ShowError(string message, bool markupLineInterpolated = true)
    {
        if (markupLineInterpolated)
            AnsiConsole.MarkupLineInterpolated($"[bold red]✗  Error:[/] {message}");
        else
            AnsiConsole.MarkupLine($"[bold red]✗  Error:[/] {message}");
    }

    public static void ShowInfo(string message, bool markupLineInterpolated = true)
    {
        if (markupLineInterpolated)
            AnsiConsole.MarkupLineInterpolated($"[bold blue]ℹ️ Info:[/] {message}");
        else
            AnsiConsole.MarkupLine($"[bold blue]ℹ️ Info:[/] {message}");
    }

    public static void ShowWarring(string message,bool markupLineInterpolated = true)
    {
        if(markupLineInterpolated)
        AnsiConsole.MarkupLineInterpolated($"[#FFA500 bold]⚠️ Warring:[/] {message}");
        else
            AnsiConsole.MarkupLine($"[#FFA500 bold]⚠️ Warring:[/] {message}");
    }

}
