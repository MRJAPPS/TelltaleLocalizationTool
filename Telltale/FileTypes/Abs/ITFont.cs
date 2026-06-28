namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;
public interface ITFont : ITelltaleFile
{
    public void Import(string TfontFile);
    public void Export(string TfontFile);
}
