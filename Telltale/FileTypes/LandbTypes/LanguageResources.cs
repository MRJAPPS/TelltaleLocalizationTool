using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Text;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class LanguageResources : IParsable
{
    public uint Size => throw new NotImplementedException();

    public TextCollection TextItems { get; set; } = [];

    public void Parse(BinaryReader reader)
    {
        reader.ReadInt32();//TODO: size IS HERE
        TextItems.Parse(reader);
    }
}
