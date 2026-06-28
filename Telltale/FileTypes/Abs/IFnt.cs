namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;
public interface IFnt : ITelltaleFile
{
    public void Import(string fntPath);
    public void Export(string fntPath);
}
