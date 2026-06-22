using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.LnagRes.Text;

public class TextCollection : TCollectionBase<TextItem>, IParsable
{
    private long _ParsePos;
    private const uint TextCollectionSizeInFile = sizeof(uint);
    private const uint TextCollectionLenSize = sizeof(uint);
    public uint Size
    {
        get
        {
            uint size = TextCollectionSizeInFile + TextCollectionLenSize;
            foreach (TextItem textItem in Items)
            {
                size += textItem.Size;
            }
            return size;
        }
    }

    public long ParsePosition => _ParsePos;

    public void Parse(BinaryReader reader)
    {
        _ParsePos = reader.BaseStream.Position;
        Items = new(reader.ReadInt32());
        for (int i = 0; i < Capacity; ++i)
        {
            TextItem textItem = new();
            textItem.Parse(reader);
            Items.Add(textItem);
        }
    }
}