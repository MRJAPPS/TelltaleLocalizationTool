using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LnagRes.Text;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LnagRes;

public class LanguageResources : IParsable
{
    private long _ParsePos;

    public uint Size => TextItems.Size;

    public TextCollection TextItems { get; set; } = [];

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        _ = reader.ReadInt32();
        TextItems.Parse(reader);
    }
}
