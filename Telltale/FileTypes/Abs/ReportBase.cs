using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

/// <summary>
/// Base class for anything that wants to brag about its parsing results.
/// </summary>
public abstract class ReportBase
{
    #region Public Properties

    /// <summary>
    /// Gets or sets whether to actually do the reporting. 
    /// </summary>
#if DEBUG
    // When debugging, start with reporting ON so we don't miss the logs.
    public bool ReportMode { get; set; } = Debugger.IsAttached;
#else
    public bool ReportMode { get; set; }
#endif

    /// <summary>
    /// Where the report text gets dumped. In debug builds, it defaults to the console output.
    /// </summary>
#if DEBUG
    // In debug, we auto-connect to the console so logs show up without extra setup.
    public StreamWriter reportStream { get; set; } = new(Console.OpenStandardOutput());
#else
    // Release build requires someone to provide a stream; otherwise, NullReferenceException is your new best friend.
    public required StreamWriter reportStream { get; set; }
#endif

    #endregion

    #region Public Methods

    /// <summary>
    /// Builds and writes a geeky report line about the given <paramref name="parsable"/> object.
    /// Requires the type to have a <see cref="DescriptionAttribute"/>, or it'll throw a tantrum.
    /// </summary>
    /// <param name="parsable">The freshly parsed chunk to report on.</param>
    /// <param name="extra">Any extra blurb to tack onto the end of the line.</param>
    /// <exception cref="NullReferenceException">
    /// Thrown when someone forgot to slap a <see cref="DescriptionAttribute"/> on the type.
    /// </exception>
    public void Report(IParsable parsable, bool write = false, string extra = "")
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
            $"{(write ? "W" : "R" )} \"{{[{typeName}], [0X{parsable.Position:X}], [{parsable.Size:X}({parsable.Size})] }}\" " +
            $"=> \"[{description}]\" {extra}{Environment.NewLine}";

        reportStream.WriteLine(message);
        reportStream.Flush(); // Make sure the log line appears immediately, even if the stream buffers.
    }

    #endregion
}