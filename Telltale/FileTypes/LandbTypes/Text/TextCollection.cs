using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Text;

public class TextCollection : TCollectionBase<TextItem>, IParsable
{
    public uint Size
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public void Parse(BinaryReader reader)
    {
        int capacity = reader.ReadInt32();
        Items = new(capacity);
        for (int i = 0; i < capacity; ++i)
        {
            TextItem textItem = new();
            textItem.Parse(reader);
            Items.Add(textItem);
        }
    }
}