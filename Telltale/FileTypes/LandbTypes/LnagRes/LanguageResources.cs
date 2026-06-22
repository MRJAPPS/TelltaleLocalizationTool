using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LnagRes.Text;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LnagRes;

[Description("Processing the game text stuff (subtitles, menus, etc.)")]
public class LanguageResources : ReportBase, IParsable
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
        Report(this);
    }
}
