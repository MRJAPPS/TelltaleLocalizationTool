using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class ResolvedLocalData : IParsable, ICpacity
{
    public uint Cpacity => throw new NotImplementedException("ResolvedLocalData.Cpacity");
    public TString mPrefix { get; set; } = [];
    public TString mText { get; set; } = [];
    public LocalizeInfo mLocalInfo { get; set; } = new();

    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadInt32();//skip cap
        mPrefix.Parse(reader);
        mText.Parse(reader);
        mLocalInfo.Parse(reader);
    }
}
