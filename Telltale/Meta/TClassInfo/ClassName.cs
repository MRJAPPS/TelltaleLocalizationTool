using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.Meta;
using TelltaleLocalizationTool.Telltale.Meta.Enums;

namespace TelltaleLocalizationTool.Telltale.Meta.TClassInfo;

/// <summary>
/// This is a class name struct used in a meta header.
/// This contains a CRC64'd string of a class name used in the file.
/// The CRC64 string of a classname is usually all lowercase
/// </summary>
[Description("Reading a ClassName")]
public class ClassName : ReportBase, ITelltaleObject
{

    #region Public props

    public Symbol mTypeNameCRC { get; set; } = new();
    public uint mVersionCRC { get; set; }

    #endregion

    #region Implement

    public MetaStreamContext? msCtx { get; protected set; }

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    public uint Size => mTypeNameCRC.Size + sizeof(uint);

    public long Position { get; private set; } = -1;


    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        mTypeNameCRC.Write(writer);
        writer.Write(mVersionCRC);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(ClassName));

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
        XmlElement myElement = reader.GetSingleElement(nameof(ClassName), name);

        mTypeNameCRC.Parse(myElement, string.Empty);//We don't need a "name" in this case because symbol adds itself to this Node like an Attribute.
        ParseBasicTypes(myElement);
    }

    #endregion

    #region Overloading

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not ClassName other)
        {
            return false;
        }

        return mTypeNameCRC == other.mTypeNameCRC
            && mVersionCRC == other.mVersionCRC;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(mTypeNameCRC, mVersionCRC);
    }

    public static bool operator ==(ClassName left, ClassName right) => left.mTypeNameCRC == right.mTypeNameCRC && left.mVersionCRC == right.mVersionCRC;

    public static bool operator !=(ClassName left, ClassName right) => left.mTypeNameCRC != right.mTypeNameCRC || left.mVersionCRC != right.mVersionCRC;


    public static bool operator ==(ClassName left, ClassNames right) => left.mTypeNameCRC.mCrc64 == (ulong)right;

    public static bool operator !=(ClassName left, ClassNames right) => left.mTypeNameCRC.mCrc64 != (ulong)right;

    public static bool operator ==(ClassNames left, ClassName right) => (ulong)left == right.mTypeNameCRC.mCrc64;

    public static bool operator !=(ClassNames left, ClassName right) => (ulong)left != right.mTypeNameCRC.mCrc64;

    #endregion

    #region XML
    private void ParseBasicTypes(XmlElement element)
    {
        mVersionCRC = uint.Parse(element.GetAttribute(nameof(mVersionCRC)));
    }

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mVersionCRC = element.OwnerDocument.CreateAttribute(nameof(this.mVersionCRC));

        mVersionCRC.Value = this.mVersionCRC.ToString();

        _ = element.Attributes.Append(mVersionCRC);
    }

    #endregion
}