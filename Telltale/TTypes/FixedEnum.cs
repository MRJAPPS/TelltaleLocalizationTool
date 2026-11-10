using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.TTypes;

using Enum = Enum;

[Description("Processing an Enum (probably, but I don't think so) with a fixed size")]
public class FixedEnum<T> : ReportBase, ITelltaleObject
    where T : struct, Enum
{
    #region Private Fields
    private readonly string TypeName = $"FixedEnum_{typeof(T).Name}_";
    #endregion

    #region Constructors

    public FixedEnum(T enumItem)
    {
        EnumItem = enumItem;
    }

    public FixedEnum() { }

    #endregion

    #region Public Properties

    public T EnumItem { get; set; } = default;

    public uint Size => (uint)(sizeof(int) + Marshal.SizeOf(Enum.GetUnderlyingType(typeof(T))));

    #endregion

    #region Implement

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // This int is just a size marker that echoes the value’s length – we don’t need it.
        _ = reader.ReadInt32();

        // We can’t call a generic reader.ReadEnum, so we ask what integer hides behind the enum.
        Type underlyingType = Enum.GetUnderlyingType(typeof(T));

        // Switch through every possible underlying type – tedious but there’s no magic shortcut.
        object rawValue = underlyingType switch
        {
            Type t when t == typeof(byte) => reader.ReadByte(),
            Type t when t == typeof(sbyte) => reader.ReadSByte(),
            Type t when t == typeof(short) => reader.ReadInt16(),
            Type t when t == typeof(ushort) => reader.ReadUInt16(),
            Type t when t == typeof(int) => reader.ReadInt32(),
            Type t when t == typeof(uint) => reader.ReadUInt32(),
            Type t when t == typeof(long) => reader.ReadInt64(),
            Type t when t == typeof(ulong) => reader.ReadUInt64(),
            _ => throw new NotSupportedException($"Enum underlying type {underlyingType} is not supported")
        };

        EnumItem = (T)Enum.ToObject(typeof(T), rawValue);
        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        Type underlyingType = Enum.GetUnderlyingType(typeof(T));

        int size = underlyingType switch
        {
            Type t when t == typeof(byte) || t == typeof(sbyte) => 1,
            Type t when t == typeof(short) || t == typeof(ushort) => 2,
            Type t when t == typeof(int) || t == typeof(uint) => 4,
            Type t when t == typeof(long) || t == typeof(ulong) => 8,
            _ => throw new NotSupportedException($"Enum underlying type {underlyingType} is not supported")
        };
        writer.Write(size + sizeof(uint));

        dynamic rawValue = Convert.ChangeType(EnumItem, underlyingType);
        switch (underlyingType)
        {
            case Type t when t == typeof(byte): writer.Write((byte)rawValue); break;
            case Type t when t == typeof(sbyte): writer.Write((sbyte)rawValue); break;
            case Type t when t == typeof(short): writer.Write((short)rawValue); break;
            case Type t when t == typeof(ushort): writer.Write((ushort)rawValue); break;
            case Type t when t == typeof(int): writer.Write((int)rawValue); break;
            case Type t when t == typeof(uint): writer.Write((uint)rawValue); break;
            case Type t when t == typeof(long): writer.Write((long)rawValue); break;
            case Type t when t == typeof(ulong): writer.Write((ulong)rawValue); break;
        }

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(TypeName);
        myElement.SetAttribute(nameof(name), name);

        AppendBasicTypes(myElement);

        element.AppendChild(myElement);
    }

    private void AppendBasicTypes(XmlElement myElement)
    {
        XmlAttribute EnumItem = myElement.OwnerDocument.CreateAttribute(nameof(EnumItem));
        EnumItem.Value = this.EnumItem.ToString();
        myElement.Attributes.Append(EnumItem);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(TypeName, name);

        ParseBasicTypes(myElement);

        Report(this);
    }

    private void ParseBasicTypes(XmlElement myElement)
    {
        string value = myElement.GetAttribute(nameof(EnumItem));

        EnumItem = Enum.Parse<T>(value);
    }

    #endregion
}