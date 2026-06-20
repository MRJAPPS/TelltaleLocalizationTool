using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes;

public class Landb : IParsable
{
    #region PROPS

    public LanguageDB mLanguageDB { get; set; } = new();

    #endregion

    public void Parse(BinaryReader reader)
    {
        mLanguageDB.Parse(reader);
    }
}
