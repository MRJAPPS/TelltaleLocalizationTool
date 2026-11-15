using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Convert;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.Graphics.Enums;

namespace TelltaleLocalizationTool.Helper;
public class FontTexName
{
    public const string DefSeparator = "|-_-|";
    public const string bmpFontSparator = "_";

    public string Separator => bmpFontMode ? bmpFontSparator : DefSeparator;

    private readonly bool bmpFontMode;

    public string? FontName { get; set; } = "";
    public string? D3dtxMetaDataName { get; set; } = "";
    public string Extension { get; set; } = "";
    public string? Directory { get; set; } = "";
    public string FullPath => !bmpFontMode
                ? Path.Combine(Directory!, $"{FontName}{Separator}{D3dtxMetaDataName}{Separator}{Counter}{Extension}")
                : Path.Combine(Directory!, $"{FontName}{bmpFontSparator}{Counter}{Extension}");

    public int Counter { get; set; }
    public TextureType FormatEnum => Converter.GetTextureTypeFromExtension(Extension);
    public bool IsSuccess = true;

    public FontTexName(string path, bool @throw = true, bool bmpFontMode = false)
    {
        this.bmpFontMode = bmpFontMode;
        Extension = Path.GetExtension(path);
        CheckProp(@throw, Extension, $"I couldn't get the \"{path}\" format!");

        Directory = Path.GetDirectoryName(path);
        CheckProp(@throw, Directory, $"I couldn't get the directory {path}!");

        string name = Path.GetFileName(path);
        CheckProp(@throw, name, $"I couldn't find and use the file name from the {path}!");

        string[] parts = name.Split([Separator], StringSplitOptions.RemoveEmptyEntries);
        if (!bmpFontMode)
        {
            if (parts.Length != 3)
            {
                CheckProp(@throw, null, "The input file should use the \"|-_-|\" separator and contain 3 parts: fontName, mD3dtxMetaDataName, and counter.format. What you provided is broken, dude!");
            }
            else
            {
                FontName = parts[0];
                D3dtxMetaDataName = parts[1];
                Counter = int.Parse(parts[2].Replace(Extension, ""));
            }
        }
        else
        {
            if (parts.Length != 2)
            {
                CheckProp(@throw, null, "The input file should use the \"_\" separator and contain 3 parts: fontName and counter.format. What you provided is broken, dude!");
            }
            else
            {
                FontName = parts[0];
                D3dtxMetaDataName = FontName;
                Counter = int.Parse(parts[1].Replace(Extension, ""));
            }

        }
        _ = FormatEnum;
    }

    private void CheckProp(bool @throw, object? input, string message)
    {
        if (input is null)
        {
            IsSuccess = @throw ? throw new ArgumentException(message) : false;
        }
    }

    public static IEnumerable<FontTexName> GetSortedTexFiles(string directory, string searchPattern, bool bmpFontMode)
    {
        return from file in System.IO.Directory.GetFiles(directory, searchPattern)
               let parts = new FontTexName(file, false, bmpFontMode)
               where parts.IsSuccess
               orderby parts.Counter ascending
               select parts;
    }
    #region Extracted methods
    public static string GetSearchPattern(string fontName, TextureType fromTextureType, bool bmfontMode)
    {
        return $"{fontName}*{(bmfontMode ? bmpFontSparator : DefSeparator)}*{Converter.GetExtensionFromTextureType(fromTextureType)}";
    }
    public static string GetFontTexOutputName(string fontName, TextureType toTextureType, bool bmpFontMode, int counter, ID3DTX d3dtxv)
    {
        return bmpFontMode ? $"{fontName}{(bmpFontMode ? bmpFontSparator : DefSeparator)}{counter}{Converter.GetExtensionFromTextureType(toTextureType)}" :
                        $"{fontName}{(bmpFontMode ? bmpFontSparator : DefSeparator)}{d3dtxv.mD3dtxMetaData.mName}{(bmpFontMode ? bmpFontSparator : DefSeparator)}{counter}{Converter.GetExtensionFromTextureType(toTextureType)}";
    }
    #endregion
}
