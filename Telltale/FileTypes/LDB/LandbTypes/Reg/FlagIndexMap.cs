using System.ComponentModel;
using System.Globalization;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Reg;

[Description("Processing a FlagIndexMap structure that includes keys, symbols")]
public class FlagIndexMap : ReportBase, ILandb
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

    public OrderedDictionary<uint, TSymbolValue?> KeyValues = [];

    public uint Size =>
        // The extra entry only contributes a uint, not a full ItemSize.
        ((uint)(KeyValues.Count - 1) * ItemSize) + FlagIndexMapSizeInFile + sizeof(uint);

    public long Position { get; private set; }

    #endregion

    #region Constructors

    public FlagIndexMap(bool reverse)
    {
        reverce = reverse;
    }

    public FlagIndexMap() { }

    #endregion

    #region Implement

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        long readerFirstPos = reader.BaseStream.Position;

        uint size = reader.ReadUInt32(); // Skip Size
        uint len = size / ItemSize;      // That’s how many real pairs we have

        CheckReverce(reader, true);      // Maybe add an extra key before the pairs
        ReadKeyValues(reader, len);      // Read the actual key‑symbol pairs
        CheckReverce(reader, false);     // Maybe add an extra key after the pairs

        // Make sure we ate exactly as many bytes as the header promised.
        CheckDataMismatch(reader, readerFirstPos, size);

        Report(this, false, $"(reverce = {reverce})");
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(FlagIndexMap), name);

        KeyValues.Clear();

        foreach (XmlNode node in myElement.ChildNodes)
        {
            if (node is not XmlElement itemElement)
                continue;

            if (itemElement.Name != "KeyValueItem")
                continue;

            uint key = uint.Parse(itemElement.GetAttribute(nameof(key)));

            bool isNull = itemElement.HasAttribute("isNull") && bool.Parse(itemElement.GetAttribute("isNull"));

            if (isNull)
            {
                KeyValues[key] = null;
                continue;
            }

            XmlElement? symbolElement = null;
            foreach (XmlNode child in itemElement.ChildNodes)
            {
                if (child is XmlElement childElement)
                {
                    symbolElement = childElement;
                    break;
                }
            }

            if (symbolElement is null)
                throw new InvalidDataException($"I can't find symbol element for key '{key}'.");

            TSymbolValue symbol = new();

            symbol.Parse(itemElement, nameof(symbol));
            KeyValues[key] = symbol;
        }

        Report(this, false, $"(reverce = {reverce})");
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);

        CheckReverce(writer, true);
        WriteReadKeyValues(writer);
        CheckReverce(writer, false);

        Report(this, true, $"(reverce = {reverce})");
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(FlagIndexMap));
        myElement.SetAttribute(nameof(name), name);

        foreach (uint key in KeyValues.Keys)
        {
            XmlElement itemElement = myElement.OwnerDocument.CreateElement("KeyValueItem");
            itemElement.SetAttribute(nameof(key), key.ToString(CultureInfo.InvariantCulture));

            TSymbolValue? symbol = KeyValues[key];
            if (symbol is not null)
            {
                symbol.Write(itemElement, nameof(symbol));
            }
            else
            {
                itemElement.SetAttribute("isNull", "true");
            }

            myElement.AppendChild(itemElement);
        }

        _ = element.AppendChild(myElement);
    }

    #endregion

    #region Private Helpers

    private static void CheckDataMismatch(BinaryReader reader, long readerFirstPos, uint size)
    {
        long currentPos = reader.BaseStream.Position;
        if (currentPos - readerFirstPos != size)
        {
            throw new InvalidDataException(
                $"FlagIndexMap trailing data mismatch: read up to offset 0x{currentPos:X} ({currentPos}) but declared size was 0x{size:X} ({size}). Remaining bytes are unexplained.");
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
    private void WriteReadKeyValues(BinaryWriter writer)
    {
        int nulls = 0;
        foreach (uint key in KeyValues.Keys)
        {
            TSymbolValue? symbol = KeyValues[key];
            if (symbol == null && ++nulls > 1)
            {
                throw new NullReferenceException("There was supposed to be only one null in the list; this was unpredictable! Please report this along with a sample file.");
            }

            if (symbol is not null)
            {
                writer.Write(key);
                symbol.Write(writer);
            }
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

    private void CheckReverce(BinaryWriter writer, bool cond)
    {
        if (reverce == cond)
        {
            foreach (uint key in KeyValues.Keys)
            {
                if (KeyValues[key] == null)
                {
                    writer.Write(key);
                    break;
                }
            }
        }
    }

    #endregion
}