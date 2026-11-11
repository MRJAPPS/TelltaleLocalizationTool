using System.ComponentModel;
using System.Globalization;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.Meta.Enums;

namespace TelltaleLocalizationTool.Telltale.Meta.TClassInfo;

[Description("Reading a symbol")]
public class Symbol(ulong crc64 = 0) : ReportBase, ITelltaleObject//We should've used TSymbolValue, but for compatibility we're treating the Symbol in the header as separate — this can be marked as a TODO for later.
{
    #region Public props

    public ulong mCrc64 { get => crc64; set => crc64 = value; }

    #endregion

    #region Implement

    public uint Size => sizeof(ulong);

    public long Position { get; private set; } = -1;


    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(mCrc64);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        AppendBasicTypes(element);
    }

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mCrc64 = reader.ReadUInt64();

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        ParseBasicTypes(reader);
    }

    #endregion

    #region XML

    private void ParseBasicTypes(XmlElement element)
    {
        string value = element.GetAttribute(nameof(mCrc64));

        if (Enum.TryParse(value, out ClassNames className))
        {
            mCrc64 = (ulong)className;
        }
        else if (!ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out crc64))
        {
            throw new XmlException($"Invalid '{nameof(mCrc64)}' value: {value}");
        }
    }

    private void AppendBasicTypes(XmlElement element)
    {
        element.SetAttribute(nameof(mCrc64), this.ToClassNames()?.ToString() ?? mCrc64.ToString(CultureInfo.InvariantCulture));
    }

    #endregion

#if DEBUG
    public override string ToString()
    {
        return this.ToClassNames()?.ToString() ?? mCrc64.ToString(CultureInfo.InvariantCulture);
    }
#endif
}