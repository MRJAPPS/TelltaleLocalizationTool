using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.Meta;

public static class MetaHeaderXml
{
    #region Private constants

    private const string HeaderName = nameof(ITelltaleFile.Header);

    #endregion

    #region Public Methods

    public static IMetaHeader? ParseHeader(XmlElement element, bool required)
    {
        if (element.SelectSingleNode("msv6Header") is XmlElement)
        {
            throw new InvalidDataException("This XML has a Base64 meta header. Export it again to use the readable header format!");
        }

        if (element.SelectSingleNode($"./*[@name='{HeaderName}']") is XmlElement)
        {
            return MetaHeaderFactory.ReadMetaHeader(element, HeaderName);
        }

        if (required)
        {
            throw new InvalidDataException($"I can't find {HeaderName} in XML!");
        }

        return null;
    }

    public static void AppendHeader(XmlElement element, IMetaHeader? header, bool required)
    {
        if (header == null && required)
        {
            throw new NullReferenceException($"{HeaderName} is not properly initialized!");
        }

        header?.Write(element, HeaderName);
    }

    #endregion
}
