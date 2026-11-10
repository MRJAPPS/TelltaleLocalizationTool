using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
[Description("Almost done — parsing StreamHeader now")]
public class StreamHeaderInfo(int version) : TCollectionBase<RegionStreamHeader>, ITelltaleObject
{
    #region Private constants
    private const int MinimumSize = sizeof(int) * 3;
    #endregion
    #region Public props

    public int mRegionCount { get; set; }
    public int mAuxDataCount { get; set; }
    public int mTotalDataSize { get; set; }

    #endregion
    #region Implement
    public uint Size
    {
        get
        {
            uint ret = MinimumSize;
            foreach (RegionStreamHeader regHeader in Items)
            {
                ret += regHeader.Size;
            }
            return ret;
        }
    }

    public long Position { get; private set; } = -1;

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(mRegionCount); //mRegionCount [4 bytes]
        writer.Write(mAuxDataCount); //mAuxDataCount [4 bytes]
        writer.Write(mTotalDataSize); //mTotalDataSize [4 bytes]
        foreach (RegionStreamHeader regionStream in Items)
        {
            regionStream.Write(writer);
        }

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(StreamHeaderInfo));
        myElement.SetAttribute(nameof(name), name);
        AppendBasicTypes(myElement);
        int counter = 0;
        foreach (RegionStreamHeader item in Items)
        {
            item.Write(myElement, $"{nameof(item)}_{counter++}");
        }
        _ = element.AppendChild(myElement);
    }

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mRegionCount = reader.ReadInt32(); //[4 bytes]
        mAuxDataCount = reader.ReadInt32(); //[4 bytes]
        mTotalDataSize = reader.ReadInt32(); //[4 bytes]
        Items = new List<RegionStreamHeader>(mRegionCount);
        for (int i = 0; i < mRegionCount; i++)
        {
            RegionStreamHeader item = new(version);
            item.Parse(reader);
            Items.Add(item);
        }

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(StreamHeaderInfo), name);
        ParseBasicTypes(myElement);
        foreach (XmlNode node in myElement.ChildNodes)
        {
            if (node is XmlElement itemElement && itemElement.Name == nameof(RegionStreamHeader))
            {
                string itemName = itemElement.GetAttribute(nameof(name));
                RegionStreamHeader regionStream = new(version);
                regionStream.Parse(myElement, itemName);
                Items.Add(regionStream);
            }
        }
    }

    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mRegionCount, mAuxDataCount, mTotalDataSize, versionAtt;

        mRegionCount = element.OwnerDocument.CreateAttribute(nameof(this.mRegionCount));
        mAuxDataCount = element.OwnerDocument.CreateAttribute(nameof(this.mAuxDataCount));
        mTotalDataSize = element.OwnerDocument.CreateAttribute(nameof(this.mTotalDataSize));
        versionAtt = element.OwnerDocument.CreateAttribute(nameof(version));

        mRegionCount.Value = this.mRegionCount.ToString();
        mAuxDataCount.Value = this.mAuxDataCount.ToString();
        mTotalDataSize.Value = this.mTotalDataSize.ToString();
        versionAtt.Value = version.ToString();

        element.Attributes.Append(mRegionCount);
        element.Attributes.Append(mAuxDataCount);
        element.Attributes.Append(mTotalDataSize);
        element.Attributes.Append(versionAtt);
    }

    private void ParseBasicTypes(XmlElement element)
    {
        mRegionCount = int.Parse(element.GetAttribute(nameof(mRegionCount)));
        mAuxDataCount = int.Parse(element.GetAttribute(nameof(mAuxDataCount)));
        mTotalDataSize = int.Parse(element.GetAttribute(nameof(mTotalDataSize)));
        version = int.Parse(element.GetAttribute(nameof(version)));
    }

    #endregion
}
