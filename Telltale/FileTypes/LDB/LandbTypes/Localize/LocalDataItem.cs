using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Localize;

[Description("Processing the localization data section")]
public class LocalDataItem : ReportBase, IParsable
{
    #region Private Fields


    #endregion

    #region Public Fields

    public LocalizeInfo mLocalInfo = new();

    #endregion

    #region Public Properties

    public TString mPrefix { get; set; } = [];

    public TString mText { get; set; } = [];

    public TSymbolValue hSymbolValue { get; set; } = new();

    public uint Size => mPrefix.Size + mText.Size + mLocalInfo.Size;

    public long Position { get; private set; }

    #endregion

    #region Implementation

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        mPrefix.Parse(reader);
        mText.Parse(reader);
        mLocalInfo.Parse(reader);
        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        mPrefix.Write(writer);
        mText.Write(writer);
        mLocalInfo.Write(writer);

        Report(this,true);
    }

    #endregion
}