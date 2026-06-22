using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.ProjDataBIDPairs;

[Description("Processing a collection of ProjectDatabaseIDPair objects that have mProjectID and mDBID. No idea what this is, honestly.")]
public class ProjectDatabaseIDPairs : TCollectionBase<ProjectDatabaseIDPair>, ITelltaleFile
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

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement =
            reader.GetSingleElement(nameof(ProjectDatabaseIDPairs), name);

        Items.Clear();

        int index = 0;

        while (true)
        {
            try
            {
                ProjectDatabaseIDPair pair = new();

                pair.Parse(
                    myElement,
                    $"{nameof(Items)}_{index}");

                Items.Add(pair);

                index++;
            }
            catch (InvalidDataException)
            {
                break;
            }
        }
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

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(ProjectDatabaseIDPairs));
        myElement.SetAttribute(nameof(name), name);

        for (int i = 0; i < Items.Count; ++i)
        {
            Items[i].Write(myElement, $"{nameof(Items)}_{i}");
        }

        _ = element.AppendChild(myElement);
    }

    #endregion
}