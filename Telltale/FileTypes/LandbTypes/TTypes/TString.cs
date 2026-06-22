using System.Collections;
using System.ComponentModel;
using System.Text;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

/// <summary>
/// A Telltale-style string that reads itself from a binary stream.  
/// Supports variable‑length, fixed‑length, and zero‑skipping shenanigans.  
/// Basically a turbo‑charged <c>string</c> with extra metadata.
/// </summary>
[Description("processing a string(Telltale string)")]
public class TString(string value, bool readSize, int FixedLength, bool skipZero, Encoding? encoding)
    : ReportBase, IEquatable<TString>, IComparable<TString>, ICollection<char>, IComparable, IParsable
{
    #region Constants

    // Always 4 – the standard "I'm a string" header cap. No cap.
    private const uint StringLenCap = 4;

    /// <summary>
    /// Sentinel value that means "hey, figure out the length yourself, I'm not fixed".
    /// </summary>
    public const int VarLength = -1;

    #endregion

    #region Private Fields

    // Did we read a prefixed size field? (some strings have it, some don't – Telltale logic)
    private readonly bool readSize = readSize;
    // Should we munch trailing zero‑bytes after the string?
    private readonly bool skipZero = skipZero;
    // Where the stream was when we started parsing – needed for the geek report.
    private long _ParsePos;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new variable‑length TString without zero‑skipping.
    /// </summary>
    public TString() : this(VarLength, false) { }

    /// <summary>
    /// Creates a new variable‑length TString with explicit zero‑skipping preference.
    /// </summary>
    /// <param name="skipZero">If <c>true</c>, trailing zeros will be gobbled up after the string.</param>
    public TString(bool skipZero) : this(VarLength, skipZero) { }

    /// <summary>
    /// Creates a new fixed‑length TString with a predetermined byte count and zero‑skipping.
    /// </summary>
    /// <param name="fixedLength">The exact number of bytes to read, no questions asked.</param>
    /// <param name="skipZero">If <c>true</c>, zeros after the fixed block get skipped.</param>
    public TString(int fixedLength, bool skipZero) : this(string.Empty, true, fixedLength, skipZero) { }

    /// <summary>
    /// Creates a new fixed‑length TString with an initial value.
    /// </summary>
    /// <param name="value">The string content.</param>
    /// <param name="fixedLength">The fixed byte count when reading/writing.</param>
    /// <param name="skipZero">Zero‑skipping mode.</param>
    public TString(string value, int fixedLength, bool skipZero) : this(value, true, fixedLength, skipZero) { }

    /// <summary>
    /// Creates a new TString without specifying an explicit encoding (UTF‑8 by default).
    /// </summary>
    public TString(string value, bool readSize, int fixedLength, bool skipZero) : this(value, readSize, fixedLength, skipZero, null) { }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the byte length of the string value after encoding.
    /// </summary>
    public uint Length => (uint)Encoding.GetBytes(Value).Length;

    /// <summary>
    /// The fixed byte count when in fixed‑mode; negative means variable length.
    /// </summary>
    public int FixedLength { get; private set; } = FixedLength;

    /// <summary>
    /// <c>true</c> if this string has a fixed byte length, <c>false</c> if it’s flexible.
    /// </summary>
    public bool IsFixed => FixedLength >= 0;

    /// <summary>
    /// How many trailing zero bytes we snacked on. Zero if not skipping.
    /// </summary>
    public int ZeroCount { get; private set; }

    /// <summary>
    /// The actual string goodness.
    /// </summary>
    private string Value { get; set; } = value ?? string.Empty;

    /// <summary>
    /// The encoding used to convert bytes ↔ characters. Defaults to UTF‑8 if nobody told us otherwise.
    /// </summary>
    public Encoding Encoding { get; set; } = encoding ?? Encoding.UTF8;

    /// <summary>
    /// Total byte size of the structure as it appears in the file.  
    /// (Yeah, the formula looks like someone doubled the cap, but that’s the Telltale way.)
    /// </summary>
    public uint Size =>
        Length + ISize.FOUR_ADD + (skipZero ? (uint)ZeroCount : StringLenCap);

    /// <summary>
    /// Number of <c>char</c>s in the string – required by <see cref="ICollection{T}"/>.
    /// </summary>
    public int Count => Value.Length;

    /// <summary>
    /// Always <c>true</c> – you can’t push new chars into this string, it’s set in stone.
    /// </summary>
    public bool IsReadOnly => true;

    /// <summary>
    /// Where the reader was before we touched it, used for debug reports.
    /// </summary>
    long IParsable.ParsePosition => _ParsePos;

    #endregion

    #region Parsing

    /// <summary>
    /// Reads the string from the binary stream according to all the rules:  
    /// optional length prefix, fixed/variable length, optional zero‑skipping.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;

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

        Report(this, $"({nameof(skipZero)} = {skipZero}, {nameof(readSize)} = {readSize}, {nameof(ZeroCount)} = {ZeroCount})");
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Chomps through trailing zero‑bytes so the next integer starts on a clean alignment.  
    /// Little‑endian junk: the zeros are just padding, not data – we skip ’em and count ’em.
    /// </summary>
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

    /// <inheritdoc/>
    public bool Equals(TString? other) => other is not null && Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as TString);

    /// <inheritdoc/>
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    /// <inheritdoc/>
    public override string ToString() => Value ?? string.Empty;

    /// <inheritdoc/>
    public int CompareTo(TString? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc/>
    public int CompareTo(object? obj)
    {
        if (obj is TString ts)
            return CompareTo(ts);

        throw new ArgumentException(
            $"Object({obj?.GetType()?.Name}) is not a {nameof(TString)}");
    }

    #endregion

    #region Operators

    public static bool operator ==(TString left, TString right) => left?.Value == right?.Value;
    public static bool operator !=(TString left, TString right) => !(left == right);

    public static bool operator <(TString left, TString right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator <=(TString left, TString right) =>
        left is null || left.CompareTo(right) <= 0;

    public static bool operator >(TString left, TString right) =>
        left is not null && left.CompareTo(right) > 0;

    public static bool operator >=(TString left, TString right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;

    #endregion

    #region Implicit Conversions

    /// <summary>
    /// Unwraps a TString back into a plain old string.
    /// </summary>
    public static implicit operator string(TString t) => t?.Value ?? string.Empty;

    /// <summary>
    /// Wraps a plain string into a TString.  
    /// (Uses the collection expression syntax – don't look too closely, it works™)
    /// </summary>
    public static implicit operator TString(string s) => [.. s];

    #endregion

    #region Static Factory

    /// <summary>
    /// Creates a TString from a plain string, same as the implicit conversion but explicit.
    /// </summary>
    public static TString FromValue(string value) => [.. value];

    #endregion

    #region ICollection<char> Implementation

    /// <summary>
    /// Not supported – this collection is read‑only.
    /// </summary>
    public void Add(char item) =>
        throw new NotSupportedException("Collection is read-only.");

    /// <summary>
    /// Not supported – you can't wipe a TString.
    /// </summary>
    public void Clear() =>
        throw new NotSupportedException("Collection is read-only.");

    /// <inheritdoc/>
    public bool Contains(char item) => Value.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(char[] array, int arrayIndex) =>
        Value.CopyTo(0, array, arrayIndex, Value.Length);

    /// <summary>
    /// Not supported – once a character is there, it stays there.
    /// </summary>
    public bool Remove(char item) =>
        throw new NotSupportedException("Collection is read-only.");

    /// <inheritdoc/>
    public IEnumerator<char> GetEnumerator() => Value.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}