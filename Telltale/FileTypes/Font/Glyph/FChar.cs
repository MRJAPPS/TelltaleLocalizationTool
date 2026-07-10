using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Font.Glyph;
public class FChar : ITelltaleObject
{
    #region Public Props

    public uint CharCode { get; set; }

    public char Char => (char)CharCode;

    #endregion

    #region Implement 

    public uint Size => sizeof(uint);

    public long Position { get; private set; }

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        CharCode = reader.ReadUInt32();

        //We don't report FChar
    }

    public void Parse(XmlElement reader, string name)
    {
        CharCode = uint.Parse(reader.GetAttribute(name));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(CharCode);

        //We don't report FChar
    }

    public void Write(XmlElement element, string name)
    {
        XmlAttribute charCodeAtt = element.OwnerDocument.CreateAttribute(name);
        charCodeAtt.Value = CharCode.ToString();
        _ = element.Attributes.Append(charCodeAtt);
    }

    #endregion
}
