using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.Graphics;
using TelltaleLocalizationTool.Telltale.Meta;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using TelltaleLocalizationTool.Telltale.Meta.TClassInfo;
using TelltaleLocalizationTool.Telltale.Meta.Versions;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
[Description("Reading a D3DTX version 9")]
public class D3DTXV9(Games game, bool autoPixelData = true, bool rMsv6Header = true, bool wMsv6Header = true) : ReportBase, ID3DTX
{
    #region Private constants

    private const int MyVersion = 9;
    private const string PixelData = nameof(PixelData);

    private const uint Enums = sizeof(T3SurfaceFormat) + sizeof(T3TextureLayout) + sizeof(T3SurfaceGamma) +
                 sizeof(T3SurfaceMultisample) + sizeof(T3ResourceUsage) + sizeof(T3TextureType) +
                sizeof(T3TextureAlphaMode) + sizeof(T3TextureColor);
    private const int BasicTypes = sizeof(int) + (sizeof(float) * 4);

    #endregion

    #region private fields
    static readonly ClassName[] mDefaultClassNamesWD3 =
[
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.T3Texture },
        mVersionCRC = 0xA730AB61
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.T3SamplerStateBlock },
        mVersionCRC = 0xB2E3CEB0
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.EnumPlatformType },
        mVersionCRC = 0xA4AC7DAF
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.RenderSwizzleParams },
        mVersionCRC = 0x5B484905
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.Vector2 },
        mVersionCRC = 0xE19FCC02
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.T3Texture_StreamHeader },
        mVersionCRC = 0x9AF4FC40
    },
    new() {
        mTypeNameCRC = new Symbol { mCrc64 = (ulong)ClassNames.T3Texture_RegionStreamHeader },
        mVersionCRC = 0x5057EA61
    }
];

    #endregion

    #region public props

    public D3DTXV9Metadata mD3dtxMetaData { get; set; } = new(MyVersion);

    #endregion

    #region Private props

    private uint asyncSectionChunkSize => (uint)mPixelData.Sum(item => item.Length);

    #endregion

    #region Implement

    public bool AutoPixelData { get => autoPixelData; set => autoPixelData = value; }

    public bool ReadMsvHeader { get => rMsv6Header; set => rMsv6Header = value; }

    public bool WriteMsvHeader { get => wMsv6Header; set => wMsv6Header = value; }

    public MetaStreamContext? msCtx { get; protected set; } = new(game);

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    D3DTXMetadata ID3DTX.mD3dtxMetaData => mD3dtxMetaData;

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
        XmlElement myElement = reader.GetSingleElement(nameof(D3DTXV9), name);

        ParseBasicTypes(myElement);
        Header = MetaHeaderXml.ParseHeader(myElement, rMsv6Header);
        Utils.CheckHeader(game, Header,rMsv6Header);

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
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(D3DTXV9));
        myElement.SetAttribute(nameof(name), name);

        mD3dtxMetaData.Write(myElement, nameof(mD3dtxMetaData));

        AppendBasicTypes(myElement);
        Utils.CheckHeader(game, Header,rMsv6Header);
        MetaHeaderXml.AppendHeader(myElement, Header, rMsv6Header);

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
        if (d3dtx.mD3dtxMetaData.mSurfaceFormat == T3SurfaceFormat.Unknown)
        {
            throw new NotSupportedException("I can't map this image's pixel format to a Telltale surface format! Save it as a supported DDS format first.");
        }
        int mipLevels = (int)d3dtx.mD3dtxMetaData.mTextureInfo.mNumMipLevels;
        int arraySize = (int)d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize;
        int depth = (int)d3dtx.mD3dtxMetaData.mTextureInfo.mDepth;
        List<(ImageSection section, int mip, int face)> images = [];
        int index = 0;
        if (d3dtx.IsVolumemap())
        {
            for (int mip = 0; mip < mipLevels; ++mip)
            {
                for (int face = 0; face < Math.Max(1, depth >> mip); ++face)
                {
                    images.Add((imageSections[index++], mip, face));
                }
            }
        }
        else
        {
            for (int face = 0; face < arraySize; ++face)
            {
                for (int mip = 0; mip < mipLevels; ++mip)
                {
                    images.Add((imageSections[index++], mip, face));
                }
            }
        }

        mD3dtxMetaData.mTextureInfo = d3dtx.mD3dtxMetaData.mTextureInfo;
        mD3dtxMetaData.mSurfaceFormat = d3dtx.mD3dtxMetaData.mSurfaceFormat;
        mD3dtxMetaData.mSurfaceGamma = d3dtx.mD3dtxMetaData.mSurfaceGamma;
        mD3dtxMetaData.mTextureLayout = d3dtx.mD3dtxMetaData.mTextureLayout;
        if (d3dtx.IsCubemap())
        {
            mD3dtxMetaData.mTextureInfo.mArraySize /= 6;
            mD3dtxMetaData.mTextureLayout = mD3dtxMetaData.mTextureInfo.mArraySize > 1
                ? T3TextureLayout.TextureCubemapArray : T3TextureLayout.TextureCubemap;
        }
        mD3dtxMetaData.mStreamHeader = new(9)
        {
            mRegionCount = imageSections.Length,
            mAuxDataCount = mD3dtxMetaData.mStreamHeader.mAuxDataCount,
            mTotalDataSize = imageSections.Sum(section => section.Pixels.Length)
        };
        mPixelData.Clear();
        //Telltale stores the smallest mip first. DDS starts with the biggest one.
        foreach ((ImageSection section, int mip, int face) in images.OrderByDescending(image => image.mip).ThenBy(image => image.face))
        {
            mPixelData.Add(section.Pixels);
            mD3dtxMetaData.mStreamHeader.Add(new(9)
            {
                mMipIndex = mip,
                mFaceIndex = face,
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
        Utils.CheckHeader(game, Header,rMsv6Header);

        uint debugSectionChunkSize = Header.mDebugSectionChunkSize;
        Header.SetMetaSectionChunkSizes(mD3dtxMetaData.Size, debugSectionChunkSize, asyncSectionChunkSize);
        msCtx ??= new(game);
        msCtx.Header = Header;
        this.ShareMetaStreamContext();
        Header.WriteToBinary(writer);
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
        Utils.CheckHeader(game, Header,rMsv6Header);
        Utils.CheckSectionsV56($"The sections({nameof(D3DTXV9)}) are incomplete bro!", Header, reader.BaseStream.Length, reader.BaseStream.Position);
        this.ShareMetaStreamContext();
    }

    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute rMsv6HeaderAtt, wMsv6HeaderAtt;

        rMsv6HeaderAtt = element.OwnerDocument.CreateAttribute(nameof(rMsv6Header));
        wMsv6HeaderAtt = element.OwnerDocument.CreateAttribute(nameof(wMsv6Header));

        rMsv6HeaderAtt.Value = rMsv6Header.ToString();
        wMsv6HeaderAtt.Value = wMsv6Header.ToString();

        _ = element.Attributes.Append(rMsv6HeaderAtt);
        _ = element.Attributes.Append(wMsv6HeaderAtt);
    }

    private void ParseBasicTypes(XmlElement element)
    {
        wMsv6Header = bool.Parse(element.GetAttribute(nameof(wMsv6Header)));
        rMsv6Header = bool.Parse(element.GetAttribute(nameof(rMsv6Header)));
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
