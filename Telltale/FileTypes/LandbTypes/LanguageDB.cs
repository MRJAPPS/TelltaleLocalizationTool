
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class LanguageDB : IParsable
{
    const int HEADER_SIZE = 140;
    private byte[] msv6Header = [];

    public Owner mOwner { get; set; } = new();
    public Generator mGenerator { get; set; } = new();


    public void Parse(BinaryReader reader)
    {
        msv6Header = reader.ReadBytes(HEADER_SIZE);
        mOwner.Parse(reader);
        mGenerator.Parse(reader);
    }
}
