using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.Font.Glyph;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Font.TFont;
public class TFontMetadata(string mName = "", bool mbUnicode = default, float mHeight = default, float mBase = default, float mWiiScale = default, float mFontScale = default, bool mIsDistanceField = default, GlyphInfo[]? glyphInfos = default) : IWriteable, IParsable
{

    #region Public props metadata

    public string Name { get; set; } = mName;

    public bool IsUnicode { get; set; } = mbUnicode;

    public float Height = mHeight;

    public float Base = mBase;

    public float WiiScale = mWiiScale;

    public float FontScale = mFontScale;

    public bool IsDistanceField { get; set; } = mIsDistanceField;

    public long Position => throw new NotImplementedException();

    public GlyphInfo[] GlyphInfos = glyphInfos ?? [];

    #endregion

    #region Implement 

    public void Write(BinaryWriter writer)
    {
        throw new NotImplementedException();
    }

    public void Write(XmlElement element, string name)
    {
        throw new NotImplementedException();
    }

    public void Parse(BinaryReader reader)
    {
        throw new NotImplementedException();
    }

    public void Parse(XmlElement reader, string name)
    {
        throw new NotImplementedException();
    }

    #endregion

}
