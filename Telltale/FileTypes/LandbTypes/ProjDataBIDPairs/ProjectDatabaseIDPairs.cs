using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.ProjDataBIDPairs;

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

    public uint Size =>
        ((uint)Items.Count * ItemSize) + ProjectDatabaseIDPairsSizeInFile + ProjectDatabaseIDPairsLenInFile;

    public long Position { get; private set; }

    #endregion

    #region Implement

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

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

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);

        writer.Write((uint)Items.Count);
        for (int i = 0; i < Items.Count; ++i)
        {
            Items[i].Write(writer);
        }

        Report(this, true);
    }

    #endregion
}