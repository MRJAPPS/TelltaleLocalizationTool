using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using TelltaleLocalizationTool.Telltale.Meta.TClassInfo;

namespace TelltaleLocalizationTool.Telltale.Meta.Versions;

[Description("Reading a MSV6 meta header")]
public class MSV6 : ReportBase, ITelltaleObject, IMetaHeader
{
    #region Public props

    public string mMetaStreamVersion { get; set; } = MetaMagics.MSV6;

    public uint mDefaultSectionChunkSize { get; set; }

    public uint mDebugSectionChunkSize { get; set; }

    public uint mAsyncSectionChunkSize { get; set; }

    public uint mClassNamesLength { get; set; }

    public ClassName[] mClassNames { get; set; } = [];

    #endregion

    #region Implement

    public uint Size => checked(5 * sizeof(uint) + (uint)mClassNames.Sum(item => item.Size));

    public long Position { get; private set; } = -1;

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        mClassNamesLength = (uint)mClassNames.Length;
        ByteFunctions.WriteFixedString(writer, mMetaStreamVersion);
        writer.Write(mDefaultSectionChunkSize);
        writer.Write(mDebugSectionChunkSize);
        writer.Write(mAsyncSectionChunkSize);
        writer.Write(mClassNamesLength);

        foreach (ClassName item in mClassNames)
        {
            item.Write(writer);
        }

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(MSV6));
        myElement.SetAttribute(nameof(name), name);

        mClassNamesLength = (uint)mClassNames.Length;
        AppendBasicTypes(myElement);

        for (int i = 0; i < mClassNames.Length; i++)
        {
            mClassNames[i].Write(myElement, $"{nameof(mClassNames)}_{i}");
        }

        _ = element.AppendChild(myElement);
    }

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mMetaStreamVersion = ByteFunctions.ReadFixedString(reader, 4);
        mDefaultSectionChunkSize = reader.ReadUInt32();
        mDebugSectionChunkSize = reader.ReadUInt32();
        mAsyncSectionChunkSize = reader.ReadUInt32();
        mClassNamesLength = reader.ReadUInt32();

        mClassNames = new ClassName[mClassNamesLength];
        for (int i = 0; i < mClassNames.Length; i++)
        {
            mClassNames[i] = new();
            mClassNames[i].Parse(reader);
        }

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(MSV6), name);
        ParseBasicTypes(myElement);

        List<ClassName> classNames = [];
        foreach (XmlNode node in myElement.ChildNodes)
        {
            if (node is XmlElement itemElement && itemElement.Name == nameof(ClassName))
            {
                string itemName = itemElement.GetAttribute(nameof(name));
                ClassName item = new();
                item.Parse(myElement, itemName);
                classNames.Add(item);
            }
        }
        mClassNames = [.. classNames];

        if (mClassNamesLength != (uint)mClassNames.Length)
        {
            throw new InvalidDataException("The meta header class count doesn't match its XML elements!");
        }
    }

    #endregion

    #region Header

    public void WriteToBinary(BinaryWriter writer) => Write(writer);

    public void ReadFromBinary(BinaryReader reader) => Parse(reader);

    public uint GetHeaderByteSize() => Size;

    public void SetMetaSectionChunkSizes(uint defaultSectionChunkSize, uint debugSectionChunkSize, uint asyncSectionChunkSize)
    {
        mDefaultSectionChunkSize = defaultSectionChunkSize;
        mDebugSectionChunkSize = debugSectionChunkSize;
        mAsyncSectionChunkSize = asyncSectionChunkSize;
    }

    public string GetDebugInfo()
    {
        string metaInfo = "||||||||||| Meta Header |||||||||||" + Environment.NewLine;

        metaInfo += string.Format("Meta Stream Keyword = {0}", mMetaStreamVersion) + Environment.NewLine;
        metaInfo += string.Format("Meta Default Section Chunk Size = {0}", mDefaultSectionChunkSize) + Environment.NewLine;
        metaInfo += string.Format("Meta Debug Section Chunk Size = {0}", mDebugSectionChunkSize) + Environment.NewLine;
        metaInfo += string.Format("Meta Async Section Chunk Size = {0}", mAsyncSectionChunkSize) + Environment.NewLine;
        metaInfo += string.Format("Meta mClassNamesLength = {0}", mClassNamesLength) + Environment.NewLine;

        for (int i = 0; i < mClassNames.Length; i++)
        {
            metaInfo += string.Format("Meta mClassName {0} = {1}", i, mClassNames[i]) + Environment.NewLine;
        }

        return metaInfo;
    }

    public void PrintConsole()
    {
        Console.WriteLine(GetDebugInfo());
    }

    #endregion

    #region XML

    private void ParseBasicTypes(XmlElement element)
    {
        mMetaStreamVersion = element.GetAttribute(nameof(mMetaStreamVersion));
        if (mMetaStreamVersion != "6VSM")
        {
            throw new InvalidDataException($"Invalid MSV6 meta stream version: {mMetaStreamVersion}");
        }

        mDefaultSectionChunkSize = uint.Parse(element.GetAttribute(nameof(mDefaultSectionChunkSize)));
        mDebugSectionChunkSize = uint.Parse(element.GetAttribute(nameof(mDebugSectionChunkSize)));
        mAsyncSectionChunkSize = uint.Parse(element.GetAttribute(nameof(mAsyncSectionChunkSize)));
        mClassNamesLength = uint.Parse(element.GetAttribute(nameof(mClassNamesLength)));
    }

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mMetaStreamVersion = element.OwnerDocument.CreateAttribute(nameof(this.mMetaStreamVersion));
        XmlAttribute mDefaultSectionChunkSize = element.OwnerDocument.CreateAttribute(nameof(this.mDefaultSectionChunkSize));
        XmlAttribute mDebugSectionChunkSize = element.OwnerDocument.CreateAttribute(nameof(this.mDebugSectionChunkSize));
        XmlAttribute mAsyncSectionChunkSize = element.OwnerDocument.CreateAttribute(nameof(this.mAsyncSectionChunkSize));
        XmlAttribute mClassNamesLength = element.OwnerDocument.CreateAttribute(nameof(this.mClassNamesLength));

        mMetaStreamVersion.Value = this.mMetaStreamVersion.ToString();
        mDefaultSectionChunkSize.Value = this.mDefaultSectionChunkSize.ToString();
        mDebugSectionChunkSize.Value = this.mDebugSectionChunkSize.ToString();
        mAsyncSectionChunkSize.Value = this.mAsyncSectionChunkSize.ToString();
        mClassNamesLength.Value = this.mClassNamesLength.ToString();

        _ = element.Attributes.Append(mMetaStreamVersion);
        _ = element.Attributes.Append(mDefaultSectionChunkSize);
        _ = element.Attributes.Append(mDebugSectionChunkSize);
        _ = element.Attributes.Append(mAsyncSectionChunkSize);
        _ = element.Attributes.Append(mClassNamesLength);
    }

    #endregion
}
