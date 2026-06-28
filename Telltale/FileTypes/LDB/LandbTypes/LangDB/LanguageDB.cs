using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LangDB;

[Description("Processing Owner and Generator info")]
public class LanguageDB : ReportBase, ITelltaleObject
{
    #region Constants

    // This header size (140 bytes) comes from the Telltale format spec. No idea what’s inside,
    // but we need to eat those bytes to keep the stream aligned.
    private const int HeaderSize = 140;

    #endregion

    #region Fields

    // Raw bytes of the header. We don't parse them, just keep a copy.
    private byte[] msv6Header = [];

    #endregion

    #region Properties

    public Owner mOwner { get; set; } = new();

    public Generator mGenerator { get; set; } = new();

    public uint Size => HeaderSize + mOwner.Size + mGenerator.Size;

    public long Position { get; private set; }

    #endregion

    #region Implementation

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // Read the whole header blob – maybe one day we'll care what’s inside.
        msv6Header = reader.ReadBytes(HeaderSize);

        mOwner.Parse(reader);
        mGenerator.Parse(reader);

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(LanguageDB), name);

        ParseHeader(myElement);

        mOwner.Parse(myElement, nameof(mOwner));
        mGenerator.Parse(myElement, nameof(mGenerator));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(msv6Header);

        mOwner.Write(writer);
        mGenerator.Write(writer);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(LanguageDB));
        myElement.SetAttribute(nameof(name), name);
        AppendHeader(myElement);
        mOwner.Write(myElement, nameof(mOwner));
        mGenerator.Write(myElement, nameof(mGenerator));
        _ = element.AppendChild(myElement);
    }

    #endregion

    #region XML

    private void ParseHeader(XmlElement element)
    {
        XmlElement msv6H = element.GetSingleElement(nameof(msv6Header));
        msv6Header = Convert.FromBase64String(msv6H.InnerText);
    }

    private void AppendHeader(XmlElement element)
    {
        string headerB64 = Convert.ToBase64String(msv6Header);
        XmlElement b64 = element.OwnerDocument.CreateElement(nameof(msv6Header));
        b64.InnerText = headerB64;
        _ = element.AppendChild(b64);
    }
    #endregion
}