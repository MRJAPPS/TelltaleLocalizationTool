using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Text;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class LanguageResources : ICpacity, IParsable
{
    public uint Cpacity
    {
        get
        {
            throw new NotImplementedException("LanguageResources.Cpacity");
        }
    }

    public TextCollection mIds { get; set; } = [];

    public void Parse(BinaryReader reader)
    {
        reader.ReadInt32();//TODO: CPA IS HERE
        mIds.Parse(reader);
    }
}
