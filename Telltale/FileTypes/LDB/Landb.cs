using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.DData;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LangDB;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LnagRes;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.ProjDataBIDPairs;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Reg;
using TelltaleLocalizationTool.Telltale.Meta;
using TelltaleLocalizationTool.Telltale.Meta.Structs;
using TelltaleLocalizationTool.Telltale.Meta.Versions;
using TelltaleTextureTool.TelltaleTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB;

[Description("Process the whole landb file")]
public class Landb(bool rMsv6Header = true, bool wMsv6Header = true) : ReportBase, ITelltaleFile
{
    private const int OffsetDefDataLen = 0x4;
    private const int OffsetDefDebugDataLen = OffsetDefDataLen + 0x4;
    private const int OffsetAsyncDataLen = OffsetDefDebugDataLen + 0x4;

    #region private fields
    /// <summary>
    /// Honestly, I didn't define className as TelltaleObject because I couldn't be bothered — marking it as a //TODO for later.
    /// </summary>
    // Array built from the DCArray type symbols and the updated version CRCs from your log.
    static readonly ClassNames[] mDefaultClassNamesWD3 =
    [
        new() { // LanguageDB
        mTypeNameCRC = new Symbol { mCrc64 = 10167596401243054749 },
        mVersionCRC = 3484166325
    },
    new() { // UID::Owner
        mTypeNameCRC = new Symbol { mCrc64 = 5213064354065280415 },
        mVersionCRC = 4008931442
    },
    new() { // UID::Generator
        mTypeNameCRC = new Symbol { mCrc64 = 5462952281277296288 },
        mVersionCRC = 1301338751
    },
    new() { // LanguageRes
        mTypeNameCRC = new Symbol { mCrc64 = 4314860780925021146 },
        mVersionCRC = 3740207120
    },
    new() { // Symbol
        mTypeNameCRC = new Symbol { mCrc64 = 22238947196837808 },
        mVersionCRC = 3040456959
    },
    new() { // LanguageResLocal
        mTypeNameCRC = new Symbol { mCrc64 = 6873471627863899232 },
        mVersionCRC = 4168609454
    },
    new() { // LocalizeInfo
        mTypeNameCRC = new Symbol { mCrc64 = 18223585787162570679 },
        mVersionCRC = 1148494836
    },
    new() { // Flags
        mTypeNameCRC = new Symbol { mCrc64 = 9522928179385669185 },
        mVersionCRC = 86496959
    },
    new() { // RecordingUtils::EnumRecordingStatus
        mTypeNameCRC = new Symbol { mCrc64 = 9141417580607954003 },
        mVersionCRC = 2762767791
    },
    new() { // LocalizationRegistry
        mTypeNameCRC = new Symbol { mCrc64 = 12621666885870520219 },
        mVersionCRC = 4129176698
    }
    ];
    private byte[] msv6Header = [];//Use this for xml export/import

    private uint defaultSectionChunkSize => mLanguageDB.Size +
        mLanguageResources.Size +
        mRegistry.Size +
        (sizeof(uint) * 2) +
        mExpandedIDRanges.Size;

    private uint debugSectionChunkSize => mDebugDatas.Size;

    #endregion

    #region Public Properties

    public LanguageDB mLanguageDB { get; set; } = new();

    public LanguageResources mLanguageResources { get; set; } = new();

    public Registry mRegistry { get; set; } = new();

    public uint mFlags { get; set; }

    public uint mProjectID { get; set; }

    public ProjectDatabaseIDPairs mExpandedIDRanges { get; set; } = [];

    public DebugDataList mDebugDatas { get; set; } = [];

    

    public uint Size =>defaultSectionChunkSize + debugSectionChunkSize;

    public long Position { get; private set; }

    public IMetaHeader? Header { get; private set; }

    #endregion

    #region Implement

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        ParseMetaHeader(reader);

        mLanguageDB.Parse(reader);
        mLanguageResources.Parse(reader);
        mRegistry.Parse(reader);

        mFlags = reader.ReadUInt32();
        mProjectID = reader.ReadUInt32();

