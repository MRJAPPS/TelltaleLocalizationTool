using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;

[Description("Reading 4-byte(T3SamplerStateValue) T3SamplerStateBlock data")]
public class T3SamplerStateBlock : ReportBase, ITelltaleObject
{
    #region Private constants 

    private const int T3SamplerStateBlockSizeInFile = sizeof(uint);

    #endregion
    #region Public Props

    public T3SamplerStateValue mData { get; set; }

    #endregion

    #region Implemnt
    public uint Size => sizeof(uint) + T3SamplerStateBlockSizeInFile;

    public long Position { get; private set; }

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        _ = reader.ReadUInt32();//Skip block size
        mData = (T3SamplerStateValue)reader.ReadUInt32();
        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(T3SamplerStateBlock), name);
        mData = Enum.Parse<T3SamplerStateValue>(myElement.GetAttribute(nameof(mData)));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);
        writer.Write((uint)mData);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(T3SamplerStateBlock));
        myElement.SetAttribute(nameof(name), name);
        AppendBasicTypes(myElement);
        _ = element.AppendChild(myElement);
    }

    public override string ToString()
    {
        StringBuilder enumFlags = new();

        var allEnums = Enum.GetValues<T3SamplerStateValue>();

        foreach (var enumMask in allEnums)
        {
            if ((mData & enumMask) != 0)
            {
                enumFlags.AppendFormat(
                    "{0}: {1} | ",
                    Enum.GetName(enumMask),
                    mData & enumMask
                );
            }
        }

        if (enumFlags.Length > 0)
        {
            enumFlags.Remove(enumFlags.Length - 3, 3);
        }
        else
        {
            enumFlags.Append("None");
        }

        return string.Format("[T3SamplerStateBlock] mData: {0} ({1})", enumFlags.ToString(), mData);
    }


    #endregion

    #region XML
    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mData = element.OwnerDocument.CreateAttribute(nameof(this.mData));
        mData.Value = this.mData.ToString();
        _ = element.Attributes.Append(mData);
    }
    private void ParseBasicTypes(XmlElement element) => this.mData = (T3SamplerStateValue)uint.Parse(element.GetAttribute(nameof(this.mData)));
    #endregion
}
