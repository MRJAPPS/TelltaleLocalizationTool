using System.Collections;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Reg;

public class FlagIndexMap : IParsable
{
    private const uint FlagIndexMapSizeInFile = sizeof(uint);

    private long _ParsePos;
    private readonly bool reverce = false;
    private readonly uint ItemSize = sizeof(uint) + new TSymbolValue().Size;

    /// <summary>
    /// Maps an ID key to the TSymbolValue that immediately follows it in the stream (in normal mode).
    /// In reverse mode, the first value is a raw uint; all subsequent values are TSymbolValue.
    /// </summary>
    public Hashtable KeyValues = [];

    public FlagIndexMap(bool reverse)
    {
        reverce = reverse;
    }

    public FlagIndexMap() { }

    public uint Size => ((uint)(KeyValues.Count - 1) * ItemSize) + FlagIndexMapSizeInFile + sizeof(uint);//forextra

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        long readerFirstPos = reader.BaseStream.Position;
        uint size = reader.ReadUInt32() /*Skip Size*/;
        uint len = size / ItemSize;
        CheckReverce(reader, true);
        ReadKeyValues(reader, len);
        CheckReverce(reader, false);
        CheckDataMismatch(reader, readerFirstPos, size);
    }

    private static void CheckDataMismatch(BinaryReader reader, long readerFirstPos, uint size)
    {
        long currentPos = reader.BaseStream.Position;
        if (currentPos - readerFirstPos != size)
        {
            throw new InvalidDataException($"FlagIndexMap trailing data mismatch: read up to offset 0x{currentPos:X} ({currentPos}) but declared size was 0x{size:X} ({size}). Remaining bytes are unexplained.");
        }
    }

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

    private void CheckReverce(BinaryReader reader, bool cond)
    {
        if (reverce == cond)
        {
            uint extra = reader.ReadUInt32();
            KeyValues.Add(extra, null);
        }
    }
}
