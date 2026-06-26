using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;

namespace TelltaleLocalizationTool.Telltale.Graphics;

public class ImageSection
{
    public nuint Width { get; set; }
    public nuint Height { get; set; }
    public DXGIFormat Format { get; set; }
    public nuint SlicePitch { get; set; }
    public nuint RowPitch { get; set; }
    public byte[] Pixels { get; set; } = [];
};