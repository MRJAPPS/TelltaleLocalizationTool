using System;
using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Xml;
using System.Xml.Linq;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Font.Glyph;

[Description("Uh oh, I've hit the glyph list! Let's dive in!")]
public class GlyphInfoCollection : TCollectionBase<GlyphInfo>, ITelltaleObject
{
    #region Private constants
    private const uint GlyphInfoCollectionLenSize = sizeof(uint);
    private const uint GlyphInfoCollectionSizeIFile = sizeof(uint);
    #endregion


    #region Implement

    public MetaStreamContext? msCtx { get; protected set; }

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    public uint Size => GlyphInfoCollectionLenSize + GlyphInfoCollectionSizeIFile + new GlyphInfo().Size * (uint)Items.Count;

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        _ = reader.ReadUInt32();//Skip capacity in file(Size — or whatever you wanna call it)
        int length = reader.ReadInt32();
        Items = new(length);
        for (int i = 0; i < length; ++i)
        {
            GlyphInfo glyphInfo = new();
            glyphInfo.Parse(reader);
            Items.Add(glyphInfo);
        }

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(GlyphInfoCollection), name);
        foreach (XmlNode node in myElement.ChildNodes)
        {
            if (node is XmlElement itemElement && itemElement.Name == nameof(GlyphInfo))
            {
                string itemName = itemElement.GetAttribute(nameof(name));
                GlyphInfo glyphInfo = new();
                glyphInfo.Parse(myElement, itemName);
                Items.Add(glyphInfo);
            }
        }
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);//Write capacity in file
        writer.Write(Items.Count);
        foreach (GlyphInfo glyphInfo in Items)
        {
            glyphInfo.Write(writer);
        }

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(GlyphInfoCollection));
        myElement.SetAttribute(nameof(name), name);
        int counter = 0;
        foreach (GlyphInfo item in Items)
        {
            item.Write(myElement, $"{nameof(item)}_{counter++}");
        }
        _ = element.AppendChild(myElement);
    }

    #endregion
}
