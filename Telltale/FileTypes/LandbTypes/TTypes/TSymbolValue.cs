using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

/// <summary>
/// Reads a single 8‑byte symbol value from the binary stream.
/// Tiny but mighty – it's like a little data potato chip.
/// </summary>
[Description("Processing a SymbolValue(8 byte)")]
public class TSymbolValue : ReportBase, IParsable
{
    #region Private Fields

    #endregion

    #region Public Properties

    /// <summary>
    /// The actual 8‑byte value that was read. Could be anything, we don't judge.
    /// </summary>
    public ulong Value { get; set; }

    /// <summary>
    /// Always 8 bytes. Because a ulong is exactly 8 bytes – duh.
    /// </summary>
    public uint Size => sizeof(ulong);

    /// <summary>
    /// The stream position when parsing kicked off. Used by the report to show where this chunk lived.
    /// </summary>
    public long Position { get; private set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Grabs a ulong from the reader, remembers the position, and brags about it via a report.
    /// </summary>
    /// <param name="reader">The binary reader pointing right at the 8‑byte value.</param>
    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        Value = reader.ReadUInt64();
        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;
        writer.Write(Value);
        Report(this, true);
    }

    #endregion
}