
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class LanguageDB : IParsable
{
    private const int HEADER_SIZE = 140;
    private byte[] msv6Header = [];

    public Owner mOwner { get; set; } = new();
    public Generator mGenerator { get; set; } = new();

    public uint Size => throw new NotImplementedException();

    public void Parse(BinaryReader reader)
    {
        msv6Header = reader.ReadBytes(HEADER_SIZE);
        mOwner.Parse(reader);
        mGenerator.Parse(reader);
    }
}
