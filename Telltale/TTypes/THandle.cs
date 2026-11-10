using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.TTypes;

[Description("Processing THandle(a wrapper for TSymbolValue + 4 byte extra)")]
public class THandle : ReportBase, ITelltaleObject
{
    #region Private Fields

    #endregion

    #region Public Properties

    public TSymbolValue hSymbolValue { get; set; } = new();

    public uint Size => hSymbolValue.Size + ISize.FOUR_ADD;

    public long Position { get; private set; } = -1;

    #endregion

    #region Public Methods

    public void Parse(BinaryReader reader)
    {
        // Stash the start pos for the report – gotta flex that offset later.
        Position = reader.BaseStream.Position;

        // Those first 4 bytes are just a duplicate size we can figure out ourselves. Nom them and move on.
        _ = reader.ReadUInt32();

        hSymbolValue.Parse(reader);

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(THandle), name);
        hSymbolValue.Parse(myElement, nameof(hSymbolValue));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;
        writer.Write(Size);

        hSymbolValue.Write(writer);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(THandle));
        myElement.SetAttribute(nameof(name), name);

        hSymbolValue.Write(myElement, nameof(hSymbolValue));

        _ = element.AppendChild(myElement);
    }

    #endregion
}