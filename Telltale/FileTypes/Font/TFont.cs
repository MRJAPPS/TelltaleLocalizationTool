using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.FileTypes.Font.Glyph;
using TelltaleLocalizationTool.Telltale.FileTypes.Font.Texture;
using TelltaleLocalizationTool.Telltale.Meta;
using TelltaleLocalizationTool.Telltale.Meta.TClassInfo;
using TelltaleLocalizationTool.Telltale.Meta.Versions;
using TelltaleLocalizationTool.Telltale.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Font;

[Description("Reading a font!(Telltale font file)")]
public class TFont(bool rMsv6Header = true, bool wMsv6Header = true) : ReportBase
#if IFNT
    , IFnt
#else
    , ITelltaleFile
#endif
{
    #region Private constants
    private const uint BasicTypes = (sizeof(float) * 4) + (sizeof(bool) * 2);
    #endregion

    #region private fields

    private uint classSizes => mName.Size + mGlyphInfos.Size;

    private uint defaultSectionChunkSize => classSizes + BasicTypes + mTexturePages.HeaderSizeSum;

    private uint asyncSectionChunkSize
    {
        get
        {
            uint size = 0;
            foreach (D3DTXV9 d3dtxv9 in mTexturePages)
            {
                size += (uint)d3dtxv9.mPixelData.Sum(item => item.Length);
            }
            return size;
        }
    }

    static readonly ClassName[] mDefaultClassNamesWD3 =
[
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = 0x9A3A4A9E63375381 },
        mVersionCRC = 0xF28FDEE8
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = 0xAB4BFA2304C2292C },
        mVersionCRC = 0x3FE91201
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = 0x53A0B3AA86983895 },
        mVersionCRC = 0x376CAB81
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = 0x3E249E7E6F38CCE2 },
        mVersionCRC = 0xA730AB61
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = 0x937F5D487A0988E3 },
        mVersionCRC = 0xB2E3CEB0
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = 0x8E88FBB78405598C },
        mVersionCRC = 0xA4AC7DAF
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = 0xFA6C8889876EBA7A },
        mVersionCRC = 0x5B484905
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = 0x7BBCA244E61F1A07 },
        mVersionCRC = 0xE19FCC02
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = 0xEFA1BA20E620F40F },
        mVersionCRC = 0x9AF4FC40
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = 0x584619F1AE300EEA },
        mVersionCRC = 0x5057EA61
    }
];

    #endregion

    #region Public props

    public bool ReadMsv6Header { get => rMsv6Header; set => rMsv6Header = value; }

    public bool WriteMsv6Header { get => wMsv6Header; set => wMsv6Header = value; }

    public TString mName { get; set; } = [];

    public bool mbUnicode { get; set; } //TODO: We need a new type for TelltaleBool! TBool!!! Remove ByteFunctions.ReadTBool... you know what u should do..

    public float mHeight { get; set; }

    public float mBase { get; set; }

    public float mWiiScale { get; set; }

    public float mFontScale { get; set; }

    public GlyphInfoCollection mGlyphInfos { get; set; } = [];

    public FontD3dtxV9Collection mTexturePages { get; set; } = [];

    public bool mIsDistanceField { get; set; }

    #endregion

    #region Implement

    public IMetaHeader? Header { get; private set; }

    public uint Size => defaultSectionChunkSize + asyncSectionChunkSize;

    public long Position { get; private set; } = -1;
#if IFNT
    public void Export(string fntPath)//Just for loadedFont
    {
        CheckImportExportArgs(fntPath, false);
        mTexturePages.ExportTextures(mName, Path.GetDirectoryName(fntPath)!, bmpFontMode: true);
        //Next step: Generate fnt xml file from glyphs and metadata
    }

    public void Import(string fntPath)
    {
        CheckImportExportArgs(fntPath, true);
        mName = Path.GetFileNameWithoutExtension(fntPath);
        mTexturePages.ImportTextures(mName, Path.GetDirectoryName(fntPath)!, bmfontMode: true);
        //Next step: Map fnt file to font
    }
