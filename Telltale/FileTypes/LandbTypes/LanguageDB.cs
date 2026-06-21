
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class LanguageDB : IParsable
{
    private const int HeaderSize = 140;
    private byte[] msv6Header = [];

    public Owner mOwner { get; set; } = new();
    public Generator mGenerator { get; set; } = new();

    public uint Size => HeaderSize + mOwner.Size + mGenerator.Size;

    public void Parse(BinaryReader reader)
    {
        msv6Header = reader.ReadBytes(HeaderSize);
        mOwner.Parse(reader);
        mGenerator.Parse(reader);
    }
}