        mExpandedIDRanges.Parse(reader);
        mDebugDatas.Parse(reader);

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(Landb), name);

        if (myElement.SelectSingleNode(nameof(msv6Header)) is XmlElement headerElement)
        {
            msv6Header = Convert.FromBase64String(headerElement.InnerText);
            using MemoryStream headerStream = new(msv6Header);
            using BinaryReader headerReader = new(headerStream);
            Header = MetaHeaderFactory.CreateMetaHeader(Meta.Enums.MetaVersion.MSV6);
            Header.ReadFromBinary(headerReader);
        }

        mLanguageDB.Parse(myElement, nameof(mLanguageDB));
        mLanguageResources.Parse(myElement, nameof(mLanguageResources));
        mRegistry.Parse(myElement, nameof(mRegistry));

        ParseBasicTypes(myElement);

        mExpandedIDRanges.Parse(myElement, nameof(mExpandedIDRanges));
        mDebugDatas.Parse(myElement, nameof(mDebugDatas));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        WriteMetaHeader(writer);

        mLanguageDB.Write(writer);
        mLanguageResources.Write(writer);
        mRegistry.Write(writer);

        writer.Write(mFlags);
        writer.Write(mProjectID);

        mExpandedIDRanges.Write(writer);
        mDebugDatas.Write(writer);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(Landb));
        myElement.SetAttribute(nameof(name), name);
        if (msv6Header.Length > 0)
        {
            XmlElement headerElement = element.OwnerDocument.CreateElement(nameof(msv6Header));
            headerElement.InnerText = Convert.ToBase64String(msv6Header);
            _ = myElement.AppendChild(headerElement);
        }
        mLanguageDB.Write(myElement, nameof(mLanguageDB));
        mLanguageResources.Write(myElement, nameof(mLanguageResources));
        mRegistry.Write(myElement, nameof(mRegistry));
        AppendBasicTypes(myElement);
        mExpandedIDRanges.Write(myElement, nameof(mExpandedIDRanges));
        mDebugDatas.Write(myElement, nameof(mDebugDatas));

        _ = element.AppendChild(myElement);
    }

    #endregion

    #region XML

    private void ParseBasicTypes(XmlElement element)
    {
        mFlags = uint.Parse(element.GetAttribute(nameof(mFlags)));
        mProjectID = uint.Parse(element.GetAttribute(nameof(mProjectID)));
    }


    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mFlags, mProjectID;
        mFlags = element.OwnerDocument.CreateAttribute(nameof(this.mFlags));
        mProjectID = element.OwnerDocument.CreateAttribute(nameof(this.mProjectID));

        mFlags.Value = this.mFlags.ToString();
        mProjectID.Value = this.mProjectID.ToString();

        _ = element.Attributes.Append(mFlags);
        _ = element.Attributes.Append(mProjectID);
    }

    public void ParseMetaHeader(BinaryReader reader)
    {//TODO: This method can be refactored too :| what the fuck are u doing?
        if (!rMsv6Header)
        {
            return;
        }

        long Position = reader.BaseStream.Position;
        uint magic = reader.ReadUInt32();
        if (magic != Config.Msv6Magic)
        {
            throw new NotImplementedException("Right now I can only handle MSV6!! I can't deal with the Meta Stream version of the input file!");
        }

        reader.BaseStream.Position = Position;
        Header = MetaHeaderFactory.CreateMetaHeader(Meta.Enums.MetaVersion.MSV6);
        Header.ReadFromBinary(reader);
        //===========================
        reader.BaseStream.Position -= Header.GetHeaderByteSize();
        msv6Header = new byte[Header.GetHeaderByteSize()];
        _ = reader.Read(msv6Header);
    }

    public void WriteMetaHeader(BinaryWriter writer)
    {
        if (!wMsv6Header)
        {
            return;
        }

        if (Header == null && rMsv6Header)
        {
            throw new NullReferenceException($"{nameof(Header)} is not properly initialized!");
        }
        else if (Header == null)
        {
            //Recreate header
            MSV6 defaultHeaderForWalkingDead3NewF = (MSV6)MetaHeaderFactory.CreateMetaHeader(Meta.Enums.MetaVersion.MSV6);
            defaultHeaderForWalkingDead3NewF.mClassNamesLength = (uint)mDefaultClassNamesWD3.Length;
            defaultHeaderForWalkingDead3NewF.mClassNames = mDefaultClassNamesWD3;
            Header = defaultHeaderForWalkingDead3NewF;
        }

        uint asyncSectionChunkSize = ((MSV6)Header).mAsyncSectionChunkSize;
        Header.SetMetaSectionChunkSizes(defaultSectionChunkSize, debugSectionChunkSize, asyncSectionChunkSize);
        Header.WriteToBinary(writer);
    }

    #endregion
}
