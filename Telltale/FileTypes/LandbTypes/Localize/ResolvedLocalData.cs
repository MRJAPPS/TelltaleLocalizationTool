using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;

public class ResolvedLocalData : IParsable
{
    private const uint ResolvedLocalDataSizeInFile = sizeof(uint);

    public TString mPrefix { get; set; } = [];

    public TString mText { get; set; } = [];

    public LocalizeInfo mLocalInfo { get; set; } = new();

    public uint Size => mPrefix.Size + mText.Size + mLocalInfo.Size + ResolvedLocalDataSizeInFile;

    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadInt32();//skip cap
        mPrefix.Parse(reader);
        mText.Parse(reader);
        mLocalInfo.Parse(reader);
    }
}
