using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.Meta.TClassInfo;


/*
 * This is a meta stream header.
 * These objects are often at the top of every telltale file.
 * They also contain info regarding the byte size of certain data chunks, along with the classes that are used (which are CRC64'd sadly).
 * Also, Telltale uses Hungarian Notation for variable naming.
*/

namespace TelltaleLocalizationTool.Telltale.Meta.Versions;

/// <summary>
/// Meta Type REference (MTRE, or ERTM), a meta header often used in telltale files
/// </summary>
public class MTRE : IMetaHeader
{
    /// <summary>
    /// [4 bytes] The version of the meta stream version.
    /// </summary>
    public string mMetaStreamVersion { get; set; } = "ERTM";

    /// <summary>
    /// [4 bytes] Amount of class name elements (CRC64 Class Names) used in the file.
    /// </summary>
    public uint mClassNamesLength { get; set; }

    /// <summary>
    /// [12 bytes for each element] An array of class names (CRC64 Class Names) that are used in the file.
    /// </summary>
    public ClassNames[] mClassNames { get; set; } = [];

    /// <summary>
    /// [4 + string length + 4 bytes for each element] An array of class names (Unhashed Class Names) that are used in the file. Used in older games.
    /// </summary>
    public UnhashedClassNames[] mUnhashedClassNames { get; set; } = [];

    /// <summary>
    /// Meta Header (empty constructor, only used for json deserialization)
    /// </summary>
    public MTRE() { }

    public void WriteToBinary(BinaryWriter writer)
    {
        ByteFunctions.WriteFixedString(writer, mMetaStreamVersion); // Meta Stream Keyword [4 bytes]
        writer.Write(mClassNamesLength); // mClassNamesLength [4 bytes]

        //--------------------------mClassNames--------------------------
        for (int i = 0; i < mClassNames.Length; i++)
        {
            mClassNames[i].Write(writer);
        }

        for (int i = 0; i < mUnhashedClassNames.Length; i++)
        {
            mUnhashedClassNames[i].Write(writer);
        }
    }

    public void ReadFromBinary(BinaryReader reader)
    {
        mMetaStreamVersion = ByteFunctions.ReadFixedString(reader, 4); // Meta Stream Keyword [4 bytes]
        mClassNamesLength = reader.ReadUInt32(); // mClassNamesLength [4 bytes]

        mClassNames = [];
        mUnhashedClassNames = [];
        if (mClassNamesLength == 0)
        {
            return;
        }

        uint checkValue = reader.ReadUInt32();

        reader.BaseStream.Position -= 4;

        // Interesting way to check if a string is hashed. Usually hashes are big numbers, while lengths are less than a couple of dozens.
        if (checkValue is < 0 or > 128)
        {
            mClassNames = new ClassNames[mClassNamesLength];

            for (int i = 0; i < mClassNames.Length; i++)
            {
                mClassNames[i] = new ClassNames();
                mClassNames[i].Parse(reader);
            }
        }
        else
        {
            mUnhashedClassNames = new UnhashedClassNames[mClassNamesLength];

            for (int i = 0; i < mUnhashedClassNames.Length; i++)
            {
                mUnhashedClassNames[i] = new UnhashedClassNames();
                mUnhashedClassNames[i].Parse(reader);
            }
        }
    }

    public void SetMetaSectionChunkSizes(uint defaultSectionChunkSize, uint debugSectionChunkSize, uint asyncSectionChunkSize)
    {
        return;
    }

    public string GetDebugInfo()
    {
        string metaInfo = "||||||||||| Meta Header |||||||||||" + Environment.NewLine;

        metaInfo += "Meta Stream Keyword = " + mMetaStreamVersion + Environment.NewLine;
        metaInfo += "Meta mClassNamesLength = " + mClassNamesLength + Environment.NewLine;

        for (int i = 0; i < mClassNames.Length; i++)
        {
            metaInfo += "Meta mClassName " + i + " = " + mClassNames[i] + Environment.NewLine;
        }

        for (int i = 0; i < mUnhashedClassNames.Length; i++)
        {
            metaInfo += "Meta mClassName " + i + " = " + mUnhashedClassNames[i] + Environment.NewLine;
        }

        return metaInfo;
    }

    public uint GetHeaderByteSize()
    {
        uint size = checked(2 * sizeof(uint) + 12 * (uint)mClassNames.Length);
        foreach (UnhashedClassNames item in mUnhashedClassNames)
        {
            size = checked(size + 2 * sizeof(uint) + (uint)System.Text.Encoding.UTF8.GetByteCount(item.ClassName));
        }
        return size;
    }

    public void PrintConsole()
    {
        Console.WriteLine(GetDebugInfo());
    }
}
