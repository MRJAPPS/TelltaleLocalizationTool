using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LocalData;

public class LocalDataItem : IParsable
{
    public TString mPrefix { get; set; } = [];

    public TString mText { get; set; } = [];

    public uint Size => throw new NotImplementedException();

    public LocalizeInfo mLocalInfo = new();

    public void Parse(BinaryReader reader)
    {
        mPrefix.Parse(reader);
        mText.Parse(reader);
        mLocalInfo.Parse(reader);
    }
}
