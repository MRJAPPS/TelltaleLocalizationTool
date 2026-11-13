using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Font.Glyph;

[Description("I'm reading a glyph's coordinates from the Atlas.")]
public class TRect : ReportBase, ITelltaleObject
{
    #region Public props

    public float Left { get; set; }

    public float Right { get; set; }

    public float Top { get; set; }

    public float Bottom { get; set; }

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

        Left = reader.ReadSingle();
        Right = reader.ReadSingle();
        Top = reader.ReadSingle();
        Bottom = reader.ReadSingle();

        Report(this, extra: $"RECT: [{Left},{Right},{Top},{Bottom}]");
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(TRect), name);
        ParseBasicTypes(myElement);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Left);
        writer.Write(Right);
        writer.Write(Top);
        writer.Write(Bottom);

        Report(this, true, $"RECT: [{Left},{Right},{Top},{Bottom}]");
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(TRect));
        myElement.SetAttribute(nameof(name), name);//TODO: This 2 lines could be refactored — the same pattern is used all over the place.

        AppendBasicTypes(myElement);

        _ = element.AppendChild(myElement);
    }


    #endregion

    #region XML

    private void ParseBasicTypes(XmlElement element)
    {
        Left = float.Parse(element.GetAttribute(nameof(Left)));
        Right = float.Parse(element.GetAttribute(nameof(Right)));
        Top = float.Parse(element.GetAttribute(nameof(Top)));
        Bottom = float.Parse(element.GetAttribute(nameof(Bottom)));
    }

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute Left, Right, Top, Bottom;

        Left = element.OwnerDocument.CreateAttribute(nameof(this.Left));
        Right = element.OwnerDocument.CreateAttribute(nameof(this.Right));
        Top = element.OwnerDocument.CreateAttribute(nameof(this.Top));
        Bottom = element.OwnerDocument.CreateAttribute(nameof(this.Bottom));

        Left.Value = this.Left.ToString();
        Right.Value = this.Right.ToString();
        Top.Value = this.Top.ToString();
        Bottom.Value = this.Bottom.ToString();

        _ = element.Attributes.Append(Left);
        _ = element.Attributes.Append(Right);
        _ = element.Attributes.Append(Top);
        _ = element.Attributes.Append(Bottom);
    }

    #endregion
}