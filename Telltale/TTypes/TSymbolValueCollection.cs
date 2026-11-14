using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.TTypes;
[Description("Reading a list of TSymbols")]
public class TSymbolValueCollection : TCollectionBase<TSymbolValue>, ITelltaleObject
{
    #region Private Constants
    private const uint TSymbolValueCollectionSize = sizeof(uint); //Item count
    private const uint TSymbolValueCollectionSizeInFile = sizeof(uint); //Capacity
    #endregion

    #region Implement

    public MetaStreamContext? msCtx { get; protected set; }

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    public uint Size => ((uint)Items.Count * new TSymbolValue().Size) + TSymbolValueCollectionSizeInFile + TSymbolValueCollectionSize;

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        _ = reader.ReadUInt32();//Skip block size
        int capacity = reader.ReadInt32();
        Items = new List<TSymbolValue>(capacity);
        for (int i = 0; i < capacity; i++)
        {
            TSymbolValue symbol = new();
            symbol.Parse(reader);
            Items.Add(symbol);
        }
        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(TSymbolValueCollection), name);
        Items = [];
        foreach (XmlNode node in myElement.ChildNodes)
        {
            if (node is XmlElement itemElement && itemElement.Name == nameof(TSymbolValue))
            {
                string itemName = itemElement.GetAttribute(nameof(name));
                TSymbolValue symbol = new();
                symbol.Parse(myElement, itemName);
                Items.Add(symbol);
            }
        }
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);
        writer.Write(Items.Count);
        foreach (TSymbolValue item in Items)
        {
            item.Write(writer);
        }

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(TSymbolValueCollection));
        myElement.SetAttribute(nameof(name), name);
        int counter = 0;
        foreach (TSymbolValue item in Items)
        {
            item.Write(myElement, $"{nameof(item)}_{counter++}");
        }
        _ = element.AppendChild(myElement);
    }

    #endregion
}
