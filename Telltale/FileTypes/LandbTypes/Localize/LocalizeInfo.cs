using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;

/// <summary>
/// Holds the localisation info flags – just a 4‑byte value that sits right after a redundant size field.
/// </summary>
[Description("Processing the localize info flag")]
public class LocalizeInfo : ReportBase, IParsable
{
    #region Constants

    // The size field that appears right before the flags – always 4 bytes.
    private const uint SizeFieldBytes = sizeof(uint);

    #endregion

    #region Private Fields

    #endregion

    #region Public Properties

    /// <summary>
    /// The raw flags value. Its meaning is probably buried in some design doc from 2008.
    /// </summary>
    public uint Flags { get; set; }

    /// <summary>
    /// Total byte size of this info block: 4 bytes for the size field (skipped) + 4 bytes for the flags.
    /// </summary>
    public uint Size => SizeFieldBytes + sizeof(uint);

    /// <summary>
    /// The stream offset where parsing started.
    /// </summary>
    public long Position { get; private set; }

    #endregion

    #region Implementation

    /// <summary>
    /// Reads the localisation info from the stream: skips the size field we don’t need,
    /// then grabs the flags.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // This 4‑byte value is just a duplicate of the block size – we already know it’s 4.
        // Read it and toss it so the stream pointer lands on the flags.
        _ = reader.ReadUInt32();

        Flags = reader.ReadUInt32();
        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);
        writer.Write(Flags);

        Report(this, true);
    }

    #endregion
}