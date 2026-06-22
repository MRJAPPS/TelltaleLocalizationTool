using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LnagRes.Text;

namespace TelltaleLocalizationTool.Helper;

public static class Extensions
{
    public static XmlElement GetSingleElement(this XmlElement element, string tagName, string? name = null)
    {
        string xpath = name is null
            ? $"./{tagName}"
            : $"./{tagName}[@name='{name}']";

        XmlNodeList? nodes = element.SelectNodes(xpath);

        if (nodes is null || nodes.Count == 0)
            throw new InvalidDataException($"I can't find <{tagName}> in xml!");

        if (nodes.Count > 1)
            throw new InvalidDataException($"I found more than one <{tagName}> in xml!");

        return (XmlElement)(nodes[0] ?? throw new NullReferenceException("WTF!?"));
    }
}
