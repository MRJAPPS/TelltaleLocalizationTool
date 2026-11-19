using Spectre.Console;
using TelltaleLocalizationTool.Helper;

namespace TelltaleLocalizationTool.CLI;

internal static class CommonCli
{
    internal const StringComparison oic = StringComparison.OrdinalIgnoreCase;

    internal static void PrepareConfiguration()
    {
        string currentDirectoryConfig = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        string applicationDirectoryConfig = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(currentDirectoryConfig) && File.Exists(applicationDirectoryConfig))
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        }
    }

    internal static void CheckExtension(string fileName, string extension)
    {
        if (!Path.GetExtension(fileName).Equals(extension, oic))
        {
            throw new FormatException($"We need a {extension} file here! \"{fileName}\"");
        }
    }

    internal static void CheckOutput(string output, bool force = false)
    {
        if (Directory.Exists(output) || (File.Exists(output) && !force))
        {
            throw new IOException($"\"{output}\" already exists! Choose another output path.");
        }
    }

    internal static string GetOutputPath(Options settings, string input)
    {
        if (!settings.Batch)
        {
            return settings.Output;
        }

        string relativeDirectory = Path.GetRelativePath(
            settings.Input,
            Path.GetDirectoryName(input) ?? settings.Input);
        string relativeName = Path.GetFileNameWithoutExtension(input);

        if (settings.FileType is Options.FileTypes.c_font or Options.FileTypes.c_d3dtx or Options.FileTypes.c_ttf)
        {
            string fileDir = relativeDirectory != "." &&
                (settings.Mode == Options.Modes.c_import || settings.Mode == Options.Modes.c_generateTestFonts || Path.GetFileName(relativeDirectory).Equals(relativeName, oic))
                ? Path.Combine(settings.Output, relativeDirectory)
                : Path.Combine(settings.Output, relativeDirectory, relativeName);
            return Path.Combine(fileDir, relativeName + GetExtension(settings, false));
        }

        string relativeFile = Path.GetRelativePath(settings.Input, input);
        return Path.Combine(settings.Output, Path.ChangeExtension(relativeFile, GetExtension(settings, false)));
    }

    internal static string GetExtension(Options settings, bool input)
    {
        if (settings.Mode == Options.Modes.c_generateTestFonts)
        {
            return Formats.fontExtension;
        }

        bool binary = settings.Mode == Options.Modes.c_export ? input : !input;
        return settings.FileType switch
        {
            Options.FileTypes.c_landb => binary ? Formats.landbExtension : Formats.xmlExtension,
            Options.FileTypes.c_font => binary ? Formats.fontExtension : Formats.fntExtension,
            Options.FileTypes.c_ttf => input ? Formats.ttfExtension : Formats.fontExtension,
            Options.FileTypes.c_d3dtx => binary ? Formats.d3dtxExtension : settings.Batch ? Formats.ddsExtension :
                Path.GetExtension(settings.Mode == Options.Modes.c_import ? settings.Input : settings.Output).ToLowerInvariant(),
            _ => throw new NotSupportedException($"We only support {Formats.landbExtension}({Formats.xmlExtension}), {Formats.fontExtension}({Formats.fntExtension}) and {Formats.d3dtxExtension}({Formats.ddsExtension},{Formats.pngExtension},{Formats.jpegExtension}) here!")
        };
    }

    internal static void ReportProgress(ProgressContext ctx, ProgressTask task, string message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        task.Description = Markup.Escape(message);
        SimpleAnsiConsole.ShowInfo(message);
        ctx.Refresh();
    }

    internal static void SaveResult(TempFile temp, string output, bool force, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        File.Copy(temp.Path, output, force);
    }

    internal static string GetErrorMessage(Exception error)
    {
        while (error is TypeInitializationException && error.InnerException is not null)
        {
            error = error.InnerException;
        }
        return error.Message;
    }

    internal static void RemoveIncompleteOutput(string path, string description)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex)
        {
            SimpleAnsiConsole.ShowWarring($"I couldn't remove the incomplete {description} \"{path}\": {ex.Message}");
        }
    }
}
