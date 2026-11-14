using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.TTypes;
using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.T3ToonGraReg;
[Description("Parsing a Collection of T3ToonGradientRegion...")]
public class T3ToonGradientRegionCollection : TCollectionBase<T3ToonGradientRegionItem>, ITelltaleObject
{
    #region Private constants

    private const uint T3ToonGradientRegionSize = sizeof(uint);//Items.Count
    private const uint T3ToonGradientRegionSizeInFile = sizeof(uint);//Blcok size,Capacity,... Why do I keep writing this? :/

    #endregion

    #region Implement

    public MetaStreamContext? msCtx { get; protected set; }

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    public uint Size => ((uint)Items.Count * new T3ToonGradientRegionItem().Size) + T3ToonGradientRegionSize + T3ToonGradientRegionSizeInFile;

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        _ = reader.ReadUInt32();
        int capacity = reader.ReadInt32();
        Items = new List<T3ToonGradientRegionItem>(capacity);
        for (int i = 0; i < capacity; i++)
        {
            T3ToonGradientRegionItem item = new();
            item.Parse(reader);
            Items.Add(item);
        }

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(T3ToonGradientRegionCollection), name);
        Items = [];
        foreach (XmlNode node in myElement.ChildNodes)
        {
            if (node is XmlElement itemElement && itemElement.Name == nameof(T3ToonGradientRegionItem))
            {
                string itemName = itemElement.GetAttribute(nameof(name));
                T3ToonGradientRegionItem t3Toon = new();
                t3Toon.Parse(myElement, itemName);
                Items.Add(t3Toon);
            }
        }
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);
        writer.Write(Items.Count);
        foreach (T3ToonGradientRegionItem t3Toon in Items)
        {
            t3Toon.Write(writer);
        }

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(T3ToonGradientRegionCollection));
        myElement.SetAttribute(nameof(name), name);
        int counter = 0;
        foreach (T3ToonGradientRegionItem item in Items)
        {
            item.Write(myElement, $"{nameof(item)}_{counter++}");
        }
        _ = element.AppendChild(myElement);
    }
    #endregion
}
