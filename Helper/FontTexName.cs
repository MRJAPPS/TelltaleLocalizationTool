using System.Diagnostics.Metrics;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Convert;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.Graphics.Enums;

namespace TelltaleLocalizationTool.Helper;
public class FontTexName
{
    public const string Separator = "|-_-|";
    public string FontName { get; set; } = "";
    public string D3dtxMetaDataName { get; set; } = "";
    public string Format { get; set; } = "";
    public string Directory { get; set; } = "";
    public string FullPath => Path.Combine(Directory, $"{FontName}{Separator}{D3dtxMetaDataName}{Separator}{Counter}{Format}");
    public int Counter { get; set; }
    public TextureType FormatEnum => Converter.GetTextureTypeFromExtension(Format);
    public bool IsSuccess = true;

    public FontTexName(string path, bool @throw = true)
    {
        try
        {
            Format = Path.GetExtension(path) ??
               throw new ArgumentException($"I couldn't get the {nameof(path)} format!", nameof(path));

            Directory = Path.GetDirectoryName(path) ??
                throw new ArgumentException($"I couldn't get the directory {nameof(path)}!", nameof(path));

            string name = Path.GetFileName(path) ??
                throw new ArgumentException($"I couldn't find and use the file name from the {nameof(path)}!", nameof(path));

            string[] parts = name.Split([Separator], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                throw new ArgumentException("The input file should use the \"|-_-|\" separator and contain 3 parts: fontName, mD3dtxMetaDataName, and counter.format. What you provided is broken, dude!", nameof(path));

            FontName = parts[0];
            D3dtxMetaDataName = parts[1];
            Counter = int.Parse(parts[2].Replace(Format, ""));
            _ = FormatEnum;
        }
        catch
        {
            if (@throw)
                throw;
            else
                IsSuccess = false;
        }
    }

    public static IEnumerable<FontTexName> GetSortedTexFiles(string directory, string searchPattern)
    => from file in System.IO.Directory.GetFiles(directory, searchPattern)
       let parts = new FontTexName(file, false)
       where parts.IsSuccess
       orderby parts.Counter ascending
       select parts;
}
