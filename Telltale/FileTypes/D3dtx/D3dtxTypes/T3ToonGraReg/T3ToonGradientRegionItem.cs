using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.T3ToonGraReg;

[Description("Parsing T3ToonGradientRegionItem")]
public class T3ToonGradientRegionItem : ReportBase, ITelltaleObject
{

    #region Public props

    public Color mColor { get; set; } = new();
    public float mSize { get; set; }

    public uint Size => mColor.Size + sizeof(float);

    public long Position { get; private set; }



    #endregion

    #region Implement

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        mColor.Parse(reader);
        mSize = reader.ReadUInt32();
        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(T3ToonGradientRegionItem), name);
        mColor.Parse(myElement, nameof(mColor));
        ParseBasicTypes(myElement);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;
        mColor.Write(writer);
        writer.Write(mSize);
        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(T3ToonGradientRegionItem));
        myElement.SetAttribute(nameof(name), name);

        mColor.Write(myElement, nameof(mColor));
        AppendBasicTypes(myElement);
    }

    #endregion
    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mSize;
        mSize = element.OwnerDocument.CreateAttribute(nameof(this.mSize));

        mSize.Value = this.mSize.ToString();

        _ = element.Attributes.Append(mSize);
    }

    private void ParseBasicTypes(XmlElement element)
    {
        mSize = float.Parse(element.GetAttribute(nameof(mSize)));
    }

    #endregion

}
