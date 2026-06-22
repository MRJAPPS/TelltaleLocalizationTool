using System.ComponentModel;
using System.Runtime.InteropServices;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Enum;

using Enum = System.Enum;

[Description("Processing an Enum (probably, but I don't think so) with a fixed size")]
public class FixedEnum<T> : ReportBase, IParsable
    where T : struct, Enum
{
    #region Private Fields

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

    #region Base Members

    public long Position { get; private set; }

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

    #endregion
}