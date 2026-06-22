using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Localize;

public class ResolvedLocalData : IParsable
{
    private long _ParsePos;

    private const uint ResolvedLocalDataSizeInFile = sizeof(uint);

    public TString mPrefix { get; set; } = [];

    public TString mText { get; set; } = [];

    public LocalizeInfo mLocalInfo { get; set; } = new();

    public uint Size => mPrefix.Size + mText.Size + mLocalInfo.Size + ResolvedLocalDataSizeInFile;

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        _ = reader.ReadInt32();//skip cap
        mPrefix.Parse(reader);
        mText.Parse(reader);
        mLocalInfo.Parse(reader);
    }
}
