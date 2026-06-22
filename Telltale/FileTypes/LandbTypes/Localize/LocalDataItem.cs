using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;

[Description("Processing the localization data section")]
public class LocalDataItem :ReportBase, IParsable
{
    private long _ParsePos;

    public TString mPrefix { get; set; } = [];

    public TString mText { get; set; } = [];

    public LocalizeInfo mLocalInfo = new();

    public TSymbolValue hSymbolValue { get; set; } = new();

    public uint Size => mPrefix.Size + mText.Size + mLocalInfo.Size;

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        mPrefix.Parse(reader);
        mText.Parse(reader);
        mLocalInfo.Parse(reader);
        Report(this);
    }
}
