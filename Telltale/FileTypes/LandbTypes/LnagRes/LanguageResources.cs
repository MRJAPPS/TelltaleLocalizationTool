using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LnagRes.Text;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LnagRes;

public class LanguageResources : IParsable
{
    public uint Size => TextItems.Size;

    public TextCollection TextItems { get; set; } = [];

    public void Parse(BinaryReader reader)
    {
        reader.ReadInt32();//TODO: size IS HERE
        TextItems.Parse(reader);
    }
}
