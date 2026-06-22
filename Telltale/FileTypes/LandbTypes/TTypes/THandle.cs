using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

/// <summary>
/// A wrapper that pairs a <see cref="TSymbolValue"/> with an extra 4 bytes we totally ignore.
/// </summary>
[Description("Processing THandle(a wrapper for TSymbolValue + 4 byte extra)")]
public class THandle : ReportBase, IParsable
{
    #region Private Fields

    /// <summary>
    /// The stream offset where we started reading – saved for the report.
    /// </summary>
    private long _ParsePos;

    #endregion

    #region Public Properties

    /// <summary>
    /// The symbol value that actually matters. The real payload.
    /// </summary>
    public TSymbolValue hSymbolValue { get; set; } = new();

    /// <summary>
    /// Total byte size: our symbol + that mysterious 4‑byte header chunk.
    /// </summary>
    public uint Size => hSymbolValue.Size + ISize.FOUR_ADD;

    /// <summary>
    /// Where the reader was when we started parsing. For debug bragging.
    /// </summary>
    public long ParsePosition => _ParsePos;

    #endregion

    #region Public Methods

    /// <summary>
    /// Reads a THandle: skips a useless 4‑byte size prefix, then parses the real TSymbolValue.
    /// </summary>
    /// <param name="reader">The binary reader, positioned right at the beginning of this handle.</param>
    public void Parse(BinaryReader reader)
    {
        // Stash the start pos for the report – gotta flex that offset later.
        _ParsePos = reader.BaseStream.Position;

        // Those first 4 bytes are just a duplicate size we can figure out ourselves. Nom them and move on.
        _ = reader.ReadUInt32();

        hSymbolValue.Parse(reader);
        Report(this);
    }

    #endregion
}