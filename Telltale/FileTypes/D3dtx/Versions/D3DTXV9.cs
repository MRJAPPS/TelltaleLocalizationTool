using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.D3dtxTypes.StreamHeaderInfo;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.Graphics;
using TelltaleLocalizationTool.Telltale.Meta;
using TelltaleLocalizationTool.Telltale.Meta.TClassInfo;
using TelltaleLocalizationTool.Telltale.Meta.Versions;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
[Description("Reading a D3DTX version 9")]
public class D3DTXV9(bool autoPixelData = true, bool rMsv6Header = true, bool wMsv6Header = true) : ReportBase, ID3DTX
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

    #region public props

    public bool AutoPixelData { get => autoPixelData; set => autoPixelData = value; }

    public bool ReadMsv6Header { get => rMsv6Header; set => rMsv6Header = value; }

    public bool WriteMsv6Header { get => wMsv6Header; set => wMsv6Header = value; }

    #endregion

    #region Private props

    private uint asyncSectionChunkSize => (uint)mPixelData.Sum(item => item.Length);

    #endregion

    #region Implement


    public D3DTXMetadata mD3dtxMetaData { get; set; } = new(MyVersion);

    public List<byte[]> mPixelData { get; set; } = [];

    public uint Size => mD3dtxMetaData.Size + asyncSectionChunkSize;

    public long Position { get; private set; }

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
            mPixelData.Add(imageData);
        }
    }

    public void Write(BinaryWriter writer)
    {
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
        d3dtx.mD3dtxMetaData.mTextureInfo = d3dtx.mD3dtxMetaData.mTextureInfo;
        d3dtx.mD3dtxMetaData.mSurfaceFormat = d3dtx.mD3dtxMetaData.mSurfaceFormat;
        d3dtx.mD3dtxMetaData.mSurfaceGamma = d3dtx.mD3dtxMetaData.mSurfaceGamma;

        mPixelData.Clear();
        mPixelData = TextureManager.GetPixelDataListFromSections(imageSections);

        d3dtx.mD3dtxMetaData.mStreamHeader = new(MyVersion)
        {
            mRegionCount = imageSections.Length,
            mAuxDataCount = mD3dtxMetaData.mStreamHeader.mAuxDataCount,
            mTotalDataSize = (int)ByteFunctions.GetByteArrayListElementsCount(mPixelData)
        };

        for (int i = 0; i < d3dtx.mD3dtxMetaData.mStreamHeader.mRegionCount; i++)
        {
            RegionStreamHeader item = new(MyVersion)
            {
                mDataSize = (uint)mPixelData[i].Length,
                mMipCount = 1, // mMipCount is a strange variable, it is always 1 for every single texture
                mPitch = (int)imageSections[i].RowPitch,
                mSlicePitch = (int)imageSections[i].SlicePitch,
            };
            d3dtx.mD3dtxMetaData.mStreamHeader.Add(item);
        }

        if (d3dtx.IsCubemap())
        {
            d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize /= 6;
            d3dtx.mD3dtxMetaData.mTextureLayout = d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize > 1 ? T3TextureLayout.TextureCubemapArray : T3TextureLayout.TextureCubemap;

            int interval = d3dtx.mD3dtxMetaData.mStreamHeader.mRegionCount / (int)d3dtx.mD3dtxMetaData.mTextureInfo.mNumMipLevels;
            // Example a cube array textures with 5 mips will have 30 regions (6 faces * 5 mips)
            // If the array is 2 element there will be 60 regions (6 faces * 5 mips * 2 elements)
            // The mip index will be the region index % interval
            for (int i = 0; i < d3dtx.mD3dtxMetaData.mStreamHeader.Count; i++)
            {
                d3dtx.mD3dtxMetaData.mStreamHeader[i].mFaceIndex = i % (6 * (int)d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize); // Unknown guess, it could be 6 or 6 * mArraySize
                d3dtx.mD3dtxMetaData.mStreamHeader[i].mMipIndex = (d3dtx.mD3dtxMetaData.mStreamHeader.mRegionCount - i - 1) / interval;
            }
        }

        else if (d3dtx.IsVolumemap())
        {
            d3dtx.mD3dtxMetaData.mTextureLayout = T3TextureLayout.Texture3D;

            int currDepth = (int)d3dtx.mD3dtxMetaData.mTextureInfo.mDepth;
            int currentMipIndex = (int)(d3dtx.mD3dtxMetaData.mTextureInfo.mNumMipLevels - 1);
            int copyOfDepth = currDepth;

            int depthIndex = 0;
            int faceIndex = 0;

            for (int i = 0; i < d3dtx.mD3dtxMetaData.mStreamHeader.Count; i++)
            {
                d3dtx.mD3dtxMetaData.mStreamHeader[d3dtx.mD3dtxMetaData.mStreamHeader.mRegionCount - currDepth + depthIndex++].mFaceIndex = faceIndex++; // NOTE: This could be different for 3D textures. I don't have information at the moment.
                d3dtx.mD3dtxMetaData.mStreamHeader[i].mMipIndex = currentMipIndex;

                if (depthIndex == copyOfDepth)
                {
                    currentMipIndex--;

                    copyOfDepth = Math.Max(copyOfDepth / 2, 1);
                    currDepth += copyOfDepth;

                    depthIndex = 0;
                    faceIndex = 0;
                }
            }
        }
        else
        {
            d3dtx.mD3dtxMetaData.mTextureLayout = d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize > 1 ? T3TextureLayout.Texture2DArray : T3TextureLayout.Texture2D;

            int interval = d3dtx.mD3dtxMetaData.mStreamHeader.mRegionCount / (int)d3dtx.mD3dtxMetaData.mTextureInfo.mNumMipLevels;

            for (int i = 0; i < d3dtx.mD3dtxMetaData.mStreamHeader.mRegionCount; i++)
            {
                d3dtx.mD3dtxMetaData.mStreamHeader[i].mFaceIndex = i % (int)d3dtx.mD3dtxMetaData.mTextureInfo.mArraySize;
                d3dtx.mD3dtxMetaData.mStreamHeader[i].mMipIndex = (d3dtx.mD3dtxMetaData.mStreamHeader.mRegionCount - i - 1) / interval;
            }
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
            MSV6 defaultHeaderForWalkingDead3NewF = new();
            defaultHeaderForWalkingDead3NewF.mClassNamesLength = (uint)mDefaultClassNamesWD3.Length;
            defaultHeaderForWalkingDead3NewF.mClassNames = mDefaultClassNamesWD3;
            Header = defaultHeaderForWalkingDead3NewF;
        }

        uint debugSectionChunkSize = Header.mDebugSectionChunkSize;
        Header.SetMetaSectionChunkSizes(mD3dtxMetaData.Size, debugSectionChunkSize, asyncSectionChunkSize);
        Header.WriteToBinary(writer);
    }

    public void ParseMetaHeader(BinaryReader reader)
    {
        if (!rMsv6Header)
        {
            return;
        }

        Header = MetaHeaderFactory.ReadMetaHeader(reader);
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

    public void AppendPixelData(XmlElement element)//TODO: Make all XML Append and Parse methods private!!!
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
