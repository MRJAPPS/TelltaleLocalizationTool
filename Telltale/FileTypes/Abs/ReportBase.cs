using System.Diagnostics;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

public abstract class ReportBase
{
#if DEBUG
    public bool ReportMode { get; set; } = Debugger.IsAttached;
#else 
    public bool ReportMode { get; set; };
#endif
    public virtual void Report(StreamWriter writer, string message)
    {
        if (ReportMode)
        {
            writer.WriteLine(message);
        }
    }
}
