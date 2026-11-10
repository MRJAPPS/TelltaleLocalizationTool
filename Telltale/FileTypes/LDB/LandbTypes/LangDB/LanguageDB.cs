using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LangDB;

[Description("Processing Owner and Generator info")]
public class LanguageDB : ReportBase, ITelltaleObject
{
    #region Properties

    public Owner mOwner { get; set; } = new();

    public Generator mGenerator { get; set; } = new();

    public uint Size => /*HeaderSize(140) +*/ mOwner.Size + mGenerator.Size;

    public long Position { get; private set; }

    #endregion

    #region Implementation

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mOwner.Parse(reader);
        mGenerator.Parse(reader);

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(LanguageDB), name);

        mOwner.Parse(myElement, nameof(mOwner));
        mGenerator.Parse(myElement, nameof(mGenerator));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        mOwner.Write(writer);
        mGenerator.Write(writer);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(LanguageDB));
        myElement.SetAttribute(nameof(name), name);
        mOwner.Write(myElement, nameof(mOwner));
        mGenerator.Write(myElement, nameof(mGenerator));
        _ = element.AppendChild(myElement);
    }

    #endregion
}