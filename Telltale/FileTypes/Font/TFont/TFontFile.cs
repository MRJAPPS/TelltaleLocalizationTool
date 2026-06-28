using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Font.TFont;
/// <summary>
/// This class isn't implemented yet, but the idea is that it creates a .tfont file containing the font metadata. Alongside that file, there's a folder with the same name holding the font's texture list. The metadata has one extra field compared to TFont that sets the output/input texture format. For example, if the format is png, the folder will contain a list of pngs. Textures are saved and loaded as fontname_number.textureformat — the number matters!
/// </summary>
public class TFontFile
{

    #region Public props

    public TFontMetadata metadata { get; set; }

    #endregion

    public TFontFile()
    {
        throw new NotImplementedException();
    }

}
