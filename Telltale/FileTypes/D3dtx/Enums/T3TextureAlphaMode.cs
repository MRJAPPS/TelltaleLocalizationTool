using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;

// Note: can't find original name, so just using the prefix eTxAlpha.
// Update: Original name could be eTxAlphaMode.
public enum T3TextureAlphaMode
{
    Unknown = -1,
    NoAlpha = 0,
    AlphaTest = 1,
    AlphaBlend = 2,
}
