using System.Xml;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using TelltaleLocalizationTool.Telltale.Meta.Versions;

namespace TelltaleLocalizationTool.Telltale.Meta
{
    public static class MetaHeaderFactory
    {
        public static IMetaHeader ReadMetaHeader(BinaryReader reader)
        {
            long position = reader.BaseStream.Position;
            uint magic = reader.ReadUInt32();
            reader.BaseStream.Position = position;
            MetaVersion version = magic switch
            {
                0x4D535636 => MetaVersion.MSV6, //6VSM
                0x4D535635 => MetaVersion.MSV5, //5VSM
                0x4D545245 => MetaVersion.MTRE, //ERTM
                0x4D42494E => MetaVersion.MBIN, //NIBM
                _ => throw new NotSupportedException($"I can't read Meta Stream magic 0x{magic:X8}! Supported headers: MSV6, MSV5, MTRE and MBIN.")
            };
            IMetaHeader header = CreateMetaHeader(version);
            header.ReadFromBinary(reader);
            return header;
        }

        public static IMetaHeader CreateMetaHeader(MetaVersion version)
        {
            return version switch
            {
                MetaVersion.MSV6 => new MSV6(),
                MetaVersion.MSV5 => new MSV5(),
                MetaVersion.MTRE => new MTRE(),
                MetaVersion.MBIN => new MBIN(),
                _ => throw new ArgumentException($"Unsupported Meta version: {version}")
            };
        }

        public static IMetaHeader ReadMetaHeader(XmlElement element, string name)
        {
            XmlNodeList nodes = element.SelectNodes($"./*[@name='{name}']")!;
            if (nodes.Count != 1 || nodes[0] is not XmlElement headerElement)
            {
                throw new InvalidDataException($"Expected one meta header named '{name}' in XML!");
            }

            MetaVersion version = headerElement.Name switch
            {
                nameof(MSV6) => MetaVersion.MSV6,
                nameof(MSV5) => MetaVersion.MSV5,
                nameof(MTRE) => MetaVersion.MTRE,
                nameof(MBIN) => MetaVersion.MBIN,
                _ => throw new NotSupportedException($"I can't read Meta Stream XML element '{headerElement.Name}'!")
            };

            IMetaHeader header = CreateMetaHeader(version);
            header.Parse(element, name);
            return header;
        }

        public static IMetaHeader? CreateMetaHeader(MetaVersion version, Newtonsoft.Json.Linq.JObject jsonObject)
        {
            return version switch
            {
                MetaVersion.MSV6 => jsonObject.ToObject<MSV6>(),
                MetaVersion.MSV5 => jsonObject.ToObject<MSV5>(),
                MetaVersion.MTRE => jsonObject.ToObject<MTRE>(),
                MetaVersion.MBIN => jsonObject.ToObject<MBIN>(),
                _ => throw new ArgumentException($"Unsupported Meta version: {version}")
            };
        }
    }
}
