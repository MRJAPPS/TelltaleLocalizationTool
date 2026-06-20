using TelltaleLocalizationTool.Telltale.Meta.Enums;
using TelltaleLocalizationTool.Telltale.Meta.Versions;

namespace TelltaleLocalizationTool.Telltale.Meta
{
    public static class MetaHeaderFactory
    {
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
