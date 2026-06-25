using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;

[Description("Reading ToolProps — no idea what this thing even is...")]
public class ToolProps : ReportBase, ITelltaleObject
{

    #region Public props

    public bool mbHasProps { get; set; }

    #endregion

    public uint Size => 1;

    public long Position { get; private set; }


    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        mbHasProps = ByteFunctions.ReadTelltaleBoolean(reader);
        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(ToolProps), name);
        ParseBasicTypes(myElement);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;
        ByteFunctions.WriteBoolean(writer, mbHasProps);
        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(ToolProps));

        myElement.SetAttribute(nameof(name), name);
        AppendBasicTypes(myElement);
        _ = element.AppendChild(myElement);
    }
    #region XML
    public void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mbHasProps = element.OwnerDocument.CreateAttribute(nameof(this.mbHasProps));
        mbHasProps.Value = this.mbHasProps.ToString();
        _ = element.Attributes.Append(mbHasProps);
    }
    public void ParseBasicTypes(XmlElement element) => this.mbHasProps = bool.Parse(element.GetAttribute(nameof(this.mbHasProps)));
    #endregion
}
