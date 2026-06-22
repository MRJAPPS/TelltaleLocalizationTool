using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.DData;

/// <summary>
/// Represents the debug data section of a Landb file.
/// This is a variable‑length list of <see cref="TString"/> objects – no count prefix,
/// just a stream of debug strings until the byte well runs dry.
/// Each string is read with <c>readSize = false</c> (no size prefix in the TString itself),
/// <c>FixedLength = VarLength</c> (so the length is read on‑the‑fly), and
/// <c>skipZero = true</c> (trailing zero‑padding gets munched after every string).
/// </summary>
[Description("Process the debug section of the file")]
public class DebugDataList : TCollectionBase<TString>, IParsable
{
    #region Private Fields


    #endregion

    #region Public Properties

    /// <summary>
    /// Total byte size of the whole debug data block, computed by summing the
    /// <see cref="TString.Size"/> of every string in the list.
    /// </summary>
    public uint Size
    {
        get
        {
            uint size = 0;
            foreach (TString ddata in Items)
            {
                size += ddata.Size;
            }
            return size;
        }
    }

    /// <summary>
    /// The stream offset where parsing began. Used by the report to show exactly where
    /// this debug section sits in the file.
    /// </summary>
    public long Position { get; private set; }

    #endregion

    #region Implementation

    /// <summary>
    /// Reads the debug section from the binary stream.
    /// Since there's no count or terminator, we rely entirely on the stream ending
    /// (i.e., when <see cref="BinaryReader.PeekChar()"/> returns -1).
    /// Each string is parsed as a variable‑length, zero‑skipped <see cref="TString"/>.
    /// </summary>
    /// <param name="reader">The binary reader positioned at the start of the debug data.</param>
    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // Start with a clean slate – the underlying Items list gets replaced.
        Items = [];

        // Keep chewing strings until the stream is completely empty.
        // PeekChar() returns -1 when there are no more bytes, which is our only stop signal.
        while (reader.PeekChar() != -1)
        {
            // Important parameters:
            //  - readSize = false   → the TString won't look for an explicit 4‑byte size cap
            //  - FixedLength = VarLength → the length is read from a leading int inside the string block
            //  - skipZero = true    → after the string bytes, zero‑padding is gobbled up
            TString ddata = new(string.Empty, false, TString.VarLength, true);
            ddata.Parse(reader);
            Items.Add(ddata);
        }

        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        foreach (TString item in Items)
        {
            item.Write(writer);
        }
    }

    #endregion
}