using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LangDB;

[Description("Processing Owner and Generator info")]
public class LanguageDB : ReportBase, IParsable
{
    private const int HeaderSize = 140;
    private long _ParsePos;
    private byte[] msv6Header = [];

    public Owner mOwner { get; set; } = new();
    public Generator mGenerator { get; set; } = new();

    public uint Size => HeaderSize + mOwner.Size + mGenerator.Size;

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        msv6Header = reader.ReadBytes(HeaderSize);
        mOwner.Parse(reader);
        mGenerator.Parse(reader);
        Report(this);
    }
}
