using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LnagRes.Text;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LnagRes;

/// <summary>
/// Processes the game text resources – subtitles, menu strings, and other wordy things.
/// Wraps a <see cref="TextCollection"/> with a header that's mostly fluff.
/// </summary>
[Description("Processing the game text stuff (subtitles, menus, etc.)")]
public class LanguageResources : ReportBase, IParsable
{
    #region Private Fields

    #endregion

    #region Public Properties

    /// <summary>
    /// Total byte size of this whole LanguageResources block.
    /// Comes entirely from the TextItems inside – we don't add any extra fat here.
    /// </summary>
    public uint Size => TextItems.Size;

    /// <summary>
    /// The actual collection of text chunks – all the localised strings in one place.
    /// </summary>
    public TextCollection TextItems { get; set; } = [];

    /// <summary>
    /// Where the reader's head was when we began parsing.
    /// </summary>
    public long Position { get; private set; }

    #endregion

    #region Implementation

    /// <summary>
    /// Reads the LanguageResources from the stream: skips a redundant capacity int,
    /// then parses the text collection.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // The first int is just a size that we can calculate ourselves from the TextItems.
        // So we read it and throw it away – it's basically a byte version of a ditto mark.
        _ = reader.ReadInt32();

        TextItems.Parse(reader);
        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);
        TextItems.Write(writer);

        Report(this, true);
    }

    #endregion
}