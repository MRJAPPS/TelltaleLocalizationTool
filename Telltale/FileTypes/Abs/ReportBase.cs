using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

public abstract class ReportBase
{
#if DEBUG
    public bool ReportMode { get; set; } = Debugger.IsAttached;
    public StreamWriter reportStream { get; set; } = new(Console.OpenStandardOutput());
#else
    public bool ReportMode { get; set; }
    public required StreamWriter reportStream {get; set; } 
#endif
    public void Report(IParsable parsable, string extra = "")
    {
        if (ReportMode)
        {
            Type type = parsable.GetType();
            string typeName = type.Name;
            DescriptionAttribute description = type.GetCustomAttribute<DescriptionAttribute>() ?? throw new NullReferenceException($"Did you forget to add {nameof(DescriptionAttribute)} to type {typeName}?");
            string message = $"\"{{[{typeName}], [0X{parsable.ParsePosition:X}], [{parsable.Size:X}({parsable.Size})] }}\" => \"[{description}]\" {extra}{Environment.NewLine}";
            reportStream.WriteLine(message);
            reportStream.Flush();
        }
    }
}
