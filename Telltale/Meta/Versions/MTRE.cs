using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using TelltaleLocalizationTool.Telltale.Meta.TClassInfo;
using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.Meta.Versions;

[Description("Reading a MTRE meta header")]
public class MTRE : ReportBase, ITelltaleObject, IMetaHeader
{
    #region Public props

    public string mMetaStreamVersion { get; set; } = MetaMagics.MTRE;

    public uint mClassNamesLength { get; set; }

    public ClassName[] mClassNames { get; set; } = [];

    public UnhashedClassName[] mUnhashedClassNames { get; set; } = [];

    #endregion

    #region Implement

    public MetaStreamContext? msCtx { get; protected set; }

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    public uint Size => checked(2 * sizeof(uint) + (uint)mClassNames.Sum(item => item.Size) +
        (uint)mUnhashedClassNames.Sum(item => item.Size));

    public long Position { get; private set; } = -1;

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        if (mClassNames.Length > 0 && mUnhashedClassNames.Length > 0)
        {
            throw new InvalidDataException("A meta header can't contain both hashed and unhashed class names!");
        }

        mClassNamesLength = checked((uint)mClassNames.Length + (uint)mUnhashedClassNames.Length);
        ByteFunctions.WriteFixedString(writer, mMetaStreamVersion);
        writer.Write(mClassNamesLength);

        foreach (ClassName item in mClassNames)
        {
            item.Write(writer);
        }

        foreach (UnhashedClassName item in mUnhashedClassNames)
        {
            item.Write(writer);
        }

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        if (mClassNames.Length > 0 && mUnhashedClassNames.Length > 0)
        {
            throw new InvalidDataException("A meta header can't contain both hashed and unhashed class names!");
        }

        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(MTRE));
        myElement.SetAttribute(nameof(name), name);

        mClassNamesLength = checked((uint)mClassNames.Length + (uint)mUnhashedClassNames.Length);
        AppendBasicTypes(myElement);

        for (int i = 0; i < mClassNames.Length; i++)
        {
            mClassNames[i].Write(myElement, $"{nameof(mClassNames)}_{i}");
        }

        for (int i = 0; i < mUnhashedClassNames.Length; i++)
        {
            mUnhashedClassNames[i].Write(myElement, $"{nameof(mUnhashedClassNames)}_{i}");
        }

        _ = element.AppendChild(myElement);
    }

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mMetaStreamVersion = ByteFunctions.ReadFixedString(reader, 4);
        mClassNamesLength = reader.ReadUInt32();

        mClassNames = [];
        mUnhashedClassNames = [];
        if (mClassNamesLength > 0)
        {
            uint checkValue = reader.ReadUInt32();
            reader.BaseStream.Position -= sizeof(uint);

            if (checkValue > 128)
            {
                mClassNames = new ClassName[mClassNamesLength];
                for (int i = 0; i < mClassNames.Length; i++)
                {
                    mClassNames[i] = new();
                    mClassNames[i].Parse(reader);
                }
            }
            else
            {
                mUnhashedClassNames = new UnhashedClassName[mClassNamesLength];
                for (int i = 0; i < mUnhashedClassNames.Length; i++)
                {
                    mUnhashedClassNames[i] = new();
                    mUnhashedClassNames[i].Parse(reader);
                }
            }
        }

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(MTRE), name);
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

        List<UnhashedClassName> unhashedClassNames = [];
        foreach (XmlNode node in myElement.ChildNodes)
        {
            if (node is XmlElement itemElement && itemElement.Name == nameof(UnhashedClassName))
            {
                string itemName = itemElement.GetAttribute(nameof(name));
                UnhashedClassName item = new();
                item.Parse(myElement, itemName);
                unhashedClassNames.Add(item);
            }
        }
        mUnhashedClassNames = [.. unhashedClassNames];

        if (mClassNames.Length > 0 && mUnhashedClassNames.Length > 0)
        {
            throw new InvalidDataException("A meta header can't contain both hashed and unhashed class names!");
        }

        if (mClassNamesLength != checked((uint)mClassNames.Length + (uint)mUnhashedClassNames.Length))
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
    }

    public string GetDebugInfo()
    {
        string metaInfo = "||||||||||| Meta Header |||||||||||" + Environment.NewLine;

        metaInfo += "Meta Stream Keyword = " + mMetaStreamVersion + Environment.NewLine;
        metaInfo += "Meta mClassNamesLength = " + mClassNamesLength + Environment.NewLine;

        for (int i = 0; i < mClassNames.Length; i++)
        {
            metaInfo += "Meta mClassName " + i + " = " + mClassNames[i] + Environment.NewLine;
        }

        for (int i = 0; i < mUnhashedClassNames.Length; i++)
        {
            metaInfo += "Meta mClassName " + i + " = " + mUnhashedClassNames[i] + Environment.NewLine;
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
        if (mMetaStreamVersion != "ERTM")
        {
            throw new InvalidDataException($"Invalid MTRE meta stream version: {mMetaStreamVersion}");
        }

        mClassNamesLength = uint.Parse(element.GetAttribute(nameof(mClassNamesLength)));
    }

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mMetaStreamVersion = element.OwnerDocument.CreateAttribute(nameof(this.mMetaStreamVersion));
        XmlAttribute mClassNamesLength = element.OwnerDocument.CreateAttribute(nameof(this.mClassNamesLength));

        mMetaStreamVersion.Value = this.mMetaStreamVersion.ToString();
        mClassNamesLength.Value = this.mClassNamesLength.ToString();

        _ = element.Attributes.Append(mMetaStreamVersion);
        _ = element.Attributes.Append(mClassNamesLength);
    }

    #endregion
}
