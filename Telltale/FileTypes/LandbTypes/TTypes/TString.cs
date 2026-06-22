using System.Collections;
using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Text;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

[Description("processing a string(Telltale string)")]
public class TString(string value, bool readSize, int FixedLength, bool skipZero, Encoding? encoding)
    : ReportBase, IEquatable<TString>, IComparable<TString>, ICollection<char>, IComparable, IParsable
{
    #region Constants

    // Always 4 – the standard "I'm a string" header cap. No cap.
    private const uint StringLenCap = 4;

    public const int VarLength = -1;

    #endregion

    #region Private Fields

    // Did we read a prefixed size field? (some strings have it, some don't – Telltale logic)
    private readonly bool readSize = readSize;
    // Should we munch trailing zero‑bytes after the string?
    private readonly bool skipZero = skipZero;

    #endregion

    #region Constructors

    public TString() : this(VarLength, false) { }

    public TString(bool skipZero) : this(VarLength, skipZero) { }

    public TString(int fixedLength, bool skipZero) : this(string.Empty, true, fixedLength, skipZero) { }

    public TString(string value, int fixedLength, bool skipZero) : this(value, true, fixedLength, skipZero) { }

    public TString(string value, bool readSize, int fixedLength, bool skipZero) : this(value, readSize, fixedLength, skipZero, null) { }

    #endregion

    #region Properties

    public uint Length => (uint)Encoding.GetBytes(Value).Length;

    public int FixedLength { get; private set; } = FixedLength;

    public bool IsFixed => FixedLength >= 0;

    public int ZeroCount { get; private set; }

    private string Value { get; set; } = value ?? string.Empty;

    public Encoding Encoding { get; set; } = encoding ?? Encoding.UTF8;

    public uint Size =>
        Length + ISize.FOUR_ADD + (skipZero ? (uint)ZeroCount : StringLenCap);

    public int Count => Value.Length;

    public bool IsReadOnly => true;

    public long Position { get; private set; }

    #endregion

    #region Parse/Write

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        if (readSize)
        {
            _ = reader.ReadInt32(); // Skip the redundant size field – we'll compute length ourselves.
        }

        int length = IsFixed ? FixedLength : reader.ReadInt32();
        byte[] stringBytes = reader.ReadBytes(length);
        Value = Encoding.GetString(stringBytes);

        if (skipZero)
        {
            SkipZeros(reader);
        }

        Report(this, false, $"({nameof(skipZero)} = {skipZero}, {nameof(readSize)} = {readSize}, {nameof(ZeroCount)} = {ZeroCount})");
    }


    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        if (readSize)
        {
            writer.Write(Size);
        }
        writer.Write(Length);
        writer.Write(Encoding.GetBytes(Value)); ;

        if (skipZero && ZeroCount > 0)
        {
            for (int i = 0; i < ZeroCount; ++i)
                writer.Write((byte)0);
        }

        Report(this, true, $"({nameof(skipZero)} = {skipZero}, {nameof(readSize)} = {readSize}, {nameof(ZeroCount)} = {ZeroCount})");

    }

    #endregion

    #region Private Helpers

    private void SkipZeros(BinaryReader reader)
    {
        while (reader.PeekChar() != -1)
        {
            byte b = reader.ReadByte();
            if (b != 0)
            {
                // Oops, that's actual data – step back so it can be read properly.
                reader.BaseStream.Position -= 1;
                break;
            }
            // Nom nom, another zero byte.
            ZeroCount++;
        }
    }

    #endregion

    #region Equality & Comparison

    public bool Equals(TString? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TString);
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
        return Value ?? string.Empty;
    }

    public int CompareTo(TString? other)
    {
        return other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);
    }

    public int CompareTo(object? obj)
    {
        return obj is TString ts
            ? CompareTo(ts)
            : throw new ArgumentException(
            $"Object({obj?.GetType()?.Name}) is not a {nameof(TString)}");
    }

    #endregion

    #region Operators

    public static bool operator ==(TString left, TString right)
    {
        return left?.Value == right?.Value;
    }

    public static bool operator !=(TString left, TString right)
    {
        return !(left == right);
    }

    public static bool operator <(TString left, TString right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    public static bool operator <=(TString left, TString right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    public static bool operator >(TString left, TString right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    public static bool operator >=(TString left, TString right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }

    #endregion

    #region Implicit Conversions

    public static implicit operator string(TString t)
    {
        return t?.Value ?? string.Empty;
    }

    public static implicit operator TString(string s)
    {
        return [.. s];
    }

    #endregion

    #region Static Factory

    public static TString FromValue(string value)
    {
        return [.. value];
    }

    #endregion

    #region ICollection<char> Implementation

    public void Add(char item)
    {
        throw new NotSupportedException("Collection is read-only.");
    }

    public void Clear()
    {
        throw new NotSupportedException("Collection is read-only.");
    }

    public bool Contains(char item)
    {
        return Value.Contains(item);
    }

    public void CopyTo(char[] array, int arrayIndex)
    {
        Value.CopyTo(0, array, arrayIndex, Value.Length);
    }

    public bool Remove(char item)
    {
        throw new NotSupportedException("Collection is read-only.");
    }

    public IEnumerator<char> GetEnumerator()
    {
        return Value.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}