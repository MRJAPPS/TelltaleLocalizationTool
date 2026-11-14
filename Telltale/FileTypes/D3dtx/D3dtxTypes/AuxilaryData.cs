using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.TTypes;
using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;
[Description("Parsing AuxilaryData!")]
public class AuxilaryData(int mAuxDataCount) : ReportBase, ITelltaleObject
{
    #region private constants

    private const uint AuxilaryDataSize = sizeof(uint);
    private const uint PaddingSize = sizeof(uint);
    private const uint UnkownSize = sizeof(ulong);

    #endregion

    #region Public 

    public TSymbolValue symbol { get; set; } = new();
    public ulong Unkown { get; set; }
    public uint Padding { get; set; }
    public byte[] Data { get; set; } = [];

    #endregion

    #region Implement

    public MetaStreamContext? msCtx { get; protected set; }

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    private uint AuxilaryDataSizeExtra => AuxilaryDataSize + PaddingSize + symbol.Size + UnkownSize;

    public uint Size => mAuxDataCount > 0 ? AuxilaryDataSizeExtra + (uint)Data.Length : 0;

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        if (mAuxDataCount <= 0)
        {
            return;
        }

        int size = reader.ReadInt32();
        symbol.Parse(reader);
        Padding = reader.ReadUInt32();
        Unkown = reader.ReadUInt64();
        Data = new byte[size - AuxilaryDataSizeExtra];
        _ = reader.Read(Data);
        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        if (mAuxDataCount <= 0)
        {
            return;
        }

        XmlElement myElement = reader.GetSingleElement(nameof(AuxilaryData), name);
        ParseBasicTypes(myElement);
        if (myElement.SelectSingleNode($"./TSymbolValue[@name='{nameof(symbol)}']") != null)
        {
            symbol.Parse(myElement, nameof(symbol));
        }
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;
        if (mAuxDataCount <= 0)
        {
            return;
        }

        writer.Write(Size);
        symbol.Write(writer);
        writer.Write(Padding);
        writer.Write(Unkown);
        writer.Write(Data);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        if (mAuxDataCount <= 0)
        {
            return;
        }

        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(AuxilaryData));
        myElement.SetAttribute(nameof(name), name);
        AppendBasicTypes(myElement);
        symbol.Write(myElement, nameof(symbol));
        _ = element.AppendChild(myElement);
    }

    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlElement Data = element.OwnerDocument.CreateElement(nameof(this.Data));
        XmlAttribute Unkown = element.OwnerDocument.CreateAttribute(nameof(this.Unkown));
        XmlAttribute Padding = element.OwnerDocument.CreateAttribute(nameof(this.Padding));

        Data.InnerText = System.Convert.ToBase64String(this.Data);
        Unkown.Value = this.Unkown.ToString();
        Padding.Value = this.Padding.ToString();

        _ = element.Attributes.Append(Unkown);
        _ = element.Attributes.Append(Padding);
        _ = element.AppendChild(Data);
    }

    private void ParseBasicTypes(XmlElement element)
    {
        XmlElement Data = element.GetSingleElement(nameof(this.Data));
        Unkown = ulong.Parse(element.GetAttribute(nameof(Unkown)));
        Padding = uint.Parse(element.GetAttribute(nameof(Padding)));
        this.Data = System.Convert.FromBase64String(Data.InnerText);
    }

    #endregion
}
