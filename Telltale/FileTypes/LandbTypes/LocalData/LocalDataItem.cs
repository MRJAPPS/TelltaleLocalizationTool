using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LocalData;

public class LocalDataItem : IParsable
{
    public TString mPrefix { get; set; } = [];

    public TString mText { get; set; } = [];

    public LocalizeInfo mLocalInfo = new();

    public SymbolValue hSymbolValue { get; set; } = new();

    public uint Size => mPrefix.Size + mText.Size + mLocalInfo.Size;

    public void Parse(BinaryReader reader)
    {
        mPrefix.Parse(reader);
        mText.Parse(reader);
        mLocalInfo.Parse(reader);
    }
}