#endif

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        ParseMetaHeader(reader);

        mName.Parse(reader);
        mbUnicode = ByteFunctions.ReadTelltaleBoolean(reader);
        mHeight = reader.ReadSingle();
        mBase = reader.ReadSingle();
        mWiiScale = reader.ReadSingle();
        mFontScale = reader.ReadSingle();
        mGlyphInfos.Parse(reader);
        mTexturePages.ParseMetadata(reader);
        mIsDistanceField = ByteFunctions.ReadTelltaleBoolean(reader);//TODO: TBOOL
        mTexturePages.ParsePixelData(reader);

        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(TFont), name);
        ParseBasicTypes(myElement);
        Header = MetaHeaderXml.ParseHeader(myElement, rMsv6Header);
        mName.Parse(myElement, nameof(mName));
        mGlyphInfos.Parse(myElement, nameof(mGlyphInfos));
        mTexturePages.Parse(myElement, nameof(mTexturePages));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        WriteMetaHeader(writer);
        mName.Write(writer);
        ByteFunctions.WriteBoolean(writer, mbUnicode);//TODO: USE TBOOL!!!
        writer.Write(mHeight);
        writer.Write(mBase);
        writer.Write(mWiiScale);
        writer.Write(mFontScale);
        mGlyphInfos.Write(writer);
        mTexturePages.WriteMetaData(writer);
        ByteFunctions.WriteBoolean(writer, mIsDistanceField);
        mTexturePages.WritePixelData(writer);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(TFont));
        myElement.SetAttribute(nameof(name), name);
        MetaHeaderXml.AppendHeader(myElement, Header, rMsv6Header);
        AppendBasicTypes(myElement);
        mName.Write(myElement, nameof(mName));
        mGlyphInfos.Write(myElement, nameof(mGlyphInfos));
        mTexturePages.Write(myElement, nameof(mTexturePages));

        _ = element.AppendChild(myElement);
    }

    #endregion

    #region Header

    public void ParseMetaHeader(BinaryReader reader)
    {
        if (!rMsv6Header)
        {
            return;
        }

        Header = MetaHeaderFactory.ReadMetaHeader(reader);
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
            MSV6 defaultHeaderForWalkingDead3NewF = new();
            defaultHeaderForWalkingDead3NewF.mClassNamesLength = (uint)mDefaultClassNamesWD3.Length;
            defaultHeaderForWalkingDead3NewF.mClassNames = mDefaultClassNamesWD3;
            Header = defaultHeaderForWalkingDead3NewF;
        }

        uint debugSectionChunkSize = Header.mDebugSectionChunkSize;
        Header.SetMetaSectionChunkSizes(defaultSectionChunkSize, debugSectionChunkSize, asyncSectionChunkSize);
        Header.WriteToBinary(writer);
    }

    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mbUnicode = element.OwnerDocument.CreateAttribute(nameof(this.mbUnicode));
        XmlAttribute mHeight = element.OwnerDocument.CreateAttribute(nameof(this.mHeight));
        XmlAttribute mBase = element.OwnerDocument.CreateAttribute(nameof(this.mBase));
        XmlAttribute mWiiScale = element.OwnerDocument.CreateAttribute(nameof(this.mWiiScale));
        XmlAttribute mFontScale = element.OwnerDocument.CreateAttribute(nameof(this.mFontScale));
        XmlAttribute mIsDistanceField = element.OwnerDocument.CreateAttribute(nameof(this.mIsDistanceField));
        XmlAttribute rMsv6HeaderAtt, wMsv6HeaderAtt;

        rMsv6HeaderAtt = element.OwnerDocument.CreateAttribute(nameof(rMsv6Header));
        wMsv6HeaderAtt = element.OwnerDocument.CreateAttribute(nameof(wMsv6Header));

        mbUnicode.Value = this.mbUnicode.ToString();
        mHeight.Value = this.mHeight.ToString();
        mBase.Value = this.mBase.ToString();
        mWiiScale.Value = this.mWiiScale.ToString();
        mFontScale.Value = this.mFontScale.ToString();
        mIsDistanceField.Value = this.mIsDistanceField.ToString();
        rMsv6HeaderAtt.Value = rMsv6Header.ToString();
        wMsv6HeaderAtt.Value = wMsv6Header.ToString();

        _ = element.Attributes.Append(mbUnicode);
        _ = element.Attributes.Append(mHeight);
        _ = element.Attributes.Append(mBase);
        _ = element.Attributes.Append(mWiiScale);
        _ = element.Attributes.Append(mFontScale);
        _ = element.Attributes.Append(mIsDistanceField);
        _ = element.Attributes.Append(rMsv6HeaderAtt);
        _ = element.Attributes.Append(wMsv6HeaderAtt);
    }

    private void ParseBasicTypes(XmlElement element)
    {
        mbUnicode = bool.Parse(element.GetAttribute(nameof(mbUnicode)));
        mHeight = float.Parse(element.GetAttribute(nameof(mHeight)));
        mBase = float.Parse(element.GetAttribute(nameof(mBase)));
        mWiiScale = float.Parse(element.GetAttribute(nameof(mWiiScale)));
        mFontScale = float.Parse(element.GetAttribute(nameof(mFontScale)));
        mIsDistanceField = bool.Parse(element.GetAttribute(nameof(mIsDistanceField)));
        wMsv6Header = bool.Parse(element.GetAttribute(nameof(wMsv6Header)));
        rMsv6Header = bool.Parse(element.GetAttribute(nameof(rMsv6Header)));
    }

    #endregion

    #region Error Checking'
#if IFNT
    public void CheckImportExportArgs(string fntPath, bool import)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(fntPath, nameof(fntPath));
        string directory = Path.GetDirectoryName(fntPath) ?? throw new DirectoryNotFoundException($"I couldn't find the directory associated with fntPath; the path you gave must be an absolute .fnt file path, not relative!{Environment.NewLine}\"{fntPath}\"");
        if (!import && !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"I couldn't find the directory associated with fntPath!{Environment.NewLine}\"{directory}\"");
        }
        if (import && !File.Exists(fntPath))
        {
            throw new FileNotFoundException($"Couldn't find the .fnt file to import — you sure it exists?{Environment.NewLine}\"{fntPath}\"");
        }
    }
#endif
    #endregion
}
