using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.FileTypes.Font.Glyph;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Font.Texture;

[Description("Got to the cool part — now I need to read the font textures!")]
public class FontD3dtxV9Collection() : TCollectionBase<D3DTXV9>, ITelltaleObject
{
    #region Private constants

    private const uint FontD3dtxV9CollectionSizeInFile = sizeof(uint);
    private const uint FontD3dtxV9CollectionLenInFile = sizeof(uint);

    #endregion

    public uint Size
    {
        get
        {
            uint size = FontD3dtxV9CollectionSizeInFile + FontD3dtxV9CollectionLenInFile;
            foreach (D3DTXV9 d3dtxv9 in Items)
            {
                size += d3dtxv9.Size;
            }
            return size;
        }
    }

    public uint HeaderSizeSum => (uint)Items.Sum(item => item.mD3dtxMetaData.Size) + FontD3dtxV9CollectionSizeInFile + FontD3dtxV9CollectionLenInFile;

    public uint PixelDataSizeSum => Size - HeaderSizeSum;

    public long Position { get; private set; }

    public void Parse(BinaryReader reader)
    {
        ParseMetadata(reader);
        ParsePixelData(reader);
    }

    public void Parse(XmlElement reader, string name)
    {
        //TODO: Refactor this — the collection parsing pattern is the same everywhere, and you've written it for every single collection when you could just make this a function with a few parameters.
        XmlElement myElement = reader.GetSingleElement(nameof(FontD3dtxV9Collection), name);
        foreach (XmlNode node in myElement.ChildNodes)
        {
            if (node is XmlElement itemElement && itemElement.Name == nameof(D3DTXV9))
            {
                string itemName = itemElement.GetAttribute(nameof(name));
                D3DTXV9 d3dtxv9 = new();
                d3dtxv9.Parse(myElement, itemName);
                Items.Add(d3dtxv9);
            }
        }
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        WriteMetaData(writer);
        WritePixelData(writer);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        //TODO: Refactor this too...
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(FontD3dtxV9Collection));
        myElement.SetAttribute(nameof(name), name);
        int counter = 0;
        foreach (D3DTXV9 item in Items)
        {
            item.Write(myElement, $"{nameof(item)}_{counter++}");
        }
        _ = element.AppendChild(myElement);
    }

    public void ParseMetadata(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        _ = reader.ReadUInt32();//Skip size in file
        int length = reader.ReadInt32();
        Items = new(length);
        for (int i = 0; i < length; ++i)
        {
            D3DTXV9 d3dtxv9 = new(false, false, false);
            d3dtxv9.mD3dtxMetaData.Parse(reader);
            Items.Add(d3dtxv9);
        }

        Report(this, extra: nameof(ParseMetadata));
    }

    public void ParsePixelData(BinaryReader reader)
    {
        long pos = Position;
        Position = reader.BaseStream.Position;

        //Now we need to read AsyncData — which is really just reading PixelData for each texture, in the same order we read them before.
        foreach (D3DTXV9 item in Items)
        {
            item.ParsePixelDatas(reader);
        }

        Report(this, extra: nameof(ParsePixelData));
        Position = pos;//reset to header or default
    }

    public void WriteMetaData(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(HeaderSizeSum);
        writer.Write(Items.Count);

        foreach (D3DTXV9 d3dtxv9 in Items)
        {
            d3dtxv9.mD3dtxMetaData.Write(writer);
        }

        Report(this, true, nameof(WriteMetaData));
    }

    public void WritePixelData(BinaryWriter writer)
    {
        long pos = Position;
        Position = writer.BaseStream.Position;

        //Now we need to write AsyncData — which is really just writing PixelData for each texture, in the same order we read them before.
        foreach (D3DTXV9 d3dtxv9 in Items)
        {
            d3dtxv9.WritePixelDatas(writer);
        }

        Report(this, extra: nameof(WritePixelData));
        Position = pos;//reset to header or default
    }
}
