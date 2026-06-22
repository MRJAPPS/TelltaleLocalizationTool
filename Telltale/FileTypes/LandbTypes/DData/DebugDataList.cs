using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.DData;

[Description("Process the debug section of the file")]
public class DebugDataList : TCollectionBase<TString>, IParsable
{
    #region Private Fields


    #endregion

    #region Public Properties

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

    public long Position { get; private set; }

    #endregion

    #region Implementation

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