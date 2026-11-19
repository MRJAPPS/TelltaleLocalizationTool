using System.Xml;
using Spectre.Console;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Ldb.LandbTypes.Enum;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using static TelltaleLocalizationTool.CLI.CommonCli;

namespace TelltaleLocalizationTool.CLI;

internal static class LandbCli
{
    internal static void ProcessLandb(string input, string output, bool import, Games game, TextTransformMode textTransform, ProgressContext ctx, ProgressTask task, bool force, CancellationToken cancellationToken)
    {
        Landb landb = new(game, textTransform, rMsv6Header: !import);
        XmlDocument doc = new();
        ReportProgress(ctx, task, $"Reading {Path.GetFileName(input)}...", cancellationToken);
        if (import)
        {
            doc.Load(input);
        }
        else
        {
            using FileStream inputStream = new(input, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new(inputStream);
            landb.Parse(reader);
        }
        task.Increment(1);

        ReportProgress(ctx, task, import ? "Importing landb XML..." : "Exporting landb XML...", cancellationToken);
        if (import)
        {
            XmlElement root = doc.DocumentElement ?? throw new InvalidDataException("The input XML file has no elements!");
            XmlElement element = root.GetSingleElement(nameof(Landb));
            landb.Parse(root, element.GetAttribute("name"));
        }
        else
        {
            XmlElement root = doc.CreateElement("Root");
            _ = doc.AppendChild(root);
            landb.Write(root, nameof(landb));
        }
        task.Increment(1);

        ReportProgress(ctx, task, $"Saving {Path.GetFileName(output)}...", cancellationToken);
        using TempFile temp = new(output);
        if (import)
        {
            using FileStream outputStream = new(temp.Path, FileMode.Create, FileAccess.Write);
            using BinaryWriter writer = new(outputStream);
            landb.Write(writer);
        }
        else
        {
            doc.Save(temp.Path);
        }
        SaveResult(temp, output, force, cancellationToken);
        task.Increment(1);
    }
}
