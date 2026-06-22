using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.DData;

public class DebugDataList : TCollectionBase<TString>, IParsable
{
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

    public void Parse(BinaryReader reader)
    {
        Items = [];
        while (reader.PeekChar() != -1)
        {
            TString ddata = new(string.Empty, false, TString.VarLength, true);
            ddata.Parse(reader);
            Items.Add(ddata);
        }
    }
}
