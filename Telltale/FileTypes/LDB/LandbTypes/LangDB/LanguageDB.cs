using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LangDB;

[Description("Processing Owner and Generator info")]
public class LanguageDB : ReportBase, ITelltaleFile
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

    public Owner mOwner { get; set; } = new();

    public Generator mGenerator { get; set; } = new();

    public uint Size => HeaderSize + mOwner.Size + mGenerator.Size;

    public long Position { get; private set; }

    #endregion

    #region Implementation

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