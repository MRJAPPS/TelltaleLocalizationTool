using System.Reflection.PortableExecutable;
using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.Graphics.DDS.Enums;

namespace TelltaleLocalizationTool.Telltale.Graphics.DDS;
public class DDS_HEADER : IParsable, IWriteable, ISize
{
    #region private constants

    private const uint ddsMagic = 0x20534444;
    private const uint Extra = sizeof(uint) * 2;
    private const int dwReserved1Len = 11;
    #endregion

    #region Public

    /// <summary>
    /// Flags to indicate which members contain valid data.
    /// </summary>
    public uint dwFlags { get; set; }
    /// <summary>
    /// Surface height (in pixels).
    /// </summary>
    public uint dwHeight { get; set; }
    /// <summary>
    /// Surface width (in pixels).
    /// </summary>
    public uint dwWidth { get; set; }
    /// <summary>
    /// The pitch or number of bytes per scan line in an uncompressed texture; the total number of bytes in the top level texture for a compressed texture. For information about how to compute the pitch, see the DDS File Layout section of the https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds-pguide.
    /// </summary>
    public uint dwPitchOrLinearSize { get; set; }
    /// <summary>
    /// Depth of a volume texture (in pixels), otherwise unused.
    /// </summary>
    public uint dwDepth { get; set; }
    /// <summary>
    /// Number of mipmap levels, otherwise unused.
    /// </summary>
    public uint dwMipMapCount { get; set; }
    /// <summary>
    /// Unused.
    /// </summary>
    public uint[] dwReserved1 { get; private set; } = new uint[dwReserved1Len];
    /// <summary>
    /// See <see cref="DDS_PIXELFORMAT"/>
    /// </summary>
    public DDS_PIXELFORMAT ddspf { get; set; } = new();
    /// <summary>
    /// Specifies the complexity of the surfaces stored.
    /// </summary>
    public uint dwCaps { get; set; }
    /// <summary>
    /// Specifies the complexity of the surfaces stored.
    /// </summary>
    public uint dwCaps2 { get; set; }
    /// <summary>
    /// Specifies the complexity of the surfaces stored.
    /// </summary>
    public uint dwCaps3 { get; set; }
    /// <summary>
    /// Specifies the complexity of the surfaces stored.
    /// </summary>
    public uint dwCaps4 { get; set; }
    /// <summary>
    /// Reserved
    /// </summary>
    public uint dwReserved2 { get; private set; }
    /// <summary>
    /// Reserved
    /// </summary>
    public uint dwReserved3 { get; private set; }
    #endregion

    #region Implement

    public long Position { get; private set; }

    public uint Size => Extra + (sizeof(uint) * 11) * 2 + ddspf.Size;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        CheckMagic(reader);//Skip magic
        reader.ReadUInt32();//Skip size

        dwFlags = reader.ReadUInt32();
        dwHeight = reader.ReadUInt32();
        dwWidth = reader.ReadUInt32();
        dwPitchOrLinearSize = reader.ReadUInt32();
        dwDepth = reader.ReadUInt32();
        dwMipMapCount = reader.ReadUInt32();
        for (int i = 0; i < dwReserved1.Length; ++i)
            dwReserved1[i] = reader.ReadUInt32();//Skip...
        ddspf.Parse(reader);
        dwCaps = reader.ReadUInt32();
        dwCaps2 = reader.ReadUInt32();
        dwCaps3 = reader.ReadUInt32();
        dwCaps4 = reader.ReadUInt32();
        dwReserved2 = reader.ReadUInt32();
        dwReserved3 = reader.ReadUInt32();
    }

    public void Parse(XmlElement reader, string name)
    {
        throw new NotImplementedException($"We don't save and load {nameof(DDS_HEADER)} as XML! Why would I even do that?");
    }

    public void Write(BinaryWriter writer)
    {
        throw new NotImplementedException($"We only use {nameof(DDS_HEADER)} to read and skip past the header, not to write it.");
    }

    public void Write(XmlElement element, string name)
    {
        throw new NotImplementedException($"We don't save and load {nameof(DDS_HEADER)} as XML! Why would I even do that?");
    }

    #endregion

    #region Private 
    private void CheckMagic(BinaryReader reader)
    {
        if (reader.ReadUInt32() != ddsMagic)
            throw new FileLoadException("Invalid dds magic!");
    }
    #endregion
}
