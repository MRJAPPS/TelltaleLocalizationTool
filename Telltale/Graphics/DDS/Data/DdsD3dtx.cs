namespace TelltaleLocalizationTool.Telltale.Graphics.DDS.Data;
public struct DdsD3dtx(uint width = 0, uint height = 0, byte[]? pixelData = null)
{
    #region Props
    
    public uint Width { get; set; } = width;
    public uint Height { get; set; } = height;
    public byte[] PixelData { get; set; } = pixelData ?? [];

    #endregion
}
