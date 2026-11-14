using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.Meta.Enums;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx;

public static class D3dtxFactory
{
    public static ID3DTX CreateD3dtx(Games game) => game switch
    {
        Games.WDS1 => new D3DTXV4(game),
        Games.WDS3NF => new D3DTXV9(game),
        _ => throw new NotSupportedException($"D3DTX isn't supported for {game}!")
    };
}
