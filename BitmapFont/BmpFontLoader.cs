using System.Xml.Serialization;

namespace TelltaleLocalizationTool.BitmapFont;

public static class BmpFontLoader
{
    public static BmpFontFile LoadFromFile(string filePath)
    {
        XmlSerializer serializer = new(typeof(BmpFontFile));
        using FileStream fileStream = new(filePath, FileMode.Open);
        return (BmpFontFile)(serializer.Deserialize(fileStream) ?? throw new InvalidDataException($"Your fnt file needs to be XML! And even if it is, I couldn't deserialize it into {nameof(BmpFontFile)}!"));
    }

    public static BmpFontFile LoadFromString(string xmlString)
    {
        XmlSerializer serializer = new(typeof(BmpFontFile));
        using StringReader reader = new(xmlString);
        return (BmpFontFile)(serializer.Deserialize(reader) ?? throw new InvalidDataException($"I couldn't deserialize your xml into {nameof(BmpFontFile)}!"));
    }
}