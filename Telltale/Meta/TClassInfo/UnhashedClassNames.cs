using System.ComponentModel;
using System.Text;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.Meta.TClassInfo;

/// <summary>
/// This is a class name struct used in a meta header.
/// This contains a CRC64'd string of a class name used in the file.
/// The CRC64 string of a classname is usually all lowercase
/// </summary>
[Description("Reading an unhashed ClassName (UnhashedClassName)")]
public class UnhashedClassNames : ReportBase, ITelltaleObject
{
    // No BOM, and scream loudly on bad bytes instead of silently mangling them, Ok???.
    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    #region private fields

    private string className = string.Empty;

    private uint mVersionCRC;

    #endregion

    #region Public props

    public uint Length => (uint)Utf8.GetByteCount(className);

    public string ClassName
    {
        get => className;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            // Sanity check: throws if there's a lone surrogate bro, that's not real text!
            _ = Utf8.GetByteCount(value);

            className = value;
        }
    }

    public uint VersionCRC { get => mVersionCRC; set => mVersionCRC = value; }

    #endregion

    #region Implement

    public uint Size => sizeof(uint) * 2 + Length;

    public long Position { get; private set; }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        byte[] bytes = Utf8.GetBytes(className);
        writer.Write((uint)bytes.Length);
        writer.Write(bytes);
        writer.Write(mVersionCRC);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(UnhashedClassNames));

        myElement.SetAttribute(nameof(name), name);
        AppendBasicTypes(myElement);

        _ = element.AppendChild(myElement);
    }

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        uint length = reader.ReadUInt32();
        ClassName = Utf8.GetString(reader.ReadBytes((int)length)); // screams on corrupt UTF-8 bytes
        VersionCRC = reader.ReadUInt32();

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(UnhashedClassNames), name);

        ParseBasicTypes(myElement);
    }

    #endregion

    #region XML

    public void ParseBasicTypes(XmlElement element)
    {
        ClassName = element.GetAttribute(nameof(className)); // setter validates here too

        if (!uint.TryParse(element.GetAttribute(nameof(mVersionCRC)), out mVersionCRC))
        {
            throw new XmlException(
                $"Missing or invalid '{nameof(mVersionCRC)}' attribute on <{nameof(UnhashedClassNames)}> bro, can't rebuild the binary without it!.");
        }
    }

    public void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute ClassName = element.OwnerDocument.CreateAttribute(nameof(className));
        XmlAttribute VersionCRC = element.OwnerDocument.CreateAttribute(nameof(mVersionCRC));

        ClassName.Value = className;
        VersionCRC.Value = mVersionCRC.ToString();

        _ = element.Attributes.Append(ClassName);
        _ = element.Attributes.Append(VersionCRC);
    }

    #endregion
}