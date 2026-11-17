using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.Graphics;
using TelltaleLocalizationTool.Telltale.Meta;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using TelltaleLocalizationTool.Telltale.Meta.TClassInfo;
using TelltaleLocalizationTool.Telltale.Meta.Versions;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
[Description("Reading a D3DTX version 4")]
public class D3DTXV4(Games game, bool autoPixelData = true, bool rMsv5Header = true, bool wMsv5Header = true) : ReportBase, ID3DTX
{
    #region Private constants

    private const int MyVersion = 4;
    private const string PixelData = nameof(PixelData);

    #endregion

    #region private fields

    private static readonly ClassName[] mDefaultClassNamesWD1 =
[
    new() { mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.T3Texture }, mVersionCRC = 0x7638D53D },
    new() { mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.T3SamplerStateBlock }, mVersionCRC = 0xD61182EA },
    new() { mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.EnumPlatformType }, mVersionCRC = 0x26AB794D },
    new() { mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.Vector2 }, mVersionCRC = 0x2A8CD6AC },
    new() { mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.T3Texture_StreamHeader }, mVersionCRC = 0xFF6B7AF5 },
    new() { mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.T3Texture_RegionStreamHeader }, mVersionCRC = 0xC2572607 }
];

    #endregion

    #region public props

    public bool AutoPixelData { get => autoPixelData; set => autoPixelData = value; }

    public bool ReadMsv5Header { get => rMsv5Header; set => rMsv5Header = value; }

    public bool WriteMsv5Header { get => wMsv5Header; set => wMsv5Header = value; }

    public D3DTXV4Metadata mD3dtxMetaData { get; set; } = new(MyVersion);

    #endregion

    #region Private props

    private uint asyncSectionChunkSize => (uint)mPixelData.Sum(item => item.Length);

    #endregion

    #region Implement

    public MetaStreamContext? msCtx { get; protected set; } = new(game);

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    D3DTXMetadata ID3DTX.mD3dtxMetaData => mD3dtxMetaData;//Cool... I love objective programming...

    public List<byte[]> mPixelData { get; set; } = [];

    public int Version => MyVersion;

    public uint Size => mD3dtxMetaData.Size + asyncSectionChunkSize;

    public long Position { get; private set; } = -1;

    public IMetaHeader? Header { get; private set; }

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        ParseMetaHeader(reader);

        mD3dtxMetaData.Parse(reader);

        if (autoPixelData)
        {
            ParsePixelDatas(reader);
        }

        Report(this, extra: $"(APD: {autoPixelData},PDLength: {asyncSectionChunkSize})");
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(D3DTXV4), name);

        ParseBasicTypes(myElement);
        
        Header = MetaHeaderXml.ParseHeader(myElement, rMsv5Header);
        Utils.CheckHeader(game, Header);

        msCtx ??= new(game);
        if (Header != null)
        {
            msCtx.Header = Header;
        }
        this.ShareMetaStreamContext();

        mD3dtxMetaData.Parse(myElement, nameof(mD3dtxMetaData));

        ParsePixelData(myElement);
    }

    public void ParsePixelDatas(BinaryReader reader)
    {
        mPixelData = [];
        foreach (RegionStreamHeader item in mD3dtxMetaData.mStreamHeader)
        {
            int dataSize = (int)item.mDataSize;
            byte[] imageData = reader.ReadBytes(dataSize);
            if (imageData.Length != dataSize)
            {
                throw new EndOfStreamException("The texture's pixel data is incomplete!");
            }
            mPixelData.Add(imageData);
        }
    }

    public void Write(BinaryWriter writer)
    {
        this.ShareMetaStreamContext();
        Position = writer.BaseStream.Position;

        WriteMetaHeader(writer);

        mD3dtxMetaData.Write(writer);

        if (autoPixelData)
        {
            WritePixelDatas(writer);
        }

        Report(this, true, $"(APD: {autoPixelData})");
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(D3DTXV4));
        myElement.SetAttribute(nameof(name), name);

        mD3dtxMetaData.Write(myElement, nameof(mD3dtxMetaData));

        AppendBasicTypes(myElement);

        Utils.CheckHeader(game, Header);
        MetaHeaderXml.AppendHeader(myElement, Header, rMsv5Header);

        AppendPixelData(myElement);

        _ = element.AppendChild(myElement);
    }

    public void WritePixelDatas(BinaryWriter writer)
    {
        for (int i = 0; i < mPixelData.Count; i++)
        {
            writer.Write(mPixelData[i]);
        }
    }

    public void ModifyD3DTX(ID3DTX d3dtx, ImageSection[] imageSections)
    {
        if (d3dtx.IsCubemap() || d3dtx.IsVolumemap() || d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize != 1)
        {
            throw new NotSupportedException("D3DTX version 4 only supports 2D textures!");
        }
        if (d3dtx.mD3dtxMetaData.mSurfaceFormat == T3SurfaceFormat.Unknown)
        {
            throw new NotSupportedException("I can't map this image's pixel format to a Telltale surface format!");
        }

        TextureInfo textureInfo = d3dtx.mD3dtxMetaData.mTextureInfo;
        if (imageSections.Length != textureInfo.mNumMipLevels)
        {
            throw new InvalidDataException("The image sections don't match the mip count!");
        }
        mD3dtxMetaData.mTextureInfo = new(MyVersion)
        {
            mWidth = textureInfo.mWidth,
            mHeight = textureInfo.mHeight,
            mNumMipLevels = textureInfo.mNumMipLevels
        };
        mD3dtxMetaData.mSurfaceFormat = d3dtx.mD3dtxMetaData.mSurfaceFormat;
        mD3dtxMetaData.mStreamHeader = new(MyVersion)
        {
            mRegionCount = imageSections.Length,
            mAuxDataCount = mD3dtxMetaData.mStreamHeader.mAuxDataCount,
            mTotalDataSize = imageSections.Sum(section => section.Pixels.Length)
        };
        mPixelData = [];
        //DDS starts with the biggest mip. Telltale stores the smallest mip first.
        for (int i = imageSections.Length - 1; i >= 0; --i)
        {
            ImageSection section = imageSections[i];
            mPixelData.Add(section.Pixels);
            mD3dtxMetaData.mStreamHeader.Add(new(MyVersion)
            {
                mMipIndex = i,
                mMipCount = 1,
                mDataSize = (uint)section.Pixels.Length,
                mPitch = (int)section.RowPitch,
                mSlicePitch = (int)section.SlicePitch
            });
        }
    }

    #endregion

    #region Header

    public void WriteMetaHeader(BinaryWriter writer)
    {
        if (!wMsv5Header)
        {
            return;
        }
        Utils.CheckHeader(game, Header);

        if (Header == null && rMsv5Header)
        {
            throw new NullReferenceException($"{nameof(Header)} is not properly initialized!");
        }
        else if (Header == null)
        {
            //Recreate header
            MSV5 defaultHeaderForWalkingDead1 = new()
            {
                mClassNamesLength = (uint)mDefaultClassNamesWD1.Length,
                mClassNames = mDefaultClassNamesWD1
            };
            Header = defaultHeaderForWalkingDead1;
        }

        uint debugSectionChunkSize = Header.mDebugSectionChunkSize;
        Header.SetMetaSectionChunkSizes(mD3dtxMetaData.Size, debugSectionChunkSize, asyncSectionChunkSize);
        msCtx ??= new(game);
        msCtx.Header = Header;
        this.ShareMetaStreamContext();
        Header.WriteToBinary(writer);
    }

    public void ParseMetaHeader(BinaryReader reader)
    {
        if (!rMsv5Header)
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

    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute rMsv5HeaderAtt, wMsv5HeaderAtt;

        rMsv5HeaderAtt = element.OwnerDocument.CreateAttribute(nameof(rMsv5Header));
        wMsv5HeaderAtt = element.OwnerDocument.CreateAttribute(nameof(wMsv5Header));

        rMsv5HeaderAtt.Value = rMsv5Header.ToString();
        wMsv5HeaderAtt.Value = wMsv5Header.ToString();

        _ = element.Attributes.Append(rMsv5HeaderAtt);
        _ = element.Attributes.Append(wMsv5HeaderAtt);
    }

    private void ParseBasicTypes(XmlElement element)
    {
        wMsv5Header = bool.Parse(element.GetAttribute(nameof(wMsv5Header)));
        rMsv5Header = bool.Parse(element.GetAttribute(nameof(rMsv5Header)));
    }

    private void AppendPixelData(XmlElement element)
    {
        foreach (byte[] pixelBytes in mPixelData)
        {
            XmlElement pixelElement = element.OwnerDocument.CreateElement(nameof(PixelData));
            pixelElement.InnerText = System.Convert.ToBase64String(pixelBytes);
            _ = element.AppendChild(pixelElement);
        }
    }

    public void ParsePixelData(XmlElement element)
    {
        mPixelData = [];
        foreach (XmlNode node in element.ChildNodes)
        {
            if (node is XmlElement pixelElement && pixelElement.Name == nameof(PixelData))
            {
                byte[] data = System.Convert.FromBase64String(pixelElement.InnerText);
                mPixelData.Add(data);
            }
        }
    }

    #endregion
}
