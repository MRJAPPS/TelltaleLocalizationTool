using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

public abstract class ReportBase
{
    #region Public Properties

#if DEBUG
    // When debugging, start with reporting ON so we don't miss the logs.
    public static bool ReportMode { get; set; } = Debugger.IsAttached && false;
#else
    public static bool ReportMode { get; set; }
#endif

    public static StreamWriter reportStream { get; set; } = new(Console.OpenStandardOutput());


    #endregion

    #region Public Methods

    public void Report(ITelltaleObject parsable, bool write = false, string extra = "")
    {
        if (!ReportMode)
            return;

        Type type = parsable.GetType();
        string typeName = type.Name;

        // We absolutely need that description for the report format – missing it means the type wasn't set up right.
        DescriptionAttribute description = type.GetCustomAttribute<DescriptionAttribute>()
            ?? throw new NullReferenceException(
                $"Did you forget to add {nameof(DescriptionAttribute)} to type {typeName}?");

        string message =
            $"{(write ? "W" : "R")} \"{{[{typeName}], [0X{parsable.Position:X}], [{parsable.Size:X}({parsable.Size})] }}\" " +
            $"=> \"[{description}]\" {extra}{Environment.NewLine}";

        reportStream.WriteLine(message);
        reportStream.Flush(); // Make sure the log line appears immediately, even if the stream buffers.
    }

    #endregion
}