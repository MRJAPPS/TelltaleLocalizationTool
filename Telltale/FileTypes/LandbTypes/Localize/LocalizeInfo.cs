using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;

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

    public uint Flags { get; set; }

    public uint Size => SizeFieldBytes + sizeof(uint);

    public long Position { get; private set; }

    #endregion

    #region Implementation

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