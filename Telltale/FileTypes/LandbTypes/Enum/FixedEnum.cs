using System.ComponentModel;
using System.Runtime.InteropServices;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Enum;

using Enum = System.Enum;

/// <summary>
/// Reads an enum value from a binary stream – the file format insists on a leading int that
/// says how big the value is, but since we already know the type, we just skip that int.
/// </summary>
/// <typeparam name="T">Must be an enum. We’ll read its underlying integer type.</typeparam>
[Description("Processing an Enum (probably, but I don't think so) with a fixed size")]
public class FixedEnum<T> : ReportBase, IParsable
    where T : struct, Enum
{
    #region Private Fields

    // Where the reader was at the start – stored for the report.
    private long _ParsePos;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new instance with a specific enum value.
    /// </summary>
    public FixedEnum(T enumItem)
    {
        EnumItem = enumItem;
    }

    /// <summary>
    /// Creates a new instance with the default enum value.
    /// </summary>
    public FixedEnum() { }

    #endregion

    #region Public Properties

    /// <summary>
    /// The actual enum value after it’s been read (or set manually).
    /// </summary>
    public T EnumItem { get; set; } = default;

    /// <summary>
    /// Total bytes this thing occupies in the file: the 4‑byte size header we skip,
    /// plus the size of the enum’s underlying integer.
    /// </summary>
    public uint Size => (uint)(sizeof(int) + Marshal.SizeOf(Enum.GetUnderlyingType(typeof(T))));

    #endregion

    #region IParsable Members

    /// <summary>
    /// The stream offset when parsing began. Needed for the report.
    /// </summary>
    long IParsable.ParsePosition => _ParsePos;

    /// <summary>
    /// Skips the useless size int, then reads the enum’s backing value and converts it to <typeparamref name="T"/>.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;

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

    #endregion
}