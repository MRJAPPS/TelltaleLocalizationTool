using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.Graphics.DDS.Enums;

namespace TelltaleLocalizationTool.Telltale.Graphics.DDS;

/// <summary>
/// To store DXGI formats such as floating-point data, use a dwFlags of DDPF_FOURCC and set dwFourCC to 'D','X','1','0'. Use the DDS_HEADER_DXT10 extension header to store the DXGI format in the dxgiFormat member. Note that there are non-standard variants of DDS files where dwFlags has DDPF_FOURCC and the dwFourCC value is set directly to a D3DFORMAT or DXGI_FORMAT enumeration value.It is not possible to disambiguate the D3DFORMAT versus DXGI_FORMAT values using this non-standard scheme, so the DX10 extension header is recommended instead.
/// </summary>
public class DDS_PIXELFORMAT : IParsable, IWriteable, ISize
{
    #region private constants

    private const uint Extra = sizeof(uint);
    private const uint DX10 = 0x30315844;

    #endregion

    #region Public props
    /// <summary>
    /// Values which indicate what type of data is in the surface.
    /// </summary>
    public uint dwFlags { get; set; }
    /// <summary>
    /// Four-character codes for specifying compressed or custom formats. Possible values include: DXT1, DXT2, DXT3, DXT4, or DXT5. A FourCC of DX10 indicates the prescense of the DDS_HEADER_DXT10 extended header, and the dxgiFormat member of that structure indicates the true format. When using a four-character code, dwFlags must include DDPF_FOURCC.
    /// </summary>
    public uint dwFourCC { get; set; }
    /// <summary>
    /// Number of bits in an RGB (possibly including alpha) format. Valid when dwFlags includes DDPF_RGB, DDPF_LUMINANCE, or DDPF_YUV.
    /// </summary>
    public uint dwRGBBitCount { get; set; }
    /// <summary>
    /// Red (or luminance or Y) mask for reading color data. For instance, given the A8R8G8B8 format, the red mask would be 0x00ff0000.
    /// </summary>
    public uint dwRBitMask { get; set; }
    /// <summary>
    /// Green (or U) mask for reading color data. For instance, given the A8R8G8B8 format, the green mask would be 0x0000ff00.
    /// </summary>
    public uint dwGBitMask { get; set; }
    /// <summary>
    /// Blue (or V) mask for reading color data. For instance, given the A8R8G8B8 format, the blue mask would be 0x000000ff.
    /// </summary>
    public uint dwBBitMask { get; set; }
    /// <summary>
    /// Alpha mask for reading alpha data. dwFlags must include DDPF_ALPHAPIXELS or DDPF_ALPHA. For instance, given the A8R8G8B8 format, the alpha mask would be 0xff000000.
    /// </summary>
    public uint dwABitMask { get; set; }

    #endregion

    #region Implement

    public uint Size => throw new NotImplementedException();

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        dwFlags = reader.ReadUInt32();
        dwFourCC = reader.ReadUInt32();
        if (dwFourCC == DX10)
            throw new NotImplementedException("I didn't bother implementing DX10 because I never needed it anyway!");
        dwRGBBitCount = reader.ReadUInt32();
        dwRBitMask = reader.ReadUInt32();
        dwGBitMask = reader.ReadUInt32();
        dwBBitMask = reader.ReadUInt32();
        dwABitMask = reader.ReadUInt32();
    }

    public void Parse(XmlElement reader, string name)
    {
        throw new NotImplementedException($"We don't save and load {nameof(DDS_PIXELFORMAT)} as XML! Why would I even do that?");
    }

    public void Write(BinaryWriter writer)
    {
        throw new NotImplementedException($"We only use {nameof(DDS_HEADER)} to read and skip past the header, not to write it.(I know, we are in {nameof(DDS_PIXELFORMAT)}...)");
    }

    public void Write(XmlElement element, string name)
    {
        throw new NotImplementedException($"We don't save and load {nameof(DDS_PIXELFORMAT)} as XML! Why would I even do that?");
    }

    #endregion
}
