using System.Collections;
using System.Text;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

public class TString(string value, bool readSize, int FixedLength, bool skipZero, Encoding? encoding) : IEquatable<TString>, IComparable<TString>, ICollection<char>, IComparable, IParsable
{
    private const uint StringLenCap = 4;
    public const int VarLength = -1;

    public uint Length => (uint)Encoding.GetBytes(Value).Length;
    public int FixedLength { get; private set; } = FixedLength;
    public bool IsFixed => FixedLength >= 0;
    public int ZeroCount { get; private set; }

    private string Value { get; set; } = value ?? string.Empty;
    private readonly bool readSize = readSize;
    private readonly bool skipZero = skipZero;
    private long _ParsePos;

    public Encoding Encoding { get; set; } = encoding ?? Encoding.UTF8;

    public uint Size => Length + ISize.FOUR_ADD + (skipZero ? (uint)ZeroCount : StringLenCap);

    public int Count => Value.Length;

    public bool IsReadOnly => true;

    long IParsable.ParsePosition => _ParsePos;

    public TString() : this(VarLength, false) { }

    public TString(bool skipZero) : this(VarLength, skipZero) { }

    public TString(int fixedLength, bool skipZero) : this(string.Empty, true, fixedLength, skipZero) { }

    public TString(string value, int fixedLength, bool skipZero) : this(value, true, fixedLength, skipZero) { }

    public TString(string value, bool readSize, int fixedLength, bool skipZero) : this(value, readSize, fixedLength, skipZero, null) { }

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        if (readSize)
        {
            _ = reader.ReadInt32();//skip size
        }
        int length = IsFixed ? FixedLength : reader.ReadInt32();
        byte[] stringBytes = reader.ReadBytes(length);
        Value = Encoding.GetString(stringBytes);
        if (skipZero)
        {
            SkipZeros(reader);
        }
    }

    // Friendly note: Since ints are Little-Endian (LSB first), 0x00000012 is stored as 0x12000000 in the file, so a leading 0 byte is just upper padding, not a zero-length string value. We safely skip zeros until we hit the actual LSB to read the integer!
    private void SkipZeros(BinaryReader reader)
    {
        while (reader.PeekChar() != -1)
        {
            byte b = reader.ReadByte();
            if (b != 0)
            {
                reader.BaseStream.Position -= 1;
                break;
            }
            else
            {
                ZeroCount++;
            }
        }
    }

    public static implicit operator string(TString t)
    {
        return t?.Value ?? string.Empty;
    }

    public static implicit operator TString(string s)
    {
        return [.. s];
    }

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
        return obj is TString ts ? CompareTo(ts) : throw new ArgumentException($"Object({obj?.GetType()?.Name}) is not a {nameof(TString)}");
    }

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

    public static TString FromValue(string value)
    {
        return [.. value];
    }

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
}