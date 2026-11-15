using System;
using System.IO;
using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.Graphics.DDS;

public class DDS_PIXELFORMAT : IParsable, IWriteable, ISize
{
    #region Constants

    private const uint PixelFormatSize = 32;
    private const uint DDPF_FOURCC = 0x00000004;
    private const uint DX10 = 0x30315844;

    #endregion

    #region Properties

    /// <summary>
    /// Pixel format flags.
    /// </summary>
    public uint dwFlags { get; set; }

    /// <summary>
    /// Compressed or custom format code.
    /// </summary>
    public uint dwFourCC { get; set; }

    /// <summary>
    /// Bits per pixel for uncompressed formats.
    /// </summary>
    public uint dwRGBBitCount { get; set; }

    /// <summary>
    /// Red channel mask.
    /// </summary>
    public uint dwRBitMask { get; set; }

    /// <summary>
    /// Green channel mask.
    /// </summary>
    public uint dwGBitMask { get; set; }

    /// <summary>
    /// Blue channel mask.
    /// </summary>
    public uint dwBBitMask { get; set; }

    /// <summary>
    /// Alpha channel mask.
    /// </summary>
    public uint dwABitMask { get; set; }

    public uint Size => PixelFormatSize;

    public long Position { get; private set; } = -1;

    #endregion

    #region Binary

    public void Parse(BinaryReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        Position = reader.BaseStream.Position;

        if (reader.ReadUInt32() != PixelFormatSize)
            throw new InvalidDataException("Invalid DDS pixel format size.");

        dwFlags = reader.ReadUInt32();
        dwFourCC = reader.ReadUInt32();
        dwRGBBitCount = reader.ReadUInt32();
        dwRBitMask = reader.ReadUInt32();
        dwGBitMask = reader.ReadUInt32();
        dwBBitMask = reader.ReadUInt32();
        dwABitMask = reader.ReadUInt32();

        CheckSupportedFormat();
    }

    public void Write(BinaryWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        CheckSupportedFormat();

        writer.Write(PixelFormatSize);
        writer.Write(dwFlags);
        writer.Write(dwFourCC);
        writer.Write(dwRGBBitCount);
        writer.Write(dwRBitMask);
        writer.Write(dwGBitMask);
        writer.Write(dwBBitMask);
        writer.Write(dwABitMask);
    }

    #endregion

    #region XML

    public void Parse(XmlElement reader, string name)
    {
        throw new NotSupportedException(
            "DDS_PIXELFORMAT XML parsing is not supported.");
    }

    public void Write(XmlElement element, string name)
    {
        throw new NotSupportedException(
            "DDS_PIXELFORMAT XML writing is not supported.");
    }

    #endregion

    #region Private

    private void CheckSupportedFormat()
    {
        if ((dwFlags & DDPF_FOURCC) != 0 && dwFourCC == DX10)
            throw new NotSupportedException(
                "DX10 headers are not supported.");
    }

    #endregion
}
