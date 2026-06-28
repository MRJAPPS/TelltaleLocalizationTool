using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Localize;

[Description("Processing the resolved localization data section")]
public class ResolvedLocalData : ReportBase, ITelltaleObject
{
    #region Constants

    // Size of the header field that stores the total length of this block.
    // We don't really need it to parse, but it's always present.
    private const uint ResolvedLocalDataSizeInFile = sizeof(uint);

    #endregion

    #region Fields

    #endregion

    #region Properties

    public TString mPrefix { get; set; } = [];

    public TString mText { get; set; } = [];

    public LocalizeInfo mLocalInfo { get; set; } = new();

    public uint Size =>
        mPrefix.Size + mText.Size + mLocalInfo.Size + ResolvedLocalDataSizeInFile;

    public long Position { get; private set; }

    #endregion

    #region Implementation

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // The first int is just a capacity/size value we can compute ourselves,
        // so we read it and throw it away – no point storing it.
        _ = reader.ReadInt32();

        mPrefix.Parse(reader);
        mText.Parse(reader);
        mLocalInfo.Parse(reader);

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(ResolvedLocalData), name);

        mPrefix.Parse(myElement, nameof(mPrefix));
        mText.Parse(myElement, nameof(mText));
        mLocalInfo.Parse(myElement, nameof(mLocalInfo));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);
        mPrefix.Write(writer);
        mText.Write(writer);
        mLocalInfo.Write(writer);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(ResolvedLocalData));
        myElement.SetAttribute(nameof(name), name);
        mPrefix.Write(myElement, nameof(mPrefix));
        mText.Write(myElement, nameof(mText));
        mLocalInfo.Write(myElement, nameof(mLocalInfo));

        _ = element.AppendChild(myElement);
    }

    #endregion
}