using Spectre.Console;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using static TelltaleLocalizationTool.CLI.CommonCli;

namespace TelltaleLocalizationTool.CLI;

internal static class BatchCli
{
    private static IEnumerable<string> EnumerateImportFiles(
        string inputDirectory,
        bool recursive,
        string[] extensions,
        CancellationToken cancellationToken)
    {
        EnumerationOptions options = new()
        {
            RecurseSubdirectories = true,
            MaxRecursionDepth = recursive ? int.MaxValue : 1,
            IgnoreInaccessible = false,
            AttributesToSkip = FileAttributes.ReparsePoint
        };

        foreach (string file in Directory.EnumerateFiles(inputDirectory, $"*", options))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (extensions.Contains(Path.GetExtension(file)))
            {
                yield return file;
            }
        }
    }

    internal static int ProcessFiles(Options settings, Games game, CancellationToken cancellationToken)
    {
        int success = 0, failed = 0;
        AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new ElapsedTimeColumn(), new SpinnerColumn())
            .Start(ctx =>
            {
                ProgressTask total = ctx.AddTask("Finding input files...");
                total.IsIndeterminate = true;
                List<string> files = FindInputFiles(settings, ctx, total, cancellationToken);

                total.IsIndeterminate = false;
                total.MaxValue = files.Count;
                total.Description = $"{settings.Mode}: 0/{files.Count} file(s)";
                ProgressTask? current = null;
                SimpleAnsiConsole.ShowInfo($"Found {files.Count} file(s). Let's {settings.Mode} them!");
                for (int i = 0; i < files.Count; i++)
                {
                    string input = files[i];
                    cancellationToken.ThrowIfCancellationRequested();
                    string output = GetOutputPath(settings, input);
                    if (current != null)
                    {
                        _ = ctx.RemoveTask(current);
                    }

                    current = ctx.AddTask(Markup.Escape(Path.GetFileName(input)), maxValue: 3);
                    try
                    {
                        CheckOutput(output, settings.Force);
                        SimpleAnsiConsole.ShowInfo($"{settings.Mode}: \"{input}\"");
                        ProcessFile(settings, game, input, output, i, ctx, current, cancellationToken);
                        success++;
                        current.Description = $"[green]Saved:[/] {Markup.Escape(Path.GetFileName(output))}";
                        SimpleAnsiConsole.ShowInfo($"Saved to \"{output}\"");
                    }
                    catch (OperationCanceledException)
                    {
                        current.Description = "[yellow]Cancelled![/]";
                        throw;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        current.Description = $"[red]Failed:[/] {Markup.Escape(Path.GetFileName(input))}";
                        SimpleAnsiConsole.ShowError($"\"{input}\": {GetErrorMessage(ex)}");
                    }
                    finally
                    {
                        current.StopTask();
                        ctx.Refresh();
                    }
                    total.Increment(1);
                    total.Description = $"{settings.Mode}: {success + failed}/{files.Count} file(s)";
                    ctx.Refresh();
                }
                total.Description = failed == 0 ? "[green]All files processed![/]" : "[yellow]Finished with errors![/]";
            });
        SimpleAnsiConsole.ShowInfo($"Done! {success} succeeded, {failed} failed.");
        return failed == 0 ? 0 : 1;
    }

    private static List<string> FindInputFiles(Options settings, ProgressContext ctx, ProgressTask total, CancellationToken cancellationToken)
    {
        List<string> files = [];
        if (settings.Batch)
        {
            if (settings.Mode == Options.Modes.c_generateTestFonts)
            {
                AddBatchFiles(settings, [Formats.fontExtension], ctx, total, files, cancellationToken);//font->testfonts
            }
            else if (settings.Mode == Options.Modes.c_import && settings.FileType == Options.FileTypes.c_d3dtx)
            {
                AddBatchFiles(settings, D3dtxCli.OutputTexExtensions, ctx, total, files, cancellationToken);//image->d3dtx
            }
            else if (settings.Mode == Options.Modes.c_import && settings.FileType == Options.FileTypes.c_font)
            {
                AddBatchFiles(settings, [Formats.fntExtension], ctx, total, files, cancellationToken);//fnt->font
            }
            else
            {
                EnumerationOptions options = new()
                {
                    RecurseSubdirectories = settings.Relative,
                    IgnoreInaccessible = false,
                    AttributesToSkip = FileAttributes.ReparsePoint
                };

                foreach (string file in Directory.EnumerateFiles(
                    settings.Input,
                    $"*{GetExtension(settings, true)}",
                    options))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    files.Add(file);
                    total.Description = $"Found {files.Count} input file(s)";
                    ctx.Refresh();
                }
            }

            files.Sort(StringComparer.OrdinalIgnoreCase);
        }
        else
        {
            files.Add(settings.Input);
        }
        cancellationToken.ThrowIfCancellationRequested();
        if (files.Count == 0)
        {
            string extension = settings.FileType == Options.FileTypes.c_d3dtx && settings.Mode == Options.Modes.c_import
                ? "DDS, PNG, JPEG, BMP, TIFF, TGA or HDR"
                : GetExtension(settings, true);
            throw new FileNotFoundException($"I couldn't find any {extension} files in your input folder!");
        }
        return files;
    }

    private static void ProcessFile(Options settings, Games game, string input, string output, int index, ProgressContext ctx, ProgressTask task, CancellationToken cancellationToken)
    {
        if (settings.Mode.Equals(Options.Modes.c_generateTestFonts, oic))
        {
            FontCli.ProcessTestFont(index, output, game, ctx, task, settings.Force, cancellationToken);
            return;
        }

        bool import = settings.Mode == Options.Modes.c_import;
        switch (settings.FileType)
        {
            case Options.FileTypes.c_landb:
                LandbCli.ProcessLandb(input, output, import, game, settings.TextTransform, ctx, task, settings.Force, cancellationToken);
                break;
            case Options.FileTypes.c_font:
                FontCli.ProcessFont(input, output, import, game, ctx, task, settings.Force, cancellationToken);
                break;
            case Options.FileTypes.c_ttf:
                FontCli.ProcessTtf(input, output, settings, game, ctx, task, cancellationToken);
                break;
            case Options.FileTypes.c_d3dtx:
                D3dtxCli.ProcessD3dtx(input, output, import, game, ctx, task, D3dtxCli.GetSurfaceFormat(settings.Format), settings.Force, cancellationToken);
                break;
        }
    }

    private static void AddBatchFiles(Options settings, string[] extensions, ProgressContext ctx, ProgressTask total, List<string> files, CancellationToken cancellationToken)
    {
        foreach (string file in EnumerateImportFiles(settings.Input, settings.Relative, extensions, cancellationToken))
        {
            files.Add(file);
            total.Description = $"Found {files.Count} input file(s)";
            ctx.Refresh();
        }
    }
}
