using TelltaleLocalizationTool.Telltale.Meta.Enums;

namespace TelltaleLocalizationTool.Telltale.Meta;

public class MetaStreamContext(Games game, IMetaHeader? header = null)
{
    public IMetaHeader? Header { get; set; } = header;

    public Games Game { get; set; } = game;
}
