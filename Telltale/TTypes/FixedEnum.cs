using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Enum;
using TelltaleLocalizationTool.Telltale.Meta;

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

    public MetaStreamContext? msCtx { get; protected set; }

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        if (!CanIParseMyself()) return;
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
        if (!CanIParseMyself()) return;
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
        if (!CanIParseMyself()) return;
        XmlElement myElement = element.OwnerDocument.CreateElement(TypeName);
        myElement.SetAttribute(nameof(name), name);

        AppendBasicTypes(myElement);

        element.AppendChild(myElement);
    }

    public void Parse(XmlElement reader, string name)
    {
        if (!CanIParseMyself()) return;
        XmlElement myElement = reader.GetSingleElement(TypeName, name);

        ParseBasicTypes(myElement);

        Report(this);
    }

    #endregion

    #region XML

    private void ParseBasicTypes(XmlElement myElement)
    {
        string value = myElement.GetAttribute(nameof(EnumItem));

        EnumItem = Enum.Parse<T>(value);
    }

    private void AppendBasicTypes(XmlElement myElement)
    {
        XmlAttribute EnumItem = myElement.OwnerDocument.CreateAttribute(nameof(EnumItem));
        EnumItem.Value = this.EnumItem.ToString();
        myElement.Attributes.Append(EnumItem);
    }

    #endregion

    #region Checking

    private bool CanIParseMyself()
    {
        if (msCtx is null) throw new NullReferenceException($"Where did I or you screw up that you're seeing this error? Either your files are messed up or the auto-assignment to {nameof(msCtx)} part! You can report this bug to me. (We are in FixedEnum)");
        Utils.CheckHeader(msCtx.Game, msCtx.Header);

        if (!msCtx.ContainsAt(0, Meta.Enums.ClassNames.LanguageDB)) //This particular filter needs to be applied to landb files!
            return true;

        return msCtx.Game switch
        {
            Meta.Enums.Games.WDS1 => false,
            Meta.Enums.Games.WDS3NF => true,
            _ => throw new NotFiniteNumberException($"Your input is not valid! and we are in FixedEnum"),
        };
    }

    #endregion
}