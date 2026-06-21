using System;
using System.IO;
using System.Runtime.InteropServices;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Enum;

using Enum = System.Enum;

public class FixedEnum<T> : IParsable, ICpacity
    where T : struct, Enum
{
    public FixedEnum(T enumItem)
    {
        EnumItem = enumItem;
    }
    public FixedEnum() { }

    public T EnumItem { get; set; } = default;

    public uint Cpacity => (uint)(sizeof(int) + Marshal.SizeOf(Enum.GetUnderlyingType(typeof(T))));

    public void Parse(BinaryReader reader)
    {
        _ = reader.ReadInt32(); //skip cpa

        Type underlyingType = Enum.GetUnderlyingType(typeof(T));

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
    }
}