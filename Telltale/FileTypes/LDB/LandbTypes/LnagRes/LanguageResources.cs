using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LnagRes.Text;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.LnagRes;

[Description("Processing the game text stuff (subtitles, menus, etc.)")]
public class LanguageResources : ReportBase, IParsable
{
    #region Private Fields

    #endregion

    #region Public Properties

    public uint Size => TextItems.Size;

    public TextCollection TextItems { get; set; } = [];

    public long Position { get; private set; }

    #endregion

    #region Implementation

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        // The first int is just a size that we can calculate ourselves from the TextItems.
        // So we read it and throw it away – it's basically a byte version of a ditto mark.
        _ = reader.ReadInt32();

        TextItems.Parse(reader);
        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(Size);
        TextItems.Write(writer);

        Report(this, true);
    }

    #endregion
}