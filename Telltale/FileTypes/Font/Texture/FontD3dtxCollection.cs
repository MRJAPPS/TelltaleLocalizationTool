using System.ComponentModel;
using System.Reflection;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Font.Texture;

[Description("Got to the cool part — now I need to read the font textures!")]
public class FontD3dtxCollection : TCollectionBase<ID3DTX>, ITelltaleObject
{
    #region Private constants

    private const uint FontD3dtxCollectionSizeInFile = sizeof(uint);
    private const uint FontD3dtxCollectionLenInFile = sizeof(uint);

    #endregion

    #region Implement

    public MetaStreamContext? msCtx { get; protected set; }

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    public uint Size
    {
        get
        {
            uint size = FontD3dtxCollectionSizeInFile + FontD3dtxCollectionLenInFile;
            foreach (ID3DTX d3dtx in Items)
            {
                size += d3dtx.Size;
            }
            return size;
        }
    }

    public uint HeaderSizeSum => (uint)Items.Sum(item => item.mD3dtxMetaData.Size) + FontD3dtxCollectionSizeInFile + FontD3dtxCollectionLenInFile;

    public uint PixelDataSizeSum => Size - HeaderSizeSum;

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        ParseMetadata(reader);
        ParsePixelData(reader);
    }

    public void Parse(XmlElement reader, string name)
    {
        //TODO: Refactor this — the collection parsing pattern is the same everywhere, and you've written it for every single collection when you could just make this a function with a few parameters.
        XmlElement myElement = reader.GetSingleElement(nameof(FontD3dtxCollection), name);
        foreach (XmlNode node in myElement.ChildNodes)
        {
            if (node is XmlElement itemElement && itemElement.Name == nameof(ID3DTX))
            {
                string itemName = itemElement.GetAttribute(nameof(name));
                ID3DTX d3dtxv = D3dtxFactory.CreateD3dtx(msCtx?.Game ?? throw new NullReferenceException("Where did I or you screw up that you're seeing this error? Either your files are messed up or the auto-assignment to msCtx part! You can report this bug to me."));
                d3dtxv.Parse(myElement, itemName);
                Items.Add(d3dtxv);
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
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(FontD3dtxCollection));
        myElement.SetAttribute(nameof(name), name);
        int counter = 0;
        foreach (ID3DTX item in Items)
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
            ID3DTX d3dtx = D3dtxFactory.CreateD3dtx(msCtx?.Game ?? throw new NullReferenceException("Where did I or you screw up that you're seeing this error? Either your files are messed up or the auto-assignment to msCtx part! You can report this bug to me."));

            d3dtx.WriteMsvHeader = true;
            d3dtx.ReadMsvHeader = false;
            d3dtx.GetType().GetProperty(nameof(ID3DTX.msCtx), BindingFlags.Instance | BindingFlags.Public)!.SetValue(d3dtx, msCtx);//TODO: make setter public
            d3dtx.ShareMetaStreamContext();
           
            d3dtx.mD3dtxMetaData.Parse(reader);
            Items.Add(d3dtx);
        }

        Report(this, extra: nameof(ParseMetadata));
    }

    public void ParsePixelData(BinaryReader reader)
    {
        long pos = Position;
        Position = reader.BaseStream.Position;

        //Now we need to read AsyncData — which is really just reading PixelData for each texture, in the same order we read them before.
        foreach (ID3DTX item in Items)
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

        foreach (ID3DTX d3dtxv in Items)
        {
            d3dtxv.mD3dtxMetaData.Write(writer);
        }

        Report(this, true, nameof(WriteMetaData));
    }

    public void WritePixelData(BinaryWriter writer)
    {
        long pos = Position;
        Position = writer.BaseStream.Position;

        //Now we need to write AsyncData — which is really just writing PixelData for each texture, in the same order we read them before.
        foreach (ID3DTX d3dtxv in Items)
        {
            d3dtxv.WritePixelDatas(writer);
        }

        Report(this, extra: nameof(WritePixelData));
        Position = pos;//reset to header or default
    }
    #endregion
}
