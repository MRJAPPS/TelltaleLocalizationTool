namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

public interface IParsable : ISize
{
    long ParsePosition { get; }
    void Parse(BinaryReader reader);
}
