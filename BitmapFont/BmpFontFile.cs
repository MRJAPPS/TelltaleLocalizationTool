using System.Collections.Generic;
using System.Xml.Serialization;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Convert;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.FileTypes.Font;
using TelltaleLocalizationTool.Telltale.FileTypes.Font.Glyph;
using TelltaleLocalizationTool.Telltale.Graphics.Enums;

namespace TelltaleLocalizationTool.BitmapFont;


[XmlRoot("font")]
public class BmpFontFile
{
    [XmlElement("info")]
    public BmpFontInfo? Info { get; set; }

    [XmlElement("common")]
    public BmpFontCommon? Common { get; set; }

    [XmlArray("pages")]
    [XmlArrayItem("page")]
    public List<BmpFontPage> Pages { get; set; }

    [XmlArray("chars")]
    [XmlArrayItem("char")]
    public List<BmpFontChar> Chars { get; set; }

    [XmlArray("kernings")]
    [XmlArrayItem("kerning")]
    public List<BmpFontKerning> Kernings { get; set; }

    public BmpFontFile()
    {
        Pages = [];
        Chars = [];
        Kernings = [];
    }

    public BmpFontFile(
        BmpFontInfo info,
        BmpFontCommon common,
        List<BmpFontPage> pages,
        List<BmpFontChar> chars,
        List<BmpFontKerning> kernings)
    {
        Info = info;
        Common = common;
        Pages = pages ?? [];
        Chars = chars ?? [];
        Kernings = kernings ?? [];
    }

    #region XML
    public static void SerializeToXml(BmpFontFile bmpFont, string filePath)
    {
        var serializer = new XmlSerializer(typeof(BmpFontFile));

        using var writer = new StreamWriter(filePath);
        serializer.Serialize(writer, bmpFont);
    }

    public static BmpFontFile DeserializeFromXml(string filePath)
    {
        var serializer = new XmlSerializer(typeof(BmpFontFile));

        using var reader = new StreamReader(filePath);

        return (BmpFontFile)serializer.Deserialize(reader)!;
    }

    #endregion

    #region Static converter

    #region Helper 

    private static GlyphInfo ConvertToGlyphInfo(BmpFontChar bmpChar, int w, int h)
    {
        // Channel in Telltale format is a bitmask (e.g., 1=R, 2=G, 4=B, 8=A, 15=all).
        // Keep the original value, do not convert to index.
        return new GlyphInfo
        {
            mChar = new()
            {
                CharCode = (uint)bmpChar.Id
            },
            mTexturePage = bmpChar.Page,
            mChannel = bmpChar.Channel,                // Use the raw bitmask
            mRect = new TRect
            {
                Left = bmpChar.X / (float)w,
                Top = bmpChar.Y / (float)h,
                Right = (bmpChar.X + bmpChar.Width) / (float)w,
                Bottom = (bmpChar.Y + bmpChar.Height) / (float)h
            },
            mGlyphMetrics = new()
            {
                mXOffset = bmpChar.XOffset,
                mYOffset = bmpChar.YOffset,
                mXAdvance = bmpChar.XAdvance,
            },
            mGlyphSize = new()
            {
                mWidth = bmpChar.Width,
                mHeight = bmpChar.Height,
            },
        };
    }

    private static BmpFontChar ConvertToBmpFontChar(GlyphInfo glyphInfo, int w, int h)
    {
        int x = (int)Math.Round(glyphInfo.mRect.Left * w);
        int y = (int)Math.Round(glyphInfo.mRect.Top * h);
        int width = (int)glyphInfo.mGlyphSize.mWidth;
        int height = (int)glyphInfo.mGlyphSize.mHeight;

        return new BmpFontChar
        {
            Id = (int)glyphInfo.mChar.CharCode,
            Page = glyphInfo.mTexturePage,
            Channel = glyphInfo.mChannel,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            XOffset = (int)glyphInfo.mGlyphMetrics.mXOffset,
            YOffset = (int)glyphInfo.mGlyphMetrics.mYOffset,
            XAdvance = (int)glyphInfo.mGlyphMetrics.mXAdvance
        };
    }

    private static List<BmpFontPage> GetPages(TFont font)
    {
        List<BmpFontPage> result = new(font.mTexturePages.Count);
        for (int i = 0; i < font.mTexturePages.Count; ++i)
        {
            D3DTXV9 texture = font.mTexturePages[i];
            BmpFontPage fontPage = new()
            {
                File = FontTexName.GetFontTexOutputName(font.mName, TextureType.DDS, true, i, texture),
                Id = i
            };
            result.Add(fontPage);
        }
        return result;
    }

