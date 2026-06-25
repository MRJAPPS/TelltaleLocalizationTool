namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;
public interface ITelltaleFile : ITelltaleObject
{
    Meta.IMetaHeader? Header { get; }
    void ParseMetaHeader(BinaryReader reader);
    void WriteMetaHeader(BinaryWriter writer);
}
