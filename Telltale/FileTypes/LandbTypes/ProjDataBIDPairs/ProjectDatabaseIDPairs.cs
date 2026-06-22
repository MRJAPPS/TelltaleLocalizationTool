using System.ComponentModel;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.ProjDataBIDPairs;

/// <summary>
/// A collection of <see cref="ProjectDatabaseIDPair"/> objects.  
/// The file block starts with a redundant size value, then a count, then the actual pairs.
/// </summary>
[Description("Processing a collection of ProjectDatabaseIDPair objects that have mProjectID and mDBID. No idea what this is, honestly.")]
public class ProjectDatabaseIDPairs : TCollectionBase<ProjectDatabaseIDPair>, IParsable
{
    #region Constants

    // Space reserved for the size field at the top.
    private const uint ProjectDatabaseIDPairsSizeInFile = sizeof(uint);
    // Space reserved for the count field.
    private const uint ProjectDatabaseIDPairsLenInFile = sizeof(uint);

    #endregion

    #region Fields

    // Pre-calculated byte size of a single pair – used to compute the total size easily.
    private readonly uint ItemSize = new ProjectDatabaseIDPair().Size;

    #endregion

    #region Properties

    /// <summary>
    /// Total byte size of this block in the file.
    /// </summary>
    public uint Size =>
        ((uint)Items.Count * ItemSize) + ProjectDatabaseIDPairsSizeInFile + ProjectDatabaseIDPairsLenInFile;

    /// <summary>
    /// Stream position when parsing began.
    /// </summary>
    public long ParsePosition { get; private set; }

    #endregion

    #region IParsable

    /// <summary>
    /// Reads the block: skips the redundant size field, grabs the pair count,
    /// and then reads exactly that many <see cref="ProjectDatabaseIDPair"/> entries.
    /// </summary>
    public void Parse(BinaryReader reader)
    {
        ParsePosition = reader.BaseStream.Position;

        // We don't need the size value because we can compute it from the known structure.
        _ = reader.ReadUInt32();

        int capacity = reader.ReadInt32();
        Items = new List<ProjectDatabaseIDPair>(capacity);
        for (int i = 0; i < capacity; ++i)
        {
            ProjectDatabaseIDPair pair = new();
            pair.Parse(reader);
            Items.Add(pair);
        }

        Report(this);
    }

    #endregion
}