    private static List<BmpFontChar> GetChars(TFont font)
    {
        GlyphInfoCollection glyphs = font.mGlyphInfos;
        int w = (int)font.mTexturePages.First().mD3dtxMetaData.mTextureInfo.mWidth;
        int h = (int)font.mTexturePages.First().mD3dtxMetaData.mTextureInfo.mHeight;
        List<BmpFontChar> ret = new(glyphs.Count);
        for (int i = 0; i < glyphs.Count; ++i)
        {
            GlyphInfo glyph = glyphs[i];
            BmpFontChar forAdd = ConvertToBmpFontChar(glyph, w, h);
            ret.Add(forAdd);
        }
        return ret;
    }

    private static void GetChars(BmpFontFile bmpFontFile, TFont font)
    {
        for (int i = 0; i < bmpFontFile.Chars.Count; ++i)
        {
            int w = bmpFontFile.Common?.ScaleW ?? (int)font.mTexturePages.First().mD3dtxMetaData.mTextureInfo.mWidth;
            int h = bmpFontFile.Common?.ScaleH ?? (int)font.mTexturePages.First().mD3dtxMetaData.mTextureInfo.mHeight;
            GlyphInfo forAdd = ConvertToGlyphInfo(bmpFontFile.Chars[i], w, h);
            font.mGlyphInfos.Add(forAdd);
        }
    }

    #endregion
    //I'm not moving these functions to the Converter class because it's already cluttered enough — some of its functions should be moved elsewhere (TODO).
    public static BmpFontFile FromTFont(TFont font)
    {
        BmpFontFile ret = new()
        {
            Info = new()
            {
                Face = font.mName,
                Unicode = font.mbUnicode ? 1 : 0,
                Charset = "",//Default
                Bold = 0,//Default
                Italic = 0,//Default
                Aa = 1,//Default
                Outline = 0,//Default
                Padding = "0,0,0,0",//Default
                Smooth = 1,//Default
                StretchH = 100,//Default
                Size = (int)font.mHeight,
                Spacing = "1,1",
            },
            Common = new()
            {
                AlphaChnl = 0,//Default
                Base = (int)font.mBase,
                BlueChnl = 4,//Default
                GreenChnl = 4,//Default
                RedChnl = 4,//Default
                LineHeight = (int)font.mHeight,
                Packed = 0,//Default
                Pages = font.mTexturePages.Count,
                ScaleH = (int)font.mTexturePages.First().mD3dtxMetaData.mTextureInfo.mHeight,
                ScaleW = (int)font.mTexturePages.First().mD3dtxMetaData.mTextureInfo.mWidth,
            },
            Pages = GetPages(font),
            Chars = GetChars(font),
            Kernings = [],
        };
        return ret;
    }

    public static TFont ToTFont(string bmpFontFile)
    {
        string inputDirectory = Path.GetDirectoryName(bmpFontFile) ?? throw new DirectoryNotFoundException($"I couldn't find the folder associated with fnt(xml)! Use an absolute path!{Environment.NewLine}\"{bmpFontFile}\"");
        BmpFontFile bmpFont = BmpFontFile.DeserializeFromXml(bmpFontFile);
        return ToTFont(bmpFont, inputDirectory, Path.GetFileNameWithoutExtension(bmpFontFile));
    }

    public static TFont ToTFont(BmpFontFile bmpFont, string inputDirectory, string fontTexNamePrefix)
    {
        TFont font = new()
        {
            mBase = bmpFont.Common!.Base,
            mbUnicode = bmpFont.Info!.Unicode == 1,
            mFontScale = 1,//Default
            mGlyphInfos = [],
            mHeight = bmpFont.Info.Size,
            mIsDistanceField = false,//Default
            mName = bmpFont.Info.Face,
            mWiiScale = 0.5F,//Default
            ReadMsv6Header = false,
            WriteMsv6Header = true,
            mTexturePages = []
        };

        GetChars(bmpFont, font);

        TextureType fromTextureType = Converter.GetTextureTypeFromExtension(bmpFont.Pages.First().File);
        font.mTexturePages.ImportTextures(fontTexNamePrefix, inputDirectory, fromTextureType, true);

        return font;
    }

    #endregion
}