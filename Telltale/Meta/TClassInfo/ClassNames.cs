using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.Meta.TClassInfo;

/// <summary>
/// This is a class name struct used in a meta header.
/// This contains a CRC64'd string of a class name used in the file.
/// The CRC64 string of a classname is usually all lowercase
/// </summary>
[Description("Reading a ClassName")]
public class ClassNames : ReportBase, ITelltaleObject
{

    #region Public props

    public Symbol mTypeNameCRC { get; set; } = new();
    public uint mVersionCRC { get; set; }

    #endregion

    #region Implement

    public uint Size => mTypeNameCRC.Size + sizeof(uint);

    public long Position { get; private set; }


    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        mTypeNameCRC.Write(writer);
        writer.Write(mVersionCRC);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(ClassNames));

        myElement.SetAttribute(nameof(name), name);
        mTypeNameCRC.Write(myElement, string.Empty);//We don't need a "name" in this case because symbol adds itself to this Node like an Attribute.
        AppendBasicTypes(myElement);

        _ = element.AppendChild(myElement);
    }

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mTypeNameCRC.Parse(reader);
        mVersionCRC = reader.ReadUInt32();

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(ClassNames), name);

        mTypeNameCRC.Parse(myElement, string.Empty);//We don't need a "name" in this case because symbol adds itself to this Node like an Attribute.
        ParseBasicTypes(myElement);
    }

    #endregion

    #region XML
    public void ParseBasicTypes(XmlElement element)
    {
        mVersionCRC = uint.Parse(element.GetAttribute(nameof(mVersionCRC)));//MAKE IT PRIVATE TODO
    }

    public void AppendBasicTypes(XmlElement element)//MAKE IT PRIVATE TODO
    {
        XmlAttribute mVersionCRC = element.OwnerDocument.CreateAttribute(nameof(this.mVersionCRC));

        mVersionCRC.Value = this.mVersionCRC.ToString();

        _ = element.Attributes.Append(mVersionCRC);
    }

    #endregion
}