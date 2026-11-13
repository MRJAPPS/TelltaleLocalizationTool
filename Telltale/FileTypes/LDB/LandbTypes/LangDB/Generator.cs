using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LangDB;

[Description("Processing Generator section")]
public class Generator : ReportBase, ITelltaleObject
{
    #region Fields

    #endregion

    #region Properties

    public uint miNextUniqueID { get; set; }

    public uint Size => sizeof(uint) + ISize.FOUR_ADD;

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

        // Skip the size cap – it's just telling us what we already know.
        _ = reader.ReadUInt32();

        miNextUniqueID = reader.ReadUInt32();

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(Generator), name);
        miNextUniqueID = uint.Parse(myElement.GetAttribute(nameof(miNextUniqueID)));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);
        writer.Write(miNextUniqueID);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(Generator));

        myElement.SetAttribute(nameof(name), name);

        XmlAttribute miNextUniqueID = myElement.OwnerDocument.CreateAttribute(nameof(miNextUniqueID));
        miNextUniqueID.Value = this.miNextUniqueID.ToString();
        _ = myElement.Attributes.Append(miNextUniqueID);

        _ = element.AppendChild(myElement);
    }

    #endregion
}