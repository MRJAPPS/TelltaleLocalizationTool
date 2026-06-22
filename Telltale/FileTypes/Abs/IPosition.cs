namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

public interface IPosition
{
    /// <summary>
    /// Gets the stream position where the parsing/writing started. Super handy for debug reports.
    /// </summary>
    long Position { get; }
}
