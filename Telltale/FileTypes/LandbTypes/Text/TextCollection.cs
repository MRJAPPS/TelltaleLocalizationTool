using System.Collections;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Text;

public class TextCollection : TCollectionBase<TextItem>, IParsable
{
    public void Parse(BinaryReader reader)
    {
        Items = new(reader.ReadInt32());
        for (int i = 0; i < Cpacity; ++i)
        {
            TextItem textItem = new();
            textItem.Parse(reader);
            Items.Add(textItem);
        }
    }
}