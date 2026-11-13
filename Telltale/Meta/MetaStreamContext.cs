using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using TelltaleLocalizationTool.Telltale.Meta.TClassInfo;
using TelltaleLocalizationTool.Telltale.Meta.Versions;

namespace TelltaleLocalizationTool.Telltale.Meta;

public class MetaStreamContext(Games game, IMetaHeader? header = null)
{
    public IMetaHeader? Header { get; set; } = header;

    public Games Game { get; set; } = game;

    #region Helper

    public bool Contains(ClassNames className) => Header switch
    {
        MSV6 msv6 => msv6.Contains(className),
        MSV5 msv5 => msv5.Contains(className),
        _ => throw new NotImplementedException($"{nameof(MTRE)} and {nameof(MBIN)} aren't implemented yet! Because I haven't gotten to a game that uses them yet."),
    };

    public bool ContainsAt(int index,ClassNames className) => Header switch
    {
        MSV6 msv6 => msv6.mClassNames[index].Equals(className),
        MSV5 msv5 => msv5.mClassNames[index].Equals(className),
        _ => throw new NotImplementedException($"{nameof(MTRE)} and {nameof(MBIN)} aren't implemented yet! Because I haven't gotten to a game that uses them yet."),
    };

    #endregion
}
