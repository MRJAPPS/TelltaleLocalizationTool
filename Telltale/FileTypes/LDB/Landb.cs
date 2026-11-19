using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Localize;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.FileTypes.Ldb.LandbTypes.Enum;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.DData;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LangDB;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LnagRes;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.ProjDataBIDPairs;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.Reg;
using TelltaleLocalizationTool.Telltale.Meta;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using TelltaleLocalizationTool.Telltale.Meta.TClassInfo;
using TelltaleLocalizationTool.Telltale.Meta.Versions;
using TelltaleLocalizationTool.Telltale.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB;

[Description("Process the whole landb file")]
public class Landb(Games game, TextTransformMode textTransform, bool rMsv6Header = true, bool wMsv6Header = true) : ReportBase, ITelltaleFile
{
    private const int OffsetDefDataLen = 0x4;
    private const int OffsetDefDebugDataLen = OffsetDefDataLen + 0x4;
    private const int OffsetAsyncDataLen = OffsetDefDebugDataLen + 0x4;//I want this line of code to stay even though it's not used — any problem with that???

    #region private fields
    // Array built from the DCArray type symbols and the updated version CRCs from your log.
    static readonly ClassName[] mDefaultClassNamesWD3 =
    [
        new() { // LanguageDB
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.LanguageDB },
        mVersionCRC = 3484166325
    },
    new() { // UID::Owner
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.UID_Owner },
        mVersionCRC = 4008931442
    },
    new() { // UID::Generator
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.UID_Generator },
        mVersionCRC = 1301338751
    },
    new() { // LanguageRes
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.LanguageRes },
        mVersionCRC = 3740207120
    },
    new() { // Symbol
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.Symbol },
        mVersionCRC = 3040456959
    },
    new() { // LanguageResLocal
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.LanguageResLocal },
        mVersionCRC = 4168609454
    },
    new() { // LocalizeInfo
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.LocalizeInfo },
        mVersionCRC = 1148494836
    },
    new() { // Flags
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.Flags },
        mVersionCRC = 86496959
    },
    new() { // RecordingUtils::EnumRecordingStatus
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.RecordingUtils_EnumRecordingStatus },
        mVersionCRC = 2762767791
    },
    new() { // LocalizationRegistry
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.LocalizationRegistry },
        mVersionCRC = 4129176698
    }
    ];

    private uint defaultSectionChunkSize => mLanguageDB.Size +
        mLanguageResources.Size +
        mRegistry.Size +
        mFlags.Size + sizeof(uint) +
        mExpandedIDRanges.Size;

    private uint debugSectionChunkSize => mDebugDatas.Size;

    #endregion

    #region Public Properties

    public LanguageDB mLanguageDB { get; set; } = new();

    public LanguageResources mLanguageResources { get; set; } = new();

    public Registry mRegistry { get; set; } = new();

    public Flags<uint> mFlags { get; set; } = new();

    public uint mProjectID { get; set; }

    public ProjectDatabaseIDPairs mExpandedIDRanges { get; set; } = [];

    public DebugDataList mDebugDatas { get; set; } = [];



    public uint Size => defaultSectionChunkSize + debugSectionChunkSize;

    public long Position { get; private set; } = -1;

    public IMetaHeader? Header { get; private set; }

    #endregion

    #region Implement

    public MetaStreamContext? msCtx { get; protected set; } = new(game);

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        ParseMetaHeader(reader);

        mLanguageDB.Parse(reader);
        mLanguageResources.Parse(reader);
        mRegistry.Parse(reader);

        mFlags.Parse(reader);
        mProjectID = reader.ReadUInt32();

        mExpandedIDRanges.Parse(reader);
        mDebugDatas.Parse(reader);

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = ApplyTextTransform(reader.GetSingleElement(nameof(Landb), name), textTransform);

        Header = MetaHeaderXml.ParseHeader(myElement, rMsv6Header);
        Utils.CheckHeader(game, Header);

        msCtx ??= new(game);
        if (Header != null)
        {
            msCtx.Header = Header;
        }
        this.ShareMetaStreamContext();

        mLanguageDB.Parse(myElement, nameof(mLanguageDB));
        mLanguageResources.Parse(myElement, nameof(mLanguageResources));
        mRegistry.Parse(myElement, nameof(mRegistry));
        mFlags.Parse(myElement, nameof(mFlags));

        ParseBasicTypes(myElement);

        mExpandedIDRanges.Parse(myElement, nameof(mExpandedIDRanges));
        mDebugDatas.Parse(myElement, nameof(mDebugDatas));
    }

    public void Write(BinaryWriter writer)
    {
        this.ShareMetaStreamContext();
        Position = writer.BaseStream.Position;

        WriteMetaHeader(writer);

        mLanguageDB.Write(writer);
        mLanguageResources.Write(writer);
        mRegistry.Write(writer);

        mFlags.Write(writer);
        writer.Write(mProjectID);

        mExpandedIDRanges.Write(writer);
        mDebugDatas.Write(writer);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(Landb));
        myElement.SetAttribute(nameof(name), name);
        XmlElement textTransform = element.OwnerDocument.CreateElement("textTransform");
        textTransform.SetAttribute("mode", textTransform.ToString());
        _ = myElement.AppendChild(textTransform);

        Utils.CheckHeader(game, Header);
        MetaHeaderXml.AppendHeader(myElement, Header, rMsv6Header);

        mLanguageDB.Write(myElement, nameof(mLanguageDB));
        mLanguageResources.Write(myElement, nameof(mLanguageResources));
        mRegistry.Write(myElement, nameof(mRegistry));
        mFlags.Write(myElement, nameof(mFlags));
        AppendBasicTypes(myElement);
        mExpandedIDRanges.Write(myElement, nameof(mExpandedIDRanges));
        mDebugDatas.Write(myElement, nameof(mDebugDatas));

        _ = element.AppendChild(myElement);
    }

    #endregion

    #region XML

    private static XmlElement ApplyTextTransform(XmlElement element, TextTransformMode textTransform)
    {
        XmlNodeList transforms = element.SelectNodes(nameof(textTransform))!;
        if (transforms.Count > 1)
            throw new FormatException($"Only one {nameof(textTransform)} is allowed!");
        TextTransformMode mode;
        if (textTransform == TextTransformMode.Def)
            mode = transforms.Count == 0 ? TextTransformMode.none : Enum.Parse<TextTransformMode>(((XmlElement)transforms[0]!).GetAttribute("mode"));
        else
            mode = textTransform;
        if (mode == TextTransformMode.none)
            return element;
        if (mode != TextTransformMode.shaped && mode != TextTransformMode.logical)
            throw new FormatException($"Unknown textTransform mode: {mode}");

        XmlElement result = (XmlElement)element.CloneNode(true);
        XmlNodeList texts = result.SelectNodes("LanguageResources/TextCollection/TextItem/ResolvedLocalData/TString[@name='mText'] | LanguageResources/TextCollection/TextItem/LocalDataCollection/LocalDataItem/TString[@name='mText']")!;
        foreach (XmlElement text in texts)
            text.InnerText = mode == TextTransformMode.shaped ? text.InnerText.FixPersian() : text.InnerText.NormalizePersian();

        return result;
    }

    private void ParseBasicTypes(XmlElement element)
    {
        mProjectID = uint.Parse(element.GetAttribute(nameof(mProjectID)));
    }


    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mProjectID = element.OwnerDocument.CreateAttribute(nameof(this.mProjectID));

        mProjectID.Value = this.mProjectID.ToString();

        _ = element.Attributes.Append(mProjectID);
    }

    public void ParseMetaHeader(BinaryReader reader)
    {
        if (!rMsv6Header)
        {
            this.ShareMetaStreamContext();
            return;
        }

        Header = MetaHeaderFactory.ReadMetaHeader(reader);
        msCtx ??= new(game);
        msCtx.Header = Header;
        Utils.CheckHeader(game, Header);
        Utils.CheckSectionsV56($"The sections({nameof(D3DTXV4)} are incomplete bro!", Header, reader.BaseStream.Length, reader.BaseStream.Position);
        this.ShareMetaStreamContext();
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
            MSV6 defaultHeaderForWalkingDead3NewF = new()
            {
                mClassNamesLength = (uint)mDefaultClassNamesWD3.Length,
                mClassNames = mDefaultClassNamesWD3
            };
            Header = defaultHeaderForWalkingDead3NewF;
        }
        Utils.CheckHeader(game, Header);

        uint asyncSectionChunkSize = Header.mAsyncSectionChunkSize;
        Header.SetMetaSectionChunkSizes(defaultSectionChunkSize, debugSectionChunkSize, asyncSectionChunkSize);
        msCtx ??= new(game);
        msCtx.Header = Header;
        this.ShareMetaStreamContext();
        Header.WriteToBinary(writer);
    }

    #endregion
}
