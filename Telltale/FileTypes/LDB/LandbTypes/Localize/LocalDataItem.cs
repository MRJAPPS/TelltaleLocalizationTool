using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Localize;

[Description("Processing the localization data section")]
public class LocalDataItem : ReportBase, ITelltaleFile
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

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(LocalDataItem), name);
        mPrefix.Parse(myElement, nameof(mPrefix));
        mText.Parse(myElement, nameof(mText));
        mLocalInfo.Parse(myElement, nameof(mLocalInfo));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        mPrefix.Write(writer);
        mText.Write(writer);
        mLocalInfo.Write(writer);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(LocalDataItem));
        myElement.SetAttribute(nameof(name), name);

        mPrefix.Write(myElement, nameof(mPrefix));
        mText.Write(myElement, nameof(mText));

        mLocalInfo.Write(myElement, nameof(mLocalInfo));

        _ = element.AppendChild(myElement);
    }

    #endregion
}