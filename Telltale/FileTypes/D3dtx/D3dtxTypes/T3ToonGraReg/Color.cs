using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.T3ToonGraReg;
[Description("Reading colors")]
public class Color : ReportBase, ITelltaleObject
{
    #region Public props

    public float r;
    public float g;
    public float b;
    public float a;

    #endregion

    #region Implement

    public MetaStreamContext? msCtx { get; protected set; }

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    public uint Size => sizeof(float) * 4;

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        r = reader.ReadSingle();
        g = reader.ReadSingle();
        b = reader.ReadSingle();
        a = reader.ReadSingle();
        Report(this, extra: $"(R: {r} G: {g} B: {b} A: {a})");
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(Color), name);
        ParseBasicTypes(myElement);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;
        writer.Write(r);
        writer.Write(g);
        writer.Write(b);
        writer.Write(a);
        Report(this, true, $"(R: {r} G: {g} B: {b} A: {a})");
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(Color));

        myElement.SetAttribute(nameof(name), name);
        AppendBasicTypes(myElement);

        _ = element.AppendChild(myElement);
    }
    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute r, g, b, a;
        r = element.OwnerDocument.CreateAttribute(nameof(this.r));
        g = element.OwnerDocument.CreateAttribute(nameof(this.g));
        b = element.OwnerDocument.CreateAttribute(nameof(this.b));
        a = element.OwnerDocument.CreateAttribute(nameof(this.a));

        r.Value = this.r.ToString();
        g.Value = this.g.ToString();
        b.Value = this.b.ToString();
        a.Value = this.a.ToString();

        _ = element.Attributes.Append(r);
        _ = element.Attributes.Append(g);
        _ = element.Attributes.Append(b);
        _ = element.Attributes.Append(a);

    }

    private void ParseBasicTypes(XmlElement element)
    {
        r = float.Parse(element.GetAttribute(nameof(r)));
        g = float.Parse(element.GetAttribute(nameof(g)));
        b = float.Parse(element.GetAttribute(nameof(b)));
        a = float.Parse(element.GetAttribute(nameof(a)));
    }

    #endregion
}