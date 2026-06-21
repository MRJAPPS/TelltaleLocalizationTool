using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;

public class LocalDataItem : IParsable
{
    public TString mPrefix { get; set; } = [];

    public TString mText { get; set; } = [];

    public LocalizeInfo mLocalInfo = new();

    public TSymbolValue hSymbolValue { get; set; } = new();

    public uint Size => mPrefix.Size + mText.Size + mLocalInfo.Size;

    public void Parse(BinaryReader reader)
    {
        mPrefix.Parse(reader);
        mText.Parse(reader);
        mLocalInfo.Parse(reader);
    }
}
