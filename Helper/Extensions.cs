using System.Xml;
using BCnEncoder.Decoder;
using BCnEncoder.Shared.ImageFiles;

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

    public static byte[] DecodeAsPvr(this byte[] data)
    {
        BcDecoder decoder = new();

        DdsFile file = DdsFile.Load(new MemoryStream(data));
        BCnEncoder.Shared.ColorRgba32[][] images = decoder.DecodeAllMipMaps(file);

        List<byte> ddsData = [];

        foreach (BCnEncoder.Shared.ColorRgba32[]? image in images)
        {

            foreach (BCnEncoder.Shared.ColorRgba32 pixel in image)
            {
                ddsData.Add(pixel.r);
                ddsData.Add(pixel.g);
                ddsData.Add(pixel.b);
                ddsData.Add(pixel.a);
            }

        }
        return [.. ddsData];
    }
}
