using System.Collections;
using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Reg;

/// <summary>
/// A FlagIndexMap reads a sequence of key-symbol pairs from the stream, plus one extra dummy key.
/// In normal mode the extra key comes at the end, in reverse mode it’s at the beginning.
/// </summary>
[Description("Processing a FlagIndexMap structure that includes keys, symbols")]
public class FlagIndexMap : ReportBase, IParsable
{
    #region Constants

    // The size field that’s always present at the start of the map.
    private const uint FlagIndexMapSizeInFile = sizeof(uint);

    #endregion

    #region Fields
    //Do u realy need comment for this?
    private readonly bool reverce = false;

    // Each real entry is a uint key followed by a TSymbolValue (8 bytes) — 12 bytes total.
    private readonly uint ItemSize = sizeof(uint) + new TSymbolValue().Size;

    #endregion

    #region Properties

    /// <summary>
    /// All the entries read from the stream. The number of entries is always one more than
    /// the number of key‑symbol pairs because an extra key with no value is added.
    /// </summary>
    public Hashtable KeyValues = [];

    /// <summary>
    /// Byte count of the whole map, as it sits in the file.
    /// </summary>
    public uint Size =>
        // The extra entry only contributes a uint, not a full ItemSize.
        ((uint)(KeyValues.Count - 1) * ItemSize) + FlagIndexMapSizeInFile + sizeof(uint);

    /// <summary>
    /// Where the reader was when we started parsing.
    /// </summary>
    public long ParsePosition { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a reverse‑mode map (extra key at the front).
    /// </summary>
    public FlagIndexMap(bool reverse)
    {
        reverce = reverse;
    }

    /// <summary>
    /// Creates a normal‑mode map (extra key at the back).
    /// </summary>
    public FlagIndexMap() { }

    #endregion

    #region IParsable

    /// <summary>
    /// Reads the map: a size field, then an optional extra key (depending on mode),
    /// then the key‑symbol pairs, and finally the other extra key.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        ParsePosition = reader.BaseStream.Position;
        long readerFirstPos = reader.BaseStream.Position;

        uint size = reader.ReadUInt32(); // Skip Size
        uint len = size / ItemSize;      // That’s how many real pairs we have

        CheckReverce(reader, true);      // Maybe add an extra key before the pairs
        ReadKeyValues(reader, len);      // Read the actual key‑symbol pairs
        CheckReverce(reader, false);     // Maybe add an extra key after the pairs

        // Make sure we ate exactly as many bytes as the header promised.
        CheckDataMismatch(reader, readerFirstPos, size);

        Report(this, $"(reverce = {reverce})");
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Verifies that the number of bytes read matches the declared size.
    /// If not, somebody messed up the stream alignment.
    /// </summary>
    private static void CheckDataMismatch(BinaryReader reader, long readerFirstPos, uint size)
    {
        long currentPos = reader.BaseStream.Position;
        if (currentPos - readerFirstPos != size)
        {
            throw new InvalidDataException(
                $"FlagIndexMap trailing data mismatch: read up to offset 0x{currentPos:X} ({currentPos}) but declared size was 0x{size:X} ({size}). Remaining bytes are unexplained.");
        }
    }

    /// <summary>
    /// Reads the real key‑symbol pairs. Each one is a uint key followed by a TSymbolValue.
    /// </summary>
    private void ReadKeyValues(BinaryReader reader, uint len)
    {
        for (int i = 0; i < len; ++i)
        {
            uint key = reader.ReadUInt32();
            TSymbolValue symbol = new();
            symbol.Parse(reader);
            KeyValues.Add(key, symbol);
        }
    }

    /// <summary>
    /// Adds the extra dummy entry (key only, no symbol) at the right place depending on the mode.
    /// When cond == reverce, we read an extra uint and store it with a null value.
    /// </summary>
    private void CheckReverce(BinaryReader reader, bool cond)
    {
        if (reverce == cond)
        {
            uint extra = reader.ReadUInt32();
            KeyValues.Add(extra, null);
        }
    }

    #endregion
}