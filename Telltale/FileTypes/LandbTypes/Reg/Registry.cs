using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Reg;

/// <summary>
/// Holds a normal FlagIndexMap and its reverse twin, plus a tiny size header.
/// </summary>
[Description("Processing a Registry object that includes a normal FlagIndexMap(and a reverse FlagIndexMap)")]
public class Registry : ReportBase, IParsable
{
    #region Constants

    // The size of the redundant length field that lives at the top of the struct.
    private const uint RegistrySizeInFile = sizeof(uint);

    #endregion

    #region Properties

    /// <summary>
    /// The forward map of flags to symbols.
    /// </summary>
    public FlagIndexMap mFlagIndexMap { get; set; } = new();

    /// <summary>
    /// The reverse map, with an extra leading entry.
    /// </summary>
    public FlagIndexMap mFlagIndexMapReverse { get; set; } = new(true);

    /// <summary>
    /// Total size of this registry as it appears in the file.
    /// </summary>
    public uint Size => mFlagIndexMap.Size + mFlagIndexMapReverse.Size + RegistrySizeInFile;

    /// <summary>
    /// Stream offset where parsing started — for the report.
    /// </summary>
    public long ParsePosition { get; private set; }

    #endregion

    #region IParsable

    /// <summary>
    /// Reads the registry: skips the redundant size word, then parses both maps in order.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        ParsePosition = reader.BaseStream.Position;

        // The first 4 bytes are just a size value we can calculate ourselves — skip 'em.
        _ = reader.ReadUInt32();

        mFlagIndexMap.Parse(reader);
        mFlagIndexMapReverse.Parse(reader);
        Report(this);
    }

    #endregion
}