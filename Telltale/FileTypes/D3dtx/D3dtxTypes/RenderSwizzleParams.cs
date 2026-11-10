using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;

[Description("Parsing RenderSwizzleParams...")]
public class RenderSwizzleParams : ReportBase, ITelltaleObject
{
    #region Private constants

    private const uint RenderSwizzleParamsSizeInFile = sizeof(uint);

    #endregion

    #region Public props
    public byte mSwizzle1 { get; set; }
    public byte mSwizzle2 { get; set; }
    public byte mSwizzle3 { get; set; }
    public byte mSwizzle4 { get; set; }

    public uint mSwizzle
    {
        set
        {
            byte[] combinedData = BitConverter.GetBytes(value);
            mSwizzle1 = combinedData[0];
            mSwizzle2 = combinedData[1];
            mSwizzle3 = combinedData[2];
            mSwizzle4 = combinedData[3];
        }
        get
        {
            byte[] combinedData =
        [
            mSwizzle1, //mSwizzle A [1 byte]
            mSwizzle2, //mSwizzle B [1 byte]
            mSwizzle3, //mSwizzle C [1 byte]
            mSwizzle4, //mSwizzle D [1 byte]
        ];
            return BitConverter.ToUInt32(combinedData, 0);
        }
    }
    #endregion
    #region Implemnt
    public uint Size => sizeof(byte) * 4 + RenderSwizzleParamsSizeInFile;

    public long Position { get; private set; } = -1;

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write((uint)Marshal.SizeOf(mSwizzle) + RenderSwizzleParamsSizeInFile);//I should've used this(Marshal.SizeOf) for the other cases too, but I doubt it's a big deal.

        byte[] combinedData =
       [
            mSwizzle1, //mSwizzle A [1 byte]
            mSwizzle2, //mSwizzle B [1 byte]
            mSwizzle3, //mSwizzle C [1 byte]
            mSwizzle4, //mSwizzle D [1 byte]
        ];

        writer.Write(combinedData);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(RenderSwizzleParams));
        myElement.SetAttribute(nameof(name), name);

        AppendBasicTypes(myElement);

        _ = element.AppendChild(myElement);
    }

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        _ = reader.ReadUInt32();//Skip block size

        mSwizzle1 = reader.ReadByte();
        mSwizzle2 = reader.ReadByte();
        mSwizzle3 = reader.ReadByte();
        mSwizzle4 = reader.ReadByte();

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(RenderSwizzleParams), name);
        ParseBasicTypes(myElement);
    }

    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mSwizzle = element.OwnerDocument.CreateAttribute(nameof(this.mSwizzle));

        mSwizzle.Value = this.mSwizzle.ToString();

        _ = element.Attributes.Append(mSwizzle);
    }

    private void ParseBasicTypes(XmlElement element) => mSwizzle = uint.Parse(element.GetAttribute(nameof(mSwizzle)));

    #endregion
}