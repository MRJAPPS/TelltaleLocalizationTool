using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.Meta;
using TelltaleLocalizationTool.Telltale.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Localize;

[Description("Processing the localize info flag")]
public class LocalizeInfo : ReportBase, ITelltaleObject
{
    #region Constants

    // The size field that appears right before the flags – always 4 bytes.
    private const uint SizeFieldBytes = sizeof(uint);

    #endregion

    #region Private Fields

    #endregion

    #region Public Properties

    public Flags<uint> Flags { get; set; } = new();

    public uint Size => SizeFieldBytes + Flags.Size;

    public long Position { get; private set; } = -1;

    #endregion

    #region Implement

    public MetaStreamContext? msCtx { get; protected set; }

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // This 4‑byte value is just a duplicate of the block size – we already know it’s 4.
        // Read it and toss it so the stream pointer lands on the flags.
        _ = reader.ReadUInt32();

        Flags.Parse(reader);
        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(LocalizeInfo), name);
        Flags.Parse(myElement, nameof(Flags));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);
        Flags.Write(writer);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(LocalizeInfo));
        myElement.SetAttribute(nameof(name), name);

        Flags.Write(myElement, nameof(Flags));

        _ = element.AppendChild(myElement);
    }

    #endregion
}
