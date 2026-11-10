using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Font.Glyph;

[Description("Reading the metrics for a glyph.")]
public class GlyphMetrics : ReportBase, ITelltaleObject
{

    #region Public props

    public float mXOffset { get; set; }

    public float mYOffset { get; set; }

    public float mXAdvance { get; set; }

    #endregion

    #region Implement


    public uint Size => sizeof(float) * 3;

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mXOffset = reader.ReadSingle();
        mYOffset = reader.ReadSingle();
        mXAdvance = reader.ReadSingle();

        Report(this);
    }

    public void Parse(XmlElement reader, string _)
    {
        mXOffset = float.Parse(reader.GetAttribute(nameof(mXOffset)));
        mYOffset = float.Parse((reader.GetAttribute(nameof(mYOffset))));
        mXAdvance = float.Parse(reader.GetAttribute(nameof(mXAdvance)));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(mXOffset);
        writer.Write(mYOffset);
        writer.Write(mXAdvance);

        Report(this, true);
    }

    public void Write(XmlElement element, string __)
    {
        XmlAttribute mXOffset, mYOffset, mXAdvance;
        mXOffset = element.OwnerDocument.CreateAttribute(nameof(this.mXOffset));
        mYOffset = element.OwnerDocument.CreateAttribute(nameof(this.mYOffset));
        mXAdvance = element.OwnerDocument.CreateAttribute(nameof(this.mXAdvance));

        mXOffset.Value = this.mXOffset.ToString();
        mYOffset.Value = this.mYOffset.ToString();
        mXAdvance.Value = this.mXAdvance.ToString();

        _ = element.Attributes.Append(mXOffset);
        _ = element.Attributes.Append(mYOffset);
        _ = element.Attributes.Append(mXAdvance);
    }

    #endregion
}
