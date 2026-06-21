using System.Collections;
using System.Text;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes;

public class TString(string value, bool readCpacity, int FixedLength, Encoding? encoding) : IEquatable<TString>, IComparable<TString>, ICollection<char>, IComparable, IParsable, ICpacity
{
    public uint Length => (uint)Encoding.GetBytes(Value).Length;
    public int FixedLength { get; private set; } = FixedLength;
    public bool IsFixed => FixedLength >= 0;
    private string Value { get; set; } = value ?? string.Empty;
    private readonly bool readCpacity = readCpacity;

    public Encoding Encoding { get; set; } = encoding ?? Encoding.UTF8;

    public uint Cpacity => Length + ICpacity.FOUR_ADD * 2/*8 for strings*/;

    public int Count => Value.Length;

    public bool IsReadOnly => true;

    public TString() : this(-1) { }

    public TString(int fixedLength) : this(string.Empty, true, fixedLength) { }

    public TString(string value, int fixedLength) : this(value, true, fixedLength) { }

    public TString(string value, bool readCpacity, int fixedLength) : this(value, readCpacity, fixedLength, null) { }

    public void Parse(BinaryReader reader)
    {
        if (readCpacity)
        {
            _ = reader.ReadInt32();//skip cap
        }
        int length = IsFixed ? FixedLength : reader.ReadInt32();
        byte[] stringBytes = reader.ReadBytes(length);
        Value = Encoding.GetString(stringBytes);
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