using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.DData;

public class DebugDataList : TCollectionBase<TString>, IParsable
{
    private long _ParsePos;

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

    long IParsable.ParsePosition => _ParsePos;

    uint ISize.Size => throw new NotImplementedException();

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        Items = [];
        while (reader.PeekChar() != -1)
        {
            TString ddata = new(string.Empty, false, TString.VarLength, true);
            ddata.Parse(reader);
            Items.Add(ddata);
        }
    }

    void IParsable.Parse(BinaryReader reader)
    {
        throw new NotImplementedException();
    }
}
