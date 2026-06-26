using System.ComponentModel.DataAnnotations;

namespace TelltaleLocalizationTool.Telltale.Graphics.Enums;

public enum ImageEffect
{
    [Display(Name = "Default Mode")]
    DEFAULT,

    [Display(Name = "Swizzle ABGR")]
    SWIZZLE_ABGR,

    [Display(Name = "Restore Z")]
    RESTORE_Z,

    [Display(Name = "Remove Z")]
    REMOVE_Z
}