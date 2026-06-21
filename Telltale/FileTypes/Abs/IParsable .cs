namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

public interface IParsable : ISize
{
    public void Parse(BinaryReader reader);
}
