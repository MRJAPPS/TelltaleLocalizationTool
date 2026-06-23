using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.TTypes;

[Description("Processing a SymbolValue(8 byte)")]
public class TSymbolValue : ReportBase, ILandb
{
    #region Private Fields

    #endregion

    #region Public Properties

    public ulong Value { get; set; }

    public uint Size => sizeof(ulong);

    public long Position { get; private set; }

    #endregion

    #region Implement

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        Value = reader.ReadUInt64();
        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(TSymbolValue), name);
        Value = ulong.Parse(myElement.GetAttribute(nameof(Value)));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;
        writer.Write(Value);
        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(TSymbolValue));
        myElement.SetAttribute(nameof(name), name);

        XmlAttribute Value = myElement.OwnerDocument.CreateAttribute(nameof(Value));
        Value.Value = this.Value.ToString();
        _ = myElement.Attributes.Append(Value);

        _ = element.AppendChild(myElement);
    }

    #endregion
}