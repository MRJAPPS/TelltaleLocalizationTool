using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.DData;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LangDB;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LnagRes;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.ProjDataBIDPairs;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Reg;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB;

[Description("Process the whole landb file")]
public class Landb : ReportBase, IParsable
{
    private const int OffsetDefDataLen = 0x4;
    private const int OffsetDefDebugDataLen = OffsetDefDataLen + 0x4;
    private const int OffsetAsyncDataLen = OffsetDefDebugDataLen + 0x4;
    #region Private Fields

    #endregion

    #region Public Properties

    public LanguageDB mLanguageDB { get; set; } = new();

    public LanguageResources mLanguageResources { get; set; } = new();

    public Registry mRegistry { get; set; } = new();

    public uint mFlags { get; set; }

    public uint mProjectID { get; set; }

    public ProjectDatabaseIDPairs mExpandedIDRanges { get; set; } = [];

    public DebugDataList mDebugDatas { get; set; } = [];


    public uint Size => mLanguageDB.Size +
        mLanguageResources.Size +
        mRegistry.Size +
        sizeof(uint) * 2 +            
        mExpandedIDRanges.Size + mDebugDatas.Size;

    public long Position { get; private set; }

    #endregion

    #region Public Methods

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mLanguageDB.Parse(reader);
        mLanguageResources.Parse(reader);
        mRegistry.Parse(reader);

        mFlags = reader.ReadUInt32();
        mProjectID = reader.ReadUInt32();

        mExpandedIDRanges.Parse(reader);
        mDebugDatas.Parse(reader);

        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        mLanguageDB.Write(writer);
        mLanguageResources.Write(writer);
        mRegistry.Write(writer);

        writer.Write(mFlags);
        writer.Write(mProjectID);

        mExpandedIDRanges.Write(writer);
        mDebugDatas.Write(writer);

        Report(this, true);
        _ = writer.Seek(OffsetDefDataLen, SeekOrigin.Begin);
        writer.Write(Size - mDebugDatas.Size - 140);
        _ = writer.Seek(OffsetDefDebugDataLen, SeekOrigin.Begin);
        writer.Write(mDebugDatas.Size);
    }

    #endregion
}