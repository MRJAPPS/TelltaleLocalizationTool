using Newtonsoft.Json;
using System.ComponentModel;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.FileTypes.Font.Glyph;
using TelltaleLocalizationTool.Telltale.FileTypes.Font.Texture;
using TelltaleLocalizationTool.Telltale.Meta;
using TelltaleLocalizationTool.Telltale.Meta.Versions;
using TelltaleLocalizationTool.Telltale.TTypes;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Font;

[Description("Reading a font!(Telltale font file)")]
public class TFont : ReportBase, IFnt
{
    #region Private constants
    private const uint BasicTypes = sizeof(float) * 4 + sizeof(bool) * 2;
    #endregion

    #region private fields

    private byte[] msv6Header = [];//Use this for xml export/import

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

    #endregion

    #region Public props

    public TString mName { get; set; } = [];

    public bool mbUnicode { get; set; } //TODO: We need a new type for TelltaleBool! TBool!!! Remove ByteFunctions.ReadTBool... you know what u should do..

    public float mHeight;

    public float mBase;

    public float mWiiScale;

    public float mFontScale;

    public GlyphInfoCollection mGlyphInfos { get; set; } = [];

    public FontD3dtxV9Collection mTexturePages { get; set; } = [];

    public bool mIsDistanceField { get; set; }

    #endregion

    #region Implement

    public IMetaHeader? Header { get; private set; }

    public uint Size => defaultSectionChunkSize + asyncSectionChunkSize;

    public long Position { get; private set; }

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
        ParseHeader(myElement);
        ParseBasicTypes(myElement);
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
        AppendHeader(myElement);
        AppendBasicTypes(myElement);
        mName.Write(myElement, nameof(mName));
        mGlyphInfos.Write(myElement, nameof(mGlyphInfos));
        mTexturePages.Write(myElement, nameof(mTexturePages));

        _ = element.AppendChild(myElement);
    }

    #endregion

    #region Header

    public void ParseMetaHeader(BinaryReader reader)
    {//TODO: This method can be refactored too :| what the fuck are u doing?
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
        if (Header == null)
        {
            throw new NullReferenceException($"{nameof(Header)} is not properly initialized!");
        }

        uint debugSectionChunkSize = ((MSV6)Header).mDebugSectionChunkSize;
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

        mbUnicode.Value = this.mbUnicode.ToString();
        mHeight.Value = this.mHeight.ToString();
        mBase.Value = this.mBase.ToString();
        mWiiScale.Value = this.mWiiScale.ToString();
        mFontScale.Value = this.mFontScale.ToString();
        mIsDistanceField.Value = this.mIsDistanceField.ToString();

        _ = element.Attributes.Append(mbUnicode);
        _ = element.Attributes.Append(mHeight);
        _ = element.Attributes.Append(mBase);
        _ = element.Attributes.Append(mWiiScale);
        _ = element.Attributes.Append(mFontScale);
        _ = element.Attributes.Append(mIsDistanceField);
    }

    private void ParseBasicTypes(XmlElement element)
    {
        mbUnicode = bool.Parse(element.GetAttribute(nameof(mbUnicode)));
        mHeight = float.Parse(element.GetAttribute(nameof(mHeight)));
        mBase = float.Parse(element.GetAttribute(nameof(mBase)));
        mWiiScale = float.Parse(element.GetAttribute(nameof(mWiiScale)));
        mFontScale = float.Parse(element.GetAttribute(nameof(mFontScale)));
        mIsDistanceField = bool.Parse(element.GetAttribute(nameof(mIsDistanceField)));
    }

    private void ParseHeader(XmlElement element)
    {
        XmlElement msv6H = element.GetSingleElement(nameof(msv6Header));
        msv6Header = Convert.FromBase64String(msv6H.InnerText);
        using MemoryStream headerTempStream = new(msv6Header);
        using BinaryReader reader = new(headerTempStream);
        Header ??= MetaHeaderFactory.CreateMetaHeader(Meta.Enums.MetaVersion.MSV6);
        Header.ReadFromBinary(reader);
    }

    private void AppendHeader(XmlElement element)
    {
        if (Header == null)
        {
            throw new NullReferenceException($"{nameof(Header)} is not properly initialized! If you were looking to save a texture from a TFont as a D3dtx file, that feature hasn't been implemented yet and will likely be added in the future.");// TODO: Implement saving TFont texture as D3dtx.
        }
        string headerB64 = Convert.ToBase64String(msv6Header);
        XmlElement b64 = element.OwnerDocument.CreateElement(nameof(msv6Header));
        b64.InnerText = headerB64;
        _ = element.AppendChild(b64);
    }

    #endregion

    #region Error Checking
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
    #endregion
}