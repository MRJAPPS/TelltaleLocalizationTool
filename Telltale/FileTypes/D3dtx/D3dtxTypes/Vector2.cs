using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;
[Description("Reading coordinates of a 2D Vector")]
public class Vector2 : ReportBase, ITelltaleObject
{

    #region Public props

    public float mX { get; set; }
    public float mY { get; set; }

    #endregion

    #region Implement

    public uint Size => sizeof(float) * 2;

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        mX = reader.ReadSingle();
        mY = reader.ReadSingle();
        Report(this, extra: $"(Vec2{{{mX},{mY}}})");
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(Vector2), name);
        ParseBasicTypes(myElement);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;
        writer.Write(mX);
        writer.Write(mY);
        Report(this, true, $"(Vec2{{{mX},{mY}}})");
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(Vector2));
        myElement.SetAttribute(nameof(name), name);
        AppendBasicTypes(myElement);
        _ = element.AppendChild(myElement);
    }
    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mX, mY;
        mX = element.OwnerDocument.CreateAttribute(nameof(this.mX));
        mY = element.OwnerDocument.CreateAttribute(nameof(this.mY));

        mX.Value = this.mX.ToString();
        mY.Value = this.mY.ToString();

        _ = element.Attributes.Append(mX);
        _ = element.Attributes.Append(mY);
    }

    private void ParseBasicTypes(XmlElement element)
    {
        mX = float.Parse(element.GetAttribute(nameof(mX)));
        mY = float.Parse(element.GetAttribute(nameof(mY)));
    }

    #endregion
}