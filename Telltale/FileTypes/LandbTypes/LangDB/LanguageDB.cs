using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LangDB;

/// <summary>
/// The main LanguageDB block – a 140‑byte mysterious header, then an Owner, then a Generator.  
/// Together they define the language database structure.
/// </summary>
[Description("Processing Owner and Generator info")]
public class LanguageDB : ReportBase, IParsable
{
    #region Constants

    // This header size (140 bytes) comes from the Telltale format spec. No idea what’s inside,
    // but we need to eat those bytes to keep the stream aligned.
    private const int HeaderSize = 140;

    #endregion

    #region Fields

    // Raw bytes of the header. We don't parse them, just keep a copy.
    private byte[] msv6Header = [];

    #endregion

    #region Properties

    /// <summary>
    /// The Owner part – just a unique ID.
    /// </summary>
    public Owner mOwner { get; set; } = new();

    /// <summary>
    /// The Generator part – the next unique ID counter.
    /// </summary>
    public Generator mGenerator { get; set; } = new();

    /// <summary>
    /// Total byte size: the 140‑byte header + Owner + Generator.
    /// </summary>
    public uint Size => HeaderSize + mOwner.Size + mGenerator.Size;

    /// <summary>
    /// The stream position when parsing began.
    /// </summary>
    public long Position { get; private set; }

    #endregion

    #region Implementation

    /// <summary>
    /// Reads the LanguageDB block: first the fixed‑size header (raw bytes), then the Owner,
    /// then the Generator. All in a straight line.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // Read the whole header blob – maybe one day we'll care what’s inside.
        msv6Header = reader.ReadBytes(HeaderSize);

        mOwner.Parse(reader);
        mGenerator.Parse(reader);

        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(msv6Header);

        mOwner.Write(writer);
        mGenerator.Write(writer);

        Report(this, true);
    }

    #endregion
}