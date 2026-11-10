using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LangDB;

[Description("Processing Owner section")]
public class Owner : ReportBase, ITelltaleObject
{
    #region Fields

    #endregion

    #region Properties

    public uint miUniqueID { get; set; }

    public uint Size => sizeof(uint) + ISize.FOUR_ADD;

    public long Position { get; private set; } = -1;

    #endregion

    #region Implementation

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // This uint is just a size marker – we already know it's 4, so we read and forget it.
        _ = reader.ReadUInt32();

        miUniqueID = reader.ReadUInt32();
        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(Owner), name);
        miUniqueID = uint.Parse(myElement.GetAttribute(nameof(miUniqueID)));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);
        writer.Write(miUniqueID);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(Owner));
        myElement.SetAttribute(nameof(name), name);

        XmlAttribute miUniqueID = myElement.OwnerDocument.CreateAttribute(nameof(miUniqueID));
        miUniqueID.Value = this.miUniqueID.ToString();
        _ = myElement.Attributes.Append(miUniqueID);

        _ = element.AppendChild(myElement);
    }

    #endregion
}