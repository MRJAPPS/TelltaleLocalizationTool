using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.Meta.TClassInfo;

[Description("Reading a symbol")]
public class Symbol(ulong crc64 = 0) : ReportBase, ITelltaleObject
{
    #region Public props

    public ulong mCrc64 { get => crc64; set => crc64 = value; }

    #endregion

    #region Implement

    public uint Size => sizeof(ulong);

    public long Position { get; private set; }


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
        mCrc64 = ulong.Parse(element.GetAttribute(nameof(mCrc64)));
    }

    private void AppendBasicTypes(XmlElement element)
    {
        element.SetAttribute(nameof(mCrc64), mCrc64.ToString());
    }

    #endregion

}